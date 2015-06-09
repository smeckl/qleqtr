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
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;

namespace qleqtr.Shared
{
    public class CommandMessageFactoryImpl
    {
        protected Dictionary<String, Type> typeMap = new Dictionary<string, Type>();

        public CollectionAgentMessage constructMessageFromJSON(String strType, String jsonMsg)
        {
            CollectionAgentMessage retMsg = null;

            Type type = typeMap[strType];

            if (type != null)
            {
                // Read String data into a MemoryStream so it can be deserialized
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonMsg));

                // Deserialize the stream into an object
                DataContractJsonSerializer ser = new DataContractJsonSerializer(type);
                retMsg = ser.ReadObject(ms) as CollectionAgentMessage;

                ms.Close();
            }

            return retMsg;
        }
    }
}
