using UnityEngine;
using System.Collections;

public static class iCS_BuiltinTextures {
    // =================================================================================
    // Properties
    // ---------------------------------------------------------------------------------
    public static Texture2D InPortIcon          { get { return myInDataPortIcon; }}
    public static Texture2D OutPortIcon         { get { return myOutDataPortIcon; }}
    public static Texture2D InTransitionPortIcon    { get { return myInTransitionPortIcon; }}
    public static Texture2D OutTransitionPortIcon   { get { return myOutTransitionPortIcon; }}


    // =================================================================================
    // Constants
    // ---------------------------------------------------------------------------------
    const int   kPortIconWidth  = 16;
    const int   kPortIconHeight = 12;

    // =================================================================================
    // Fields
    // ---------------------------------------------------------------------------------
	static Texture2D    myInDataPortIcon;
	static Texture2D    myOutDataPortIcon;
	static Texture2D    myInTransitionPortIcon;
	static Texture2D    myOutTransitionPortIcon;

    // =================================================================================
    // Constrcutor
    // ---------------------------------------------------------------------------------
    static iCS_BuiltinTextures() {
        BuildPortIcons(Color.green, Color.red);
        BuildTransitionIcons();
    }
    
    // ---------------------------------------------------------------------------------
	static void BuildPortIcons(Color nodeColor, Color typeColor) {
		Texture2D portTemplate= new Texture2D(kPortIconHeight, kPortIconHeight);
		float radius= 0.5f*(kPortIconHeight-3f);
        iCS_PortIcons.BuildDataPortTemplateImp(radius, ref portTemplate);
        Texture2D portIcon= iCS_PortIcons.BuildDataPortIcon(nodeColor, typeColor, portTemplate);

        myInDataPortIcon= new Texture2D(kPortIconWidth, kPortIconHeight);
        myOutDataPortIcon= new Texture2D(kPortIconWidth, kPortIconHeight);
        TextureUtil.Clear(ref myInDataPortIcon);
        TextureUtil.Clear(ref myOutDataPortIcon);
        
        int radiusInt= (int)radius;
        int lineLength= kPortIconWidth-radiusInt;
        int lineHeight= kPortIconHeight/2;
        for(int x= 0; x < lineLength; ++x) {
            myInDataPortIcon.SetPixel(x, lineHeight, typeColor);
            myOutDataPortIcon.SetPixel(radiusInt+x, lineHeight, typeColor);
        }
        int inOffset= kPortIconWidth-kPortIconHeight;
        TextureUtil.AlphaBlend(0, 0, portIcon, inOffset, 0, ref myInDataPortIcon,  portIcon.width, portIcon.height);
        TextureUtil.AlphaBlend(0, 0, portIcon, 0,        0, ref myOutDataPortIcon, portIcon.width, portIcon.height);

        // Finalize icons.
        myInDataPortIcon.Apply();
        myOutDataPortIcon.Apply();
        myInDataPortIcon.hideFlags= HideFlags.DontSave;
        myOutDataPortIcon.hideFlags= HideFlags.DontSave;
	
        Texture2D.DestroyImmediate(portIcon);
        Texture2D.DestroyImmediate(portTemplate);	    
	}

    // ---------------------------------------------------------------------------------
    static void BuildTransitionIcons() {
        // Create textures.
        myInTransitionPortIcon= new Texture2D(kPortIconWidth, kPortIconHeight);
        myOutTransitionPortIcon= new Texture2D(kPortIconWidth, kPortIconHeight);
        TextureUtil.Clear(ref myInTransitionPortIcon);
        TextureUtil.Clear(ref myOutTransitionPortIcon);
        
        // Build out transition port.
        float halfHeight= 0.5f*kPortIconHeight;
        float radius= halfHeight-1.5f;
		TextureUtil.Circle(radius, Color.white, Color.white, ref myOutTransitionPortIcon, new Vector2(halfHeight, halfHeight));

        // Add horizontal line.
		int halfHeightInt= (int)halfHeight;
		for(int x= halfHeightInt; x < kPortIconWidth; ++x) {
		    myOutTransitionPortIcon.SetPixel(x, halfHeightInt, Color.white);
            myInTransitionPortIcon.SetPixel(x-halfHeightInt, halfHeightInt, Color.white);
		}
		
        // Finalize icons.
        myInTransitionPortIcon.Apply();
        myOutTransitionPortIcon.Apply();
        myInTransitionPortIcon.hideFlags= HideFlags.DontSave;
        myOutTransitionPortIcon.hideFlags= HideFlags.DontSave;
    }
}