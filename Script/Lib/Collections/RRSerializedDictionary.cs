using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRCollections
{
    public class RRSerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField, HideInInspector]
        private List<TKey> keyData = new List<TKey>();

        [SerializeField, HideInInspector]
        private List<TValue> valueData = new List<TValue>();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.Clear();
            for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++)
            {
                this[this.keyData[i]] = this.valueData[i];
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            this.keyData.Clear();
            this.valueData.Clear();

            foreach (var item in this)
            {
                this.keyData.Add(item.Key);
                this.valueData.Add(item.Value);
            }
        }
    }

    [Serializable]
    public class StringDictionary : RRSerializedDictionary<string, string> { }

    /*[Serializable]
    public class KeyCodeGameObjectListDictionary : RRSerializedDictionnary<KeyCode, List<GameObject>> { }

    [Serializable]
    public class StringScriptableObjectDictionary : RRSerializedDictionnary<string, ScriptableObject> { }*/

}
