namespace Assets.Scripts.Base.JsonParsing
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEngine;

    public sealed class JsonParser<T>
    {
        private Dictionary<string, object> m_currentDictionary;

        #region public methods

        // Read the configuration file and load the configurations
        public IEnumerable<T> GetConfigurations(string fileName)
        {
            string filePath = fileName.Replace(".json", "");
            Debug.Log(filePath);
            TextAsset targetFile = Resources.Load<TextAsset>(filePath);
            var fileAsString = targetFile.text;
            var items = Json.Deserialize(fileAsString).SafeCast<IEnumerable<object>>("items");
            var itemsAsDictionaries = items.Select(item => item as Dictionary<string, object>);
            return itemsAsDictionaries.Select(item => this.ConvertToObject(item)).Materialize();
        }

        public T ConvertToObject(Dictionary<string, object> item)
        {
            this.m_currentDictionary = item;
            return this.ConvertCurrentItemToObject();
        }

        #endregion public methods

        #region private methods

        private T ConvertCurrentItemToObject()
        {
            return ObjectConstructor.ParseObject<T>(this.m_currentDictionary);
        }

        #endregion private methods
    }
}