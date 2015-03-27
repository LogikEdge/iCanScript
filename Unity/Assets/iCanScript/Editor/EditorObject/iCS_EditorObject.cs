using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using iCanScript.Editor;

public partial class iCS_EditorObject {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    iCS_IStorage    myIStorage       = null;
    int             myId             = -1;
    bool            myIsFloating     = false;
    List<int>		myChildren       = new List<int>();
    bool            myIsSticky       = false;

    // ======================================================================
    // Cache
    // ----------------------------------------------------------------------
    Type    cachedRuntimeType     = null;
    string  cachedNodeTitle       = null;
    Vector2 cachedNodeTitleSize   = Vector2.zero;
    string  cachedNodeSubTitle    = null;
    Vector2 cachedNodeSubTitleSize= Vector2.zero;
    string  cachedCodeName        = null;

    // ======================================================================
    // Conversion Utilities
    // ----------------------------------------------------------------------
    public iCS_IStorage IStorage {
        get { return myIStorage; }
    }
	public iCS_VisualScriptData Storage {
		get { return myIStorage.Storage; }
	}
    public iCS_MonoBehaviourImp iCSMonoBehaviour {
        get { return myIStorage.iCSMonoBehaviour; }
    }
    public List<iCS_EditorObject> EditorObjects {
        get { return myIStorage.EditorObjects; }
    }
    public iCS_EditorObject EditorObject {
		get { return EditorObjects[myId]; }
	}
    public List<iCS_EngineObject> EngineObjects {
        get { return myIStorage.EngineObjects; }
    }
    public iCS_EngineObject EngineObject {
		get { return EngineObjects[myId]; }
	}
    List<iCS_EngineObject> EditorToEngineList(List<iCS_EditorObject> editorObjects) {
        return Prelude.map(eo=> (eo != null ? eo.EngineObject : null), editorObjects);
    }
    
    // ======================================================================
    // Engine Object Accessors
    // ----------------------------------------------------------------------
    public iCS_ObjectTypeEnum ObjectType {
		get { return EngineObject.ObjectType; }
		set {
            var engineObject= EngineObject;
            if(engineObject.ObjectType == value) return;
		    engineObject.ObjectType= value;
		}
	}
    // ----------------------------------------------------------------------
    public int ParentId {
		get { return EngineObject.ParentId; }
		set {
			int pid= EngineObject.ParentId;
			if(pid == value) return;
			if(IsIdValid(pid)) {
				var oldParent= EditorObjects[pid];
				oldParent.RemoveChild(this);
			}
			EngineObject.ParentId= value;
			if(IsIdValid(value)) {
				var newParent= EditorObjects[value];
				newParent.AddChild(this);
			}
		}
	}
    // ----------------------------------------------------------------------
    public iCS_DisplayOptionEnum DisplayOption {
        get { return EngineObject.DisplayOption; }
        set {
            var engineObject= EngineObject;
            if(engineObject.DisplayOption == value) return;
            engineObject.DisplayOption= value;
        }
    }
    // ----------------------------------------------------------------------
    public Type RuntimeType {
		get {
            if(cachedRuntimeType == null) {
                cachedRuntimeType= EngineObject.RuntimeType;
            }
            return cachedRuntimeType;
        }
	}
//    // ----------------------------------------------------------------------
//    /// Returns the name as per the underlying code.
//    public string CodeName {
//        get {
//            if(cachedCodeName == null) {
//                if(IsPackage) {
//                    cachedCodeName= "Package";
//                }
//                else if(IsConstructor) {
//                    cachedCodeName= iCS_Types.TypeName(RuntimeType);
//                }
//                else if(IsNode) {
//                    var desc= iCS_LibraryDatabase.GetAssociatedDescriptor(this);
//                    if(desc != null) {
//                        var funcInfo= desc.ToFunctionPrototypeInfo;
//                        cachedCodeName= funcInfo.MethodName;
//                    }
//                }
//                else {
//                    //cachedCodeName= EngineObject.Name;
//                }
//            }
//            return cachedCodeName ?? "";
//        }
//    }
    // ----------------------------------------------------------------------
    public string Name {
		get {
            if(IsDataPort) {
                if(IsProgrammaticInstancePort) {
                    if(IsOutputPort) return "Self";
                    return "Target";
                }                
            }
            return EngineObject.Name;
        }
		set {
            var engineObject= EngineObject;
            if(engineObject.Name == value) return;
		    engineObject.Name= value;
            ResetNameCaches();
		}
	}
    // ----------------------------------------------------------------------
    public string FullName {
        get { return Storage.GetFullName(iCSMonoBehaviour, EngineObject); }
    }
    // ----------------------------------------------------------------------
    public string DefaultName {
        get {
            var defaultName= Name;
            if(IsPackage) {
                defaultName= "";                
            }
            else if(IsConstructor) {
                defaultName= "Variable";
            }
            else {
                if(IsNode) {
                    var desc= iCS_LibraryDatabase.GetAssociatedDescriptor(this);
                    if(desc != null) {
                        defaultName= desc.DisplayName;
                    }
                    else {
                        defaultName= EngineObject.MethodName;                        
                    }
                }
                else {
                    // TODO: Support retreiving the initial port name.
                }
            }
            return defaultName;
        }
    }
    // ----------------------------------------------------------------------
    /// This functions resets all name related caches.
    void ResetNameCaches() {
        cachedNodeTitle= null;
        cachedNodeTitleSize= Vector2.zero;
    }
    // ----------------------------------------------------------------------
    public bool IsNameEditable {
		get { return EngineObject.IsNameEditable && !IsMessageHandler; }
		set {
            var engineObject= EngineObject;
            if(engineObject.IsNameEditable == value) return;
		    engineObject.IsNameEditable= value;
		}
	}
    // ----------------------------------------------------------------------
    public string Tooltip {
		get { return EngineObject.Tooltip; }
		set {
            var engineObject= EngineObject;
            if(engineObject.Tooltip == value) return;
		    engineObject.Tooltip= value;
		}
	}
    // ----------------------------------------------------------------------
    public string NodeTitle {
        get {
            if(cachedNodeTitle == null) {
                cachedNodeTitle= iCS_TextUtility.NicifyName(DisplayName);
            }
            return cachedNodeTitle;
        }
    }
    // ----------------------------------------------------------------------
    public Vector2 NodeTitleSize {
        get {
            if(Math3D.IsZero(cachedNodeTitleSize)) {
                var titleContent= new GUIContent(NodeTitle);
                cachedNodeTitleSize= iCS_Layout.DefaultTitleStyle.CalcSize(titleContent);
            }
            return cachedNodeTitleSize;
        }
    }

