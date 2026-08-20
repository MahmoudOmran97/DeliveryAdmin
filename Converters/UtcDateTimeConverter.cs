using System.Text.Json;
using System.Text.Json.Serialization;
using DeliveryAdmin.Helpers;

namespace DeliveryAdmin.Converters
{
    /// <summary>
    /// ✅ متسجل مرة واحدة في JsonSerializerOptions المشتركة (ApiService._opts).
    /// أي DateTime جاي من الـ API بيتحول تلقائي لتوقيت مصر (صراحة، مش
    /// ToLocalTime لأن ده Server-side وتوقيت السيرفر نفسه هو المشكلة الأصلية).
    /// </summary>
    public class UtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetDateTime();
            return EgyptTimeZoneHelper.ToEgyptTime(value);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime());
        }
    }

    public class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var value = reader.GetDateTime();
            return EgyptTimeZoneHelper.ToEgyptTime(value);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToUniversalTime());
            else
                writer.WriteNullValue();
        }
    }
}
