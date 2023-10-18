using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotos.Models.Data;
using MyPhotos.Models.DTO;
using MyPhotos.Models.Extensions;

namespace MyPhotos.Controllers
{
    /// <summary>
    /// Инкапсулирует методы взаимодейстия пользователя с друзьями
    /// </summary>
    [ApiController]
    [Route("myfriends")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public sealed class FriendsController : ContextController
    {
        private readonly MyPhotosContext _db;

        /// <inheritdoc/>
        public FriendsController(MyPhotosContext dataContext) => _db = dataContext;

        /// <summary>
        /// Возвращает друзей текущего пользователя
        /// </summary>
        /// <remarks>Возвращаются только те, чьи заявки на добавление в друзья были приняты</remarks>
        [HttpGet]
        public ActionResult<IEnumerable<FriendshipDTO>> GetFriends()
        {
            User user = FindCurrentUser(_db);

            IEnumerable<FriendshipDTO> friends = user.FriendshipRelatingUsers
                .Where(f => f.StatusId == 2)
                .Union(user.FriendshipRelatedUsers
                .Where(f => f.StatusId == 2))
                .AsDTO();

            return Ok(friends);
        }

        /// <summary>
        /// Возвращает те заявки в друзья, которые отправлял текущий пользователь и которые не были приняты
        /// </summary>
        [HttpGet("myrequests")]
        public ActionResult<IEnumerable<FriendshipDTO>> GetMyRequestsToFriendship()
        {
            User user = FindCurrentUser(_db);
            IEnumerable<FriendshipDTO> requests = user.FriendshipRelatingUsers.Where(f => f.StatusId == 1).AsDTO();
            return Ok(requests);
        }

        /// <summary>
        /// Возвращает те заявки в друзья, которые отправлялись текущему пользователю, и которые он не принял
        /// </summary>
        [HttpGet("receivedrequests")]
        public ActionResult<IEnumerable<FriendshipDTO>> GetReceivedRequestsToFriendship()
        {
            User user = FindCurrentUser(_db);
            IEnumerable<FriendshipDTO> requests = user.FriendshipRelatedUsers.Where(f => f.StatusId == 1).AsDTO();
            return Ok(requests);
        }

        /// <summary>
        /// Добавляет друга текущему пользователю
        /// </summary>
        /// <param name="friendId">ID друга</param>
        /// <remarks>
        /// Если друг ранее не отправлял заявку в друзья, будет создана новая заявка <br/>
        /// Если друг ранее отправлял заявку в друзья, то статус заявки изменится с "ignored" на "friend"
        /// </remarks>
        /// <response code="200">Пользователь успешно добавлен в друзья или ему отправлена заявка</response>
        /// <response code="400">
        /// * Пользователя с ID = <paramref name="friendId"/> не существует;
        /// * Пользователю с ID = <paramref name="friendId"/> уже была отправлена заявка в друзья
        /// </response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult AddFriend(int friendId)
        {
            User user = FindCurrentUser(_db);
            User? friend = _db.Users.Find(friendId);

            if (friend == null)
                return BadRequest("Невозможно добавить несуществующего пользователя в друзья");

            var request = user.FriendshipRelatingUsers.FirstOrDefault(f => f.RelatedUserId == friend.Id);

            if (request != default)
                return BadRequest("Вы уже отправили запрос на дружбу этому пользователю");

            request = user.FriendshipRelatedUsers.FirstOrDefault(f => f.RelatingUserId == friend.Id);

            // Если friend отправлял текущему пользователю запрос на дружбу:
            if (request != default)
            {
                request.StatusId = 2; // Подтверждение дружбы
            }
            else
            {
                var fs = new Friendship() { RelatingUserId = user.Id, RelatedUserId = friend.Id, StatusId = 1 };
                _db.Friendships.Add(fs);
            }

            _db.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Удаляет пользователя из списка друзей
        /// </summary>
        /// <param name="friendId">ID друга</param>
        /// <remarks>
        /// Если заявка на дружбу была отправлена текущим пользователем:<br/>
        /// * Если заявка была принята — изменится статус на "ignored"<br/>
        /// * Если заявка не была принята — заявка будет удалена<br/>
        /// Если заявка на дружбу была отправлена другом <paramref name="friendId"/>:<br/>
        /// * Если заявка была принята — изменится статус на "ignored"<br/>
        /// </remarks>
        /// <response code="200">Успешное удаление пользователя из друзей или перевод заявки в статус "ignored"</response>
        /// <response code="400">
        /// * Пользователя с ID = <paramref name="friendId"/> не существует;
        /// * Пользователь не состоит в дружеских отношениях с <paramref name="friendId"/>;
        /// * Запрет на удаление чужой заявки на дружбу
        /// </response>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult DeleteFriend(int friendId)
        {
            User user = FindCurrentUser(_db);
            User? friend = _db.Users.Find(friendId);

            if (friend == null)
                return BadRequest("Невозможно удалить несуществующего пользователя из друзей");

            var sentRS = user.FriendshipRelatingUsers.FirstOrDefault(f => f.RelatedUserId == friend.Id);
            var receivedRS = user.FriendshipRelatedUsers.FirstOrDefault(f => f.RelatingUserId == friend.Id);

            // Если запрос на дружбу не был отправлен
            if (sentRS == null)
            {
                // Если запроса на дружбу от другого пользователя не было
                if (receivedRS == null)
                    return BadRequest("Вы не состоите в дружеских отношениях");
                else
                {
                    if (receivedRS.StatusId == 2)
                        receivedRS.StatusId = 1; // Удаление подтверждения дружбы
                    else // Если пользователю отправили запрос и он не был подтверждён
                        return BadRequest("Нельзя удалить чужой запрос на дружбу");
                }
            }
            else // Если запрос на дружбу был отправлен
            {
                // Если запрос на дружбу не подтверждался
                if (sentRS.StatusId == 1)
                    _db.Friendships.Remove(sentRS); // Удаление запроса на дружбу
                else
                    sentRS.StatusId = 1; // Удаление подтверждения дружбы
            }

            _db.SaveChanges();

            return Ok();
        }
    }
}
