using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Interhaptics.ObjectSnapper.core
{
    [Serializable]
    public class SnappingDataDictionary : ISerializationCallbackReceiver
    {
        #region Structures
        [Serializable]
        private struct Datas
        {
            public string name;
            public SnappableActorData snappableActorPosData;
        }
        #endregion

        #region Variables
        [SerializeField] [HideInInspector] private string[] serializedData;
        
        private Dictionary<string, SnappableActorData> _SnappableActorDataDictionnary = new Dictionary<string, SnappableActorData>();
        #endregion

        #region Serialization process
        public void OnBeforeSerialize()
        {
            if (_SnappableActorDataDictionnary == null)
                return;

            string[] keys = _SnappableActorDataDictionnary.Keys.ToArray();
            serializedData = new string[keys.Length];

            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                MemoryStream memoryStream;

                for (int i = 0; i < keys.Length; i++)
                {
                    using (memoryStream = new MemoryStream())
                    {
                        Datas data = new Datas() { name = keys[i], snappableActorPosData = _SnappableActorDataDictionnary[keys[i]] };
                        memoryStream.Position = 0;
                        formatter.Serialize(memoryStream, data);
                        serializedData[i] = Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
            catch (SerializationException se) { Debug.Log(se); }
        }
        
        public void OnAfterDeserialize()
        {
            if (serializedData == null)
                return;

            _SnappableActorDataDictionnary.Clear();

            try{
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                MemoryStream memoryStream;

                for (int i = 0; i < serializedData.Length; i++)
                {
                    using (memoryStream = new MemoryStream(Convert.FromBase64String(serializedData[i])))
                    {
                        memoryStream.Position = 0;
                        Datas datas = (Datas)binaryFormatter.Deserialize(memoryStream);

                        if (!string.IsNullOrEmpty(datas.name) && !_SnappableActorDataDictionnary.ContainsKey(datas.name))
                            _SnappableActorDataDictionnary.Add(datas.name, datas.snappableActorPosData);
                    }
                }
            }
            catch(SerializationException se) { Debug.Log(se); }
        }
        #endregion

        #region Publics
        /// <summary>
        /// Add a SnappableActorData to the Dictionary.
        /// </summary>
        /// <param name="name">Key</param>
        /// <param name="snappableActorData">SnappableActorData</param>
        public void Add(string name, SnappableActorData snappableActorData)
        {
            if (string.IsNullOrEmpty(name) || _SnappableActorDataDictionnary.ContainsKey(name))
                return;

            _SnappableActorDataDictionnary.Add(name, snappableActorData);
        }
        
        /// <summary>
        /// Remove a SnappableActorData from the Dictionary
        /// </summary>
        /// <param name="name">Key</param>
        public void Remove(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            _SnappableActorDataDictionnary.Remove(name);
        }
        
        public bool TryGetSnappableActorData(string name, out SnappableActorData snappableActorData) { return _SnappableActorDataDictionnary.TryGetValue(name, out snappableActorData); }
        
        public bool Contains(string name) { return _SnappableActorDataDictionnary.ContainsKey(name); }

        /// <summary>
        /// Get all the keys contained in the dictionary.
        /// </summary>
        /// <returns>The dictionary's keys</returns>
        public string[] GetDatabaseKeys() { return _SnappableActorDataDictionnary.Keys.ToArray(); }
        #endregion
    }
}
