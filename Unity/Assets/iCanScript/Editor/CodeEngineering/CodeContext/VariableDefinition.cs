using UnityEngine;
using System;
using System.Text;
using System.Collections;

namespace iCanScript.Editor.CodeEngineering {

    public class VariableDefinition : CodeBase {
        // ===================================================================
        // FIELDS
        // -------------------------------------------------------------------
        public AccessSpecifier  myAccessSpecifier = AccessSpecifier.PRIVATE;
        public ScopeSpecifier   myScopeSpecifier  = ScopeSpecifier.NONSTATIC;
		public Type				myRuntimeType     = null;
        
        // ===================================================================
        // INFORMATION GATHERING FUNCTIONS
        // -------------------------------------------------------------------
        /// Builds a class field description..
        ///
        /// @param port The visual script port representing the variable.
        /// @param codeBlock The code block that contains this code.
        /// @param accessType THe Access property of the field.
        /// @param scopeType The Scope property of the field.
        /// @return The newly created variable defintion.
        ///
        public VariableDefinition(iCS_EditorObject vsObject, CodeBase codeBlock, AccessSpecifier accessType, ScopeSpecifier scopeType)
        : base(vsObject, codeBlock) {
            myAccessSpecifier = accessType;
            myScopeSpecifier  = scopeType;
			myRuntimeType     = vsObject.RuntimeType;
        }
        
        // ===================================================================
        // COMMON INTERFACE FUNCTIONS
        // -------------------------------------------------------------------
		/// Resolves code dependencies.
		public override void ResolveDependencies() {
			// Determine proper variable type.
            var consumerType= GetCommonBaseTypeForProducerPort(VSObject);
            if(consumerType != typeof(void) && !iCS_Types.IsA(consumerType, myRuntimeType)) {
				myRuntimeType= consumerType;
            }			
		}
        // -------------------------------------------------------------------
		/// Returns the runtime type of the variable.
		public override Type GetRuntimeType() {
			return myRuntimeType;
		}
		
        // ===================================================================
        // CODE GENERATION FUNCTIONS
        // -------------------------------------------------------------------
        /// Generate the code for all fields of the class.
        ///
        /// @param indentSize The indentation of the fields.
        /// @param field The field for which to generate code.
        /// @return The generated code for the given field.
        ///
        public override string GenerateCode(int indentSize) {
            if(myRuntimeType == null) {
                var message= "Unable to determine type for variable: "+myVSObject.FullName;
                Context.AddError(message, myVSObject.InstanceId);
                return "";
			}
            var result= new StringBuilder(128);
            string initializer= "";
            // Generate variable from port.
            if(VSObject.IsDataPort) {
                if(VSObject.IsInDataPort && !iCS_Types.IsA<UnityEngine.Object>(myRuntimeType)) {
                    initializer= GetNameFor(VSObject, /*valueInsteadOfSelf=*/true);                    
                }
                else {
                    initializer= "default("+ToTypeName(myRuntimeType)+")";
                }
            }
    		// Generate variable from constructor.
            else if(VSObject.IsConstructor) {
                var nbOfParams= GetNbOfParameters(myVSObject);
                var initValues= new string[nbOfParams];
                myVSObject.ForEachChildPort(
                    p=> {
                        if(p.PortIndex < (int)iCS_PortIndex.ParametersEnd) {
                            initValues[p.PortIndex]= GetNameFor(p.FirstProducerPort, /*valueInsteadOfSelf=*/true);
                        }
                    }
                );
                initializer= GenerateAllocatorFragment(myRuntimeType, initValues);
            }
            else {
                Debug.LogWarning("iCanScript: Unreconized variable type");
                return "";
            }
            string variableName;
            if(CodeBlock is TypeDefinition) {
                if(myAccessSpecifier == AccessSpecifier.PUBLIC) {
                    variableName= CodeBlock.GetPublicFieldName(myVSObject);
                }
                else {
                    variableName= CodeBlock.GetPrivateFieldName(myVSObject);
                }                
            }
            else {
                // FIXME: should be going to common parent.
                variableName= CodeBlock.CodeBlock.GetLocalVariableName(myVSObject);
                initializer= null;
            }
			result.Append(GenerateVariable(indentSize, myAccessSpecifier, myScopeSpecifier, myRuntimeType, variableName, initializer));                    
            return result.ToString();
        }
        
        // -------------------------------------------------------------------
        /// Generate the code for a class field.
        ///
		public string GenerateVariable(int indentSize, AccessSpecifier accessType, ScopeSpecifier scopeType,
									   Type variableType, string variableName, string initializer) {
			string indent= ToIndent(indentSize);
            StringBuilder result= new StringBuilder(indent);
            if(myCodeBlock is TypeDefinition) {
                if(accessType == AccessSpecifier.PUBLIC) {
                    result.Append("[iCS_InOutPort]\n");
                    result.Append(indent);
                }
                result.Append(ToAccessString(accessType));
                result.Append(" ");
                result.Append(ToScopeString(scopeType));
                result.Append(" ");                
            }
			result.Append(ToTypeName(variableType));
			result.Append(" ");
			result.Append(variableName);
			if(!String.IsNullOrEmpty(initializer)) {
				result.Append("= ");
				result.Append(initializer);
			}
			result.Append(";\n");
			return result.ToString();
		}

    }

}
