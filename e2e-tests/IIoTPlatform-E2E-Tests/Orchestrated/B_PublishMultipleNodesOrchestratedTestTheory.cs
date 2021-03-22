// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace IIoTPlatform_E2E_Tests.Orchestrated
{
    using System;
    using System.Dynamic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Newtonsoft.Json;
    using RestSharp;
    using TestExtensions;
    using Xunit.Abstractions;
    using System.Threading;
    using Newtonsoft.Json.Converters;
    using System.Collections.Generic;

    /// <summary>
    /// The test theory using different (ordered) test cases to go thru all required steps of publishing OPC UA node
    /// </summary>
    [TestCaseOrderer(TestCaseOrderer.FullName, TestConstants.TestAssemblyName)]
    [Collection(IIoTMultipleNodesTestCollection.CollectionName)]
    [Trait(TestConstants.TraitConstants.PublisherModeTraitName, TestConstants.TraitConstants.PublisherModeOrchestratedTraitValue)]
    public class B_PublishMultipleNodesOrchestratedTestTheory {
        private readonly ITestOutputHelper _output;
        private readonly IIoTMultipleNodesTestContext _context;

        public B_PublishMultipleNodesOrchestratedTestTheory(IIoTMultipleNodesTestContext context, ITestOutputHelper output) {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.OutputHelper = _output;
        }

        [Fact, PriorityOrder(1)]
        public void Test_PublishNodeWithDefaults_Expect_DataAvailableAtIoTHub() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            var testPlc = _context.SimulatedPublishedNodes[_context.ConsumedOpcUaNodes.First().Key];
            _context.ConsumedOpcUaNodes.First().Value.OpcNodes = testPlc.OpcNodes.Skip(250).ToArray();
            var body = new {
                NodesToAdd = _context.ConsumedOpcUaNodes.First().Value.OpcNodes.Select(node => new {
                    nodeId = node.Id,
                    samplingInterval = "00:00:00.250",
                    publishingInterval = "00:00:00.500",
                }).ToArray()
            };

            var route = string.Format(TestConstants.APIRoutes.PublisherBulkFormat, _context.OpcUaEndpointId);
            var response = TestHelper.CallRestApi(_context, Method.POST, route, body, expectSuccess: true, ct: cts.Token);
            Assert.NotNull(response);
        }

        [Fact, PriorityOrder(2)]
        public void Test_GetListOfJobs_Expect_JobWithEndpointId() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            var route = TestConstants.APIRoutes.PublisherJobs;
            var response = TestHelper.CallRestApi(_context, Method.GET, route, expectSuccess: true, ct: cts.Token);
            dynamic json = JsonConvert.DeserializeObject<ExpandoObject>(response.Content, new ExpandoObjectConverter());

            bool found = false;
            for (int jobIndex = 0; jobIndex < (int)json.jobs.Count; jobIndex++) {
                var id = (string)json.jobs[jobIndex].id;
                if (id == _context.OpcUaEndpointId) {
                    found = true;
                    break;
                }
            }
            Assert.True(found, "Publishing Job was not created!");
        }

        [Fact, PriorityOrder(3)]
        public void Test_VerifyDataAvailableAtIoTHub() {

            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);

            //use test event processor to verify data send to IoT Hub (expected* set to zero as data gap analysis is not part of this test case)
            TestHelper.StartMonitoringIncomingMessagesAsync(_context, 50, 1000, 90_000_000, cts.Token).GetAwaiter().GetResult();
            // wait some time to generate events to process
            // on VM in the cloud 90 seconds were not sufficient to publish data for 250 slow nodes
            var delay = TestConstants.DefaultTimeoutInMilliseconds * 2;
            Task.Delay(delay, cts.Token).GetAwaiter().GetResult();
            var json = TestHelper.StopMonitoringIncomingMessagesAsync(_context, cts.Token).GetAwaiter().GetResult();
            Assert.True((int)json.totalValueChangesCount > 0, "No messages received at IoT Hub");
            Assert.True((uint)json.droppedValueCount == 0, "Dropped messages detected");
            Assert.True((uint)json.duplicateValueCount == 0, "Duplicate values detected");

            var unexpectedNodesThatPublish = new List<string>();
            // check that every published node is sending data
            if (_context.ConsumedOpcUaNodes != null) {
                var expectedNodes = new List<string>(_context.ConsumedOpcUaNodes.First().Value.OpcNodes.Select(n => n.Id));
                foreach(dynamic property in json.valueChangesByNodeId) {
                    var propertyName = (string)property.Name;
                    var nodeId = propertyName.Split('#').Last();
                    var expected = expectedNodes.FirstOrDefault(n => n.EndsWith(nodeId));
                    if (expected != null) {
                        expectedNodes.Remove(expected);
                    } else {
                        unexpectedNodesThatPublish.Add(propertyName);
                    }
                }

                expectedNodes.ForEach(n => _context.OutputHelper.WriteLine(n));
                Assert.Empty(expectedNodes);

                unexpectedNodesThatPublish.ForEach(node => _context.OutputHelper.WriteLine($"Publishing from unexpected node: {node}"));
            }
        }

        [Fact, PriorityOrder(4)]
        public void Test_BulkUnpublishedNodes_Expect_Success() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            var testPlc = _context.SimulatedPublishedNodes[_context.ConsumedOpcUaNodes.First().Key];
            _context.ConsumedOpcUaNodes.First().Value.OpcNodes = testPlc.OpcNodes.Skip(250).ToArray();
            var body = new {
                NodesToRemove = _context.ConsumedOpcUaNodes.First().Value.OpcNodes.Select(node => node.Id ).ToArray()
            };

            var route = string.Format(TestConstants.APIRoutes.PublisherBulkFormat, _context.OpcUaEndpointId);
            var response = TestHelper.CallRestApi(_context, Method.POST, route, body, expectSuccess: true, ct: cts.Token);
        }

        [Fact, PriorityOrder(5)]
        public void Test_VerifyNoDataIncomingAtIoTHub() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            Task.Delay(TestConstants.DefaultTimeoutInMilliseconds, cts.Token).GetAwaiter().GetResult(); //wait till the publishing has stopped
            //use test event processor to verify data send to IoT Hub (expected* set to zero as data gap analysis is not part of this test case)
            TestHelper.StartMonitoringIncomingMessagesAsync(_context, 0, 0, 0, cts.Token).GetAwaiter().GetResult();
            // wait some time to generate events to process
            Task.Delay(TestConstants.DefaultTimeoutInMilliseconds, cts.Token).GetAwaiter().GetResult();
            var json = TestHelper.StopMonitoringIncomingMessagesAsync(_context, cts.Token).GetAwaiter().GetResult();
            Assert.True((int)json.totalValueChangesCount == 0, "Unexpected Messages received at IoT Hub");
        }


        [Fact, PriorityOrder(6)]
        public void RemoveJob_Expect_Success() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            var route = string.Format(TestConstants.APIRoutes.PublisherJobsFormat, _context.OpcUaEndpointId);
            TestHelper.CallRestApi(_context, Method.DELETE, route, expectSuccess: true, ct: cts.Token);
        }
    }
}
