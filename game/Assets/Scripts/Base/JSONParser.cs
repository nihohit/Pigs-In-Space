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

        #endregion public methods

        #region private methods

        private bool TryGetValue<ValType>(Dictionary<string, object> dict, string propertyName, out ValType result)
        {
            result = default(ValType);
            object value = null;
            if (!dict.TryGetValue(propertyName, out value))
            {
                return false;
            }

            if (!(value is ValType))
            {
                throw new WrongValueType(propertyName, typeof(ValType), value.GetType());
            }

            result = (ValType)value;
            return true;
        }

        protected ValType TryGetValueAndFail<ValType>(Dictionary<string, object> dict, string propertyName)
        {
            ValType result;
            if (!TryGetValue<ValType>(dict, propertyName, out result))
            {
                throw new ValueNotFoundException(propertyName, typeof(T));
            }
            return result;
        }

        protected ValType TryGetValueOrSetDefaultValue<ValType>
            (Dictionary<string, object> dict, string propertyName, ValType defaultValue)
        {
            ValType result;
            if (!TryGetValue<ValType>(dict, propertyName, out result))
            {
                result = defaultValue;
            }
            return result;
        }

        protected abstract T ConvertToObject(Dictionary<string, object> item);

        #endregion private methods
    }

    #endregion JSONParser
}