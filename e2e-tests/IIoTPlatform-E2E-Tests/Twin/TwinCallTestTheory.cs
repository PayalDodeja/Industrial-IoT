// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace IIoTPlatform_E2E_Tests.Twin {
    using IIoTPlatform_E2E_Tests.TestExtensions;
    using Microsoft.CSharp.RuntimeBinder;
    using System.Collections.Generic;
    using Xunit;

    [TestCaseOrderer(TestCaseOrderer.FullName, TestConstants.TestAssemblyName)]
    [Collection("IIoT Twin Test Collection")]
    [Trait(TestConstants.TraitConstants.TwinModeTraitName, TestConstants.TraitConstants.DefaultTraitValue)]
    public class TwinCallTestTheory {
        private readonly TwinTestsFixture _context;

        public TwinCallTestTheory(TwinTestsFixture fixture)  {
            _context = fixture;
        }

        [Fact, PriorityOrder(0)]
        public void Twin_Call_GetMethodMetadata() {
            var methodId = "i=13358"; // CreateFile
            var methodMetadata = _context.Twin_GetMethodMetadata(methodId);

            Assert.Equal(methodMetadata.objectId, "i=13354");
            Assert.Equal(methodMetadata.inputArguments.Count, 2);
            Assert.Equal(methodMetadata.outputArguments.Count, 2);

            Assert.Equal(methodMetadata.inputArguments[0].name, "FileName");
            Assert.Equal(methodMetadata.inputArguments[1].name, "RequestFileOpen");
            Assert.Equal(methodMetadata.inputArguments[0].type.displayName, "String");            
            Assert.Equal(methodMetadata.inputArguments[1].type.displayName, "Boolean");

            Assert.Equal(methodMetadata.outputArguments[0].name, "FileNodeId");
            Assert.Equal(methodMetadata.outputArguments[1].name, "FileHandle");
            Assert.Equal(methodMetadata.outputArguments[0].type.displayName, "NodeId");          
            Assert.Equal(methodMetadata.outputArguments[1].type.displayName, "UInt32");
        }


        [Fact, PriorityOrder(1)]
        public void Twin_Call_CallMethod() {
            // CreateFile method - not implemented
            var methodId = "i=13358"; // CreateFile
            var arguments = new List<object> {
                new {dataType = "String", value = "TestFile"},
                new {dataType = "Boolean", value = "false"}
            };
            var methodMetadata = _context.Twin_GetMethodMetadata(methodId);
            var response = _context.Twin_CallMethod(methodId, methodMetadata.objectId, arguments);
            Assert.Equal("BadNotImplemented", GetErrorMessage(response));

            // ConditionRefresh method - wrong arguments
            methodId = "i=3875";
            arguments = new List<object> {
                new {dataType = "IntegerId", value = "0"}
            };

            methodMetadata = _context.Twin_GetMethodMetadata(methodId);
            response = _context.Twin_CallMethod(methodId, methodMetadata.objectId, arguments);
            Assert.True(GetErrorMessage(response).Contains("Cannot refresh conditions for a subscription that does not exist"));

            // HeaterOn method - no arguments expected
            methodId = "http://microsoft.com/Opc/OpcPlc/Boiler#s=HeaterOn";
            methodMetadata = _context.Twin_GetMethodMetadata(methodId).GetAwaiter().GetResult();
            response = _context.Twin_CallMethod(methodId, methodMetadata.objectId, new List<object>());
            Assert.Null(GetErrorMessage(response));
        }

        [Fact, PriorityOrder(5)]
        public void T1_1_CallMethodTest() {
            var ids = new List<string> {
                "i=14479",
                "i=14482",
                "i=16842",
                "i=16881",
                "i=14493",
                "i=14496",
                "i=16935",
                "i=16960",
                "i=15482",
                "i=14555",
                "i=15052",
                "i=15115",
                "i=17386",
                "i=17389",
                "i=15491",
                "i=12886",
                "i=15215",
                "i=15444",
                "i=15454",
                "i=15461",
                "i=12883",
                "i=3875",
                "i=12912",
                "i=9213",
                "i=2949",
                "i=15907",
                "i=15914",
                "i=17296",
                "i=3875",
                "i=12912",
                "i=9213"
            };

            var errorDict = new Dictionary<string, List<string>>();
            var good = new List<dynamic>();
            var names = new List<string>();

            foreach (var methodId in ids) {
                var methodMetadata = _context.Twin_GetMethodMetadata(methodId);
                //var response = TestHelper.Twin_CallMethodAsync(_context, _context.OpcUaEndpointId, methodId, methodMetadata.objectId, new List<object>()).GetAwaiter().GetResult();
                good.Add(methodMetadata);
                var node = _context.Twin_GetBrowseNode(methodId).GetAwaiter();
                names.Add(node.node.displayName);
            }
        }

        private string GetErrorMessage(dynamic content) {
            try {
                return content.errorInfo.errorMessage.ToString();
            }
            catch (RuntimeBinderException) {
                return null;
            }
        }        
    }
}
