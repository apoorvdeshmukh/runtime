// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Provides configuration extensions for adding user secrets configuration source.
    /// </summary>
    public static class UserSecretsConfigurationExtensions
    {
        /// <summary>
        /// <para>
        /// Adds the user secrets configuration source. Searches the assembly that contains type <typeparamref name="T"/>
        /// for an instance of <see cref="UserSecretsIdAttribute"/>, which specifies a user secrets ID.
        /// </para>
        /// <para>
        /// A user secrets ID is unique value used to store and identify a collection of secret configuration values.
        /// </para>
        /// </summary>
        /// <param name="configuration">The configuration builder.</param>
        /// <typeparam name="T">The type from the assembly to search for an instance of <see cref="UserSecretsIdAttribute"/>.</typeparam>
        /// <exception cref="InvalidOperationException">The assembly containing <typeparamref name="T"/> does not have <see cref="UserSecretsIdAttribute"/>.</exception>
        /// <returns>The configuration builder.</returns>
        public static IConfigurationBuilder AddUserSecrets<T>(this IConfigurationBuilder configuration)
            where T : class
            => configuration.AddUserSecrets(typeof(T).Assembly, optional: true, reloadOnChange: false);

        /// <summary>
        /// <para>
        /// Adds the user secrets configuration source. Searches the assembly that contains type <typeparamref name="T"/>
        /// for an instance of <see cref="UserSecretsIdAttribute"/>, which specifies a user secrets ID.
        /// </para>
        /// <para>
        /// A user secrets ID is unique value used to store and identify a collection of secret configuration values.
        /// </para>
        /// </summary>
        /// <param name="configuration">The configuration builder.</param>
        /// <param name="optional">Whether loading secrets is optional. When false, this method may throw.</param>
        /// <exception cref="InvalidOperationException"><paramref name="optional"/> is false and the assembly containing <typeparamref name="T"/> does not have a valid <see cref="UserSecretsIdAttribute"/>.</exception>
        /// <typeparam name="T">The type from the assembly to search for an instance of <see cref="UserSecretsIdAttribute"/>.</typeparam>
        /// <returns>The configuration builder.</returns>
        public static IConfigurationBuilder AddUserSecrets<T>(this IConfigurationBuilder configuration, bool optional)
            where T : class
            => configuration.AddUserSecrets(typeof(T).Assembly, optional, reloadOnChange: false);

        /// <summary>
        /// <para>
        /// Adds the user secrets configuration source. Searches the assembly that contains type <typeparamref name="T"/>
        /// for an instance of <see cref="UserSecretsIdAttribute"/>, which specifies a user secrets ID.
        /// </para>
        /// <para>
        /// A user secrets ID is unique value used to store and identify a collection of secret configuration values.
        /// </para>
        /// </summary>
        /// <param name="configuration">The configuration builder.</param>
        /// <param name="optional">Whether loading secrets is optional. When false, this method may throw.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <exception cref="InvalidOperationException"><paramref name="optional"/> is <see langword="false"/> and the assembly containing <typeparamref name="T"/> does not have a valid <see cref="UserSecretsIdAttribute"/>.</exception>
        /// <typeparam name="T">The type from the assembly to search for an instance of <see cref="UserSecretsIdAttribute"/>.</typeparam>
        /// <returns>The configuration builder.</returns>
        public static IConfigurationBuilder AddUserSecrets<T>(this IConfigurationBuilder configuration, bool optional, bool reloadOnChange)
            where T : class
            => configuration.AddUserSecrets(typeof(T).Assembly, optional, reloadOnChange);

        /// <summary>
        /// <para>
        /// Adds the user secrets configuration source. This searches <paramref name="assembly"/> for an instance
        /// of <see cref="UserSecretsIdAttribute"/>, which specifies a user secrets ID.
        /// </para>
        /// <para>
        /// A user secrets ID is unique value used to store and identify a collection of secret configuration values.
        /// </para>
        /// </summary>
        /// <param name="configuration">The configuration builder.</param>
        /// <param name="assembly">The assembly with the <see cref="UserSecretsIdAttribute" />.</param>
        /// <exception cref="InvalidOperationException"><paramref name="assembly"/> does not have a valid <see cref="UserSecretsIdAttribute"/>.</exception>
        /// <returns>The configuration builder.</returns>
        public static IConfigurationBuilder AddUserSecrets(this IConfigurationBuilder configuration, Assembly assembly)
            => configuration.AddUserSecrets(assembly, optional: true, reloadOnChange: false);

        /// <summary>
        /// <para>
        /// Adds the user secrets configuration source. This searches <paramref name="assembly"/> for an instance
        /// of <see cref="UserSecretsIdAttribute"/>, which specifies a user secrets ID.
        /// </para>
        /// <para>
        /// A user secrets ID is unique value used to store and identify a collection of secret configuration values.
        /// </para>
        /// </summary>
        /// <param name="configuration">The configuration builder.</param>
        /// <param name="assembly">The assembly with the <see cref="UserSecretsIdAttribute" />.</param>
        /// <param name="optional">Whether loading secrets is optional. When false, this method may throw.</param>
        /// <exception cref="InvalidOperationException"><paramref name="optional"/> is false and <paramref name="assembly"/> does not have a valid <see cref="UserSecretsIdAttribute"/>.</exception>
        /// <returns>The configuration builder.</returns>
        public static IConfigurationBuilder AddUserSecrets(this IConfigurationBuilder configuration, Assembly assembly, bool optional)
            => configuration.AddUserSecrets(assembly, optional, reloadOnChange: false);

        /// <summary>
        /// <para>
        /// Adds the user secrets configuration source. This searches <paramref name="assembly"/> for an instance
        /// of <see cref="UserSecretsIdAttribute"/>, which specifies a user secrets ID.
        /// </para>
        /// <para>
        /// A user secrets ID is unique value used to store and identify a collection of secret configuration values.
        /// </para>
        /// </summary>
        /// <param name="configuration">The configuration builder.</param>
        /// <param name="assembly">The assembly with the <see cref="UserSecretsIdAttribute" />.</param>
        /// <param name="optional">Whether loading secrets is optional. When false, this method may throw.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <exception cref="InvalidOperationException"><paramref name="optional"/> is false and <paramref name="assembly"/> does not have a valid <see cref="UserSecretsIdAttribute"/>.</exception>
        /// <returns>The configuration builder.</returns>
        public static IConfigurationBuilder AddUserSecrets(this IConfigurationBuilder configuration, Assembly assembly, bool optional, bool reloadOnChange)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(assembly);

            UserSecretsIdAttribute? attribute = assembly.GetCustomAttribute<UserSecretsIdAttribute>();
            if (attribute != null)
            {
                return AddUserSecretsInternal(configuration, attribute.UserSecretsId, optional, reloadOnChange);
            }

            if (!optional)
            {
                throw new InvalidOperationException(SR.Format(SR.Error_Missing_UserSecretsIdAttribute, assembly.GetName().Name));
            }

            return configuration;
        }

        /// <summary>
        /// <para>
        /// Adds the user secrets configuration source with specified user secrets ID.
        /// </para>
        /// <para>
        /// A user secrets ID is unique value used to store and identify a collection of secret configuration values.
        /// </para>
        /// </summary>
        /// <param name="configuration">The configuration builder.</param>
        /// <param name="userSecretsId">The user secrets ID.</param>
        /// <returns>The configuration builder.</returns>
        public static IConfigurationBuilder AddUserSecrets(this IConfigurationBuilder configuration, string userSecretsId)
            => configuration.AddUserSecrets(userSecretsId, reloadOnChange: false);

        /// <summary>
        /// <para>
        /// Adds the user secrets configuration source with specified user secrets ID.
        /// </para>
        /// <para>
        /// A user secrets ID is unique value used to store and identify a collection of secret configuration values.
        /// </para>
        /// </summary>
        /// <param name="configuration">The configuration builder.</param>
        /// <param name="userSecretsId">The user secrets ID.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <returns>The configuration builder.</returns>
        public static IConfigurationBuilder AddUserSecrets(this IConfigurationBuilder configuration, string userSecretsId, bool reloadOnChange)
            => AddUserSecretsInternal(configuration, userSecretsId, true, reloadOnChange);

        private static IConfigurationBuilder AddUserSecretsInternal(IConfigurationBuilder configuration, string userSecretsId, bool optional, bool reloadOnChange)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(userSecretsId);

            return AddSecretsFile(configuration, PathHelper.InternalGetSecretsPathFromSecretsId(userSecretsId, throwIfNoRoot: !optional), optional, reloadOnChange);
        }

        private static IConfigurationBuilder AddSecretsFile(IConfigurationBuilder configuration, string secretPath, bool optional, bool reloadOnChange)
        {
            if (string.IsNullOrEmpty(secretPath))
            {
                return configuration;
            }

            string? directoryPath = Path.GetDirectoryName(secretPath);
            PhysicalFileProvider? fileProvider = Directory.Exists(directoryPath)
                ? new PhysicalFileProvider(directoryPath)
                : null;
            return configuration.AddJsonFile(fileProvider, PathHelper.SecretsFileName, optional, reloadOnChange);
        }
    }
}
