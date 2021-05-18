using System.Collections;
using System.Collections.Generic;
using RRCollections;
using UnityEngine;

namespace RRNarration
{
    [System.Serializable]
    public class DialogData: RRCollections.RRDictionnaryItemSerialized
    {
        private readonly Dictionary<string, string> DefaultValue = new Dictionary<string, string>() {
            { "id", "myDia" },
            { "txtId", "txtId" },
            { "goto", "gotoId" },
            { "charId", "characterId" }
        };

        public int test = 0;

        public DialogData()
        {
            Debug.Log("DialogData Constructor");
            test = 1;
            SetDefaultValues();
        }

        void SetDefaultValues()
        {
            foreach (KeyValuePair<string, string> pair in DefaultValue)
            {
                SetAttributeEditor(pair.Key, pair.Value);
                Debug.Log("SetAttributeEditor " + pair.Key + " : " + pair.Value);
            }
        }
    }

}
