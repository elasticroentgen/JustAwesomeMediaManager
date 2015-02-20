using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Xml;

namespace JustAwesomeMediaManager.Models
{
    public class TheTVDB
    {
        private readonly string APIKEY = "B40C19FE1802CB27";

        private int _seriesid;
        private XmlDocument fullxml;

        public TheTVDB(int seriesid)
        {
            _seriesid = seriesid;
        }


        public void Fetch(string lang)
        {

            string localFile = HostingEnvironment.MapPath(String.Format("~/App_Data/thetvdb/{0}-{1}.xml",_seriesid,lang));

            string xmlstr = String.Empty;

            if(File.Exists(localFile))
                xmlstr = File.ReadAllText(localFile);
            else 
            {
                string url = String.Format("http://thetvdb.com/api/{0}/series/{1}/all/{2}.xml",APIKEY,_seriesid,lang);
                WebClient wc = new WebClient();
                xmlstr = wc.DownloadString(url);
                File.WriteAllText(localFile, xmlstr);
            }

            fullxml = new XmlDocument();
            fullxml.LoadXml(xmlstr);

        }

        public EpisodeInfo GetEpisodeInfo(int season, int episode)
        {
            if (fullxml == null)
                Fetch("de");

            foreach(XmlNode nd in fullxml.SelectNodes("/Data/Episode"))
            {
                if (int.Parse(nd.SelectSingleNode("EpisodeNumber").InnerText) == episode && int.Parse(nd.SelectSingleNode("SeasonNumber").InnerText) == season)
                {
                    EpisodeInfo ei = new EpisodeInfo();
                    ei.Title = nd.SelectSingleNode("EpisodeName").InnerText;
                    ei.ID = int.Parse(nd.SelectSingleNode("id").InnerText);

                    string imgfile = nd.SelectSingleNode("filename").InnerText;

                    //download episode image
                    if(!File.Exists(HostingEnvironment.MapPath(ei.ImageFile)))
                    {
                        string url = String.Format("http://thetvdb.com/banners/{0}", imgfile);
                        WebClient wc = new WebClient();
                        wc.DownloadFile(url, HostingEnvironment.MapPath(ei.ImageFile));
                    }

                    return ei;
                }
            }

            throw new ArgumentException(String.Format("Episode S{0}E{1} unkown!",season,episode));
        }

        public static int FindSeries(string name)
        {
            string url = String.Format("http://thetvdb.com/api/GetSeries.php?seriesname={0}&language=all", HttpUtility.UrlEncode(name));

            WebClient wc = new WebClient();
            string xmlstr = wc.DownloadString(url);

            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(xmlstr);
                return int.Parse(xdoc.SelectNodes("/Data/Series/seriesid")[0].InnerText);
            }
            catch
            {
                return -1;
            }
            
            
        }



        internal Dictionary<int, Dictionary<int, bool>> GetEpisodeDict()
        {

            Dictionary<int, Dictionary<int, bool>> ep = new Dictionary<int, Dictionary<int, bool>>();
            if (fullxml == null)
                Fetch("de");

            foreach(XmlNode nd in fullxml.SelectNodes("/Data/Episode"))
            {
                
                int epnr = int.Parse(nd.SelectSingleNode("EpisodeNumber").InnerText);
                int seasonnr = int.Parse(nd.SelectSingleNode("SeasonNumber").InnerText);

                if (!ep.ContainsKey(seasonnr))
                    ep.Add(seasonnr, new Dictionary<int, bool>());
                if (!ep[seasonnr].ContainsKey(epnr))
                    ep[seasonnr].Add(epnr, false);

            }

            return ep;
        }
    }
}