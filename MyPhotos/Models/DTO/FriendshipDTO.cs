namespace MyPhotos.Models.DTO
{
#pragma warning disable CS1591
    public sealed record FriendshipDTO
    {
        public int Id { get; set; }

        public int StatusId { get; set; }

        public int RelatingUserId { get; set; }

        public int RelatedUserId { get; set; }
    }
#pragma warning restore CS1591
}
