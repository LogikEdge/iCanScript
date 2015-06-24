﻿using UnityEngine;
using System.Collections;

namespace iCanScript.Internal.Editor {
    
    // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    /// This class creates an active project singleton.
    ///
    /// On stratup, it will attempt to recover the last active project.  If no
    /// active project can be found, the user will be asked to create or select
    /// an existing project.
    ///
    public static class ProjectController {
        // =================================================================================
        // Fields
        // ---------------------------------------------------------------------------------
        static ProjectInfo  myProject= null;
        
        // =================================================================================
        // Properties
        // ---------------------------------------------------------------------------------
        public static ProjectInfo Project {
            get {
                if(myProject == null) {
                    GetProject();
                }
                return myProject;
            }
            set {
                if(myProject != value) {
                    SaveProject();
                }
            }
        }
        // =================================================================================
        // INIT / SHUTDOWN
        // ---------------------------------------------------------------------------------
        static ProjectController() {
            LoadProjectFromPreferences();
        }
        public static void Start() {}
        public static void Shutdown() {
            SaveProjectToPreferences();
        }
        
        // =================================================================================
        /// Loads the active project from the user preferences.
        public static void LoadProjectFromPreferences() {
            // TODO:
            GetProject();
        }

        // =================================================================================
        /// Save to active project reference in the user preferences.
        public static void SaveProjectToPreferences() {
            if(myProject == null) return;
            SaveProject();
            //TODO:
        }

        // =================================================================================
        /// Save the project file to disk.
        public static void SaveProject() {
            // TODO:
        }

        // =================================================================================
        /// Load the project file from disk.
        public static void LoadProject() {
            // TODO:
        }

        // =================================================================================
        /// Ask the user to create or select an exist project.
        public static void GetProject() {
            // TODO:
            var projects= FileUtils.GetFilesWithExtension("icsproject");
            foreach(var p in projects) {
                Debug.Log(p);
            }
        }
    }

}