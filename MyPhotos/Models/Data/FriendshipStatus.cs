using System;
using System.Collections.Generic;

namespace MyPhotos.Models.Data;

public partial class FriendshipStatus
{
    public int Id { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Friendship> Friendships { get; set; } = new List<Friendship>();
}
