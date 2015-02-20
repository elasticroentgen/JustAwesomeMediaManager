using JustAwesomeMediaManager.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JustAwesomeMediaManager.Controllers
{
    public class CollectionProgressModel
    {

        public List<TVSeriesProgress> AllSeries;

        public CollectionProgressModel()
        {
            AllSeries = new List<TVSeriesProgress>();
        }

        internal void Fetch()
        {
            var connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase("jamm");
            var coll = db.GetCollection<MediaInfo>("episodes");

            var task1 = coll.DistinctAsync(x => x.SeriesTitle, x => true);
            var allseries = task1.Result;

            foreach(string title in allseries.ToListAsync().Result)
            {
                TVSeriesProgress tvp = new TVSeriesProgress(title);
                AllSeries.Add(tvp);
            }

        }
    }
}
