using UnityEngine;
using System.Collections;

public partial class iCS_Graphics {
    // ======================================================================
    // Picking functionality
    // ----------------------------------------------------------------------
    public bool IsNodeTitleBarPicked(iCS_EditorObject node, Vector2 pick, iCS_IStorage iStorage) {
        if(node == null || !node.IsNode || !IsVisible(node, iStorage)) return false;
        if(node.IsIconized) {
            Rect nodeNamePos= GetNodeNamePosition(node, iStorage);
            return nodeNamePos.Contains(pick);
        }
        Rect titleRect= GetDisplayPosition(node, iStorage);
        titleRect.height= kNodeTitleHeight;
        return titleRect.Contains(pick);
    }
    
    // ======================================================================
    // Fold/Unfold icon functionality.
    // ----------------------------------------------------------------------
    public bool IsNodeNamePicked(iCS_EditorObject node, Vector2 pick, iCS_IStorage iStorage) {
        if(IsIconized(node, iStorage)) {
            Rect nodePos= GetNodeNamePosition(node, iStorage);
            float invScale= 1.0f/Scale;
            nodePos.width*= invScale;
            nodePos.height*= invScale;
            return nodePos.Contains(pick);
        } else {
            if(!IsNodeTitleBarPicked(node, pick, iStorage)) return false;
            if(IsFoldIconPicked(node, pick, iStorage)) return false;
            if(IsMinimizeIconPicked(node, pick, iStorage)) return false;
            return true;            
        }
    }
    // ----------------------------------------------------------------------
    bool ShouldShowTitle() {
        return Scale >= 0.4f;    
    }
	
    // ======================================================================
    // Fold/Unfold icon functionality.
    // ----------------------------------------------------------------------
    public bool IsFoldIconPicked(iCS_EditorObject node, Vector2 pick, iCS_IStorage iStorage) {
        if(!ShouldDisplayFoldIcon(node, iStorage)) return false;
        Rect foldIconPos= GetFoldIconPosition(node, iStorage);
        return foldIconPos.Contains(pick);
    }
    bool ShouldDisplayFoldIcon(iCS_EditorObject obj, iCS_IStorage iStorage) {
        if(obj.IsIconized) return false;
        return (obj.IsModule || obj.IsStateChart || obj.IsState);
    }
    Rect GetFoldIconPosition(iCS_EditorObject obj, iCS_IStorage iStorage) {
        Rect objPos= GetDisplayPosition(obj, iStorage);
        return new Rect(objPos.x+8, objPos.y, foldedIcon.width, foldedIcon.height);
    }

    // ======================================================================
    // Minimize icon functionality
    // ----------------------------------------------------------------------
    public bool IsMinimizeIconPicked(iCS_EditorObject node, Vector2 pick, iCS_IStorage iStorage) {
        if(!ShouldDisplayMinimizeIcon(node, iStorage)) return false;
        Rect minimizeIconPos= GetMinimizeIconPosition(node, iStorage);
        return minimizeIconPos.Contains(pick);
    }
    bool ShouldDisplayMinimizeIcon(iCS_EditorObject obj, iCS_IStorage iStorage) {
        return obj.InstanceId != 0 && obj.IsNode && !obj.IsIconized;
    }
    Rect GetMinimizeIconPosition(iCS_EditorObject node, iCS_IStorage iStorage) {
        Rect objPos= GetDisplayPosition(node, iStorage);
        return new Rect(objPos.xMax-2-/*minimizeIcon.width*/16, objPos.y, /*minimizeIcon.width*/16, /*minimizeIcon.height*/16);
    }

    // ======================================================================
    // Maximize icon functionality
    // ----------------------------------------------------------------------
    public static Vector2 GetMaximizeIconSize(iCS_EditorObject node) {
        Texture2D icon= null;
        if(node != null && node.IconGUID != null) {
            icon= iCS_TextureCache.GetIconFromGUID(node.IconGUID);
            if(icon != null) return new Vector2(icon.width, icon.height);
        }
        return new Vector2(maximizeIcon.width, maximizeIcon.height);        
    }
    // ----------------------------------------------------------------------
    public Texture2D GetMaximizeIcon(iCS_EditorObject node) {
        Texture2D icon= null;
        if(node != null && node.IconGUID != null) {
            icon= iCS_TextureCache.GetIconFromGUID(node.IconGUID);
            if(icon != null) return icon;
        }
        return GetNodeDefaultMaximizeIcon(node);
    }
    // ----------------------------------------------------------------------
    public bool IsMaximizeIconPicked(iCS_EditorObject obj, Vector2 mousePos, iCS_IStorage iStorage) {
        if(!ShouldDisplayMaximizeIcon(obj)) return false;
        Rect maximizeIconPos= GetMaximizeIconPosition(obj, iStorage);
        return maximizeIconPos.Contains(mousePos);
    }
    bool ShouldDisplayMaximizeIcon(iCS_EditorObject obj) {
        return obj.InstanceId != 0 && obj.IsNode && obj.IsIconized;
    }
    Rect GetMaximizeIconPosition(iCS_EditorObject obj, iCS_IStorage iStorage) {
        return GetDisplayPosition(obj, iStorage);
    }