    // ----------------------------------------------------------------------
    /// Builds and returns the Node SubTitle text.
    public string NodeSubTitle {
        get {
            if(cachedNodeSubTitle == null) {
                cachedNodeSubTitleSize= Vector2.zero;
                if(IsConstructor) {
                    cachedNodeSubTitle= BuildIsASubTitle("Self", RuntimeType);
                }
                else if(IsKindOfFunction || IsMessageHandler || IsInstanceNode) {
                    cachedNodeSubTitle= BuildIsASubTitle("Target", RuntimeType);
                }
                else if(IsKindOfPackage) {
                    cachedNodeSubTitle= "Node is a Package";
                }
                else {
                    cachedNodeSubTitle= null;
                }
            }
            return cachedNodeSubTitle ?? "";
        }
    }

    // ----------------------------------------------------------------------
    /// Builds a standard IsA type of node subtitle string.
    ///
    /// @param name The name to be used that _"is a"_.
    /// @param type The type of the _"is a"_.
    ///
    string BuildIsASubTitle(string name, Type type) {
        var result= new StringBuilder(name);
        result.Append(" is a");
        var typeName= iCS_TextUtility.NicifyName(iCS_Types.TypeName(type));
        if(iCS_TextUtility.StartsWithAVowel(typeName)) {
            result.Append('n');
        }
        result.Append(" ");
        result.Append(typeName);
        return result.ToString();        
    }

    // ----------------------------------------------------------------------
    /// Returns the rendering dimension of the Node SubTitle text.
    public Vector2 NodeSubTitleSize {
        get {
            if(Math3D.IsZero(cachedNodeSubTitleSize)) {
                var guiContent= new GUIContent(NodeSubTitle);
                cachedNodeSubTitleSize= iCS_Layout.DefaultSubTitleStyle.CalcSize(guiContent);
            }
            return cachedNodeSubTitleSize;
        }
    }    
        
    // ======================================================================
    // High-Level Properties
    // ----------------------------------------------------------------------
	public bool IsIdValid(int id)	{ return id >= 0 && id < EditorObjects.Count && id < EngineObjects.Count && EditorObjects[id] != null; }
	public bool	IsParentValid		{ get { return IsIdValid(ParentId); }}
	public bool IsSourceValid		{ get { return IsIdValid(ProducerPortId); }}
    public bool IsSelected          { get { return myIStorage.SelectedObject == this; }}

