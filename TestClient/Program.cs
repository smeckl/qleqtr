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
using System.Collections;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using qleqtr.Shared;
using qleqtr.CommandMessages;

namespace qleqtr.TestClient
{
    class Program
    {
        private static Hashtable certificateErrors = new Hashtable();

        // The following method is invoked by the RemoteCertificateValidationDelegate. 
        public static bool ValidateServerCertificate(
                                                      object sender,
                                                      X509Certificate certificate,
                                                      X509Chain chain,
                                                      SslPolicyErrors sslPolicyErrors)
        {
             if (sslPolicyErrors == SslPolicyErrors.None
                || sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers. 
            return false;
        }

        public static void RunClient(string machineName, string serverName)
        {
            // Create a TCP/IP client socket. 
            // machineName is the host running the server application.
            TcpClient client = new TcpClient(machineName, 12345);

            Console.WriteLine("Client connected.");

            // Create an SSL stream that will close the client's stream.
            SslStream sslStream = new SslStream(client.GetStream(),
                                                false,
                                                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                                                null
                                                );

            // The server name must match the name on the server certificate. 
            try
            {
                sslStream.AuthenticateAsClient(serverName);
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                client.Close();
                return;
            }

            GetRegistryKeyRequestMessage msg = new GetRegistryKeyRequestMessage(1, RootKey.HKEY_CURRENT_USER, "Software\\CollectionAgentTest");
            //CollectionAgentMessage msg = new CollectionAgentMessage(1);

            // Send the message to the CollectionAgent
            SendMessage(sslStream, msg);

            // Read message from the server. 
            CollectionAgentMessage caMsg = ReadMessage(sslStream);

            Console.WriteLine("Server says: {0}", caMsg.ToJSON());

            // Close the client connection.
            client.Close();
            Console.WriteLine("Client closed.");
        }
        static CollectionAgentMessage ReadMessage(SslStream sslStream)
        {
            // Read the  message sent by the server. 
            // The end of the message is signaled using the 
            // "<EOF>" marker.
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;

            do
            {
                bytes = sslStream.Read(buffer, 0, buffer.Length);

                // Use Decoder class to convert from bytes to UTF8 
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);

                // Check for EOF. 
                if (messageData.ToString().IndexOf("<EOF>") != -1)
                {
                    break;
                }
            } while (bytes != 0);

            Console.WriteLine(messageData.ToString());

            String strJSON = messageData.ToString();

            // If there is a trailing <EOF> character, strip it so that JSON
            // deserialization will work correctly
            int index = (strJSON.IndexOf("<EOF>"));
            if (index != -1)
            {
                strJSON = strJSON.Substring(0, index);
            }

            ICommandMessageFactory factory = new CommandMessageFactory();

            GetRegistryKeyResponseMessage deserializedMsg = 
                (GetRegistryKeyResponseMessage)factory.constructMessageFromJSON("GetRegistryKeyResponseMessage", strJSON);

            // Return the new object
            return deserializedMsg;
        }

        private static void SendMessage(SslStream sslStream, CollectionAgentMessage message)
        {
            Console.WriteLine(message.ToJSON());

            // Send hello message to the server. 
            sslStream.Write(Encoding.UTF8.GetBytes(message.ToJSON()));
            sslStream.Flush();
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("To start the client specify:");
            Console.WriteLine("clientSync machineName [serverName]");
            Environment.Exit(1);
        }

        public static int Main(string[] args)
        {
            string serverCertificateName = null;
            string machineName = null;

            if (args == null || args.Length < 1)
            {
                DisplayUsage();
            }

            // User can specify the machine name and server name. 
            // Server name must match the name on the server's certificate. 
            machineName = args[0];
            if (args.Length < 2)
            {
                serverCertificateName = machineName;
            }
            else
            {
                serverCertificateName = args[1];
            }

            Program.RunClient(machineName, serverCertificateName);
            return 0;
        }
    }
}
