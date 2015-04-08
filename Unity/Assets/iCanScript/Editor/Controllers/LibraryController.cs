using UnityEngine;
using System;
using System.Reflection;
using System.Collections;

namespace iCanScript.Editor {
    
    /// @date 2015-04-08    Code Review
    public class LibraryController {
        // ======================================================================
        // INIT / SHUTDOWN
        // ----------------------------------------------------------------------
        /// Scans the application library and extracts the needed nodes.
    	static LibraryController() {
            iCS_Reflection.ParseAppDomain();
    	}        
        // ----------------------------------------------------------------------
        /// Start the application controller.
    	public static void Start() {}
        // ----------------------------------------------------------------------
        /// Shutdowns the application controller.
        public static void Shutdown() {}
    }
    
}