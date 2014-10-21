using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;

public partial class iCS_VisualEditor : iCS_EditorBase {
	
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
	private string myHelpText  = null;
	string nameColour = EditorGUIUtility.isProSkin ? "<color=cyan>" : "<color=blue>";
	string noHelp= "no tip available";
	string defaultHelp= 
		"<b>Press on selected:</b>\n" +
		" <b>H-</b> more Help\t\t<b>drag-</b> moves node\n" +
		" <b>F-</b> Focus in view\t\t<b>ctrl drag-</b> moves node outside\n" +
		" <b>L-</b> Auto Layout\t\t<b>shift drag-</b> move copies node\n" +
		" <b>B-</b> Bookmark\t\t<b>del-</b> deletes object\n" +
		" <b>G-</b> Move to Bookmark\t<b>C-</b> Connect to Bookmark\n";
	
    // ======================================================================
    // Poplulates the help string which will be displayed on on GUI when 
	// a node/port is floated over.
	// ----------------------------------------------------------------------
	void PopulateHelp(iCS_EditorObject edObj) {
		if(edObj==null)
			return;
		
		Rect position= edObj.AnimatedRect;
		bool isMouseOver= position.Contains(GraphMousePosition);
		if(isMouseOver) {
			myHelpText="";
			// Polpulate help if pointed object is a port
			if(edObj.IsPort) {
				iCS_EditorObject   firstProducerPort= edObj.FirstProducerPort;
				iCS_EditorObject[] endConsumerPortArray= edObj.EndConsumerPorts;
							
			    if (edObj.IsInputPort) {
					// there should only be one end consumer port for an input port.
					
					if(endConsumerPortArray[0] != null){
						myHelpText= myHelpText + GetPortHelpString("", endConsumerPortArray[0]);
					}	
			   		if(firstProducerPort != null && firstProducerPort != endConsumerPortArray[0]) {
						myHelpText= myHelpText + "\n\n" + GetPortHelpString("connected-> ", firstProducerPort);
			   		}	
			   	}
				else if(edObj.IsOutputPort) {
			   		if(firstProducerPort != null) {
			   			myHelpText= myHelpText + GetPortHelpString("", firstProducerPort);
			   		}
					myHelpText= myHelpText + "\n";
					
					//For real estate reasons, show only first connection.
					iCS_EditorObject endConsumerPort= endConsumerPortArray[0];
					if(endConsumerPort != null && firstProducerPort != endConsumerPort) {
						myHelpText= myHelpText + "\n" + GetPortHelpString("connected-> ", endConsumerPort);
					}
				}
			}
			// Polpulate help if pointed object is a node
			else {
				iCS_MemberInfo memberInfo= iCS_LibraryDatabase.GetAssociatedDescriptor(edObj);
				myHelpText= memberInfo != null ? memberInfo.Summary : null;
				if(String.IsNullOrEmpty(myHelpText)) {
					myHelpText= noHelp;
				}
				else {
					myHelpText= "<b>" + nameColour + edObj.DisplayName + "</color></b>" + "\n" + myHelpText;
				}
			}
		}
	}

    // ======================================================================
    // Used by populate help to build help string
	string GetPortHelpString(string prefix, iCS_EditorObject edObj) {
		if(edObj.ParentNode != null) {
			if(edObj.ParentNode.IsKindOfFunction) {

				iCS_EditorObject portNameEdObj= edObj;
				if(edObj.ParentNode.ParentNode.IsInstanceNode) {
					portNameEdObj= edObj.IsInputPort ? edObj.ProducerPort : edObj.ConsumerPorts[0];
				}
				
				string summary= null;
	    		iCS_MemberInfo memberInfo= iCS_LibraryDatabase.GetAssociatedDescriptor(edObj.ParentNode);
				
				if(memberInfo != null) {
					// Handle special types of ports.
					if (portNameEdObj.PortIndex == 400 && edObj.ParentNode.EngineObject.IsClassField)
						// return port will be same as parent node description.
						summary= memberInfo.Summary;		
					else if( portNameEdObj.PortIndex >= 400 )
						summary= null;
					else
						summary= memberInfo.Summary;	
				}
				if(String.IsNullOrEmpty(summary)) {
					summary= noHelp;
				}
				
				string displayName= portNameEdObj.DisplayName;
				displayName= Regex.Replace(displayName, "<Color>", "< Color >");	
				
				string typeName= iCS_Types.TypeName(portNameEdObj.RuntimeType);
							
				return "<b>" + prefix + typeName + " " + nameColour + displayName + "</color></b>" + "\n" + summary;
			}
		}
		return null;
	}
	
    // ======================================================================
    // Display the help already populated in myHelpText
	void DisplayHelp() {
		GUIStyle style =  EditorStyles.textArea;
		style.richText = true;
		style.alignment = TextAnchor.UpperLeft;
		if (myHelpText==noHelp)
			myHelpText= defaultHelp;
		GUI.Box(new Rect(Screen.width-400, Screen.height-100, 400, 100), myHelpText, style);
	}
}
