using UnityEngine;
using UnityEditor;
using System;
//using System.Threading;
using System.Collections;
using System.Collections.Generic;
using P= Prelude;

public partial class iCS_IStorage {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
            bool                myIsDirty           = true;
            bool                CleanupNeeded       = true;
            bool                AnimationNeeded     = true;
    public  bool                AnimateLayout       = true;
    public  iCS_Storage         Storage             = null;
    List<iCS_EditorObject>      myEditorObjects     = null;
    public  int                 UndoRedoId          = 0;
    public  int                 ModificationId      = -1;
    public  bool                CleanupDeadPorts    = true;
			P.TimeRatio			myAnimationTimeRatio= new P.TimeRatio();
    
    // ======================================================================
    // Initialization
    // ----------------------------------------------------------------------
    public iCS_IStorage(iCS_Storage storage) {
        Init(storage);
    }
    public void Init(iCS_Storage storage) {
        if(Storage != storage) {
            IsDirty= true;
            Storage= storage;
            UndoRedoId= Storage.UndoRedoId;          
            GenerateEditorData();
        }
    }
    // ----------------------------------------------------------------------
    void GenerateEditorData() {
		// Rebuild Editor Objects from the Engine Objects.
		myEditorObjects= new List<iCS_EditorObject>();
		iCS_EditorObject.RebuildFromEngineObjects(this);
		
        // Re-initialize internal values.
        if(EditorObjects.Count > 0 && IsValid(EditorObjects[0])) {
            Vector2 graphCenter= Math3D.Middle(GetLayoutPosition(EditorObjects[0]));
            ForEach(obj=> {
		        // Initialize display position.
                obj.AnimatedPosition.Reset(new Rect(graphCenter.x,graphCenter.y,0,0));
				// Initialize initial port values.
				if(obj.IsInDataPort) {
					LoadInitialPortValueFromArchive(obj);
				}
            });            
        }
        CleanupUnityObjects();
    }
    
    
    // ======================================================================
    // Basic Accessors
    // ----------------------------------------------------------------------
    public List<iCS_EditorObject>    EditorObjects    { get { return myEditorObjects; }}
    public List<iCS_EngineObject>    EngineObjects    { get { return Storage.EngineObjects; }}
	public Vector2					 ScrollPosition   { get { return Storage.ScrollPosition; } set { Storage.ScrollPosition= value; }}
    public float                     GuiScale         { get { return Storage.GuiScale; } set { Storage.GuiScale= value; }}
    public int                       SelectedObjectId { get { return Storage.SelectedObject; } set { Storage.SelectedObject= value; }}
    public iCS_EditorObject          SelectedObject   { get { return this[SelectedObjectId]; } set { SelectedObjectId= value != null ? value.InstanceId : -1; }}
    // ----------------------------------------------------------------------
    public bool IsBehaviour         { get { return IsValid(EditorObjects[0]) && EditorObjects[0].IsBehaviour; }}
    public bool IsEmptyBehaviour    {
        get {
            if(!IsBehaviour) return false;
            for(int i= 1; i < EditorObjects.Count; ++i) {
                if(IsValid(EditorObjects[i])) return false;
            }
            return true;
        }
    }
    public bool IsLibrary           { get { return IsValid(EditorObjects[0]) && !EditorObjects[0].IsBehaviour; }}
    // ----------------------------------------------------------------------
    public bool IsIdValid(int id)                    { return id >= 0 && id < EditorObjects.Count; }
	public bool IsValid(int id)						 { return IsIdValid(id) && EditorObjects[id] != null && EditorObjects[id].InstanceId == id; }
    public bool IsValid(iCS_EditorObject obj)        { return obj != null && obj.InstanceId != -1; }
    public bool IsSourceValid(iCS_EditorObject obj)  { return obj.SourceId != -1; }
    public bool IsParentValid(iCS_EditorObject obj)  { return obj.ParentId != -1; }
    // ----------------------------------------------------------------------
    public bool IsDirty            { get { DetectUndoRedo(); return myIsDirty; } set { myIsDirty= value; }}
//	public int  ModificationId     { get { return UndoRedoId; }}
	public bool IsAnimationPlaying { get { return myAnimationTimeRatio.IsActive; }}
    // ----------------------------------------------------------------------
	public iCS_EditorObject GetOutMuxPort(iCS_EditorObject eObj) { return eObj.IsOutMuxPort ? eObj : (eObj.IsInMuxPort ? eObj.Parent : null); }
    // ----------------------------------------------------------------------
    public iCS_EditorObject this[int id] {
        get {
            if(!IsIdValid(id)) return null;
            return EditorObjects[id];
        }
        set {
            DetectUndoRedo();
            EditorObjects[id]= value;
            SetDirty(EditorObjects[id]);
            iCS_EditorObject parent= EditorObjects[id].Parent;
            if(parent != null) SetDirty(parent);
        }
    }
    // ----------------------------------------------------------------------
	public iCS_EditorObject      GetParentNode(iCS_EditorObject obj)		{ var parent= obj.Parent; while(parent != null && !parent.IsNode) parent= parent.Parent; return parent; }
    public Rect            GetDisplayPosition(iCS_EditorObject obj)           { return IsValid(obj) ? obj.AnimatedPosition.CurrentValue : default(Rect); }
    public void            SetDisplayPosition(iCS_EditorObject obj, Rect pos) { if(IsValid(obj)) obj.AnimatedPosition.Reset(pos); }
	public P.TimeRatio	AnimationTimeRatio { get { return myAnimationTimeRatio; }}
    // ----------------------------------------------------------------------
    public object          GetRuntimeObject(iCS_EditorObject obj) {
        iCS_Behaviour bh= Storage as iCS_Behaviour;
        return obj == null || bh == null ? null : bh.GetRuntimeObject(obj.InstanceId);
    }
    // ----------------------------------------------------------------------
    public void SetDirty(iCS_EditorObject obj) {
        myIsDirty= true;
        obj.IsDirty= true;        
        var parent= obj.Parent;
        if(parent != null) { parent.IsDirty= true; }
        ++ModificationId;
    }
    // ----------------------------------------------------------------------
    public void SetParent(iCS_EditorObject edObj, iCS_EditorObject newParent) {
        Rect pos= GetLayoutPosition(edObj);
        iCS_EditorObject oldParent= edObj.Parent;
        edObj.Parent= newParent;
        SetLayoutPosition(edObj, pos);
        SetDirty(edObj);
        SetDirty(oldParent);
        SetDirty(newParent);
    }
    
