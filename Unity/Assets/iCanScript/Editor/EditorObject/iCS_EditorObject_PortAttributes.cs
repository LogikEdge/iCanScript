using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using P=Prelude;

// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
//  PORT ATTRIBUTES
// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
public partial class iCS_EditorObject {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
	public object		    InitialValue= null;

    // ======================================================================
	// Port source related attributes.
	// ----------------------------------------------------------------------
    public int PortIndex {
		get { return EngineObject.PortIndex; }
		set {
            var engineObject= EngineObject;
			if(engineObject.PortIndex == value) return;
			engineObject.PortIndex= value;
			IsDirty= true;
		}
	}
	// ----------------------------------------------------------------------
    public int SourceId {
		get { return EngineObject.SourceId; }
		set {
            var engineObject= EngineObject;
            if(engineObject.SourceId == value) return;
			EngineObject.SourceId= value;
			IsDirty= true;
		}
	}
	// ----------------------------------------------------------------------
    public iCS_EditorObject Source {
		get { return SourceId != -1 ? myIStorage[SourceId] : null; }
		set { SourceId= (value != null ? value.InstanceId : -1); }
	}
	// ----------------------------------------------------------------------
	public iCS_EditorObject SourceEndPoint {
		get {
			var iter= this;
			for(var tmp= iter.Source; tmp != null; tmp= tmp.Source) {
				iter= tmp;
			}
			return iter;
		}
	}
	// ----------------------------------------------------------------------
	public iCS_EditorObject[] Destinations {
		get {
			return Filter(c=> c.IsPort && c.SourceId == InstanceId).ToArray();
		}
	}
	// ----------------------------------------------------------------------
	public iCS_EditorObject[] DestinationEndPoints {
		get {
			var result= new List<iCS_EditorObject>();
			BuildDestinationEndPoints(ref result);
			return result.ToArray();
		}
	}
	private void BuildDestinationEndPoints(ref List<iCS_EditorObject> r) {
		var destinations= Destinations;
		if(destinations.Length == 0) {
			r.Add(this);
		} else {
			foreach(var p in destinations) {
				p.BuildDestinationEndPoints(ref r);
			}
		}
	}
	// ----------------------------------------------------------------------
	public P.Tuple<iCS_EditorObject,iCS_EditorObject>[] Connections {
		get {
			var result= new List<P.Tuple<iCS_EditorObject,iCS_EditorObject> >();
			var source= SourceEndPoint;
			foreach(var destination in source.DestinationEndPoints) {
				result.Add(new P.Tuple<iCS_EditorObject,iCS_EditorObject>(source, destination));
			}			        
			return result.ToArray();
		}
	}
	// ----------------------------------------------------------------------
	public bool IsPartOfConnection(iCS_EditorObject testedPort) {
		if(this == testedPort) return true;
		var src= Source;
		if(src == null) return false;
		return src.IsPartOfConnection(testedPort);
	} 
	
    // ======================================================================
	// Port value attributes.
    public string InitialValueArchive {
		get { return EngineObject.InitialValueArchive; }
		set {
            var engineObject= EngineObject;
			if(engineObject.InitialValueArchive == value) return;
			engineObject.InitialValueArchive= value;
			IsDirty= true;
		}
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
            int retry= 0;
			while(port.Source != null) {
			    port= port.Source;
                if(++retry > 100) {
                    Debug.LogWarning("iCanScript: Circular port connection detected on: "+port.ParentNode.Name+"."+port.Name);
                    return null;
                }
		    }
			iCS_IParameters funcBase= myIStorage.GetRuntimeObject(port) as iCS_IParameters;
			if(funcBase != null) {
			    return funcBase.GetParameter(0);
			}
			funcBase= myIStorage.GetRuntimeObject(port.Parent) as iCS_IParameters;
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
			iCS_IParameters funcBase= myIStorage.GetRuntimeObject(port) as iCS_IParameters;
			if(funcBase != null) {
			    return funcBase.GetParameter(0);
			}
			funcBase= myIStorage.GetRuntimeObject(port.Parent) as iCS_IParameters;
			return funcBase == null ? port.InitialPortValue : null;			
		}
		set {
	        if(!IsInDataPort) return;
	        // Just set the port if it has its own runtime.
			iCS_IParameters funcBase= myIStorage.GetRuntimeObject(this) as iCS_IParameters;
	        if(funcBase != null) {
	            funcBase.SetParameter(0, value);
	            return;
	        }
	        // Propagate value for module port.
	        if(IsModulePort) {
	            iCS_EditorObject[] connectedPorts= Destinations;
	            foreach(var cp in connectedPorts) {
	                cp.RuntimePortValue= value;
	            }
	            return;
	        }
	        if(PortIndex < 0) return;
	        iCS_EditorObject parent= Parent;
	        if(parent == null) return;
	        // Get runtime object if it exists.
	        iCS_IParameters runtimeObject= myIStorage.GetRuntimeObject(parent) as iCS_IParameters;
	        if(runtimeObject == null) return;
	        runtimeObject.SetParameter(PortIndex, value);			
		}
	}
}
