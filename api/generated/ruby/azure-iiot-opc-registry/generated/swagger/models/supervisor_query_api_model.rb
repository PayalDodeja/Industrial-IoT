# encoding: utf-8
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
#
# Code generated by Microsoft (R) AutoRest Code Generator 1.0.0.0
# Changes may cause incorrect behavior and will be lost if the code is
# regenerated.

module azure.iiot.opc.registry
  module Models
    #
    # Supervisor registration query
    #
    class SupervisorQueryApiModel
      # @return [String] Site of the supervisor
      attr_accessor :site_id

      # @return [DiscoveryMode] Discovery mode of supervisor. Possible values
      # include: 'Off', 'Local', 'Network', 'Fast', 'Scan'
      attr_accessor :discovery

      # @return [Boolean] Included connected or disconnected
      attr_accessor :connected


      #
      # Mapper for SupervisorQueryApiModel class as Ruby Hash.
      # This will be used for serialization/deserialization.
      #
      def self.mapper()
        {
          client_side_validation: true,
          required: false,
          serialized_name: 'SupervisorQueryApiModel',
          type: {
            name: 'Composite',
            class_name: 'SupervisorQueryApiModel',
            model_properties: {
              site_id: {
                client_side_validation: true,
                required: false,
                serialized_name: 'siteId',
                type: {
                  name: 'String'
                }
              },
              discovery: {
                client_side_validation: true,
                required: false,
                serialized_name: 'discovery',
                type: {
                  name: 'Enum',
                  module: 'DiscoveryMode'
                }
              },
              connected: {
                client_side_validation: true,
                required: false,
                serialized_name: 'connected',
                type: {
                  name: 'Boolean'
                }
              }
            }
          }
        }
      end
    end
  end
end