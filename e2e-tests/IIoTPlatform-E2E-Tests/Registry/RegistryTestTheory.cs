// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace IIoTPlatform_E2E_Tests.Registry {
    using RestSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using TestExtensions;
    using Xunit;
    using Xunit.Abstractions;

    [TestCaseOrderer(TestCaseOrderer.FullName, TestConstants.TestAssemblyName)]
    [Collection(RegistryTestCollection.CollectionName)]
    [Trait(TestConstants.TraitConstants.DiscoveryModeTraitName, TestConstants.TraitConstants.DefaultTraitValue)]
    public class RegistryTestTheory {
        private readonly ITestOutputHelper _output;
        private readonly RegistryTestContext _context;

        public RegistryTestTheory(RegistryTestContext context, ITestOutputHelper output) {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.OutputHelper = _output;
        }

        [Fact, PriorityOrder(1)]
        public void Test_CollectOAuthToken() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            var token = TestHelper.GetTokenAsync(_context, cts.Token).GetAwaiter().GetResult();
            Assert.NotEmpty(token);
        }

        [Fact, PriorityOrder(2)]
        public void Test_RegisterOPCServer_Expect_Success() {
            // We will wait for microservices of IIoT platform to be healthy and modules to be deployed.
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            TestHelper.WaitForServicesAsync(_context, cts.Token).GetAwaiter().GetResult();
            _context.RegistryHelper.WaitForIIoTModulesConnectedAsync(_context.DeviceConfig.DeviceId, cts.Token).GetAwaiter().GetResult();
         
            var simulatedOpcServer = TestHelper.GetSimulatedPublishedNodesConfigurationAsync(_context, cts.Token).GetAwaiter().GetResult();
            Assert.NotNull(simulatedOpcServer);
            Assert.NotEmpty(simulatedOpcServer.Keys);
            Assert.NotEmpty(simulatedOpcServer.Values);
          
            var body = new {
                discoveryUrl = simulatedOpcServer.Values.First().EndpointUrl
            };

            var route = TestConstants.APIRoutes.RegistryApplications;
            var response = TestHelper.CallRestApi(_context, Method.POST, route, body, expectSuccess: true, ct: cts.Token);
            Assert.True(response.IsSuccessful);
        }

        [Fact, PriorityOrder(3)]
        public void Test_GetApplicationsFromRegistry_ExpectOneRegisteredApplication() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            dynamic json = TestHelper.Discovery.WaitForDiscoveryToBeCompletedAsync(_context, cts.Token, new List<string> { _context.TestServer.EndpointUrl }).GetAwaiter().GetResult();

            var numberOfItems = (int)json.items.Count;
            bool found = false;
            for (int indexOfTestPlc = 0; indexOfTestPlc < numberOfItems; indexOfTestPlc++) {

                var endpoint = ((string)json.items[indexOfTestPlc].discoveryUrls[0]).TrimEnd('/');
                if (endpoint == _context.TestServer.EndpointUrl) {
                    found = true;

                    break;
                }
            }
            Assert.True(found, "OPC Application not activated");
        }

        [Fact, PriorityOrder(4)]
        public void Test_GetEndpoints_Expect_OneWithMultipleAuthentication() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            var json = TestHelper.Discovery.WaitForEndpointDiscoveryToBeCompleted(_context, cts.Token, new List<string> { _context.TestServer.EndpointUrl }).GetAwaiter().GetResult();

            var numberOfItems = (int)json.items.Count;
            bool found = false;
            for (int indexOfOpcUaEndpoint = 0; indexOfOpcUaEndpoint < numberOfItems; indexOfOpcUaEndpoint++) {

                var endpoint = ((string)json.items[indexOfOpcUaEndpoint].registration.endpointUrl).TrimEnd('/');
                if (endpoint == _context.TestServer.EndpointUrl) {
                    found = true;

                    //Authentication Checks
                    var id = (string)json.items[indexOfOpcUaEndpoint].registration.id;
                    Assert.NotEmpty(id);
                    var securityMode = (string)json.items[indexOfOpcUaEndpoint].registration.endpoint.securityMode;
                    Assert.Equal("SignAndEncrypt", securityMode);
                    var authenticationModeNone = (string)json.items[indexOfOpcUaEndpoint].registration.authenticationMethods[0].credentialType;
                    Assert.Equal("None", authenticationModeNone);
                    var authenticationModeUserName = (string)json.items[indexOfOpcUaEndpoint].registration.authenticationMethods[1].credentialType;
                    Assert.Equal("UserName", authenticationModeUserName);
                    var authenticationModeCertificate = (string)json.items[indexOfOpcUaEndpoint].registration.authenticationMethods[2].credentialType;
                    Assert.Equal("X509Certificate", authenticationModeCertificate);
                    break;
                }
            }
            Assert.True(found, "OPC UA Endpoint not found");
        }

        [Fact, PriorityOrder(5)]
        public void Test_CheckIfEndpointWasActivated_Expect_ActivatedAndConnected() {

            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            var json = TestHelper.Registry.WaitForEndpointToBeActivatedAsync(_context, cts.Token, new List<string> { _context.TestServer.EndpointUrl }).GetAwaiter().GetResult();

            var numberOfItems = (int)json.items.Count;
            bool found = false;
            for (int indexOfOpcUaEndpoint = 0; indexOfOpcUaEndpoint < numberOfItems; indexOfOpcUaEndpoint++) {

                var endpoint = ((string)json.items[indexOfOpcUaEndpoint].registration.endpointUrl).TrimEnd('/');
                if (endpoint == _context.TestServer.EndpointUrl) {
                    found = true;

                    var endpointState = (string)json.items[indexOfOpcUaEndpoint].endpointState;
                    Assert.Equal("Ready", endpointState);
                    break;
                }
            }
            Assert.True(found, "OPC UA Endpoint not found");
        }
    }
}
