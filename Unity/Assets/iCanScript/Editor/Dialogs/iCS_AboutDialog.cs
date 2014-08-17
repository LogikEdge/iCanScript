﻿using UnityEngine;
using UnityEditor;
using System.Collections;

public class iCS_AboutDialog : EditorWindow {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
	const float kSpacer= 5f;
	GUIStyle    h1     = null;
	
    // ======================================================================
    // Initialization/Teardown
    // ----------------------------------------------------------------------
    public void OnEnable() {
        title= "About iCanScript";
    }
    public void OnDisable() {
    }

    // ======================================================================
    // GUI Update
    // ----------------------------------------------------------------------
    public void OnGUI() {
		// Init.
		if(h1 == null) {
			BuildStyles();
		}
		
        // Show product icon
        var logoWidth= 64f;
        var logoHeight= 64f;
        Texture2D iCanScriptLogo= null;
        if(iCS_TextureCache.GetTexture(iCS_EditorStrings.LogoIcon, out iCanScriptLogo)) {
            Rect r= new Rect(kSpacer, kSpacer, logoWidth, logoHeight);
            GUI.DrawTexture(r, iCanScriptLogo);
        }

		// Show product name
		GUIContent title= new GUIContent("iCanScript");
		var titleSize= h1.CalcSize(title);
		Rect rTitle= new Rect(2f*kSpacer+logoWidth, kSpacer, titleSize.x, titleSize.y);
		GUI.Label(rTitle, title, h1);
		
		// Show version
        GUIContent versionContent= new GUIContent(iCS_EditorConfig.VersionStr);
        Vector2 versionSize= GUI.skin.label.CalcSize(versionContent);
        Rect rVersion= new Rect(2f*kSpacer+logoWidth, rTitle.yMax, versionSize.x, versionSize.y);
        GUI.Label(rVersion, versionContent);


		// Column info
		float column1X= kSpacer;
		var editionTitle= new GUIContent("Edition: ");
		var operatingModeTitle= new GUIContent("Operating Mode: ");
		var buildDateTitle= new GUIContent("Build Date: ");
		var userLicenseTitle= new GUIContent("User License: ");
		var operatingModeTitleSize= GUI.skin.label.CalcSize(operatingModeTitle);
		var buildDateTitleSize= GUI.skin.label.CalcSize(buildDateTitle);
		var userLicenseTitleSize= GUI.skin.label.CalcSize(userLicenseTitle);
		float column1Width= Math3D.Max(operatingModeTitleSize.x,
									   buildDateTitleSize.x,
									   userLicenseTitleSize.x);
		var labelHeight= operatingModeTitleSize.y;
		
		float column2X= column1X+column1Width+kSpacer;
		float column2Width= position.width-column2X-kSpacer;
		
//		// License Type
//		var rLicenseType= new Rect(column1X, kSpacer+logoHeight+labelHeight, column2Width, labelHeight);
//		GUI.Label(rLicenseType, licenseTypeTitle);
//		GUIContent licenseType= new GUIContent(iCS_LicenseController.LicenseTypeAsString());
//		rLicenseType.x= column2X; rLicenseType.width= column2Width;
//		GUI.Label(rLicenseType, licenseType);
		
		// Operating Mode
		var rOperatingMode= new Rect(column1X, kSpacer+logoHeight+labelHeight, column2Width, labelHeight);
		GUI.Label(rOperatingMode, operatingModeTitle);
		GUIContent operatingMode= new GUIContent(iCS_LicenseController.OperatingModeAsString());
		rOperatingMode.x= column2X; rOperatingMode.width= column2Width;
		GUI.Label(rOperatingMode, operatingMode);
		
		// Build date
		var rBuildDate= new Rect(column1X, rOperatingMode.yMax, column2Width, labelHeight);
		GUI.Label(rBuildDate, buildDateTitle);
		GUIContent buildDate= new GUIContent(iCS_BuildInfo.kBuildDateStr);
		rBuildDate.x= column2X; rBuildDate.width= column2Width;
		GUI.Label(rBuildDate, buildDate);
		
		// Edition
		var rEdition= new Rect(column1X, rBuildDate.yMax, column2Width, labelHeight);
		GUI.Label(rEdition, editionTitle);
		rEdition.x= column2X; rEdition.width= column2Width;
		GUI.Label(rEdition, iCS_EditionController.ToString());

		// User License
		var rUserLicense= new Rect(column1X, rEdition.yMax, column1Width, labelHeight);
		GUI.Label(rUserLicense, userLicenseTitle);
		var userLicense= iCS_LicenseController.LicenseAsString();
		if(string.IsNullOrEmpty(userLicense)) {
			userLicense= "None";
		}
		var userLicenseSize= GUI.skin.label.CalcSize(new GUIContent(userLicense));
		rUserLicense.x= column2X; rUserLicense.width= column2Width;
		GUI.Label(rUserLicense, userLicense);

		// Disclamer
		GUIContent copyright= new GUIContent("(c) copyright Disruptive Software 2014.  All rights reserved.");
		var copyrightSize= GUI.skin.label.CalcSize(copyright);
		var rCopyright= new Rect(column1X, rUserLicense.yMax+copyrightSize.y, copyrightSize.x, copyrightSize.y);
		GUI.Label(rCopyright, copyright);
	
		var p= position;
		p.width= Math3D.Max(copyrightSize.x+2f*kSpacer, column2X+userLicenseSize.x+kSpacer);
		p.height= rCopyright.yMax+kSpacer;
		position= p;
    }

    // ======================================================================
    // Helpers
    // ----------------------------------------------------------------------
	void BuildStyles() {
		// Build H1 style
		h1= new GUIStyle(EditorStyles.boldLabel);
		h1.fontSize= 24;		
	}
}
