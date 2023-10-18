using System;
using System.Collections.Generic;

namespace MyPhotos.Models.Data;

public partial class UserImage
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Image { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
