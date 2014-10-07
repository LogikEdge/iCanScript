﻿using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Xml;
using System.Text;
using DisruptiveSoftware;
using System.Text.RegularExpressions;
using System.Collections.Generic;


public class iCS_HelpSearch {

	static private string unityHelpIndex;
	static private Dictionary<string, string> unityHelpSummary= new Dictionary<string, string>();

	/*
	// For testing: Add a menu item named "Do Something" to MyMenu in the menu bar.
	[MenuItem ("MyMenu/Do Something")]
	static void DoSomething () {
		Debug.Log(getUnityHelpSummary ("Debug.Log"));
	}
	*/

	public static void Start() {
		buildUnityHelpIndex();
		buildUnityHelpSummary();
	}
	
	public static void Shutdown() {
	}


	// =================================================================================
	// Convert the JavaScript unity help index file "index.js" to a json string.
	// ---------------------------------------------------------------------------------
	private static void buildUnityHelpIndex() {
		string path = "/Applications/Unity/Unity.app/Contents/Documentation/html/en/ScriptReference/index.js";
		
		var fileStream = new StreamReader (path);
		StringBuilder unityHelpIndexBuilder = new StringBuilder ("");
		string line;
		
		// Read the info file one line at a time (stripping spaces), and convert the java script formating to json where needed.
		using(fileStream) { 
			while( (line= fileStream.ReadLine()) != null ) {
				line= line.Trim();
				// Remove comments which are not valid for JSON.  Warning there are some "//" in the strings, so do not use regex through entire line to find comments.
				if( !(line.StartsWith("//"))) {
					// Change ' var xxx = ' format to ' {"xxx": '
					Regex regex = new Regex("(var )(.*?)(=)");
					Match match = regex.Match(line);
					if (match.Success) {
						string name= match.Groups[2].ToString().Trim();	
						line= Regex.Replace(line, "var .*?=", "{\"" + name + "\":");
					}
					
					// Change "};" to "}", and "];" to ]}". 
					line= Regex.Replace(line, "};", "}");
					line= Regex.Replace(line, "];", "]}");
					unityHelpIndexBuilder.Append(line + "\n");
				}
			}
			fileStream.Close ();
		}
		unityHelpIndex= unityHelpIndexBuilder.ToString();
	}

	private static string formatFromHTML(string stringToFormat)
	{
		// TODO: can support rich text, but at present rich text does not seem to work in 
		// Just strip out HTML tags for now
		return Regex.Replace(stringToFormat, "<.*?>", string.Empty);
	}	
		

	// =================================================================================
	// Get the summary descriptin for a unity API by providing class.function
	// ---------------------------------------------------------------------------------
	private static void buildUnityHelpSummary()
	{
		JObject rootObject= JSON.GetRootObject(unityHelpIndex);	
		JArray  arrayOfEntries= rootObject.GetValueFor("info") as JArray;
			
		foreach (JObject jObject in arrayOfEntries.value) {
			// Get the title, summary as per the index.js file.
//			JString jTitle= jObject.GetValueFor("title") as JString;
			JString jUrl= jObject.GetValueFor("url") as JString;
			JString jSummary= jObject.GetValueFor("summary") as JString;
	
			// replace HTML formating in summary
			string summary= formatFromHTML(jSummary.value);
			
			// Transfer information to dictionary
			// Warning: "title" and "url" are not all unique in current index.js
			try {
				unityHelpSummary.Add(jUrl.value, summary);
			}
			catch {
//				Debug.Log("Duplicate: "+ jUrl.value + "\nsummary: " + summary);
			}
		}
	}

