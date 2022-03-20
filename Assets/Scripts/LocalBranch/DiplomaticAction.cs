using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZhukovEngine.LocalBranch
{
    public class DiplomaticAction : MonoBehaviour
    {
        public int diplomaticId;
        LocalBranch.RuntimeScript runtimeScript;
        public Button actionButton;

        void Start()
        {
            runtimeScript = GameObject.Find("Runtime").GetComponent<LocalBranch.RuntimeScript>();
            actionButton = this.GetComponent<Button>();
            actionButton.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            runtimeScript.RunDiplomaticAction(diplomaticId, runtimeScript.playerTag, runtimeScript.diploTag);
        }
    }
}