using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
// This non-persistante class is used to edit the WD_Behaviour.
public class WD_Editor : EditorWindow {
    // ======================================================================
    // PROPERTIES
    // ----------------------------------------------------------------------
    WD_Behaviour        Graph        = null;
    WD_EditorObjectMgr  EditorObjects= null;
    WD_EditorObject     RootNode     = null;
	WD_Inspector        Inspector    = null;
    WD_EditorObject     DisplayRoot  = null;

    // ----------------------------------------------------------------------
    public  WD_Mouse           Mouse           = null;
    private WD_Graphics        Graphics        = null;
    public  WD_ScrollView      ScrollView      = null;
    
    // ----------------------------------------------------------------------
    bool    IsRootNodeSelected  { get { return SelectedObject.IsRuntimeA<WD_RootNode>(); }}
    bool    IsNodeSelected      { get { return SelectedObject.IsRuntimeA<WD_Node>(); }}
    bool    IsPortSelected      { get { return SelectedObject.IsRuntimeA<WD_Port>(); }}
    
    // ----------------------------------------------------------------------
    WD_EditorObject DragObject          = null;
    Vector2         DragStartPosition   = Vector2.zero;
    bool            IsDragEnabled       = true;
    bool            IsDragging          { get { return DragObject != null; }}


    // ======================================================================
    // ACCESSORS
	// ----------------------------------------------------------------------
    WD_EditorObject SelectedObject {
        get { return mySelectedObject; }
        set { Inspector.SelectedObject= mySelectedObject= value; }
    }
    WD_EditorObject mySelectedObject= null;

    // ======================================================================
    // INITIALIZATION
	// ----------------------------------------------------------------------
    // Prepares the editor for editing a graph.  Not that the graph to edit
    // is not configured at this point.  We must wait for an activate from
    // the graph inspector to know which graph to edit. 
	void OnEnable() {        
		// Tell Unity we want to be informed of move drag events
		wantsMouseMove= true;

        // Create worker objects.
        Mouse           = new WD_Mouse(this);
        Graphics        = new WD_Graphics();
        ScrollView      = new WD_ScrollView();
	}

	// ----------------------------------------------------------------------
    // Releases all resources used by the WD_Behaviour editor.
    void OnDisable() {
        // Release all worker objects.
        Mouse           = null;
        Graphics        = null;
        ScrollView      = null;
    }
    
    // ----------------------------------------------------------------------
    // Activates the editor and initializes all Graph shared variables.
	public void Activate(WD_Behaviour graph, WD_Inspector _inspector) {
        Graph= graph;
        EditorObjects= graph.EditorObjects;
        RootNode= EditorObjects.GetRootNode();
        DisplayRoot= RootNode;
        Inspector= _inspector;
    }
    
    // ----------------------------------------------------------------------
    public void Deactivate() {
        Inspector    = null;
		DisplayRoot  = null;
		RootNode     = null;
		EditorObjects= null;
		Graph        = null;
    }

	// ----------------------------------------------------------------------
    // Assures proper initialization and returns true if editor is ready
    // to execute.
	public bool IsInitialized() {
        // Nothing to do if we don't have a Graph to edit...
		if(Graph == null ||
		   EditorObjects == null ||
		   RootNode == null ||
		   Inspector == null ||
           DisplayRoot == null) {
               return false;
        }
        
		// Don't run if graphic sub-system did not initialise.
		if(WD_Graphics.IsInitialized == false) {
            WD_Graphics.Init();
			return false;
		}
		
        return true;
	}


    // ======================================================================
    // UPDATE FUNCTIONALITY
	// ----------------------------------------------------------------------
    static int refreshCnt= 0;
	void Update() {
		// Force a repaint to allow for snappy controls.
        if((++refreshCnt & 7) == 0) {
    		Repaint();            
        }
	}
	
