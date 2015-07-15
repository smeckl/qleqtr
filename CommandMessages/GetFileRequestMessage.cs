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
    public class GetFileRequestMessage : CollectionAgentMessage
    {
        public static String strRequestType = "GetFileRequestMessage";
        private static String strFilePathRegEx = @"(([a-z]:|\\\\[a-z0-9_.$]+\\[a-z0-9_.$]+)?(\\?(?:[^\\/:*?""<>|\r\n]+\\)+)[^\\/:*?""<>|\r\n]+)";

        [DataMember]
        public String filePath { get; set; }

        public GetFileRequestMessage(ulong ulReqID, String strFilePath) : base(ulReqID)
        {
            filePath = strFilePath;
            requestType = strRequestType;
        }

        // This object is valid if the path is valid.
        public override bool isValid()
        {
            bool retVal = true;

            Regex pathRegEx = new Regex(strFilePathRegEx, RegexOptions.IgnoreCase);

            if (!pathRegEx.IsMatch(filePath))
                retVal = false;

            return (base.isValid() && 0 == requestType.CompareTo(strRequestType) && retVal);
        }
    }
}
