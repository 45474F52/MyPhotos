using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyPhotos.Models;
using MyPhotos.Models.Data;
using MyPhotos.Models.DTO;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace MyPhotos.Controllers
{
    /// <summary>
    /// Инкапсулирует методы авторизации и регистрации
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public sealed class AccountController : ControllerBase
    {
        private readonly MyPhotosContext _db;

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="AccountController"/>
        /// </summary>
        /// <param name="dataContext">Контекст данных</param>
        public AccountController(MyPhotosContext dataContext) => _db = dataContext;

        /// <summary>
        /// Выполняет вход пользователя. Регистрирет пользователя в HttpContext
        /// </summary>
        /// <param name="data">Авторизационные данные пользователя</param>
        /// <returns>Возвращает JWT токен при успешном входе</returns>
        /// <response code="200">Успешная авторизация пользователя. Возвращается JWT токен</response>
        /// <response code="400">Введены неверные учётные данные. Пользователь не найден</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Login([FromBody] UserData data)
        {
            User? user = _db.Users.AsNoTracking().FirstOrDefault(u => u.Login == data.Login && u.Password == data.Password);

            if (user == null)
                return BadRequest("Такого пользователя не существует");

            IEnumerable<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Login)
            };

            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(2d),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                login = user.Login
            };

            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            return Ok(response);
        }

        /// <summary>
        /// Выполняет выход текущего пользователя
        /// </summary>
        /// <remarks>Удаляет пользователя из HttpContext и очищает сессию</remarks>
        /// <returns>Возвращает <see cref="StatusCodes.Status200OK"/></returns>
        [HttpGet("logout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult Logout()
        {
            HttpContext.User = new ClaimsPrincipal();
            HttpContext.Session.Clear();
            return Ok();
        }

        /// <summary>
        /// Выполняет регистрацию и вход пользователя
        /// </summary>
        /// <param name="data">Регистрационные данные пользователя</param>
        /// <returns>
        /// Возвращает <see cref="StatusCodes.Status400BadRequest"/>, если не удалось зарегистрировать пользователя
        /// </returns>
        /// <response code="200">После успешной регистрации сразу идёт авторизация, возвращающая JWT токен</response>
        /// <response code="400">Введён логин, который уже есть в базе данных</response>
        [HttpPost("signin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Signin([FromBody] UserData data)
        {
            User? user = _db.Users.FirstOrDefault(u => u.Login == data.Login);

            if (user != null)
                return BadRequest("Пользователь с таким логином уже существует");

            user = new User()
            {
                Login = data.Login,
                Password = data.Password
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            return Login(data);
        }
    }
}
