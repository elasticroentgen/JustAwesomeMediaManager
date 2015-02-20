using JustAwesomeMediaManager.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Threading;

namespace JustAwesomeMediaManager
{
    public class LibraryUpdater
    {
        // Singleton instance
        private readonly static Lazy<LibraryUpdater> _instance = new Lazy<LibraryUpdater>(
            () => new LibraryUpdater(GlobalHost.ConnectionManager.GetHubContext<MediaHub>().Clients));

        private readonly object _marketStateLock = new object();
        private readonly object _updateStockPricesLock = new object();

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(250);
        private readonly Random _updateOrNotRandom = new Random();

        private Dictionary<string, int> seriesids = new Dictionary<string, int>();
        private int _totalCt;

        private List<string> badfiles = new List<string>();

        private LibraryUpdater(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
        }

        public static LibraryUpdater Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        public void RunUpdate()
        {
            string directory = "\\\\scotty\\Videos\\Serien";

            //scann all directories

            _totalCt = Directory.GetDirectories(directory).Length;
            _currentCt = 0;

            scanFiles(directory,true);
        }

        private void scanFiles(string directory, bool updateDirProgress)
        {

            foreach (string file in Directory.GetFiles(directory))
            {
                Clients.All.currentFile(file,0);
                var minfo = getMediaInfo(file);

                try
                {
                    MediaInfo mi = new MediaInfo(minfo);

                    int sid = 0;
                    if (seriesids.ContainsKey(mi.SeriesTitle))
                        sid = seriesids[mi.SeriesTitle];
                    else
                    {
                        sid = TheTVDB.FindSeries(mi.SeriesTitle);
                        seriesids.Add(mi.SeriesTitle, sid);
                    }

                    mi.TVDBSeriesID = sid;

                    TheTVDB tvdb = new TheTVDB(sid);
                    mi.Info = tvdb.GetEpisodeInfo(mi.Season, mi.Episode);
                    mi.State = DataState.OK;

                    SaveToMongoAsync(mi);
                    Clients.All.currentMedia(mi);
                }
                catch (Exception e)
                {
                    badfiles.Add(file);
                    Clients.All.faultyFile(file,e.Message);

                }

            }

            //scan subfolders
            foreach (string dir in Directory.GetDirectories(directory))
            {
                if(updateDirProgress)
                    _currentCt++;
                
                Clients.All.currentDir(directory,_currentCt);
                scanFiles(dir,false);
            }
        }


        private void SaveToMongoAsync(MediaInfo mi)
        {
            
            var connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase("jamm");
            var coll = db.GetCollection<MediaInfo>("episodes");

            coll.FindOneAndDeleteAsync(x => x.SeriesTitle == mi.SeriesTitle && x.Season == mi.Season && x.Episode == mi.Episode);
            coll.InsertOneAsync(mi);
        }

        private Dictionary<string, object> getMediaInfo(string file)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "c:\\ffmpeg\\ffprobe.exe";
            psi.Arguments = String.Format("-v quiet -print_format json -show_format -show_streams \"{0}\"",file);
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;

            var p = Process.Start(psi);
            string json = p.StandardOutput.ReadToEnd();
            Dictionary<string, object> mediaDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            });
            return mediaDict;
        }

        public int _currentCt { get; set; }
    }
}