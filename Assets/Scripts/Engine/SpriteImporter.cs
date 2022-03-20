using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ZhukovEngine {

	public class SpriteImporter
	{
		//Contains all the sprites themselves
		public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

		//I do not understand this code but it works
		[System.Obsolete]
		public void Generate(string key, string fileName)
		{
			Texture2D tex;
			tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
			WWW www = new WWW(Path.Combine(Application.streamingAssetsPath, "GFX/", fileName));
			www.LoadImageIntoTexture(tex);
			Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
			sprite.name = fileName;
			sprites[key] = sprite;
		}

		public bool Contains(string key)
		{
			return sprites.ContainsKey(key);
		}

		//Allows you to do the [] thingy
		public Sprite this[string key]
		{
			get
			{
				return sprites[key];
			}
			set
			{
				sprites[key] = value;
			}
		}
	}
}