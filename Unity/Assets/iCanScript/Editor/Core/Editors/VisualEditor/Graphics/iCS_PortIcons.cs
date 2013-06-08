using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// TODO: Need to cleanup obsoleted data port icon code.
public static class iCS_PortIcons {
    // ======================================================================
    // PROPERTIES
    // ----------------------------------------------------------------------
	static float		myScale					    = 0f;
	static Texture2D	myInEndPortTemplate            = null;
	static Texture2D	myOutEndPortTemplate           = null;
	static Texture2D	myInRelayPortTemplate  	       = null;
	static Texture2D	myOutRelayPortTemplate  	   = null;
	static Texture2D	mySelectedInEndPortTemplate    = null;
	static Texture2D	mySelectedOutEndPortTemplate   = null;
	static Texture2D	mySelectedInRelayPortTemplate  = null;
	static Texture2D	mySelectedOutRelayPortTemplate = null;
	
    // ----------------------------------------------------------------------
	static Dictionary<Color,Texture2D>	myInEndPortIcons           = null;
	static Dictionary<Color,Texture2D>	myOutEndPortIcons          = null;
	static Dictionary<Color,Texture2D>	mySelectedInEndPortIcons   = null;
	static Dictionary<Color,Texture2D>	mySelectedOutEndPortIcons  = null;
	static Dictionary<Color,Texture2D>	myInRelayPortIcons         = null;
	static Dictionary<Color,Texture2D>	myOutRelayPortIcons        = null;
	static Dictionary<Color,Texture2D>	mySelectedInRelayPortIcons = null;
	static Dictionary<Color,Texture2D>	mySelectedOutRelayPortIcons= null;

