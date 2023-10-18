namespace MyPhotos.Models.Data;

public partial class Friendship
{
    public int Id { get; set; }

    public int StatusId { get; set; }
    /// <summary>
    /// Тот, кто отправлял заявку в друзья
    /// </summary>
    public int RelatingUserId { get; set; }
    /// <summary>
    /// Тот, кому отправлялась заявка в друзья
    /// </summary>
    public int RelatedUserId { get; set; }
    /// <summary>
    /// Тот, кому отправлялась заявка в друзья
    /// </summary>
    public virtual User RelatedUser { get; set; } = null!;
    /// <summary>
    /// Тот, кто отправлял заявку в друзья
    /// </summary>
    public virtual User RelatingUser { get; set; } = null!;

    public virtual FriendshipStatus Status { get; set; } = null!;
}
