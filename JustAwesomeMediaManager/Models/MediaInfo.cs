using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace JustAwesomeMediaManager.Models
{
    public enum Resolution { SD, HD720, HD1080 }
    public enum Lang { German, English }

    public enum DataState { OK, Faulty}

    public class MediaInfo
    {
        public Guid Id { get; set; } 
        public Dictionary<string, object> RawInfo { get; set; }

        public string SeriesTitle { get; set; }

        public DataState State { get; set; }

        public int Season { get; set; }
        public int Episode { get; set; }

        public int TVDBSeriesID { get; set; }

        public EpisodeInfo Info { get; set; }

        public Resolution VideoResolution {get;set;}

        public string VideoResolutionText { get { return VideoResolution.ToString(); } }

        public List<AudioStream> AudioStreams { get; set; }

        public MediaInfo()
        {
            AudioStreams = new List<AudioStream>();
            State = DataState.Faulty;
            TVDBSeriesID = -1;
        }

        public MediaInfo(Dictionary<string,object> info)
        {
            AudioStreams = new List<AudioStream>();
            //RawInfo = info;
            RawInfo = new Dictionary<string, object>();
            State = DataState.Faulty;

            TVDBSeriesID = -1;

            string filename = ((JObject)(info["format"]))["filename"].ToString();
            string[] fileparts = filename.Split('\\');
            SeriesTitle = fileparts[fileparts.Length - 3];

            Match rm = Regex.Match(filename, "[S,s][0-9]+[E,e][0-9]+");

            if(rm.Success)
            {
                Match innerMatch = Regex.Match(rm.Value,"[0-9]+");
                Season = int.Parse(innerMatch.Value);
                Episode = int.Parse(innerMatch.NextMatch().Value);
            }


            foreach(JObject stream in ((JArray)info["streams"]))
            {
                if(stream["codec_type"].ToString() == "video")
                {
                    int height = (int)stream["height"];
                    if (height >= 1080)
                        VideoResolution = Resolution.HD1080;
                    else if (height >= 720)
                        VideoResolution = Resolution.HD720;
                    else
                        VideoResolution = Resolution.SD;
                }
                else if (stream["codec_type"].ToString() == "audio")
                {
                    AudioStream _as = new AudioStream();
                    _as.Channels = (int)stream["channels"];
                    _as.Language = Lang.German;

                    try
                    {
                        string lang = stream["tags"]["language"].ToString();
                        if (lang == "eng")
                            _as.Language = Lang.English;
                    }
                    catch {}

                    AudioStreams.Add(_as);
                }

            }
        }

        
    }
}