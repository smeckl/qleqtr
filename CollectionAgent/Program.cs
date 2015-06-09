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

namespace qleqtr.CollectionAgent
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            // If there are no command-line arguments, then run this as a service
            if (args.Length < 1)
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new CollectionAgentService()
                };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                // If there are no command-line arguments, then run this as a service
                CollectionAgentService svc = new CollectionAgentService();
                svc.runService(args[0], Convert.ToInt32(args[1]));
            }
        }
    }
}
