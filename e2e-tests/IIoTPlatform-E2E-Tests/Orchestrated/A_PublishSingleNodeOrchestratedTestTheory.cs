// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace IIoTPlatform_E2E_Tests.Orchestrated
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Newtonsoft.Json;
    using RestSharp;
    using TestExtensions;
    using Xunit.Abstractions;
    using System.Threading;

    /// <summary>
    /// The test theory using different (ordered) test cases to go thru all required steps of publishing OPC UA node
    /// </summary>
    [TestCaseOrderer(TestCaseOrderer.FullName, TestConstants.TestAssemblyName)]
    [Collection(IIoTMultipleNodesTestCollection.CollectionName)]
    [Trait(TestConstants.TraitConstants.PublisherModeTraitName, TestConstants.TraitConstants.PublisherModeOrchestratedTraitValue)]
    public class A_PublishSingleNodeOrchestratedTestTheory {
        private readonly ITestOutputHelper _output;
        private readonly IIoTMultipleNodesTestContext _context;

        public A_PublishSingleNodeOrchestratedTestTheory(IIoTMultipleNodesTestContext context, ITestOutputHelper output) {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.OutputHelper = _output;
        }
      
        [Fact, PriorityOrder(1)]
        public void Test_PublishNodeWithDefaults_Expect_DataAvailableAtIoTHub() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            var simulatedOpcServer = TestHelper.GetSimulatedPublishedNodesConfigurationAsync(_context, cts.Token).GetAwaiter().GetResult();
            var body = new {
                item = new {
                    nodeId = simulatedOpcServer.Values.First().OpcNodes.First().Id,
                    samplingInterval = "00:00:00.250",
                    publishingInterval = "00:00:00.500",
                }
            };

            var route = string.Format(TestConstants.APIRoutes.PublisherStartFormat, _context.OpcUaEndpointId);
            var response = TestHelper.CallRestApi(_context, Method.POST, route, body, expectSuccess: true, ct: cts.Token);          
            Assert.Equal("{}",response.Content);
        }

        [Fact, PriorityOrder(2)]
        public void Test_GetListOfJobs_Expect_OneJobWithPublishingOneNode() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            var simulatedOpcServer = TestHelper.GetSimulatedPublishedNodesConfigurationAsync(_context, cts.Token).GetAwaiter().GetResult();
            var route = TestConstants.APIRoutes.PublisherJobs;
            var response = TestHelper.CallRestApi(_context, Method.GET, route, expectSuccess: true, ct: cts.Token);
            dynamic json = JsonConvert.DeserializeObject(response.Content);

            var count = (int)json.jobs.Count;
            Assert.NotEqual(0, count);
            Assert.NotNull(json.jobs[0].jobConfiguration);
            Assert.NotNull(json.jobs[0].jobConfiguration.writerGroup);
            Assert.NotNull(json.jobs[0].jobConfiguration.writerGroup.dataSetWriters);
            count = (int)json.jobs[0].jobConfiguration.writerGroup.dataSetWriters.Count;
            Assert.Equal(1, count);
            Assert.NotNull(json.jobs[0].jobConfiguration.writerGroup.dataSetWriters[0].dataSet);
            Assert.NotNull(json.jobs[0].jobConfiguration.writerGroup.dataSetWriters[0].dataSet.dataSetSource);
            Assert.NotNull(json.jobs[0].jobConfiguration.writerGroup.dataSetWriters[0].dataSet.dataSetSource.publishedVariables.publishedData);
            count = (int)json.jobs[0].jobConfiguration.writerGroup.dataSetWriters[0].dataSet.dataSetSource.publishedVariables.publishedData.Count;
            Assert.Equal(1, count);
            Assert.NotEmpty((string)json.jobs[0].jobConfiguration.writerGroup.dataSetWriters[0].dataSet.dataSetSource.publishedVariables.publishedData[0].publishedVariableNodeId);
            var publishedNodeId = (string)json.jobs[0].jobConfiguration.writerGroup.dataSetWriters[0].dataSet.dataSetSource.publishedVariables.publishedData[0].publishedVariableNodeId;
            Assert.Equal(simulatedOpcServer.Values.First().OpcNodes.First().Id, publishedNodeId);
        }

        [Fact, PriorityOrder(3)]
        public void Test_VerifyDataAvailableAtIoTHub() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            // Use test event processor to verify data send to IoT Hub (expected* set to zero as data gap analysis is not part of this test case)
            TestHelper.StartMonitoringIncomingMessagesAsync(_context, 0, 0, 0, cts.Token).GetAwaiter().GetResult();

            // Wait some time to generate events to process
            Task.Delay(TestConstants.DefaultTimeoutInMilliseconds, cts.Token).GetAwaiter().GetResult();
            var json = TestHelper.StopMonitoringIncomingMessagesAsync(_context, cts.Token).GetAwaiter().GetResult();
            Assert.True((int)json.totalValueChangesCount > 0, "No messages received at IoT Hub");
            Assert.True((uint)json.droppedValueCount == 0, "Dropped messages detected");
            Assert.True((uint)json.duplicateValueCount == 0, "Duplicate values detected");
        }

        [Fact, PriorityOrder(4)]
        public void RemoveJob_Expect_Success() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);
            var route = string.Format(TestConstants.APIRoutes.PublisherJobsFormat, _context.OpcUaEndpointId);
            TestHelper.CallRestApi(_context, Method.DELETE, route, expectSuccess: true, ct: cts.Token);
        }

        [Fact, PriorityOrder(5)]
        public void Test_VerifyNoDataIncomingAtIoTHub() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);

            // TODO: Use a more deterministic approach instead of the delay
            Task.Delay(TestConstants.DefaultTimeoutInMilliseconds, cts.Token).GetAwaiter().GetResult(); // Wait until the publishing has stopped
            // Use test event processor to verify data send to IoT Hub (expected* set to zero as data gap analysis is not part of this test case)
            TestHelper.StartMonitoringIncomingMessagesAsync(_context, 0, 0, 0, cts.Token).GetAwaiter().GetResult();

            // Wait some time to generate events to process
            Task.Delay(TestConstants.DefaultTimeoutInMilliseconds, cts.Token).GetAwaiter().GetResult();
            var json = TestHelper.StopMonitoringIncomingMessagesAsync(_context, cts.Token).GetAwaiter().GetResult();
            Assert.True((int)json.totalValueChangesCount == 0, "Messages received at IoT Hub");
        }
    }
}
