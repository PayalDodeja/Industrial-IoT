// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace IIoTPlatform_E2E_Tests.TestExtensions {
    using IIoTPlatform_E2E_Tests.Deploy;
    using IIoTPlatform_E2E_Tests.TestModels;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Test context to pass data between test cases for standalone tests.
    /// </summary>
    public class IIoTStandaloneTestContext : IIoTPlatformTestContext {

        /// <summary>
        /// Deployment for edgeHub and edgeAgent so called "base deployment"
        /// </summary>
        public readonly IIoTHubEdgeDeployment IoTHubEdgeBaseDeployment;

        /// <summary>
        /// Deployment for OPC Publisher as standalone
        /// </summary>
        public readonly IIoTHubEdgeDeployment IoTHubPublisherDeployment;

        /// <summary>
        /// Constructor of test context.
        /// </summary>
        public IIoTStandaloneTestContext() {
            // Create deployments.
            IoTHubEdgeBaseDeployment = new IoTHubEdgeBaseDeployment(this);
            IoTHubPublisherDeployment = new IoTHubPublisherDeployment(this);
            ConsumedOpcUaNodes = new Dictionary<string, PublishedNodesEntryModel>();
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
        /// Reset the consumed nodes
        /// </summary>
        public void Reset() {
            ConsumedOpcUaNodes?.Clear();
        }
    }
}
