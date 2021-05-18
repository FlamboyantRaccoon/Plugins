using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRCollections
{
    
    public class RRDictionnaryItemScriptable : ScriptableObject
    {

        public StringDictionary m_params;

        public RRDictionnaryItemScriptable()
        {
            m_params = new StringDictionary();
        }

        public RRDictionnaryItemScriptable(ref lwXmlReader xr)
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


    }
}