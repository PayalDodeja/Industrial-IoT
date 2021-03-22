// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace IIoTPlatform_E2E_Tests.Registry {
    using Xunit;

    [CollectionDefinition(CollectionName, DisableParallelization = true)]
    public class RegistryTestCollection : ICollectionFixture<RegistryTestContext> {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.

        public const string CollectionName = "IIoT Registry Test Collection";
    }
}