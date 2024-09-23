using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Media.Imaging;

namespace Hackathon1
{

    public class AppDataItemConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // This converter is only for types derived from AppDataItem
            return typeof(AppDataItem).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            
            // If the token is a string, assume it's a StringData
            if (token["Location"]==null)
            {
                return new StringData(token["Value"].ToString());
            }

            // If the token is a byte array (base64 string), assume it's ImageData
            if (token["Location"] != null)
            {
               
                return new ImageData(token["Location"].ToString(),new BitmapImage(new Uri(token["Value"].ToString())));
            }

            throw new JsonSerializationException("Unknown data type");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Not needed unless you also want custom serialization logic
            throw new NotImplementedException();
        }
    }

}
