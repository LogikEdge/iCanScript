using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using P=Prelude;

public static class iCS_WebUtils {
    // ----------------------------------------------------------------------
    // Performs a Web form request on the given url.
    public static WWW WebRequest(string url, WWWForm form, float waitTime= 500f) {
        return WaitForTransaction(new WWW(url, form), waitTime);
    }
    // ----------------------------------------------------------------------
    // Performs a Web request on the given url.
    public static WWW WebRequest(string url, float waitTime= 500f) {
        return WaitForTransaction(new WWW(url), waitTime);
    }
    // ----------------------------------------------------------------------
	// Wait for Web transaction to complete.
    public static WWW WaitForTransaction(WWW transaction, float waitTime= 500f) {
        var startTime= Time.realtimeSinceStartup;
        while(!transaction.isDone) {
            if((Time.realtimeSinceStartup-startTime) > waitTime) {
                Debug.LogWarning("iCanScript: Timeout waiting for URL: "+transaction.url);
                break;
            }
        }
        return transaction;	
	}
	
    // ----------------------------------------------------------------------
    // Returns the version string of the latest available release.
    public static string GetLatestReleaseId() {
		var url= iCS_WebConfig.WebService_Versions;
        var download = WebRequest(url);
        if(!String.IsNullOrEmpty(download.error)) {
            return null;
        }
        var jVersion= JSON.GetValueFor(download.text, "versions.iCanScript") as JString;
        return jVersion == null ? null : jVersion.value;
    }

    // ----------------------------------------------------------------------
    // Returns true if the current version is the latest version.
    public static Maybe<bool> IsLatestVersion() {
        var latestVersion= GetLatestReleaseId();
        if(String.IsNullOrEmpty(latestVersion)) {
            return new Nothing<bool>();
        }
        var currentVersion= "v"+iCS_EditorConfig.VersionId;
        return new Just<bool>(currentVersion == latestVersion);
    }
}