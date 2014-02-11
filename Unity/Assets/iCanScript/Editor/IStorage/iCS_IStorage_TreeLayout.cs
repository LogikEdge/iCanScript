using UnityEngine;
using System.Collections;

public partial class iCS_IStorage {
	// ======================================================================
    // ----------------------------------------------------------------------
	public void ForcedRelayoutOfTree(iCS_EditorObject root) {
        ForEachRecursiveDepthFirst(root,
            obj=> {
                // Nothing to do if not visible in layout.
            	if(!obj.IsVisibleInLayout) {
            	    return;
        	    }
                // Layout all nodes (ports will also be updated).
				if(obj.IsNode) {
					obj.LayoutNode();
				}
            }
        );    		
	}
	
}
