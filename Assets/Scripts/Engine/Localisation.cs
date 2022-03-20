using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ZhukovEngine.LocalBranch;

namespace ZhukovEngine
{
	public class Localisation
	{
		private Dictionary<string, string> content = new Dictionary<string, string>();
		LocalBranch.RuntimeScript runtimeScript;

		public Localisation()
		{
			runtimeScript = GameObject.Find("Runtime").GetComponent<LocalBranch.RuntimeScript>();
		}

		//Generates Localisation
		public void Generator(string fileName, bool ignoreLang = false)
		{
			//Temporary data variables
			string[] lines = new string[] { };
			Dictionary<string, string> localisation = new Dictionary<string, string>();

			//Tests for Language
			if (ignoreLang)
			{
				lines = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, "localisation/", (fileName + ".zhlocalisation")));
			}
			else
			{
				lines = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, "localisation/", (fileName + "_" + runtimeScript.language + ".zhlocalisation")));
			}

			//Adds to dictionary if it ain't a comment or empty
			foreach (string line in lines)
			{
				string coolLine = line.Trim(' ', '	');
				if (!coolLine.StartsWith("#") && coolLine != "")
				{
					this[coolLine.Substring(0, coolLine.IndexOf(':'))] = coolLine.Substring(coolLine.IndexOf(':') + 1);
				}
			}
		}

		public bool Contains(string key)
		{
			return content.ContainsKey(key);
		}

		public string GetReal(string key, List<(ScopeTypes, string)> scopes, Dictionary<string, object> passedVars )
		{
			string output = content[key];
			string[] elements = output.Split('$');
			for (int i = 0; i < elements.Length; i++)
			{
				if (i % 2 == 1)
				{
					string[] element = elements[i].Split('?');
					switch (element[0])
					{
						case "GetCountryName":
							elements[i] = GetReal(runtimeScript.countries[runtimeScript.GrabCountryScope(element[1], scopes, passedVars)].CosmeticKey(), scopes, passedVars);
							break;
						case "GetCountryNonideologyName":
							elements[i] = GetReal(runtimeScript.countries[runtimeScript.GrabCountryScope(element[1], scopes, passedVars)].CosmeticNonideology, scopes, passedVars);
							break;
						default:
							break;
					}
				}
			}
			return String.Join("", elements);
		}

		public String staticKey(string key)
		{
			return content[key];
		}

		public String GetString(string key, List<(ScopeTypes, string)> scopes, Dictionary<string, object> passedVars)
		{
			if (content.ContainsKey(key))
			{
				return GetReal(key, scopes, passedVars);
			}
			else
			{
				return key;
			}
		}

		public String this[string key]
		{
			set
			{
				content[key] = value;
			}
		}

		public List<string> Keys()
		{
			return new List<string>( content.Keys );
		}
	}
}
