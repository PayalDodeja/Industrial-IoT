# encoding: utf-8
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
#
# Code generated by Microsoft (R) AutoRest Code Generator 1.0.0.0
# Changes may cause incorrect behavior and will be lost if the code is
# regenerated.

module azure.iiot.opc.history
  module Models
    #
    # Filter operand
    #
    class FilterOperandApiModel
      # @return [Integer] Element reference in the outer list if
      # operand is an element operand
      attr_accessor :index

      # @return Variant value if operand is a literal
      attr_accessor :value

      # @return [String] Type definition node id if operand is
      # simple or full attribute operand.
      attr_accessor :node_id

      # @return [Array<String>] Browse path of attribute operand
      attr_accessor :browse_path

      # @return [NodeAttribute] Possible values include: 'NodeClass',
      # 'BrowseName', 'DisplayName', 'Description', 'WriteMask',
      # 'UserWriteMask', 'IsAbstract', 'Symmetric', 'InverseName',
      # 'ContainsNoLoops', 'EventNotifier', 'Value', 'DataType', 'ValueRank',
      # 'ArrayDimensions', 'AccessLevel', 'UserAccessLevel',
      # 'MinimumSamplingInterval', 'Historizing', 'Executable',
      # 'UserExecutable', 'DataTypeDefinition', 'RolePermissions',
      # 'UserRolePermissions', 'AccessRestrictions'
      attr_accessor :attribute_id

      # @return [String] Index range of attribute operand
      attr_accessor :index_range

      # @return [String] Optional alias to refer to it makeing it a
      # full blown attribute operand
      attr_accessor :alias_property


      #
      # Mapper for FilterOperandApiModel class as Ruby Hash.
      # This will be used for serialization/deserialization.
      #
      def self.mapper()
        {
          client_side_validation: true,
          required: false,
          serialized_name: 'FilterOperandApiModel',
          type: {
            name: 'Composite',
            class_name: 'FilterOperandApiModel',
            model_properties: {
              index: {
                client_side_validation: true,
                required: false,
                serialized_name: 'index',
                type: {
                  name: 'Number'
                }
              },
              value: {
                client_side_validation: true,
                required: false,
                serialized_name: 'value',
                type: {
                  name: 'Object'
                }
              },
              node_id: {
                client_side_validation: true,
                required: false,
                serialized_name: 'nodeId',
                type: {
                  name: 'String'
                }
              },
              browse_path: {
                client_side_validation: true,
                required: false,
                serialized_name: 'browsePath',
                type: {
                  name: 'Sequence',
                  element: {
                      client_side_validation: true,
                      required: false,
                      serialized_name: 'StringElementType',
                      type: {
                        name: 'String'
                      }
                  }
                }
              },
              attribute_id: {
                client_side_validation: true,
                required: false,
                serialized_name: 'attributeId',
                type: {
                  name: 'Enum',
                  module: 'NodeAttribute'
                }
              },
              index_range: {
                client_side_validation: true,
                required: false,
                serialized_name: 'indexRange',
                type: {
                  name: 'String'
                }
              },
              alias_property: {
                client_side_validation: true,
                required: false,
                serialized_name: 'alias',
                type: {
                  name: 'String'
                }
              }
            }
          }
        }
      end
    end
  end
end