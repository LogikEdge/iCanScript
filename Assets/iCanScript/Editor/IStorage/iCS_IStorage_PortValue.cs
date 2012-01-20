using UnityEngine;
using System;
using System.Collections;

public partial class iCS_IStorage {
    // ----------------------------------------------------------------------
	public void LoadInitialPortValueFromArchive(iCS_EditorObject port) {
		if(!port.IsInDataPort) return;
		if(port.Source != -1) return;
		if(port.InitialValueArchive == null || port.InitialValueArchive == "") {
			TreeCache[port.InstanceId].InitialValue= null;
			return;
		}
		iCS_Coder coder= new iCS_Coder(port.InitialValueArchive);
		TreeCache[port.InstanceId].InitialValue= coder.DecodeObjectForKey("InitialValue", Storage);
	}
    // ----------------------------------------------------------------------
	public object GetInitialPortValue(iCS_EditorObject port) {
		if(!port.IsInDataPort) return null;
		if(port.Source != -1) return null;
		return TreeCache[port.InstanceId].InitialValue;
	}
    // ----------------------------------------------------------------------
	public void SetInitialPortValue(iCS_EditorObject port, object value) {
		if(!port.IsInDataPort) return;
		if(port.Source != -1) return;
		TreeCache[port.InstanceId].InitialValue= value;
		iCS_Coder coder= new iCS_Coder();
		coder.EncodeObject("InitialValue", value, Storage);
		port.InitialValueArchive= coder.Archive; 
	}
    // ----------------------------------------------------------------------
	public object GetPortValue(iCS_EditorObject port) {
		if(!port.IsDataPort) return null;
		iCS_FunctionBase funcBase= GetRuntimeObject(GetParent(port)) as iCS_FunctionBase;
		return funcBase == null ? GetInitialPortValue(port) : funcBase[port.PortIndex];
	}
    // ----------------------------------------------------------------------
	public void SetPortValue(iCS_EditorObject port, object value) {
		if(!port.IsDataPort) return;
		iCS_FunctionBase funcBase= GetRuntimeObject(GetParent(port)) as iCS_FunctionBase;
		if(funcBase == null) return;
		funcBase[port.PortIndex]= value;
	}
}
