using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace JustAwesomeMediaManager.Models
{
    public class EpisodeInfo
    {
        public string Title { get; set; }

        public int ID { get; set; }

        public string ImageFile
        {
            get
            {
                return String.Format("~/Images/thetvdb-{0}.jpg", ID);
            }
        }

        public string ImageUrl
        {
            get
            {
                return VirtualPathUtility.ToAbsolute(ImageFile);
            }
        }
    }
}
