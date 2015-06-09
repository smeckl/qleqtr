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
    public enum RootKey
    {
        HKEY_CLASSES_ROOT,
        HKEY_CURRENT_USER,
        HKEY_LOCAL_MACHINE,
        HKEY_USERS,
        HKEY_CURRENT_CONFIG
    };

    // This class demonstrates how to derive, serialize, and
    // deserialize message classes derived from CollectionAgentMessage
    [DataContract]
    public class GetRegistryKeyRequestMessage : CollectionAgentMessage
    {
        private static String strRequestType = "GetRegistryKeyRequestMessage";
        private static String keyPathRegEx = "^" + CollectionAgentMessage.RegexPrintableChars + "*$";

        [DataMember]
        public RootKey root { get; set; }

        [DataMember]
        public String keyPath { get; set; }

        public GetRegistryKeyRequestMessage(ulong ulReqID, RootKey rootKey, String strKeyPath) : base(ulReqID)
        {
            requestType = strRequestType;
            root = rootKey;
            keyPath = strKeyPath;
        }

        // This object is valid if the path is a valid Registry path and the
        // Registy root is a valid root path.
        public override bool isValid()
        {
            bool retVal = true;

            Regex pathRegEx = new Regex(keyPathRegEx, RegexOptions.IgnoreCase);

            if (!pathRegEx.IsMatch(keyPath))
                retVal = false;

            return (base.isValid() && 0 == requestType.CompareTo(strRequestType) && retVal);
        }
    }
}
