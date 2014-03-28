﻿using UnityEngine;
using System.Collections;

public partial class iCS_IStorage {
    // ======================================================================
    // ----------------------------------------------------------------------
    public void SaveNavigationState() {
        NavigationHistory.Save(this);
    }
    public void ReloadNavigationFromBackwardHistory() {
        NavigationHistory.ReloadFromBackwardHistory(this);
    }
    public void ReloadNavigationFromForwardHistory() {
        NavigationHistory.ReloadFromForwardHistory(this);
    }
    public bool HasNavigationBackwardHistory {
        get { return NavigationHistory.HasBackwardHistory; }
    }
    public bool HasNavigationForwardHistory {
        get { return NavigationHistory.HasForwardHistory; }
    }
    public void ClearNavigationHistory() {
        NavigationHistory.Clear();
    }
}