    // ======================================================================
    // Storage Update
    // ----------------------------------------------------------------------
    public void Update() {
        // Processing any changed caused by Undo/Redo
        DetectUndoRedo();
/*
	TODO: Optimize update.
*/        
        // Update display if animation is disabled.
        if(!AnimateLayout || (!myIsDirty && !AnimationNeeded && !myAnimationTimeRatio.IsActive)) {
            ForEach(
                obj=> {
                    var animation= obj.AnimatedPosition;
                    animation.Reset(GetAnimationTarget(obj));                    
                }
            );
        }
        
        // Perform layout if one or more objects has changed.
        if(myIsDirty) {
            // Tell Unity that our storage has changed.
            EditorUtility.SetDirty(Storage);
            // Prepare for cleanup after storage change.
            CleanupNeeded= true;
            AnimationNeeded= true;
            myIsDirty= false;

            // Perform layout of modified nodes.
            ForEachRecursiveDepthLast(EditorObjects[0],
                obj=> {
                    if(obj.IsDirty) {
//                        Debug.Log(obj.Name+" is dirty");
                        Layout(obj);
                    }
                }
            );
            return;
        }

        // Graph is now stable.  Recompute animation target if needed.
        if(AnimationNeeded && AnimateLayout) {
            ForEach(
                obj=> {
                    Rect target= GetAnimationTarget(obj);
                    var animation= obj.AnimatedPosition;
                    animation.Start(animation.CurrentValue,
                                    target,
                                    myAnimationTimeRatio,
                                    (start,end,ratio)=>Math3D.Lerp(start,end,ratio));
                }
            );
            myAnimationTimeRatio.Start(iCS_PreferencesEditor.AnimationTime);
            AnimationNeeded= false;
        }

        // Animate position.
        if(myAnimationTimeRatio.IsActive) {
            if(myAnimationTimeRatio.IsElapsed) {
                myAnimationTimeRatio.Reset();
            }
            ForEach(
                obj=> {
                    var animation= obj.AnimatedPosition;
                    if(myAnimationTimeRatio.IsActive) {
                        animation.Update();                        
                    } else {
                        animation.Reset(animation.TargetValue);
                    }
                }
            );
        }

        // Perform graph cleanup once objects & layout are stable.
        if(CleanupNeeded) {
            CleanupNeeded= Cleanup();
        }
        
        // Perform sanity check
        SanityCheck();
    }
    // ----------------------------------------------------------------------
    Rect GetAnimationTarget(iCS_EditorObject eObj) {
        Rect target;
        if(eObj.IsVisible) {
            target= GetLayoutPosition(eObj);
        } else {
            // Find first visible parent.
            var visibleParent= eObj.Parent;
            for(; visibleParent != null && !visibleParent.IsVisible; visibleParent= visibleParent.Parent);
            Vector2 center= Math3D.Middle(GetLayoutPosition(visibleParent ?? eObj));
            target= new Rect(center.x, center.y, 0, 0);
        }
        return target;
    }
    // ----------------------------------------------------------------------
    public bool Cleanup() {
        bool modified= false;
        ForEach(
            obj=> {
                // Cleanup disconnected dynamic state or module ports.
				var parent= obj.Parent;
                if(CleanupDeadPorts) {
					bool shouldRemove= false;
					if(obj.IsOutMuxPort) {
						int nbOfChildren= NbOfChildren(obj, c=> c.IsInDataPort);
						if(nbOfChildren == 1) {
							iCS_EditorObject child= GetChildInputDataPorts(obj)[0];
							obj.SourceId= child.SourceId;
							obj.ObjectType= iCS_ObjectTypeEnum.OutDynamicModulePort;
							DestroyInstanceInternal(child);
						} else {
							shouldRemove= nbOfChildren == 0 && IsPortDisconnected(obj);							
						}
					} else if(obj.IsInMuxPort) {
						shouldRemove= obj.Source == null;
					} else {
						shouldRemove= ((obj.IsStatePort || obj.IsDynamicModulePort) && IsPortDisconnected(obj)) ||
						              (obj.IsDynamicModulePort && obj.Source == null && (parent.IsStateChart || parent.IsState));
						
					}
					if(shouldRemove) {
                        DestroyInstanceInternal(obj);                            
                        modified= true;						
					}
                    // Cleanup disconnected typecasts.
    				if(obj.IsTypeCast) {
						var inDataPort= FindInChildren(obj, c=> c.IsInDataPort);
                        if(inDataPort.Source == null &&
                           FindAConnectedPort(FindInChildren(obj, c=> c.IsOutDataPort)) == null) {
                           DestroyInstanceInternal(obj);
                           modified= true;
                        }
                    }                    
				}
            }
        );        
        return modified;
    }
    // ----------------------------------------------------------------------
    public void CleanupUnityObjects() {
        Storage.ClearUnityObjects();
        ForEach(
            obj=> {
                if(obj.IsInDataPort && obj.SourceId == -1 && obj.InitialValue != null) {
                    StoreInitialPortValueInArchive(obj);
                }
                else {
                    obj.InitialValueArchive= null; 
                }
            }
        );
    }
    
