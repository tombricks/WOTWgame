using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZhukovEngine.LocalBranch
{
	public class Console : MonoBehaviour
	{
		LocalBranch.RuntimeScript runtimeScript;
		public Button runButton;
		public TMP_InputField inputField;

		void Start()
		{
			runtimeScript = GameObject.Find("Runtime").GetComponent<LocalBranch.RuntimeScript>();
			runButton.onClick.AddListener(OnClick);
		}

		void OnClick()
		{
			runtimeScript.scripts["debug_console"] = runtimeScript.Abstract(inputField.text);
			runtimeScript.RunScript(runtimeScript.scripts["debug_console"], new Dictionary<string, object>(), new List<(ScopeTypes, string)>() { (ScopeTypes.Country, runtimeScript.playerTag), (ScopeTypes.Global, null) });
			inputField.text = "";
		}
	}
}