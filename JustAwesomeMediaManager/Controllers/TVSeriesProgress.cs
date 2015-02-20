using JustAwesomeMediaManager.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace JustAwesomeMediaManager.Controllers
{
    public class TVSeriesProgress
    {
        public string Title {get; private set;}

        public Dictionary<int, Dictionary<int, bool>> Episodes { get; private set;}

        public TVSeriesProgress(string title)
        {
            // TODO: Complete member initialization
            this.Title = title;
            Episodes = new Dictionary<int, Dictionary<int, bool>>();

            //get all episodes from the XML
            var connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase("jamm");
            var coll = db.GetCollection<MediaInfo>("episodes");

            int tvdbid = coll.Find(x => x.SeriesTitle == title).FirstAsync().Result.TVDBSeriesID;

            TheTVDB tvdb = new TheTVDB(tvdbid);
            Episodes = tvdb.GetEpisodeDict();

            //now try to find all episodes in the database
            foreach(MediaInfo mi in coll.Find(x => x.SeriesTitle == title).ToListAsync().Result)
            {
                if (Episodes.ContainsKey(mi.Season) && Episodes[mi.Season].ContainsKey(mi.Episode))
                    Episodes[mi.Season][mi.Episode] = true;
            }

        }
    }
}
