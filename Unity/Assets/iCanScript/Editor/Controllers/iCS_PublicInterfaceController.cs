﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using P=Prelude;

public static class iCS_PublicInterfaceController {
    // ======================================================================
    // Initialization
    // ----------------------------------------------------------------------
    static iCS_PublicInterfaceController() {
        // Verify that the scene controller is ran before us.
        if(iCS_SceneController.VisualScriptsInScene == null) {
            Debug.LogError("iCanScript: Please move PublicInterfaceController after the SceneController in AppController");
        }
        // Events to refresh scene content information.
        iCS_SystemEvents.OnSceneChanged    = RefreshPublicInterfaceInfo;
        iCS_SystemEvents.OnHierarchyChanged= RefreshPublicInterfaceInfo;
        iCS_SystemEvents.OnProjectChanged  = RefreshPublicInterfaceInfo;
        // Events to refresh visual script information.
        iCS_SystemEvents.OnVisualScriptUndo                = OnVisualScriptUndo;                
        iCS_SystemEvents.OnVisualScriptElementAdded        = OnVisualScriptElementAdded;        
        iCS_SystemEvents.OnVisualScriptElementWillBeRemoved= OnVisualScriptElementWillBeRemoved;
        iCS_SystemEvents.OnVisualScriptElementNameChanged  = OnVisualScriptElementNameChanged;
        // Force an initial refresh of the scene info.
        RefreshPublicInterfaceInfo();
    }
    public static void Start() {}
    public static void Shutdown() {
    }
    

    // ======================================================================
    // Types
    // ----------------------------------------------------------------------
	public class VSPublicGroups {
		Dictionary<string, VSObjectReferenceGroup>	myGroups= null;
		
		public VSPublicGroups() {
			myGroups= new Dictionary<string, VSObjectReferenceGroup>();
		}
		public int	NbOfGroups { get { return myGroups.Count; }}
		public void Add(VSObjectReference objRef) {
			var objName= objRef.EngineObject.Name;
			VSObjectReferenceGroup	group= null;
			if(myGroups.TryGetValue(objName, out group)) {
				group.Add(objRef);
			}
			else {
				var newGroup= new VSObjectReferenceGroup();
				newGroup.Add(objRef);
				myGroups.Add(objName, newGroup);
			}
		}
        public void Add(iCS_EditorObject element) {
            Add(new VSObjectReference(element.IStorage.VisualScript, element.InstanceId));
        }
		public void Remove(VSObjectReference objRef) {
			var objName= objRef.EngineObject.Name;
			VSObjectReferenceGroup	group= null;
			if(myGroups.TryGetValue(objName, out group)) {
				group.Remove(objRef);
				if(group.IsEmpty) {
					myGroups.Remove(objName);
				}
			}
		}
        public void Remove(iCS_EditorObject element) {
            Remove(new VSObjectReference(element.IStorage.VisualScript, element.InstanceId));
        }
		public void ForEach(Action<string, VSObjectReferenceGroup> action) {
			foreach(var p in myGroups) {
				action(p.Key, p.Value);
			}
		}
		public VSObjectReferenceGroup Find(string name) {
			VSObjectReferenceGroup	group= null;
			myGroups.TryGetValue(name, out group);
			return group;
		}
	}
    // ----------------------------------------------------------------------	
	public class VSObjectReferenceGroup {
		List<VSObjectReference>	myDefinitions= new List<VSObjectReference>();
		List<VSObjectReference> myReferences = new List<VSObjectReference>();
		
		public VSObjectReferenceGroup() {}
		public List<VSObjectReference>	Definitions 	{ get { return myDefinitions; }}
		public List<VSObjectReference>	References		{ get { return myReferences; }}
		public int 						NbOfDefinitions	{ get { return myDefinitions.Count; }}
		public int 						NbOfReferences	{ get { return myReferences.Count; }}
		public bool						IsEmpty			{ get { return NbOfDefinitions+NbOfReferences == 0; }}
		public void Add(VSObjectReference objRef) {
			if(objRef.IsPublicVariable || objRef.IsPublicFunction) {
			   myDefinitions.Add(objRef);	
			}
			else {
				myReferences.Add(objRef);
			}
		}
		public void Remove(VSObjectReference objRef) {
			if(objRef.IsPublicVariable || objRef.IsPublicFunction) {
                   for(int i= 0; i < myDefinitions.Count; ++i) {
                       var definition= myDefinitions[i];
                       if(definition.ObjectId == objRef.ObjectId && definition.VisualScript == objRef.VisualScript) {
                           myDefinitions.RemoveAt(i);
                           break;
                       }
                   }
			}
			else {
                for(int i= 0; i < myReferences.Count; ++i) {
                    var reference= myReferences[i];
                    if(reference.ObjectId == objRef.ObjectId && reference.VisualScript == objRef.VisualScript) {
                        myReferences.RemoveAt(i);
                        break;
                    }
                }
			}			
		}
	};
    public class VSObjectReference {
        iCS_VisualScriptImp     myVisualScript= null;
        int                     myObjectId= -1;

