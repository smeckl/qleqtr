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
using qleqtr.Shared;

namespace qleqtr.CommandMessages
{
    [DataContract]
    public class RegKey
    {
        [DataMember]
        public String path { get; set; }

        [DataMember]
        public RegKey[] subKeys { get; set; }
        [DataMember]
        public RegValue[] values { get; set; }

        // A RegKey object is valid if all sub-keys are valid and all
        // values are valid.
        public bool isValid()
        {
            bool bRet = true;

            for(int i = 0; null != subKeys && bRet && i < subKeys.Length; i++)
            {
                if(!subKeys[i].isValid())
                    bRet = false;
            }

            for(int i = 0; null != values && bRet && i < values.Length; i++)
            {
                if (!values[i].isValid())
                    bRet = false;
            }

            return bRet;
        }
    }
}
