using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPhotos.Models.Data;
using MyPhotos.Models.DTO;
using MyPhotos.Models.Extensions;

namespace MyPhotos.Controllers
{
    /// <summary>
    /// Инкапсулирует методы взаимодействия с изображениями пользователя и его друзей
    /// </summary>
    [ApiController]
    [Route("photos")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public sealed class UserImagesController : ContextController
    {
        private readonly string _directory;
        private readonly IConfiguration _configuration;
        private readonly MyPhotosContext _db;

        /// <inheritdoc/>
        public UserImagesController(MyPhotosContext dataContext, IConfiguration configuration)
        {
            _db = dataContext;
            _configuration = configuration;
            _directory = _configuration.GetValue(typeof(string), "PhotosDirectory") as string
                ?? throw new Exception("Нет директории для хранения изображений");
        }

        /// <summary>
        /// Возвращает изображения текущего пользователя
        /// </summary>
        /// <remarks>Возвращает изображения с полным путём</remarks>
        [HttpGet]
        public ActionResult<IEnumerable<PhotoDTO>> GetPhotos()
        {
            User user = FindCurrentUser(_db);

            List<PhotoDTO> dtos = new List<PhotoDTO>();

            foreach (var i in user.UserImages)
            {
                i.Image = GetFullPath(i.Image, user.Login);
                dtos.Add(i.AsDTO());
            }

            return Ok(dtos);
        }

        /// <summary>
        /// Возвращает изображения друга текущего пользователя
        /// </summary>
        /// <param name="friendId">ID друга</param>
        /// <remarks>Возвращает изображения друга с полным путём</remarks>
        /// <response code="200">Успешный вывод фотографий друга</response>
        /// <response code="400">
        /// * Пользователя с ID = <paramref name="friendId"/> не существует;
        /// * Пользователь не состоит в дружеских отношениях с <paramref name="friendId"/>
        /// </response>
        [HttpGet("{friendId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<IEnumerable<UserImage>> GetFriendPhotos(int friendId)
        {
            User user = FindCurrentUser(_db);
            User? friend = _db.Users.Find(friendId);

            if (friend == null)
                return BadRequest("Такого пользователя не существует");

            // Заявка в друзья от user к friend
            var sentRS = user.FriendshipRelatingUsers.FirstOrDefault(f => f.RelatedUserId == friend.Id);
            // Заявка в друзья от friend к user
            var receivedRS = user.FriendshipRelatedUsers.FirstOrDefault(f => f.RelatingUserId == friend.Id);

            // Если запрос на дружбу не был отправлен
            if (sentRS == null)
            {
                // Если запроса на дружбу от другого пользователя не было
                if (receivedRS == null)
                    return BadRequest("Вы не состоите в дружеских отношениях");
                else
                {
                    // Если запрос на дружбу не подтверждался
                    if (receivedRS.StatusId != 2)
                        return BadRequest("Вы не состоите в дружеских отношениях");
                }
            }
            else // Если запрос на дружбу был отправлен
            {
                // Если запрос на дружбу не подтверждался
                if (sentRS.StatusId == 1)
                    return BadRequest("Вы не состоите в дружеских отношениях");
            }

            List<PhotoDTO> dtos = new List<PhotoDTO>();
            friend = _db.Users.Include(u => u.UserImages).First(u => u.Id == friend.Id);
            foreach (var i in friend.UserImages)
            {
                i.Image = GetFullPath(i.Image, friend.Login);
                dtos.Add(i.AsDTO());
            }

            return Ok(dtos);
        }

        /// <summary>
        /// Загружает изображение
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> PostPhoto(IFormFile file)
        {
            User user = FindCurrentUser(_db);

            string path = GetFullPath(file.FileName, user.Login);

            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(stream);
            }

            UserImage image = new UserImage() { UserId = user.Id, Image = file.FileName };
            user.AddImage(image);

            await _db.SaveChangesAsync();

            return Ok();
        }

        private string GetFullPath(string fileName, string login)
        {
            string dir = Path.Combine(_directory, login);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string path = Path.Combine(dir, fileName);

            return path;
        }
    }
}
