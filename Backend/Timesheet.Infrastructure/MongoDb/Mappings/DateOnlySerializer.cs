using System.Globalization;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Timesheet.Infrastructure.MongoDb.Mappings;

public sealed class DateOnlySerializer : SerializerBase<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var str = context.Reader.ReadString();
        return DateOnly.ParseExact(str, Format, CultureInfo.InvariantCulture);
    }

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        DateOnly value)
    {
        context.Writer.WriteString(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
