using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZhukovEngine.LocalBranch
{
	public class MapTile : MonoBehaviour
	{
        RuntimeScript runtimeScript;
		public Button mapButton;
		public int tileId;
		public string owner;
		public string tileName;
		public List<int> borders = new List<int>();
		void Start()
		{
			runtimeScript = GameObject.Find("Runtime").GetComponent<LocalBranch.RuntimeScript>();
			mapButton = this.GetComponent<Button>();
			mapButton.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1F;
			mapButton.onClick.AddListener(OnClick);
		}

		void OnClick()
		{
			runtimeScript.OpenDiplomacy(owner);
			Debug.Log("clicked on " + tileId.ToString());
		}

		public void SetOwner(string tag)
        {
			this.owner = tag;
		}

		public void SetColor(Color color)
        {
			this.gameObject.GetComponent<Image>().color = color;

		}
	}
}