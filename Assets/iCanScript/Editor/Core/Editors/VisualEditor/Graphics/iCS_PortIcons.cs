using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public static class iCS_PortIcons {
    // ======================================================================
    // PROPERTIES
    // ----------------------------------------------------------------------
	static float		myScale						  = 0f;
	static Texture2D	myCircularPortTemplate        = null;
	static Texture2D	mySquarePortTemplate  		  = null;
	static Texture2D	mySelectedCircularPortTemplate= null;
	static Texture2D	mySelectedSquarePortTemplate  = null;
	
    // ----------------------------------------------------------------------
	static Dictionary<Color,Dictionary<Color,Texture2D>>	myCircularPortIcons        = null;
	static Dictionary<Color,Dictionary<Color,Texture2D>>	mySquarePortIcons          = null;
	static Dictionary<Color,Dictionary<Color,Texture2D>>	mySelectedCircularPortIcons= null;
	static Dictionary<Color,Dictionary<Color,Texture2D>>	mySelectedSquarePortIcons  = null;

	// ----------------------------------------------------------------------
    //  Build template for all port icons
	public static void BuildPortIconTemplates(float scale) {
		if(Math3D.IsEqual(scale, myScale)) return;
		BuildCircularPortTemplates(scale);
		BuildSquarePortTemplates(scale);
		myScale= scale;
		FlushCachedIcons();
	}
	// ----------------------------------------------------------------------
	static void BuildSquarePortTemplates(float scale) {
        float len= scale*iCS_Config.PortRadius*3.7f;
		BuildSquarePortTemplate(len, ref mySquarePortTemplate);
		BuildSquarePortTemplate(1.67f*len, ref mySelectedSquarePortTemplate);
	}
	// ----------------------------------------------------------------------
	static void BuildSquarePortTemplate(float len, ref Texture2D template) {
        // Build new template.
		float margin= len*0.3f;		
		// Create texture.
		int lenInt= (int)(len+1f);
		int marginInt= (int)(margin);
		int topMarginInt= (int)(len-margin);
        // Remove previous template.
        if(template != null) Texture2D.DestroyImmediate(template);
		template= new Texture2D(lenInt, lenInt);
		for(int x= 0; x < lenInt; ++x) {
			for(int y= 0; y < lenInt; ++y) {
				if(x == 0 || y == 0 || x == lenInt-1 || y == lenInt-1) {
					template.SetPixel(x,y,Color.red);
				} else  if(x < marginInt || x > topMarginInt || y < marginInt || y > topMarginInt) {
					template.SetPixel(x,y,Color.black);
				} else {
					template.SetPixel(x,y,Color.blue);					
				}
			}
		}
		template.hideFlags= HideFlags.DontSave;
		template.Apply();
	}
	// ----------------------------------------------------------------------
	static void BuildCircularPortTemplates(float scale) {
        float radius= scale*iCS_Config.PortRadius*1.85f;
		BuildCircularPortTemplate(radius, ref myCircularPortTemplate);
		BuildCircularPortTemplate(1.67f*radius, ref mySelectedCircularPortTemplate);
	}
	// ----------------------------------------------------------------------
	static void BuildCircularPortTemplate(float radius, ref Texture2D template) {
        // Remove previous template.
        if(template != null) Texture2D.DestroyImmediate(template);
		// Create texture.
		int widthInt= (int)(2f*radius+3f);
		int heightInt= (int)(2f*radius+3f);
		template= new Texture2D(widthInt, heightInt, TextureFormat.ARGB32, false);
		BuildCircularPortTemplateImp(radius, ref template);
		// Finalize texture.
		template.hideFlags= HideFlags.DontSave;
		template.Apply();
	}
	// ----------------------------------------------------------------------
	static void BuildCircularPortTemplateImp(float radius, ref Texture2D texture) {
		float ringWidth= 2f;
		float fillRatio= 0.5f;
		float outterRingRadius= radius+0.5f*ringWidth;
		float innerRingRadius= radius-0.5f*ringWidth;
		float fillRadius= fillRatio*radius;
		float outterFillRadius= fillRadius+0.5f*ringWidth;
		float innerFillRadius= fillRadius-0.5f*ringWidth;
		float cx= 0.5f*texture.width;
		float cy= 0.5f*texture.height;
		
		float outterRingRadius2= outterRingRadius*outterRingRadius;
		float innerRingRadius2= innerRingRadius*innerRingRadius;
		float outterFillRadius2= outterFillRadius*outterFillRadius;
		float innerFillRadius2= innerFillRadius*innerFillRadius;
		for(int x= 0; x < texture.width; ++x) {
			for(int y= 0; y < texture.height; ++y) {
				float rx= (float)x;
				float ry= (float)y;
				float ci= rx-cx;
				float cj= ry-cy;
				float r2= ci*ci+cj*cj;
				if(r2 > outterRingRadius2) {
					texture.SetPixel(x,y,Color.clear);
				} else if(r2 > innerRingRadius2) {
					float r= Mathf.Sqrt(r2);
					if(r > radius) {
						float ratio= (ringWidth-2f*(r-radius))/ringWidth;
						Color c= Color.red;
						c.a= ratio;
						texture.SetPixel(x,y,c);
					} else {
						float ratio= (ringWidth-2f*(radius-r))/ringWidth;
						Color c= Color.black;
						c.r= ratio;
						c.g= (1f-ratio);
						texture.SetPixel(x,y,c);						
					}
				} else if(r2 > outterFillRadius2) {
					texture.SetPixel(x,y,Color.green);
				} else if(r2 > innerFillRadius2) {
					float r= Mathf.Sqrt(r2);
					if(r > fillRadius) {
						float ratio= (ringWidth-2f*(r-fillRadius))/ringWidth;
						Color c= Color.black;
						c.g= (1f-ratio);
						c.b= ratio;
						texture.SetPixel(x,y,c);
					} else {
						texture.SetPixel(x,y,Color.blue);						
					}
				} else {
					texture.SetPixel(x,y,Color.blue);
				}
			}
		}
		
	}
	
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested circular port icon.
	public static Texture2D GetCircularPortIcon(Color nodeColor, Color typeColor) {
		return GetPortIcon(nodeColor, typeColor, ref myCircularPortIcons, ref myCircularPortTemplate);
	}
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested square port icon.
	public static Texture2D GetSquarePortIcon(Color nodeColor, Color typeColor) {
		return GetPortIcon(nodeColor, typeColor, ref mySquarePortIcons, ref mySquarePortTemplate);
	}
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested circular port icon.
	public static Texture2D GetSelectedCircularPortIcon(Color nodeColor, Color typeColor) {
		return GetPortIcon(nodeColor, typeColor,
			               ref mySelectedCircularPortIcons, ref mySelectedCircularPortTemplate);
	}
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested square port icon.
	public static Texture2D GetSelectedSquarePortIcon(Color nodeColor, Color typeColor) {
		return GetPortIcon(nodeColor, typeColor,
			               ref mySelectedSquarePortIcons, ref mySelectedSquarePortTemplate);
	}
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested circular port icon.
	static Texture2D GetPortIcon(Color nodeColor, Color typeColor,
								 ref Dictionary<Color,Dictionary<Color,Texture2D>> iconSet,
								 ref Texture2D iconTemplate) {
		if(iconSet == null) {
			iconSet= new Dictionary<Color,Dictionary<Color,Texture2D>>();
		}
		Dictionary<Color,Texture2D> dict;
		if(iconSet.ContainsKey(nodeColor)) {
			dict= iconSet[nodeColor];
		} else {
			dict= new Dictionary<Color,Texture2D>();
			iconSet[nodeColor]= dict;
		}
		if(dict.ContainsKey(typeColor)) {
			var existingIcon= dict[typeColor];
			if(existingIcon != null) return existingIcon;
		}
		int width= iconTemplate.width;
		int height= iconTemplate.height;
		Texture2D icon= new Texture2D(width, height);
		for(int x= 0; x < width; ++x) {
			for(int y= 0; y < height; ++y) {
				Color pixel= iconTemplate.GetPixel(x,y);
				float totalChannels= pixel.r+pixel.g+pixel.b;
				if(totalChannels == 0) {
					icon.SetPixel(x,y, pixel);					
				} else {
					// Anti-Aliasing fill.
					float nodeColorRatio= pixel.r/totalChannels;
					float fillColorRatio= pixel.g/totalChannels;
					float typeColorRatio= pixel.b/totalChannels;
					Color c;
					c.r= nodeColorRatio*nodeColor.r+typeColorRatio*typeColor.r+fillColorRatio*Color.black.r;
					c.g= nodeColorRatio*nodeColor.g+typeColorRatio*typeColor.g+fillColorRatio*Color.black.g;
					c.b= nodeColorRatio*nodeColor.b+typeColorRatio*typeColor.b+fillColorRatio*Color.black.b;
					c.a= pixel.a;
					icon.SetPixel(x,y, c);					
				}
			}
		}
		icon.Apply();
		icon.hideFlags= HideFlags.DontSave;
		dict[typeColor]= icon;
		return icon;
	}
	// ----------------------------------------------------------------------
	// Flush cached icons.
	static void FlushCachedIcons() {
		FlushCachedIcons(ref myCircularPortIcons);
		FlushCachedIcons(ref mySquarePortIcons);
		FlushCachedIcons(ref mySelectedSquarePortIcons);
		FlushCachedIcons(ref mySelectedCircularPortIcons);
	}
	// ----------------------------------------------------------------------
	static void FlushCachedIcons(ref Dictionary<Color,Dictionary<Color,Texture2D>> iconSet) {
		if(iconSet == null) return;
		foreach(var dictPair in iconSet) {
			foreach(var pair in dictPair.Value) {
				Texture2D.DestroyImmediate(pair.Value);
			}
		}
	}
}
