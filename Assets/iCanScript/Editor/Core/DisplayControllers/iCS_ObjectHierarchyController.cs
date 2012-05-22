using UnityEngine;
using UnityEditor;
using System.Collections;

public class iCS_ObjectHierarchyController : DSTreeViewDataSource {
    // =================================================================================
    // Fields
    // ---------------------------------------------------------------------------------
	iCS_EditorObject	    myTarget    = null;
	iCS_IStorage		    myStorage   = null;
	iCS_EditorObject		myCursor    = null;
	DSTreeView				myTreeView  = null;
	float                   myFoldOffset= 0;
	
    // =================================================================================
    // Properties
    // ---------------------------------------------------------------------------------
	public DSView 			View 	{ get { return myTreeView; }}
	public iCS_EditorObject Target	{ get { return myTarget; }}
	
    // =================================================================================
    // Initialization
    // ---------------------------------------------------------------------------------
	public iCS_ObjectHierarchyController(iCS_EditorObject target, iCS_IStorage storage) {
		myTarget= target;
		myStorage= storage;
		myCursor= target;
		myTreeView = new DSTreeView(new RectOffset(0,0,0,0), false, this, 16);
        var emptySize= EditorStyles.foldout.CalcSize(new GUIContent(""));
		myFoldOffset= emptySize.x;
	}
	
	// =================================================================================
    // TreeViewDataSource
    // ---------------------------------------------------------------------------------
	public void	Reset() { myCursor= myTarget; }
	public bool	MoveToNext() {
		if(myStorage == null) return false;
		if(MoveToFirstChild()) return true;
		if(MoveToNextSibling()) return true;
		do {
			myCursor= myStorage.GetParent(myCursor);
			if(myCursor == null) return false;
			if(myTarget != null && !myStorage.IsChildOf(myCursor, myTarget)) {
				return false;
			}
		} while(!MoveToNextSibling());
		return true;
	}
    // ---------------------------------------------------------------------------------
	public bool	MoveToNextSibling() {
		if(myCursor == null || myCursor == myTarget) return false;
		bool takeNext= false;
		iCS_EditorObject parent= myStorage.GetParent(myCursor);
        if(parent == null) return false;
		return myStorage.ForEachChild(parent,
			c=> {
				if(takeNext) {
					myCursor= c;
					return true;
				}
				if(c == myCursor) {
					takeNext= true;
				}
				return false;
			}
		);
	}
    // ---------------------------------------------------------------------------------
	public bool MoveToParent() {
		if(myStorage == null || myCursor == null) return false;
		if(myStorage.EditorObjects.Count == 0) return false;
		myCursor= myStorage.GetParent(myCursor);
		return myCursor != myTarget;
	}
	// ---------------------------------------------------------------------------------
	public bool	MoveToFirstChild() {
		if(myStorage == null) return false;
        if(myStorage.EditorObjects.Count == 0) return false;
        if(myCursor == null) {
            myCursor= myStorage.EditorObjects[0];
            return true;
        }
		if(myStorage.NbOfChildren(myCursor) == 0) return false;
		myStorage.ForEachChild(myCursor, c=> { myCursor= c; return true; });
		return true;
	}
    // ---------------------------------------------------------------------------------
	public Vector2	CurrentObjectDisplaySize() {
		if(myStorage == null) return Vector2.zero;
        var nameSize= EditorStyles.label.CalcSize(new GUIContent(myCursor.Name));
        return new Vector2(myFoldOffset+16.0f+nameSize.x, nameSize.y);
	}
    // ---------------------------------------------------------------------------------
	public bool	DisplayCurrentObject(Rect displayArea, bool foldout) {
		if(myStorage == null) return true;
		bool result= ShouldUseFoldout() ? EditorGUI.Foldout(displayArea, foldout, "") : false;
        var content= GetContent();
        var pos= new Rect(myFoldOffset+displayArea.x, displayArea.y, displayArea.width-myFoldOffset, displayArea.height);
	    GUI.Label(pos, content.image);
	    GUI.Label(new Rect(pos.x+16.0f, pos.y, pos.width-16.0f, pos.height), content.text);
		return result;
	}
    // ---------------------------------------------------------------------------------
	public object	CurrentObjectKey() {
		return myCursor;
	}
    // ---------------------------------------------------------------------------------
    GUIContent GetContent() {
        Texture2D icon= null;
        if(myCursor.IsFunction) {
            icon= iCS_TextureCache.GetIcon(iCS_Config.GuiAssetPath+"/"+iCS_EditorStrings.FunctionHierarchyIcon, myStorage);            
        } else if(myCursor.IsState || myCursor.IsStateChart) {
            icon= iCS_TextureCache.GetIcon(iCS_Config.GuiAssetPath+"/"+iCS_EditorStrings.StateHierarchyIcon, myStorage);                        
        } else if(myCursor.IsClassModule) {
            icon= iCS_TextureCache.GetIcon(iCS_Config.GuiAssetPath+"/"+iCS_EditorStrings.ClassHierarchyIcon, myStorage);                            
        } else if(myCursor.IsNode) {
            icon= iCS_TextureCache.GetIcon(iCS_Config.GuiAssetPath+"/"+iCS_EditorStrings.ModuleHierarchyIcon, myStorage);            
        } else if(myCursor.IsDataPort) {
            if(myCursor.IsInputPort) {
                icon= iCS_TextureCache.GetIcon(iCS_Config.GuiAssetPath+"/"+iCS_EditorStrings.InPortHierarchyIcon, myStorage);                
            } else {
                icon= iCS_TextureCache.GetIcon(iCS_Config.GuiAssetPath+"/"+iCS_EditorStrings.OutPortHierarchyIcon, myStorage);                                    
            }
        }
        return new GUIContent(myCursor.Name, icon); 
    }
    // ---------------------------------------------------------------------------------
    bool ShouldUseFoldout() {
        if(myStorage == null) return false;
        return myCursor.IsNode;
    }
}
