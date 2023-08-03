namespace ReunionShareModel
{
    public class SetCampaignScoreRequest : ApiRequest<EmptyResponse>
    {
        /// <inheritdoc />
        public SetCampaignScoreRequest(string name, bool isGood = true)
        {
            Name = name;
            IsGood = isGood;
        }

        /// <inheritdoc />
        public override string Path { get; } = RequestPaths.SetCampaignScore;

        public string Name { get; set; }

        public bool IsGood { get; set; }
    }
}
