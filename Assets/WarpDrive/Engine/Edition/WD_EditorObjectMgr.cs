using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WD_EditorObjectMgr {
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    public bool                     IsDirty= true;
    public List<WD_EditorObject>    EditorObjects= new List<WD_EditorObject>();

    // ======================================================================
    // Editor Object Container Management
    // ----------------------------------------------------------------------
    public WD_EditorObject this[int i] {
        get { return EditorObjects[i]; }
    }
    // ----------------------------------------------------------------------
    public bool IsIdValid(int id) { return id >= 0 && id < EditorObjects.Count; }
    // ----------------------------------------------------------------------
    public void AddObject(WD_Object obj) {
        // Attempt to use an empty slot.
        for(int i= 0; i < EditorObjects.Count; ++i) {
            if(EditorObjects[i].InstanceId == -1) {
                EditorObjects[i].Serialize(obj, i);
                IsDirty= true;
                return;
            }
        }
        // Serialize the given object.
        WD_EditorObject so= new WD_EditorObject();
        so.Serialize(obj, EditorObjects.Count);
        EditorObjects.Add(so);
        IsDirty= true;
    }
    // ----------------------------------------------------------------------
    public void ReplaceObject(WD_Object obj) {
        EditorObjects[obj.InstanceId].Serialize(obj, obj.InstanceId);
        IsDirty= true;
    }
    // ----------------------------------------------------------------------
    public void RemoveObject(int id) {
        if(!IsIdValid(id)) return;
        EditorObjects[id].InstanceId= -1;
        IsDirty= true;        
    }
    public void RemoveObject(WD_EditorObject obj) {
        RemoveObject(obj.InstanceId);
    }
    public void RemoveObject(WD_Object obj) {
        RemoveObject(obj.InstanceId);
    }

    // ======================================================================
    // Editor Object Iteration Utilities
    // ----------------------------------------------------------------------
    // Executes the given action if the given object matches the T type.
    public void ExecuteIf<T>(WD_EditorObject obj, Action<WD_EditorObject> fnc) where T : WD_Object {
        if(obj.IsRuntimeA<T>()) fnc(obj);
    }
    public void ExecuteIf<T>(int id, Action<WD_EditorObject> fnc) where T : WD_Object {
        if(!IsIdValid(id)) return;
        ExecuteIf<T>(EditorObjects[id], fnc);
    }
    public void Case<T1,T2>(WD_EditorObject obj, Action<WD_EditorObject> fnc1,
                                                 Action<WD_EditorObject> fnc2,
                                                 Action<WD_EditorObject> defaultFnc= null) where T1 : WD_Object
                                                                                           where T2 : WD_Object {
        if(obj.IsRuntimeA<T1>())         { fnc1(obj); }
        else if(obj.IsRuntimeA<T2>())    { fnc2(obj); }
        else if(defaultFnc != null)      { defaultFnc(obj); }                                    
    }
    public void Case<T1,T2>(int id, Action<WD_EditorObject> fnc1,
                                    Action<WD_EditorObject> fnc2,
                                    Action<WD_EditorObject> defaultFnc= null) where T1 : WD_Object
                                                                              where T2 : WD_Object {
        if(!IsIdValid(id)) return;
        Case<T1,T2>(EditorObjects[id], fnc1, fnc2, defaultFnc);
    }
    public void Case<T1,T2,T3>(WD_EditorObject obj, Action<WD_EditorObject> fnc1,
                                                    Action<WD_EditorObject> fnc2,
                                                    Action<WD_EditorObject> fnc3,
                                                    Action<WD_EditorObject> defaultFnc= null) where T1 : WD_Object
                                                                                              where T2 : WD_Object
                                                                                              where T3 : WD_Object {
        if(obj.IsRuntimeA<T1>())         { fnc1(obj); }
        else if(obj.IsRuntimeA<T2>())    { fnc2(obj); }
        else if(obj.IsRuntimeA<T3>())    { fnc3(obj); }
        else if(defaultFnc != null)      { defaultFnc(obj); }                                    
    }
    public void Case<T1,T2,T3>(int id, Action<WD_EditorObject> fnc1,
                                       Action<WD_EditorObject> fnc2,
                                       Action<WD_EditorObject> fnc3,
                                       Action<WD_EditorObject> defaultFnc= null) where T1 : WD_Object
                                                                                 where T2 : WD_Object
                                                                                 where T3 : WD_Object {
        if(!IsIdValid(id)) return;
        Case<T1,T2,T3>(EditorObjects[id], fnc1, fnc2, fnc3, defaultFnc);
    }
    public void ForEachChild(WD_EditorObject parent, Action<WD_EditorObject> fnc) {
        foreach(var child in EditorObjects) {
            if(child.ParentId == parent.InstanceId) {
                fnc(child);
            }
        }
    }
    public void ForEachChild<T>(WD_EditorObject parent, Action<WD_EditorObject> fnc) where T : WD_Object {
        ForEachChild(parent, (child) => { ExecuteIf<T>(child, fnc); });
    }
    
    // ======================================================================
    // Editor Graph Layout Functions
    // ----------------------------------------------------------------------
    public void Layout(WD_EditorObject obj) {
        Case<WD_Node, WD_Port>(obj,
            (node) => { NodeLayout(node); },
            (port) => { }
        );
    }
    // ----------------------------------------------------------------------
    // Recompute the layout of a parent node.
    // Returns "true" if the new layout is within the window area.
    public void NodeLayout(WD_EditorObject node) {
        // Don't layout node if it is not visible.
        if(!node.IsVisible) return;
        
        // Resolve collision on children.
        ResolveCollisionOnChildren(node, Vector2.zero);
        
        // Determine needed child rect.
        Rect  childRect   = ComputeChildRect(node);

        // Compute needed width.
        float titleWidth  = WD_EditorConfig.GetNodeWidth(node.NameOrTypeName);
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
        if(!IsIdValid(node.ParentId)) return node.LocalPosition;
        Rect position= GetPosition(EditorObjects[node.ParentId]);
        return new Rect(position.x+node.LocalPosition.x,
                        position.y+node.LocalPosition.y,
                        node.LocalPosition.width,
                        node.LocalPosition.height);
    }
    // ----------------------------------------------------------------------
    void SetPosition(WD_EditorObject node, Rect _newPos) {
        // Adjust node size.
        node.LocalPosition.width = _newPos.width;
        node.LocalPosition.height= _newPos.height;
        // Reposition node.
        if(!IsIdValid(node.ParentId)) {
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
    void LayoutParent(WD_EditorObject node, Vector2 _deltaMove) {
        if(!IsIdValid(node.ParentId)) return;
        WD_EditorObject parentNode= EditorObjects[node.ParentId];
        ResolveCollision(parentNode, _deltaMove);
        Layout(parentNode);
    }
    // ----------------------------------------------------------------------
    void AdjustChildLocalPosition(WD_EditorObject node, Vector2 _delta) {
        ForEachChild<WD_Node>(node, (child)=> { DeltaMoveInternal(child, _delta); } );
    }
    // ----------------------------------------------------------------------
    // Returns the space used by all children.
    Rect ComputeChildRect(WD_EditorObject node) {
        // Compute child space.
        Rect childRect= new Rect(0.5f*node.LocalPosition.width,0.5f*node.LocalPosition.height,0,0);
        ForEachChild<WD_Node>(node,
            (child)=> {
                if(child.IsVisible) {
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
        ForEachChild<WD_Port>(node,
            (port)=> {
                if(port.IsOnTopEdge)         { topPorts.Add(port); }
                else if(port.IsOnBottomEdge) { bottomPorts.Add(port); }
                else if(port.IsOnLeftEdge)   { leftPorts.Add(port); }
                else if(port.IsOnRightEdge)  { rightPorts.Add(port); }
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
        ForEachChild<WD_Port>(node,
            (port)=> {
                if(port.IsOnTopEdge) {
                    fnc(port);
                }
            }
        );        
    }
    
    // ----------------------------------------------------------------------
    public void ForEachBottomPort(WD_EditorObject node, System.Action<WD_EditorObject> fnc) {
        ForEachChild<WD_Port>(node,
            (port)=> {
                if(port.IsOnBottomEdge) {
                    fnc(port);
                }
            }
        );        
    }
    
    // ----------------------------------------------------------------------
    public void ForEachLeftPort(WD_EditorObject node, System.Action<WD_EditorObject> fnc) {
        ForEachChild<WD_Port>(node,
            (port)=> {
                if(port.IsOnLeftEdge) {
                    fnc(port);
                }
            }
        );        
    }
    
    // ----------------------------------------------------------------------
    public void ForEachRightPort(WD_EditorObject node, System.Action<WD_EditorObject> fnc) {
        ForEachChild<WD_Port>(node,
            (port)=> {
                if(port.IsOnRightEdge) {
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
        if(!IsIdValid(node.ParentId)) return;
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
            if(!child1.IsVisible) continue;
            if(!child1.IsRuntimeA<WD_Node>()) continue;
            for(int j= i+1; j < EditorObjects.Count; ++j) {
                WD_EditorObject child2= EditorObjects[j];
                if(child2.ParentId != node.InstanceId) continue;
                if(!child2.IsVisible) continue;
                if(!child2.IsRuntimeA<WD_Node>()) continue;
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

}
