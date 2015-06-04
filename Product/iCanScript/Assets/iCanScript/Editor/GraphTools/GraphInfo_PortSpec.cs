﻿using UnityEngine;
using System;
using System.Collections;
using iCanScript.Internal.Engine;

namespace iCanScript.Internal.Editor {

    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    /// This class provides information about the iCanScript graph.
    public static partial class GraphInfo {

        // ===================================================================
		// VARIABLE TYPE ENUMS
		public enum TypeVariables {
            PublicVariable=        PortSpecification.PublicVariable,
            PrivateVariable=       PortSpecification.PrivateVariable,
            StaticPublicVariable=  PortSpecification.StaticPublicVariable,
            StaticPrivateVariable= PortSpecification.StaticPrivateVariable
		};
		public enum ParameterVariable {
			Parameter= PortSpecification.Parameter
		};
        public enum InVariableType {
            PublicVariable=        PortSpecification.PublicVariable,
            PrivateVariable=       PortSpecification.PrivateVariable,
            StaticPublicVariable=  PortSpecification.StaticPublicVariable,
            StaticPrivateVariable= PortSpecification.StaticPrivateVariable,
            Constant=              PortSpecification.Constant
        };
        public enum InUnityObjectVariableType {
            PublicVariable=        PortSpecification.PublicVariable,
            PrivateVariable=       PortSpecification.PrivateVariable,
            StaticPublicVariable=  PortSpecification.StaticPublicVariable,
            StaticPrivateVariable= PortSpecification.StaticPrivateVariable
        };
        public enum InOwnerAndUnityObjectVariableType {
			Owner=				   PortSpecification.Owner,
            PublicVariable=        PortSpecification.PublicVariable,
            PrivateVariable=       PortSpecification.PrivateVariable,
            StaticPublicVariable=  PortSpecification.StaticPublicVariable,
            StaticPrivateVariable= PortSpecification.StaticPrivateVariable
        };
        public enum OutVariableType {
            LocalVariable=         PortSpecification.LocalVariable,
            PublicVariable=        PortSpecification.PublicVariable,
            PrivateVariable=       PortSpecification.PrivateVariable,
            StaticPublicVariable=  PortSpecification.StaticPublicVariable,
            StaticPrivateVariable= PortSpecification.StaticPrivateVariable            
        };
		
        // ===================================================================
		/// Returns the allowed types of variable the given port can support.
		///
		/// @param port The port used to filter the types of variable.
		/// @return The filtered port specification.
		///
		public static Enum GetAllowedPortSpecification(iCS_EditorObject port) {
			if(MustBeATypeVariable(port)) {
				return TypeVariables.PublicVariable;
			}
			if(MustBeAParameter(port)) {
				return ParameterVariable.Parameter;
			}
            if(port.IsInDataPort) {
				var runtimeType= port.RuntimeType;
                if(iCS_Types.IsA<UnityEngine.Object>(runtimeType)) {
					if(runtimeType == typeof(GameObject) ||
					   runtimeType == typeof(Transform)) {
	                  	return InOwnerAndUnityObjectVariableType.PublicVariable;
					   	
					}
					else {
	                    return InUnityObjectVariableType.PublicVariable;						
					}
                }
                else {
                    return InVariableType.PublicVariable;
                }
            }
            else if(port.IsOutDataPort) {
                return OutVariableType.PublicVariable;
            }
			
			return TypeVariables.PublicVariable;
		}
		
	}

}

