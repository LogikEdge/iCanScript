﻿using System.Collections.Generic;
using System;
using UnityEditor;

public static class iCS_HelpDictionary {
	
	public static Dictionary<string, string> typeHelp = new Dictionary<string, string>()
	{
	    { "Constructor", 
			"A Builder node is used to construct and initialise a class or variable."
		},
	    { "Instance", 
			"Class nodes have an instance port, you can use a builder node to create an instance."
		},
	    { "Function", 
			"Function nodes are used to execute behaviours."
		},
	    { "VariableReference", 
			"Variable References are used to accesses public variables."
		},
	    { "FunctionCall", 
			"Function Calls invokes function."
		},		
	    { "InstancePort", 
			"Instance ports are used to connect instances to functions, classes, and variables."
		},		
	    { "TriggerPort", 
			"Trigger Ports are used for flow control.  Will be set to true when execution of the attached node is complete."
		},		
	    { "EnablePort", 
			"Enable ports are used for flow control.  When set to true, attached node will execute"
		},		
	    { "UnityEditor.SceneHierarchyWindow", 
			"<tcolor><b>Scene Hierarchy tips:</b></color>\n" +
			"You can drag iCanScript Objects from the Hierarchy into a message handler to add Variables and User Functions."
		},
		{ "UnityEditor.ProjectBrowser", 
			"<tcolor><b>Project Browser tips:</b></color>\n" +
			"Find prefabs in the Project Browser."
		},
		{ "UnityEditor.ConsoleWindow", 
			"<tcolor><b>ConsoleWindow tips:</b></color>\n" +
			"The Console Window is where you can find details on your iCanScript errors."
		},
		{ "UnityEditor.InspectorWindow", 
			"<tcolor><b>Inspector Window:</b></color>\n" +
			"You can edit parameters of your iCanScript Object in the inspector window under 'iCS_VisualScript (Script)'."
		},
		{ "iCS_InstanceEditorWindow",
			"<tcolor><b>Instance Wizard tips:</b></color>\n" +
			"The Instance Wizard is used to add and remove ports in a class node.  Select a class node in the visual editor to use.\n"	
		},		
		{ "iCS_LibraryEditorWindow",
			"<tcolor><b>Library tips:</b></color>\n" +
			"From the library window, you can drag objects into a message handler in the Visual Editor window to create nodes.\n" +
			"\n" +
			"<tcolor><b>Keyboard shortcuts:</b></color>\n" +
			"<tcolor><b>H:</b></color> additional help for selected\t <tcolor><b>Enter:</b></color> fold/unfold"
		},
		{ "iCS_VisualEditorWindow",
			"<tcolor><b>Keyboard shortcuts (for selected):</b></color>\n" +
			"<tcolor><b>H:</b></color> more Help\t\t<tcolor><b>drag:</b></color> moves node\n" +
			"<tcolor><b>F:</b></color> Focus in view\t\t<tcolor><b>ctrl-drag:</b></color> moves node outside\n" +
			"<tcolor><b>L:</b></color> auto Layout\t\t<tcolor><b>shift-drag:</b></color> move copies node\n" +
			"<tcolor><b>B:</b></color> Bookmark\t\t<tcolor><b>del:</b></color> deletes object\n" +
			"<tcolor><b>G:</b></color> move to bookmark\t<tcolor><b>C:</b></color> Connect to bookmark\n"
		},
		// Duplicate of iCS_VisualEditorWindow, 
		{ "iCS_VisualScriptImp",
			"<tcolor><b>Keyboard shortcuts (for selected):</b></color>\n" +
			"<tcolor><b>H:</b></color> more Help\t\t<tcolor><b>drag:</b></color> moves node\n" +
			"<tcolor><b>F:</b></color> Focus in view\t\t<tcolor><b>ctrl-drag:</b></color> moves node outside\n" +
			"<tcolor><b>L:</b></color> auto Layout\t\t<tcolor><b>shift-drag:</b></color> move copies node\n" +
			"<tcolor><b>B:</b></color> Bookmark\t\t<tcolor><b>del:</b></color> deletes object\n" +
			"<tcolor><b>G:</b></color> move to bookmark\t<tcolor><b>C:</b></color> Connect to bookmark\n"
		},
		{ "iCS_Package", 
			"The Package is iCanScript most flexible node. It can contain complex graphs and expose only those ports that are made public by the visual script designer."
		}
	};

}