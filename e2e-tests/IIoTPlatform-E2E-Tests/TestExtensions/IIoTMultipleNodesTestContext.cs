// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace IIoTPlatform_E2E_Tests.TestExtensions {
    using IIoTPlatform_E2E_Tests.TestModels;
    using RestSharp;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// Context to pass between test, for test that handle multiple OPC UA nodes
    /// </summary>
    public class IIoTMultipleNodesTestContext : IIoTPlatformTestContext  {
        public IIoTMultipleNodesTestContext() {
            ConsumedOpcUaNodes = new Dictionary<string, PublishedNodesEntryModel>();
            TestHelper.SwitchToOrchestratedModeAsync(this).GetAwaiter().GetResult();

            // Prepare the environment for the publisher tests
            PrepareEnvironment();
        }

        /// <summary>
        /// Save to simulated OPC UA Nodes, key is the discovery url of opc server
        /// </summary>
        public IReadOnlyDictionary<string, PublishedNodesEntryModel> SimulatedPublishedNodes { get; private set; }

        /// <summary>
        /// Dictionary that can be used to save the nodes, that are currently published
        /// </summary>
        public IDictionary<string, PublishedNodesEntryModel> ConsumedOpcUaNodes { get; }

        /// <summary>
        /// Uses the Testhelper to load the simulated OPC UA Nodes and transform them
        /// </summary>
        /// <returns></returns>
        public async Task LoadSimulatedPublishedNodes(CancellationToken token) {
            var simulatedPlcs = await TestHelper.GetSimulatedPublishedNodesConfigurationAsync(this, token);

            SimulatedPublishedNodes = new ReadOnlyDictionary<string, PublishedNodesEntryModel>(
                simulatedPlcs.ToDictionary(kvp => kvp.Value.EndpointUrl, kvp => kvp.Value));
        }

        /// <summary>
        /// Create a Copy except the OpcNodes array
        /// </summary>
        /// <param name="testPlc">Source object</param>
        /// <returns>Copy</returns>
        public PublishedNodesEntryModel GetEntryModelWithoutNodes(PublishedNodesEntryModel testPlc) {
            return new PublishedNodesEntryModel {
                EncryptedAuthPassword = testPlc.EncryptedAuthPassword,
                EncryptedAuthUsername = testPlc.EncryptedAuthUsername,
                EndpointUrl = testPlc.EndpointUrl,
                OpcAuthenticationPassword = testPlc.OpcAuthenticationPassword,
                OpcAuthenticationUsername = testPlc.OpcAuthenticationUsername,
                UseSecurity = testPlc.UseSecurity,
                OpcNodes = null
            };
        }

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

        private void PrepareEnvironment() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);

            // We will wait for microservices of IIoT platform to be healthy and modules to be deployed.
            TestHelper.WaitForServicesAsync(this, cts.Token).GetAwaiter().GetResult();
            RegistryHelper.WaitForIIoTModulesConnectedAsync(DeviceConfig.DeviceId, cts.Token).GetAwaiter().GetResult();
            LoadSimulatedPublishedNodes(cts.Token).GetAwaiter().GetResult();

            // use the second OPC PLC for testing
            var testPlc = SimulatedPublishedNodes.Values.Skip(1).First();
            ConsumedOpcUaNodes[testPlc.EndpointUrl] = GetEntryModelWithoutNodes(testPlc);
            var body = new {
                discoveryUrl = testPlc.EndpointUrl
            };
            var route = TestConstants.APIRoutes.RegistryApplications;
            TestHelper.CallRestApi(this, Method.POST, route, body, expectSuccess: true, ct: cts.Token);

            // check that Application was registered
            cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            dynamic json = TestHelper.Discovery.WaitForDiscoveryToBeCompletedAsync(this, cts.Token, new List<string> { testPlc.EndpointUrl }).GetAwaiter().GetResult();
            bool found = false;
            for (int indexOfTestPlc = 0; indexOfTestPlc < (int)json.items.Count; indexOfTestPlc++) {

                var endpoint = ((string)json.items[indexOfTestPlc].discoveryUrls[0]).TrimEnd('/');
                if (endpoint == testPlc.EndpointUrl) {
                    found = true;
                    break;
                }
            }
            Assert.True(found, "OPC Application not activated");

            // Read OPC UA Endpoint ID
            OpcUaEndpointId = TestHelper.Discovery.GetOpcUaEndpointIdAsync(this, testPlc.EndpointUrl, cts.Token).GetAwaiter().GetResult();

            // Activate OPC UA Endpoint and wait until it's activated
            TestHelper.Registry.ActivateEndpointAsync(this, OpcUaEndpointId, cts.Token).GetAwaiter().GetResult();
        }
    }
}