    // ======================================================================
    // Editor Object Creation/Destruction
    // ----------------------------------------------------------------------
    int GetNextAvailableId() {
        // Covers Undo?redo for all creation operation
        DetectUndoRedo();
        // Find the next available id.
        int id= 0;
        int len= EditorObjects.Count;
        while(id < len && IsValid(EditorObjects[id])) { ++id; }
        return id;
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject Copy(iCS_EditorObject srcObj, iCS_IStorage srcStorage,
                                 iCS_EditorObject destParent, iCS_IStorage destStorage, Vector2 initialPos) {
        // Create new EditorObject
        Vector2 parentPos= IsValid(destParent) ? Math3D.ToVector2(GetLayoutPosition(destParent)) : Vector2.zero;
        Vector2 sizeOffset= 0.5f*new Vector2(srcObj.LocalPosition.width, srcObj.LocalPosition.height);
        Vector2 localPos= initialPos-parentPos-sizeOffset;
        List<Prelude.Tuple<int, int>> xlat= new List<Prelude.Tuple<int, int>>();
        iCS_EditorObject instance= Copy(srcObj, srcStorage, destParent, destStorage, localPos, xlat);
        ReconnectCopy(srcObj, srcStorage, destStorage, xlat);
        SetDisplayPosition(instance, new Rect(initialPos.x, initialPos.y,0,0));
        return instance;
    }
    iCS_EditorObject Copy(iCS_EditorObject srcObj, iCS_IStorage srcStorage,
                          iCS_EditorObject destParent, iCS_IStorage destStorage, Vector2 localPos, List<Prelude.Tuple<int,int>> xlat) {
        // Create new EditorObject
        int id= destStorage.GetNextAvailableId();
        xlat.Add(new Prelude.Tuple<int,int>(srcObj.InstanceId, id));
        var newObj= destStorage[id]= iCS_EditorObject.Clone(id, srcObj, destParent, localPos, destStorage);
        newObj.IconGUID= srcObj.IconGUID;
        srcObj.ForEachChild(
            child=> Copy(child, srcStorage, newObj, destStorage, Math3D.ToVector2(child.LocalPosition), xlat)
        );
		if(newObj.IsInDataPort) {
			LoadInitialPortValueFromArchive(this[id]);
		}
		destStorage.SetDirty(newObj);
        return newObj;
    }
    void ReconnectCopy(iCS_EditorObject srcObj, iCS_IStorage srcStorage, iCS_IStorage destStorage, List<Prelude.Tuple<int,int>> xlat) {
        srcStorage.ForEachRecursive(srcObj,
            child=> {
                if(child.SourceId != -1) {
                    int id= -1;
                    int sourceId= -1;
                    foreach(var pair in xlat) {
                        if(pair.Item1 == child.InstanceId) {
                            id= pair.Item2;
                            if(sourceId != -1) break;
                        }
                        if(pair.Item1 == child.SourceId) {
                            sourceId= pair.Item2;
                            if(id != -1) break;
                        }
                    }
                    if(sourceId != -1) {
                        destStorage.SetSource(destStorage.EditorObjects[id], destStorage.EditorObjects[sourceId]);                        
                    }
                }
            }
        );
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject CreateBehaviour() {
        // Create the function node.
        int id= GetNextAvailableId();
        // Validate that behaviour is at the root.
        if(id != 0) {
            Debug.LogError("Behaviour MUST be the root object !!!");
        }
        // Determine best initial position.
        Rect initialPos= VisualEditorCenter();
        // Create new EditorObject
        iCS_EditorObject.CreateInstance(id, null, typeof(iCS_Behaviour), -1, iCS_ObjectTypeEnum.Behaviour, initialPos, this);
		this[id].IsNameEditable= false;
		SetDirty(this[id]);
        return this[id];
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject CreateModule(int parentId, Vector2 initialPos, string name= "", iCS_ObjectTypeEnum objectType= iCS_ObjectTypeEnum.Module, Type runtimeType= null) {
		if(runtimeType == null) runtimeType= typeof(iCS_Module);
        // Create the function node.
        int id= GetNextAvailableId();
        // Calcute the desired screen position of the new object.
        Rect localPos= PositionNewNodeInParent(initialPos, parentId);
        // Create new EditorObject
        iCS_EditorObject.CreateInstance(id, name, runtimeType, parentId, objectType, localPos, this);
        var center= Math3D.Middle(GetLayoutPosition(this[id]));
        SetDisplayPosition(this[id], new Rect(center.x,center.y,0,0));
	    this[id].IconGUID= iCS_TextureCache.IconPathToGUID(iCS_EditorStrings.ModuleIcon);			
        if(this[id].IsClassModule) ClassModuleCompleteCreation(this[id]);
		SetDirty(this[id]);
        return this[id];
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject CreateStateChart(int parentId, Vector2 initialPos, string name= "") {
        // Create the function node.
        int id= GetNextAvailableId();
        // Calcute the desired screen position of the new object.
        Rect localPos= PositionNewNodeInParent(initialPos, parentId);
        // Create new EditorObject
        this[id]= iCS_EditorObject.CreateInstance(id, name, typeof(iCS_StateChart), parentId, iCS_ObjectTypeEnum.StateChart, localPos, this);
        var center= Math3D.Middle(GetLayoutPosition(this[id]));
        SetDisplayPosition(this[id], new Rect(center.x,center.y,0,0));
		SetDirty(this[id]);
        // Automatically create entry state.
        CreateState(this[id].InstanceId, Vector2.zero, "EntryState");
        return this[id];
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject CreateState(int parentId, Vector2 initialPos, string name= "") {
        // Validate that we have a good parent.
        iCS_EditorObject parent= EditorObjects[parentId];
        if(parent == null || (!parent.IsStateChart && !parent.IsState)) {
            Debug.LogError("State must be created as a child of StateChart or State.");
        }
        // Create the function node.
        int id= GetNextAvailableId();
        // Calcute the desired screen position of the new object.
        Rect localPos= PositionNewNodeInParent(initialPos, parentId);
        // Create new EditorObject
        this[id]= iCS_EditorObject.CreateInstance(id, name, typeof(iCS_State), parentId, iCS_ObjectTypeEnum.State, localPos, this);
        SetDisplayPosition(this[id], new Rect(initialPos.x,initialPos.y,0,0));
        // Set first state as the default entry state.
        this[id].IsRawEntryState= !ForEachChild(parent,
            child=> {
                if(child.IsEntryState) {
                    return true;
                }
                return false;
            }
        );
        SetDirty(this[id]);
        return this[id];
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject CreateMethod(int parentId, Vector2 initialPos, iCS_ReflectionInfo desc) {
        iCS_EditorObject instance= desc.ObjectType == iCS_ObjectTypeEnum.InstanceMethod || desc.ObjectType == iCS_ObjectTypeEnum.InstanceField ?
                    				CreateInstanceMethod(parentId, initialPos, desc) : 
                    				CreateStaticMethod(parentId, initialPos, desc);

		instance.MethodName= desc.MethodName;
		instance.NbOfParams= desc.ParamTypes != null ? desc.ParamTypes.Length : 0;
		return instance;
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject CreateStaticMethod(int parentId, Vector2 initialPos, iCS_ReflectionInfo desc) {
        // Create the conversion node.
        int id= GetNextAvailableId();
        // Determine minimized icon.
        var iconGUID= iCS_TextureCache.IconPathToGUID(desc.IconPath);
        if(iconGUID == null && desc.ObjectType == iCS_ObjectTypeEnum.StaticMethod) {
            iconGUID= iCS_TextureCache.IconPathToGUID(iCS_EditorStrings.MethodIcon);
        }        
        // Calcute the desired screen position of the new object.
        Rect localPos= PositionNewNodeInParent(initialPos, parentId, iconGUID);
        // Create new EditorObject
        this[id]= iCS_EditorObject.CreateInstance(id, desc.DisplayName, desc.ClassType, parentId, desc.ObjectType, localPos, this);
        this[id].IconGUID= iconGUID;
        // Create parameter ports.
		int portIdx= 0;
		iCS_EditorObject port= null;
        for(; portIdx < desc.ParamNames.Length; ++portIdx) {
            if(desc.ParamTypes[portIdx] != typeof(void)) {
                iCS_ObjectTypeEnum portType= desc.ParamDirs[portIdx] == iCS_ParamDirectionEnum.Out ? iCS_ObjectTypeEnum.OutFunctionPort : iCS_ObjectTypeEnum.InFunctionPort;
                port= CreatePort(desc.ParamNames[portIdx], id, desc.ParamTypes[portIdx], portType);
                port.PortIndex= portIdx;
				object initialPortValue= desc.ParamInitialValues[portIdx];
				if(initialPortValue == null) {
					initialPortValue= iCS_Types.DefaultValue(desc.ParamTypes[portIdx]);
				}
                port.InitialPortValue= initialPortValue;
            }
        }
		// Create return port.
		if(desc.ReturnType != null && desc.ReturnType != typeof(void)) {
            port= CreatePort(desc.ReturnName, id, desc.ReturnType, iCS_ObjectTypeEnum.OutFunctionPort);
            port.PortIndex= portIdx;			
		}
        
        SetDisplayPosition(this[id], new Rect(initialPos.x,initialPos.y,0,0));
		SetDirty(this[id]);
        return this[id];
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject CreateInstanceMethod(int parentId, Vector2 initialPos, iCS_ReflectionInfo desc) {
        // Create the conversion node.
        int id= GetNextAvailableId();
        // Determine minimized icon.
        var iconGUID= iCS_TextureCache.IconPathToGUID(desc.IconPath);
        if(iconGUID == null && desc.ObjectType == iCS_ObjectTypeEnum.StaticMethod) {
            iconGUID= iCS_TextureCache.IconPathToGUID(iCS_EditorStrings.MethodIcon);
        }        
        // Calcute the desired screen position of the new object.
        Rect localPos= PositionNewNodeInParent(initialPos, parentId, iconGUID);
        // Create new EditorObject
        iCS_EditorObject.CreateInstance(id, desc.DisplayName, desc.ClassType, parentId, desc.ObjectType, localPos, this);
        this[id].IconGUID= iconGUID;
        // Create parameter ports.
		int portIdx= 0;
		iCS_EditorObject port= null;
        for(; portIdx < desc.ParamNames.Length; ++portIdx) {
            if(desc.ParamTypes[portIdx] != typeof(void)) {
                iCS_ObjectTypeEnum portType= desc.ParamDirs[portIdx] == iCS_ParamDirectionEnum.Out ? iCS_ObjectTypeEnum.OutFunctionPort : iCS_ObjectTypeEnum.InFunctionPort;
                port= CreatePort(desc.ParamNames[portIdx], id, desc.ParamTypes[portIdx], portType);
                port.PortIndex= portIdx;                
				object initialPortValue= desc.ParamInitialValues[portIdx];
				if(initialPortValue == null) {
					initialPortValue= iCS_Types.DefaultValue(desc.ParamTypes[portIdx]);
				}
                port.InitialPortValue= initialPortValue;
            }
        }
		// Create return port.
		if(desc.ReturnType != null && desc.ReturnType != typeof(void)) {
            port= CreatePort(desc.ReturnName, id, desc.ReturnType, iCS_ObjectTypeEnum.OutFunctionPort);
            port.PortIndex= portIdx++;			
		} else {
		    ++portIdx;
		}
		// Create 'this' ports.
        port= CreatePort("this", id, desc.ClassType, iCS_ObjectTypeEnum.InFunctionPort);
        port.PortIndex= portIdx++;			
        port= CreatePort("this", id, desc.ClassType, iCS_ObjectTypeEnum.OutFunctionPort);
        port.PortIndex= portIdx;			

        SetDisplayPosition(this[id], new Rect(initialPos.x,initialPos.y,0,0));
		SetDirty(this[id]);
        return this[id];
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject CreatePort(string name, int parentId, Type valueType, iCS_ObjectTypeEnum portType) {
        int id= GetNextAvailableId();
        iCS_EditorObject port= iCS_EditorObject.CreateInstance(id, name, valueType, parentId, portType, new Rect(0,0,0,0), this);
        // Reajust data port position 
        iCS_EditorObject parent= port.Parent;
		if(port.IsDataPort && parent.IsDataPort) {
			port.LocalPosition= new Rect(0,0,0,0);
		} else if(port.IsDataPort && !port.IsEnablePort) {
            if(port.IsInputPort) {
                int nbOfPorts= GetNbOfLeftPorts(parent);
                port.LocalPosition= new Rect(0, parent.LocalPosition.height/(nbOfPorts+1), 0, 0);
            } else {
                int nbOfPorts= GetNbOfRightPorts(parent);
                port.LocalPosition= new Rect(parent.LocalPosition.width, parent.LocalPosition.height/(nbOfPorts+1), 0, 0);                
            }
        }
        if(port.IsModulePort || port.IsInMuxPort) 	{ AddDynamicPort(port); }
        Rect parentPos= GetLayoutPosition(port.Parent);
        SetDisplayPosition(this[id], new Rect(0.5f*(parentPos.x+parentPos.xMax), 0.5f*(parentPos.y+parentPos.yMax),0,0));
		SetDirty(this[id]);
        return EditorObjects[id];        
    }
    // ----------------------------------------------------------------------
    Rect PositionNewNodeInParent(Vector2 initialPos, int parentId, string iconGUID= null) {
        Rect localPos;
        Texture2D icon= iCS_TextureCache.GetTextureFromGUID(iconGUID);
        var size= icon != null ? new Vector2(icon.width, icon.height) : iCS_Graphics.GetMaximizeIconSize(null);            
        if(IsValid(parentId)) {
            iCS_EditorObject parent= EditorObjects[parentId];
            var parentRect= GetLayoutPosition(parent);
            if(parentRect.Contains(initialPos)) {
                localPos= new Rect(initialPos.x-parentRect.x, initialPos.y-parentRect.y,size.x,size.y);
            } else {
                localPos= new Rect(parentRect.width, parentRect.height, size.x, size.y);
                parent.LocalPosition= new Rect(parent.LocalPosition.x, parent.LocalPosition.y,
                                               parent.LocalPosition.width +2f*iCS_Config.GutterSize+size.x,
                                               parent.LocalPosition.height+2f*iCS_Config.GutterSize+size.y);
            }
        } else {
            localPos= VisualEditorCenter();
        }        
        return localPos;
    }
    // ----------------------------------------------------------------------
    Rect VisualEditorCenter() {
        Rect initialPos= new Rect(0,0,16,16);
        iCS_VisualEditor editor= iCS_EditorMgr.FindVisualEditor();
        if(editor != null) {
            Vector2 center= editor.ViewportToGraph(editor.ViewportCenter);
            initialPos.x= center.x-8f;
            initialPos.y= center.y-8f;
        }
        return initialPos;        
    }
    
    // ======================================================================
    // Port Connectivity
    // ----------------------------------------------------------------------
    public void SetSource(iCS_EditorObject obj, iCS_EditorObject src) {
        int id= src == null ? -1 : src.InstanceId;
        if(id != obj.SourceId) {
            obj.SourceId= id; 
            SetDirty(obj);            
        }
    }
    // ----------------------------------------------------------------------
    public void SetSource(iCS_EditorObject inPort, iCS_EditorObject outPort, iCS_ReflectionInfo convDesc) {
        if(convDesc == null) { SetSource(inPort, outPort); return; }
        Rect inPos= GetLayoutPosition(inPort);
        Rect outPos= GetLayoutPosition(outPort);
        Vector2 convPos= new Vector2(0.5f*(inPos.x+outPos.x), 0.5f*(inPos.y+outPos.y));
        int grandParentId= inPort.ParentId;
        iCS_EditorObject conv= CreateMethod(grandParentId, convPos, convDesc);
        ForEachChild(conv,
            (child) => {
                if(child.IsInputPort) {
                    SetSource(child, outPort);
                } else if(child.IsOutputPort) {
                    SetSource(inPort, child);
                }
            }
        );
        Iconize(conv);
    }
    // ----------------------------------------------------------------------
    public void DisconnectPort(iCS_EditorObject port) {
        SetSource(port, null);
        Prelude.forEach(p=> SetSource(p, null), FindConnectedPorts(port));        
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject[] FindConnectedPorts(iCS_EditorObject port) {
        return Filter(p=> p.IsPort && p.SourceId == port.InstanceId).ToArray();
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject FindAConnectedPort(iCS_EditorObject port) {
        iCS_EditorObject[] connectedPorts= FindConnectedPorts(port);
        return connectedPorts.Length != 0 ? connectedPorts[0] : null;
    }
    // ----------------------------------------------------------------------
    bool IsPortConnected(iCS_EditorObject port) {
        if(port.IsSourceValid) return true;
        if(FindFirst(o=> o.IsPort && o.SourceId == port.InstanceId) != null) return true;
        return false;
    }
    bool IsPortDisconnected(iCS_EditorObject port) { return !IsPortConnected(port); }
    // ----------------------------------------------------------------------
    // Returns the last data port in the connection or NULL if none exist.
    public iCS_EditorObject GetDataConnectionSource(iCS_EditorObject port) {
        iCS_EngineObject engineObject= Storage.GetDataConnectionSource(port.EngineObject);
        return engineObject != null ? EditorObjects[engineObject.InstanceId] : null;
    }
    
}
