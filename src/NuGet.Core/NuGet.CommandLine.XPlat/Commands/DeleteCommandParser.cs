// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using NuGet.Commands;
using NuGet.Configuration;
using NuGet.Credentials;

namespace NuGet.CommandLine.XPlat
{
    internal partial class DeleteCommandParser
    {
        public static async Task DeleteHandlerAsync(
            bool forceEnglishOutput,
            string source,
            bool nonInteractive,
            string apiKey,
            string packageId,
            string packageVersion,
            bool noServiceEndpoint,
            bool interactive)
        {
            if (string.IsNullOrEmpty(packageId) || string.IsNullOrEmpty(packageVersion))
            {
                throw new ArgumentException(Strings.Delete_MissingArguments);
            }

            DefaultCredentialServiceUtility.SetupDefaultCredentialService(GetLoggerFunction(), interactive);

#pragma warning disable CS0618 // Type or member is obsolete
            PackageSourceProvider sourceProvider = new PackageSourceProvider(XPlatUtility.GetSettingsForCurrentWorkingDirectory(), enablePackageSourcesChangedEvent: false);
#pragma warning restore CS0618 // Type or member is obsolete

            await DeleteRunner.Run(
                sourceProvider.Settings,
                sourceProvider,
                packageId,
                packageVersion,
                source,
                apiKey,
                nonInteractive,
                noServiceEndpoint,
                Confirm,
                GetLoggerFunction());
        }

        private static bool Confirm(string description)
        {
            ConsoleColor currentColor = ConsoleColor.Gray;
            try
            {
                currentColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, Strings.ConsoleConfirmMessage, description));
                var result = Console.ReadLine();
                return result.StartsWith(Strings.ConsoleConfirmMessageAccept, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                Console.ForegroundColor = currentColor;
            }
        }
    }
}
