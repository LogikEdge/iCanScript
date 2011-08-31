using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AP_Node : AP_Aggregate {
    // ======================================================================
    // OBJECT LIFETIME MANAGEMENT
    // ----------------------------------------------------------------------
    protected new AP_Node Init(string _name, AP_Aggregate _parent) {
        base.Init(_name, _parent);
        
        // Create ports for each field taged with InPort or OutPort.
        foreach(var field in GetInputFields()) {
            AP_DataPort.CreateInstance(field.Name, this, AP_DataPort.DirectionEnum.In);                                
        }
        foreach(var field in GetOutputFields()) {
            AP_DataPort.CreateInstance(field.Name, this, AP_DataPort.DirectionEnum.Out);            
        }
        return this;
    }
    
    // ======================================================================
    // LAYOUT FUNCTIONS
    // ----------------------------------------------------------------------
    public Vector2 GetTopLeftCorner()     { return new Vector2(Position.xMin, Position.yMin); }
    public Vector2 GetTopRightCorner()    { return new Vector2(Position.xMax, Position.yMin); }
    public Vector2 GetBottomLeftCorner()  { return new Vector2(Position.xMin, Position.yMax); }
    public Vector2 GetBottomRightCorner() { return new Vector2(Position.xMax, Position.yMax); }

    // ----------------------------------------------------------------------
    // Moves the node without changing its size.
    public void SetInitialPosition(Vector2 _initialPosition) {
        myLocalPosition.x= _initialPosition.x - Position.x;
        myLocalPosition.y= _initialPosition.y - Position.y;
        IsEditorDirty= true;
    }

    // ----------------------------------------------------------------------
    // Moves the node without changing its size.
    public void MoveTo(Vector2 _newPos) {
        DeltaMove(new Vector2(_newPos.x - Position.x, _newPos.y - Position.y));
    }

    // ----------------------------------------------------------------------
    // Moves the node without changing its size.
    public void DeltaMove(Vector2 _delta) {
        // Move the node
        DeltaMoveInternal(_delta);
        // Resolve collision between siblings.
        LayoutParent(_delta);
	}

    // ----------------------------------------------------------------------
    // Moves the node without changing its size.
    void DeltaMoveInternal(Vector2 _delta) {
        if(MathfExt.IsNotZero(_delta)) {
            myLocalPosition.x+= _delta.x;
            myLocalPosition.y+= _delta.y;
            IsEditorDirty= true;
        }
    }

    // ----------------------------------------------------------------------
    // Recompute the layout of a parent node.
    // Returns "true" if the new layout is within the window area.
    public override void DoLayout() {
        // Don't layout node if it is not visible.
        if(!IsVisible) return;
        
        // Resolve collision on children.
        ResolveCollisionOnChildren(Vector2.zero);
        
        // Determine needed child rect.
        Rect  childRect   = ComputeChildRect();

        // Compute needed width.
        float titleWidth  = AP_EditorConfig.GetNodeWidth(NameOrTypeName);
        float leftMargin  = ComputeLeftMargin();
        float rightMargin = ComputeRightMargin();
        float width       = 2.0f*AP_EditorConfig.GutterSize + Mathf.Max(titleWidth, leftMargin + rightMargin + childRect.width);

        // Process case without child nodes
        if(MathfExt.IsZero(childRect.width) || MathfExt.IsZero(childRect.height)) {
            // Compute needed height.
            List<AP_Port> leftPorts= GetLeftPorts();
            List<AP_Port> rightPorts= GetRightPorts();
            int nbOfPorts= leftPorts.Count > rightPorts.Count ? leftPorts.Count : rightPorts.Count;
            bool isCompact= nbOfPorts > 1 ? false : true;
            if(nbOfPorts == 1) {
                if(leftPorts.Count == 1)  { if(leftPorts[0].IsNameVisible)  { isCompact= false; }}
                if(rightPorts.Count == 1) { if(rightPorts[0].IsNameVisible) { isCompact= false; }}
            }
            float height= isCompact ? 
                AP_EditorConfig.MinimumNodeHeight :
                AP_EditorConfig.NodeTitleHeight+nbOfPorts*AP_EditorConfig.MinimumPortSeparation;                                

            // Apply new width and height.
            if(MathfExt.IsNotEqual(height, Position.height) || MathfExt.IsNotEqual(width, Position.width)) {
                float deltaWidth = width - Position.width;
                float deltaHeight= height - Position.height;
                Rect newPos= new Rect(Position.xMin-0.5f*deltaWidth, Position.yMin-0.5f*deltaHeight, width, height);
                SetPosition(newPos);
            }
        }
        // Process case with child nodes.
        else {
            // Adjust children local offset.
            float neededChildXOffset= AP_EditorConfig.GutterSize+leftMargin;
            float neededChildYOffset= AP_EditorConfig.GutterSize+AP_EditorConfig.NodeTitleHeight;
            if(MathfExt.IsNotEqual(neededChildXOffset, childRect.x) ||
               MathfExt.IsNotEqual(neededChildYOffset, childRect.y)) {
                   AdjustChildLocalPosition(new Vector2(neededChildXOffset-childRect.x, neededChildYOffset-childRect.y));
            }
            
            // Compute needed height.
            int nbOfLeftPorts = GetNbOfLeftPorts();
            int nbOfRightPorts= GetNbOfRightPorts();
            int nbOfPorts= nbOfLeftPorts > nbOfRightPorts ? nbOfLeftPorts : nbOfRightPorts;
            float portHeight= nbOfPorts*AP_EditorConfig.MinimumPortSeparation;                                
            float height= AP_EditorConfig.NodeTitleHeight+Mathf.Max(portHeight, childRect.height+2.0f*AP_EditorConfig.GutterSize);

            float deltaWidth = width - myLocalPosition.width;
            float deltaHeight= height - myLocalPosition.height;
            float xMin= Position.xMin-0.5f*deltaWidth;
            float yMin= Position.yMin-0.5f*deltaHeight;
            if(MathfExt.IsNotEqual(xMin, Position.xMin) || MathfExt.IsNotEqual(yMin, Position.yMin) ||
               MathfExt.IsNotEqual(width, myLocalPosition.width) || MathfExt.IsNotEqual(height, myLocalPosition.height)) {
                Rect newPos= new Rect(xMin, yMin, width, height);
                SetPosition(newPos);
            }
        }

        // Layout child ports
        LayoutPorts();
    }

    // ----------------------------------------------------------------------
    void AdjustChildLocalPosition(Vector2 _delta) {
        ForEachChild<AP_Node>( (child)=> { child.DeltaMoveInternal(_delta); } );
    }
    
    // ----------------------------------------------------------------------
    void SetPosition(Rect _newPos) {
        // Adjust node size.
        myLocalPosition.width = _newPos.width;
        myLocalPosition.height= _newPos.height;
        // Reposition node.
        if(Parent == null) {
            myLocalPosition.x= _newPos.x;
            myLocalPosition.y= _newPos.y;            
        }
        else {
            Rect deltaMove= new Rect(_newPos.xMin-Position.xMin, _newPos.yMin-Position.yMin, _newPos.width-Position.width, _newPos.height-Position.height);
            myLocalPosition.x+= deltaMove.x;
            myLocalPosition.y+= deltaMove.y;
            myLocalPosition.width= _newPos.width;
            myLocalPosition.height= _newPos.height;
            float separationX= Mathf.Abs(deltaMove.x) > Mathf.Abs(deltaMove.width) ? deltaMove.x : deltaMove.width;
            float separationY= Mathf.Abs(deltaMove.y) > Mathf.Abs(deltaMove.height) ? deltaMove.y : deltaMove.height;
            LayoutParent(new Vector2(separationX, separationY));
        }
    }

    // ----------------------------------------------------------------------
    void LayoutParent(Vector2 _deltaMove) {
        AP_Node parentNode= Parent as AP_Node;
        if(parentNode != null) {
            parentNode.ResolveCollision(_deltaMove);
            parentNode.Layout();
		}        
    }

    
    // ======================================================================
    // CONNECTOR MANAGEMENT
    // ----------------------------------------------------------------------
    // Recomputes the port position.
    public void LayoutPorts() {
		// Gather all ports.
        List<AP_Port> topPorts= new List<AP_Port>();
        List<AP_Port> bottomPorts= new List<AP_Port>();
        List<AP_Port> leftPorts= new List<AP_Port>();
        List<AP_Port> rightPorts= new List<AP_Port>();
        ForEachChild<AP_Port>(
            (port)=> {
                if(port.IsOnTopEdge)         { topPorts.Add(port); }
                else if(port.IsOnBottomEdge) { bottomPorts.Add(port); }
                else if(port.IsOnLeftEdge)   { leftPorts.Add(port); }
                else if(port.IsOnRightEdge)  { rightPorts.Add(port); }
            }
        );
        
        // Relayout top ports.
        if(topPorts.Count != 0) {
            SortPorts(topPorts);
            float xStep= Position.width / topPorts.Count;
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
            float xStep= Position.width / bottomPorts.Count;
            for(int i= 0; i < bottomPorts.Count; ++i) {
                if(bottomPorts[i].IsBeingDragged == false) {
                    bottomPorts[i].LocalPosition.x= (i+0.5f) * xStep;
                    bottomPorts[i].LocalPosition.y= Position.height;                
                }
            }            
        }

        // Relayout left ports.
        if(leftPorts.Count != 0) {
            SortPorts(leftPorts);
            float topOffset= IsCompactNode() ? 0 : AP_EditorConfig.NodeTitleHeight;
            float yStep= (Position.height-topOffset) / leftPorts.Count;
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
            float topOffset= IsCompactNode() ? 0 : AP_EditorConfig.NodeTitleHeight;
            float yStep= (Position.height-topOffset) / rightPorts.Count;
            for(int i= 0; i < rightPorts.Count; ++i) {
                if(rightPorts[i].IsBeingDragged == false) {
                    rightPorts[i].LocalPosition.x= Position.width;
                    rightPorts[i].LocalPosition.y= topOffset + (i+0.5f) * yStep;                
    
                }
            }
        }        
    }

    // ----------------------------------------------------------------------
    // Sorts the given port according to their relative positions.
    void SortPorts(List<AP_Port> _ports) {
        for(int i= 0; i < _ports.Count-1; ++i) {
            float sqrMag= _ports[i].LocalPosition.sqrMagnitude;
            for(int j= i+1; j < _ports.Count; ++j) {
				float sqrMag2= _ports[j].LocalPosition.sqrMagnitude;
				if(sqrMag > sqrMag2) {
                    AP_Port p= _ports[i];
                    _ports[i]= _ports[j];
                    _ports[j]= p;
					sqrMag= sqrMag2;
                }
            }
        }
    }
        
    // ----------------------------------------------------------------------
    // Returns the width of the node title.
    float ComputeTitleWidth() {
        return AP_EditorConfig.GetNodeWidth(NameOrTypeName);
    }
    // ----------------------------------------------------------------------
    // Returns the inner left margin.
    float ComputeLeftMargin() {
        float LeftMargin= 0;
        ForEachLeftPort(
            (port)=> {
                if(port.IsNameVisible) {
                    Vector2 labelSize= AP_EditorConfig.GetPortLabelSize(port.Name);
                    float nameSize= labelSize.x+AP_EditorConfig.PortSize;
                    if(LeftMargin < nameSize) LeftMargin= nameSize;
                }
            }
        );
        return LeftMargin;
    }
    // ----------------------------------------------------------------------
    // Returns the inner right margin.
    float ComputeRightMargin() {
        float RightMargin= 0;
        ForEachRightPort(
            (port) => {
                if(port.IsNameVisible) {
                    Vector2 labelSize= AP_EditorConfig.GetPortLabelSize(port.Name);
                    float nameSize= labelSize.x+AP_EditorConfig.PortSize;
                    if(RightMargin < nameSize) RightMargin= nameSize;
                }
            }
        );
        return RightMargin;
    }
    // ----------------------------------------------------------------------
    // Returns the inner top margin.
    float ComputeTopMargin() {
        return AP_EditorConfig.GetNodeHeight(NameOrTypeName);
    }
    // ----------------------------------------------------------------------
    // Returns the inner bottom margin.
    float ComputeBottomMargin() {
        return 0;
    }
    // ----------------------------------------------------------------------
    // Returns the space used by all children.
    Rect ComputeChildRect() {
        // Compute child space.
        Rect childRect= new Rect(0.5f*myLocalPosition.width,0.5f*myLocalPosition.height,0,0);
        ForEachChild<AP_Node>(
            (child)=> {
                if(child.IsVisible) {
                    childRect= Physics2D.Merge(childRect, child.myLocalPosition);
                }
            }
        );
        return childRect;
    }
    
    
    // ======================================================================
    // COLLISION FUNCTIONS
    // ----------------------------------------------------------------------
    // Resolve collision on parents.
    public void ResolveCollision(Vector2 _delta) {
        ResolveCollisionOnChildren(_delta);
        AP_Node parentNode= Parent as AP_Node;
        if(parentNode != null) parentNode.ResolveCollision(_delta);
    }

    // ----------------------------------------------------------------------
    // Resolves the collision between children.  "true" is returned if a
    // collision has occured.
    public void ResolveCollisionOnChildren(Vector2 _delta) {
        bool didCollide= false;
        for(int i= 0; i < Children.Count-1; ++i) {
            AP_Node child1= Children[i] as AP_Node;
            if(child1 == null || !child1.IsVisible) continue;
            for(int j= i+1; j < Children.Count; ++j) {
                AP_Node child2= Children[j] as AP_Node;
                if(child2 == null || !child2.IsVisible) continue;
                didCollide |= child1.ResolveCollisionBetweenTwoNodes(child2, _delta);                            
            }
        }
        if(didCollide) ResolveCollisionOnChildren(_delta);
    }

    // ----------------------------------------------------------------------
    // Resolves collision between two nodes. "true" is returned if a collision
    // has occured.
    public bool ResolveCollisionBetweenTwoNodes(AP_Node _other, Vector2 _delta) {
        // Nothing to do if they don't collide.
        if(!DoesCollideWithGutter(_other)) return false;

        // Compute penetration.
        Vector2 penetration= GetSeperationVector(_other.Position);
		if(Mathf.Abs(penetration.x) < 1.0f && Mathf.Abs(penetration.y) < 1.0f) return false;

		// Seperate using the known movement.
        if( !MathfExt.IsZero(_delta) ) {
    		if(Vector2.Dot(_delta, penetration) > 0) {
    		    _other.DeltaMoveInternal(penetration);
    		}
    		else {
    		    DeltaMoveInternal(-penetration);
    		}            
    		return true;
        }

		// Seperate nodes by the penetration that is not a result of movement.
        penetration*= 0.5f;
        _other.DeltaMoveInternal(penetration);
        DeltaMoveInternal(-penetration);
        return true;
    }

    // ----------------------------------------------------------------------
    // Returns if the given rectangle collides with the node.
    public bool DoesCollide(AP_Node _node) {
        return Physics2D.DoesCollide(Position, _node.Position);
    }

    // ----------------------------------------------------------------------
    // Returns if the given rectangle collides with the node.
    public bool DoesCollideWithGutter(AP_Node _node) {
        return Physics2D.DoesCollide(RectWithGutter(Position), _node.Position);
    }

    // ----------------------------------------------------------------------
    Rect RectWithGutter(Rect _rect) {
        float gutterSize= AP_EditorConfig.GutterSize;
        float gutterSize2= 2.0f*gutterSize;
        return new Rect(_rect.x-gutterSize, _rect.y-gutterSize, _rect.width+gutterSize2, _rect.height+gutterSize2);        
    }
    
    // ----------------------------------------------------------------------
	// Returns the seperation vector of two colliding nodes.
	public Vector2 GetSeperationVector(Rect _rect) {
        Rect myRect= RectWithGutter(Position);
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
	public Vector2 GetSeperationVector(AP_Node _node) {
	    return GetSeperationVector(_node.Position);
	}

    // ----------------------------------------------------------------------
    // Returns true if the given point is inside the node coordinates.
    public bool IsInside(Vector2 _point) {
        return Physics2D.IsInside(_point, Position);
    }

    // ----------------------------------------------------------------------
    // Returns true if we can draw a compact node.
    public bool IsCompactNode() {
        List<AP_Port> leftPorts= GetLeftPorts();
        if(leftPorts.Count > 1) return false;
        foreach(var port in leftPorts) {
            if(port.IsNameVisible) return false;
        }
        List<AP_Port> rightPorts= GetRightPorts();
        if(rightPorts.Count > 1) return false;
        foreach(var port in rightPorts) {
            if(port.IsNameVisible) return false;
        }
        return true;
    }

    // ----------------------------------------------------------------------
    // Returns all ports position on the top edge.
    public List<AP_Port> GetTopPorts() {
        List<AP_Port> ports= new List<AP_Port>();
        ForEachTopPort( (port)=> { ports.Add(port); } );
        return ports;
    }

    // ----------------------------------------------------------------------
    // Returns all ports position on the bottom edge.
    public List<AP_Port> GetBottomPorts() {
        List<AP_Port> ports= new List<AP_Port>();
        ForEachBottomPort( (port)=> { ports.Add(port); } );
        return ports;
    }

    // ----------------------------------------------------------------------
    // Returns all ports position on the left edge.
    public List<AP_Port> GetLeftPorts() {
        List<AP_Port> ports= new List<AP_Port>();
        ForEachLeftPort( (port)=> { ports.Add(port); } );
        return ports;        
    }

    // ----------------------------------------------------------------------
    // Returns all ports position on the right edge.
    public List<AP_Port> GetRightPorts() {
        List<AP_Port> ports= new List<AP_Port>();
        ForEachRightPort( (port)=> { ports.Add(port); } );
        return ports;
    }

    // ----------------------------------------------------------------------
    // Returns the number of ports on the top edge.
    public int GetNbOfTopPorts() {
        int nbOfPorts= 0;
        ForEachTopPort( (port)=> { ++nbOfPorts; } );
        return nbOfPorts;
    }

    // ----------------------------------------------------------------------
    // Returns the number of ports on the bottom edge.
    public int GetNbOfBottomPorts() {
        int nbOfPorts= 0;
        ForEachBottomPort( (port)=> { ++nbOfPorts; } );
        return nbOfPorts;
    }

    // ----------------------------------------------------------------------
    // Returns the number of ports on the left edge.
    public int GetNbOfLeftPorts() {
        int nbOfPorts= 0;
        ForEachLeftPort( (port)=> { ++nbOfPorts; } );
        return nbOfPorts;
    }

    // ----------------------------------------------------------------------
    // Returns the number of ports on the right edge.
    public int GetNbOfRightPorts() {
        int nbOfPorts= 0;
        ForEachRightPort( (port)=> { ++nbOfPorts; } );
        return nbOfPorts;
    }


    // ----------------------------------------------------------------------
    public void ForEachTopPort(System.Action<AP_Port> fnc) {
        ForEachChild<AP_Port>(
            (port)=> {
                if(port.IsOnTopEdge) {
                    fnc(port);
                }
            }
        );        
    }
    
    // ----------------------------------------------------------------------
    public void ForEachBottomPort(System.Action<AP_Port> fnc) {
        ForEachChild<AP_Port>(
            (port)=> {
                if(port.IsOnBottomEdge) {
                    fnc(port);
                }
            }
        );        
    }
    
    // ----------------------------------------------------------------------
    public void ForEachLeftPort(System.Action<AP_Port> fnc) {
        ForEachChild<AP_Port>(
            (port)=> {
                if(port.IsOnLeftEdge) {
                    fnc(port);
                }
            }
        );        
    }
    
    // ----------------------------------------------------------------------
    public void ForEachRightPort(System.Action<AP_Port> fnc) {
        ForEachChild<AP_Port>(
            (port)=> {
                if(port.IsOnRightEdge) {
                    fnc(port);
                }
            }
        );        
    }
    
    
    // ======================================================================
    // PROPERTIES
    // ----------------------------------------------------------------------
    public Rect Position {
        get {
            AP_Node parentNode= Parent as AP_Node;
            if(parentNode == null) return myLocalPosition;
            return new Rect(parentNode.Position.x+myLocalPosition.x,
                            parentNode.Position.y+myLocalPosition.y,
                            myLocalPosition.width,
                            myLocalPosition.height);
        }
        set { SetPosition(value); }
    }
	[SerializeField]
    Rect myLocalPosition= new Rect(AP_EditorConfig.NodeInitialOffset,
                                   AP_EditorConfig.NodeInitialOffset,
                                   AP_EditorConfig.NodeInitialWidth,
                                   AP_EditorConfig.NodeInitialHeight);

}
