using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(MapMaker))] 
public class MapMakerEditor : Editor {

	SerializedProperty layers;
	MapMaker mTarget;

	//choices for pixels per unit (ppu)
	int[] Ppus = new int[]{1,2,4,8,16,32,64};
	string[] sPpus = new string[]{"1","2","4","8","16","32","64"};

	int activeLayer = -1;

	bool eraserOn = false;

	Rect highlightRect = Rect.zero;
	Texture2D highlight;
	Rect tempRect = Rect.zero;
	int pushedX = -1;
	int pushedY = -1;
	int startShiftX = -1;
	int startShiftY = -1;
	int endShiftX = -1;
	int endShiftY = -1;
	int buttonRowWidth = -1;

	List<Rect> drawTileRects = new List<Rect>();
	List<int> drawTileIndex = new List<int>();

	List<int> PaletteSelection = new List<int>();

	void OnEnable(){
		
		mTarget = (MapMaker)target;
		highlight = new Texture2D(1,1);
		Color newColor = (Color.blue + Color.white*4)/5;
		newColor.a = .5f;
		highlight.SetPixel(0,0,newColor);
		highlight.Apply();
	}

	public override void OnInspectorGUI(){

		GUILayout.BeginVertical();
			MapWidthField();
			GUILayout.BeginHorizontal();
				mTarget.map.ppu = EditorGUILayout.IntPopup( mTarget.map.ppu , sPpus , Ppus , GUILayout.Width(60) );
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				if(GUILayout.Button("New Layer"))
				{
					CreateNewMesh();
				}
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			CreatePaletteButtons();

			GUILayout.Space(20);

			GUILayout.BeginHorizontal();
			mTarget.paletteTex = (Texture2D) EditorGUILayout.ObjectField("texture: " , mTarget.paletteTex , typeof (Texture2D),false);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			Color cl = GUI.color;
			if( eraserOn ){
				GUI.color = Color.blue;
			}

			if(GUILayout.Button("eraser")){
				if( eraserOn ){
					eraserOn = false;
				}
				else
				{
					eraserOn = true;
				}
				Debug.Log("eraserOn: " + eraserOn );
			}

			GUI.color = cl;

			GUILayout.Space(20);
			MakePalette(mTarget.paletteTex);

			if(GUILayout.Button("create map file")){
				CreateMapfile();
			}

		GUILayout.EndVertical();

	}

	void CreateMapfile(){
		string s = "";
		s += mTarget.map.height + "," + mTarget.map.width + ",";
		for( int i = 0; i < mTarget.map.layers[0].LinearGrid.Length; i++ ){
			s+= mTarget.map.layers[0].LinearGrid[i] + ",";
		}

		mTarget.map.Save("Assets/SceneMapData/"+EditorSceneManager.GetActiveScene().name + ".txt");
	}

