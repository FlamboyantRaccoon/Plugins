using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRNarration
{
    [CreateAssetMenu(fileName = "effectData", menuName = "ReRolled/Narration/Default/EffectData", order = 1)]
    public class EffectData : ScriptableObject
    {
        //public ConversationData m_dia = new ConversationData();
        public RRCollections.StringDictionary m_dia2 = new RRCollections.StringDictionary();
        public RRCollections.StringDictionary m_dia22 = new RRCollections.StringDictionary();

        public RRCollections.RRDictionnaryItemSerialized m_dia = new RRCollections.RRDictionnaryItemSerialized();

    }
}
