using DropNet.Models;
using WinBox.Utility;

namespace WinBox.Model
{
    public class Quota
    {
        public Quota(QuotaInfo quotaInfo)
        {
            _quotaInfo = quotaInfo;
        }

        public string Used
        {
            get
            {
                return Utilities.FormatBytes(_quotaInfo.normal);
            }
        }

        public string TotalQuota
        {
            get
            {
                return Utilities.FormatBytes(_quotaInfo.quota);
            }
        }

        public string Shared
        {
            get
            {
                return Utilities.FormatBytes(_quotaInfo.shared);
            }
        }

        public string Remaining
        {
            get
            {
                return Utilities.FormatBytes(_quotaInfo.quota - _quotaInfo.normal);
            }
        }

        public long Maximum
        {
            get
            {
                return _quotaInfo.quota;
            }
        }

        public long Value
        {
            get
            {
                return _quotaInfo.normal;
            }
        }

        readonly QuotaInfo _quotaInfo;
    }
}
