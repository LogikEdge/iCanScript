using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class DSTableView : DSView {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    DSTitleView     		myMainView         = null;
    Vector2                 myScrollbarPosition= Vector2.zero;
    DSTableViewDataSource	myDataSource       = null;
	List<DSTableColumn>		myColumns		   = new List<DSTableColumn>();
	float[]				    myRowHeights	   = new float[0];
	Vector2					myColumnTitleSize;
	Vector2					myColumnDataSize;
	GUIStyle				myColumnTitleGUIStyle= null;
	bool					myColumnTitleSeperator;
	bool					myDisplayColumnFrame;
	
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    public DSTableViewDataSource DataSource {
        get { return myDataSource; }
        set { myDataSource= value; RecomputeColumnAreas(); }
    }
	public GUIStyle ColumnTitleGUIStyle {
		get { return myColumnTitleGUIStyle ?? EditorStyles.boldLabel; }
		set { myColumnTitleGUIStyle= value; }
	}
	
    // ======================================================================
    // Initialization
    // ----------------------------------------------------------------------
    public DSTableView(RectOffset margins, bool displayFrame,
                       GUIContent title, AnchorEnum titleAlignment, bool titleSeperator,
					   bool displayColumnFrame, bool columnTitleSeperator= true) {
        myMainView= new DSTitleView(margins, displayFrame,
                                    title, titleAlignment, titleSeperator,
                                    DisplayMainView, GetMainViewDisplaySize);
		myColumnTitleSeperator= columnTitleSeperator;
		myDisplayColumnFrame= displayColumnFrame;
    }
    
    // ======================================================================
    // DSView implementation.
    // ----------------------------------------------------------------------
    public override void Display(Rect frameArea) {
        myMainView.Display(frameArea);
    }
    public override Vector2 GetSizeToDisplay(Rect frameArea) {
		RecomputeColumnAreas();
        return myMainView.GetSizeToDisplay(frameArea);
    }
    public override AnchorEnum GetAnchor() {
        return myMainView.Anchor;
    }
    public override void SetAnchor(AnchorEnum anchor) {
        myMainView.Anchor= anchor;
    }

    // ======================================================================
    // Main View implementation.
    // ----------------------------------------------------------------------
    void DisplayMainView(DSTitleView view, Rect displayArea) {
        // Compute piece-part display areas.
        Rect titleDisplayArea= new Rect(displayArea.x, displayArea.y,
                                        Mathf.Min(displayArea.width, myColumnTitleSize.x),
                                        Mathf.Min(displayArea.height, myColumnTitleSize.y));
        Rect dataDisplayArea= new Rect(displayArea.x, titleDisplayArea.yMax,
                                       Mathf.Min(displayArea.width, myColumnDataSize.x),
                                       Mathf.Min(displayArea.height-titleDisplayArea.height, myColumnDataSize.y));
        if(dataDisplayArea.height < 0) dataDisplayArea.height= 0;
        
        // Compute scrollbar information.
        bool needHorizontalScrollbar= false;
        bool needVerticalScrollbar= false;
        
        float displayWidth= displayArea.width;
        float displayHeight= displayArea.height;
        if(myColumnTitleSize.x > displayWidth) {
            needHorizontalScrollbar= true;
            dataDisplayArea.height-= kScrollbarSize;
            if(dataDisplayArea.height < 0) dataDisplayArea.height= 0;
        }
        float dataHeight= displayHeight-myColumnTitleSize.y-(needHorizontalScrollbar ? kScrollbarSize : 0);
        if(myColumnDataSize.y > dataHeight) {
            needVerticalScrollbar= true;
            dataDisplayArea.width-= kScrollbarSize;
            if(dataDisplayArea.width < 0) dataDisplayArea.width= 0;
        }
		
        // Display column titles.
        GUI.BeginGroup(titleDisplayArea);
            DisplayColumnTitles();              
        GUI.EndGroup();
		if(myColumnTitleSeperator) {
			GUI.Box(new Rect(titleDisplayArea.x, titleDisplayArea.yMax-2, titleDisplayArea.width, 3), "");
		}
        // Display column data.
        GUI.BeginGroup(dataDisplayArea);
            DisplayColumnData();
        GUI.EndGroup();

        // Display scrollbar if needed.
        if(needHorizontalScrollbar) {
            Rect scrollbarPos= new Rect(titleDisplayArea.x, displayArea.yMax-kScrollbarSize, titleDisplayArea.width, kScrollbarSize);
            myScrollbarPosition.x= GUI.HorizontalScrollbar(scrollbarPos, myScrollbarPosition.x, dataDisplayArea.width, 0, myColumnTitleSize.x);
        }
        if(needVerticalScrollbar) {
            Rect scrollbarPos= new Rect(dataDisplayArea.xMax, dataDisplayArea.y, kScrollbarSize, displayArea.height-myColumnTitleSize.y-(needHorizontalScrollbar ? kScrollbarSize : 0));
            myScrollbarPosition.y= GUI.VerticalScrollbar(scrollbarPos, myScrollbarPosition.y, dataDisplayArea.height, 0, myColumnDataSize.y);
        }
    }
    Vector2 GetMainViewDisplaySize(DSTitleView view, Rect displayArea) {
        float width= Mathf.Max(myColumnTitleSize.x, myColumnDataSize.x);
        float height= myColumnTitleSize.y+myColumnDataSize.y;
        return new Vector2(width, height);
    }
    // ----------------------------------------------------------------------
    void DisplayColumnTitles() {
        Rect titleArea= new Rect(-myScrollbarPosition.x, 0, myColumnTitleSize.x, myColumnTitleSize.y);
		foreach(var column in myColumns) {
			Rect titleFrameArea= titleArea;
			titleFrameArea.width= column.DataSize.x;
			Rect columnTitleArea= titleFrameArea;
			columnTitleArea.x+= column.Margins.left;
			columnTitleArea.width-= column.Margins.horizontal;
			columnTitleArea.y+= column.Margins.top;
			columnTitleArea.height-= column.Margins.vertical;
			if(column.Title != null) {
				Rect titleDisplayArea= DSCellView.PerformAlignment(columnTitleArea, ColumnTitleGUIStyle.CalcSize(column.Title), column.Anchor);
				GUI.Label(titleDisplayArea, column.Title, ColumnTitleGUIStyle);
				if(myDisplayColumnFrame) {
					GUI.Box(titleFrameArea, "");
				}			
			}
			titleArea.x+= column.DataSize.x;
			titleArea.width-= column.DataSize.x;
		}
    }
    // ----------------------------------------------------------------------
    void DisplayColumnData() {
        Rect dataArea= new Rect(-myScrollbarPosition.x, -myScrollbarPosition.y, myColumnDataSize.x, myColumnDataSize.y);
        float y= dataArea.y;
        for(int row= 0; row < myRowHeights.Length; ++row) {
            float x= dataArea.x;
            foreach(var column in myColumns) {
                Rect displayRect= new Rect(x+column.Margins.left, y+column.Margins.top, column.DataSize.x-column.Margins.horizontal, myRowHeights[row]-column.Margins.vertical);
                Vector2 dataSize= myDataSource.DisplaySizeForObjectInTableView(this, column, row);
                displayRect= DSCellView.PerformAlignment(displayRect, dataSize, column.Anchor);
	            myDataSource.DisplayObjectInTableView(this, column, row, displayRect);					
                x+= column.DataSize.x;
            }
            y+= myRowHeights[row];
        }
		if(myDisplayColumnFrame) {
            float x= dataArea.x;
			foreach(var column in myColumns) {
				GUI.Box(new Rect(x, dataArea.y, column.DataSize.x, dataArea.height), "");
                x+= column.DataSize.x;				
			}
		}
    }
    
    // ======================================================================
    // Column Methods
    // ----------------------------------------------------------------------
    public DSTableColumn FindTableColumn(string identifier) {
        DSTableColumn result= null;
		foreach(var tableColumn in myColumns) {
            if(string.Compare(tableColumn.Identifier, identifier) == 0) {
                result= tableColumn;
				break;
            }			
		}
        return result;
    }
	
    // ----------------------------------------------------------------------
	void RecomputeColumnAreas() {
		// Clear previous column information.
		myRowHeights= new float[0];
		if(myDataSource == null) return;
		
		// Recompute column data size.
		float maxTitleHeight= 0;
		float dataWidth= 0;
		int nbOfRows= myDataSource.NumberOfRowsInTableView(this);
		myRowHeights= new float[nbOfRows]; for(int row= 0; row < nbOfRows; ++row) myRowHeights[row]= 0f;
		foreach(var tableColumn in myColumns) {
			// Determine title area height.
			var titleSize= tableColumn.Title != null ? ColumnTitleGUIStyle.CalcSize(tableColumn.Title) : Vector2.zero;
			if(titleSize.y > maxTitleHeight) maxTitleHeight= titleSize.y;
			// Determine column data area.
			float maxCellWidth= titleSize.x;
			for(int row= 0; row < nbOfRows; ++row) {
				var cellSize= myDataSource.DisplaySizeForObjectInTableView(this, tableColumn, row);
				if(cellSize.x > maxCellWidth) maxCellWidth= cellSize.x;
				if(cellSize.y > myRowHeights[row]) myRowHeights[row]= cellSize.y;
			}
			maxCellWidth+= tableColumn.Margins.horizontal;
			dataWidth+= maxCellWidth;
			tableColumn.DataSize= new Vector2(maxCellWidth, 0);			
		}
		float dataHeight= 0; foreach(var height in myRowHeights) { dataHeight+= height; }
		foreach(var column in myColumns) { var tmp= column.DataSize; tmp.y= dataHeight; column.DataSize= tmp; }
		myColumnDataSize= new Vector2(dataWidth, dataHeight);
		myColumnTitleSize= new Vector2(dataWidth, maxTitleHeight+(myColumnTitleSeperator ? 3 : 0));
	}
	
	// ======================================================================
    // Column management
    // ----------------------------------------------------------------------
    public void AddColumn(DSTableColumn column) {
		myColumns.Add(column);
    }
    public bool RemoveColumn(DSTableColumn column) {
		bool result= myColumns.Remove(column);
		return result;
    }
    
}
