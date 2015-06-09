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

namespace qleqtr.Shared
{
    // This class defines the error message sent from the CollectionAgent to
    // the client that is communicating with it.
    [DataContract]
    public class CollectionAgentErrorMessage : CollectionAgentMessage
    {
        private static String strRequestType = "CollectionAgentErrorMessage";

        [DataMember]
        public String errorMessage { get; set; }

        public CollectionAgentErrorMessage(ulong ulReqID, String strErrorMsg) : base(ulReqID)
        {
            requestType = strRequestType;
            errorMessage = strErrorMsg;
        }

        public override bool isValid()
        {
            return (base.isValid() && 0 == requestType.CompareTo(strRequestType));
        }
    }
}
