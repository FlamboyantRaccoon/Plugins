using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRNarration
{
    public class EltData : RRCollections.RRDictionnaryItemScriptable
    {
        private readonly Dictionary<string, string> DefaultValue = new Dictionary<string, string>(){ { "id", "myId"}, { "txtId", "txtId" }, { "descTxtId", "descTxtId" } };
        
        public EltData()
        {
            SetDefaultValues();
        }

        protected virtual void SetDefaultValues()
        {
            foreach( KeyValuePair<string, string> pair in DefaultValue )
            {
                SetAttribute(pair.Key, pair.Value);
            }
        }
    }
}
