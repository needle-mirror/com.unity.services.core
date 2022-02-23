using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Internal
{
    /// <summary>
    /// Helper object to initialize all <see cref="IInitializablePackage"/> registered in a <see cref="CoreRegistry"/>.
    /// </summary>
    class CoreRegistryInitializer
    {
        [NotNull]
        readonly CoreRegistry m_Registry;

        [NotNull]
        readonly List<int> m_SortedPackageTypeHashes;

        public CoreRegistryInitializer([NotNull] CoreRegistry registry, [NotNull] List<int> sortedPackageTypeHashes)
        {
            m_Registry = registry;
            m_SortedPackageTypeHashes = sortedPackageTypeHashes;
        }

        public async Task InitializeRegistryAsync()
        {
            if (m_SortedPackageTypeHashes.Count <= 0)
            {
                Succeed();
                return;
            }

            var failureReasons = new List<Exception>(m_SortedPackageTypeHashes.Count);
            for (var i = 0; i < m_SortedPackageTypeHashes.Count; i++)
            {
                try
                {
                    await InitializePackageAtIndexAsync(i);
                }
                catch (Exception e)
                {
                    failureReasons.Add(e);
                }
            }

            if (failureReasons.Count > 0)
            {
                Fail();
            }
            else
            {
                Succeed();
            }

            void Succeed()
            {
                m_Registry.PackageRegistry.Tree = null;
                failureReasons = null;
            }

            async Task InitializePackageAtIndexAsync(int index)
            {
                var packageTypeHash = m_SortedPackageTypeHashes[index];
                var package = m_Registry.PackageRegistry.Tree.PackageTypeHashToInstance[packageTypeHash];
                await package.Initialize(m_Registry);
            }

            void Fail()
            {
                const string errorMessage = "Some services couldn't be initialized."
                    + " Look at inner exceptions to get more information.";
                var innerException = new AggregateException(failureReasons);
                throw new ServicesInitializationException(errorMessage, innerException);
            }
        }
    }
}