	// ----------------------------------------------------------------------
	// User GUI function.
	void OnGUI() {
		// Don't do start editor if not properly initialized.
		if( !IsInitialized() ) return;
       	
        // Load Editor Skin.
        GUI.skin= EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
        
        // Update scroll view.
        Inf.DebugWarning(ScrollView == null, "ScrollView not set");
        Inf.DebugWarning(Graph == null, "Graph is not set");
        ScrollView.Update(position, Graph.EditorObjects.GetPosition(DisplayRoot));
        
		// Draw editor widgets.
		DrawEditorWidgets();
		
        // Draw Graph.
        DrawGraph();

		// Compute new EMouse position.
		Mouse.Update();

        // Process user inputs
        ProcessEvents();
        
        // Process new accumulated commands.
        if(Graph.IsDirty) {
            Graph.IsDirty= false;
            Undo.RegisterUndo(Graph, "WarpDrive");
            EditorUtility.SetDirty(Graph);
        }
	}

    // ======================================================================
    // EDITOR WINDOW MAIN LAYOUT
	// ----------------------------------------------------------------------
	// Draws all editor widgets
	void DrawEditorWidgets() {
        DrawEditorToolbar();
	}

	// ----------------------------------------------------------------------
	void DrawEditorToolbar() {
//    	GUILayout.BeginHorizontal(EditorStyles.toolbar);
//    	
//        // Display root node selection.
//        string selected= SelectedObject != null ? SelectedObject.Name : "(No Object Selected)";
//        EditorGUILayout.TextField("Selected Node= ", selected);
//        
//		// Show display depth configuration.
//    	EditorGUILayout.Separator();
//		
//		GUILayout.EndHorizontal();
	}
    
	// ----------------------------------------------------------------------
    float UsableWindowWidth() {
        return position.width-2*WD_EditorConfig.EditorWindowGutterSize;
    }
    
	// ----------------------------------------------------------------------
    float UsableWindowHeight() {
        return position.height-2*WD_EditorConfig.EditorWindowGutterSize+WD_EditorConfig.EditorWindowToolbarHeight;
    }
    

#region User Interaction
    // ======================================================================
    // USER INTERACTIONS
	// ----------------------------------------------------------------------
    public enum UserCommandStateEnum { Idle, Dragging, LeftButtonMenu, RightButtonMenu };
    public UserCommandStateEnum UserCommandState= UserCommandStateEnum.Idle;
    WD_Mouse.ButtonStateEnum   PreviousLeftButtonState= WD_Mouse.ButtonStateEnum.Idle;    
    public void ProcessEvents() {
        // Update the inspector object.
        DetermineSelectedObject();

        // Process left button state.
        switch(Mouse.LeftButtonState) {
            case WD_Mouse.ButtonStateEnum.Idle:
                if(PreviousLeftButtonState == WD_Mouse.ButtonStateEnum.Dragging) EndDragging();
                break;
            case WD_Mouse.ButtonStateEnum.SingleClick:
                break;
            case WD_Mouse.ButtonStateEnum.DoubleClick:
                ProcessMainMenu(Mouse.LeftButtonDownPosition);
                break;
            case WD_Mouse.ButtonStateEnum.Dragging:
                ProcessDragging();
                break;
        }
        PreviousLeftButtonState= Mouse.LeftButtonState;

        // Process right button state.
        switch(Mouse.RightButtonState) {
            case WD_Mouse.ButtonStateEnum.SingleClick:
                ProcessMainMenu(Mouse.RightButtonDownPosition);
                break;
        }        
    }
    
