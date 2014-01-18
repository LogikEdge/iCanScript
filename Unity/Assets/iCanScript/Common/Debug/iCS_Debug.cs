﻿using UnityEngine;
using System.Collections;

public static class iCS_Debug {
	public static string Message(string message) {
		return "iCanScript: "+message;
	}
	public static string NullMessage(string message) {
		return Message(message)+" is Null";
	}

	public static void Log(string message) {
		Debug.Log(Message(message));
	}
	public static void LogWarning(string message) {
		Debug.LogWarning(Message(message));
	}
	public static void LogError(string message) {
		Debug.LogError(Message(message));
	}
}