	void MakePalette(Texture2D tex){

		int pScale = 2;

		//divide into buttons based on currently selected layer ppu.
		int ppu = mTarget.map.ppu;
		int xCount = tex.width/ppu;
		int yCount = tex.height/ppu;

		int windowWidth = (Screen.width-19-(ppu*2))/pScale;

		int widthCount = 0;

		GUILayout.BeginHorizontal();

		int i = 0;

		tempRect.x = -1;
		tempRect.y = -1;

		int rowWidthCount = 0;

		for( int y = 0; y < yCount; y++ ){
			for( int x = 0; x < xCount; x++ ){

				if(GUILayout.Button("b",GUIStyle.none,GUILayout.Width(ppu*pScale),GUILayout.Height(ppu*pScale))){
					if( Event.current.shift ){
						endShiftX = rowWidthCount;
						endShiftY = i/buttonRowWidth;

						Debug.Log( "ended shift at : " + endShiftX + "," + endShiftY ); 

						drawTileRects = new List<Rect>();
						drawTileIndex = new List<int>();
					}
					else
					{
						pushedX = x;
						pushedY = y;

						startShiftX = rowWidthCount;
						startShiftY = i/buttonRowWidth;

						endShiftX = -1;
						endShiftY = -1;
					}

				}
					
				if( pushedX == x && pushedY == y ){
					if( Event.current.type == EventType.Repaint ){
						tempRect = GUILayoutUtility.GetLastRect();

						Texture2D t = mTarget.paletteTex;

						float tileWidth = ppu/(t.width*1f);
						float tileHeight = ppu/(t.height*1f);

						drawTileRects = new List<Rect>();
						drawTileRects.Add( new Rect(x*tileWidth ,(1-tileHeight)-y*tileHeight , tileWidth, tileHeight ) );
						drawTileIndex.Add( (x + (y*x)) );
					}
				}

				//draw the texture on the button
				if( Event.current.type == EventType.Repaint ){
					DrawPaletteTex( GUILayoutUtility.GetLastRect() , x , y , ppu);
				}

				if( tempRect.x != -1 && tempRect.y != -1 ){
					highlightRect = tempRect;
					tempRect.x = -1; 
					tempRect.y = -1;
				}

				widthCount+=ppu;
				rowWidthCount++;

				if( widthCount >= windowWidth ){
					buttonRowWidth = rowWidthCount;
					rowWidthCount = 0;

					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					widthCount=0;
				}


				if( startShiftX != -1 && startShiftY != -1 && endShiftX != -1 && endShiftY != -1 ){
					if( 
						rowWidthCount > startShiftX 
						&& rowWidthCount <= endShiftX+1 
						&& i/buttonRowWidth >= startShiftY
						&& i/buttonRowWidth <= endShiftY
					)
					{
						Rect r = GUILayoutUtility.GetLastRect();
						GUI.DrawTextureWithTexCoords( r,highlight,new Rect(0,0,1,1));


						//add to the selection 
						//need to add UVs and x,y index.
						Texture2D t = mTarget.paletteTex;

						float tileWidth = ppu/(t.width*1f);
						float tileHeight = ppu/(t.height*1f);

						Rect addRect = new Rect(x*tileWidth ,(1-tileHeight)-y*tileHeight , tileWidth, tileHeight );
						if( drawTileRects.Contains( addRect ) == false ){
							drawTileRects.Add( addRect );
							drawTileIndex.Add( (x + (y*x)) );
						}
					}
				}

				i++;


			}
		}
		GUILayout.EndHorizontal();

		if( PaletteSelection.Count == 0 )
			GUI.DrawTextureWithTexCoords(highlightRect,highlight,new Rect(0,0,1,1));

	}
		
	void DrawPaletteTex( Rect r , int x , int y , int ppu ){

		//use x,y, and ppu to split up the texture and draw it to rect r.
		Texture2D tex = mTarget.paletteTex;

		float tileWidth = ppu/(tex.width*1f);
		float tileHeight = ppu/(tex.height*1f);

		Rect texRect = new Rect(x*tileWidth ,(1-tileHeight)-y*tileHeight , tileWidth, tileHeight );

		GUI.DrawTextureWithTexCoords( r , tex , texRect );
	}