	// ----------------------------------------------------------------------
    void ProcessMainMenu(Vector2 position) {
        WD_EditorObject selectedObject= GetObjectAtScreenPosition(position);
        if(selectedObject == null) return;
        WD_MenuContext context= WD_MenuContext.CreateInstance(selectedObject, position, ScrollView.ScreenToGraph(position), Graph);
        string menuName= "CONTEXT/"+WD_EditorConfig.ProductName;
        if(selectedObject.IsRuntimeA<WD_RootNode>()) menuName+= "/RootNode";
        else if(selectedObject.IsRuntimeA<WD_StateChart>()) menuName+= "/StateChart";
        else if(selectedObject.IsRuntimeA<WD_State>()) menuName+= "/State";
        else if(selectedObject.IsRuntimeA<WD_Module>()) menuName+= "/Module";
        else if(selectedObject.IsRuntimeA<WD_Function>()) menuName+= "/Function";
        EditorUtility.DisplayPopupMenu (new Rect (position.x,position.y,0,0), menuName, new MenuCommand(context));
    }
    
	// ----------------------------------------------------------------------
    void ProcessDragging() {
        // Return if dragging is not enabled.
        if(!IsDragEnabled) return;

        // Process dragging start.
        WD_EditorObject port;
        WD_EditorObject node;
        Vector2 MousePosition= ScrollView.ScreenToGraph(Mouse.Position);
        if(DragObject == null) {
            Vector2 pos= ScrollView.ScreenToGraph(Mouse.LeftButtonDownPosition);
            port= EditorObjects.GetPortAt(pos);
            if(port != null) {
                DragObject= port;
                DragStartPosition= new Vector2(port.LocalPosition.x, port.LocalPosition.y);
                port.IsBeingDragged= true;
            }
            else {
                node= EditorObjects.GetNodeAt(pos);                
                if(node != null) {
                    DragObject= node;
                    Rect position= EditorObjects.GetPosition(node);
                    DragStartPosition= new Vector2(position.x, position.y);                                                    
                }
                else {
                    // Disable dragging since mouse is not over Node or Port.
                    IsDragEnabled= false;
                    DragObject= null;
                    return;
                }
            }
        }

        // Compute new object position.
        Vector2 delta= MousePosition - ScrollView.ScreenToGraph(Mouse.LeftButtonDownPosition);
        if(DragObject.IsRuntimeA<WD_Port>()) {
            port= DragObject;
            Vector2 newLocalPos= DragStartPosition+delta;
            port.LocalPosition.x= newLocalPos.x;
            port.LocalPosition.y= newLocalPos.y;
            port.IsDirty= true;
            if(!EditorObjects.IsNearParent(port)) {
            /*
                TODO : create a temporary port to show new connection.
            */    
            }
        }
        if(DragObject.IsRuntimeA<WD_Node>()) {
            node= DragObject;
            EditorObjects.MoveTo(node, DragStartPosition+delta);
            node.IsDirty= true;                        
        }
    }    

	// ----------------------------------------------------------------------
    void EndDragging() {
        if(DragObject.IsRuntimeA<WD_Port>()) {
            WD_EditorObject port= DragObject;
            port.IsBeingDragged= false;
            // Verify for a new connection.
            if(!VerifyNewConnection(port)) {
                // Verify for disconnection.
                if(!EditorObjects.IsNearParent(port)) {
                    if(port.IsRuntimeA<WD_FunctionPort>()) {
                        (EditorObjects.GetRuntimeObject(port) as WD_FunctionPort).Disconnect();
                    }
                    port.LocalPosition.x= DragStartPosition.x;
                    port.LocalPosition.y= DragStartPosition.y;
                }                    
                else {
                    // Assume port relocation.
                    EditorObjects.SnapToParent(port);
                    EditorObjects.Layout(EditorObjects[port.ParentId]);
                }
            }
            port.IsDirty= true;
        }
    
        // Reset dragging state.
        DragObject= null;
        IsDragEnabled= true;
    }
#endregion User Interaction
    
	// ----------------------------------------------------------------------
    // Manages the object selection.
    WD_EditorObject DetermineSelectedObject() {
        // Object selection is performed on left mouse button only.
        if(!Mouse.IsLeftButtonDown && !Mouse.IsRightButtonDown) return SelectedObject;
        WD_EditorObject newSelected= GetObjectAtMousePosition();
        SelectedObject= newSelected;
        return SelectedObject;
    }

