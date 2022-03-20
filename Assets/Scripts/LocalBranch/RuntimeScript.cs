using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using UnityEditor;

namespace ZhukovEngine.LocalBranch
{
	public enum ScopeTypes
	{
		Global,
		Country
	}

	public class RuntimeScript : MonoBehaviour
	{
		//Basic Game Stuff
		public string language;
		public string playerTag;
		public Dictionary<string, GameObject> uiObjects;
		public GameObject[] mapTiles;
		public string diploTag = null;

		//Sprite Importers
		public SpriteImporter spriteImporter = new SpriteImporter();
		public TMP_FontAsset mainFont;

		//Localisation
		public Localisation localisation;

		//Most Runtime Setup
		public Dictionary<string, Country> countries = new Dictionary<string, Country>();
		public List<string> partyTypes;
		public List<Color32> partyColors;
		public Dictionary<string, string[][]> scripts = new Dictionary<string, string[][]>();

		public List<string> actionsTags = new List<string>();

		//Early References

		void Awake()
		{
			LoadGameInfo();
			localisation = new Localisation();
			localisation.Generator("Core");
			foreach (string path in Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "localisation/")))
			{
				string filename = Path.GetFileName(path);
				if (filename.Substring(filename.Length - 15, 15) == ".zhlocalisation")
				{
					string filepart = filename.Substring(0, filename.Length - 19);
					Localisation newLocalisation = new Localisation();
					newLocalisation.Generator(filepart);
					foreach(string key in newLocalisation.Keys())
                    {
						localisation[key] = newLocalisation.staticKey(key);
                    }
				}
			}


			spriteImporter.Generate("Flag_Overlay", "Flag_Overlay.png");
			spriteImporter.Generate("tiling_1", "tiling/1.png");

			mainFont = (TMP_FontAsset)Resources.Load("Assets/trebuc SDF.asset", typeof(TMP_FontAsset));
			uiObjects = new Dictionary<string, GameObject>();
			uiObjects["topbar"] = GameObject.Find("Topbar");
			spriteImporter["tiling_1"] = Sprite.Create(spriteImporter["tiling_1"].texture, new Rect(0, 0, spriteImporter["tiling_1"].texture.width, spriteImporter["tiling_1"].texture.height), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.Tight, new Vector4(32, 32, 32, 32));
			uiObjects["topbar"].GetComponent<Image>().sprite = spriteImporter["tiling_1"];
			uiObjects["player_flag"] = uiObjects["topbar"].transform.Find("Flag").gameObject;
			uiObjects["player_flag_overlay"] = uiObjects["player_flag"].transform.Find("Flag_Overlay").gameObject;
			uiObjects["player_name"] = uiObjects["topbar"].transform.Find("CountryName").gameObject;
			uiObjects["player_ideology"] = uiObjects["topbar"].transform.Find("PartyIdeology").gameObject;
			uiObjects["player_party"] = uiObjects["topbar"].transform.Find("Party").gameObject;
			uiObjects["player_party_long"] = uiObjects["topbar"].transform.Find("PartyLong").gameObject;
			uiObjects["player_flag_overlay"].GetComponent<Image>().sprite = spriteImporter["Flag_Overlay"];

			uiObjects["diplomacy"] = GameObject.Find("Diplomacy");
			uiObjects["diplomacy_tab"] = uiObjects["diplomacy"].transform.Find("BGtab").gameObject;
			uiObjects["diplomacy_tab"].GetComponent<Image>().sprite = spriteImporter["tiling_1"];
			uiObjects["diplomacy_tab"].GetComponent<Image>().type = Image.Type.Tiled;

			uiObjects["diplomacy_flag"] = uiObjects["diplomacy"].transform.Find("Flag").gameObject;
			uiObjects["diplomacy_flag_overlay"] = uiObjects["diplomacy_flag"].transform.Find("Flag_Overlay").gameObject;
			uiObjects["diplomacy_name"] = uiObjects["diplomacy"].transform.Find("CountryName").gameObject;
			uiObjects["diplomacy_ideology"] = uiObjects["diplomacy"].transform.Find("PartyIdeology").gameObject;
			uiObjects["diplomacy_party"] = uiObjects["diplomacy"].transform.Find("Party").gameObject;
			uiObjects["diplomacy_party_long"] = uiObjects["diplomacy"].transform.Find("PartyLong").gameObject;
			uiObjects["diplomacy_flag_overlay"].GetComponent<Image>().sprite = spriteImporter["Flag_Overlay"];
			uiObjects["diplomacy_actions"] = uiObjects["diplomacy"].transform.Find("Options").gameObject;
			uiObjects["diplomacy_actions"].GetComponent<Image>().sprite = spriteImporter["tiling_1"];
			uiObjects["diplomacy_actions"].GetComponent<Image>().type = Image.Type.Tiled;
			uiObjects["diplomacy"].SetActive(false);

