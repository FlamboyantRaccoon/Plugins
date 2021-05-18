using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRNarration
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "ReRolled/Narration/Default/CharacterData", order = 1)]
    public class CharacterData : EltData
    {
        private readonly Dictionary<string, string> DefaultValue = new Dictionary<string, string>() { { "prefabKey", "" } };

        protected override void SetDefaultValues()
        {
            base.SetDefaultValues();
            foreach (KeyValuePair<string, string> pair in DefaultValue)
            {
                SetAttribute(pair.Key, pair.Value);
            }
        }
    }

}
