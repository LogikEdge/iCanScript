using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class iCS_ProjectController : DSTreeViewDataSource {
    // =================================================================================
    // Types
    // ---------------------------------------------------------------------------------
    public enum NodeTypeEnum { Root, Company, Package, Class, Constructor, Field, Property, Method};
    public class Node {
        public NodeTypeEnum        Type;
        public string              Name;
        public iCS_ReflectionDesc  Desc;
        public Node(NodeTypeEnum type, string name, iCS_ReflectionDesc desc) {
            Type= type;
            Name= name;
            Desc= desc;
        }
    };
    
    // =================================================================================
    // Fields
    // ---------------------------------------------------------------------------------
    Node                        mySelected     = null;
	DSTreeView		    		myTreeView     = null;
	float               		myFoldOffset   = 0;
	bool                		myNameEdition  = false;
	string              		mySearchString = null;
	Prelude.Tree<Node>	        myTree		   = null;
    // Used to move selection up/down
    Node                        myLastDisplayed  = null;
    int                         myChangeSelection= 0;
    // Used to iterate through content
	Stack<Prelude.Tree<Node>>   myIterStackNode    = null;
	Stack<int>					myIterStackChildIdx= null;
	
    // =================================================================================
    // Properties
    // ---------------------------------------------------------------------------------
	public DSView 		View 	     { get { return myTreeView; }}
	public Node 		Selected     { get { return mySelected; } set { mySelected= value; }}
	public bool         IsSelected   { get { return IterNode != null ? IterNode.Value == Selected : false; }}
	public bool         NameEdition  { get { return myNameEdition; } set { myNameEdition= value; }}
	public string		SearchString { get { return mySearchString; } set { if(mySearchString != value) { mySearchString= value; BuildTree(); }}}

	Prelude.Tree<Node>  IterNode	 { get { return myIterStackNode.Count != 0 ? myIterStackNode.Peek() : null; }}
	int					IterChildIdx { get { return myIterStackChildIdx.Count  != 0 ? myIterStackChildIdx.Peek()  : 0; }}
	Node				IterValue	 { get { return IterNode != null ? IterNode.Value : null; }}
	
    // =================================================================================
    // Constants
    // ---------------------------------------------------------------------------------
    const float kIconWidth= 16.0f;
    const float kLabelSpacer= 4.0f;
    
    // =================================================================================
    // Initialization
    // ---------------------------------------------------------------------------------
	public iCS_ProjectController() {
		BuildTree();
		myTreeView = new DSTreeView(new RectOffset(0,0,0,0), false, this, 16);
		myIterStackNode= new Stack<Prelude.Tree<Node>>();
		myIterStackChildIdx = new Stack<int>();
	}
	
	// =================================================================================
    // Filter & reorder.
    // ---------------------------------------------------------------------------------
    void BuildTree() {
        // Build filter list of object...
        var allFunctions= iCS_DataBase.AllFunctions();
        var filterFlags= Prelude.map(o=> FilterIn(o), allFunctions);
		// Build tree and sort it elements.
		myTree= BuildTreeNode(allFunctions, filterFlags);
    }
	Prelude.Tree<Node> BuildTreeNode(List<iCS_ReflectionDesc> functions, List<bool> filterFlags) {
        if(functions.Count == 0) return null;
		Prelude.Tree<Node> tree= new Prelude.Tree<Node>(new Node(NodeTypeEnum.Root, "Root", null));
        foreach(var desc in functions) {
            var parentTree= GetParentTree(desc, tree);
            Node toAdd= null;
            if(desc.IsField) {
                toAdd= new Node(NodeTypeEnum.Field, desc.FieldName, desc);
            } else if(desc.IsProperty) {
                toAdd= new Node(NodeTypeEnum.Property, desc.PropertyName, desc);
            } else if(desc.IsConstructor) {
                toAdd= new Node(NodeTypeEnum.Constructor, iCS_Types.TypeName(desc.ClassType), desc);
            } else if(desc.IsMethod) {
                toAdd= new Node(NodeTypeEnum.Method, desc.MethodName, desc);                
            }
            if(toAdd != null) {
                parentTree.AddChild(toAdd);
            }
        }
        Sort(tree);
		return tree;
	}
    int FindInTreeChildren(string name, Prelude.Tree<Node> tree) {
        var children= tree.Children;
        if(children == null) return -1;
        for(int i= 0; i < children.Count; ++i) {
            if(children[i].Value.Name == name) return i;
        }
        return -1;
    }
    Prelude.Tree<Node> GetParentTree(iCS_ReflectionDesc desc, Prelude.Tree<Node> tree) {
        if(!iCS_Strings.IsEmpty(desc.Company)) {
            var idx= FindInTreeChildren(desc.Company, tree);
            if(idx < 0) {
                tree.AddChild(new Node(NodeTypeEnum.Company, desc.Company, desc));
                idx= FindInTreeChildren(desc.Company, tree);
            }
            tree= tree.Children[idx];
        }
        if(!iCS_Strings.IsEmpty(desc.Package)) {
            var idx= FindInTreeChildren(desc.Package, tree);
            if(idx < 0) {
                tree.AddChild(new Node(NodeTypeEnum.Package, desc.Package, desc));
                idx= FindInTreeChildren(desc.Package, tree);
            }
            tree= tree.Children[idx];            
        }
        string className= iCS_Types.TypeName(desc.ClassType);
        if(!iCS_Strings.IsEmpty(className)) {
            var idx= FindInTreeChildren(className, tree);
            if(idx < 0) {
                tree.AddChild(new Node(NodeTypeEnum.Package, className, desc));
                idx= FindInTreeChildren(className, tree);
            }
            tree= tree.Children[idx];            
        }
        return tree;
    }
    void Sort(Prelude.Tree<Node> tree) {
        if(tree == null) return;
        var children= tree.Children;
        if(children != null) {
            tree.Sort(SortComparaison);
            foreach(var child in children) {
                Sort(child);
            }
        }
    }
	int SortComparaison(Node x, Node y) {
        if(x.Type == NodeTypeEnum.Field && y.Type != NodeTypeEnum.Field) return -1;
        if(x.Type != NodeTypeEnum.Field && y.Type == NodeTypeEnum.Field) return 1;
        if(x.Type == NodeTypeEnum.Property && y.Type != NodeTypeEnum.Property) return -1;
        if(x.Type != NodeTypeEnum.Property && y.Type == NodeTypeEnum.Property) return 1;
        if(x.Type == NodeTypeEnum.Constructor && y.Type != NodeTypeEnum.Constructor) return -1;
        if(x.Type != NodeTypeEnum.Constructor && y.Type == NodeTypeEnum.Constructor) return 1;
        if(x.Type == NodeTypeEnum.Method && y.Type != NodeTypeEnum.Method) return -1;
        if(x.Type != NodeTypeEnum.Method && y.Type == NodeTypeEnum.Method) return 1;
        if(x.Type == NodeTypeEnum.Class && y.Type != NodeTypeEnum.Class) return -1;
        if(x.Type != NodeTypeEnum.Class && y.Type == NodeTypeEnum.Class) return 1;
        if(x.Type == NodeTypeEnum.Package && y.Type != NodeTypeEnum.Package) return -1;
        if(x.Type != NodeTypeEnum.Package && y.Type == NodeTypeEnum.Package) return 1;
        if(x.Type == NodeTypeEnum.Company && y.Type != NodeTypeEnum.Company) return -1;
        if(x.Type != NodeTypeEnum.Company && y.Type == NodeTypeEnum.Company) return 1;
		return String.Compare(x.Name, y.Name);
	}
    // ---------------------------------------------------------------------------------
    bool FilterIn(iCS_ReflectionDesc desc) {
        if(desc == null) return false;
        if(iCS_Strings.IsEmpty(mySearchString)) return true;
        if(desc.DisplayName.ToUpper().IndexOf(mySearchString.ToUpper()) != -1) return true;
        if(!iCS_Strings.IsEmpty(desc.Package) && desc.Package.ToUpper().IndexOf(mySearchString.ToUpper()) != -1) return true;
        if(!iCS_Strings.IsEmpty(desc.Company) && desc.Company.ToUpper().IndexOf(mySearchString.ToUpper()) != -1) return true;
        return false;
    }
    

	// =================================================================================
    // TreeViewDataSource
    // ---------------------------------------------------------------------------------
	public void	Reset() {
		myIterStackNode.Clear();
		myIterStackChildIdx.Clear();
		if(myTree != null) {
			myIterStackNode.Push(myTree);
			myIterStackChildIdx.Push(0);
		}
	}
	public void BeginDisplay() { EditorGUIUtility.LookLikeControls(); }
	public void EndDisplay() {}
	public bool	MoveToNext() {
		if(myTree == null || myIterStackNode.Count == 0) return false;
		if(MoveToFirstChild()) return true;
		if(MoveToNextSibling()) return true;
		do {
			if(!MoveToParent()) return false;			
		} while(!MoveToNextChild());
		return true;
	}
    // ---------------------------------------------------------------------------------
	public bool	MoveToNextSibling() {
		if(myTree == null || myIterStackNode.Count == 0) return false;
		if(myIterStackNode.Count == 1) return false;
		var savedNode= myIterStackNode.Pop();
		var savedIdx= myIterStackChildIdx.Pop();
		if(!MoveToNextChild()) {
			myIterStackNode.Push(savedNode);
			myIterStackChildIdx.Push(savedIdx);
			return false;
		}
		return true;
	}
    // ---------------------------------------------------------------------------------
	public bool MoveToParent() {
		if(myTree == null || myIterStackNode.Count == 0) return false;
		myIterStackNode.Pop();
		myIterStackChildIdx.Pop();
		return myIterStackNode.Count != 0;
	}
	// ---------------------------------------------------------------------------------
	public bool	MoveToFirstChild() {
		if(myTree == null || myIterStackNode.Count == 0) return false;
		var node= IterNode;
		if(node == null || node.Children == null) return false;
		myIterStackChildIdx.Pop();
		myIterStackChildIdx.Push(1);
		if(node.Children.Count < 1) return false;
		myIterStackNode.Push(node.Children[0]);
		myIterStackChildIdx.Push(0);
		return true;
	}
	// ---------------------------------------------------------------------------------
	public bool	MoveToNextChild() {
		if(myTree == null || myIterStackNode.Count == 0) return false;
		var node= IterNode;
		if(node == null || node.Children == null) return false;
		int idx= myIterStackChildIdx.Pop();
		myIterStackChildIdx.Push(idx+1);
		if(idx >= node.Children.Count) return false;
		myIterStackNode.Push(node.Children[idx]);
		myIterStackChildIdx.Push(0);
		return true;
	}

    // ---------------------------------------------------------------------------------
	public Vector2	CurrentObjectDisplaySize() {
		if(myFoldOffset == 0) {
            var emptySize= EditorStyles.foldout.CalcSize(new GUIContent(""));
    		myFoldOffset= emptySize.x;
		}
        var nameSize= EditorStyles.label.CalcSize(new GUIContent(IterValue.Name));
        return new Vector2(myFoldOffset+kIconWidth+kLabelSpacer+nameSize.x, nameSize.y);
//        return EditorStyles.foldout.CalcSize(new GUIContent(GetContent()));
	}
    // ---------------------------------------------------------------------------------
	public bool	DisplayCurrentObject(Rect displayArea, bool foldout, Rect frameArea) {
        // Show selected outline.
        GUIStyle labelStyle= EditorStyles.label;
		if(IsSelected) {
            Color selectionColor= EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).settings.selectionColor;
            iCS_Graphics.DrawBox(frameArea, selectionColor, selectionColor, new Color(1.0f, 1.0f, 1.0f, 0.65f));
            labelStyle= EditorStyles.whiteLabel;
		}
		bool result= ShouldUseFoldout() ? EditorGUI.Foldout(new Rect(displayArea.x, displayArea.y, myFoldOffset, displayArea.height), foldout, "") : false;
        var content= GetContent();
        var pos= new Rect(myFoldOffset+displayArea.x, displayArea.y, displayArea.width-myFoldOffset, displayArea.height);
	    GUI.Label(pos, content.image);
        pos= new Rect(pos.x+kIconWidth+kLabelSpacer, pos.y-1f, pos.width-(kIconWidth+kLabelSpacer), pos.height);  // Move label up a bit.
        if(NameEdition && IsSelected) {
    	    IterValue.Name= GUI.TextField(new Rect(pos.x, pos.y, frameArea.xMax-pos.x, pos.height+2.0f), IterValue.Name);            
        } else {
    	    GUI.Label(pos, content.text, labelStyle);            
        }
        ProcessChangeSelection();
		return result;
//        bool result= false;
//        if(ShouldUseFoldout()) {
//            result= EditorGUI.Foldout(displayArea, foldout, GetContent());
//        } else {
//            GUI.Label(new Rect(displayArea.x+myFoldOffset, displayArea.y, displayArea.width, displayArea.height), GetContent());
//        }
//        return result;
	}
    // ---------------------------------------------------------------------------------
	public object	CurrentObjectKey() {
		return IterValue;
	}
    // ---------------------------------------------------------------------------------
    GUIContent GetContent() {
        EditorGUIUtility.SetIconSize(new Vector2(16.0f,12.0f));
        Texture2D icon= null;
		var current= IterValue;
		var nodeType= current.Type;
//        if(nodeType == NodeTypeEnum.Company) {
//            icon= iCS_TextureCache.GetIcon(iCS_Config.GuiAssetPath+"/"+iCS_EditorStrings.FunctionHierarchyIcon, myStorage);            
//        } else if(nodeType == NodeTypeEnum.Package) {
//            icon= iCS_TextureCache.GetIcon(iCS_Config.GuiAssetPath+"/"+iCS_EditorStrings.ClassHierarchyIcon, myStorage);                            
//        } else if(nodeType == NodeTypeEnum.Class) {
//            icon= iCS_TextureCache.GetIcon(iCS_Config.GuiAssetPath+"/"+iCS_EditorStrings.FunctionHierarchyIcon, myStorage);            
//        } else if(nodeType == NodeTypeEnum.Field) {
//            icon= iCS_TextureCache.GetIcon(iCS_Config.GuiAssetPath+"/"+iCS_EditorStrings.FunctionHierarchyIcon, myStorage);            
//        } else if(nodeType == NodeTypeEnum.Property) {
//            icon= iCS_TextureCache.GetIcon(iCS_Config.GuiAssetPath+"/"+iCS_EditorStrings.FunctionHierarchyIcon, myStorage);            
//        } else if(nodeType == NodeTypeEnum.Constructor) {
//            icon= iCS_TextureCache.GetIcon(iCS_Config.GuiAssetPath+"/"+iCS_EditorStrings.FunctionHierarchyIcon, myStorage);            
//        } else if(nodeType == NodeTypeEnum.Method) {
//            icon= iCS_TextureCache.GetIcon(iCS_Config.GuiAssetPath+"/"+iCS_EditorStrings.FunctionHierarchyIcon, myStorage);            
//        }
        return new GUIContent(current.Name, icon); 
    }
    // ---------------------------------------------------------------------------------
    bool ShouldUseFoldout() {
        return IterValue.Type == NodeTypeEnum.Company || IterValue.Type == NodeTypeEnum.Package || IterValue.Type == NodeTypeEnum.Class;
    }
    // ---------------------------------------------------------------------------------
    public void MouseDownOn(object key, Vector2 mouseInScreenPoint, Rect screenArea) {
        if(key == null) {
            return;
        }
        Node node= key as Node;
        myNameEdition= node == Selected;
        Selected= node;
    }
    // ---------------------------------------------------------------------------------
    public void SelectPrevious() {
        myChangeSelection= -1;
        NameEdition= false;
    }
    // ---------------------------------------------------------------------------------
    public void SelectNext() {
        myChangeSelection= 1;
        NameEdition= false;
    }
    // ---------------------------------------------------------------------------------
    void ProcessChangeSelection() {
        if(myChangeSelection == -1) {   // Move up
            if(Selected == IterValue) {
                Selected= myLastDisplayed;
                myChangeSelection= 0;
            }
        }
        if(myChangeSelection == 1) {    // Move down
            if(Selected == myLastDisplayed) {
                Selected= IterValue;
                myChangeSelection= 0;
            }
        }
        myLastDisplayed= IterValue;
    }
    // ---------------------------------------------------------------------------------
    public void FoldSelected() {
        if(Selected == null) return;
        myTreeView.Fold(Selected);
    }
    // ---------------------------------------------------------------------------------
    public void UnfoldSelected() {
        if(Selected == null) return;
        myTreeView.Unfold(Selected);
    }
    // ---------------------------------------------------------------------------------
    public void ToggleFoldUnfoldSelected() {
        if(Selected == null) return;
        myTreeView.ToggleFoldUnfold(Selected);
    }
}
