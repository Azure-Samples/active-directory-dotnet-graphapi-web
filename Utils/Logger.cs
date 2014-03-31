// Copyright © Microsoft
//
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION
// ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A
// PARTICULAR PURPOSE, MERCHANTABILITY OR NON-INFRINGEMENT.
//
// See the Apache License, Version 2.0 for the specific language
// governing permissions and limitations under the License.

namespace AadExplorer.Utils
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Trace based logger
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Log errors and exceptions.
        /// </summary>
        /// <param name="message">Formatted message.</param>
        /// <param name="args">Message arguments.</param>
        public static void Error(string message, params object[] args)
        {
            Trace.TraceError(message, args);
            Console.WriteLine(message, args);
        }

        /// <summary>
        /// Log warnings.
        /// </summary>
        /// <param name="message">Formatted message.</param>
        /// <param name="args">Message arguments.</param>
        public static void Warning(string message, params object[] args)
        {
            Trace.TraceWarning(message, args);
            Console.WriteLine(message, args);
        }

        /// <summary>
        /// Log information.
        /// </summary>
        /// <param name="message">Formatted message.</param>
        /// <param name="args">Message arguments.</param>
        public static void Info(string message, params object[] args)
        {
            Trace.TraceInformation(message, args);
            Console.WriteLine(message, args);
        }
    }
}