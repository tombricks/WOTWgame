using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZhukovEngine.LocalBranch
{
    public class MapModeButton : MonoBehaviour
    {
        public int mapmodeId;
        LocalBranch.RuntimeScript runtimeScript;
        public Button mapmodeButton;

        void Start()
        {
            runtimeScript = GameObject.Find("Runtime").GetComponent<LocalBranch.RuntimeScript>();
            mapmodeButton = this.GetComponent<Button>();
            mapmodeButton.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            runtimeScript.UpdateMap(mapmodeId);
        }
    }
}