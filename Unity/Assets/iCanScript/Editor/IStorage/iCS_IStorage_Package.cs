using UnityEngine;
using System;
using System.Collections;

public partial class iCS_IStorage {    
    // ======================================================================
    // Enable Ports
    // ----------------------------------------------------------------------
    public iCS_EditorObject CreateEnablePort(int parentId) {
        iCS_EditorObject port= CreatePort(iCS_Strings.EnablePort, parentId, typeof(bool), iCS_ObjectTypeEnum.EnablePort);
        port.IsNameEditable= false;
        port.InitialPortValue= true;
        return port;
    }
    // -------------------------------------------------------------------------
    public bool HasEnablePort(iCS_EditorObject package) {
        return GetEnablePorts(package).Length != 0;
    }
    // -------------------------------------------------------------------------
    public iCS_EditorObject[] GetEnablePorts(iCS_EditorObject package) {
        return BuildFilteredListOfChildren(c=> c.IsEnablePort, package);
    }

    // =========================================================================
    // Out Trigger Port
    // -------------------------------------------------------------------------
    public iCS_EditorObject CreateOutTriggerPort(int parentId) {
        iCS_EditorObject port= CreatePort(iCS_Strings.OutTriggerPort, parentId, typeof(bool), iCS_ObjectTypeEnum.OutTriggerPort);
        port.IsNameEditable= false;
        port.InitialPortValue= true;
        return port;
    }
    // -------------------------------------------------------------------------
    public bool HasOutTriggerPort(iCS_EditorObject package)  {
        return GetOutTriggerPorts(package).Length != 0;
    }
    // -------------------------------------------------------------------------
    public iCS_EditorObject[] GetOutTriggerPorts(iCS_EditorObject package) {
        return BuildFilteredListOfChildren(c=> c.IsOutTriggerPort, package);
    }
}
