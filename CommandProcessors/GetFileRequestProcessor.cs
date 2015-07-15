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
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using qleqtr.Shared;
using qleqtr.CommandMessages;

namespace qleqtr.CommandProcessors
{
    class GetFileRequestProcessor : ICommandProcessor
    {
        private static int BUFFER_LENGTH = 1024 * 4;

        public String requestType { get; private set; }

        public GetFileRequestProcessor()
        {
            requestType = GetFileRequestMessage.strRequestType;
        }

        public CollectionAgentMessage processCommand(CollectionAgentMessage msg)
        {
            CollectionAgentMessage responseMsg = null;
            GetFileRequestMessage requestMessage = null;

            if (typeof(GetFileRequestMessage) != msg.GetType())
            {
                responseMsg = new CollectionAgentErrorMessage(msg.requestID, "Invalid request type.");
            }
            else // Process message
            {
                requestMessage = (GetFileRequestMessage)msg;

                GetFileResponseMessage fileResp = new GetFileResponseMessage(requestMessage.requestID);

                fileResp.filePath = requestMessage.filePath;

                // Check if the fiel exists.  If so, then read/base64 encode it.
                // TODO:  This needs a more memory-efficient way of handling large files.
                if(File.Exists(requestMessage.filePath))
                {
                    fileResp.fileFound = true;

                    FileStream inStream = File.OpenRead(requestMessage.filePath);
                    byte [] byteBuffer = new byte[BUFFER_LENGTH];
                    int lastOffset = 0;
                    int bytesRead = 0;
                    StringWriter outWriter = new StringWriter();

                    // Read the file BUFFER_LENGTH bytes at a time, base64 encoding it into 
                    // a string in memory.
                    do
                    {
                        bytesRead = inStream.Read(byteBuffer, 0, BUFFER_LENGTH);
                        lastOffset += bytesRead;

                        outWriter.Write(Convert.ToBase64String(byteBuffer, 0, bytesRead));
                    } while (bytesRead == BUFFER_LENGTH);

                    inStream.Close();

                    fileResp.fileData = outWriter.ToString();
                }
                else // Otherwise, set fileFound to false.
                {
                    fileResp.fileFound = false;
                }

                responseMsg = fileResp;
            }


            return responseMsg;
        }
    }
}
