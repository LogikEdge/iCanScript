using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class WD_IStorage {
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    bool            IsDirty  = true;
    WD_Storage      Storage  = null;
    WD_TreeCache    TreeCache= null;
    
    // ======================================================================
    // Initialization
    // ----------------------------------------------------------------------
    public WD_IStorage(WD_Storage storage) {
        Init(storage);
    }
    public void Init(WD_Storage storage) {
        if(Storage != storage) {
            IsDirty= true;
            Storage= storage;
            GenerateEditorData();            
        }
    }
    public void Reset() {
        IsDirty= true;
        Storage= null;
        TreeCache= null;
    }
    // ----------------------------------------------------------------------
    void GenerateEditorData() {
        TreeCache= new WD_TreeCache();
        foreach(var obj in EditorObjects) {
            TreeCache.CreateInstance(obj);
        }
    }
    
    
    // ======================================================================
    // Basic Accessors
    // ----------------------------------------------------------------------
    public List<WD_EditorObject> EditorObjects { get { return Storage.EditorObjects; }}
    public WD_UserPreferences    Preferences   { get { return Storage.Preferences; }}
    // ----------------------------------------------------------------------
    public bool IsValid(int id)     { return id >= 0 && id < EditorObjects.Count && this[id].InstanceId != -1; }
    public bool IsInvalid(int id)   { return !IsValid(id); }
    // ----------------------------------------------------------------------
    public WD_EditorObject this[int id] {
        get { return EditorObjects[id]; }
        set {
            if(value.InstanceId != id) Debug.LogError("Trying to add EditorObject at wrong index.");
            EditorObjects[id]= value;
            if(TreeCache.IsValid(id)) TreeCache.UpdateInstance(value);
            else                      TreeCache.CreateInstance(value);            
        }
    }
    // ----------------------------------------------------------------------
    public WD_EditorObject GetParent(WD_EditorObject obj) { return IsValid(obj.ParentId) ? EditorObjects[obj.ParentId] : null; }
    public WD_EditorObject GetSource(WD_EditorObject obj) { return IsValid(obj.Source) ? EditorObjects[obj.Source] : null; }


    // ======================================================================
    // Storage Update
    // ----------------------------------------------------------------------
    public void Update() {
        if(IsDirty) {
            IsDirty= false;
            Undo.RegisterUndo(Storage, "WarpDrive");
            EditorUtility.SetDirty(Storage);
        }
    }

    // ======================================================================
    // Editor Object Creation/Destruction
    // ----------------------------------------------------------------------
    public int GetNextAvailableId() {
        // Find the next available id.
        int id= 0;
        int len= EditorObjects.Count;
        while(id < len && IsValid(id)) { ++id; }
        if(id >= len) {
            id= len;
            EditorObjects.Add(null);
        }
        return id;
    }
    // ----------------------------------------------------------------------
    public WD_EditorObject CreateInstance(string name, int parentId, WD_ObjectTypeEnum objType, Vector2 initialPos, Type rtType) {
        // Create the function node.
        int id= GetNextAvailableId();
        // Calcute the desired screen position of the new object.
        Rect parentPos= IsValid(parentId) ? GetPosition(parentId) : new Rect(0,0,0,0);
        Rect localPos= new Rect(initialPos.x-parentPos.x, initialPos.y-parentPos.y,0,0);
        // Create new EditorObject
        EditorObjects[id]= new WD_EditorObject(id, name, rtType, parentId, objType, localPos);
        TreeCache.CreateInstance(EditorObjects[id]);
        return EditorObjects[id];
    }
    // ----------------------------------------------------------------------
    public WD_EditorObject CreateBehaviour() {
        // Create the function node.
        int id= GetNextAvailableId();
        // Validate that behaviour is at the root.
        if(id != 0) {
            Debug.LogError("Behaviour MUST be the root object !!!");
        }
        // Create new EditorObject
        EditorObjects[id]= new WD_EditorObject(id, "Behaviour", typeof(WD_Behaviour), -1, WD_ObjectTypeEnum.Behaviour, new Rect(0,0,0,0));
        TreeCache.CreateInstance(EditorObjects[id]);
        return EditorObjects[id];
    }
    // ----------------------------------------------------------------------
    public WD_EditorObject CreateModuleLibrary() {
        // Validate that a library can only be create at the root.
        if(EditorObjects.Count != 0) {
            Debug.LogError("Module Library MUST be the root object !!!");
        }
        return CreateModule(-1, Vector2.zero, "Module Library");
    }
    // ----------------------------------------------------------------------
    public WD_EditorObject CreateModule(int parentId, Vector2 initialPos, string name= "") {
        // Create the function node.
        int id= GetNextAvailableId();
        // Calcute the desired screen position of the new object.
        Rect parentPos= IsValid(parentId) ? GetPosition(parentId) : new Rect(0,0,0,0);
        Rect localPos= new Rect(initialPos.x-parentPos.x, initialPos.y-parentPos.y,0,0);
        // Create new EditorObject
        EditorObjects[id]= new WD_EditorObject(id, name, typeof(WD_Module), parentId, WD_ObjectTypeEnum.Module, localPos);
        TreeCache.CreateInstance(EditorObjects[id]);
        return EditorObjects[id];
    }
    // ----------------------------------------------------------------------
    public WD_EditorObject CreateStateChartLibrary() {
        // Validate that a library can only be create at the root.
        if(EditorObjects.Count != 0) {
            Debug.LogError("Module Library MUST be the root object !!!");
        }
        return CreateStateChart(-1, Vector2.zero, "StateChart Library");
    }
    // ----------------------------------------------------------------------
    public WD_EditorObject CreateStateChart(int parentId, Vector2 initialPos, string name= "") {
        // Create the function node.
        int id= GetNextAvailableId();
        // Calcute the desired screen position of the new object.
        Rect parentPos= IsValid(parentId) ? GetPosition(parentId) : new Rect(0,0,0,0);
        Rect localPos= new Rect(initialPos.x-parentPos.x, initialPos.y-parentPos.y,0,0);
        // Create new EditorObject
        EditorObjects[id]= new WD_EditorObject(id, name, typeof(WD_StateChart), parentId, WD_ObjectTypeEnum.StateChart, localPos);
        TreeCache.CreateInstance(EditorObjects[id]);
        return EditorObjects[id];
    }
    // ----------------------------------------------------------------------
    public WD_EditorObject CreateState(int parentId, Vector2 initialPos, string name= "") {
        // Validate that we have a good parent.
        WD_EditorObject parent= EditorObjects[parentId];
        if(parent == null || (!WD.IsStateChart(parent) && !WD.IsState(parent))) {
            Debug.LogError("State must be created as a child of StateChart or State.");
        }
        // Create the function node.
        int id= GetNextAvailableId();
        // Calcute the desired screen position of the new object.
        Rect parentPos= GetPosition(parentId);
        Rect localPos= new Rect(initialPos.x-parentPos.x, initialPos.y-parentPos.y,0,0);
        // Create new EditorObject
        EditorObjects[id]= new WD_EditorObject(id, name, typeof(WD_State), parentId, WD_ObjectTypeEnum.State, localPos);
        TreeCache.CreateInstance(EditorObjects[id]);
        return EditorObjects[id];
    }
    // ----------------------------------------------------------------------
    public WD_EditorObject CreateFunction(int parentId, Vector2 initialPos, WD_BaseDesc desc) {
        if(desc is WD_ClassDesc) {
            return CreateFunction(parentId, initialPos, desc as WD_ClassDesc);
        }
        else if(desc is WD_FunctionDesc) {
            return CreateFunction(parentId, initialPos, desc as WD_FunctionDesc);
        }
        else if(desc is WD_ConversionDesc) {
            return CreateFunction(parentId, initialPos, desc as WD_ConversionDesc);
        }
        return null;
    }
    // ----------------------------------------------------------------------
    public WD_EditorObject CreateFunction(int parentId, Vector2 initialPos, WD_ClassDesc desc) {
        // Create the class node.
        int id= GetNextAvailableId();
        // Calcute the desired screen position of the new object.
        Rect parentPos= GetPosition(parentId);
        Rect localPos= new Rect(initialPos.x-parentPos.x, initialPos.y-parentPos.y,0,0);
        EditorObjects[id]= new WD_EditorObject(id, desc.Name, desc.ClassType, parentId, WD_ObjectTypeEnum.Class, localPos, desc.Icon);
        TreeCache.CreateInstance(EditorObjects[id]);
        // Create field ports
        for(int i= 0; i < desc.FieldNames.Length; ++i) {
            WD_ObjectTypeEnum portType= desc.FieldInOuts[i] ? WD_ObjectTypeEnum.OutFieldPort : WD_ObjectTypeEnum.InFieldPort;
            CreatePort(desc.FieldNames[i], id, desc.FieldTypes[i], portType);
        }
        // Create property ports
        for(int i= 0; i < desc.PropertyNames.Length; ++i) {
            WD_ObjectTypeEnum portType= desc.PropertyInOuts[i] ? WD_ObjectTypeEnum.OutPropertyPort : WD_ObjectTypeEnum.InPropertyPort;
            CreatePort(desc.PropertyNames[i], id, desc.PropertyTypes[i], portType);
        }
        // Create methods.
        int nbOfMethodsToShow= 0;
        for(int i= 0; i < desc.MethodNames.Length; ++i) {
            if(desc.ParameterNames[i].Length != 0) ++nbOfMethodsToShow;
        }
        for(int i= 0; i < desc.MethodNames.Length; ++i) {
            int methodId= -1;
            if(nbOfMethodsToShow > 1) {
                methodId= GetNextAvailableId();
                EditorObjects[methodId]= new WD_EditorObject(methodId, desc.MethodNames[i], desc.ClassType, id, WD_ObjectTypeEnum.Function, new Rect(0,0,0,0), desc.MethodIcons[i]);
                TreeCache.CreateInstance(EditorObjects[methodId]);                
            }
            for(int p= 0; p < desc.ParameterNames[i].Length; ++p) {
                WD_ObjectTypeEnum portType= desc.ParameterInOuts[i][p] ? WD_ObjectTypeEnum.OutModulePort : WD_ObjectTypeEnum.InModulePort;
                WD_EditorObject classPort= CreatePort(desc.ParameterNames[i][p], id, desc.ParameterTypes[i][p], portType);
                if(nbOfMethodsToShow > 1) {
                    portType= desc.ParameterInOuts[i][p] ? WD_ObjectTypeEnum.OutFunctionPort : WD_ObjectTypeEnum.InFunctionPort;
                    WD_EditorObject funcPort= CreatePort(desc.ParameterNames[i][p], methodId, desc.ParameterTypes[i][p], portType);
                    if(portType == WD_ObjectTypeEnum.OutFunctionPort) {
                        SetSource(classPort, funcPort);
                    }
                    else {
                        SetSource(funcPort, classPort);
                    }                    
                }
            }
            if(desc.ReturnTypes[i] != null) {
                WD_EditorObject classPort= CreatePort(desc.ReturnNames[i], id, desc.ReturnTypes[i], WD_ObjectTypeEnum.OutModulePort);
                if(nbOfMethodsToShow > 1) {
                    WD_EditorObject funcPort= CreatePort(desc.ReturnNames[i], methodId, desc.ReturnTypes[i], WD_ObjectTypeEnum.OutFunctionPort);
                    SetSource(classPort, funcPort);                    
                }
            }
        }
        return EditorObjects[id];
    }
    // ----------------------------------------------------------------------
    public WD_EditorObject CreateFunction(int parentId, Vector2 initialPos, WD_FunctionDesc desc) {
        // Create the conversion node.
        int id= GetNextAvailableId();
        // Calcute the desired screen position of the new object.
        Rect parentPos= GetPosition(parentId);
        Rect localPos= new Rect(initialPos.x-parentPos.x, initialPos.y-parentPos.y,0,0);
        // Create new EditorObject
        EditorObjects[id]= new WD_EditorObject(id, desc.Name, desc.ClassType, parentId, WD_ObjectTypeEnum.Function, localPos, desc.Icon);
        TreeCache.CreateInstance(EditorObjects[id]);
        // Create input/output ports.
        for(int i= 0; i < desc.ParameterNames.Length; ++i) {
            WD_ObjectTypeEnum portType= desc.ParameterInOuts[i] ? WD_ObjectTypeEnum.OutFunctionPort : WD_ObjectTypeEnum.InFunctionPort;
            CreatePort(desc.ParameterNames[i], id, desc.ParameterTypes[i], portType);
        }
        if(desc.ReturnType != null) {
            CreatePort(desc.ReturnName, id, desc.ReturnType, WD_ObjectTypeEnum.OutFunctionPort);
        }
        return EditorObjects[id];
    }
    // ----------------------------------------------------------------------
    public WD_EditorObject CreateFunction(int parentId, Vector2 initialPos, WD_ConversionDesc desc) {
        // Create the function node.
        int id= GetNextAvailableId();
        // Calcute the desired screen position of the new object.
        Rect parentPos= GetPosition(parentId);
        Rect localPos= new Rect(initialPos.x-parentPos.x, initialPos.y-parentPos.y,0,0);
        // Create new EditorObject
        EditorObjects[id]= new WD_EditorObject(id, desc.Name, desc.ClassType, parentId, WD_ObjectTypeEnum.Conversion, localPos, desc.Icon);
        TreeCache.CreateInstance(EditorObjects[id]);
        // Create input/output ports.
        CreatePort(desc.FromType.Name, id, desc.FromType, WD_ObjectTypeEnum.InFunctionPort);
        CreatePort(desc.ToType.Name,   id, desc.ToType,   WD_ObjectTypeEnum.OutFunctionPort);
        return EditorObjects[id];
    }
    // ----------------------------------------------------------------------
    public WD_EditorObject CreatePort(string name, int parentId, Type valueType, WD_ObjectTypeEnum portType) {
        int id= GetNextAvailableId();
        WD_EditorObject port= EditorObjects[id]= new WD_EditorObject(id, name, valueType, parentId, portType, new Rect(0,0,0,0));
        TreeCache.CreateInstance(port);
        // Reajust data port position 
        if(port.IsDataPort && !port.IsEnablePort) {
            WD_EditorObject parent= EditorObjects[port.ParentId];
            if(port.IsInputPort) {
                int nbOfPorts= GetNbOfLeftPorts(parent);
                port.LocalPosition= new Rect(0, parent.LocalPosition.height/(nbOfPorts+1), 0, 0);
            } else {
                int nbOfPorts= GetNbOfRightPorts(parent);
                port.LocalPosition= new Rect(parent.LocalPosition.width, parent.LocalPosition.height/(nbOfPorts+1), 0, 0);                
            }
        }
        return EditorObjects[id];        
    }
    // ----------------------------------------------------------------------
    public void DestroyInstance(int id) {
        DestroyInstanceInternal(id);
        ForEach(
            (port) => {
                if(port.IsModulePort && EditorObjects[port.ParentId].IsModule && IsPortDisconnected(port)) {
                    DestroyInstanceInternal(port.InstanceId);
                }
            }
        );
    }
    // ----------------------------------------------------------------------
    public void DestroyInstance(WD_EditorObject eObj) {
        DestroyInstance(eObj.InstanceId);
    }
    // ----------------------------------------------------------------------
    void DestroyInstanceInternal(int id) {
        if(IsInvalid(id)) {
            Debug.LogError("Trying the delete a non-existing EditorObject with id= "+id);
        }
        // Remove all children first
        while(TreeCache[id].Children.Count != 0) {
            DestroyInstanceInternal(TreeCache[id].Children[0]);
        }
        // Disconnect ports linking to this port.
        ExecuteIf(EditorObjects[id], WD.IsPort, (instance) => { DisconnectPort(EditorObjects[id]); });
        // Remove all related objects.
        if(IsValid(EditorObjects[id].ParentId)) EditorObjects[EditorObjects[id].ParentId].IsDirty= true;
        TreeCache.DestroyInstance(id);
        EditorObjects[id].Reset();
    }


    // ======================================================================
    // Display Options
    // ----------------------------------------------------------------------
    public bool IsVisible(WD_EditorObject eObj) {
        if(eObj.IsHidden) return false;
        if(IsInvalid(eObj.ParentId)) return true;
        WD_EditorObject parent= EditorObjects[eObj.ParentId];
        if(eObj.IsNode && (parent.IsFolded || parent.IsMinimized)) return false;
        return IsVisible(parent);
    }
    public bool IsVisible(int id) { return IsInvalid(id) ? false : IsVisible(EditorObjects[id]); }
    // ----------------------------------------------------------------------
    public void Fold(WD_EditorObject eObj) {
        if(!eObj.IsNode) return;    // Only nodes can be folded.
        eObj.Fold();
        eObj.IsDirty= true;
    }
    public void Fold(int id) { if(IsValid(id)) Fold(EditorObjects[id]); }
    // ----------------------------------------------------------------------
    public void Unfold(WD_EditorObject eObj) {
        if(!eObj.IsNode) return;    // Only nodes can be folded.
        eObj.Unfold();
        eObj.IsDirty= true;
    }
    public void Unfold(int id) { if(IsValid(id)) Unfold(EditorObjects[id]); }
    // ----------------------------------------------------------------------
    public bool IsMinimized(WD_EditorObject eObj) {
        return eObj.IsMinimized;
    }
    public void Minimize(WD_EditorObject eObj) {
        if(!eObj.IsNode) return;
        eObj.Minimize();
        ForEachChild(eObj, (child) => { if(child.IsPort) child.Minimize(); });
        eObj.IsDirty= true;
        if(IsValid(eObj.ParentId)) EditorObjects[eObj.ParentId].IsDirty= true;
    }
    public void Minimize(int id) { if(IsValid(id)) Minimize(EditorObjects[id]); }
    // ----------------------------------------------------------------------
    public void Maximize(WD_EditorObject eObj) {
        if(!eObj.IsNode) return;
        eObj.Maximize();
        ForEachChild(eObj, (child) => { if(child.IsPort) child.Maximize(); });
        eObj.IsDirty= true;
        if(IsValid(eObj.ParentId)) EditorObjects[eObj.ParentId].IsDirty= true;
    }
    public void Maximize(int id) { if(IsValid(id)) Maximize(EditorObjects[id]); }
    


    // ======================================================================
    // Port Connectivity
    // ----------------------------------------------------------------------
    public void SetSource(WD_EditorObject obj, WD_EditorObject src) {
        obj.Source= src == null ? -1 : src.InstanceId;
    }
    // ----------------------------------------------------------------------
    public void SetSource(WD_EditorObject inPort, WD_EditorObject outPort, WD_ConversionDesc convDesc) {
        Rect inPos= GetPosition(inPort);
        Rect outPos= GetPosition(outPort);
        Vector2 convPos= new Vector2(0.5f*(inPos.x+outPos.x), 0.5f*(inPos.y+outPos.y));
        int grandParentId= EditorObjects[inPort.ParentId].ParentId;
        WD_EditorObject conv= CreateFunction(grandParentId, convPos, convDesc);
        ForEachChild(conv,
            (child) => {
                if(child.IsInputPort) {
                    SetSource(child, outPort);
                } else if(child.IsOutputPort) {
                    SetSource(inPort, child);
                }
            }
        );
        Minimize(conv);
    }
    // ----------------------------------------------------------------------
    public void DisconnectPort(WD_EditorObject port) {
        SetSource(port, null);
        ForEach(
            (obj) => {
                if(obj.IsPort && obj.Source == port.InstanceId) {
                    SetSource(obj, null);                            
                }
            }
        );                
    }
    // ----------------------------------------------------------------------
    bool IsPortConnected(WD_EditorObject port) {
        if(port.Source != -1) return true;
        foreach(var obj in EditorObjects) {
            if(obj.IsValid && obj.IsPort && obj.Source == port.InstanceId) return true;
        }
        return false;
    }
    bool IsPortDisconnected(WD_EditorObject port) { return !IsPortConnected(port); }


    // ======================================================================
    // Object Picking
    // ----------------------------------------------------------------------
    public WD_EditorObject GetRootNode() {
        foreach(var obj in EditorObjects) {
            if(obj.ParentId == -1) return obj;
        }
        Debug.LogError("No RootNode found!!!");
        return null;
    }
    // ----------------------------------------------------------------------
    // Returns the node at the given position
    public WD_EditorObject GetNodeAt(Vector2 pick) {
        WD_EditorObject foundNode= null;
        ForEach(
            (node) => {
                if(node.IsNode && IsVisible(node) && IsInside(node, pick)) {
                    if(foundNode == null || node.LocalPosition.width < foundNode.LocalPosition.width) {
                        foundNode= node;
                    }
                }                
            }
        );
        return foundNode;
    }
    
    // ----------------------------------------------------------------------
    // Returns the connection at the given position.
    public WD_EditorObject GetPortAt(Vector2 pick) {
        WD_EditorObject bestPort= null;
        float bestDistance= 100000;     // Simply a big value
        ForEach(
            (port) => {
                if(port.IsPort && IsVisible(port)) {
                    Rect tmp= GetPosition(port);
                    Vector2 position= new Vector2(tmp.x, tmp.y);
                    float distance= Vector2.Distance(position, pick);
                    if(distance < 1.5f * WD_EditorConfig.PortRadius && distance < bestDistance) {
                        bestDistance= distance;
                        bestPort= port;
                    }                
                }                
            }
        );
        return bestPort;
    }


    // ======================================================================
    // Node Layout
    // ----------------------------------------------------------------------
    // Moves the node without changing its size.
    public void SetInitialPosition(WD_EditorObject obj, Vector2 initialPosition) {
        if(IsValid(obj.ParentId)) {
            Rect position= GetPosition(EditorObjects[obj.ParentId]);
            obj.LocalPosition.x= initialPosition.x - position.x;
            obj.LocalPosition.y= initialPosition.y - position.y;            
        }
        else {
            obj.LocalPosition.x= initialPosition.x;
            obj.LocalPosition.y= initialPosition.y;                        
        }
        IsDirty= true;
    }

    // ----------------------------------------------------------------------
    public void Layout(WD_EditorObject obj) {
        obj.IsDirty= false;
        Case(obj,
            WD.IsNode, (node) => { NodeLayout(node); },
            WD.IsPort, (port) => { PortLayout(port); }
        );
    }
    public void Layout(int id) {
        if(IsInvalid(id)) return;
        Layout(EditorObjects[id]);
    }
    // ----------------------------------------------------------------------
    // Recompute the layout of a parent node.
    // Returns "true" if the new layout is within the window area.
    public void NodeLayout(WD_EditorObject node) {
        // Don't layout node if it is not visible.
        if(!IsVisible(node)) return;

        // Minimized nodes are fully collapsed.
        if(node.IsMinimized) {
            float iconWidth= WD_EditorConfig.MaximizeNodeWidth;
            float iconHeight= WD_EditorConfig.MaximizeNodeHeight;
//            if(node.Icon != null) {
//                Texture icon= Asset
//                AssetDatabase.GetCachedIcon(node.Icon);
//                if(icon != null) {
//                    iconWidth= icon.width;
//                    iconHeight= icon.height;
//                }
//            }
            if(node.LocalPosition.width != iconWidth || node.LocalPosition.height != iconHeight) {
                   node.LocalPosition.x+= 0.5f*(node.LocalPosition.width-iconWidth);
                   node.LocalPosition.y+= 0.5f*(node.LocalPosition.height-iconHeight);
                   node.LocalPosition.width= iconWidth;
                   node.LocalPosition.height= iconHeight;
            }
            Vector2 nodeCenter= new Vector2(0.5f*node.LocalPosition.width, 0.5f*node.LocalPosition.height);
            ForEachChild(node,
                (child) => {
                    if(child.IsPort) {
                        child.LocalPosition.x= nodeCenter.x;
                        child.LocalPosition.y= nodeCenter.y;
                    }
                }
            );
            return;
        }

        // Resolve collision on children.
        ResolveCollisionOnChildren(node, Vector2.zero);

        // Determine needed child rect.
        Rect  childRect   = ComputeChildRect(node);

        // Compute needed width.
        float titleWidth  = WD_EditorConfig.GetNodeWidth(node.NameOrTypeName)+WD_EditorConfig.ExtraIconWidth;
        float leftMargin  = ComputeLeftMargin(node);
        float rightMargin = ComputeRightMargin(node);
        float width       = 2.0f*WD_EditorConfig.GutterSize + Mathf.Max(titleWidth, leftMargin + rightMargin + childRect.width);

        // Process case without child nodes
        Rect position= GetPosition(node);
        if(MathfExt.IsZero(childRect.width) || MathfExt.IsZero(childRect.height)) {
            // Compute needed height.
            List<WD_EditorObject> leftPorts= GetLeftPorts(node);
            List<WD_EditorObject> rightPorts= GetRightPorts(node);
            int nbOfPorts= leftPorts.Count > rightPorts.Count ? leftPorts.Count : rightPorts.Count;
            float height= Mathf.Max(WD_EditorConfig.NodeTitleHeight+nbOfPorts*WD_EditorConfig.MinimumPortSeparation, WD_EditorConfig.MinimumNodeHeight);                                

            // Apply new width and height.
            if(MathfExt.IsNotEqual(height, position.height) || MathfExt.IsNotEqual(width, position.width)) {
                float deltaWidth = width - position.width;
                float deltaHeight= height - position.height;
                Rect newPos= new Rect(position.xMin-0.5f*deltaWidth, position.yMin-0.5f*deltaHeight, width, height);
                SetPosition(node, newPos);
            }
        }
        // Process case with child nodes.
        else {
            // Adjust children local offset.
            float neededChildXOffset= WD_EditorConfig.GutterSize+leftMargin;
            float neededChildYOffset= WD_EditorConfig.GutterSize+WD_EditorConfig.NodeTitleHeight;
            if(MathfExt.IsNotEqual(neededChildXOffset, childRect.x) ||
               MathfExt.IsNotEqual(neededChildYOffset, childRect.y)) {
                   AdjustChildLocalPosition(node, new Vector2(neededChildXOffset-childRect.x, neededChildYOffset-childRect.y));
            }

            // Compute needed height.
            int nbOfLeftPorts = GetNbOfLeftPorts(node);
            int nbOfRightPorts= GetNbOfRightPorts(node);
            int nbOfPorts= nbOfLeftPorts > nbOfRightPorts ? nbOfLeftPorts : nbOfRightPorts;
            float portHeight= nbOfPorts*WD_EditorConfig.MinimumPortSeparation;                                
            float height= WD_EditorConfig.NodeTitleHeight+Mathf.Max(portHeight, childRect.height+2.0f*WD_EditorConfig.GutterSize);

            float deltaWidth = width - node.LocalPosition.width;
            float deltaHeight= height - node.LocalPosition.height;
            float xMin= position.xMin-0.5f*deltaWidth;
            float yMin= position.yMin-0.5f*deltaHeight;
            if(MathfExt.IsNotEqual(xMin, position.xMin) || MathfExt.IsNotEqual(yMin, position.yMin) ||
               MathfExt.IsNotEqual(width, node.LocalPosition.width) || MathfExt.IsNotEqual(height, node.LocalPosition.height)) {
                Rect newPos= new Rect(xMin, yMin, width, height);
                SetPosition(node, newPos);
            }
        }

        // Layout child ports
        LayoutPorts(node);
    }
    // ----------------------------------------------------------------------
    // Moves the node without changing its size.
    public void MoveTo(WD_EditorObject node, Vector2 _newPos) {
        Rect position = GetPosition(node);
        DeltaMove(node, new Vector2(_newPos.x - position.x, _newPos.y - position.y));
    }
    // ----------------------------------------------------------------------
    // Moves the node without changing its size.
    public void DeltaMove(WD_EditorObject node, Vector2 _delta) {
        // Move the node
        DeltaMoveInternal(node, _delta);
        // Resolve collision between siblings.
        LayoutParent(node, _delta);
	}
    // ----------------------------------------------------------------------
    // Moves the node without changing its size.
    void DeltaMoveInternal(WD_EditorObject node, Vector2 _delta) {
        if(MathfExt.IsNotZero(_delta)) {
            node.LocalPosition.x+= _delta.x;
            node.LocalPosition.y+= _delta.y;
            node.IsDirty= true;
        }
    }
    // ----------------------------------------------------------------------
    // Returns the absolute position of the node.
    public Rect GetPosition(WD_EditorObject node) {
        if(!IsValid(node.ParentId)) return node.LocalPosition;
        Rect position= GetPosition(EditorObjects[node.ParentId]);
        return new Rect(position.x+node.LocalPosition.x,
                        position.y+node.LocalPosition.y,
                        node.LocalPosition.width,
                        node.LocalPosition.height);
    }
    public Rect GetPosition(int id) {
        return GetPosition(EditorObjects[id]);
    }
    // ----------------------------------------------------------------------
    void SetPosition(WD_EditorObject node, Rect _newPos) {
        // Adjust node size.
        node.LocalPosition.width = _newPos.width;
        node.LocalPosition.height= _newPos.height;
        // Reposition node.
        if(!IsValid(node.ParentId)) {
            node.LocalPosition.x= _newPos.x;
            node.LocalPosition.y= _newPos.y;            
        }
        else {
            Rect position= GetPosition(node);
            Rect deltaMove= new Rect(_newPos.xMin-position.xMin, _newPos.yMin-position.yMin, _newPos.width-position.width, _newPos.height-position.height);
            node.LocalPosition.x+= deltaMove.x;
            node.LocalPosition.y+= deltaMove.y;
            node.LocalPosition.width= _newPos.width;
            node.LocalPosition.height= _newPos.height;
            float separationX= Mathf.Abs(deltaMove.x) > Mathf.Abs(deltaMove.width) ? deltaMove.x : deltaMove.width;
            float separationY= Mathf.Abs(deltaMove.y) > Mathf.Abs(deltaMove.height) ? deltaMove.y : deltaMove.height;
            LayoutParent(node, new Vector2(separationX, separationY));
        }
    }    
    // ----------------------------------------------------------------------
    Vector2 GetTopLeftCorner(WD_EditorObject node)     {
        Rect position= GetPosition(node);
        return new Vector2(position.xMin, position.yMin);
    }
    Vector2 GetTopRightCorner(WD_EditorObject node)    {
        Rect position= GetPosition(node);
        return new Vector2(position.xMax, position.yMin);
    }
    Vector2 GetBottomLeftCorner(WD_EditorObject node)  {
        Rect position= GetPosition(node);
        return new Vector2(position.xMin, position.yMax);
    }
    Vector2 GetBottomRightCorner(WD_EditorObject node) {
        Rect position= GetPosition(node);
        return new Vector2(position.xMax, position.yMax);
    }
    // ----------------------------------------------------------------------
    void LayoutParent(WD_EditorObject node, Vector2 _deltaMove) {
        if(!IsValid(node.ParentId)) return;
        WD_EditorObject parentNode= EditorObjects[node.ParentId];
        ResolveCollision(parentNode, _deltaMove);
        Layout(parentNode);
    }
    // ----------------------------------------------------------------------
    void AdjustChildLocalPosition(WD_EditorObject node, Vector2 _delta) {
        ForEachChild(node, (child)=> { if(child.IsNode) DeltaMoveInternal(child, _delta); } );
    }
    // ----------------------------------------------------------------------
    // Returns the space used by all children.
    Rect ComputeChildRect(WD_EditorObject node) {
        // Compute child space.
        Rect childRect= new Rect(0.5f*node.LocalPosition.width,0.5f*node.LocalPosition.height,0,0);
        ForEachChild(node,
            (child)=> {
                if(child.IsNode && IsVisible(child)) {
                    childRect= Physics2D.Merge(childRect, child.LocalPosition);
                }
            }
        );
        return childRect;
    }
    // ----------------------------------------------------------------------
    // Returns the inner left margin.
    float ComputeLeftMargin(WD_EditorObject node) {
        float LeftMargin= 0;
        ForEachLeftPort(node,
            (port)=> {
                Vector2 labelSize= WD_EditorConfig.GetPortLabelSize(port.Name);
                float nameSize= labelSize.x+WD_EditorConfig.PortSize;
                if(LeftMargin < nameSize) LeftMargin= nameSize;
            }
        );
        return LeftMargin;
    }
    // ----------------------------------------------------------------------
    // Returns the inner right margin.
    float ComputeRightMargin(WD_EditorObject node) {
        float RightMargin= 0;
        ForEachRightPort(node,
            (port) => {
                Vector2 labelSize= WD_EditorConfig.GetPortLabelSize(port.Name);
                float nameSize= labelSize.x+WD_EditorConfig.PortSize;
                if(RightMargin < nameSize) RightMargin= nameSize;
            }
        );
        return RightMargin;
    }
    // ----------------------------------------------------------------------
    // Returns the inner top margin.
    static float ComputeTopMargin(WD_EditorObject node) {
        return WD_EditorConfig.GetNodeHeight(node.NameOrTypeName);
    }
    // ----------------------------------------------------------------------
    // Returns the inner bottom margin.
    static float ComputeBottomMargin(WD_EditorObject node) {
        return 0;
    }


    // ======================================================================
    // Port Layout
    // ----------------------------------------------------------------------
    // Recomputes the port position.
    public void LayoutPorts(WD_EditorObject node) {
		// Gather all ports.
        List<WD_EditorObject> topPorts   = new List<WD_EditorObject>();
        List<WD_EditorObject> bottomPorts= new List<WD_EditorObject>();
        List<WD_EditorObject> leftPorts  = new List<WD_EditorObject>();
        List<WD_EditorObject> rightPorts = new List<WD_EditorObject>();
        ForEachChild(node,
            (port)=> {
                if(port.IsPort) {
                    if(port.IsOnTopEdge)         { topPorts.Add(port); }
                    else if(port.IsOnBottomEdge) { bottomPorts.Add(port); }
                    else if(port.IsOnLeftEdge)   { leftPorts.Add(port); }
                    else if(port.IsOnRightEdge)  { rightPorts.Add(port); }                    
                }
            }
        );
        
        // Relayout top ports.
        Rect position= GetPosition(node);
        if(topPorts.Count != 0) {
            SortPorts(topPorts);
            float xStep= position.width / topPorts.Count;
            for(int i= 0; i < topPorts.Count; ++i) {
                if(topPorts[i].IsBeingDragged == false) {
                    topPorts[i].LocalPosition.x= (i+0.5f) * xStep;
                    topPorts[i].LocalPosition.y= 0;                
                }
            }            
        }

        // Relayout bottom ports.
        if(bottomPorts.Count != 0) {
            SortPorts(bottomPorts);
            float xStep= position.width / bottomPorts.Count;
            for(int i= 0; i < bottomPorts.Count; ++i) {
                if(bottomPorts[i].IsBeingDragged == false) {
                    bottomPorts[i].LocalPosition.x= (i+0.5f) * xStep;
                    bottomPorts[i].LocalPosition.y= position.height;                
                }
            }            
        }

        // Relayout left ports.
        if(leftPorts.Count != 0) {
            SortPorts(leftPorts);
            float topOffset= WD_EditorConfig.NodeTitleHeight;
            float yStep= (position.height-topOffset) / leftPorts.Count;
            for(int i= 0; i < leftPorts.Count; ++i) {
                if(leftPorts[i].IsBeingDragged == false) {
                    leftPorts[i].LocalPosition.x= 0;
                    leftPorts[i].LocalPosition.y= topOffset + (i+0.5f) * yStep;                
                }
            }            
        }

        // Relayout right ports.
        if(rightPorts.Count != 0) {
            SortPorts(rightPorts);
            float topOffset= WD_EditorConfig.NodeTitleHeight;
            float yStep= (position.height-topOffset) / rightPorts.Count;
            for(int i= 0; i < rightPorts.Count; ++i) {
                if(rightPorts[i].IsBeingDragged == false) {
                    rightPorts[i].LocalPosition.x= position.width;
                    rightPorts[i].LocalPosition.y= topOffset + (i+0.5f) * yStep;                
    
                }
            }
        }        
    }

    // ----------------------------------------------------------------------
    // Sorts the given port according to their relative positions.
    void SortPorts(List<WD_EditorObject> _ports) {
        for(int i= 0; i < _ports.Count-1; ++i) {
            Vector2 localPos= new Vector2(_ports[i].LocalPosition.x, _ports[i].LocalPosition.y);
            float sqrMag= localPos.sqrMagnitude;
            for(int j= i+1; j < _ports.Count; ++j) {
                localPos= new Vector2(_ports[j].LocalPosition.x, _ports[j].LocalPosition.y);
				float sqrMag2= localPos.sqrMagnitude;
				if(sqrMag > sqrMag2) {
                    WD_EditorObject p= _ports[i];
                    _ports[i]= _ports[j];
                    _ports[j]= p;
					sqrMag= sqrMag2;
                }
            }
        }
    }
    // ----------------------------------------------------------------------
    // Returns all ports position on the top edge.
    public List<WD_EditorObject> GetTopPorts(WD_EditorObject node) {
        List<WD_EditorObject> ports= new List<WD_EditorObject>();
        ForEachTopPort(node, (port)=> { ports.Add(port); } );
        return ports;
    }

    // ----------------------------------------------------------------------
    // Returns all ports position on the bottom edge.
    public List<WD_EditorObject> GetBottomPorts(WD_EditorObject node) {
        List<WD_EditorObject> ports= new List<WD_EditorObject>();
        ForEachBottomPort(node, (port)=> { ports.Add(port); } );
        return ports;
    }

    // ----------------------------------------------------------------------
    // Returns all ports position on the left edge.
    public List<WD_EditorObject> GetLeftPorts(WD_EditorObject node) {
        List<WD_EditorObject> ports= new List<WD_EditorObject>();
        ForEachLeftPort(node, (port)=> { ports.Add(port); } );
        return ports;        
    }

    // ----------------------------------------------------------------------
    // Returns all ports position on the right edge.
    public List<WD_EditorObject> GetRightPorts(WD_EditorObject node) {
        List<WD_EditorObject> ports= new List<WD_EditorObject>();
        ForEachRightPort(node, (port)=> { ports.Add(port); } );
        return ports;
    }
    // ----------------------------------------------------------------------
    // Returns the number of ports on the top edge.
    public int GetNbOfTopPorts(WD_EditorObject node) {
        int nbOfPorts= 0;
        ForEachTopPort(node, (port)=> { ++nbOfPorts; } );
        return nbOfPorts;
    }

    // ----------------------------------------------------------------------
    // Returns the number of ports on the bottom edge.
    public int GetNbOfBottomPorts(WD_EditorObject node) {
        int nbOfPorts= 0;
        ForEachBottomPort(node, (port)=> { ++nbOfPorts; } );
        return nbOfPorts;
    }

    // ----------------------------------------------------------------------
    // Returns the number of ports on the left edge.
    public int GetNbOfLeftPorts(WD_EditorObject node) {
        int nbOfPorts= 0;
        ForEachLeftPort(node, (port)=> { ++nbOfPorts; } );
        return nbOfPorts;
    }

    // ----------------------------------------------------------------------
    // Returns the number of ports on the right edge.
    public int GetNbOfRightPorts(WD_EditorObject node) {
        int nbOfPorts= 0;
        ForEachRightPort(node, (port)=> { ++nbOfPorts; } );
        return nbOfPorts;
    }

    // ----------------------------------------------------------------------
    public void ForEachTopPort(WD_EditorObject node, System.Action<WD_EditorObject> fnc) {
        ForEachChild(node,
            (port)=> {
                if(port.IsPort && port.IsOnTopEdge) {
                    fnc(port);
                }
            }
        );        
    }
    
    // ----------------------------------------------------------------------
    public void ForEachBottomPort(WD_EditorObject node, System.Action<WD_EditorObject> fnc) {
        ForEachChild(node,
            (port)=> {
                if(port.IsPort && port.IsOnBottomEdge) {
                    fnc(port);
                }
            }
        );        
    }
    
    // ----------------------------------------------------------------------
    public void ForEachLeftPort(WD_EditorObject node, System.Action<WD_EditorObject> fnc) {
        ForEachChild(node,
            (port)=> {
                if(port.IsPort && port.IsOnLeftEdge) {
                    fnc(port);
                }
            }
        );        
    }
    
    // ----------------------------------------------------------------------
    public void ForEachRightPort(WD_EditorObject node, System.Action<WD_EditorObject> fnc) {
        ForEachChild(node,
            (port)=> {
                if(port.IsPort && port.IsOnRightEdge) {
                    fnc(port);
                }
            }
        );        
    }


    // ======================================================================
    // Collision Functions
    // ----------------------------------------------------------------------
    // Resolve collision on parents.
    void ResolveCollision(WD_EditorObject node, Vector2 _delta) {
        ResolveCollisionOnChildren(node, _delta);
        if(!IsValid(node.ParentId)) return;
        ResolveCollision(EditorObjects[node.ParentId], _delta);
    }

    // ----------------------------------------------------------------------
    // Resolves the collision between children.  "true" is returned if a
    // collision has occured.
    public void ResolveCollisionOnChildren(WD_EditorObject node, Vector2 _delta) {
        bool didCollide= false;
        for(int i= 0; i < EditorObjects.Count-1; ++i) {
            WD_EditorObject child1= EditorObjects[i];
            if(child1.ParentId != node.InstanceId) continue;
            if(!IsVisible(child1)) continue;
            if(!child1.IsNode) continue;
            for(int j= i+1; j < EditorObjects.Count; ++j) {
                WD_EditorObject child2= EditorObjects[j];
                if(child2.ParentId != node.InstanceId) continue;
                if(!IsVisible(child2)) continue;
                if(!child2.IsNode) continue;
                didCollide |= ResolveCollisionBetweenTwoNodes(child1, child2, _delta);                            
            }
        }
        if(didCollide) ResolveCollisionOnChildren(node, _delta);
    }

    // ----------------------------------------------------------------------
    // Resolves collision between two nodes. "true" is returned if a collision
    // has occured.
    public bool ResolveCollisionBetweenTwoNodes(WD_EditorObject node, WD_EditorObject otherNode, Vector2 _delta) {
        // Nothing to do if they don't collide.
        if(!DoesCollideWithGutter(node, otherNode)) return false;

        // Compute penetration.
        Vector2 penetration= GetSeperationVector(node, GetPosition(otherNode));
		if(Mathf.Abs(penetration.x) < 1.0f && Mathf.Abs(penetration.y) < 1.0f) return false;

		// Seperate using the known movement.
        if( !MathfExt.IsZero(_delta) ) {
    		if(Vector2.Dot(_delta, penetration) > 0) {
    		    DeltaMoveInternal(otherNode, penetration);
    		}
    		else {
    		    DeltaMoveInternal(node, -penetration);
    		}            
    		return true;
        }

		// Seperate nodes by the penetration that is not a result of movement.
        penetration*= 0.5f;
        DeltaMoveInternal(otherNode, penetration);
        DeltaMoveInternal(node, -penetration);
        return true;
    }

    // ----------------------------------------------------------------------
    // Returns if the given rectangle collides with the node.
    public bool DoesCollide(WD_EditorObject node, WD_EditorObject otherNode) {
        return Physics2D.DoesCollide(GetPosition(node), GetPosition(otherNode));
    }

    // ----------------------------------------------------------------------
    // Returns if the given rectangle collides with the node.
    public bool DoesCollideWithGutter(WD_EditorObject node, WD_EditorObject otherNode) {
        return Physics2D.DoesCollide(RectWithGutter(GetPosition(node)), GetPosition(otherNode));
    }

    // ----------------------------------------------------------------------
    static Rect RectWithGutter(Rect _rect) {
        float gutterSize= WD_EditorConfig.GutterSize;
        float gutterSize2= 2.0f*gutterSize;
        return new Rect(_rect.x-gutterSize, _rect.y-gutterSize, _rect.width+gutterSize2, _rect.height+gutterSize2);        
    }
    
    // ----------------------------------------------------------------------
	// Returns the seperation vector of two colliding nodes.
	Vector2 GetSeperationVector(WD_EditorObject node, Rect _rect) {
        Rect myRect= RectWithGutter(GetPosition(node));
        Rect otherRect= _rect;
        float xMin= Mathf.Min(myRect.xMin, otherRect.xMin);
        float yMin= Mathf.Min(myRect.yMin, otherRect.yMin);
        float xMax= Mathf.Max(myRect.xMax, otherRect.xMax);
        float yMax= Mathf.Max(myRect.yMax, otherRect.yMax);
        float xDistance= xMax-xMin;
        float yDistance= yMax-yMin;
        float totalWidth= myRect.width+otherRect.width;
        float totalHeight= myRect.height+otherRect.height;
        if(xDistance >= totalWidth) return Vector2.zero;
        if(yDistance >= totalHeight) return Vector2.zero;
        if((totalWidth-xDistance) < (totalHeight-yDistance)) {
            if(myRect.xMin < otherRect.xMin) {
                return new Vector2(totalWidth-xDistance, 0);
            }
            else {
                return new Vector2(xDistance-totalWidth, 0);
            }
        }
        else {
            if(myRect.yMin < otherRect.yMin) {
                return new Vector2(0, totalHeight-yDistance);
            }
            else {
                return new Vector2(0, yDistance-totalHeight);                
            }            
        }
	}
	Vector2 GetSeperationVector(WD_EditorObject node, WD_EditorObject otherNode) {
	    return GetSeperationVector(node, GetPosition(otherNode));
	}

    // ----------------------------------------------------------------------
    // Returns true if the given point is inside the node coordinates.
    bool IsInside(WD_EditorObject node, Vector2 _point) {
        return Physics2D.IsInside(_point, GetPosition(node));
    }


    // ======================================================================
    // Layout from WD_Port
    // ----------------------------------------------------------------------
    public void PortLayout(WD_EditorObject port) {
        // Don't interfear with dragging.
        if(port.IsBeingDragged) return;

        // Retreive parent layout information.
        if(!IsValid(port.ParentId)) {
            Debug.LogWarning("Trying to layout a port who does not have a parent!!!");
            return;
        }
        WD_EditorObject parentNode= EditorObjects[port.ParentId];
        Rect parentPosition= GetPosition(parentNode);

        // Make certain that the port is on an edge.
        switch(port.Edge) {
            case WD_EditorObject.EdgeEnum.Top:
                if(!MathfExt.IsZero(port.LocalPosition.y)) {
                    port.LocalPosition.y= 0;
                    port.IsDirty= true;
                    parentNode.IsDirty= true;                    
                }
                if(port.LocalPosition.x > parentPosition.width) {
                    port.LocalPosition.x= parentPosition.width-WD_EditorConfig.PortSize;
                    port.IsDirty= true;
                    parentNode.IsDirty= true;
                }
                break;
            case WD_EditorObject.EdgeEnum.Bottom:
                if(MathfExt.IsNotEqual(port.LocalPosition.y, parentPosition.height)) {
                    port.LocalPosition.y= parentPosition.height;
                    port.IsDirty= true;
                    parentNode.IsDirty= true;                    
                }
                if(port.LocalPosition.x > parentPosition.width) {
                    port.LocalPosition.x= parentPosition.width-WD_EditorConfig.PortSize;
                    port.IsDirty= true;
                    parentNode.IsDirty= true;
                }
                break;
            case WD_EditorObject.EdgeEnum.Left:
                if(!MathfExt.IsZero(port.LocalPosition.x)) {
                    port.LocalPosition.x= 0;
                    port.IsDirty= true;
                    parentNode.IsDirty= true;                    
                }
                if(port.LocalPosition.y > parentPosition.height) {
                    port.LocalPosition.y= parentPosition.height-WD_EditorConfig.PortSize;
                    port.IsDirty= true;
                    parentNode.IsDirty= true;
                }
                break;
            case WD_EditorObject.EdgeEnum.Right:
                if(MathfExt.IsNotEqual(port.LocalPosition.x, parentPosition.width)) {
                    port.LocalPosition.x= parentPosition.width;
                    port.IsDirty= true;
                    parentNode.IsDirty= true;                    
                }
                if(port.LocalPosition.y > parentPosition.height) {
                    port.LocalPosition.y= parentPosition.height-WD_EditorConfig.PortSize;
                    port.IsDirty= true;
                    parentNode.IsDirty= true;
                }
                break;            
        }
    }
    // ----------------------------------------------------------------------
    public void SnapToParent(WD_EditorObject port) {
        WD_EditorObject parentNode= EditorObjects[port.ParentId];
        Rect parentPosition= GetPosition(parentNode);
        float parentHeight= parentPosition.height;
        float parentWidth= parentPosition.width;
        float portRadius= WD_EditorConfig.PortRadius;
        if(MathfExt.IsWithin(port.LocalPosition.y, -portRadius, portRadius)) {
            port.Edge= WD_EditorObject.EdgeEnum.Top;
        }        
        if(MathfExt.IsWithin(port.LocalPosition.y, parentHeight-portRadius, parentHeight+portRadius)) {
            port.Edge= WD_EditorObject.EdgeEnum.Bottom;
        }
        if(MathfExt.IsWithin(port.LocalPosition.x, -portRadius, portRadius)) {
            port.Edge= WD_EditorObject.EdgeEnum.Left;
        }
        if(MathfExt.IsWithin(port.LocalPosition.x, parentWidth-portRadius, parentWidth+portRadius)) {
            port.Edge= WD_EditorObject.EdgeEnum.Right;
        }
        port.IsDirty= true;
        PortLayout(port);
    }

    // ----------------------------------------------------------------------
    // Returns the minimal distance from the parent.
    public float GetDistanceFromParent(WD_EditorObject port) {
        WD_EditorObject parentNode= EditorObjects[port.ParentId];
        Rect tmp= GetPosition(port);
        Vector2 position= new Vector2(tmp.x, tmp.y);
        if(IsInside(parentNode, position)) return 0;
        Rect parentPosition= GetPosition(parentNode);
        if(position.x > parentPosition.xMin && position.x < parentPosition.xMax) {
            return Mathf.Min(Mathf.Abs(position.y-parentPosition.yMin),
                             Mathf.Abs(position.y-parentPosition.yMax));
        }
        if(position.y > parentPosition.yMin && position.y < parentPosition.yMax) {
            return Mathf.Min(Mathf.Abs(position.x-parentPosition.xMin),
                             Mathf.Abs(position.x-parentPosition.xMax));
        }
        float distance= Vector2.Distance(position, GetTopLeftCorner(parentNode));
        distance= Mathf.Min(distance, Vector2.Distance(position, GetTopRightCorner(parentNode)));
        distance= Mathf.Min(distance, Vector2.Distance(position, GetBottomLeftCorner(parentNode)));
        distance= Mathf.Min(distance, Vector2.Distance(position, GetBottomRightCorner(parentNode)));
        return distance;
    }

    // ----------------------------------------------------------------------
    // Returns true if the distance to parent is less then twice the port size.
    public bool IsNearParent(WD_EditorObject port) {
        return GetDistanceFromParent(port) <= WD_EditorConfig.PortSize*2;
    }

	// ----------------------------------------------------------------------
    public WD_EditorObject GetOverlappingPort(WD_EditorObject port) {
        WD_EditorObject foundPort= null;
        Rect tmp= GetPosition(port);
        Vector2 position= new Vector2(tmp.x, tmp.y);
        ForEach(
            (p) => {
                if(p.IsPort && p != port) {
                    tmp= GetPosition(p);
                    Vector2 pPos= new Vector2(tmp.x, tmp.y);
                    float distance= Vector2.Distance(pPos, position);
                    if(distance <= 1.5*WD_EditorConfig.PortSize) {
                        foundPort= p;
                    }
                }                
            }
        );
        return foundPort;
    }	


    // ======================================================================
    // Editor Object Iteration Utilities
    // ----------------------------------------------------------------------
    // Executes the given action if the given object matches the T type.
    public void ExecuteIf(WD_EditorObject obj, Func<WD_EditorObject,bool> cmp, Action<WD_EditorObject> f) {
        if(cmp(obj)) f(obj);
    }
    public void ExecuteIf(int id, Func<WD_EditorObject,bool> cmp, Action<WD_EditorObject> f) {
        if(!IsValid(id)) return;
        ExecuteIf(EditorObjects[id], cmp, f);
    }
    public void Case(WD_EditorObject obj,
                     Func<WD_EditorObject,bool> c1, Action<WD_EditorObject> f1,
                     Func<WD_EditorObject,bool> c2, Action<WD_EditorObject> f2,
                                                    Action<WD_EditorObject> defaultFnc= null) {
        if(c1(obj)) f1(obj);
        else if(c2(obj)) f2(obj);
        else if(defaultFnc != null) defaultFnc(obj);
    }
    public void Case(WD_EditorObject obj, 
                     Func<WD_EditorObject,bool> c1, Action<WD_EditorObject> f1,
                     Func<WD_EditorObject,bool> c2, Action<WD_EditorObject> f2,
                     Func<WD_EditorObject,bool> c3, Action<WD_EditorObject> f3,
                                                    Action<WD_EditorObject> defaultFnc= null) {
        if(c1(obj)) f1(obj);
        else if(c2(obj)) f2(obj);
        else if(c3(obj)) f3(obj);
        else if(defaultFnc != null) defaultFnc(obj);
    }
    public void Case(WD_EditorObject obj, 
                     Func<WD_EditorObject,bool> c1, Action<WD_EditorObject> f1,
                     Func<WD_EditorObject,bool> c2, Action<WD_EditorObject> f2,
                     Func<WD_EditorObject,bool> c3, Action<WD_EditorObject> f3,
                     Func<WD_EditorObject,bool> c4, Action<WD_EditorObject> f4,
                                                    Action<WD_EditorObject> defaultFnc= null) {
        if(c1(obj)) f1(obj);
        else if(c2(obj)) f2(obj);
        else if(c3(obj)) f3(obj);
        else if(c4(obj)) f4(obj);
        else if(defaultFnc != null) defaultFnc(obj);
    }
    public void Case(WD_EditorObject obj, 
                     Func<WD_EditorObject,bool> c1, Action<WD_EditorObject> f1,
                     Func<WD_EditorObject,bool> c2, Action<WD_EditorObject> f2,
                     Func<WD_EditorObject,bool> c3, Action<WD_EditorObject> f3,
                     Func<WD_EditorObject,bool> c4, Action<WD_EditorObject> f4,
                     Func<WD_EditorObject,bool> c5, Action<WD_EditorObject> f5,
                                                    Action<WD_EditorObject> defaultFnc= null) {
        if(c1(obj)) f1(obj);
        else if(c2(obj)) f2(obj);
        else if(c3(obj)) f3(obj);
        else if(c4(obj)) f4(obj);
        else if(c5(obj)) f5(obj);
        else if(defaultFnc != null) defaultFnc(obj);
    }
    public void Case(WD_EditorObject obj, 
                     Func<WD_EditorObject,bool> c1, Action<WD_EditorObject> f1,
                     Func<WD_EditorObject,bool> c2, Action<WD_EditorObject> f2,
                     Func<WD_EditorObject,bool> c3, Action<WD_EditorObject> f3,
                     Func<WD_EditorObject,bool> c4, Action<WD_EditorObject> f4,
                     Func<WD_EditorObject,bool> c5, Action<WD_EditorObject> f5,
                     Func<WD_EditorObject,bool> c6, Action<WD_EditorObject> f6,
                                                    Action<WD_EditorObject> defaultFnc= null) {
        if(c1(obj)) f1(obj);
        else if(c2(obj)) f2(obj);
        else if(c3(obj)) f3(obj);
        else if(c4(obj)) f4(obj);
        else if(c5(obj)) f5(obj);
        else if(c6(obj)) f6(obj);
        else if(defaultFnc != null) defaultFnc(obj);
    }
    public void Case(WD_EditorObject obj,
                     Func<WD_EditorObject,bool> c1, Action<WD_EditorObject> f1,
                     Func<WD_EditorObject,bool> c2, Action<WD_EditorObject> f2,
                     Func<WD_EditorObject,bool> c3, Action<WD_EditorObject> f3,
                     Func<WD_EditorObject,bool> c4, Action<WD_EditorObject> f4,
                     Func<WD_EditorObject,bool> c5, Action<WD_EditorObject> f5,
                     Func<WD_EditorObject,bool> c6, Action<WD_EditorObject> f6,
                     Func<WD_EditorObject,bool> c7, Action<WD_EditorObject> f7,
                                                    Action<WD_EditorObject> defaultFnc= null) {
        if(c1(obj)) f1(obj);
        else if(c2(obj)) f2(obj);
        else if(c3(obj)) f3(obj);
        else if(c4(obj)) f4(obj);
        else if(c5(obj)) f5(obj);
        else if(c6(obj)) f6(obj);
        else if(c7(obj)) f7(obj);
        else if(defaultFnc != null) defaultFnc(obj);
    }
    public void Case(WD_EditorObject obj,
                     Func<WD_EditorObject,bool> c1, Action<WD_EditorObject> f1,
                     Func<WD_EditorObject,bool> c2, Action<WD_EditorObject> f2,
                     Func<WD_EditorObject,bool> c3, Action<WD_EditorObject> f3,
                     Func<WD_EditorObject,bool> c4, Action<WD_EditorObject> f4,
                     Func<WD_EditorObject,bool> c5, Action<WD_EditorObject> f5,
                     Func<WD_EditorObject,bool> c6, Action<WD_EditorObject> f6,
                     Func<WD_EditorObject,bool> c7, Action<WD_EditorObject> f7,
                     Func<WD_EditorObject,bool> c8, Action<WD_EditorObject> f8,
                                                    Action<WD_EditorObject> defaultFnc= null) {
        if(c1(obj)) f1(obj);
        else if(c2(obj)) f2(obj);
        else if(c3(obj)) f3(obj);
        else if(c4(obj)) f4(obj);
        else if(c5(obj)) f5(obj);
        else if(c6(obj)) f6(obj);
        else if(c7(obj)) f7(obj);
        else if(c8(obj)) f8(obj);
        else if(defaultFnc != null) defaultFnc(obj);
    }
    public void Case(WD_EditorObject obj, 
                     Func<WD_EditorObject,bool> c1, Action<WD_EditorObject> f1,
                     Func<WD_EditorObject,bool> c2, Action<WD_EditorObject> f2,
                     Func<WD_EditorObject,bool> c3, Action<WD_EditorObject> f3,
                     Func<WD_EditorObject,bool> c4, Action<WD_EditorObject> f4,
                     Func<WD_EditorObject,bool> c5, Action<WD_EditorObject> f5,
                     Func<WD_EditorObject,bool> c6, Action<WD_EditorObject> f6,
                     Func<WD_EditorObject,bool> c7, Action<WD_EditorObject> f7,
                     Func<WD_EditorObject,bool> c8, Action<WD_EditorObject> f8,
                     Func<WD_EditorObject,bool> c9, Action<WD_EditorObject> f9,
                                                    Action<WD_EditorObject> defaultFnc= null) {
        if(c1(obj)) f1(obj);
        else if(c2(obj)) f2(obj);
        else if(c3(obj)) f3(obj);
        else if(c4(obj)) f4(obj);
        else if(c5(obj)) f5(obj);
        else if(c6(obj)) f6(obj);
        else if(c7(obj)) f7(obj);
        else if(c8(obj)) f8(obj);
        else if(c8(obj)) f9(obj);
        else if(defaultFnc != null) defaultFnc(obj);
    }
    public void ForEachChild(WD_EditorObject parent, Action<WD_EditorObject> fnc) {
        if(parent == null) {
            TreeCache.ForEachChild((id) => { fnc(EditorObjects[id]); } );            
        }
        else {
            TreeCache.ForEachChild(parent.InstanceId, (id) => { fnc(EditorObjects[id]); } );            
        }
    }
    public void ForEach(Action<WD_EditorObject> fnc) {
        foreach(var obj in EditorObjects) {
            if(obj.IsValid) fnc(obj);
        }
    }
    public void ForEachRecursive(WD_EditorObject parent, Action<WD_EditorObject> fnc) {
        ForEachRecursiveDepthLast(parent, fnc);
    }
    public void ForEachRecursiveDepthLast(WD_EditorObject parent, Action<WD_EditorObject> fnc) {
        if(parent == null) {
            TreeCache.ForEachRecursiveDepthLast((id) => { fnc(EditorObjects[id]); });                                
        } else {
            TreeCache.ForEachRecursiveDepthLast(parent.InstanceId, (id) => { fnc(EditorObjects[id]); });                    
        }
    }
    public void ForEachRecursiveDepthFirst(WD_EditorObject parent, Action<WD_EditorObject> fnc) {
        if(parent == null) {
            TreeCache.ForEachRecursiveDepthFirst((id) => { fnc(EditorObjects[id]); });        
        } else {
            TreeCache.ForEachRecursiveDepthFirst(parent.InstanceId, (id) => { fnc(EditorObjects[id]); });                    
        }
    }
    public void ForEachChildRecursive(WD_EditorObject parent, Action<WD_EditorObject> fnc) {
        ForEachChildRecursiveDepthLast(parent, fnc);
    }
    public void ForEachChildRecursiveDepthLast(WD_EditorObject parent, Action<WD_EditorObject> fnc) {
        if(parent == null) {
            TreeCache.ForEachRecursiveDepthLast((id) => { fnc(EditorObjects[id]); });        
        } else {
            TreeCache.ForEachChildRecursiveDepthLast(parent.InstanceId, (id) => { fnc(EditorObjects[id]); });                    
        }
    }
    public void ForEachChildRecursiveDepthFirst(WD_EditorObject parent, Action<WD_EditorObject> fnc) {
        if(parent == null) {
            TreeCache.ForEachRecursiveDepthFirst((id) => { fnc(EditorObjects[id]); });                    
        } else {
            TreeCache.ForEachChildRecursiveDepthFirst(parent.InstanceId, (id) => { fnc(EditorObjects[id]); });        
        }
    }
    // ----------------------------------------------------------------------
    public bool IsChildOf(WD_EditorObject child, WD_EditorObject parent) {
        if(IsInvalid(child.ParentId)) return false;
        if(child.ParentId == parent.InstanceId) return true;
        return IsChildOf(EditorObjects[child.ParentId], parent);
    }

}