        public VSObjectReference(iCS_VisualScriptImp visualScript, int objectId) {
            myVisualScript= visualScript;
            myObjectId= objectId;
        }
        public iCS_VisualScriptImp VisualScript 		{ get { return myVisualScript; }}
        public int                 ObjectId     		{ get { return myObjectId; }}
        public iCS_EngineObject    EngineObject 		{ get { return myVisualScript.EngineObjects[myObjectId]; }}
        public bool                IsVariableReference  { get { return EngineObject.IsVariableReference; }}
        public bool                IsFunctionCall       { get { return EngineObject.IsFunctionCall; }}
        public bool                IsPublicVariable
			{ get { return iCS_VisualScriptData.IsPublicVariable(VisualScript, EngineObject); }}
        public bool                IsPublicFunction
			{ get { return iCS_VisualScriptData.IsPublicFunction(VisualScript, EngineObject); }}
		public bool				   IsPublicInterface
			{ get { return IsPublicVariable || IsPublicFunction; }}
		public bool				   IsReferenceToPublicInterface
			{ get { return IsVariableReference || IsFunctionCall; }}
		public bool				   IsDynamicReference
			{ get { return VisualScript.IsReferenceNodeUsingDynamicBinding(EngineObject); }}
		public iCS_VisualScriptImp TargetVisualScript
			{ get { return VisualScript.GetVisualScriptFromReferenceNode(EngineObject); }}
		public iCS_EngineObject	   TargetEngineObject
			{ get { return VisualScript.GetEngineObjectFromReferenceNode(EngineObject); }}
    };


    // ======================================================================
    // Fields 
    // ----------------------------------------------------------------------
    static VSObjectReference[]	ourPublicVariables   = null;
    static VSObjectReference[]	ourPublicFunctions   = null;
    static VSObjectReference[]	ourVariableReferences= null;
    static VSObjectReference[]	ourFunctionCalls     = null;
	static VSPublicGroups		ourPublicVariableGroups= null;
	static VSPublicGroups		ourPublicFunctionGroups= null;
	
    // ======================================================================
    // Scene properties
    // ----------------------------------------------------------------------
    public static VSObjectReference[] PublicVariables {
        get { return ourPublicVariables; }
    }
    public static VSObjectReference[] PublicFunctions {
        get { return ourPublicFunctions; }
    }
    public static VSObjectReference[] VariableReferences {
        get { return ourVariableReferences; }
    }
    public static VSObjectReference[] FunctionCalls {
        get { return ourFunctionCalls; }
    }
	public static VSPublicGroups	PublicVariableGroups {
		get { return ourPublicVariableGroups; }
		set { ourPublicVariableGroups= value; }
	}
	public static VSPublicGroups	PublicFunctionGroups {
		get { return ourPublicFunctionGroups; }
		set { ourPublicFunctionGroups= value; }
	}


    // ======================================================================
    // Update scene content changed
    // ----------------------------------------------------------------------
    static void RefreshPublicInterfaceInfo() {
		// Extract all public interface definitions and usages
        ourPublicVariables   = ScanForPublicVariables();
        ourPublicFunctions   = ScanForPublicFunctions();
        ourVariableReferences= ScanForVariableReferences();
        ourFunctionCalls     = ScanForFunctionCalls();
		
		// Build groups out of linked objects
		PublicVariableGroups= new VSPublicGroups();
		PublicFunctionGroups= new VSPublicGroups();
		foreach(var pv in ourPublicVariables)		{ PublicVariableGroups.Add(pv); }
		foreach(var vr in ourVariableReferences)	{ PublicVariableGroups.Add(vr); }
		foreach(var pf in ourPublicFunctions)		{ PublicFunctionGroups.Add(pf); }
		foreach(var fc in ourFunctionCalls)			{ PublicFunctionGroups.Add(fc); }
		
//#if DEBUG
//        Debug.Log("Scene as changed =>"+EditorApplication.currentScene);
//		Debug.Log("NbOfPublicVariableGroups=> "+PublicVariableGroups.NbOfGroups);
//		PublicVariableGroups.ForEach(
//			(name, group)=> {
//				Debug.Log("Group Name=> "+name+" #Definitions=> "+group.NbOfDefinitions+" #References=> "+group.NbOfReferences);
//			}
//		);
//		Debug.Log("NbOfPublicFunctionGroups=> "+PublicFunctionGroups.NbOfGroups);
//		PublicFunctionGroups.ForEach(
//			(name, group)=> {
//				Debug.Log("Group Name=> "+name+" #Definitions=> "+group.NbOfDefinitions+" #References=> "+group.NbOfReferences);
//			}
//		);
//#endif
    }

