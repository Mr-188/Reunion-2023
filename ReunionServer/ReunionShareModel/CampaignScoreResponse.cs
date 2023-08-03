using System;
using System.Collections.Generic;
using System.Text;

namespace ReunionShareModel
{
    public class CampaignScoreResponse : ApiResponse
    {
        public int Good { get; set; }

        public int Bad { get; set; }
    }
}