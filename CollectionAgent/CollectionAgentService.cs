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
using System.ServiceProcess;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Json;
using qleqtr.Shared;

namespace qleqtr.CollectionAgent
{
    public partial class CollectionAgentService : ServiceBase
    {
        static X509Certificate serverCertificate = null;
        static string m_certificateFile = ".\\win8-dev.cer";

        private Dictionary<String, ICommandProcessor> processorMap = new Dictionary<String, ICommandProcessor>();

        private Dictionary<String, ICommandMessageFactory> msgFactoryMap = new Dictionary<string, ICommandMessageFactory>();

        private static String[] dllList =
        {
            "CommandProcessors.dll",
            "CommandMessages.dll",
            "Shared.dll"
        };

        private Assembly[] assemblyList = new Assembly[dllList.Length];

        private ArrayList knownTypeList = new ArrayList();

        public CollectionAgentService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            runService(m_certificateFile, 12345);
        }

        protected override void OnStop()
        {
        }

        public void runService(String certificateFile, int port)
        {
            m_certificateFile = certificateFile;

            // Initialize internal data
            Initialize();

            // Create a TCP/IP (IPv4) socket and listen for incoming connections.
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            while (true)
            {
                Console.WriteLine("Waiting for a client to connect...");
                // Application blocks while waiting for an incoming connection. 
                // Type CNTL-C to terminate the server.
                TcpClient client = listener.AcceptTcpClient();
                ProcessClient(client);
            }
        }

        // This method initializes the service by loading the certifiate and loading
        // Processor DLLs
        private void Initialize()
        {
            // Load certificate from a file
            serverCertificate = X509Certificate.CreateFromCertFile(m_certificateFile);


            loadMapsFromDLLList();
        }

        private void loadMapsFromDLLList()
        {
            // Load command processor map
            for (int i = 0; i < dllList.Length; i++)
            {
                // Attempt to load the DLL
                assemblyList[i] = Assembly.LoadFrom(dllList[i]);
               
                // If we successfully loaded the file, then attempt to load interfaces
                if (null != assemblyList[i])
                {
                    // Get the list of types exported from the DLL
                    Type[] allTypes = assemblyList[i].GetTypes();

                    foreach (Type type in allTypes)
                    {
                        // If this is an ICommandProcessor object, add it to the processor map
                        if (!type.IsAbstract && typeof(ICommandProcessor).IsAssignableFrom(type))
                        {
                            // Create a instance of the class
                            ICommandProcessor proc = (ICommandProcessor)Activator.CreateInstance(type);

                            // Add the instance to the processorMap
                            if (null != proc)
                            {
                                processorMap.Add(proc.requestType, proc);
                            }
                        }
                        else if (!type.IsAbstract && typeof(ICommandMessageFactory).IsAssignableFrom(type))
                        {
                            // Create a instance of the class
                            ICommandMessageFactory factory = (ICommandMessageFactory)Activator.CreateInstance(type);

                            // Add the instance to the message factory map
                            if (null != factory)
                            {
                                for(int j = 0; j < factory.supportedMessages.Length; j++)
                                {
                                    msgFactoryMap.Add(factory.supportedMessages[j], factory);
                                }
                            }
                        }
                        else if (!type.IsAbstract && typeof(CollectionAgentMessage).IsAssignableFrom(type))
                        {
                            knownTypeList.Add(type);
                        }
                    }
                }
            }
        }

        private void ProcessClient(TcpClient client)
        {
            // A client has connected. Create the  
            // SslStream using the client's network stream.
            SslStream sslStream = new SslStream(client.GetStream(), false);

            // Authenticate the server but don't require the client to authenticate. 
            sslStream.AuthenticateAsServer(serverCertificate, false, SslProtocols.Tls, true);

            try
            {
                // Set timeouts for the read and write to 5 seconds.
                sslStream.ReadTimeout = 5000;
                sslStream.WriteTimeout = 5000;

                // Read a message from the client.   
                Console.WriteLine("Waiting for client message...");
                CollectionAgentMessage caMsg = ReadMessage(sslStream);            

                if (null != caMsg)
                {
                    Console.WriteLine("Received: {0}", caMsg.ToJSON());

                    // We have a valid query, process it
                    CollectionAgentMessage caResp = processClientQuery(caMsg);                    
                 
                    Console.WriteLine("Sending response message.");
                    sslStream.Write(Encoding.UTF8.GetBytes(caResp.ToJSON()));
                }
                else
                {
                    // Write a response message to the client. 
                    CollectionAgentErrorMessage caResp = new CollectionAgentErrorMessage(caMsg.requestID,
                                                                                         "ERROR parsing JSON request.");

                    Console.WriteLine("Sending ERROR response message.");
                    sslStream.Write(Encoding.UTF8.GetBytes(caResp.ToJSON()));
                }                
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                sslStream.Close();
                client.Close();
                return;
            }
            finally
            {
                // The client stream will be closed with the sslStream 
                // because we specified this behavior when creating 
                // the sslStream.
                sslStream.Close();
                client.Close();
            }
        }

        private CollectionAgentMessage ReadMessage(SslStream sslStream)
        {
            CollectionAgentMessage deserializedMsg = null;

            // Read the  message sent by the client. 
            // The client signals the end of the message using the 
            // "<EOF>" marker.
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            do
            {
                // Read the client's test message.
                bytes = sslStream.Read(buffer, 0, buffer.Length);

                // Use Decoder class to convert from bytes to UTF8 
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();

                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);

                if (messageData.ToString().IndexOf("<EOF>") != -1)
                {                    
                    break;
                }
            } while (bytes != 0);

            String strJSON = messageData.ToString();

            // If there is a trailing <EOF> character, strip it so that JSON
            // deserialization will work correctly
            int index = (strJSON.IndexOf("<EOF>"));
            if (index != -1)
            {
                strJSON = strJSON.Substring(0, index);
            }

            // Read String data into a MemoryStream so it can be deserialized
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(strJSON));

            // Deserialize the stream into an object
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(CollectionAgentMessage));
            
            CollectionAgentMessage baseMsg = ser.ReadObject(ms) as CollectionAgentMessage;

            ms.Close();

            if(null != baseMsg)
            {
                ICommandMessageFactory factory = msgFactoryMap[baseMsg.requestType];

                if (null != factory)
                    deserializedMsg = factory.constructMessageFromJSON(baseMsg.requestType, strJSON);
            }

            // Return the new object
            return deserializedMsg;
        }

        // This method is responsible for processing a query based on a message passed
        // by a client.
        // For now, this is a skeleton function only.  As reall queries are created, this function
        // will be populated with calls to code that will actually process and respond to the
        // queries.
        private CollectionAgentMessage processClientQuery(CollectionAgentMessage caMsg)
        {
             // Write a response message to the client. 
            CollectionAgentMessage caResp = null;

            // Get the appropriate ICommandProcessor object based on the request type.
            ICommandProcessor processor = processorMap[caMsg.requestType];

            // If the object is not null, then call processCommand()
            if (null != processor)
                caResp = processor.processCommand(caMsg);
            else
                caResp = new CollectionAgentErrorMessage(caMsg.requestID, "ERROR:  Invalid request type.");

            return caResp.isValid() ? caResp : null;
        }
    }
}
