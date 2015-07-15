// Copyright 2015 Steve Meckl
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using qleqtr.Shared;

namespace qleqtr.CommandMessages
{
    [DataContract]
    public class GetFileResponseMessage : CollectionAgentMessage
    {
        private static String strRequestType = "GetFileResponseMessage";
        private static String strFilePathRegEx = @"(([a-z]:|\\\\[a-z0-9_.$]+\\[a-z0-9_.$]+)?(\\?(?:[^\\/:*?""<>|\r\n]+\\)+)[^\\/:*?""<>|\r\n]+)";
        private static String fileDataRegEx = @"^[a-zA-Z0-9\+\/\=]*$";

        [DataMember]
        public bool fileFound { get; set; }

        [DataMember]
        public String filePath { get; set; }

        [DataMember]
        // This member is a Base64 encoded string  containing the file data
        public String fileData { get; set; }

        public GetFileResponseMessage(ulong ulReqID) : base(ulReqID)
        {
            requestType = "GetFileResponseMessage";
        }

        public override bool isValid()
        {
            bool retVal = true;

            Regex pathRegEx = new Regex(strFilePathRegEx, RegexOptions.IgnoreCase);

            if (!pathRegEx.IsMatch(filePath))
                retVal = false;

            Regex dataRegEx = new Regex(fileDataRegEx);

            if (null != fileData && !dataRegEx.IsMatch(fileData))
                retVal = false;

            return (retVal && base.isValid() && 0 == requestType.CompareTo(strRequestType) );
        }
    }
}
