using UnityEngine;
using System.Collections;

public class iCS_EngineStrings {
    // Special node names.
    public const string UpdateAction         = "Update";
    public const string LateUpdateAction     = "LateUpdate";
    public const string FixedUpdateAction    = "FixedUpdate";
    public const string OnEntryModule        = "OnEntry";
    public const string OnUpdateModule       = "OnUpdate";
    public const string OnExitModule         = "OnExit";
    public const string TransitionEntryModule= "TransitionEntry";
    public const string TransitionExitModule = "TransitionExit";

    // Special port names.
    public const string EnablePort= "enable";
    
    // Reflection methods
    public const string AddChildMethod   = "AddChild";
    public const string RemoveChildMethod= "RemoveChild";

}
