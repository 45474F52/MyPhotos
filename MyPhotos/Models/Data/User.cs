namespace MyPhotos.Models.Data;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Nickname { get; set; }
    /// <summary>
    /// Те, кто отправил заявку в друзья
    /// </summary>
    public virtual ICollection<Friendship> FriendshipRelatedUsers { get; set; } = new List<Friendship>();
    /// <summary>
    /// Те, кому отправлялась заявка в друзья
    /// </summary>
    public virtual ICollection<Friendship> FriendshipRelatingUsers { get; set; } = new List<Friendship>();

    private List<UserImage> _userImages = new List<UserImage>();
    public IReadOnlyCollection<UserImage> UserImages => _userImages.AsReadOnly();

    public void AddImage(UserImage image) => _userImages.Add(image);
}
