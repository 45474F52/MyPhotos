using System.ComponentModel.DataAnnotations;

namespace MyPhotos.Models.DTO
{
#pragma warning disable CS1591
    public sealed record UserDTO
    {
        [Required]
        public required int Id { get; init; }

        [StringLength(maximumLength: 32, MinimumLength = 5)]
        public string? Nickname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(maximumLength: 256)]
        public required string Login { get; init; }

        [Required]
        [StringLength(maximumLength: 256, MinimumLength = 8)]
        public required string Password { get; init; }

        private readonly List<PhotoDTO> _photos = new List<PhotoDTO>();
        public IReadOnlyCollection<PhotoDTO> Photos => _photos.AsReadOnly();

        public void AddPhoto(PhotoDTO photo) => _photos.Add(photo);
    }
}

#pragma warning restore CS1591