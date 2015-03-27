﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Subspace;
using iCanScript.Editor;

public partial class iCS_EditorObject {
    // ----------------------------------------------------------------------
	public SSObject GetRuntimeObject {
		get {
	        iCS_VisualScriptImp bh= IStorage.iCSMonoBehaviour as iCS_VisualScriptImp;
	        return bh == null ? null : (bh.GetRuntimeObject(InstanceId) as SSObject);
		}
	}
    // ----------------------------------------------------------------------
	public int GetExecutionFrameId {
		get {
			var action= GetRuntimeObject as SSAction;
			return action != null ? action.ExecutedRunId : 0;
		}
	}
}
