// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator 1.0.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.IIoT.Opc.Registry.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Gateway registration update request
    /// </summary>
    public partial class GatewayUpdateApiModel
    {
        /// <summary>
        /// Initializes a new instance of the GatewayUpdateApiModel class.
        /// </summary>
        public GatewayUpdateApiModel()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the GatewayUpdateApiModel class.
        /// </summary>
        /// <param name="siteId">Site of the Gateway</param>
        public GatewayUpdateApiModel(string siteId = default(string))
        {
            SiteId = siteId;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets site of the Gateway
        /// </summary>
        [JsonProperty(PropertyName = "siteId")]
        public string SiteId { get; set; }

    }
}