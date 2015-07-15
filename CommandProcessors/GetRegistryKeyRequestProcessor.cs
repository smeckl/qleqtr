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
using Microsoft.Win32;
using qleqtr.Shared;
using qleqtr.CommandMessages;

namespace qleqtr.CommandProcessors
{
    public class GetRegistryKeyRequestProcessor : ICommandProcessor
    {
        public String requestType { get; private set; }

        public GetRegistryKeyRequestProcessor()
        {
            requestType = "GetRegistryKeyRequestMessage";
        }

        public CollectionAgentMessage processCommand(CollectionAgentMessage msg)
        {
            CollectionAgentMessage responseMsg = null;
            GetRegistryKeyRequestMessage requestMessage = null;

            if(typeof(GetRegistryKeyRequestMessage) != msg.GetType())
            {
                responseMsg = new CollectionAgentErrorMessage(msg.requestID, "Invalid request type.");
            }
            else
            {
                RegistryKey regKey = null;

                requestMessage = (GetRegistryKeyRequestMessage)msg;

                // Open the key from the appropriate root key
                switch (requestMessage.root)
                {
                    case RootKey.HKEY_CLASSES_ROOT:
                        regKey = Registry.ClassesRoot.OpenSubKey(requestMessage.keyPath, false);
                        break;

                    case RootKey.HKEY_CURRENT_CONFIG:
                        regKey = Registry.CurrentConfig.OpenSubKey(requestMessage.keyPath, false);
                        break;

                    case RootKey.HKEY_CURRENT_USER:
                        regKey = Registry.CurrentUser.OpenSubKey(requestMessage.keyPath, false);
                        break;

                    case RootKey.HKEY_LOCAL_MACHINE:
                        regKey = Registry.LocalMachine.OpenSubKey(requestMessage.keyPath, false);
                        break;

                    case RootKey.HKEY_USERS:
                        regKey = Registry.Users.OpenSubKey(requestMessage.keyPath, false);
                        break;
                }

                // If we found the key, then read the values and 
                if (null != regKey)
                {
                    GetRegistryKeyResponseMessage regResponse = new GetRegistryKeyResponseMessage(requestMessage.requestID);

                    // Set the path on the Registry key
                    regResponse.regKey.path = requestMessage.keyPath;

                    // Populate the Registry key with values and subkeys
                    populateRegistrykey(regResponse.regKey, regKey);

                    // Set the return value
                    responseMsg = regResponse;
                }
                else // send an error message instead
                {
                    responseMsg = new CollectionAgentErrorMessage(requestMessage.requestID, "Registry key not found");
                }
            }

            return responseMsg;
        }

        private void populateRegistrykey(qleqtr.CommandMessages.RegKey respKey, RegistryKey sysKey)
        {
            // First, populate the values for this key
            populateRegKeyValues(respKey, sysKey);

            // Second, recursively populate sub-keys for this key
            String[] subKeyNames = sysKey.GetSubKeyNames();

            if (subKeyNames.Length > 0)
            {
                RegKey[] childKeys = new RegKey[subKeyNames.Length];

                for (int i = 0; i < subKeyNames.Length; i++)
                {
                    RegistryKey subKey = sysKey.OpenSubKey(subKeyNames[i], false);

                    // Allocate a new RegKey to hold the subkey
                    childKeys[i] = new RegKey();

                    // Populate the subkey
                    populateRegistrykey(childKeys[i], subKey);
                }

                respKey.subKeys = childKeys;
            }
        }

        private void populateRegKeyValues(qleqtr.CommandMessages.RegKey respKey, RegistryKey sysKey)
        {
            String[] valNames = sysKey.GetValueNames();

            if (valNames.Length > 0)
            {
                RegValue[] values = new RegValue[valNames.Length];

                for (int i = 0; i < valNames.Length; i++)
                {
                    RegistryValueKind valKind = sysKey.GetValueKind(valNames[i]);

                    RegValue val = null;

                    switch (valKind)
                    {
                        case RegistryValueKind.String:
                            val = new RegStringValue(valNames[i], (String)sysKey.GetValue(valNames[i]));
                            break;

                        case RegistryValueKind.Binary:
                            val = new RegBinaryValue(valNames[i], (byte[])sysKey.GetValue(valNames[i]));
                            break;

                        case RegistryValueKind.DWord:
                            val = new RegDWORDValue(valNames[i], (int)sysKey.GetValue(valNames[i]));
                            break;

                        case RegistryValueKind.ExpandString:
                            val = new RegExpandStringValue(valNames[i],
                                                (String)sysKey.GetValue(valNames[i], null, RegistryValueOptions.DoNotExpandEnvironmentNames),
                                                (String)sysKey.GetValue(valNames[i]));
                            break;

                        case RegistryValueKind.MultiString:
                            val = new RegMultiStringValue(valNames[i], (String[])sysKey.GetValue(valNames[i]));
                            break;

                        case RegistryValueKind.QWord:
                            val = new RegQWORDValue(valNames[i], (long)sysKey.GetValue(valNames[i]));
                            break;

                        case RegistryValueKind.Unknown:
                            val = null; // Can't handle Unknown Type
                            break;

                        case RegistryValueKind.None:
                            val = null; // Can't handle None Type
                            break;
                    }
                    // Save the value
                    values[i] = (RegValue)val;
                }

                respKey.values = values;
            }
        }
    }
}
