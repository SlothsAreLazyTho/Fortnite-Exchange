using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fortnite_Exchange.Responses
{
    class DeviceResponse
    {
        public string deviceId { get; set; }
        public string accountId { get; set;  }
        public string secret { get; set;  }

        public DeviceResponse(string deviceId, string accountId, string secret)
        {
            this.deviceId = deviceId;
            this.accountId = accountId;
            this.secret = secret;
        }

    }
}
