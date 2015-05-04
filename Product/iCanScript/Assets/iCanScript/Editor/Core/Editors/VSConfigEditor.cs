﻿using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using iCanScript.Engine;
using TimedAction= iCanScript.Prelude.TimerService.TimedAction;

namespace iCanScript.Editor {
    using Prefs= PreferencesController;

    public class VSConfigEditor : ConfigEditorBase {
        // =================================================================================
        // Fields
        // ---------------------------------------------------------------------------------
        iCS_IStorage    iStorage             = null;
    	string[]        vsConfigOptionStrings= new string[]{
    	    "General"
    	};
    	
        // =================================================================================
        // INITIALIZATION
        // ---------------------------------------------------------------------------------
        public static void Init(iCS_IStorage iStorage) {
            var editor= EditorWindow.CreateInstance<VSConfigEditor>();
            editor.ShowUtility();
            editor.iStorage= iStorage;
        }
        // =================================================================================
        // INTERFACES TO BE PROVIDED
        // ---------------------------------------------------------------------------------
        protected override string   GetTitle() {
            return "Visual Script Configuration";
        }
        protected override string[] GetMainSelectionGridStrings() {
            return vsConfigOptionStrings;
        }
        protected override void     ProcessSelection(int selection) {
            // -- Don't continue if our VS is not accessible --
            if(iStorage == null) {
                Close();
                return;
            }
            // -- Execute option specific panel. --
            switch(selection) {
                case 0: General(); break;
                default: break;
            }
        }
        

    	// =================================================================================
        // DISPLAY OPTION PANEL
        // ---------------------------------------------------------------------------------
        void General() {
            // -- Label column --
            var pos= GetLabelColumnPositions(8);
            GUI.Label(pos[0], "Type Name");
            GUI.Label(pos[2], "Is Editor Script");
            GUI.Label(pos[3], "Base Type Override");
            EditorGUI.BeginDisabledGroup(!iStorage.BaseTypeOverride);
            GUI.Label(pos[4], "Base Type Name");
            EditorGUI.EndDisabledGroup();
            GUI.Label(pos[6], "Namespace Override");
            EditorGUI.BeginDisabledGroup(!iStorage.NamespaceOverride);
            GUI.Label(pos[7], "Namespace");
            EditorGUI.EndDisabledGroup();
            
            // -- Value column --
            pos= GetValueColumnPositions(8);
            iStorage.TypeName= EditorGUI.TextField(pos[0], iStorage.TypeName);
            var savedGuiChanged= GUI.changed;
            GUI.changed= false;
            iStorage.IsEditorScript= EditorGUI.Toggle(pos[2], iStorage.IsEditorScript);
            if(GUI.changed) {
                UpdateEditorScriptOption();
            }
            GUI.changed |= savedGuiChanged;
            iStorage.BaseTypeOverride= EditorGUI.Toggle(pos[3], iStorage.BaseTypeOverride);
            EditorGUI.BeginDisabledGroup(!iStorage.BaseTypeOverride);
            iStorage.BaseType= EditorGUI.TextField(pos[4], iStorage.BaseType);
            GUI.Label(pos[5], "<i>(format: namespace.type)</i>");
            EditorGUI.EndDisabledGroup();
            iStorage.NamespaceOverride= EditorGUI.Toggle(pos[6], iStorage.NamespaceOverride);
            EditorGUI.BeginDisabledGroup(!iStorage.NamespaceOverride);
            iStorage.Namespace= EditorGUI.TextField(pos[7], iStorage.Namespace);
            EditorGUI.EndDisabledGroup();
    
            // -- Reset button --
            if(GUI.Button(new Rect(kColumn2X+kMargin, position.height-kMargin-20.0f, 0.75f*kColumn2Width, 20.0f),"Use Defaults")) {
                iStorage.TypeName         = iStorage.EditorObjects[0].CodeName;
                iStorage.IsEditorScript   = false;
                iStorage.BaseTypeOverride = false;
                iStorage.BaseType         = Prefs.EngineBaseType;
                iStorage.NamespaceOverride= false;
                iStorage.Namespace        = CodeGenerationUtility.GetDefaultNamespace(iStorage);
            }

    		// -- Save changes --
            if(GUI.changed) {
                iStorage.SaveStorage();
            }

            // -- Validate user entries --
            var message= Sanity.ValidateVisualScriptBaseType(iStorage);
            if(message != null) {
                DisplayError(pos[4], message);
                return;
            }
            message= Sanity.ValidateVisualScriptNamespace(iStorage, /*shortFormat=*/true);
            if(message != null) {
                DisplayError(pos[7], message);
                return;
            }
            message= Sanity.ValidateVisualScriptTypeName(iStorage, /*shortFormat=*/true);
            if(message != null) {
                DisplayError(pos[0], message);
                return;
            }
        }
        
        // ---------------------------------------------------------------------------------
        /// Updates the option to determines if this is an editor script.
        void UpdateEditorScriptOption() {
            if(iStorage.IsEditorScript) {
                iStorage.BaseTypeOverride= true;
                iStorage.BaseType        = "";
                if(iStorage.NamespaceOverride == false) {
                    iStorage.Namespace= Prefs.EditorNamespace;
                }
                if(Prefs.UseUnityEditorLibrary == false) {
                    if(EditorUtility.DisplayDialog("The Unity Editor Library must be enabled to create editor scipts.", "Do you want to enable the Unity Editor Library?", "Enable", "Abort")) {
                        Prefs.UseUnityEditorLibrary= true;                        
                    }
                }
            }
            else {
                iStorage.BaseTypeOverride= false;
                iStorage.BaseType        = Prefs.EngineBaseType;
                if(iStorage.NamespaceOverride == false) {
                    iStorage.Namespace= Prefs.EngineNamespace;
                }
            }
        }
    }

}
