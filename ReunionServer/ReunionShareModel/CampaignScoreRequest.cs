namespace ReunionShareModel
{
    public class CampaignScoreRequest : ApiRequest<CampaignScoreResponse>
    {
        public CampaignScoreRequest(string name)
        {
            Name = name;
        }

        /// <inheritdoc />
        public override string Path => RequestPaths.GetCampaignScore;

        public string Name { get; set; }
    }
}