	// ----------------------------------------------------------------------
    //  Build template for all port icons
	public static void BuildPortIconTemplates(float scale) {
		if(Math3D.IsEqual(scale, myScale)) return;
		BuildEndPortTemplates(scale);
		BuildRelayPortTemplates(scale);
		myScale= scale;
		FlushCachedIcons();
	}
	// ----------------------------------------------------------------------
	static void BuildRelayPortTemplates(float scale) {
        float len= scale*iCS_EditorConfig.PortDiameter;
        float selectedLen= len*iCS_EditorConfig.SelectedPortFactor;
		BuildInRelayPortTemplate(len, ref myInRelayPortTemplate);
		BuildOutRelayPortTemplate(len, ref myOutRelayPortTemplate);
		BuildInRelayPortTemplate(selectedLen, ref mySelectedInRelayPortTemplate);
		BuildOutRelayPortTemplate(selectedLen, ref mySelectedOutRelayPortTemplate);
	}
	// ----------------------------------------------------------------------
	public static void BuildInRelayPortTemplate(float len, ref Texture2D template) {
		// Create texture.
		int lenInt= (int)(len+1f);
		int borderSize= myScale > 1.25f ? 3 : (myScale > 0.75f ? 2 : (myScale > 0.5 ? 1 : 0));
        // Remove previous template.
        if(template != null) Texture2D.DestroyImmediate(template);
		template= new Texture2D(lenInt, lenInt);
		for(int x= 0; x < lenInt; ++x) {
			for(int y= 0; y < lenInt; ++y) {
				if(x <= borderSize || y <= borderSize || x >= lenInt-1-borderSize || y >= lenInt-1-borderSize) {
					template.SetPixel(x,y,Color.black);
				} else  if(x <= borderSize+1 || x >= lenInt-borderSize-2 || y <= borderSize+1 || y >= lenInt-borderSize-2) {
					template.SetPixel(x,y,Color.red);
				} else {
					template.SetPixel(x,y,Color.black);					
				}
			}
		}
		template.hideFlags= HideFlags.DontSave;
		template.Apply();
	}
	// ----------------------------------------------------------------------
	public static void BuildOutRelayPortTemplate(float len, ref Texture2D template) {
		// Create texture.
		int lenInt= (int)(len+1f);
		int borderSize= myScale > 1.25f ? 3 : (myScale > 0.75f ? 2 : (myScale > 0.5 ? 1 : 0));
        // Remove previous template.
        if(template != null) Texture2D.DestroyImmediate(template);
		template= new Texture2D(lenInt, lenInt);
		for(int x= 0; x < lenInt; ++x) {
			for(int y= 0; y < lenInt; ++y) {
				if(x <= borderSize || y <= borderSize || x >= lenInt-1-borderSize || y >= lenInt-1-borderSize) {
					template.SetPixel(x,y,Color.black);
				} else {
					template.SetPixel(x,y,Color.red);					
				}
			}
		}
		template.hideFlags= HideFlags.DontSave;
		template.Apply();
	}
	// ----------------------------------------------------------------------
	static void BuildEndPortTemplates(float scale) {
        float radius= scale*iCS_EditorConfig.PortRadius;
        float inInnerRadius= radius-2f*scale;
        float outInnerRadius= radius-3f*scale;
		BuildInEndPortTemplate(radius, inInnerRadius, 1f, ref myInEndPortTemplate);
		BuildOutEndPortTemplate(radius, outInnerRadius, 1f, ref myOutEndPortTemplate);
        float selectedFactor= iCS_EditorConfig.SelectedPortFactor;
		float selectedRadius= selectedFactor*radius;
		float selectedInInnerRadius= selectedFactor*inInnerRadius;
		float selectedOutInnerRadius= selectedFactor*outInnerRadius;
		BuildInEndPortTemplate(selectedRadius, selectedInInnerRadius, 1f, ref mySelectedInEndPortTemplate);
		BuildOutEndPortTemplate(selectedRadius, selectedOutInnerRadius, 1f, ref mySelectedOutEndPortTemplate);
	}
	// ----------------------------------------------------------------------
    delegate void PortTemplateBuilder(float radius, float innerRadius, float aaWidth, ref Texture2D template);
	static void BuildPortTemplate(float radius, float innerRadius, float aaWidth, ref Texture2D template, PortTemplateBuilder builder) {
        // Remove previous template.
        if(template != null) Texture2D.DestroyImmediate(template);
		// Create texture.
		int widthInt= (int)(2f*radius+3f);
		int heightInt= (int)(2f*radius+3f);
		template= new Texture2D(widthInt, heightInt, TextureFormat.ARGB32, false);
		builder(radius, innerRadius, aaWidth, ref template);
		// Finalize texture.
		template.hideFlags= HideFlags.DontSave;
		template.Apply();
	}
	// ----------------------------------------------------------------------
	static void BuildInEndPortTemplate(float radius, float innerRadius, float aaWidth, ref Texture2D template) {
        BuildPortTemplate(radius, innerRadius, aaWidth, ref template, BuildInEndPortTemplateImp);
	}
	// ----------------------------------------------------------------------
	static void BuildOutEndPortTemplate(float radius, float innerRadius, float aaWidth, ref Texture2D template) {
        BuildPortTemplate(radius, innerRadius, aaWidth, ref template, BuildOutEndPortTemplateImp);
	}
	// ----------------------------------------------------------------------
	public static void BuildDataPortTemplateImp(float radius, ref Texture2D texture, float ringWidth= 2f) {
        if(ringWidth < 2f) ringWidth= 2f;
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
	public static void BuildInEndPortTemplateImp(float radius, float innerRadius, float aaWidth, ref Texture2D texture) {
        float innerRadiusMax= innerRadius+0.5f*myScale;
        float innerRadiusMin= innerRadius-0.5f*myScale;
        float aaRadiusMax= radius+aaWidth;
        float aaInnerRadiusMax= innerRadiusMax+aaWidth;
        float aaInnerRadiusMin= innerRadiusMin-aaWidth;
		float cx= 0.5f*texture.width;
		float cy= 0.5f*texture.height;
		
        float aaRadiusMax2= aaRadiusMax*aaRadiusMax;
        float radius2= radius*radius;
        float aaInnerRadiusMax2= aaInnerRadiusMax*aaInnerRadiusMax;
        float innerRadiusMax2= innerRadiusMax*innerRadiusMax;
        float innerRadiusMin2= innerRadiusMin*innerRadiusMin;
        float aaInnerRadiusMin2= aaInnerRadiusMin*aaInnerRadiusMin;
		for(int x= 0; x < texture.width; ++x) {
			for(int y= 0; y < texture.height; ++y) {
				float rx= (float)x;
				float ry= (float)y;
				float ci= rx-cx;
				float cj= ry-cy;
				float r2= ci*ci+cj*cj;
				if(r2 > aaRadiusMax2) {
					texture.SetPixel(x,y,Color.clear);
				} else if(r2 > radius2) {
					float r= Mathf.Sqrt(r2);
    				float ratio= (aaRadiusMax-r)/aaWidth;
    				Color c= Color.black;
    				c.a= ratio;
    				texture.SetPixel(x,y,c);
				} else if(r2 > aaInnerRadiusMax2) {
					texture.SetPixel(x,y,Color.black);
				} else if(r2 > innerRadiusMax2) {
					float r= Mathf.Sqrt(r2);
					float ratio= (aaInnerRadiusMax-r)/aaWidth;
					Color c= Color.red;
					c.r*= ratio;
					texture.SetPixel(x,y,c);
				} else if(r2 > innerRadiusMin2) {
					texture.SetPixel(x,y,Color.red);				    
				} else if(r2 > aaInnerRadiusMin2) {
					float r= Mathf.Sqrt(r2);
					float ratio= (r-aaInnerRadiusMin)/aaWidth;
					Color c= Color.red;
					c.r*= ratio;
					texture.SetPixel(x,y,c);
				} else {
					texture.SetPixel(x,y,Color.black);
				}
			}
		}		
	}
	// ----------------------------------------------------------------------
	public static void BuildOutEndPortTemplateImp(float radius, float innerRadius, float aaWidth, ref Texture2D texture) {
		float aaRadiusMax= radius+aaWidth;
		float aaInnerRadiusMax= innerRadius+aaWidth;
		float cx= 0.5f*texture.width;
		float cy= 0.5f*texture.height;
		
        float radius2= radius*radius;
        float innerRadius2= innerRadius*innerRadius;
		float aaRadiusMax2= aaRadiusMax*aaRadiusMax;
		float aaInnerRadiusMax2= aaInnerRadiusMax*aaInnerRadiusMax;
		for(int x= 0; x < texture.width; ++x) {
			for(int y= 0; y < texture.height; ++y) {
				float rx= (float)x;
				float ry= (float)y;
				float ci= rx-cx;
				float cj= ry-cy;
				float r2= ci*ci+cj*cj;
				if(r2 > aaRadiusMax2) {
					texture.SetPixel(x,y,Color.clear);
				} else if(r2 > radius2) {
					float r= Mathf.Sqrt(r2);
					float ratio= (aaWidth-(r-radius))/aaWidth;
					Color c= Color.black;
					c.a= ratio;
					texture.SetPixel(x,y,c);
				} else if(r2 > aaInnerRadiusMax2){
					texture.SetPixel(x,y,Color.black);
			    } else if(r2 > innerRadius2) {
					float r= Mathf.Sqrt(r2);
					float ratio= (aaWidth-(r-innerRadius))/aaWidth;
					Color c= Color.red;
					c.r*= ratio;
					texture.SetPixel(x,y,c);
				} else {
					texture.SetPixel(x,y,Color.red);
				}
			}
		}		
	}
	
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested circular port icon.
	public static Texture2D GetInEndPortIcon(Color typeColor) {
		return GetPortIcon(typeColor, ref myInEndPortIcons, ref myInEndPortTemplate);
	}
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested end port icon.
	public static Texture2D GetOutEndPortIcon(Color typeColor) {
		return GetPortIcon(typeColor, ref myOutEndPortIcons, ref myOutEndPortTemplate);
	}
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested relay port icon.
	public static Texture2D GetInRelayPortIcon(Color typeColor) {
		return GetPortIcon(typeColor, ref myInRelayPortIcons, ref myInRelayPortTemplate);
	}
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested relay port icon.
	public static Texture2D GetOutRelayPortIcon(Color typeColor) {
		return GetPortIcon(typeColor, ref myOutRelayPortIcons, ref myOutRelayPortTemplate);
	}
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested end port icon.
	public static Texture2D GetSelectedInEndPortIcon(Color typeColor) {
		return GetPortIcon(typeColor, ref mySelectedInEndPortIcons, ref mySelectedInEndPortTemplate);
	}
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested end port icon.
	public static Texture2D GetSelectedOutEndPortIcon(Color typeColor) {
		return GetPortIcon(typeColor, ref mySelectedOutEndPortIcons, ref mySelectedOutEndPortTemplate);
	}
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested relay port icon.
	public static Texture2D GetSelectedInRelayPortIcon(Color typeColor) {
		return GetPortIcon(typeColor,
			               ref mySelectedInRelayPortIcons, ref mySelectedInRelayPortTemplate);
	}
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested relay port icon.
	public static Texture2D GetSelectedOutRelayPortIcon(Color typeColor) {
		return GetPortIcon(typeColor,
			               ref mySelectedOutRelayPortIcons, ref mySelectedOutRelayPortTemplate);
	}
	// ----------------------------------------------------------------------
	// Returns a texture representing the requested circular port icon.
	static Texture2D GetPortIcon(Color typeColor,
								 ref Dictionary<Color,Texture2D> iconSet,
								 ref Texture2D iconTemplate) {
		if(iconSet == null) {
			iconSet= new Dictionary<Color,Texture2D>();
		}
		if(iconSet.ContainsKey(typeColor)) {
			var existingIcon= iconSet[typeColor];
			if(existingIcon != null) {
			    return existingIcon;
		    }
		}
		Texture2D icon= BuildPortIcon(typeColor, iconTemplate);
		iconSet[typeColor]= icon;
		return icon;
	}
	// ----------------------------------------------------------------------
    public static Texture2D BuildPortIcon(Color typeColor, Texture2D iconTemplate) {
		int width= iconTemplate.width;
		int height= iconTemplate.height;
		Texture2D icon= new Texture2D(width, height);
		for(int x= 0; x < width; ++x) {
			for(int y= 0; y < height; ++y) {
				Color pixel= iconTemplate.GetPixel(x,y);
				if(pixel.r == 0) {
					icon.SetPixel(x,y, pixel);					
				} else {
					// Anti-Aliasing fill.
					Color c;
					c.r= pixel.r*typeColor.r;
					c.g= pixel.r*typeColor.g;
					c.b= pixel.r*typeColor.b;
					c.a= pixel.a;
					icon.SetPixel(x,y, c);					
				}
			}
		}
		icon.Apply();
		icon.hideFlags= HideFlags.DontSave;
		return icon;        
    }
    
	// ----------------------------------------------------------------------
	// Flush cached icons.
	static void FlushCachedIcons() {
		FlushCachedIcons(ref myInEndPortIcons);
		FlushCachedIcons(ref myOutEndPortIcons);
		FlushCachedIcons(ref myInRelayPortIcons);
		FlushCachedIcons(ref myOutRelayPortIcons);
		FlushCachedIcons(ref mySelectedInEndPortIcons);
		FlushCachedIcons(ref mySelectedOutEndPortIcons);
		FlushCachedIcons(ref mySelectedInRelayPortIcons);
		FlushCachedIcons(ref mySelectedOutRelayPortIcons);
	}
	// ----------------------------------------------------------------------
	static void FlushCachedIcons(ref Dictionary<Color,Texture2D> iconSet) {
		if(iconSet == null) return;
		foreach(var pair in iconSet) {
			Texture2D.DestroyImmediate(pair.Value);
		}
	}
}
