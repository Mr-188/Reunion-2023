using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace Localization;
public class CampaignSelectorService
{
    private Dao dao;
    public bool error = false;
    public CampaignSelectorService()
    {
        dao = new Dao();
    }

    public async Task UpdateTaskRating(string USName, bool isGoodRating)
    {
        try
        {
            dao.UpdateTaskRating(USName, isGoodRating);
        }
        catch (Exception ex) {
            error = true;
        }
    }

    public async Task ConnectTest()
    {
        try
        {
            error = !dao.Connect();
        }
        catch (Exception ex)
        {
            error = true;
        }
    }

    public async Task<(int goodCount, int badCount)> GetTaskRatingsAsync(string USName)
    {
        try
        {
            return await dao.GetTaskRatingsAsync(USName);
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); error = true; return (0, 0); }
    }

}
