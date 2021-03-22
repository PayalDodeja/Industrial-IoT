// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace IIoTPlatform_E2E_Tests.Registry {
    using IIoTPlatform_E2E_Tests.TestExtensions;
    using RestSharp;
    using System.Linq;
    using System.Threading;

    public class RegistryTestContext : IIoTPlatformTestContext {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryTestContext"/> class.
        /// Used for preparation executed once before any tests of the collection are started.
        /// </summary>
        public RegistryTestContext() : base() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            var simulatedOpcServer = TestHelper.GetSimulatedPublishedNodesConfigurationAsync(this, cts.Token).GetAwaiter().GetResult();
            TestServer = simulatedOpcServer.Values.First();
        }

        /// <summary>
        /// Gets or sets the test servers
        /// </summary>
        public dynamic TestServer;

        /// <summary>
        /// Disposes resources.
        /// Used for cleanup executed once after all tests of the collection were executed.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (!disposing) {
                return;
            }

            // OutputHelper cannot be used outside of test calls, we get rid of it before a helper method would use it
            OutputHelper = null;

            // Remove all applications
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            var route = TestConstants.APIRoutes.RegistryApplications;
            TestHelper.CallRestApi(this, Method.DELETE, route, expectSuccess: true, ct: cts.Token);

            base.Dispose(true);
        }
    }
}
