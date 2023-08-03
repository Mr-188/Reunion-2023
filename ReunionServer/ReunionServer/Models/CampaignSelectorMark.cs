using System;
using System.Collections.Generic;

namespace ReunionServer.Models;

public partial class CampaignSelectorMark
{
    public int Id { get; set; }

    public string? Usname { get; set; }

    public int Good { get; set; }

    public int Bad { get; set; }
}