	public bool IsValid {
		get { return IsIdValid(InstanceId); }
	}
    public int InstanceId { 
		get { return myId; }
	}
    public iCS_EditorObject Parent {
		get { return IsParentValid ? myIStorage[ParentId] : null; }
		set { ParentId= (value != null ? value.InstanceId : -1); }
	}
	public iCS_EditorObject ParentNode {
	    get {
	        var parent= Parent;
	        while(parent != null && !parent.IsNode) parent= parent.Parent;
	        return parent;
	    }
	}
    public bool IsFloating {
		get { return myIsFloating; }
		set {
            if(myIsFloating == value) return;
		    myIsFloating= value;
		}
	}
	public bool IsParentFloating {
		get {
			for( var parent= ParentNode; parent != null; parent= parent.ParentNode)  {
				if( parent.IsFloating ) return true;
			}
			return false;
		}
	}
	public bool IsSticky {
	    get { return myIsSticky; }
	    set { myIsSticky= value; }
	}
	public bool IsDisplayRoot {
	    get {
            return this.InstanceId == Storage.DisplayRoot;
	    }
	}
    
    // ======================================================================
    // Constructors/Builders
    // ----------------------------------------------------------------------
	// Creates an instance of an editor/engine object pair.
	public static iCS_EditorObject CreateInstance(int id, string name, Type type,
												  int parentId, iCS_ObjectTypeEnum objectType,
                            					  iCS_IStorage iStorage) {
		if(id < 0) return null;
		// Create engine object.
		var engineObject= new iCS_EngineObject(id, name, type, parentId, objectType);
		AddEngineObject(id, engineObject, iStorage);
		// Create editor object.
		var editorObject= new iCS_EditorObject(id, iStorage);
		AddEditorObject(id, editorObject);
        RunOnCreated(editorObject);
		return editorObject;
	}
    // ----------------------------------------------------------------------
    // Duplicate the given editor object with a new id and parent.
    public static iCS_EditorObject Clone(int id, iCS_EditorObject toClone, iCS_EditorObject parent,
                                         iCS_IStorage iStorage) {
		if(id < 0) return null;
		// Create engine object.
        var engineObject= iCS_EngineObject.Clone(id, toClone.EngineObject,
												 (parent == null ? null : parent.EngineObject));
		AddEngineObject(id, engineObject, iStorage);
		// Create editor object.
		var editorObject= new iCS_EditorObject(id, iStorage);
		AddEditorObject(id, editorObject);
        editorObject.LocalSize= toClone.LocalSize;
        RunOnCreated(editorObject);
        if(editorObject.IsInDataOrControlPort && toClone.ProducerPortId == -1) {
            editorObject.InitialValue= toClone.IStorage.GetInitialPortValueFromArchive(toClone);
            editorObject.IStorage.StoreInitialPortValueInArchive(editorObject);
        }
		return editorObject;
    }

    // ----------------------------------------------------------------------
    // Reinitialize the editor object to its default values.
    public void DestroyInstance() {
        // Invoke event
        RunOnWillDestroy(this);
        // Destroy any children.
        ForEachChild(child=> child.DestroyInstance());        
        // Disconnect any port sourcing from this object.
        if(IsPort) {
            myIStorage.ForEach(
                child=> {
                    if(child.IsPort && child.ProducerPortId == InstanceId) {
                        child.ProducerPortId= -1;
                    }                    
                }
            );
        }
		// Update child lists.
		if(IsParentValid) {
			Parent.RemoveChild(this);
		}
        // Assure that the selected object is not us.
        if(myIStorage.SelectedObject == EditorObject) myIStorage.SelectedObject= null;
		// Reset the associated engine object.
        EngineObject.DestroyInstance();
        // Remove editor object.
        EditorObjects[myId]= null;
    }
    
