﻿// Copyright 2015 Steve Meckl
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
using qleqtr.Shared;

namespace qleqtr.CommandMessages
{
    public class CommandMessageFactory : CommandMessageFactoryImpl, ICommandMessageFactory
    {
        // Definen the strings for supported messae types
        public String[] supportedMessages { get; } =
        {
            "GetRegistryKeyRequestMessage",
            "GetRegistryKeyResponseMessage",
            "GetFileRequestMessage",
            "GetFileResponseMessage"
        };

        // Add the types to the map
        public CommandMessageFactory()
        {
            typeMap.Add(supportedMessages[0], typeof(GetRegistryKeyRequestMessage));
            typeMap.Add(supportedMessages[1], typeof(GetRegistryKeyResponseMessage));
            typeMap.Add(supportedMessages[2], typeof(GetFileRequestMessage));
            typeMap.Add(supportedMessages[3], typeof(GetFileResponseMessage));
        }
    }
}
