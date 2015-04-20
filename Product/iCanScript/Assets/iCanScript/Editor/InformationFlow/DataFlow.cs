﻿using UnityEngine;
using System.Collections;

namespace iCanScript.Editor {
    // ===================================================================
    /// This class implements data flow utilities.
    public static class DataFlow {

    	// -------------------------------------------------------------------------
        /// Returns the target port.
        ///
        /// @param node The node with the target port.
        ///
        /// @return The _'target'_ port if found. _'null'_ otherwise.
        ///
        public static iCS_EditorObject GetTargetPort(iCS_EditorObject node) {
            iCS_EditorObject result= null;
            node.ForEachChildPort(
                p=> {
                    if(p.PortIndex == (int)iCS_PortIndex.Target) {
                        result= p;
                    }
                }
            );
            return result;
        }

    }    
}
