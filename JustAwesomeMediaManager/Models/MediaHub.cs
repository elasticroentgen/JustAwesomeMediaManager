using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace JustAwesomeMediaManager
{
    [HubName("mediaHub")]
    public class MediaHub : Hub
    {
        private readonly LibraryUpdater _libUpdater;

        public MediaHub()
            : this(LibraryUpdater.Instance)
        {
        }

        public MediaHub(LibraryUpdater libUpdater)
        {
            _libUpdater = libUpdater;
        }

        public void RunUpdate()
        {
            _libUpdater.RunUpdate();
        }
    }
}