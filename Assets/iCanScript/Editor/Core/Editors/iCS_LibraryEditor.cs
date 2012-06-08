using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class iCS_LibraryEditor : iCS_EditorWindow {
    // =================================================================================
    // Fields
    // ---------------------------------------------------------------------------------
    DSScrollView            myMainView;
	iCS_LibraryController   myController;
	Rect                    mySelectedAreaCache= new Rect(0,0,0,0);
	    
    // =================================================================================
    // Activation/Deactivation.
    // ---------------------------------------------------------------------------------
	public override void OnStorageChange() {
        if(IStorage == null) return;
        myController= new iCS_LibraryController(IStorage);
        myMainView= new DSScrollView(new RectOffset(0,0,0,0), false, true, true, myController.View);
		Repaint();
    }
    
	// =================================================================================
    // Display.
    // ---------------------------------------------------------------------------------
    void OnGUI() {
        iCS_EditorMgr.Update();
		if(IStorage == null) return;
		var toolbarRect= ShowToolbar();
        var frameArea= new Rect(0,toolbarRect.height,position.width,position.height-toolbarRect.height);
		myMainView.Display(frameArea);
		ProcessEvents(frameArea);
		// Make new selection visible
		if(mySelectedAreaCache != myController.SelectedArea) {
		    mySelectedAreaCache= myController.SelectedArea;
		    myMainView.MakeVisible(mySelectedAreaCache, frameArea);
		}
	}
    // ---------------------------------------------------------------------------------
	Rect ShowToolbar() {
		var toolbarRect= iCS_ToolbarUtility.BuildToolbar(position.width);
		string searchString= myController.SearchString ?? "";
		myController.SearchString= iCS_ToolbarUtility.Search(ref toolbarRect, 120.0f, searchString, 0, 0, true);
		return toolbarRect;
	}
    // ---------------------------------------------------------------------------------
    void OnInspectorUpdate() {
        Repaint();
    }
	// =================================================================================
    // Event processing
    // ---------------------------------------------------------------------------------
    void ProcessEvents(Rect frameArea) {
     	Vector2 mousePosition= Event.current.mousePosition;
        var selected= myController.Selected;
		switch(Event.current.type) {
            case EventType.MouseDrag: {
                switch(Event.current.button) {
                    case 0: { // Left mouse button
                        StartDragAndDrop(selected);                            
                        Event.current.Use();
                        break;
                    }
                }
                break;
            }
            case EventType.ScrollWheel: {
                break;
            }
            case EventType.MouseDown: {
                var mouseInScreenPoint= GUIUtility.GUIToScreenPoint(mousePosition);
                var areaInScreenPoint= GUIUtility.GUIToScreenPoint(new Vector2(frameArea.x, frameArea.y));
                var areaInScreenPosition= new Rect(areaInScreenPoint.x, areaInScreenPoint.y, frameArea.width, frameArea.height);
                myController.MouseDownOn(null, mouseInScreenPoint, areaInScreenPosition);
                Event.current.Use();
                // Move keyboard focus to this window.
                Focus();
				break;
			}
            case EventType.MouseUp: {
				break;
			}
			case EventType.KeyDown: {
				var ev= Event.current;
				if(!ev.isKey) break;
                switch(ev.keyCode) {
                    // Tree navigation
                    case KeyCode.UpArrow: {
                        myController.SelectPrevious();
                        ev.Use();
                        break;
                    }
                    case KeyCode.DownArrow: {
                        myController.SelectNext();
                        ev.Use();
                        break;
                    }
                    // Fold/Unfold toggle
                    case KeyCode.Return: {
                        myController.ToggleFoldUnfoldSelected();
                        ev.Use();
                        break;
                    }
                }
                switch(ev.character) {
                    // Fold/Unfold.
                    case '+': {
                        myController.UnfoldSelected();
                        ev.Use();
                        break;
                    }
                    case '-': {
                        myController.FoldSelected();
                        ev.Use();
                        break;
                    }
                }
                break;
			}
        }   
    }

	// =================================================================================
    // Drag events.
    // ---------------------------------------------------------------------------------
    void StartDragAndDrop(iCS_LibraryController.Node node) {
        if(node == null) return;
        // Build drag object.
        GameObject go= new GameObject(node.Name);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.AddComponent("iCS_Library");
        iCS_Library library= go.GetComponent<iCS_Library>();
        iCS_IStorage iStorage= new iCS_IStorage(library);
        CreateInstance(node, iStorage);
        // Fill drag info.
        DragAndDrop.PrepareStartDrag();
        DragAndDrop.objectReferences= new UnityEngine.Object[1]{go};
        DragAndDrop.StartDrag(node.Name);
        iCS_AutoReleasePool.AutoRelease(go, 60f);
    }
    // ---------------------------------------------------------------------------------
    void CreateInstance(iCS_LibraryController.Node node, iCS_IStorage iStorage) {
        if(node.Type == iCS_LibraryController.NodeTypeEnum.Company) {
            CreateModule(node.Name, iStorage);        
            return;
        }
        if(node.Type == iCS_LibraryController.NodeTypeEnum.Package) {
            CreateModule(node.Name, iStorage);        
            return;
        }
        if(node.Type == iCS_LibraryController.NodeTypeEnum.Class) {
            CreateClassModule(node.Desc.ClassType, iStorage);        
            return;
        }
        if(node.Type == iCS_LibraryController.NodeTypeEnum.Field) {
            CreateMethod(node.Desc, iStorage);        
            return;
        }
        if(node.Type == iCS_LibraryController.NodeTypeEnum.Property) {
            CreateMethod(node.Desc, iStorage);        
            return;
        }
        if(node.Type == iCS_LibraryController.NodeTypeEnum.Constructor) {
            CreateMethod(node.Desc, iStorage);        
            return;
        }
        if(node.Type == iCS_LibraryController.NodeTypeEnum.Method) {
            CreateMethod(node.Desc, iStorage);        
            return;
        }
    }
    // ======================================================================
    // Creation Utilities
    // ---------------------------------------------------------------------------------
    iCS_EditorObject CreateModule(string name, iCS_IStorage iStorage) {
        return iStorage.CreateModule(-1, Vector2.zero, name);
    }
    // ---------------------------------------------------------------------------------
    iCS_EditorObject CreateClassModule(Type classType, iCS_IStorage iStorage) {
        return iStorage.CreateModule(-1, Vector2.zero, null, iCS_ObjectTypeEnum.Module, classType);
    }
    // ---------------------------------------------------------------------------------
    iCS_EditorObject CreateMethod(iCS_ReflectionDesc desc, iCS_IStorage iStorage) {
        return iStorage.CreateMethod(-1, Vector2.zero, desc);            
    }    
}
