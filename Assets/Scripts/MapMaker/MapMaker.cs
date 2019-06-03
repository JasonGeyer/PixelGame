using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class MapMaker : MonoBehaviour {

	public Map map = new Map() ;

	public int paletteScale = 1 ;

	public Texture2D paletteTex ;

	public void LoadMapData(){
		string s = transform.Find("MapData").GetComponent<MapDataHolder>().mapData.text ;
		map = map.LoadFromText(s) ;
	}

	[System.Serializable]
	public class Map{

		public int width = 200;
		public int height = 100;

		//public Texture2D paletteTex ;
		public int ppu = 32 ;

		public List<Layer> layers = new List<Layer>() ;

		[System.Serializable]
		public class Layer{
			
			//public int[,] Grid ;

			[SerializeField]
			public int[] LinearGrid ;
			public Rect[] LinearUvs ;

			//public Rect[,] Uvs ;

			public string name = "layer" ;

			public bool visible = true;
			public bool selected = false;

			//public int ppu = 32 ;

		}

		public void Save( string path ){
			var serializer = new XmlSerializer(typeof(Map));
			using(var stream = new FileStream(path, FileMode.Create))
			{
				serializer.Serialize(stream, this);
			}
		}

		public Map LoadFromText( string text ){
			var serializer = new XmlSerializer(typeof(Map));
			return serializer.Deserialize(new StringReader(text)) as Map;
		}

	}
}
