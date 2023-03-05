using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VetClinicServer
{
    [Serializable]
    public class RequestData
    {
        public RequestData(string email, string aadId, string requestType, string additionalData)//, string requestSendData, float requestVersion)
        {
            Email = email;
            AadId = aadId;
            RequestType = requestType;
            AdditionalData = additionalData;
            //RequestSendData = requestSendData;
            //RequestVersion = requestVersion;
        }

        public string Email { get; set; }

        public string AadId { get; set; }

        public string RequestType { get; set; }

        public string AdditionalData { get; set; }

        //public string RequestSendData { get; set; }

        //public float RequestVersion { get; set; }
    }
}
