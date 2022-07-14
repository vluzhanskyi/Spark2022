using System;
using System.Collections.Generic;
using System.Text;

namespace PlaybackModels
{
    public class PlaybackBulkRequest
    {
        public int? SiteId { get; set; }


        #region Authentication
        public string ApplicationId { get; set; }

        public string ApplicationUserToken { get; set; }

        public string ApplicationUserId { get; set; }

        public string ApplicationUserName { get; set; }

        #endregion

        public List<PlaybackBulkItem> Calls { get; set; }
    }

    public class PlaybackBulkItem
    {
        public string CallId { get; set; }
        public MediaOutputType MediaOutputType { get; set; }
    }
}
