// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Globalization;
using NuGet.Commands;

namespace NuGet.CommandLine.XPlat
{
    internal static partial class LocalsCommandParser
    {
        internal static void LocalsHandler(
            bool forceEnglishOutput,
            bool clear,
            bool list,
            string arguments)
        {
            var logger = GetLoggerFunction();
            var setting = XPlatUtility.GetSettingsForCurrentWorkingDirectory();

            // Using both -clear and -list command options, or neither one of them, is not supported.
            // We use MinArgs = 0 even though the first argument is required,
            // to avoid throwing a command argument validation exception and
            // immediately show usage help for this command instead.
            if ((arguments.Length < 1) || string.IsNullOrWhiteSpace(arguments))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.LocalsCommand_NoArguments));
            }
            else if (clear && list)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.LocalsCommand_MultipleOperations));
            }
            else if (!clear && !list)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.LocalsCommand_NoOperation));
            }
            else
            {
                var localsArgs = new LocalsArgs(new List<string>() { arguments },
                    setting,
                    logger.LogInformation,
                    logger.LogError,
                    clear,
                    list);

                var localsCommandRunner = new LocalsCommandRunner();
                localsCommandRunner.ExecuteCommand(localsArgs);
            }
        }
    }
}
