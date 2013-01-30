using UnityEngine;
using System;
using System.Collections;

public partial class iCS_IStorage {
    // ======================================================================
    // Port Archiving / Initial Value
    // ----------------------------------------------------------------------
	public void LoadInitialPortValueFromArchive(iCS_EditorObject port) {
		if(!port.IsInDataPort) return;
		if(port.SourceId != -1) return;
		if(iCS_Strings.IsEmpty(port.InitialValueArchive)) {
			port.InitialValue= null;
			return;
		}
		iCS_Coder coder= new iCS_Coder(port.InitialValueArchive);
		port.InitialValue= coder.DecodeObjectForKey("InitialValue", Storage);
	}
    // ----------------------------------------------------------------------
    public void StoreInitialPortValueInArchive(iCS_EditorObject port) {
        if(port.InitialValue == null) {
            port.InitialValueArchive= null;
            return;
        }
		iCS_Coder coder= new iCS_Coder();
		coder.EncodeObject("InitialValue", port.InitialValue, Storage);
		port.InitialValueArchive= coder.Archive;         
    }
}