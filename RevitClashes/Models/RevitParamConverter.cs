using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.CustomParams;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;

using pyRevitLabs.Json;
using pyRevitLabs.Json.Linq;

namespace RevitClashDetective.Models
{
    /// <summary>
    /// Class for adjusting serialization/deserialization of type <see cref="SystemParam"/> for adjusting property <see cref="SystemParam.StorageType"/>
    /// </summary>
    internal class RevitParamConverter : JsonConverter
    {
        private readonly Document _document;
        private readonly SystemParamsConfig _systemParamsConfig = SystemParamsConfig.Instance;
        private readonly SharedParamsConfig _sharedParamsConfig = SharedParamsConfig.Instance;
        private readonly ProjectParamsConfig _projectParamsConfig = ProjectParamsConfig.Instance;

        /// <summary>
        /// Type converter constructor <see cref="SystemParam"/> to adjust the assignment of the <see cref="SystemParam.StorageType"/> property
        /// </summary>
        /// <param name="document">The document in which the conversion was started</param>
        /// <exception cref="ArgumentNullException">Exception if the input parameter is a null reference</exception>
        public RevitParamConverter(Document document)
        {
            if(document is null)
            { throw new ArgumentNullException(nameof(document)); }

            _document = document;
        }

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RevitParam);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("The converter only supports the ReadJson method");
        }

        // property reader.Value returns the current value of the JSON token
        // method reader.Read() reads moves 1 token to the end of the JSON file
        // https://stackoverflow.com/questions/23017716/json-net-how-to-deserialize-without-using-the-default-constructor
        // https://stackoverflow.com/questions/20995865/deserializing-json-to-abstract-class
        // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-6-0#sample-factory-pattern-converter
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if(reader is null)
            { throw new ArgumentNullException(nameof(reader)); }

            JObject jobj = JObject.Load(reader);
            string typeName = jobj["$type"].Value<string>()?.Split(',').FirstOrDefault();
            if(string.IsNullOrWhiteSpace(typeName))
            {
                throw new JsonSerializationException($"Failed to get parameter type name");
            }

            if(typeName.Equals(typeof(SystemParam).FullName))
            {
                string id = jobj["Id"].Value<string>();
                if(string.IsNullOrWhiteSpace(id))
                { throw new JsonSerializationException($"Failed to get property {nameof(RevitParam.Id)}"); }
                BuiltInParameter builtInParameter = (BuiltInParameter) Enum.Parse(typeof(BuiltInParameter), id);
                SystemParam systemParam = _systemParamsConfig.CreateRevitParam(_document, builtInParameter);
                return systemParam;
            }
            else if(typeName.Equals(typeof(SharedParam).FullName))
            {
                string name = jobj["Name"].Value<string>();
                if(string.IsNullOrWhiteSpace(name))
                { throw new JsonSerializationException($"Failed to get property {nameof(RevitParam.Name)}"); }
                try
                {
                    return _sharedParamsConfig.CreateRevitParam(_document, name);
                }
                catch(ArgumentNullException)
                {
                    throw new JsonSerializationException($"In the document \'{_document.PathName}\' missing general parameter \'{name}\'");
                }
            }
            else if(typeName.Equals(typeof(ProjectParam).FullName))
            {
                string name = jobj["Name"].Value<string>();
                if(string.IsNullOrWhiteSpace(name))
                { throw new JsonSerializationException($"Failed to get property {nameof(RevitParam.Name)}"); }
                try
                {
                    return _projectParamsConfig.CreateRevitParam(_document, name);
                }
                catch(ArgumentNullException)
                {
                    throw new JsonSerializationException($"In the document \'{_document.PathName}\' missing project parameter \'{name}\'");
                }
            }
            else if(typeName.Equals(typeof(CustomParam).FullName))
            {
                return jobj.ToObject<CustomParam>();
            }
            else
            { throw new JsonSerializationException($"Unsupported parameter type name: {typeName}"); }
        }
    }
}