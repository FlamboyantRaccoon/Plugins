using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RRNarration
{
    [CreateAssetMenu(fileName = "Conversation", menuName = "ReRolled/Narration/Default/ConversationData", order = 1)]
    public class ConversationData : EltData
    {
        [System.Serializable]
        public class DiaItemPair
        {
            public string diaKey = "DiaKey";
            public DialogData diaData = null;

            public DiaItemPair()
            {
                Debug.Log("default construct ");
                diaKey = "";
                diaData = new DialogData();
                Debug.Log("dia pairs : " + diaData.m_pairs.Count);
            }

            public DiaItemPair(string k, DialogData v)
            {
                Debug.Log("with param ");
                diaKey = k;
                diaData = v;
            }
        }


        private readonly Dictionary<string, string> DefaultValue = new Dictionary<string, string>() { { "actionEventTime", "-1" }, { "persistent", "false" }, { "occurenceMax", "-1" } };

        [SerializeField]
        private List<DiaItemPair> m_dialog = new List<DiaItemPair>();
        //[SerializeField]
        //DialogData dialogData = new DialogData();

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
