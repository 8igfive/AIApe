using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Buaa.AIBot.Utils
{
    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                DateTime.ParseExact
                    (
                        reader.GetString(),
                        "yyyy/MM/dd-HH:mm:ss", 
                        CultureInfo.InvariantCulture
                    );

        public override void Write(
            Utf8JsonWriter writer,
            DateTime dateTimeValue,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(
                    dateTimeValue.ToString
                        (
                            "yyyy/MM/dd-HH:mm:ss", 
                            CultureInfo.InvariantCulture
                        )
                    );
    }
}
