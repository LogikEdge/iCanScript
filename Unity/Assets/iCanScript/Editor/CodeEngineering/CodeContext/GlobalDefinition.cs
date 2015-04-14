using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace iCanScript.Editor.CodeEngineering {

    public class GlobalDefinition : CodeBase {
        // ===================================================================
        // FIELDS
        // -------------------------------------------------------------------
        string                myNamespace        = null;
        List<string>          myUsingDirectives  = new List<string>();
        List<TypeDefinition>  myTypes            = new List<TypeDefinition>();
        
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
            // Initialise attributes
			myNamespace= namespaceName;

			// Collect the used assemblies.
			CollectUsedNamespaces();
			
			// Build root types
			BuildRootTypes(vsRootObject);
			
			// Resolve dependencies.
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
							AddUsingDirective(runtimeType.Namespace);
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
            var classDefinition= new TypeDefinition(vsRootObject, this,
                                                    typeof(MonoBehaviour),
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
            typeDefinition.CodeBlock= this;
        }
        
        // -------------------------------------------------------------------
        /// Adds a using directive to the file scope.
        ///
        /// @param usingDirective A string with a using directive.
        ///
        public void AddUsingDirective(string usingDirective) {
            if(string.IsNullOrEmpty(usingDirective)) return;
            if(!myUsingDirectives.Exists((s1)=> s1 == usingDirective) &&
               Namespace != usingDirective) {
                myUsingDirectives.Add(usingDirective);
				myUsingDirectives.Sort((s1,s2)=> s1.Length - s2.Length);
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
            foreach(var u in myUsingDirectives) {
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
        
    }
}