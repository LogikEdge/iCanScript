using UnityEngine;
using System.Collections;

public partial class iCS_Graphics {
    // ======================================================================
    // Constants
    // ----------------------------------------------------------------------
	const  float kTileRatio= 1f/3f;
	const  float kTilePos0 = 0f;
	const  float kTilePos1 = 1f/3f;
	const  float kTilePos2 = 2f/3f;
	static Rect  kTopLeftTileCoord    = new Rect(kTilePos0, kTilePos2, kTileRatio, kTileRatio);
	static Rect  kTopMidTileCoord     = new Rect(kTilePos1, kTilePos2, kTileRatio, kTileRatio);
	static Rect  kTopRightTileCoord   = new Rect(kTilePos2, kTilePos2, kTileRatio, kTileRatio);
	static Rect  kMidLeftTileCoord    = new Rect(kTilePos0, kTilePos1, kTileRatio, kTileRatio-0.01f);
	static Rect  kMidMidTileCoord     = new Rect(kTilePos1, kTilePos1, kTileRatio, kTileRatio-0.01f);
	static Rect  kMidRightTileCoord   = new Rect(kTilePos2, kTilePos1, kTileRatio, kTileRatio-0.01f);
	static Rect  kBottomLeftTileCoord = new Rect(kTilePos0, kTilePos0, kTileRatio, kTileRatio);
	static Rect  kBottomMidTileCoord  = new Rect(kTilePos1, kTilePos0, kTileRatio, kTileRatio);
	static Rect  kBottomRightTileCoord= new Rect(kTilePos2, kTilePos0, kTileRatio, kTileRatio);

	// ----------------------------------------------------------------------
	void DrawNode(Rect r, Color nodeColor, Color backgroundColor, Color shadowColor, GUIContent title, GUIStyle titleStyle) {
		// Reajust screen position for fix size shadow.
		float shadowSize= iCS_EditorConfig.NodeShadowSize;
		float shadowSize2= 2f*shadowSize;
		Rect screenPos= new Rect(r.x-shadowSize, r.y-shadowSize, r.width+shadowSize2, r.height+shadowSize2);

		// Get texture.
		Texture2D nodeTexture= iCS_NodeTextures.GetNodeTexture(nodeColor, backgroundColor, shadowColor);
		int tileSize= (int)(nodeTexture.width*kTileRatio+0.1f);
		int tileSize2= 2*tileSize;
		
		float middleWidth = screenPos.width -tileSize2;
		float middleHeight= screenPos.height-tileSize2;

		Rect pos= new Rect(screenPos.x, screenPos.y, tileSize, tileSize);
		GUI.DrawTextureWithTexCoords(pos, nodeTexture, kTopLeftTileCoord);
		pos.x= pos.xMax;
		pos.width= middleWidth;
		GUI.DrawTextureWithTexCoords(pos, nodeTexture, kTopMidTileCoord);
		pos.x= pos.xMax;
		pos.width= tileSize;
		GUI.DrawTextureWithTexCoords(pos, nodeTexture, kTopRightTileCoord);
		if(middleHeight > 0f) {
            float heightRatio= middleHeight >= tileSize ? kTileRatio-0.01f : middleHeight/(3f*tileSize);
    		pos= new Rect(screenPos.x, pos.yMax,tileSize,middleHeight);
            Rect coord= kMidLeftTileCoord;
            coord.height= heightRatio;
    		GUI.DrawTextureWithTexCoords(pos, nodeTexture, coord);                
			pos.x= pos.xMax;
			pos.width= middleWidth;
            coord= kMidMidTileCoord;
            coord.height= heightRatio;
			GUI.DrawTextureWithTexCoords(pos, nodeTexture, coord);
			pos.x= pos.xMax;
			pos.width= tileSize;
            coord= kMidRightTileCoord;
            coord.height= heightRatio;
			GUI.DrawTextureWithTexCoords(pos, nodeTexture, coord);			
		}

		pos= new Rect(screenPos.x, pos.yMax, tileSize, tileSize);
		GUI.DrawTextureWithTexCoords(pos, nodeTexture, kBottomLeftTileCoord);
		pos.x= pos.xMax;
		pos.width= middleWidth;
		GUI.DrawTextureWithTexCoords(pos, nodeTexture, kBottomMidTileCoord);
		pos.x= pos.xMax;
		pos.width= tileSize;
		GUI.DrawTextureWithTexCoords(pos, nodeTexture, kBottomRightTileCoord);

        // Show title.
		GUI.color= Color.white;
        if(!ShouldShowTitle()) return;
        Vector2 titleCenter= new Vector2(0.5f*(r.x+r.xMax), r.y+0.5f*(tileSize-shadowSize));
        Vector2 titleSize= titleStyle.CalcSize(title);
        GUI.Label(new Rect(titleCenter.x-0.5f*titleSize.x, titleCenter.y-0.5f*titleSize.y, titleSize.x, titleSize.y), title, titleStyle);
	}
}