    // ======================================================================
    // Update visual script content changed
    // ----------------------------------------------------------------------
    static void OnVisualScriptUndo(iCS_IStorage iStorage) {
#if DEBUG
        Debug.Log("Visual Script undo=> "+iStorage.VisualScript.name);
#endif
        RefreshPublicInterfaceInfo();
    }
    static void OnVisualScriptElementAdded(iCS_IStorage iStorage, iCS_EditorObject element) {
#if DEBUG
        Debug.Log("Visual Script element added=> "+iStorage.VisualScript.name+"."+element.Name);
#endif
        if(element.IsPublicVariable || element.IsVariableReference) {
            PublicVariableGroups.Add(element);
        }
        if(element.IsPublicFunction || element.IsFunctionCall) {
            PublicFunctionGroups.Add(element);
        }
        ValidatePublicGroups();
    }
    static void OnVisualScriptElementWillBeRemoved(iCS_IStorage iStorage, iCS_EditorObject element) {
#if DEBUG
        Debug.Log("Visual Script element will be removed=> "+iStorage.VisualScript.name+"."+element.Name);
#endif
        if(element.IsPublicVariable || element.IsVariableReference) {
            PublicVariableGroups.Remove(element);
        }
        if(element.IsPublicFunction || element.IsFunctionCall) {
            PublicFunctionGroups.Remove(element);
        }
        ValidatePublicGroups();
    }
    static void OnVisualScriptElementNameChanged(iCS_IStorage iStorage, iCS_EditorObject element) {
#if DEBUG
        Debug.Log("Visual Script element name changed=> "+iStorage.VisualScript.name+"."+element.Name);
#endif
        ValidatePublicGroups();
    }

    // ======================================================================
    // PUBLIC INTERFACES
    // ----------------------------------------------------------------------
    static VSObjectReference[] ScanForPublicVariables() {
        var result= new List<VSObjectReference>();
        P.forEach(vs=> {
                var publicVariables= iCS_VisualScriptData.FindPublicVariables(vs);
                P.forEach(
                    pv=> {
                        result.Add(new VSObjectReference(vs, pv.InstanceId));
                    },
                    publicVariables
                );
            },
            iCS_SceneController.VisualScriptsInOrReferencedByScene
        );
        return result.ToArray();

//        return P.fold(
//            (result,vs)=> (
//                P.append(
//                    P.map(
//                        pv=> new VSObjectReference(vs, pv.InstanceId),
//                        iCS_VisualScriptData.FindPublicVariables(vs)
//                    ),
//                    result
//                )                
//            ),
//            new VSObjectReference[0],
//            VisualScriptsInOrReferencedByScene
//        );

//        return P.fold(
//            (result,vs)=> (
//                P.fold(
//                    (acc,pv)=> {
//                        acc.Add(new VSObjectReference(vs, pv.InstanceId));
//                        return acc;
//                    },
//                    result,
//                    iCS_VisualScriptData.FindPublicVariables(vs)        
//                )
//            ),
//            new List<VSObjectReference>(),
//            VisualScriptsInOrReferencedByScene
//        ).ToArray();
    }
    static VSObjectReference[] ScanForPublicFunctions() {
        var result= new List<VSObjectReference>();
        P.forEach(vs=> {
                var publicFunctions= iCS_VisualScriptData.FindPublicFunctions(vs);
                P.forEach(
                    pf=> {
                        result.Add(new VSObjectReference(vs, pf.InstanceId));
                    },
                    publicFunctions
                );
            },
            iCS_SceneController.VisualScriptsInOrReferencedByScene
        );
        return result.ToArray();
    }
    static VSObjectReference[] ScanForVariableReferences() {
        var result= new List<VSObjectReference>();
        P.forEach(vs=> {
                var variableReferences= iCS_VisualScriptData.FindVariableReferences(vs);
                P.forEach(
                    vr=> {
                        result.Add(new VSObjectReference(vs, vr.InstanceId));
                    },
                    variableReferences
                );
            },
            iCS_SceneController.VisualScriptsInOrReferencedByScene
        );
        return result.ToArray();
    }
    static VSObjectReference[] ScanForFunctionCalls() {
        var result= new List<VSObjectReference>();
        P.forEach(vs=> {
                var functionCalls= iCS_VisualScriptData.FindFunctionCalls(vs);
                P.forEach(
                    fc=> {
                        result.Add(new VSObjectReference(vs, fc.InstanceId));
                    },
                    functionCalls
                );
            },
            iCS_SceneController.VisualScriptsInOrReferencedByScene
        );
        return result.ToArray();
    }
    // ----------------------------------------------------------------------
	static iCS_VisualScriptImp FindVisualScriptFromReferenceNode(VSObjectReference objRef) {
		return objRef.VisualScript.GetVisualScriptFromReferenceNode(objRef.EngineObject);
	}
	
