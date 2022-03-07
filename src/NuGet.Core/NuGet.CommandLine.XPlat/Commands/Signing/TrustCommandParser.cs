// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using NuGet.Commands;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Signing;
using static NuGet.Commands.TrustedSignersArgs;

namespace NuGet.CommandLine.XPlat
{
    internal static partial class TrustCommandParser
    {
        internal static Action<LogLevel> SetLogLevelAction;

        internal static async Task<int> TrustListHandlerAsync(
            string configfile,
            string verbosity)
        {
            return await ExecuteCommandAsync(TrustCommand.List, algorithm: null, allowUntrustedRootOption: false, owners: null, verbosity, configfile);
        }

        internal static async Task<int> TrustSyncHandlerAsync(
            string configfile,
            string verbosity,
            string name)
        {
            return await ExecuteCommandAsync(TrustCommand.Sync, algorithm: null, allowUntrustedRootOption: false, owners: null, verbosity, configfile, name: name);
        }

        internal static async Task<int> TrustRemoveHandlerAsync(
            string configfile,
            string verbosity,
            string name)
        {
            return await ExecuteCommandAsync(TrustCommand.Remove, algorithm: null, allowUntrustedRootOption: false, owners: null, verbosity, configfile, name: name);
        }

        internal static async Task<int> TrustAuthorHandlerAsync(
            bool allowUntrustedRoot,
            string configfile,
            string verbosity,
            string name,
            string package)
        {
            return await ExecuteCommandAsync(TrustCommand.Author, algorithm: null, allowUntrustedRoot, owners: null, verbosity, configfile, name: name, sourceUrl: null, packagePath: package);
        }

        internal static async Task<int> TrustRepositoryHandlerAsync(
            bool allowUntrustedRoot,
            string configfile,
            string owners,
            string verbosity,
            string name,
            string package)
        {
            return await ExecuteCommandAsync(TrustCommand.Repository, algorithm: null, allowUntrustedRoot, owners: owners, verbosity, configfile, name: name, sourceUrl: null, packagePath: package);
        }

        internal static async Task<int> TrustCertificateHandlerAsync(
            string algorithm,
            bool allowUntrustedRoot,
            string configfile,
            string verbosity,
            string name,
            string fingerprint)
        {
            return await ExecuteCommandAsync(TrustCommand.Certificate, algorithm, allowUntrustedRoot, owners: null, verbosity, configfile, name: name, sourceUrl: null, packagePath: null, fingerprint: fingerprint);
        }

        internal static async Task<int> TrustSourceHandlerAsync(
            string configfile,
            string owners,
            string sourceUrl,
            string verbosity,
            string name)
        {
            return await ExecuteCommandAsync(TrustCommand.Source, algorithm: null, allowUntrustedRootOption: false, owners, verbosity, configfile, name: name, sourceUrl: sourceUrl);
        }

        internal static async Task<int> TrustHandlerAsync(
            string configfile,
            string verbosity)
        {
            return await ExecuteCommandAsync(TrustCommand.List, algorithm: null, allowUntrustedRootOption: false, owners: null, verbosity, configfile);
        }

        private static async Task<int> ExecuteCommandAsync(TrustCommand action,
                      string algorithm,
                      bool allowUntrustedRootOption,
                      string owners,
                      string verbosity,
                      string configFile,
                      string name = null,
                      string sourceUrl = null,
                      string packagePath = null,
                      string fingerprint = null)
        {
            ILogger logger = GetLoggerFunction();

            try
            {
                ISettings settings = XPlatUtility.ProcessConfigFile(configFile);

                var trustedSignersArgs = new TrustedSignersArgs()
                {
                    Action = MapTrustEnumAction(action),
                    PackagePath = packagePath,
                    Name = name,
                    ServiceIndex = sourceUrl,
                    CertificateFingerprint = fingerprint,
                    FingerprintAlgorithm = algorithm,
                    AllowUntrustedRoot = allowUntrustedRootOption,
                    Author = action == TrustCommand.Author,
                    Repository = action == TrustCommand.Repository,
                    Owners = CommandLineUtility.SplitAndJoinAcrossMultipleValues(owners.Split(';')),
                    Logger = logger
                };

                SetLogLevelAction(XPlatUtility.MSBuildVerbosityToNuGetLogLevel(verbosity));

#pragma warning disable CS0618 // Type or member is obsolete
                var sourceProvider = new PackageSourceProvider(settings, enablePackageSourcesChangedEvent: false);
#pragma warning restore CS0618 // Type or member is obsolete
                var trustedSignersProvider = new TrustedSignersProvider(settings);

                var runner = new TrustedSignersCommandRunner(trustedSignersProvider, sourceProvider);
                Task<int> trustedSignTask = runner.ExecuteCommandAsync(trustedSignersArgs);
                return await trustedSignTask;
            }
            catch (InvalidOperationException e)
            {
                // nuget trust command handled exceptions.
                if (!string.IsNullOrWhiteSpace(name))
                {
                    var error_TrustedSignerAlreadyExistsMessage = string.Format(CultureInfo.CurrentCulture, Strings.Error_TrustedSignerAlreadyExists, name);

                    if (e.Message == error_TrustedSignerAlreadyExistsMessage)
                    {
                        logger.LogError(error_TrustedSignerAlreadyExistsMessage);
                        return 1;
                    }
                }

                if (!string.IsNullOrWhiteSpace(sourceUrl))
                {
                    var error_TrustedRepoAlreadyExists = string.Format(CultureInfo.CurrentCulture, Strings.Error_TrustedRepoAlreadyExists, sourceUrl);

                    if (e.Message == error_TrustedRepoAlreadyExists)
                    {
                        logger.LogError(error_TrustedRepoAlreadyExists);
                        return 1;
                    }
                }

                throw;
            }
            catch (ArgumentException e)
            {
                if (e.Data is System.Collections.IDictionary)
                {
                    logger.LogError(string.Format(CultureInfo.CurrentCulture, Strings.Error_TrustFingerPrintAlreadyExist));
                    return 1;
                }

                throw;
            }
        }

        private static TrustedSignersAction MapTrustEnumAction(TrustCommand trustCommand)
        {
            switch (trustCommand)
            {
                case TrustCommand.List:
                    return TrustedSignersAction.List;
                case TrustCommand.Remove:
                    return TrustedSignersAction.Remove;
                case TrustCommand.Sync:
                    return TrustedSignersAction.Sync;
                default:
                    return TrustedSignersAction.Add;
            }
        }
    }
}
