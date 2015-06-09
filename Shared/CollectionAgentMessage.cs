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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace qleqtr.Shared
{
    // This is the base class tht all CollectionAgent message classes are
    // derived from.  It contains the attributes associated with all 
    // messages and the code to serialize the object as JSON.
    [DataContract]
    //[KnownType(typeof(GetRegistryKeyRequestMessage))]
    public class CollectionAgentMessage
    {
        private static String strRequestType = "CollectionAgentMessage";

        public static String RegexPrintableChars = "[ -~]";
        public static String RegexPrintableCharsMinusBackslash = "[ -\\[\\]-~]";

        [DataMember]
        public ulong requestID { get; set; }

        [DataMember]
        public String requestType { get; set; }

        public CollectionAgentMessage() { }

        public CollectionAgentMessage(ulong ulReqID)
        {
            requestID = ulReqID;
            requestType = strRequestType;
        }

        // Serialize the object into a JSON-formatted string.
        public String ToJSON()
        {
            //Create a stream to serialize the object to.
            MemoryStream ms = new MemoryStream();

            // Serializer the User object to the stream.
            DataContractJsonSerializer ser = new DataContractJsonSerializer(this.GetType());
            ser.WriteObject(ms, this);

            byte[] jsonMsg = ms.ToArray();
            ms.Close();

            // Use Decoder class to convert from bytes to UTF8 
            // in case a character spans two buffers.
            Decoder decoder = Encoding.UTF8.GetDecoder();

            int bytes = jsonMsg.Length;
            char[] chars = new char[decoder.GetCharCount(jsonMsg, 0, bytes)];
            decoder.GetChars(jsonMsg, 0, bytes, chars, 0);

            StringBuilder strJSONMsg = new StringBuilder();
            strJSONMsg.Append(chars);
            strJSONMsg.Append("<EOF>");

            return strJSONMsg.ToString();
        }   
        
        // Determine if the object is in a valid state.
        // This method should be overridden by all subclasses.
        public virtual Boolean isValid()
        {
            return (requestID > 0);
        }     
    }
}
