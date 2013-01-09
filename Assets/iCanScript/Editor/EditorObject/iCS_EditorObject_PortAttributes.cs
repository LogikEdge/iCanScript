using UnityEngine;
using System.Collections;

// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
//  PORT ATTRIBUTES
// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
public partial class iCS_EditorObject {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
	public object		    InitialValue= null;

    // ======================================================================
    // Port layout attributes.
	public bool IsOnTopEdge         { get { return Edge == iCS_EdgeEnum.Top; }}
    public bool IsOnBottomEdge      { get { return Edge == iCS_EdgeEnum.Bottom; }}
    public bool IsOnRightEdge       { get { return Edge == iCS_EdgeEnum.Right; }}
    public bool IsOnLeftEdge        { get { return Edge == iCS_EdgeEnum.Left; }}
    public bool IsOnHorizontalEdge  { get { return IsOnTopEdge   || IsOnBottomEdge; }}
    public bool IsOnVerticalEdge    { get { return IsOnRightEdge || IsOnLeftEdge; }}
    // ----------------------------------------------------------------------
    public iCS_EdgeEnum Edge {
		get { return EngineObject.Edge; }
		set {
            var engineObject= EngineObject;
            if(engineObject.Edge == value) return;
            engineObject.Edge= value;
            if(!IsFloating) CleanupPortEdgePosition();
            IsDirty= true;
		}
	}
    // ----------------------------------------------------------------------
    public float PortPositionRatio {
        get { return EngineObject.PortPositionRatio; }
		set { EngineObject.PortPositionRatio= value; }
    }
    
    
    // ======================================================================
	// Port source related attributes.
    public int PortIndex {
		get { return EngineObject.PortIndex; }
		set { EngineObject.PortIndex= value; }
	}
    public int SourceId {
		get { return EngineObject.SourceId; }
		set {
            var engineObject= EngineObject;
            if(engineObject.SourceId == value) return;
			EngineObject.SourceId= value;
			IsDirty= true;
		}
	}
    public iCS_EditorObject Source {
		get { return SourceId != -1 ? myIStorage[SourceId] : null; }
		set { SourceId= (value != null ? value.InstanceId : -1); }
	}

    // ======================================================================
	// Port value attributes.
    public string InitialValueArchive {
		get { return EngineObject.InitialValueArchive; }
		set { EngineObject.InitialValueArchive= value;}
	}
	// ----------------------------------------------------------------------
	public object InitialPortValue {
		get {
			if(!IsInDataPort) return null;
			if(SourceId != -1) return null;
			return InitialValue;			
		}
		set {
			if(!IsInDataPort) return;
			if(SourceId != -1) return;
			InitialValue= value;
	        myIStorage.StoreInitialPortValueInArchive(this);			
		}
	}
	// ----------------------------------------------------------------------
    // Fetches the runtime value if it exists, otherwise returns the initial value
	public object PortValue {
		get {
			if(!IsDataPort) return null;
			var port= this;
			while(port.Source != null) port= port.Source;
			iCS_IParams funcBase= myIStorage.GetRuntimeObject(port) as iCS_IParams;
			if(funcBase != null) {
			    return funcBase.GetParameter(0);
			}
			funcBase= myIStorage.GetRuntimeObject(port.Parent) as iCS_IParams;
			return funcBase == null ? port.InitialPortValue : funcBase.GetParameter(port.PortIndex);			
		}
		set {
			InitialPortValue= value;
			RuntimePortValue= value;
	        Parent.IsDirty= true;			
		}
	}
	// ----------------------------------------------------------------------
	public object RuntimePortValue {
		get {
			if(!IsDataPort) return null;
			var port= this;
			while(port.Source != null) port= port.Source;
			iCS_IParams funcBase= myIStorage.GetRuntimeObject(port) as iCS_IParams;
			if(funcBase != null) {
			    return funcBase.GetParameter(0);
			}
			funcBase= myIStorage.GetRuntimeObject(port.Parent) as iCS_IParams;
			return funcBase == null ? port.InitialPortValue : null;			
		}
		set {
	        if(!IsInDataPort) return;
	        // Just set the port if it has its own runtime.
			iCS_IParams funcBase= myIStorage.GetRuntimeObject(this) as iCS_IParams;
	        if(funcBase != null) {
	            funcBase.SetParameter(0, value);
	            return;
	        }
	        // Propagate value for module port.
	        if(IsModulePort) {
	            iCS_EditorObject[] connectedPorts= myIStorage.FindConnectedPorts(this);
	            foreach(var cp in connectedPorts) {
	                cp.RuntimePortValue= value;
	            }
	            return;
	        }
	        if(PortIndex < 0) return;
	        iCS_EditorObject parent= Parent;
	        if(parent == null) return;
	        // Get runtime object if it exists.
	        iCS_IParams runtimeObject= myIStorage.GetRuntimeObject(parent) as iCS_IParams;
	        if(runtimeObject == null) return;
	        runtimeObject.SetParameter(PortIndex, value);			
		}
	}
}