    // ======================================================================
    // Determines if the pick is within the port name label.
    // ----------------------------------------------------------------------
    public bool IsPortNamePicked(iCS_EditorObject port, Vector2 pick, iCS_IStorage iStorage) {
        if(!ShouldDisplayPortName(port, iStorage)) return false;
        Rect portNamePos= GetPortNamePosition(port, iStorage);
        float invScale= 1.0f/Scale;
        portNamePos.width*= invScale;
        portNamePos.height*= invScale;
        return portNamePos.Contains(pick);
    }
    // ----------------------------------------------------------------------
    bool ShouldShowLabel() {
        return Scale >= 0.5f;        
    }
    // ----------------------------------------------------------------------
    bool ShouldShowPort() {
        return Scale >= 0.45f;        
    }
    
    // ======================================================================
    // Determines if the pick is within the port value label.
    // ----------------------------------------------------------------------
    public bool IsPortValuePicked(iCS_EditorObject port, Vector2 pick, iCS_IStorage iStorage) {
        if(!ShouldDisplayPortValue(port, iStorage)) return false;
        if(!port.IsInputPort) return false;
        if(port.Source != null) return false;
        Rect portValuePos= GetPortValuePosition(port, iStorage);
        float invScale= 1.0f/Scale;
        portValuePos.width*= invScale;
        portValuePos.height*= invScale;
        return portValuePos.Contains(pick);
    }
	