			uiObjects["debug_console"] = GameObject.Find("Console");
			uiObjects["debug_console"].SetActive(false);

			uiObjects["mapmode_country"] = GameObject.Find("MapModeCountry");
			uiObjects["mapmode_country"].GetComponent<Image>().sprite = spriteImporter["tiling_1"];
			uiObjects["mapmode_country"].GetComponent<Image>().type = Image.Type.Tiled;
			uiObjects["mapmode_ideology"] = GameObject.Find("MapModeIdeology");
			uiObjects["mapmode_ideology"].GetComponent<Image>().sprite = spriteImporter["tiling_1"];
			uiObjects["mapmode_ideology"].GetComponent<Image>().type = Image.Type.Tiled;
		}

        void Start()
		{
			uiObjects.Add("map", GameObject.Find("Map"));
			uiObjects.Add("map_background", uiObjects["map"].transform.Find("MapBG").gameObject);
			spriteImporter.Generate("map_background", "map/Map.png");
			uiObjects["map_background"].GetComponent<Image>().sprite = spriteImporter["map_background"];
			uiObjects["map_background"].GetComponent<Image>().rectTransform.sizeDelta = new Vector2(spriteImporter["map_background"].rect.width, spriteImporter["map_background"].rect.height);
			uiObjects["map_background"].GetComponent<Image>().mainTexture.filterMode = FilterMode.Point;

			partyTypes = new List<string>();
			
			LoadParties();
			LoadFlags();
			LoadCountries();
			LoadMap();
			LoadActions();

			LoadScript("start", "scripts/start.txt");
			RunScript(scripts["start"], new Dictionary<string, object>());

			UpdateUI(playerTag);
			UpdateMap(0);
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.BackQuote))
				uiObjects["debug_console"].SetActive(!uiObjects["debug_console"].activeSelf);
        }
		public void UpdateMap(int type)
        {
			foreach(GameObject tile in mapTiles)
            {
				switch (type)
				{
					case 0:
						tile.GetComponent<MapTile>().SetColor(countries[tile.GetComponent<MapTile>().owner].color);
						break;
					case 1:
						tile.GetComponent<MapTile>().SetColor(partyColors[countries[tile.GetComponent<MapTile>().owner].GetRulingParty()]);
						break;
				}
            }
        }
		public void RunDiplomaticAction(int id, string from, string target)
        {
			/* switch (id)
            {
				case 0:
					countries[from].SetCosmeticBase(target);
					break;
            } */
			RunScript(scripts["action_" + actionsTags[id]], new Dictionary<string, object>() { { "from", from }, { "target", target } }, new List<(ScopeTypes, string)>() { (ScopeTypes.Country, from), (ScopeTypes.Global, null) } );
        }
		void LoadScript(string key, string filePath)
        {
			scripts[key] = Abstract(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, filePath)));
		}
		void LoadCountries()
		{
			string[] lines = AbstractString(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "scripts/countries.txt"))).Split('\n');
			foreach (string line in lines)
			{
				string[] parsedLine = line.Split(' ');
				LoadScript(parsedLine[1], "scripts/countries/" + parsedLine[1] + ".txt");

				countries[parsedLine[0]] = new Country(parsedLine[0]);
				RunScript(scripts[parsedLine[1]], new Dictionary<string, object>(), new List<(ScopeTypes, string)>() { ( ScopeTypes.Country, parsedLine[0] ), ( ScopeTypes.Global, null ) } );
				//string countryLoad = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "scripts/countries/load_" + country.Tag + ".txt"));
				//RunScript(countryLoad, new Dictionary<string, int>());
			}
		}
		void LoadMap()
		{
			string[] lines = AbstractString(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "GFX/map/map.txt"))).Split('\n');
			mapTiles = new GameObject[lines.Length];
			int x = 0;
			foreach (string line in lines)
			{
				spriteImporter.Generate("map_tile_" + x.ToString(), "map/tiles/tile_" + x.ToString() + ".png");

				string[] parsedLine = line.Split(' ');
				GameObject gameObject = new GameObject();
				gameObject.transform.SetParent(uiObjects["map"].transform);
				gameObject.AddComponent<RectTransform>();
				gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
				gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
				gameObject.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
				gameObject.GetComponent<RectTransform>().localScale = new Vector2(1, 1);

				gameObject.AddComponent<Image>();
				gameObject.GetComponent<Image>().sprite = spriteImporter["map_tile_" + x.ToString()];
				gameObject.GetComponent<Image>().mainTexture.filterMode = FilterMode.Point;

				gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(
					(int)(Convert.ToInt32(parsedLine[1]) - Math.Floor(gameObject.GetComponent<Image>().sprite.rect.width / 2)),
					(int)-((Convert.ToInt32(parsedLine[2]) - Math.Floor(gameObject.GetComponent<Image>().sprite.rect.height / 2))));

				gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(gameObject.GetComponent<Image>().sprite.rect.width, gameObject.GetComponent<Image>().sprite.rect.height);

				gameObject.AddComponent<Button>();

				gameObject.AddComponent<MapTile>();
				gameObject.GetComponent<MapTile>().mapButton = gameObject.GetComponent<Button>();
				gameObject.GetComponent<MapTile>().tileId = x;
				//Debug.Log(countries.ContainsKey(parsedLine[3]));
				gameObject.GetComponent<MapTile>().owner = "ZZZ";
				gameObject.GetComponent<MapTile>().name = "tile_" + x.ToString();
				gameObject.GetComponent<MapTile>().SetColor(countries["ZZZ"].color);

				mapTiles[x] = gameObject;
				gameObject.name = "MapTile" + x.ToString();
				x++;
			}

			lines = AbstractString(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "GFX/map/connections.txt"))).Split('\n');
			foreach (string line in lines)
			{
				string[] parsedLine = line.Split(' ');
				mapTiles[Convert.ToInt32(parsedLine[0])].GetComponent<MapTile>().borders.Add(Convert.ToInt32(parsedLine[1]));
			}
		}
		public void UpdateUI(string uiTag)
		{
			if (uiTag == playerTag)
			{
				uiObjects["player_flag"].GetComponent<Image>().sprite = spriteImporter[countries[playerTag].CosmeticFlag() + "_flag"];
				uiObjects["player_name"].GetComponent<TMP_Text>().text = localisation.GetString(countries[playerTag].CosmeticKey(), new List<(ScopeTypes, string)>() { (ScopeTypes.Country, playerTag), (ScopeTypes.Global, null) }, new Dictionary<string, object>());
				uiObjects["player_ideology"].GetComponent<TMP_Text>().text = localisation.GetString(partyTypes[countries[playerTag].GetRulingParty()] + "_noun", new List<(ScopeTypes, string)>() { (ScopeTypes.Country, playerTag), (ScopeTypes.Global, null) }, new Dictionary<string, object>());
				uiObjects["player_party"].GetComponent<TMP_Text>().text = localisation.GetString(countries[playerTag].partyNames[countries[playerTag].GetRulingParty()], new List<(ScopeTypes, string)>() { (ScopeTypes.Country, playerTag), (ScopeTypes.Global, null) }, new Dictionary<string, object>());
				uiObjects["player_party_long"].GetComponent<TMP_Text>().text = localisation.GetString(countries[playerTag].partyNamesLong[countries[playerTag].GetRulingParty()], new List<(ScopeTypes, string)>() { (ScopeTypes.Country, playerTag), (ScopeTypes.Global, null) }, new Dictionary<string, object>());
			}
			if (uiTag == diploTag)
			{
				uiObjects["diplomacy_flag"].GetComponent<Image>().sprite = spriteImporter[countries[diploTag].CosmeticFlag() + "_flag"];
				uiObjects["diplomacy_name"].GetComponent<TMP_Text>().text = localisation.GetString(countries[diploTag].CosmeticKey(), new List<(ScopeTypes, string)>() { (ScopeTypes.Country, diploTag), (ScopeTypes.Global, null) }, new Dictionary<string, object>());
				uiObjects["diplomacy_ideology"].GetComponent<TMP_Text>().text = localisation.GetString(partyTypes[countries[diploTag].GetRulingParty()] + "_noun", new List<(ScopeTypes, string)>() { (ScopeTypes.Country, diploTag), (ScopeTypes.Global, null) }, new Dictionary<string, object>());
				uiObjects["diplomacy_party"].GetComponent<TMP_Text>().text = localisation.GetString(countries[diploTag].partyNames[countries[diploTag].GetRulingParty()], new List<(ScopeTypes, string)>() { (ScopeTypes.Country, diploTag), (ScopeTypes.Global, null) }, new Dictionary<string, object>());
				uiObjects["diplomacy_party_long"].GetComponent<TMP_Text>().text = localisation.GetString(countries[diploTag].partyNamesLong[countries[diploTag].GetRulingParty()], new List<(ScopeTypes, string)>() { (ScopeTypes.Country, diploTag), (ScopeTypes.Global, null) }, new Dictionary<string, object>());
			
			}
		}
		public string AbstractString(string script)
		{
			string[] lines = script.Split('\n');
			string finalScript = "";
			foreach (string line in lines)
			{
				string lineToParse = line;
				if (!String.IsNullOrWhiteSpace(lineToParse))
				{
					if (lineToParse.Contains('#'))
						lineToParse = line.Substring(0, lineToParse.IndexOf('#'));
					if (!String.IsNullOrWhiteSpace(lineToParse))
						finalScript += lineToParse.Trim() + '\n';
				}
			}
			return finalScript.Trim();
		}
		public string[][] Abstract(string script)
		{
			string[] lines = script.Split('\n');
			List<string[]> finalScript = new List<string[]>();
			foreach (string line in lines)
			{
				string lineToParse = line;
				if (!String.IsNullOrWhiteSpace(lineToParse))
				{
					if (lineToParse.Contains('#'))
						lineToParse = line.Substring(0, lineToParse.IndexOf('#'));
					if (!String.IsNullOrWhiteSpace(lineToParse))
						finalScript.Add(lineToParse.Trim().Split(' '));
				}
			}
			return finalScript.ToArray();
		}
		public string GrabCountryScope(string tag, List<(ScopeTypes, string)> scopes, Dictionary<string, object> passedVars)
        {

			if (countries.ContainsKey(tag))
			{
				return tag;
			}
			else
			{
				string scope = scopes[0].Item2;
				switch (tag.ToLower())
				{
					case "this":
						break;
					case "prev":
						scope = scopes[1].Item2;
						break;
					case "player":
						scope = playerTag;
						break;
					case "from":
						scope = (string)passedVars["from"];
						break;
					case "target":
						scope = (string)passedVars["target"];
						break;
					default:
						Debug.Log("Indeterminable scope: " + tag + ", using this (" + scope + ")");
						break;
				}

				return scope;
			}
		}
		public void RunScript(string[][] script, Dictionary<string, object> passedVars, List<(ScopeTypes, string)> scopes = null)
		{
			if (scopes == null)
				scopes = new List<(ScopeTypes, string)>
				{
					(ScopeTypes.Global, null)
				};
			string sectionName = null;
			string section = null;

			//string if_section = null;
			//bool if_trigger = false;
			bool is_in_if = false;
			bool is_in_else = false;
			bool is_if_true = true;
			
			foreach (string[] parsedLine in script)
			{
				if (sectionName == null)
				{
					if ((!is_in_if && !is_in_else) || (is_in_if && is_if_true) || (is_in_else && !is_if_true))
					{
						switch (parsedLine[0])
						{
							// GENERIC
							case "scope":
								if (parsedLine[1] == "country")
									scopes.Insert(0, (ScopeTypes.Country, GrabCountryScope(parsedLine[2], scopes, passedVars)));
								else if (parsedLine[2] == "global")
									scopes.Insert(0, (ScopeTypes.Global, null));
								break;

							case "end_scope":
								scopes.RemoveAt(0);
								break;

							case "start_section":
								section = "";
								sectionName = parsedLine[1];
								break;

							case "run_section":
								RunScript(scripts[parsedLine[1]], passedVars, scopes);
								break;

							case "if":
								is_in_if = true;
								if (parsedLine[2] == "yes")
								{
									is_if_true = EvaluateScript(scripts[parsedLine[1]], passedVars, scopes);
								}
								else
                                {
									is_if_true = !EvaluateScript(scripts[parsedLine[1]], passedVars, scopes);
								}
								break;

							case "end_if":
								is_in_if = false;
								break;

							case "end_else":
								is_in_else = false;
								break;

							case "else":
								is_in_else = true;
								break;

							// POLITICS
							case "set_ruling_party":
								countries[scopes[0].Item2].SetRulingParty(partyTypes.IndexOf(parsedLine[1]));
								break;

							// GENERIC COUNTRY
							case "set_country_flag":
								if (!countries[scopes[0].Item2].flags.Contains(parsedLine[1]))
									countries[scopes[0].Item2].flags.Add(parsedLine[1]);
								break;

							case "clr_country_flag":
								if (countries[scopes[0].Item2].flags.Contains(parsedLine[1]))
									countries[scopes[0].Item2].flags.Remove(parsedLine[1]);
								break;

							case "become_tile_owner":
								mapTiles[Convert.ToInt32(parsedLine[1])].GetComponent<MapTile>().SetOwner(scopes[0].Item2);
								mapTiles[Convert.ToInt32(parsedLine[1])].GetComponent<MapTile>().SetColor( countries[scopes[0].Item2].color );
								break;

							// COSMETICS
							case "set_cosmetic_base":
								countries[scopes[0].Item2].SetCosmeticBase(parsedLine[1]);
								break;

							case "reset_cosmetic_base":
								countries[scopes[0].Item2].SetCosmeticBase(scopes[0].Item2);
								break;

							case "set_color":
								if (parsedLine[4] == "yes")
								{
									countries[scopes[0].Item2].baseColor = new Color32(Convert.ToByte(parsedLine[1]), Convert.ToByte(parsedLine[2]), Convert.ToByte(parsedLine[3]), 255);
								}
								else
								{
									countries[scopes[0].Item2].color = new Color32(Convert.ToByte(parsedLine[1]), Convert.ToByte(parsedLine[2]), Convert.ToByte(parsedLine[3]), 255);
									countries[scopes[0].Item2].SetTileColors(countries[scopes[0].Item2].color);
								}
								break;

							case "reset_color":
								countries[scopes[0].Item2].color = countries[scopes[0].Item2].baseColor;
								countries[scopes[0].Item2].SetTileColors(countries[scopes[0].Item2].color);
								break;

							case "copy_color":
								countries[scopes[0].Item2].color = countries[GrabCountryScope(parsedLine[1], scopes, passedVars)].color;
								countries[scopes[0].Item2].SetTileColors(countries[scopes[0].Item2].color);
								break;

							case "set_party_name":
								countries[scopes[0].Item2].partyNames[partyTypes.IndexOf(parsedLine[1])] = parsedLine[2];
								countries[scopes[0].Item2].partyNamesLong[partyTypes.IndexOf(parsedLine[1])] = parsedLine[2] + "_long";
								UpdateUI(scopes[0].Item2);
								break;

							default:
								Debug.Log( "Unknown effect: " + String.Join(" ", parsedLine));
								break;
						}
					}
                    else
                    {
						if (is_in_if)
						{
							is_in_if = parsedLine[0] != "end_if";
						}
						if (is_in_else)
						{
							is_in_else = parsedLine[0] != "end_else";
						}
                    }
				}
				else
				{
					if (parsedLine[0] != "end_section")
						section += String.Join(" ", parsedLine) + '\n';
					else
					{
						scripts[sectionName] = Abstract(section);
						sectionName = null;
						section = null;
					}
				}
			}
		}
		public bool EvaluateScript(string[][] script, Dictionary<string, object> passedVars, List<(ScopeTypes, string)> scopes = null)
		{
			if (scopes == null)
				scopes = new List<(ScopeTypes, string)>
				{
					(ScopeTypes.Global, null)
				};
			foreach (string[] parsedLine in script)
			{
				switch (parsedLine[0])
				{
					// GENERIC
					case "scope":
						if (parsedLine[1] == "country")
							scopes.Insert(0, (ScopeTypes.Country, GrabCountryScope(parsedLine[2], scopes, passedVars)));
						else if (parsedLine[2] == "global")
							scopes.Insert(0, (ScopeTypes.Global, null));
						break;

					case "end_scope":
						scopes.RemoveAt(0);
						break;

					// GENERAL
					case "is_scope":
						if (scopes[0].Item2 != GrabCountryScope(parsedLine[1], scopes, passedVars))
							return false;
						break;

					case "eval_section":
						if (!EvaluateScript(scripts[parsedLine[1]], passedVars, scopes))
							return false;
						break;

					// GENERIC COUNTRY
					case "has_country_flag":
						if (!countries[scopes[0].Item2].flags.Contains(parsedLine[1]))
							return false;
						break;

					default:
						Debug.Log("Unknown trigger: " + String.Join(" ", parsedLine));
						break;
				}
			}

			return true;
		}
		public void LoadGameInfo()
		{
			string[] lines = AbstractString(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "scripts/gameinfo.txt"))).Split('\n');
			foreach (string line in lines)
			{
				string[] parsedLine = line.Split(' ');
				switch (parsedLine[0])
				{
					case "default_lang":
						language = parsedLine[1];
						break;
					case "default_tag":
						playerTag = parsedLine[1];
						break;
				}
			}
		}
		public void LoadParties()
		{
			string[] lines = AbstractString(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "scripts/parties.txt"))).Split('\n');
			foreach (string line in lines)
			{
				string[] parsedLine = line.Split(' ');
				partyTypes.Add(parsedLine[0]);
				partyColors.Add( new Color32(Convert.ToByte(parsedLine[1]), Convert.ToByte(parsedLine[2]), Convert.ToByte(parsedLine[3]), 255) );
			}
		}
		public void LoadFlags()
		{
			foreach (string path in Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "GFX/flags/")))
			{
				string line = Path.GetFileName(path);
				if (line.Substring(line.Length - 4, 4) == ".png")
					spriteImporter.Generate(line.Substring(0, line.Length - 4) + "_flag", "flags/" + line);
			}
		}
		public void LoadActions()
		{
			string[] lines = AbstractString(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "scripts/diplomatic_actions.txt"))).Split('\n');
			int i = 0;
			foreach (string action in lines)
			{
				GameObject gameObject = new GameObject();
				gameObject.transform.parent = uiObjects["diplomacy_actions"].transform;
				gameObject.AddComponent<RectTransform>();
				gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
				gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
				gameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
				gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(512, 64);
				gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -244 - (i * 96), 0);

				gameObject.AddComponent<Image>();
				gameObject.GetComponent<Image>().sprite = spriteImporter["tiling_1"];
				gameObject.GetComponent<Image>().type = Image.Type.Tiled;

				gameObject.AddComponent<Button>();
				gameObject.GetComponent<Button>().targetGraphic = gameObject.GetComponent<Image>();
				ColorBlock colors = gameObject.GetComponent<Button>().colors;
				colors.disabledColor = new Color32(128, 128, 128, 255);
				gameObject.GetComponent<Button>().colors = colors;

				gameObject.AddComponent<DiplomaticAction>();
				gameObject.GetComponent<DiplomaticAction>().diplomaticId = i;

				gameObject.name = "action_" + action;

				uiObjects["action_" + action] = gameObject;

				GameObject textObject = new GameObject();
				textObject.transform.parent = gameObject.transform;
				textObject.AddComponent<RectTransform>();
				textObject.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
				textObject.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
				textObject.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
				textObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

				textObject.AddComponent<TextMeshProUGUI>();
				textObject.GetComponent<TextMeshProUGUI>().font = mainFont;
				textObject.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
				textObject.GetComponent<TextMeshProUGUI>().fontSize = 28;
				textObject.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
				textObject.GetComponent<TextMeshProUGUI>().text = ("action_" + action);
				textObject.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
				textObject.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);

				actionsTags.Add(action);

				i++;
			}
		}
		public void OpenDiplomacy(string diploTag)
        {
			if (this.diploTag != diploTag)
			{
				this.diploTag = diploTag;
				UpdateUI(diploTag);
				uiObjects["diplomacy"].SetActive(true);
				foreach (string action in actionsTags)
				{
					uiObjects["action_" + action].GetComponent<Button>().interactable = EvaluateScript(scripts["action_" + action + "_available"], new Dictionary<string, object>() { { "target", diploTag }, { "from", playerTag } }, new List<(ScopeTypes, string)>() { (ScopeTypes.Country, playerTag), (ScopeTypes.Country, diploTag), (ScopeTypes.Global, null) });
                }
			}
			else
            {
				this.diploTag = null;
				uiObjects["diplomacy"].SetActive(false);
            }
        }
	}
}