using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReunionServer.Models;
using ReunionShareModel;

namespace ReunionServer.Controllers;

public class CampaignScoreController : ApiControllerBase
{
    private readonly DataContext _dataContext;

    /// <inheritdoc />
    public CampaignScoreController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    /// <summary>
    /// 获取任务打分
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost(RequestPaths.GetCampaignScore)]
    public async Task<CampaignScoreResponse> GetCampaignScore(CampaignScoreRequest request)
    {
        var score = await _dataContext.CampaignSelectorMarks.FirstOrDefaultAsync(m => m.Usname == request.Name);
        var response = new CampaignScoreResponse();
        if (score != null)
        {
            response.Bad = score.Bad;
            response.Good = score.Good;
        }

        return response;
    }

    /// <summary>
    /// 设置任务分数
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost(RequestPaths.SetCampaignScore)]
    public async Task SetCampaignScore(SetCampaignScoreRequest request)
    {
        var score = await _dataContext.CampaignSelectorMarks.FirstOrDefaultAsync(m => m.Usname == request.Name);
        if (score == null)
        {
            score = new CampaignSelectorMark()
            {
                Usname = request.Name
            };
            _dataContext.Add(score);
        }

        if (request.IsGood)
        {
            score.Good++;
        }
        else
        {
            score.Bad++;
        }

        await _dataContext.SaveChangesAsync();
    }
}