	// ----------------------------------------------------------------------
    // Returns the object at the given mouse position.
    public WD_EditorObject GetObjectAtMousePosition() {
        return GetObjectAtScreenPosition(Mouse.Position);
    }

	// ----------------------------------------------------------------------
    // Returns the object at the given mouse position.
    public WD_EditorObject GetObjectAtScreenPosition(Vector2 _screenPos) {
        Vector2 graphPosition= ScrollView.ScreenToGraph(_screenPos);
        WD_EditorObject port= EditorObjects.GetPortAt(graphPosition);
        if(port != null) return port;
        WD_EditorObject node= EditorObjects.GetNodeAt(graphPosition);                
        if(node != null) return node;
        return null;
    }
        
	// ----------------------------------------------------------------------
    bool VerifyNewConnection(WD_EditorObject port) {
        // No new connection if no overlapping port found.
        WD_EditorObject overlappingPort= EditorObjects.GetOverlappingPort(port);
        if(overlappingPort == null) return false;
        
        // Connect function & modules ports together.
        if(port.IsDataPort && overlappingPort.IsDataPort) {            
            WD_EditorObject inPort = port.IsInputPort             ? port : overlappingPort;
            WD_EditorObject outPort= overlappingPort.IsOutputPort ? overlappingPort : port;
            if(inPort != outPort) {
                Type inPortType = WD_Reflection.GetPortFieldType(inPort, EditorObjects[inPort.ParentId]);
                Type outPortType= WD_Reflection.GetPortFieldType(outPort, EditorObjects[outPort.ParentId]);
                Type connectionType= WD_TypeSystem.GetBestUpConversionType(inPortType, outPortType);
                if(connectionType != null) {
                    // No conversion needed.
                    if(inPortType == outPortType) {
                        EditorObjects.SetSource(inPort, outPort);                       
                    }
                    // A conversion is required.
                    else {
                        Debug.Log("A conversion node is required.");
                    }
                }
            }
            return true;
        }

        // Connect transition port together.
        if(port.IsTransitionPort && overlappingPort.IsTransitionPort) {
            return true;
        }
        
        Debug.LogWarning("Trying to connect incompatible port types: "+port.TypeName+"<=>"+overlappingPort.TypeName);
        return true;
    }
    
    
    // ======================================================================
    // NODE GRAPH DISPLAY
	// ----------------------------------------------------------------------
    void DrawGrid() {
        Graphics.DrawGrid(position,
                          Graph.Preferences.Grid.BackgroundColor,
                          Graph.Preferences.Grid.GridColor,
                          Graph.Preferences.Grid.GridSpacing,
                          ScrollView.ScreenToGraph(Vector2.zero));
    }
    
	// ----------------------------------------------------------------------
	void DrawGraph () {
        // Perform layout of modified nodes.
        EditorObjects.ForEachRecursiveDepthLast(DisplayRoot,
            (obj)=> {
                if(obj.IsDirty) {
                    EditorObjects.Layout(obj);
                }
            }
        );            
        
        // Draw editor grid.
        DrawGrid();
        
        // Draw editor window.
        ScrollView.Begin();
    	DrawNodes();
        DrawConnections();            
        ScrollView.End();
	}

	// ----------------------------------------------------------------------
    void DrawNodes() {
        // Display node starting from the root node.
        EditorObjects.ForEachRecursiveDepthLast<WD_Node>(DisplayRoot,
            (node)=> {
                Graphics.DrawNode(node, SelectedObject, Graph);
            }
        );
    }	
	
	// ----------------------------------------------------------------------
    private void DrawConnections() {
        // Display all connections.
        EditorObjects.ForEachChildRecursive<WD_Port>(DisplayRoot, (port)=> { Graphics.DrawConnection(port, SelectedObject, Graph); } );

        // Display ports.
        EditorObjects.ForEachChildRecursive<WD_Port>(DisplayRoot, (port)=> { Graphics.DrawPort(port, SelectedObject, Graph); } );
    }

}