    // ======================================================================
    // Displays which element is being picked.
    // ----------------------------------------------------------------------
    public iCS_PickInfo GetPickInfo(Vector2 pick, iCS_IStorage iStorage) {
        iCS_PickInfo pickInfo= new iCS_PickInfo(iStorage);
        pickInfo.PickedPoint= pick;
        var port= iStorage.GetPortAt(pick);
        if(port != null) {
//            Debug.Log("Port: "+port.Name+" is being picked");
            pickInfo.PickedObject= port;
            pickInfo.PickedPart= iCS_PickPartEnum.EditorObject;
            pickInfo.PickedPartGraphPosition= port.AnimatedGlobalLayoutRect;
            pickInfo.PickedPartGUIPosition= TranslateAndScale(pickInfo.PickedPartGraphPosition);
            return pickInfo;
        }
        var pickedNode= iStorage.GetNodeAt(pick);
        if(pickedNode != null) {
            if(IsFoldIconPicked(pickedNode, pick, iStorage)) {
//                Debug.Log("Fold icon of: "+pickedNode.Name+" is being picked");
                pickInfo.PickedObject= pickedNode;
                pickInfo.PickedPart= iCS_PickPartEnum.FoldIcon;
                pickInfo.PickedPartGraphPosition= GetFoldIconPosition(pickedNode, iStorage);
                pickInfo.PickedPartGUIPosition= TranslateAndScale(pickInfo.PickedPartGraphPosition);
                return pickInfo;
            }
            if(IsMinimizeIconPicked(pickedNode, pick, iStorage)) {
//                Debug.Log("Minimize icon of: "+pickedNode.Name+" is being picked");
                pickInfo.PickedObject= pickedNode;
                pickInfo.PickedPart= iCS_PickPartEnum.MinimizeIcon;
                pickInfo.PickedPartGraphPosition= GetMinimizeIconPosition(pickedNode, iStorage);
                pickInfo.PickedPartGUIPosition= TranslateAndScale(pickInfo.PickedPartGraphPosition);
                return pickInfo;
            }
            if(IsNodeNamePicked(pickedNode, pick, iStorage)) {
//                Debug.Log("Node name: "+pickedNode.Name+" is being picked");
                pickInfo.PickedObject= pickedNode;
                pickInfo.PickedPart= iCS_PickPartEnum.Name;
                Rect namePos= GetNodeNamePosition(pickedNode, iStorage);
                float invScale= 1.0f/Scale;
                pickInfo.PickedPartGraphPosition= new Rect(namePos.x, namePos.y, namePos.width*invScale, namePos.height*invScale);
                var guiPos= TranslateAndScale(Math3D.ToVector2(namePos));
                pickInfo.PickedPartGUIPosition= new Rect(guiPos.x, guiPos.y, namePos.width, namePos.height);
                return pickInfo;
            }
            bool result= iStorage.UntilMatchingChildNode(pickedNode,
                c=> {
                    if(IsIconized(c, iStorage)) {
                        if(IsNodeNamePicked(c, pick, iStorage)) {
//                            Debug.Log("Node name: "+c.Name+" is being picked");
                            pickInfo.PickedObject= c;
                            pickInfo.PickedPart= iCS_PickPartEnum.Name;
                            Rect namePos= GetNodeNamePosition(c, iStorage);
                            float invScale= 1.0f/Scale;
                            pickInfo.PickedPartGraphPosition= new Rect(namePos.x, namePos.y, namePos.width*invScale, namePos.height*invScale);
                            var guiPos= TranslateAndScale(Math3D.ToVector2(namePos));
                            pickInfo.PickedPartGUIPosition= new Rect(guiPos.x, guiPos.y, namePos.width, namePos.height);
                            return true;
                        }
                    } 
                    return false;
                }
            );
            if(result) return pickInfo;
        }
        var closestPort= iStorage.GetClosestPortAt(pick);
        if(closestPort != null) {
            if(IsPortNamePicked(closestPort, pick, iStorage)) {
//                Debug.Log((closestPort.IsInputPort ? "Input":"Output")+" port name: "+closestPort.Name+" of "+iStorage.GetParent(closestPort).Name+" is being picked");
                pickInfo.PickedObject= closestPort;
                pickInfo.PickedPart= iCS_PickPartEnum.Name;
                Rect namePos= GetPortNamePosition(closestPort, iStorage);
                float invScale= 1.0f/Scale;
                pickInfo.PickedPartGraphPosition= new Rect(namePos.x, namePos.y, namePos.width*invScale, namePos.height*invScale);
                var guiPos= TranslateAndScale(Math3D.ToVector2(namePos));
                pickInfo.PickedPartGUIPosition= new Rect(guiPos.x, guiPos.y, namePos.width, namePos.height);
                return pickInfo;
            }
            if(IsPortValuePicked(closestPort, pick, iStorage)) {
//                Debug.Log((closestPort.IsInputPort ? "Input":"Output")+" port value: "+closestPort.Name+" of "+iStorage.GetParent(closestPort).Name+" is being picked");
                pickInfo.PickedObject= closestPort;
                pickInfo.PickedPart= iCS_PickPartEnum.Value;
                Rect namePos= GetPortValuePosition(closestPort, iStorage);
                float invScale= 1.0f/Scale;
                pickInfo.PickedPartGraphPosition= new Rect(namePos.x, namePos.y, namePos.width*invScale, namePos.height*invScale);
                var guiPos= TranslateAndScale(Math3D.ToVector2(namePos));
                pickInfo.PickedPartGUIPosition= new Rect(guiPos.x, guiPos.y, namePos.width, namePos.height);
                return pickInfo;
            }
        }
        if(pickedNode != null) {
//            Debug.Log("Node: "+pickedNode.Name+" is being picked");
            pickInfo.PickedObject= pickedNode;
            pickInfo.PickedPart= iCS_PickPartEnum.EditorObject;
            pickInfo.PickedPartGraphPosition= pickedNode.AnimatedGlobalLayoutRect;
            pickInfo.PickedPartGUIPosition= TranslateAndScale(pickInfo.PickedPartGraphPosition);
            return pickInfo;
        }
        return null;
    }
    // ======================================================================
    // Extract graph information at a given point.
    // ----------------------------------------------------------------------
    Prelude.Tuple<iCS_EditorObject,iCS_EditorObject> GetGraphInfoAt(Vector2 point, iCS_IStorage iStorage) {
        iCS_EditorObject objectAt= iStorage.GetPortAt(point);
        if(objectAt == null) objectAt= iStorage.GetNodeAt(point);
        return new Prelude.Tuple<iCS_EditorObject,iCS_EditorObject>(objectAt, iStorage.GetClosestPortAt(point));
    }
}