	// =========================================
	// = Public Interface Validation Utilities =
	// =========================================
    // ----------------------------------------------------------------------
	public static P.Tuple<VSObjectReference,VSObjectReference>[] ValidateVariableDefinitions() {
		var result= new List<P.Tuple<VSObjectReference,VSObjectReference> >();
        PublicVariableGroups.ForEach(
            (name, group)=> {
				var definitions= group.Definitions;
				if(P.length(definitions) < 2) return;
				var qualifiedType= definitions[0].EngineObject.QualifiedType;
				P.fold(
					(acc,o)=> {
						if(qualifiedType != o.EngineObject.QualifiedType) {
							acc.Add(new P.Tuple<VSObjectReference,VSObjectReference>(definitions[0], o));
						}
						return acc;
					}
					, result
					, definitions
				);
            }
        );
		return result.ToArray();
	}
    // ----------------------------------------------------------------------
	public static bool IsVariableDefinitionCompliant(iCS_EditorObject variable) {
		var group= PublicVariableGroups.Find(variable.Name);
		if(group == null) return true;
		var definitions= group.Definitions;
		if(P.length(definitions) == 0) return true;
		var type= variable.RuntimeType;
		return P.fold((acc,o)=> acc || type == o.EngineObject.RuntimeType, false, definitions);
	}
    // ----------------------------------------------------------------------
	public static P.Tuple<VSObjectReference,VSObjectReference>[] ValidateFunctionDefinitions() {
		var result= new List<P.Tuple<VSObjectReference,VSObjectReference> >();
        PublicFunctionGroups.ForEach(
            (name, group)=> {
				var definitions= group.Definitions;
				if(P.length(definitions) < 2) return;
				var refFnc= definitions[0];
				var vs = definitions[0].VisualScript;
				var obj= definitions[0].EngineObject;
				P.fold(
					(acc,o)=> {
						if(!IsSameFunction(vs, obj, o.VisualScript, o.EngineObject)) {
							acc.Add(new P.Tuple<VSObjectReference,VSObjectReference>(refFnc, o));
						}
						return acc;
					}
					, result
					, definitions
				);
            }
        );
		return result.ToArray();
	}
    // ----------------------------------------------------------------------
	/// Determines if the given functions have the same composition.
	static bool IsSameFunction(iCS_VisualScriptImp vs1, iCS_EngineObject f1,
									 iCS_VisualScriptImp vs2, iCS_EngineObject f2) {
		if(f1.Name != f2.Name) return false;
		var ps1= iCS_VisualScriptData.GetChildPorts(vs1, f1);
		var ps2= iCS_VisualScriptData.GetChildPorts(vs2, f2);
		if(P.length(ps1) != P.length(ps2)) return false;
		return P.and(P.zipWith( (p1,p2)=> IsSamePort(vs1,p1,vs2,p2), ps1, ps2));
	}
    // ----------------------------------------------------------------------
	/// Determines if the given ports have the same relative identification.
	static bool IsSamePort(iCS_VisualScriptImp vs1, iCS_EngineObject p1,
						   iCS_VisualScriptImp vs2, iCS_EngineObject p2) {
		if(p1.ParameterIndex != p2.ParameterIndex) return false;
		if(p1.RuntimeType != p2.RuntimeType) return false;
		return true;
	}




    // ----------------------------------------------------------------------
	public static bool ValidatePublicGroups() {
        // Validate Variables
		ValidateVariableDefinitions();
		ValidateFunctionDefinitions();
		
        PublicVariableGroups.ForEach(
            (name, group)=> {
                var definitions= group.Definitions;
                var references = group.References;
                if(references.Count != 0 && definitions.Count == 0) {
                    foreach(var varRef in references) {
                        Debug.LogWarning("iCanScript: No definition found for variable=> "+name+" in visual script=> "+varRef.VisualScript.name);                    
                    }
                }                
            }
        );
        
        // Validate Functions
        PublicFunctionGroups.ForEach(
            (name, group)=> {
                var definitions= group.Definitions;
                var references = group.References;
                if(references.Count != 0 && definitions.Count == 0) {
                    foreach(var varRef in references) {
                        Debug.LogWarning("iCanScript: No definition found for variable=> "+name+" in visual script=> "+varRef.VisualScript.name);
                    }
                }
            }
        );
		return true;
	}


}
