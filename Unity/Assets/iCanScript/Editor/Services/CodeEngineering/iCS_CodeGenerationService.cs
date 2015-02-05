using UnityEngine;
using System.Collections;

namespace CodeEngineering {
    
    public static class iCS_CodeGenerationService {
        // ======================================================================
    	// Start Code generator service.
        // ----------------------------------------------------------------------
    	static iCS_CodeGenerationService() {
            // Create default code generation path.
            FileUtils.CreateCodeGenerationFolder("VisualScripts");
            
            // Install events.
            iCS_EditorObject.OnNodeCreated     += OnNodeCreated;
            iCS_EditorObject.OnWillDestroyNode += OnWillDestroyNode;
            iCS_SystemEvents.OnHierarchyChanged+= OnHierarchyChanged;
            iCS_SystemEvents.OnEditorStarted   += OnEditorStarted;				
    	}
    	public static void Start() {}
        public static void Shutdown() {}
    
        // ----------------------------------------------------------------------
        // Generate code according to created node.
        static void OnNodeCreated(iCS_EditorObject node) {
        }
        // ----------------------------------------------------------------------
        // Generate code according to node destruction.
        static void OnWillDestroyNode(iCS_EditorObject node) {
        }
    
        // ----------------------------------------------------------------------
        static void OnEditorStarted() {
            iCS_CSGenerateBehaviour.UpdateBehaviourCode();        
        }
        
        // ----------------------------------------------------------------------
        // Update Visual Script to Behaviour relationship.
        static void OnHierarchyChanged() {
            var allGameObjects= GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
            if(allGameObjects != null) {
                foreach(var go in allGameObjects) {
                    iCS_MenuUtility.UpdateBehaviourComponent(go);
                }
            }
        }
    }

}
