using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assets.Scripts.Base
{
    #region JSONParser

    public abstract class JSONParser<T> 
    {
        #region public methods

        public IEnumerable<T> GetConfigurations(string fileName)
        {
            using (var fileReader = new StreamReader("Config/{0}.json".FormatWith(fileName)))
            {
                var fileAsString = fileReader.ReadToEnd();
                var items = Json.Deserialize(fileAsString) as IEnumerable<object>;
                var itemsAsDictionaries = items.Select(item => item as Dictionary<string, object>);
                return itemsAsDictionaries.Select(item => ConvertToObject(item)).Materialize();
            }
        }

        #endregion

        #region private methods

        private ValType TryGetValue<ValType>(Dictionary<string, object> dict, string propertyName, bool failIfNotFound)
        {
            object value = null;
            if(!dict.TryGetValue(propertyName, out value))
            {
                if(failIfNotFound)
                {
                    throw new ValueNotFoundException(propertyName, typeof(T));
                }
                return default(ValType);
            }

            if(!(value is ValType))
            {
                throw new WrongValueType(propertyName, typeof(ValType), value.GetType());
            }

            return (ValType)value;
        }

        protected ValType TryGetValue<ValType>(Dictionary<string, object> dict, string propertyName)
        {
            return TryGetValue<ValType>(dict, propertyName, true);
        }

        protected ValType TryGetValue<ValType>(Dictionary<string, object> dict, string propertyName, ValType defaultValue)
        {
            var initialResult = TryGetValue<ValType>(dict, propertyName, false);
            if (EqualityComparer<ValType>.Default.Equals(initialResult, default(ValType)))
            {
                initialResult = defaultValue;
            }
            return initialResult;
        }

        protected abstract T ConvertToObject(Dictionary<string, object> item);

        #endregion
    }

    #endregion
}
