using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace iCanScript.Editor.CodeEngineering {

    public class GlobalDefinition : CodeBase {
        // ===================================================================
        // FIELDS
        // -------------------------------------------------------------------
        string                myNamespace= null;
        List<TypeDefinition>  myTypes    = new List<TypeDefinition>();
        Type                  myBaseType = null;
        
        // ===================================================================
        // PROPERTIES
        // -------------------------------------------------------------------
        /// Namespace in which to generate the code.
        public string Namespace {
            get { return myNamespace; }
            set { myNamespace= value; }
        }

        // ===================================================================
        // INFORMATION GATHERING FUNCTIONS
        // -------------------------------------------------------------------
        /// Builds the code global scope.
        public GlobalDefinition(iCS_EditorObject vsRootObject, string namespaceName)
        : base(vsRootObject, null) {
            // -- Initialise attributes --
			myNamespace= namespaceName;
            myBaseType = CodeGenerationConfig.BaseType;
            
			// -- Collect the used namespaces --
            if(myBaseType != null && myBaseType != typeof(void)) {
                AddNamespace(myBaseType.Namespace);
            }
			CollectUsedNamespaces();
			
			// -- Build root types --
			BuildRootTypes(vsRootObject);
			
			// -- Resolve dependencies --
			ResolveDependencies();
        }

        // -------------------------------------------------------------------
		/// Collect the used assemblies
		void CollectUsedNamespaces() {
			VSObject.ForEachChildRecursiveDepthFirst(
				c=> {
					if(c.IsKindOfFunction) {
						var runtimeType= c.RuntimeType;
						if(runtimeType != null) {
							AddNamespace(runtimeType.Namespace);
						}
					}
				}
			);
		}
		
        // -------------------------------------------------------------------
		/// Builds the root class defintions
		///
		/// @param iStorage The VS storage.
		///
		void BuildRootTypes(iCS_EditorObject vsRootObject) {
            // Add root class defintion.
            var baseType= CodeGenerationConfig.BaseType;
            var classDefinition= new TypeDefinition(vsRootObject, this,
                                                    baseType,
                                                    AccessSpecifier.PUBLIC,
                                                    ScopeSpecifier.NONSTATIC);
            AddType(classDefinition);			
		}
		
        // -------------------------------------------------------------------
		/// Resolves the code dependencies.
		public override void ResolveDependencies() {
			foreach(var rt in myTypes) {
				rt.ResolveDependencies();
			}			
		}
		
        // ===================================================================
        // COMMON INTERFACE FUNCTIONS
        // -------------------------------------------------------------------
        /// Adds a class definition to the global scope
        ///
        /// @param typeDefinition Type (class or struct) definition to add.
        ///
        public override void AddType(TypeDefinition typeDefinition) {
            myTypes.Add(typeDefinition);
            typeDefinition.Parent= this;
        }
        
        // -------------------------------------------------------------------
        /// Adds a using directive to the file scope.
        ///
        /// @param usingDirective A string with a using directive.
        ///
        public void AddNamespace(string namespaceName) {
            if(string.IsNullOrEmpty(namespaceName)) return;
            if(Namespace != namespaceName) {
                Context.AddNamespace(namespaceName);
            }
        }

        // ===================================================================
        // CODE GENERATION FUNCTIONS
        // -------------------------------------------------------------------
        /// Generate the global scope header code.
        ///
        /// @param indentSize The indentation needed for the class definition.
        /// @return The formatted header code for the global scope.
        ///
        public override string GenerateHeader(int indentSize) {
            var result= new StringBuilder(2048);
            // Generate using directives.
            var usedNamespaces= Context.UsedNamespaces;
            usedNamespaces.Sort((s1,s2)=> s1.Length - s2.Length);
            foreach(var u in usedNamespaces) {
                result.Append("using ");
                result.Append(u);
                result.Append(";\n");
            }
            // Generate the namespace header.
            if(!string.IsNullOrEmpty(myNamespace)) {
                result.Append("\nnamespace ");
                result.Append(myNamespace);
                result.Append(" {\n");
            }
            return result.ToString();
        }

        // -------------------------------------------------------------------
        /// Generate the global scope body code.
        ///
        /// @param indentSize The indentation needed for the class definition.
        /// @return The formatted body code for the global scope.
        ///
        public override string GenerateBody(int indentSize) {
            // Generate each internal type.
            var result= new StringBuilder(1024);
            foreach(var typeDef in myTypes) {
                result.Append("\n");
                result.Append(typeDef.GenerateCode(indentSize));
            }
            return result.ToString();
        }

        // -------------------------------------------------------------------
        /// Generate the global scope trailer code.
        ///
        /// @param indentSize The indentation needed for the class definition.
        /// @return The formatted trailer code for the global scope.
        ///
        public override string GenerateTrailer(int indentSize) {
            // Generate the namespace trailer.
            if(!string.IsNullOrEmpty(myNamespace)) {
                return "}\n";
            }
            return "";
        }

        // ===================================================================
        // UTILITY
        // -------------------------------------------------------------------
        int NumberOfNamespacesWithTypeName(string typeName) {
            int cnt= 0;
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach(var t in assembly.GetTypes()) {
                    if(t.Name == typeName) {
                        foreach(var ns in Context.UsedNamespaces) {
                            if(t.Namespace == ns) {
                                ++cnt;
                            }
                        }
                    }
                }
            }
            return cnt;
        }
    }
}