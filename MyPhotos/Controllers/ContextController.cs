using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPhotos.Models.Data;
using System.Security.Claims;

namespace MyPhotos.Controllers
{
#pragma warning disable CS1591
    [Controller]
    public abstract class ContextController : ControllerBase
    {
        protected internal User FindCurrentUser(MyPhotosContext dataContext) =>
            dataContext.Users
            .Include(u => u.UserImages)
            .Include(u => u.FriendshipRelatingUsers)
            .Include(u => u.FriendshipRelatedUsers)
            .First(u => u.Login == HttpContext.User.FindFirstValue(ClaimTypes.Email));
    }
#pragma warning restore CS1591
}
