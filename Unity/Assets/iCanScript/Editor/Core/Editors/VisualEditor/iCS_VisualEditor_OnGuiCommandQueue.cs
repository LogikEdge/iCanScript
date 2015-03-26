﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using iCanScript.Utilities;

namespace iCanScript.Editor {
public partial class iCS_VisualEditor : iCS_EditorBase {
    ActionQueue  OnGUICommandQueue= new ActionQueue();

    // ======================================================================
    // ----------------------------------------------------------------------
    void RunOnGUICommands() {
		OnGUICommandQueue.RunQueuedActions();
    }
    public void QueueOnGUICommand(Action fnc) {
        OnGUICommandQueue.QueueAction(fnc);
    }
}
}