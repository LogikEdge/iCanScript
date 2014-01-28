using UnityEngine;
using System;
using System.Collections;

public class iCS_Version {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
	public uint MajorVersion;
	public uint MinorVersion;
	public uint BugFixVersion;
	
    // ======================================================================
    // Initialization
    // ----------------------------------------------------------------------
	public iCS_Version(uint major, uint minor, uint bugFix) {
		MajorVersion = major;
		MinorVersion = minor;
		BugFixVersion= bugFix;
	}

    // ======================================================================
    // Comparaisons
    // ----------------------------------------------------------------------
	public bool IsEqual(iCS_Version other) {
		return IsEqual(other.MajorVersion, other.MinorVersion, other.BugFixVersion);
	}
	public bool IsEqual(uint major, uint minor, uint bugFix) {
		return MajorVersion  == major &&
		       MinorVersion  == minor &&
			   BugFixVersion == bugFix;
	}
							
    // ----------------------------------------------------------------------
	public bool IsNewerThen(iCS_Version other) {
		return IsNewerThen(other.MajorVersion, other.MinorVersion, other.BugFixVersion);		
	}
	public bool IsNewerThen(uint major, uint minor, uint bugFix) {
		if(MajorVersion > major) return true;
		if(MajorVersion < major) return false;
		if(MinorVersion > minor) return true;
		if(MinorVersion < minor) return false;
		return BugFixVersion > bugFix;
	}

    // ----------------------------------------------------------------------
	public bool IsNewerOrSameAs(iCS_Version other) {
		return IsNewerOrSameAs(other.MajorVersion, other.MinorVersion, other.BugFixVersion);
	}
	public bool IsNewerOrSameAs(uint major, uint minor, uint bugFix) {
		if(MajorVersion > major) return true;
		if(MajorVersion < major) return false;
		if(MinorVersion > minor) return true;
		if(MinorVersion < minor) return false;
		return BugFixVersion >= bugFix;
	}
	
    // ----------------------------------------------------------------------
	public bool IsOlderThen(iCS_Version other) {
		return IsOlderThen(other.MajorVersion, other.MinorVersion, other.BugFixVersion);
	}
	public bool IsOlderThen(uint major, uint minor, uint bugFix) {
		if(MajorVersion < major) return true;
		if(MajorVersion > major) return false;
		if(MinorVersion < minor) return true;
		if(MinorVersion > minor) return false;
		return BugFixVersion < bugFix;
	}

    // ======================================================================
    // Common
    // ----------------------------------------------------------------------
	// Serializes a version string.
	public override string ToString() {
		return MajorVersion.ToString()+"."+MinorVersion.ToString()+"."+BugFixVersion.ToString();
	}
    // ----------------------------------------------------------------------
	// Deserializes a version string.
	public static iCS_Version FromString(string versionStr) {
		uint major = 0;
		uint minor = 0;
		uint bugFix= 0;
		int idx= versionStr.IndexOf('.');
		if(idx >= 1) {
			string majorStr= versionStr.Substring(0, idx);
			versionStr= versionStr.Substring(idx+1);		
			major = (uint)Convert.ChangeType(majorStr, typeof(uint));
			idx= versionStr.IndexOf('.');
			if(idx >= 1) {
				string minorStr= versionStr.Substring(0, idx);
				minor = (uint)Convert.ChangeType(minorStr, typeof(uint));
				versionStr= versionStr.Substring(idx+1);		
				if(versionStr.Length > 0) {
					bugFix= (uint)Convert.ChangeType(versionStr, typeof(uint));												
				}
			}
		}
		return new iCS_Version(major, minor, bugFix);
	}
}
