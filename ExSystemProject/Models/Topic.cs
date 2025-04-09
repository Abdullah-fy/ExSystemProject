using System;
using System.Collections.Generic;

namespace ExSystemProject.Models;

public partial class Topic
{
    public int TopicId { get; set; }

    public string? TopicName { get; set; }

    public string? Descrtption { get; set; }

    public int? CrsId { get; set; }

    public bool? Isactive { get; set; }

    public virtual Course? Crs { get; set; }
}