    // ======================================================================
	// Grow database if needed.
    // ----------------------------------------------------------------------
	private static void AddEngineObject(int id, iCS_EngineObject engineObject, iCS_IStorage iStorage) {
		int len= iStorage.EngineObjects.Count;
		if(id < 0) return;
		if (id < len) {
			iStorage.EngineObjects[id]= engineObject;
			return;
		}
		if(id == len) {
			iStorage.EngineObjects.Add(engineObject);
			return;
		}
		if(id > len) {
			GrowEngineObjectList(id, iStorage);
			iStorage.EngineObjects.Add(engineObject);
		}		
	}
    // ----------------------------------------------------------------------
	private static void AddEditorObject(int id, iCS_EditorObject editorObject) {
		var iStorage= editorObject.myIStorage;
		int len= iStorage.EditorObjects.Count;
		if(id < 0) return;
		if (id < len) {
			iStorage.EditorObjects[id]= editorObject;
			return;
		}
		if(id == len) {
			iStorage.EditorObjects.Add(editorObject);
			return;
		}
		if(id > len) {
			GrowEditorObjectList(id, iStorage);
			iStorage.EditorObjects.Add(editorObject);
		}		
	}
	private static void GrowEngineObjectList(int size, iCS_IStorage iStorage) {
        // Reserve space to contain the total amount of objects.
        if(size > iStorage.EngineObjects.Capacity) {
            iStorage.EngineObjects.Capacity= size;
        }
        // Add the number of missing objects.
        for(int len= iStorage.EngineObjects.Count; size > len; ++len) {
            iStorage.EngineObjects.Add(iCS_EngineObject.CreateInvalidInstance());
        }
	}
	private static void GrowEditorObjectList(int size, iCS_IStorage iStorage) {
        // Reserve space to contain the total amount of objects.
        if(size > iStorage.EditorObjects.Capacity) {
            iStorage.EditorObjects.Capacity= size;
        }
        // Add the number of missing objects.
        for(int len= iStorage.EditorObjects.Count; size > len; ++len) {
            iStorage.EditorObjects.Add(null);
        }
	}
    
    // ======================================================================
	// Rebuild from engine database.
    // ----------------------------------------------------------------------
    public iCS_EditorObject(int id, iCS_IStorage iStorage) {
        myIStorage= iStorage;
        myId= id;
		var parent= Parent;
		if(parent != null) parent.AddChild(this);
    }
    // ----------------------------------------------------------------------
	public static void RebuildFromEngineObjects(iCS_IStorage iStorage) {
		iStorage.EditorObjects.Clear();
		iStorage.EditorObjects.Capacity= iStorage.EngineObjects.Count;		
		for(int i= 0; i < iStorage.EngineObjects.Count; ++i) {
            iCS_EditorObject editorObj= null;
            var engineObj= iStorage.EngineObjects[i];
		    if(iCS_VisualScriptData.IsValid(engineObj, iStorage.EngineStorage)) {
		        editorObj= new iCS_EditorObject(i, iStorage);
		    }
	        iStorage.EditorObjects.Add(editorObj);
		}
		RebuildChildrenLists(iStorage);
	}
    // ----------------------------------------------------------------------
	private static void RebuildChildrenLists(iCS_IStorage iStorage) {
		iStorage.ForEach(
		    obj=> {
    			if(obj.IsParentValid) {
    				obj.Parent.AddChild(obj);
    			}					        
		    }
		);
	}

    // ======================================================================
    // Child container management.
    // ----------------------------------------------------------------------
	public List<int> Children { get { return myChildren; }}
    // ----------------------------------------------------------------------
    public void AddChild(iCS_EditorObject toAdd) {
        int id= toAdd.InstanceId;
        if(Prelude.elem(id, myChildren.ToArray())) return;
        myChildren.Add(id);
    }
    // ----------------------------------------------------------------------
    public void RemoveChild(iCS_EditorObject toDelete) {
        int id= toDelete.InstanceId;
        for(int i= 0; i < myChildren.Count; ++i) {
            if(myChildren[i] == id) {
                myChildren.RemoveAt(i);
                return;
            }
        }
    }
    // ----------------------------------------------------------------------
    public bool AreChildrenInSameOrder(int[] orderedChildren) {
        int i= 0;
        for(int j= 0; j < Children.Count; ++j) {
            if(Children[j] == orderedChildren[i]) {
                if(++i >= orderedChildren.Length) return true;
            };
        }
        return false;
    }
    // ----------------------------------------------------------------------
    public void ReorderChildren(int[] orderedChildren) {
        if(AreChildrenInSameOrder(orderedChildren)) return;
        List<int> others= Prelude.filter(c=> Prelude.notElem(c,orderedChildren), Children);
        int i= 0;
        Prelude.forEach(c=> Children[i++]= c, orderedChildren);
        Prelude.forEach(c=> Children[i++]= c, others);
    }

}
