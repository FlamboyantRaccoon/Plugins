using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRCollections
{
    [System.Serializable]
    public class RRDictionnaryItemSerialized
    {

        [System.Serializable]
        public class DictionnaryItemPair
        {
            public string key = "Key";
            public string value = "value";

            public DictionnaryItemPair(string k, string v)
            {
                key = k;
                value = v;
            }
        }

        public List<DictionnaryItemPair> m_pairs = new List<DictionnaryItemPair>();
        public Dictionary<string, string> m_params;
        
        public RRDictionnaryItemSerialized()
        {
            m_params = new StringDictionary();
        }

        public RRDictionnaryItemSerialized(ref lwXmlReader xr)
        {
            m_params = new StringDictionary();
            string[] key = xr.GetAttributeNames();
            for (int i = 0; i < key.Length; i++)
            {
                string sKey = key[i];
                string sParam = xr.GetAttribute(sKey);
                m_params.Add(sKey, sParam);
            }
        }

        public string GetId()
        {
            string sId = GetAttributeString("id");
            Debug.Assert(!string.IsNullOrEmpty(sId));
            return sId;
        }

        public int GetAttributeInt(string key, int nDefault = 0)
        {
            string sValue = GetValue(key);
            if (sValue == null)
            {
                return nDefault;
            }
            return System.Convert.ToInt32(sValue);
        }

        public bool GetAttributeBool(string key, bool bDefault = false)
        {
            string sValue = GetValue(key);
            if (sValue == null)
            {
                return bDefault;
            }
            return lwParseTools.ParseBoolSafe(sValue, bDefault);
        }

        public float GetAttributeFloat(string key, float fDefault = 0f)
        {
            string sValue = GetValue(key);
            if (sValue == null)
            {
                return fDefault;
            }
            return lwParseTools.ParseFloatSafe(sValue, fDefault);
        }

        public string GetAttributeString(string key, string sDefault = "")
        {
            string sValue = GetValue(key);
            if (sValue == null)
            {
                return sDefault;
            }
            return sValue;
        }

        public void SetAttribute(string key, string value)
        {
            string sValue = GetValue(key);
            if (sValue == null)
            {
                m_params.Add(key, value);
            }
            else
            {
                m_params[key] = value;
            }
        }

        public T GetEnumValue<T>(string key, T eDefault)
        {
            string sValue = GetValue(key); ;
            if (sValue == null)
            {
                return eDefault;
            }
            return lwParseTools.ParseEnumSafe<T>(sValue, eDefault);
        }

        private string GetValue(string sKey)
        {
            string sValue = null;
            if (m_params != null) m_params.TryGetValue(sKey, out sValue);
            return sValue;
        }

        // for editor passerelle
        internal void SetAttributeEditor(string key, string value)
        {
            int index = FindKey(key);
            if (index == -1)
            {
                m_pairs.Add(new DictionnaryItemPair(key, value));
            }
            else
            {
                m_pairs[index].value = value;
            }
        }

        private int FindKey( string key )
        {
            int index = -1;
            int searchIndex = 0;
            while( index==-1 && searchIndex < m_pairs.Count )
            {
                if( m_pairs[searchIndex].key == key )
                {
                    index = searchIndex;
                }
                else
                {
                    searchIndex++;
                }
            }
            return index;
        }

    }
}