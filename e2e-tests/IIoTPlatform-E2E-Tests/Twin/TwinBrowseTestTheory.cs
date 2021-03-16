// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace IIoTPlatform_E2E_Tests.Twin {
    using IIoTPlatform_E2E_Tests.TestExtensions;
    using System.Threading;
    using Xunit;

    [TestCaseOrderer(TestCaseOrderer.FullName, TestConstants.TestAssemblyName)]
    [Collection("IIoT Twin Test Collection")]
    [Trait(TestConstants.TraitConstants.TwinModeTraitName, TestConstants.TraitConstants.DefaultTraitValue)]
    public class TwinBrowseTestTheory {
        private readonly TwinTestsFixture _context;

        public TwinBrowseTestTheory(TwinTestsFixture fixture) {
            _context = fixture;
        }

        [Fact, PriorityOrder(0)]
        public void Test_Twin_Browse_BrowseAddressSpace() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);

            var nodes = _context.Twin_GetBrowseEndpoint(nodeId: null);

            Assert.NotNull(nodes);
            Assert.NotEmpty(nodes);

            Assert.Contains(nodes, n => string.Equals("i=85", n.NodeId));
            Assert.Contains(nodes, n => string.Equals("i=86", n.NodeId));
            Assert.Contains(nodes, n => string.Equals("i=87", n.NodeId));

            Assert.Equal(3, nodes.Count);
        }

        [Fact, PriorityOrder(1)]
        public void Test_Twin_Browse_BrowseSpecificNode() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);

            var nodes = _context.Twin_GetBrowseEndpoint(nodeId: null);

            Assert.NotNull(nodes);
            Assert.NotEmpty(nodes);

            const string nodeId = "i=85";

            Assert.Contains(nodes, n => string.Equals(nodeId, n.NodeId));

            nodes = _context.Twin_GetBrowseEndpoint(nodeId);

            Assert.NotNull(nodes);
            Assert.NotEmpty(nodes);

            Assert.Contains(nodes, n => string.Equals("i=2253", n.NodeId));
            Assert.Contains(nodes, n => string.Equals("http://microsoft.com/Opc/OpcPlc/Boiler#i=15070", n.NodeId));
            Assert.Contains(nodes, n => string.Equals("http://microsoft.com/Opc/OpcPlc/#s=OpcPlc", n.NodeId));
        }

        [Fact, PriorityOrder(2)]
        public void Test_Twin_Browse_BrowseAllNodesOfTypeObject() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);

            var nodes = _context.Twin_GetBrowseEndpoint_Recursive("Object", null, cts.Token);

            Assert.NotNull(nodes);
            Assert.NotEmpty(nodes);

            Assert.True(nodes.Count > 150);

            Assert.Contains(nodes, n => string.Equals("i=85", n.NodeId));
            Assert.Contains(nodes, n => string.Equals("i=2253", n.NodeId));
            Assert.Contains(nodes, n => string.Equals("http://microsoft.com/Opc/OpcPlc/Boiler#i=15070", n.NodeId));
            Assert.Contains(nodes, n => string.Equals("http://microsoft.com/Opc/OpcPlc/#s=OpcPlc", n.NodeId));
            Assert.Contains(nodes, n => string.Equals("i=86", n.NodeId));
            Assert.Contains(nodes, n => string.Equals("i=87", n.NodeId));
        }

        [Fact, PriorityOrder(3)]
        public void Test_Twin_Browse_BrowseAllNodesOfTypeVariable() {
            var cts = new CancellationTokenSource(TestConstants.MaxTestTimeoutMilliseconds);

            var nodes = _context.Twin_GetBrowseEndpoint_Recursive("Variable", null, cts.Token);

            Assert.NotNull(nodes);
            Assert.NotEmpty(nodes);

            Assert.True(nodes.Count > 2000);

            Assert.Contains(nodes, n => string.Equals("i=2254", n.NodeId));
            Assert.Contains(nodes, n => string.Equals("i=11312", n.NodeId));
            Assert.Contains(nodes, n => string.Equals("http://microsoft.com/Opc/OpcPlc/#s=SlowUInt1", n.NodeId));
        }      
    }
}
