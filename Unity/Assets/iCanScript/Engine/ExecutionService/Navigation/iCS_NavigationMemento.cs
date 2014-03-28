﻿using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class iCS_NavigationMemento {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    bool                    ShowDisplayRootNode;
    Vector2                 ScrollPosition;
    float                   GuiScale;
    int                     DisplayRoot;
    int                     SelectedObject;

    // ----------------------------------------------------------------------
    // Creates a new memento and saves the navigation state of the given
    // IStorage.
    public iCS_NavigationMemento(iCS_Storage storage) {
        SaveState(storage);
    }
    // ----------------------------------------------------------------------
    // Save the navigation state of the given IStorage.
    public void SaveState(iCS_Storage storage) {
        ShowDisplayRootNode    = storage.ShowDisplayRootNode;
        ScrollPosition         = storage.ScrollPosition;
        GuiScale               = storage.GuiScale;
        DisplayRoot            = storage.DisplayRoot;
        SelectedObject         = storage.SelectedObject;
    }
    // ----------------------------------------------------------------------
    // Restores the navigation state into the given IStorage.
    public void RestoreState(iCS_Storage storage) {
        if(DisplayRoot < 0 || DisplayRoot >= storage.EngineObjects.Count) return;
        if(storage.EngineObjects[DisplayRoot].InstanceId == -1) return;
        storage.ShowDisplayRootNode= ShowDisplayRootNode;
        storage.ScrollPosition     = ScrollPosition;
        storage.GuiScale           = GuiScale;
        storage.DisplayRoot        = DisplayRoot;
        storage.SelectedObject     = SelectedObject; 
    }

    // ----------------------------------------------------------------------
    // Duplication functionality
    public void CopyFrom(iCS_NavigationMemento from) {
        ShowDisplayRootNode= from.ShowDisplayRootNode;
        ScrollPosition     = from.ScrollPosition;
        GuiScale           = from.GuiScale;
        DisplayRoot        = from.DisplayRoot;
        SelectedObject     = from.SelectedObject;
    }
    // ----------------------------------------------------------------------
    public iCS_NavigationMemento Clone() {
        var newMemento= new iCS_NavigationMemento();
        newMemento.CopyFrom(this);
        return newMemento;
    }
    private iCS_NavigationMemento() {}
}
