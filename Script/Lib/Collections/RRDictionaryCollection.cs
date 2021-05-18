using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRCollections
{
    public class RRDictionnaryCollection
    {
        public Dictionary<string, RRDictionnaryItem> ELT_DICTIONARY;

        public RRDictionnaryItem GetEltData(string sId)
        {
            if (string.IsNullOrEmpty(sId))
            {
                return null;
            }
            RRDictionnaryItem elt = null;
            ELT_DICTIONARY.TryGetValue(sId, out elt);
            return elt;
        }

        public void Clean()
        {
            ELT_DICTIONARY.Clear();
        }

        #region initialisation
        public void InitEmpty()
        {
            ELT_DICTIONARY = new Dictionary<string, RRDictionnaryItem>();
        }

        public void Init(string sFileName)
        {
            ELT_DICTIONARY = new Dictionary<string, RRDictionnaryItem>();
            TextAsset textAsset = Resources.Load<TextAsset>(sFileName);
            try
            {
                AddXml(textAsset.text);
            }
            catch (System.SystemException e)
            {
                Debug.LogError("error in : " + sFileName + " on : " + e.Message);
            }
        }

        public void AddXml(string sXml)
        {
            lwXmlReader xr = new lwXmlReader();

            if (xr.Init(sXml))
            {
                ReadXmlReader(xr);
                xr.Close();
            }
        }

        public void ReadXmlReader(lwXmlReader xr)
        {
            while (xr.Read())
            {
                if (xr.IsNodeElement())
                {
                    if (xr.Name == "ITEM")
                    {
                        RRDictionnaryItem eltData = new RRDictionnaryItem(ref xr);
                        ELT_DICTIONARY.Add(eltData.GetId(), eltData);
                    }
                }
            }
        }

        public void ReadXmlNode(ref lwXmlReader xr, string sElemName, System.Func<lwXmlReader, RRDictionnaryItem> checkAndGenerateEltFunction)
        {
            ELT_DICTIONARY = new Dictionary<string, RRDictionnaryItem>();

            lwXmlReader subTree = xr.ReadSubtree();

            while (subTree.Read())
            {
                if (subTree.IsNodeElement())
                {
                    if (subTree.Name == sElemName)
                    {
                        RRDictionnaryItem eltData = checkAndGenerateEltFunction(subTree);
                        ELT_DICTIONARY.Add(eltData.GetId(), eltData);
                    }
                }
            }
        }

        #endregion
    }
}
