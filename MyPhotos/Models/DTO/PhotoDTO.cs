using System.ComponentModel.DataAnnotations;

namespace MyPhotos.Models.DTO
{
#pragma warning disable CS1591
    public sealed record PhotoDTO
    {
        [Required]
        public required int Id { get; init; }

        [Required]
        [StringLength(maximumLength: 256)]
        public required string Image { get; init; }
    }
#pragma warning restore CS1591
}