	// =================================================================================
	// Get the summary descriptin for a unity API by providing class.function
	// ---------------------------------------------------------------------------------
	public static string getHelpSummary(iCS_MemberInfo memberInfo )
	{	
		if (memberInfo.Company == "Unity") {
			
			string className="";
			string demarcator="";
			string methodName="";
			
			if (memberInfo.IsTypeInfo) {
				// First level libray entries (classes and packages), just return className
				className = memberInfo.ToTypeInfo.ClassName;
			}
			else if (memberInfo.IsMethod) {
				className= memberInfo.ParentTypeInfo.ClassName;
				methodName= memberInfo.ToMethodInfo.MethodName;
				if (memberInfo.IsProperty) {
					// Property Nodes
					demarcator="-";
					methodName= Regex.Replace(methodName, "get_", string.Empty);
					methodName= Regex.Replace(methodName, "set_", string.Empty);
				}
				else if(memberInfo.IsConstructor) {
					// Builders
					demarcator="-ctor";
					methodName= "";
				}
				else {
					// Functions, etc.
					demarcator= ".";
					// Remap arithmetic operator names
					if (methodName.Contains("op_")) {
						demarcator="-";
						methodName= Regex.Replace(methodName, "op_Addition", "operator_add");
						methodName= Regex.Replace(methodName, "op_Division", "operator_divide");
						methodName= Regex.Replace(methodName, "op_Equality", "operator_eq");
						methodName= Regex.Replace(methodName, "op_Inequality", "operator_ne");
						methodName= Regex.Replace(methodName, "op_Multiply", "operator_multiply");
						methodName= Regex.Replace(methodName, "op_Subtraction", "operator_subtract");
						//methodName= Regex.Replace(methodName, "op_UnaryNegation", ???);
					}
				}
			}
			else if (memberInfo.IsField) {
				// Field Nodes
				className= memberInfo.ParentTypeInfo.ClassName;
				methodName= memberInfo.ToFieldInfo.MethodName;	
				if(memberInfo.ToFieldInfo.IsClassMember)
					demarcator= ".";
				else if(memberInfo.ToFieldInfo.IsInstanceMember) 
					demarcator= "-";
				else 
					demarcator= ".";
			}		
	
			string summary;
			string search= className + demarcator + methodName;
			unityHelpSummary.TryGetValue(search, out summary);
			
			return summary;
		}
		return null;
		
	}


	// =================================================================================
	// DEPRICATED
	// ---------------------------------------------------------------------------------
	string getHelpFromHTMLFiles(string apiToSearch, string sectionToGet) {

		// Create Filename that will contain help in Unity folder.
		string path = "/Applications/Unity/Unity.app/Contents/Documentation/html/en/ScriptReference";
		string fileName = string.Concat (apiToSearch, ".html");

		// Find file if it exists
		string[] helpFiles = Directory.GetFiles (path, fileName);

		// Variables use "-" instead of "." in filename, try replacing "." with "-" if we did not find it the first time.
		if (helpFiles.Length == 0) {
			fileName = string.Concat ( apiToSearch.Replace('.', '-'), ".html");
			helpFiles = Directory.GetFiles (path, fileName);
		}

		if (helpFiles.Length != 0) {
				string helpFile = helpFiles [0];

				// Create an XML document from the file (even though it is HTML)
				XmlDocument doc = new XmlDocument ();
				doc.Load (helpFile);
				XmlNodeReader reader = new XmlNodeReader (doc);
		
				string currentText;
				string helpText="";
				
				// Parse through the "XML"
				while (reader.Read()) {
					currentText= reader.ReadString();

					// Look for "Section"
					if(currentText == sectionToGet ) {
						while(reader.Read ()) {
							// Section will be broken up in formating, keep concatinating the bits and pieces.
							currentText = reader.ReadString();

							if(reader.Name == "div")
								helpText= string.Concat (helpText, "\n");

							if(reader.Name == "a")
								helpText= string.Concat (helpText, " ");

							if(reader.Name == "td")
								helpText= string.Concat (helpText, "\n"); 

							currentText=currentText.Replace('\n', ' ');
							helpText= string.Concat (helpText, currentText);

							Debug.Log (string.Concat ("---->", reader.Name));
						    Debug.Log (currentText);

							// If there is another header, or a format change, section is probably over.
							if(reader.Name=="h2" || reader.Name=="pre")
								return helpText;
						}
					}

				}
		}
		return "";
					
	} 
}