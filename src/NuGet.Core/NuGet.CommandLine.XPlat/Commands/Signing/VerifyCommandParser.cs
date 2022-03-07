// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Commands;
using NuGet.Common;
using static NuGet.Commands.VerifyArgs;

namespace NuGet.CommandLine.XPlat
{
    internal static partial class VerifyCommandParser
    {
        internal static Action<LogLevel> SetLogLevelAction;
        internal static Func<IVerifyCommandRunner> GetCommandRunnerFunc;

        internal static async Task<int> VerifyHandlerAsync(
            string[] packagePaths,
            bool all,
            string[] certificateFingerprint,
            string configfile,
            string verbosity)
        {
            ValidatePackagePaths(packagePaths);

            VerifyArgs args = new VerifyArgs
            {
                PackagePaths = packagePaths,
                Verifications = all ?
                    new List<Verification>() { Verification.All } :
                    new List<Verification>() { Verification.Signatures },
                CertificateFingerprint = certificateFingerprint,
                Logger = GetLoggerFunction(),
                Settings = XPlatUtility.ProcessConfigFile(configfile)
            };
            SetLogLevelAction(XPlatUtility.MSBuildVerbosityToNuGetLogLevel(verbosity));

            var runner = GetCommandRunnerFunc();
            int result = await runner.ExecuteCommandAsync(args);

            return result;
        }

        private static void ValidatePackagePaths(string[] arguments)
        {
            if (arguments.Length == 0 || arguments.Any(packagePath => string.IsNullOrEmpty(packagePath)))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.Error_PkgMissingArgument,
                    "verify",
                    "packagePaths"));
            }
        }
    }
}
