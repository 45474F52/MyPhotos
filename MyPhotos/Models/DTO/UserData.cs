using System.ComponentModel.DataAnnotations;

namespace MyPhotos.Models.DTO
{
#pragma warning disable CS1591
    public sealed record UserData
    {
        [Required]
        [EmailAddress]
        [StringLength(maximumLength: 256)]
        public required string Login { get; init; }

        [Required]
        [StringLength(maximumLength: 256, MinimumLength = 8)]
        public required string Password { get; init; }

        [StringLength(maximumLength: 32, MinimumLength = 5)]
        public string? Nickname { get; set; }
    }
#pragma warning restore CS1591
}
