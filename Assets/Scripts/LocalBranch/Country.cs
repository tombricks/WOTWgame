using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZhukovEngine.LocalBranch
{
    public class Country
    {
        LocalBranch.RuntimeScript runtimeScript;

        private string tag;
        private string cosmeticBase;
        private string cosmeticKey;
        private string cosmeticAdj;
        private string cosmeticFlag;
        private string cosmeticNonideology;
        private string cosmeticNonideologyAdj;
        private int rulingParty = 0;
        private float[] parties;
        public string[] partyNames;
        public string[] partyNamesLong;
        public List<int> tiles = new List<int>();
        public List<string> flags = new List<string>();
        public Color32 color = new Color32(128, 128, 128, 255);
        public Color32 baseColor = new Color32(128, 128, 128, 255);

        public Country(string tag)
        {
            runtimeScript = GameObject.Find("Runtime").GetComponent<LocalBranch.RuntimeScript>();
            this.tag = tag;
            this.cosmeticBase = tag;
            this.cosmeticKey = tag;
            this.cosmeticAdj = tag + "_adj";
            this.cosmeticNonideology = tag;
            this.cosmeticNonideologyAdj = tag + "_adj";
            if (runtimeScript.spriteImporter.Contains(this.tag + "_flag"))
                this.cosmeticFlag = this.tag;
            else
                this.cosmeticFlag = "ZZZ";
            parties = new float[runtimeScript.partyTypes.Count];
            partyNames = new string[runtimeScript.partyTypes.Count];
            partyNamesLong = new string[runtimeScript.partyTypes.Count];
            for (var i = 0; i < parties.Length; i++ )
            {
                parties[i] = runtimeScript.partyTypes.Count / 100;
                if (runtimeScript.localisation.Contains(tag + "_" + runtimeScript.partyTypes[i] + "_party"))
                {
                    partyNames[i] = tag + "_" + runtimeScript.partyTypes[i] + "_party";
                    partyNamesLong[i] = tag + "_" + runtimeScript.partyTypes[i] + "_party_long";
                }
                else
                {
                    partyNames[i] = runtimeScript.partyTypes[i] + "_party";
                    partyNamesLong[i] = runtimeScript.partyTypes[i] + "_party_long";
                }
            }
        }

        public string Tag { get => tag; }
        public string CosmeticNonideology { get => cosmeticNonideology; }
        public string CosmeticKey()
        {
            return cosmeticKey;
        }
        public string CosmeticFlag()
        {
            return cosmeticFlag;
        }
        public void SetCosmeticBase(string cosmetic = null)
        {
            if (cosmetic != null)
                this.cosmeticBase = cosmetic;

            if (runtimeScript.localisation.Contains(this.cosmeticBase + "_" + runtimeScript.partyTypes[rulingParty]))
                this.cosmeticKey = this.cosmeticBase + "_" + runtimeScript.partyTypes[rulingParty];
            else if (runtimeScript.localisation.Contains(this.tag + "_" + runtimeScript.partyTypes[rulingParty]))
                this.cosmeticKey = this.tag + "_" + runtimeScript.partyTypes[rulingParty];
            else if (runtimeScript.localisation.Contains(this.cosmeticBase))
                this.cosmeticKey = this.cosmeticBase;
            else
                this.cosmeticKey = this.tag;

            if (runtimeScript.localisation.Contains(this.cosmeticBase + "_" + runtimeScript.partyTypes[rulingParty] + "_adj"))
                this.cosmeticAdj = this.cosmeticBase + "_" + runtimeScript.partyTypes[rulingParty] + "_adj";
            else if (runtimeScript.localisation.Contains(this.tag + "_" + runtimeScript.partyTypes[rulingParty] + "_adj"))
                this.cosmeticAdj = this.tag + "_" + runtimeScript.partyTypes[rulingParty] + "_adj";
            else if (runtimeScript.localisation.Contains(this.cosmeticBase + "_adj"))
                this.cosmeticAdj = this.cosmeticBase + "_adj";
            else
                this.cosmeticAdj = this.tag + "_adj";

            if (runtimeScript.spriteImporter.Contains(this.cosmeticBase + "_" + runtimeScript.partyTypes[rulingParty] + "_flag"))
                this.cosmeticFlag = this.cosmeticBase + "_" + runtimeScript.partyTypes[rulingParty];
            else if (runtimeScript.spriteImporter.Contains(this.tag + "_" + runtimeScript.partyTypes[rulingParty] + "_flag"))
                this.cosmeticFlag = this.tag + "_" + runtimeScript.partyTypes[rulingParty];
            else if (runtimeScript.spriteImporter.Contains(this.cosmeticBase + "_flag"))
                this.cosmeticFlag = this.cosmeticBase;
            else if (runtimeScript.spriteImporter.Contains(this.tag + "_flag"))
                this.cosmeticFlag = this.tag;
            else
                this.cosmeticFlag = "ZZZ";

            runtimeScript.UpdateUI(this.tag);
        }
        public int GetRulingParty()
        {
            return this.rulingParty;
        }
        public void SetRulingParty(int newParty)
        {
            int oldParty = this.rulingParty;
            this.rulingParty = newParty;
            SetCosmeticBase();

            if (runtimeScript.scripts.ContainsKey("ruling_party_change_" + this.tag))
                runtimeScript.RunScript(runtimeScript.scripts["ruling_party_change_" + this.tag], new Dictionary<string, object>() { { "old_party", oldParty }, { "new_party", newParty } });

            runtimeScript.UpdateUI(this.tag);
        }
        public void SetTileColors(Color color)
        {
            foreach (int tile in tiles)
            {
                runtimeScript.mapTiles[tile].
    gameObject.GetComponent<MapTile>().SetColor(color);
            }
        }
    }
}