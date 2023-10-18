using MyPhotos.Models.Data;
using MyPhotos.Models.DTO;

namespace MyPhotos.Models.Extensions
{
#pragma warning disable CS1591
    public static class TransformsDTO
    {
        public static PhotoDTO AsDTO(this UserImage photo)
        {
            return new PhotoDTO()
            {
                Id = photo.Id,
                Image = photo.Image
            };
        }

        public static IReadOnlyCollection<PhotoDTO> AsDTO(this IReadOnlyCollection<UserImage> photos)
        {
            List<PhotoDTO> dtos = new List<PhotoDTO>(photos.Count);
            dtos.AddRange(photos.Select(photo => photo.AsDTO()));
            return dtos.AsReadOnly();
        }

        public static UserDTO AsDTO(this User user)
        {
            UserDTO dto = new UserDTO()
            {
                Id = user.Id,
                Login = user.Login,
                Password = user.Password,
                Nickname = user.Nickname,
            };

            foreach (UserImage photo in user.UserImages)
                dto.AddPhoto(photo.AsDTO());

            return dto;
        }

        public static FriendshipDTO AsDTO(this Friendship fs)
        {
            return new FriendshipDTO()
            {
                Id = fs.Id,
                StatusId = fs.StatusId,
                RelatingUserId = fs.RelatingUserId,
                RelatedUserId = fs.RelatedUserId
            };
        }

        public static IEnumerable<FriendshipDTO> AsDTO(this IEnumerable<Friendship> fss)
        {
            foreach (var fs in fss)
            {
                yield return fs.AsDTO();
            }
        }
    }
}

#pragma warning restore CS1591