	void CreatePaletteButtons(){
		int ct = mTarget.map.layers.Count;
		int removeIndex = -1;

		for( int i = 0; i < ct; i++ ){
			GUILayout.BeginHorizontal();
			string sel = "select";
			Color c = GUI.color;
			if(mTarget.map.layers[i].selected)
			{
				sel = "active";
				GUI.color = (Color.blue + Color.white + Color.white ) / 3;
				activeLayer = i;
			}
			if( GUILayout.Button(sel)){
				for( int j = 0; j < mTarget.map.layers.Count; j++ ){
					if( j == i ) mTarget.map.layers[j].selected = !mTarget.map.layers[j].selected;
					else
						mTarget.map.layers[j].selected = false;
				}
			}
			GUI.color = c;

			bool b =  mTarget.map.layers[i].visible;
			string str = "+";
			if( b == false ) str = "-";
			if(GUILayout.Button(str,GUILayout.Width(20))){
				mTarget.map.layers[i].visible=!mTarget.map.layers[i].visible;
			}
			EditorGUI.BeginChangeCheck();
			string s = EditorGUILayout.DelayedTextField( mTarget.map.layers[i].name , GUILayout.Width(80) );
			if( EditorGUI.EndChangeCheck() ){
				mTarget.transform.GetChild(i).gameObject.name = s;
			}
			mTarget.map.layers[i].name = s;

			if(GUILayout.Button("Delete")){
				removeIndex = i;	
			}
			GUILayout.Space(10);
			if( i != 0 ){
				if(GUILayout.Button("^",GUILayout.Width(20))){
					MapMaker.Map.Layer tmp = mTarget.map.layers[i];
					mTarget.map.layers[i] = mTarget.map.layers[i-1];
					mTarget.map.layers[i-1] = tmp;
					mTarget.transform.GetChild(i).SetSiblingIndex(i-1);
				}
			}
			else{
				GUILayout.Space(24);
			}

			if( i != ct-1 ){
				if(GUILayout.Button("v",GUILayout.Width(20))){
					MapMaker.Map.Layer tmp = mTarget.map.layers[i];
					mTarget.map.layers[i] = mTarget.map.layers[i+1];
					mTarget.map.layers[i+1] = tmp;
					mTarget.transform.GetChild(i).SetSiblingIndex(i+1);
				}
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		if( removeIndex != -1 ){
			mTarget.map.layers.RemoveAt(removeIndex);
			GameObject.DestroyImmediate(mTarget.transform.GetChild(removeIndex).gameObject);
		}
	}

	void MapWidthField(){
		GUILayout.BeginHorizontal();
		GUILayout.Label("MapSize: ");
		EditorGUI.BeginChangeCheck();
		mTarget.map.width = EditorGUILayout.IntField( mTarget.map.width, GUILayout.Width(40));
		mTarget.map.height = EditorGUILayout.IntField( mTarget.map.height, GUILayout.Width(40));
		if( EditorGUI.EndChangeCheck() ){

			Debug.Log("changing grid sizes to :" + mTarget.map.width + "," + mTarget.map.height);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	void CreateNewMesh(){
		mTarget.map.layers.Add( new MapMaker.Map.Layer() );
		Mesh newMesh = new Mesh();
		newMesh.name = "layerMesh";
		GameObject go = new GameObject();
		go.AddComponent<MeshFilter>();
		go.AddComponent<MeshRenderer>();
		go.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));

		go.transform.GetComponent<MeshFilter>().sharedMesh = newMesh;
		go.transform.parent = mTarget.transform;
		go.name = "layer";

		int x = mTarget.map.layers.Count-1;
		mTarget.map.layers[x].LinearGrid = new int[mTarget.map.width*mTarget.map.height];
		mTarget.map.layers[x].LinearUvs = new Rect[mTarget.map.width*mTarget.map.height];
	}

	#if UNITY_EDITOR
	void OnSceneGUI(){

		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

		//allows user to draw into the scene
		if( activeLayer != -1 ){
			if( Event.current.type == EventType.MouseDown | Event.current.type == EventType.MouseDrag )
			{
				//draw button
				if( Event.current.button == 0 )
				{
					Ray ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );
					Vector2 point = ray.GetPoint(0);

					int mX = Mathf.FloorToInt(point.x);
					int mY = Mathf.FloorToInt(point.y);

					if( endShiftX != -1 && startShiftX != -1 && endShiftY != -1 && startShiftY != -1 )
					{
						int selectWidth = endShiftX - startShiftX;
						int selectHeight = endShiftY - startShiftY;

						int i = 0;
						if( selectWidth >= 0 && selectHeight >= 0 )
						{
							for( int y = selectHeight; y >= 0; y--)
							{
								for( int x = 0; x <= selectWidth; x++)
								{
									if( eraserOn ){
										SetMap(mX+x,mY+y,0,drawTileRects[i]);
									}
									else{
										SetMap(mX+x,mY+y,drawTileIndex[i],drawTileRects[i]);
									}
									i++;
								}
							}
						}
						UpdateGrid(activeLayer);

					}
					else
					{
						if( eraserOn ){
							SetMap(mX,mY,0,drawTileRects[0]);
						}
						else
						{
                            try
                            {
                                SetMap(mX, mY, 1, drawTileRects[0]);
                            }
                            catch(System.Exception e)
                            {
                                Debug.Log(e);
                                Debug.Log("mX = " + mX + " mY = " + mY);
                            }
						}
						UpdateGrid(activeLayer);
					}	
				}

			}
		}
	}
	#endif

	void SetMap(int x , int y , int gridVal , Rect uvVal ){
		Debug.Log(x+","+y);
		if( x >= 0 && x < mTarget.map.width && y >= 0 && y < mTarget.map.height ){

			mTarget.map.layers[activeLayer].LinearGrid[grid2Dto1D(x,y)] = gridVal;
			mTarget.map.layers[activeLayer].LinearUvs[grid2Dto1D(x,y)] = uvVal;

		}

	}

	void UpdateGrid(int active){
		
		Mesh mesh = mTarget.transform.GetChild(active).GetComponent<MeshFilter>().sharedMesh;

		//go through the x,y of Grid and set triangles/vertexes for each one.

		//count all Grids that are not 0

		int[] g = mTarget.map.layers[activeLayer].LinearGrid;
		Rect[] u = mTarget.map.layers[activeLayer].LinearUvs;
		int len = 0;

		List<Vector3> GridVerts = new List<Vector3>();
		List<Vector2> UvCoords = new List<Vector2>();

		int i = 0;
		for( int x = 0; x < mTarget.map.width; x++ ){
			for( int y = 0; y < mTarget.map.height; y++){
				if( g[grid2Dto1D(x,y)] != 0 )
				{
					len++;
					GridVerts.Add( new Vector3(x,y,0));
					GridVerts.Add( new Vector3(x,y+1,0));
					GridVerts.Add( new Vector3(x+1,y,0));

					GridVerts.Add( new Vector3(x+1,y,0));
					GridVerts.Add( new Vector3(x,y+1,0));
					GridVerts.Add( new Vector3(x+1,y+1,0));

					UvCoords.Add( new Vector2( u[grid2Dto1D(x,y)].x , u[grid2Dto1D(x,y)].y ));
					UvCoords.Add( new Vector2( u[grid2Dto1D(x,y)].x , u[grid2Dto1D(x,y)].yMax ));
					UvCoords.Add( new Vector2( u[grid2Dto1D(x,y)].xMax , u[grid2Dto1D(x,y)].y ));

					UvCoords.Add( new Vector2( u[grid2Dto1D(x,y)].xMax , u[grid2Dto1D(x,y)].y ));
					UvCoords.Add( new Vector2( u[grid2Dto1D(x,y)].x , u[grid2Dto1D(x,y)].yMax ));
					UvCoords.Add( new Vector2( u[grid2Dto1D(x,y)].xMax , u[grid2Dto1D(x,y)].yMax ));

					i++;
				}
			}
		}

		int[] tIndex = new int[len*6];
		int ct = 0;
		for( int t = 0; t < tIndex.Length; t++ ){
			tIndex[t] = ct;
			ct++;
		}
		mesh.vertices = GridVerts.ToArray();
		mesh.triangles = tIndex;
		mesh.uv = UvCoords.ToArray();
	}

	int grid2Dto1D( int x , int y ){
		return (x + (mTarget.map.width*y));
	}

}
