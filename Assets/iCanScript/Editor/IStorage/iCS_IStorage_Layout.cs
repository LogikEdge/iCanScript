using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class iCS_IStorage {
    // ======================================================================
    // Constants
    // ----------------------------------------------------------------------
    const float kLayoutEpsilon= 0.01f;
    
    // ======================================================================
    // Node Layout
    // ----------------------------------------------------------------------
    // Moves the node without changing its size.
    public void SetInitialPosition(iCS_EditorObject obj, Vector2 initialPosition) {
        if(IsValid(obj.ParentId)) {
            Rect position= GetLayoutPosition(EditorObjects[obj.ParentId]);
            obj.LocalPosition.x= initialPosition.x - position.x;
            obj.LocalPosition.y= initialPosition.y - position.y;            
        }
        else {
            obj.LocalPosition.x= initialPosition.x;
            obj.LocalPosition.y= initialPosition.y;                        
        }
        SetDirty(obj);
    }

    // ----------------------------------------------------------------------
    public void Layout(iCS_EditorObject obj) {
        obj.IsDirty= false;
        ExecuteIf(obj, iCS_ObjectType.IsNode, o=> NodeLayout(o));
    }

    // ----------------------------------------------------------------------
    // Recompute the layout of a parent node.
    // Returns "true" if the new layout is within the window area.
    public void NodeLayout(iCS_EditorObject node, bool needsToBeCentered= false) {
        // Don't layout node if it is not visible.
        if(!IsVisible(node)) return;

        // Update transition module name
        if(node.IsTransitionModule) {
            GetTransitionName(node);
        }
        
        // Minimized nodes are fully collapsed.
        if(node.IsMinimized) {
            Vector2 iconSize= iCS_Graphics.GetMaximizeIconSize(node);
            if(node.LocalPosition.width != iconSize.x || node.LocalPosition.height != iconSize.y) {
                if(IsValid(node.ParentId)) {
                    if(node.LocalPosition.x == 0) node.LocalPosition.x= 0.5f*GetParent(node).LocalPosition.width;
                    if(node.LocalPosition.y == 0) node.LocalPosition.y= 0.5f*GetParent(node).LocalPosition.height;                    
                }
                node.LocalPosition.x+= 0.5f*(node.LocalPosition.width-iconSize.x);
                node.LocalPosition.y+= 0.5f*(node.LocalPosition.height-iconSize.y);
                node.LocalPosition.width= iconSize.x;
                node.LocalPosition.height= iconSize.y;
                LayoutPorts(node);
            }
            return;
        }

        // Refresh children that have not yet perfromed a layout.
        ForEachChild(node, c=> { if(c.LocalPosition.x == 0 && c.LocalPosition.y == 0) SetDirty(c); });
        
        // Resolve collision on children.
        ResolveCollisionOnChildren(node, Vector2.zero);

        // Determine needed child rect (in global space).
        Rect  childrenGlobalRect= ComputeChildrenRect(node);

        // Compute needed width.
        float titleWidth  = iCS_Config.GetNodeWidth(node.Name)+iCS_Config.ExtraIconWidth;
        float leftMargin  = ComputeLeftMargin(node);
        float rightMargin = ComputeRightMargin(node);
        float width       = 2.0f*iCS_Config.GutterSize + Mathf.Max(titleWidth, leftMargin + rightMargin + childrenGlobalRect.width);

        // Process case without child nodes
        Rect globalPosition= GetLayoutPosition(node);
        if(Math3D.IsZero(childrenGlobalRect.width) || Math3D.IsZero(childrenGlobalRect.height)) {
            // Compute needed height.
            iCS_EditorObject[] leftPorts= GetLeftPorts(node);
            iCS_EditorObject[] rightPorts= GetRightPorts(node);
            int nbOfPorts= leftPorts.Length > rightPorts.Length ? leftPorts.Length : rightPorts.Length;
            float height= Mathf.Max(iCS_Config.NodeTitleHeight+nbOfPorts*iCS_Config.MinimumPortSeparation, iCS_Config.MinimumNodeHeight);                                
            
            // Apply new width and height.
            if(Math3D.IsNotEqual(height, globalPosition.height) || Math3D.IsNotEqual(width, globalPosition.width)) {
                float deltaWidth = width - globalPosition.width;
                float deltaHeight= height - globalPosition.height;
                Rect newPos= new Rect(globalPosition.xMin-0.5f*deltaWidth, globalPosition.yMin-0.5f*deltaHeight, width, height);
                SetLayoutPosition(node, newPos);
            }
        }
        // Process case with child nodes.
        else {
            // Adjust children local offset.
            float neededChildXOffset= iCS_Config.GutterSize+leftMargin;
            float neededChildYOffset= iCS_Config.GutterSize+iCS_Config.NodeTitleHeight;
            float neededNodeGlobalX= childrenGlobalRect.x-neededChildXOffset;
            float neededNodeGlobalY= childrenGlobalRect.y-neededChildYOffset;
            if(Math3D.IsNotEqual(neededNodeGlobalX, globalPosition.x) ||
               Math3D.IsNotEqual(neededNodeGlobalY, globalPosition.y)) {
                   AdjustChildLocalPosition(node, new Vector2(globalPosition.x-neededNodeGlobalX, globalPosition.y-neededNodeGlobalY));
            }

            // Compute needed height.
            int nbOfLeftPorts = GetNbOfLeftPorts(node);
            int nbOfRightPorts= GetNbOfRightPorts(node);
            int nbOfPorts= nbOfLeftPorts > nbOfRightPorts ? nbOfLeftPorts : nbOfRightPorts;
            float portHeight= nbOfPorts*iCS_Config.MinimumPortSeparation;                                
            float height= iCS_Config.NodeTitleHeight+Mathf.Max(portHeight, childrenGlobalRect.height+2.0f*iCS_Config.GutterSize);
            
            // Relocate node if centering is needed 
            Rect newPos= new Rect(neededNodeGlobalX, neededNodeGlobalY, width, height);
            if(needsToBeCentered) {
                var currentCenter= Math3D.Middle(globalPosition);
                var newCenter= Math3D.Middle(newPos);
                var diff= currentCenter-newCenter; 
                if(Math3D.IsNotZero(diff)) {
                    newPos.x+= diff.x;
                    newPos.y+= diff.y;
                }
            }
            if(!IsSamePosition(globalPosition, newPos)) {
                SetLayoutPosition(node, newPos);
            }
        }

        // Layout child ports
        LayoutPorts(node);
    }
    // ----------------------------------------------------------------------
    // Determine if position are the same.
    public bool IsSamePosition(Rect r1, Rect r2) {
        return Math3D.IsWithin(r1.x, r2.x-kLayoutEpsilon, r2.x+kLayoutEpsilon) &&
               Math3D.IsWithin(r1.y, r2.y-kLayoutEpsilon, r2.y+kLayoutEpsilon) &&
               Math3D.IsWithin(r1.width, r2.width-kLayoutEpsilon, r2.width+kLayoutEpsilon) &&
               Math3D.IsWithin(r1.height, r2.height-kLayoutEpsilon, r2.height+kLayoutEpsilon);
    }
    // ----------------------------------------------------------------------
    // Moves the node without changing its size.
    public void MoveTo(iCS_EditorObject node, Vector2 _newPos) {
        Rect position = GetLayoutPosition(node);
        var delta= new Vector2(_newPos.x - position.x, _newPos.y - position.y);
        DeltaMove(node, delta);
    }
    // ----------------------------------------------------------------------
    // Moves the node without changing its size.
    public void DeltaMove(iCS_EditorObject node, Vector2 _delta) {
        // Move the node
        DeltaMoveInternal(node, _delta);
        // Resolve collision between siblings.
        LayoutParent(node, _delta);
	}
    // ----------------------------------------------------------------------
    // Moves the node without changing its size.
    void DeltaMoveInternal(iCS_EditorObject node, Vector2 _delta) {
        if(Math3D.IsNotZero(_delta)) {
            node.LocalPosition.x+= _delta.x;
            node.LocalPosition.y+= _delta.y;
            SetDirty(node);
        }
    }
    // ----------------------------------------------------------------------
    Vector2 GetTopLeftCorner(iCS_EditorObject node)     {
        Rect position= GetLayoutPosition(node);
        return new Vector2(position.xMin, position.yMin);
    }
    Vector2 GetTopRightCorner(iCS_EditorObject node)    {
        Rect position= GetLayoutPosition(node);
        return new Vector2(position.xMax, position.yMin);
    }
    Vector2 GetBottomLeftCorner(iCS_EditorObject node)  {
        Rect position= GetLayoutPosition(node);
        return new Vector2(position.xMin, position.yMax);
    }
    Vector2 GetBottomRightCorner(iCS_EditorObject node) {
        Rect position= GetLayoutPosition(node);
        return new Vector2(position.xMax, position.yMax);
    }
    // ----------------------------------------------------------------------
    void LayoutParent(iCS_EditorObject node, Vector2 _deltaMove) {
        if(!IsValid(node.ParentId)) return;
        iCS_EditorObject parentNode= GetParent(node);
        ResolveCollision(parentNode, _deltaMove);
        Layout(parentNode);
    }
    // ----------------------------------------------------------------------
    void AdjustChildLocalPosition(iCS_EditorObject node, Vector2 _delta) {
        ForEachChild(node, (child)=> { if(child.IsNode) DeltaMoveInternal(child, _delta); } );
    }
    // ----------------------------------------------------------------------
    // Returns the space used by all children.
    Rect ComputeChildRect(iCS_EditorObject node) {
        // Compute child space.
        Rect childRect= new Rect(0.5f*node.LocalPosition.width,0.5f*node.LocalPosition.height,0,0);
        ForEachChild(node,
            (child)=> {
                if(child.IsNode && IsVisible(child) && !child.IsFloating) {
                    childRect= Math3D.Merge(childRect, child.LocalPosition);
                }
            }
        );
        return childRect;
    }
    // ----------------------------------------------------------------------
    // Returns the space used by all children.
    Rect ComputeChildrenRect(iCS_EditorObject node) {
        // Compute child space.
        Rect childrenRect= new Rect(0,0,0,0);
        ForEachChild(node,
            (child)=> {
                if(child.IsNode && IsVisible(child) && !child.IsFloating) {
                    var childPos= GetLayoutPosition(child);
                    if(Math3D.IsZero(childrenRect.width)) {
                        childrenRect= childPos;
                    } else {
                        childrenRect= Math3D.Merge(childrenRect, childPos);                        
                    }
                }
            }
        );
        return childrenRect;
    }
    // ----------------------------------------------------------------------
    // Returns the inner left margin.
    float ComputeLeftMargin(iCS_EditorObject node) {
        float LeftMargin= 0;
        ForEachLeftPort(node,
            port=> {
                if(!port.IsStatePort && IsPortOnParent(port)) {
                    Vector2 labelSize= iCS_Config.GetPortLabelSize(port.Name);
                    float nameSize= labelSize.x+iCS_Config.PortSize;
                    if(LeftMargin < nameSize) LeftMargin= nameSize;
                }
            }
        );
        return LeftMargin;
    }
    // ----------------------------------------------------------------------
    // Returns the inner right margin.
    float ComputeRightMargin(iCS_EditorObject node) {
        float RightMargin= 0;
        ForEachRightPort(node,
            port => {
                if(!port.IsStatePort && IsPortOnParent(port)) {
                    Vector2 labelSize= iCS_Config.GetPortLabelSize(port.Name);
                    float nameSize= labelSize.x+iCS_Config.PortSize;
                    if(RightMargin < nameSize) RightMargin= nameSize;                    
                }
            }
        );
        return RightMargin;
    }


    // ======================================================================
    // Port Layout
    // ----------------------------------------------------------------------
    // Recomputes the port position.
    public void LayoutPorts(iCS_EditorObject node) {
        // Update all port edges.
        ForEachChild(node, child=> ExecuteIf(child, port=> port.IsPort, port=> UpdatePortEdge(port)));
        
        // Special case for minimized nodes.
        if(node.IsMinimized) {
            float cx= 0.5f*node.LocalPosition.width;
            float cy= 0.5f*node.LocalPosition.height;
            ForEachChild(node, child=> ExecuteIf(child, p=> p.IsPort, port=> { port.LocalPosition.x= cx; port.LocalPosition.y= cy; }));
            return;
        }

        // Relayout top ports.
        Rect position= GetLayoutPosition(node);        
        iCS_EditorObject[] ports= SortTopPorts(node);
        if(ports.Length != 0) {
            float xStep= position.width / ports.Length;
            for(int i= 0; i < ports.Length; ++i) {
                if(!ports[i].IsFloating) {
                    ports[i].LocalPosition.x= (i+0.5f) * xStep;
                    ports[i].LocalPosition.y= 0;                    
                }
            }
            if(!AreChildrenInSameOrder(node, ports)) {
                ReorderChildren(node, ports);
            }            
        }

        // Relayout bottom ports.
        ports= SortBottomPorts(node);
        if(ports.Length != 0) {
            float xStep= position.width / ports.Length;
            for(int i= 0; i < ports.Length; ++i) {
                if(!ports[i].IsFloating) {
                    ports[i].LocalPosition.x= (i+0.5f) * xStep;
                    ports[i].LocalPosition.y= position.height;                
                }
            }            
            if(!AreChildrenInSameOrder(node, ports)) {
                ReorderChildren(node, ports);
            }            
        }

        // Relayout left ports.
        ports= SortLeftPorts(node);
        if(ports.Length != 0) {
            float topOffset= iCS_Config.NodeTitleHeight-2;
            float yStep= (position.height-topOffset) / ports.Length;
            for(int i= 0; i < ports.Length; ++i) {
                if(!ports[i].IsFloating) {
                    ports[i].LocalPosition.x= 0;
                    ports[i].LocalPosition.y= topOffset + (i+0.5f) * yStep;                
                }
            }
            if(!AreChildrenInSameOrder(node, ports)) {
                ReorderChildren(node, ports);
            }            
        }

        // Relayout right ports.
        ports= SortRightPorts(node);
        if(ports.Length != 0) {
            float topOffset= iCS_Config.NodeTitleHeight-2;
            float yStep= (position.height-topOffset) / ports.Length;
            for(int i= 0; i < ports.Length; ++i) {
                if(!ports[i].IsFloating) {
                    ports[i].LocalPosition.x= position.width;
                    ports[i].LocalPosition.y= topOffset + (i+0.5f) * yStep;
                }
            }
            if(!AreChildrenInSameOrder(node, ports)) {
                ReorderChildren(node, ports);
            }            
        }        
    }

    // ----------------------------------------------------------------------
    bool AreChildrenInSameOrder(iCS_EditorObject node, iCS_EditorObject[] orderedChildren) {
        return StorageCache[node.InstanceId].AreChildrenInSameOrder(Prelude.map(c=> c.InstanceId, orderedChildren));
    }
    void ReorderChildren(iCS_EditorObject node, iCS_EditorObject[] orderedChildren) {
        StorageCache[node.InstanceId].ReorderChildren(Prelude.map(c=> c.InstanceId, orderedChildren));
    }
    // ----------------------------------------------------------------------
    iCS_EditorObject[] SortTopPorts(iCS_EditorObject node) {
        Rect nodePos= GetLayoutPosition(node);
        float refPos= 0.5f*(nodePos.xMin+nodePos.xMax);
        iCS_EditorObject[] ports= GetTopPorts(node);
        Vector2[] connectedPos= Prelude.map(p=> Math3D.ToVector2(GetLayoutPosition(p)), ports);
        float[] firstKeys = Prelude.map(cp=> cp.x, connectedPos); 
        float[] secondKeys= Prelude.map(cp=> refPos < cp.x ? cp.y : -cp.y, connectedPos);
        return SortPorts(ports, firstKeys, secondKeys);
    }
    // ----------------------------------------------------------------------
    iCS_EditorObject[] SortBottomPorts(iCS_EditorObject node) {
        Rect nodePos= GetLayoutPosition(node);
        float refPos= 0.5f*(nodePos.xMin+nodePos.xMax);
        iCS_EditorObject[] ports= GetBottomPorts(node);
        Vector2[] connectedPos= Prelude.map(p=> Math3D.ToVector2(GetLayoutPosition(p)), ports);
        float[] firstKeys = Prelude.map(cp=> cp.x, connectedPos); 
        float[] secondKeys= Prelude.map(cp=> refPos < cp.x ? -cp.y : cp.y, connectedPos);
        return SortPorts(ports, firstKeys, secondKeys);
    }
    // ----------------------------------------------------------------------
    iCS_EditorObject[] SortLeftPorts(iCS_EditorObject node) {
        Rect nodePos= GetLayoutPosition(node);
        float refPos= 0.5f*(nodePos.yMin+nodePos.yMax);
        iCS_EditorObject[] ports= GetLeftPorts(node);                             
        Vector2[] connectedPos= Prelude.map(p=> Math3D.ToVector2(GetLayoutPosition(p)), ports);
        float[] firstKeys = Prelude.map(cp=> cp.y, connectedPos); 
        float[] secondKeys= Prelude.map(cp=> refPos < cp.y ? cp.x : -cp.x, connectedPos);
        return SortPorts(ports, firstKeys, secondKeys);
    }
    // ----------------------------------------------------------------------
    iCS_EditorObject[] SortRightPorts(iCS_EditorObject node) {
        Rect nodePos= GetLayoutPosition(node);
        float refPos= 0.5f*(nodePos.yMin+nodePos.yMax);
        iCS_EditorObject[] ports= GetRightPorts(node);
        Vector2[] connectedPos= Prelude.map(p=> Math3D.ToVector2(GetLayoutPosition(p)), ports);
        float[] firstKeys = Prelude.map(cp=> cp.y, connectedPos); 
        float[] secondKeys= Prelude.map(cp=> refPos < cp.y ? -cp.x : cp.x, connectedPos);
        return SortPorts(ports, firstKeys, secondKeys);
    }
    // ----------------------------------------------------------------------
    // Sorts the given port according to their relative positions.
    iCS_EditorObject[] SortPorts(iCS_EditorObject[] ports, float[] keys1, float[] keys2) {
        for(int i= 0; i < ports.Length-1; ++i) {
            for(int j= i+1; j < ports.Length; ++j) {
                if(Math3D.IsGreater(keys1[i], keys1[j])) {
                    Exchange(ref ports[i], ref ports[j]);
                    Exchange(ref keys1[i], ref keys1[j]);
                    Exchange(ref keys2[i], ref keys2[j]);
                } else if(Math3D.IsEqual(keys1[i], keys1[j])) {                
                    if(Math3D.IsGreater(keys2[i], keys2[j])) {
                        Exchange(ref ports[i], ref ports[j]);
                        Exchange(ref keys1[i], ref keys1[j]);
                        Exchange(ref keys2[i], ref keys2[j]);                    
                    }
                }
            }
        }
        return ports;
    }
    void Exchange(ref iCS_EditorObject a, ref iCS_EditorObject b) {
        iCS_EditorObject tmp= a;
        a= b;
        b= tmp;
    }
    void Exchange(ref float a, ref float b) {
        float tmp= a;
        a= b;
        b= tmp;
    }
    // ----------------------------------------------------------------------
    // Returns all ports position on the top edge.
    public iCS_EditorObject[] GetTopPorts(iCS_EditorObject node) {
        List<iCS_EditorObject> ports= new List<iCS_EditorObject>();
        ForEachTopPort(node, port=> { if(IsPortOnParent(port)) ports.Add(port);});
        return ports.ToArray();
    }

    // ----------------------------------------------------------------------
    // Returns all ports position on the bottom edge.
    public iCS_EditorObject[] GetBottomPorts(iCS_EditorObject node) {
        List<iCS_EditorObject> ports= new List<iCS_EditorObject>();
        ForEachBottomPort(node, port=> { if(IsPortOnParent(port)) ports.Add(port);});
        return ports.ToArray();
    }

    // ----------------------------------------------------------------------
    // Returns all ports position on the left edge.
    public iCS_EditorObject[] GetLeftPorts(iCS_EditorObject node) {
        List<iCS_EditorObject> ports= new List<iCS_EditorObject>();
        ForEachLeftPort(node, port=> { if(IsPortOnParent(port)) ports.Add(port);});
        return ports.ToArray();        
    }

    // ----------------------------------------------------------------------
    // Returns all ports position on the right edge.
    public iCS_EditorObject[] GetRightPorts(iCS_EditorObject node) {
        List<iCS_EditorObject> ports= new List<iCS_EditorObject>();
        ForEachRightPort(node, port=> { if(IsPortOnParent(port)) ports.Add(port);});
        return ports.ToArray();
    }
    // ----------------------------------------------------------------------
    bool IsPortOnParent(iCS_EditorObject port) {
        return !port.IsFloating;
    }
    // ----------------------------------------------------------------------
    // Returns the number of ports on the top edge.
    public int GetNbOfTopPorts(iCS_EditorObject node) {
        int nbOfPorts= 0;
        ForEachTopPort(node, _=> ++nbOfPorts);
        return nbOfPorts;
    }

    // ----------------------------------------------------------------------
    // Returns the number of ports on the bottom edge.
    public int GetNbOfBottomPorts(iCS_EditorObject node) {
        int nbOfPorts= 0;
        ForEachBottomPort(node, _=> ++nbOfPorts);
        return nbOfPorts;
    }

    // ----------------------------------------------------------------------
    // Returns the number of ports on the left edge.
    public int GetNbOfLeftPorts(iCS_EditorObject node) {
        int nbOfPorts= 0;
        ForEachLeftPort(node, _=> ++nbOfPorts);
        return nbOfPorts;
    }

    // ----------------------------------------------------------------------
    // Returns the number of ports on the right edge.
    public int GetNbOfRightPorts(iCS_EditorObject node) {
        int nbOfPorts= 0;
        ForEachRightPort(node, _=> ++nbOfPorts);
        return nbOfPorts;
    }

    // ----------------------------------------------------------------------
    public void ForEachTopPort(iCS_EditorObject node, System.Action<iCS_EditorObject> fnc) {
        ForEachChildPort(node, child=> ExecuteIf(child, port=> port.IsOnTopEdge, fnc));
    }
    // ----------------------------------------------------------------------
    public void ForEachBottomPort(iCS_EditorObject node, System.Action<iCS_EditorObject> fnc) {
        ForEachChildPort(node, child=> ExecuteIf(child, port=> port.IsOnBottomEdge, fnc));
    }
    // ----------------------------------------------------------------------
    public void ForEachLeftPort(iCS_EditorObject node, System.Action<iCS_EditorObject> fnc) {
        ForEachChildPort(node, child=> ExecuteIf(child, port=> port.IsOnLeftEdge, fnc));
    }
    // ----------------------------------------------------------------------
    public void ForEachRightPort(iCS_EditorObject node, System.Action<iCS_EditorObject> fnc) {
        ForEachChildPort(node, child=> ExecuteIf(child, port=> port.IsOnRightEdge, fnc));
    }


    // ======================================================================
    // Collision Functions
    // ----------------------------------------------------------------------
    // Resolve collision on parents.
    void ResolveCollision(iCS_EditorObject node, Vector2 _delta) {
        ResolveCollisionOnChildren(node, _delta);
        if(!IsValid(node.ParentId)) return;
        ResolveCollision(GetParent(node), _delta);
    }

    // ----------------------------------------------------------------------
    // Resolves the collision between children.  "true" is returned if a
    // collision has occured.
    public void ResolveCollisionOnChildren(iCS_EditorObject node, Vector2 _delta) {
        bool didCollide= false;
        iCS_EditorObject[] children= BuildListOfChildren(c=> IsVisible(c) && c.IsNode && !c.IsFloating,node);
        for(int i= 0; i < children.Length-1; ++i) {
            for(int j= i+1; j < children.Length; ++j) {
                didCollide |= ResolveCollisionBetweenTwoNodes(children[i], children[j], _delta);                            
            }
        }
        if(didCollide) ResolveCollisionOnChildren(node, _delta);
    }

    // ----------------------------------------------------------------------
    // Resolves collision between two nodes. "true" is returned if a collision
    // has occured.
    public bool ResolveCollisionBetweenTwoNodes(iCS_EditorObject node, iCS_EditorObject otherNode, Vector2 _delta) {
        // Nothing to do if they don't collide.
        if(!DoesCollideWithGutter(node, otherNode)) return false;

        // Compute penetration.
        Vector2 penetration= GetSeperationVector(node, GetLayoutPosition(otherNode));
		if(Mathf.Abs(penetration.x) < 1.0f && Mathf.Abs(penetration.y) < 1.0f) return false;

		// Seperate using the known movement.
        if( !Math3D.IsZero(_delta) ) {
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
    public bool DoesCollide(iCS_EditorObject node, iCS_EditorObject otherNode) {
        return Math3D.DoesCollide(GetLayoutPosition(node), GetLayoutPosition(otherNode));
    }

    // ----------------------------------------------------------------------
    // Returns if the given rectangle collides with the node.
    public bool DoesCollideWithGutter(iCS_EditorObject node, iCS_EditorObject otherNode) {
        return Math3D.DoesCollide(RectWithGutter(GetLayoutPosition(node)), GetLayoutPosition(otherNode));
    }

    // ----------------------------------------------------------------------
    static Rect RectWithGutter(Rect _rect) {
        float gutterSize= iCS_Config.GutterSize;
        float gutterSize2= 2.0f*gutterSize;
        return new Rect(_rect.x-gutterSize, _rect.y-gutterSize, _rect.width+gutterSize2, _rect.height+gutterSize2);        
    }
    
    // ----------------------------------------------------------------------
	// Returns the seperation vector of two colliding nodes.
	Vector2 GetSeperationVector(iCS_EditorObject node, Rect _rect) {
        Rect myRect= RectWithGutter(GetLayoutPosition(node));
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


    // ======================================================================
    // Layout from iCS_Port
    // ----------------------------------------------------------------------
    public void UpdatePortEdge(iCS_EditorObject port) {
        if(IsValid(port.Source)) UpdatePortEdges(port, EditorObjects[port.Source]);
        Prelude.forEach(p=> UpdatePortEdges(port, p), FindConnectedPorts(port));
    }
    // ----------------------------------------------------------------------
    public void UpdatePortEdges(iCS_EditorObject p1, iCS_EditorObject p2) {
        // Don't update port edges for a transition bridge.  Leave the update
        // to the corresponding data connection & transition connection.
        iCS_EditorObject.EdgeEnum p1Edge= p1.Edge;
        iCS_EditorObject.EdgeEnum p2Edge= p2.Edge;
        UpdatePortEdgesInternal(p1, p2);
        if(p1Edge != p1.Edge) SetDirty(p1);
        if(p2Edge != p2.Edge) SetDirty(p2);
    }
    void UpdatePortEdgesInternal(iCS_EditorObject p1, iCS_EditorObject p2) {
        // Reset edge information.
        p1.Edge= iCS_EditorObject.EdgeEnum.None;
        p2.Edge= iCS_EditorObject.EdgeEnum.None;
        UpdatePortEdgeHardConstraints(p1);
        UpdatePortEdgeHardConstraints(p2);
        if(p1.Edge != iCS_EditorObject.EdgeEnum.None && p2.Edge != iCS_EditorObject.EdgeEnum.None) return;
        iCS_EditorObject p1Parent= GetParent(p1);
        iCS_EditorObject p2Parent= GetParent(p2);
        // Verify connection between nested nodes.
        Rect parent1Rect= GetLayoutPosition(p1Parent);
        Rect parent2Rect= GetLayoutPosition(p2Parent);
        // Nested
        if(IsChildOf(p1Parent, p2Parent) ||
           IsChildOf(p2Parent, p1Parent)) {               
            iCS_EditorObject parent= null;
            iCS_EditorObject child= null;
            iCS_EditorObject pPort= null;
            iCS_EditorObject cPort= null;
            if(IsChildOf(p1Parent, p2Parent)) {
                parent= p2Parent;
                child= p1Parent;
                pPort= p2;
                cPort= p1;
            } else {
                parent= p1Parent;
                child= p2Parent;
                pPort= p1;
                cPort= p2;
            }
            Rect parentPos= GetLayoutPosition(parent);
            Rect childPos = GetLayoutPosition(child);
            Rect childLocalPos= new Rect(childPos.x-parentPos.x, childPos.y-parentPos.y, childPos.width, childPos.height);
            float dx= parentPos.width-childLocalPos.xMax;
            float dy= parentPos.height-childLocalPos.yMax;
            if(childLocalPos.x < childLocalPos.y) {
                if(dx < dy) {
                    if(childLocalPos.x < dx) {
                        pPort.Edge= iCS_EditorObject.EdgeEnum.Left;
                        cPort.Edge= iCS_EditorObject.EdgeEnum.Left;
                    } else {
                        pPort.Edge= iCS_EditorObject.EdgeEnum.Right;
                        cPort.Edge= iCS_EditorObject.EdgeEnum.Right;                        
                    }
                } else {
                    if(childLocalPos.x < dy) {
                        pPort.Edge= iCS_EditorObject.EdgeEnum.Left;
                        cPort.Edge= iCS_EditorObject.EdgeEnum.Left;                        
                    } else {
                        pPort.Edge= iCS_EditorObject.EdgeEnum.Bottom;
                        cPort.Edge= iCS_EditorObject.EdgeEnum.Bottom;                        
                    }
                }
            } else {
                if(dx < dy) {
                    if(childLocalPos.y < dx) {
                        pPort.Edge= iCS_EditorObject.EdgeEnum.Top;
                        cPort.Edge= iCS_EditorObject.EdgeEnum.Top;
                    } else {
                        pPort.Edge= iCS_EditorObject.EdgeEnum.Right;
                        cPort.Edge= iCS_EditorObject.EdgeEnum.Right;                        
                    }
                } else {
                    if(childLocalPos.y < dy) {
                        pPort.Edge= iCS_EditorObject.EdgeEnum.Top;
                        cPort.Edge= iCS_EditorObject.EdgeEnum.Top;                        
                    } else {
                        pPort.Edge= iCS_EditorObject.EdgeEnum.Bottom;
                        cPort.Edge= iCS_EditorObject.EdgeEnum.Bottom;                        
                    }
                }                
            }
            return;
        }
        // Horizontal
        if(parent1Rect.xMin <= parent2Rect.xMin && parent1Rect.xMax > parent2Rect.xMin ||
           parent2Rect.xMin <= parent1Rect.xMin && parent2Rect.xMax > parent1Rect.xMin) {
            if(parent1Rect.yMin < parent2Rect.yMin) {
                p1.Edge= iCS_EditorObject.EdgeEnum.Bottom;
                p2.Edge= iCS_EditorObject.EdgeEnum.Top;
            } else {
                p1.Edge= iCS_EditorObject.EdgeEnum.Top;
                p2.Edge= iCS_EditorObject.EdgeEnum.Bottom;                
            }
            return;
        }
        // Vertical
        if(parent1Rect.yMin <= parent2Rect.yMin && parent1Rect.yMax > parent2Rect.yMin ||
           parent2Rect.yMin <= parent1Rect.yMin && parent2Rect.yMax > parent1Rect.yMin) {
            if(parent1Rect.xMin < parent2Rect.xMin) {
                p1.Edge= iCS_EditorObject.EdgeEnum.Right;
                p2.Edge= iCS_EditorObject.EdgeEnum.Left;
            } else {
                p1.Edge= iCS_EditorObject.EdgeEnum.Left;
                p2.Edge= iCS_EditorObject.EdgeEnum.Right;                
            }
            return;
        }
        // Diagonal
        if(parent1Rect.xMin < parent2Rect.xMin) {
            if(parent1Rect.yMin < parent2Rect.yMin) {
                if(p1.Source == p2.InstanceId) {
                    p1.Edge= iCS_EditorObject.EdgeEnum.Bottom;
                    p2.Edge= iCS_EditorObject.EdgeEnum.Left;
                } else {
                    p1.Edge= iCS_EditorObject.EdgeEnum.Right;
                    p2.Edge= iCS_EditorObject.EdgeEnum.Top;                    
                }
            } else {
                if(p1.Source == p2.InstanceId) {
                    p1.Edge= iCS_EditorObject.EdgeEnum.Right;
                    p2.Edge= iCS_EditorObject.EdgeEnum.Bottom;
                } else {
                    p1.Edge= iCS_EditorObject.EdgeEnum.Top;
                    p2.Edge= iCS_EditorObject.EdgeEnum.Left;                    
                }                
            }
            return;
        }
        if(parent1Rect.yMin < parent2Rect.yMin) {
            if(p1.Source == p2.InstanceId) {
                p1.Edge= iCS_EditorObject.EdgeEnum.Left;
                p2.Edge= iCS_EditorObject.EdgeEnum.Top;
            } else {
                p1.Edge= iCS_EditorObject.EdgeEnum.Bottom;
                p2.Edge= iCS_EditorObject.EdgeEnum.Right;                    
            }
        } else {
            if(p1.Source == p2.InstanceId) {
                p1.Edge= iCS_EditorObject.EdgeEnum.Top;
                p2.Edge= iCS_EditorObject.EdgeEnum.Right;
            } else {
                p1.Edge= iCS_EditorObject.EdgeEnum.Left;
                p2.Edge= iCS_EditorObject.EdgeEnum.Bottom;                    
            }            
        }
    }
    void UpdatePortEdgeHardConstraints(iCS_EditorObject port) {
        if(port.IsEnablePort) {
            port.Edge= iCS_EditorObject.EdgeEnum.Top;
            return;            
        }
        if(port.IsDataPort) {
            port.Edge= port.IsInputPort ? iCS_EditorObject.EdgeEnum.Left : iCS_EditorObject.EdgeEnum.Right;
            return;
        }
    }
    // ----------------------------------------------------------------------
    public void PositionOnEdge(iCS_EditorObject port) {
        var parent= GetParent(port);
        var parentPos= GetLayoutPosition(parent);
        switch(GetClosestEdge(parent, Math3D.ToVector2(GetLayoutPosition(port)))) {
            case iCS_EditorObject.EdgeEnum.Top:      port.LocalPosition.y= 0; break; 
            case iCS_EditorObject.EdgeEnum.Bottom:   port.LocalPosition.y= parentPos.height; break;
            case iCS_EditorObject.EdgeEnum.Left:     port.LocalPosition.x= 0; break;
            case iCS_EditorObject.EdgeEnum.Right:    port.LocalPosition.x= parentPos.width; break;
        }
    }
	// ----------------------------------------------------------------------
    public iCS_EditorObject GetOverlappingPort(iCS_EditorObject port) {
        iCS_EditorObject foundPort= null;
        Rect tmp= GetLayoutPosition(port);
        Vector2 position= new Vector2(tmp.x, tmp.y);
        FilterWith(
            p=> p.IsPort && p != port && IsVisible(p),
            p=> {
                tmp= GetLayoutPosition(p);
                Vector2 pPos= new Vector2(tmp.x, tmp.y);
                float distance= Vector2.Distance(pPos, position);
                if(distance <= 1.5*iCS_Config.PortSize) {
                    foundPort= p;
                }
            }
        );
        return foundPort;
    }	
}
