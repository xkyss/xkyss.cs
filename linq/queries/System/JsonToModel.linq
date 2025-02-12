<Query Kind="Statements">
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
</Query>

// Json 转换为 Model
using System.Dynamic;

var jsonString = """
{
	"Username": "appstore",
	"HostIp": "192.168.1.144",
	"BaseIp": "192.168.1.144",
	"HostShort": "144",
	"WorkDir": "/home/Ml-Cache",
	"Version": "1.1.7",
	"Platform": "amd64"
}
""";
var serializerOptions = new JsonSerializerOptions
{
	Converters = { new DynamicJsonConverter() },
	WriteIndented = true
};

dynamic obj = JsonSerializer.Deserialize<dynamic>(jsonString, serializerOptions);

LINQPad.Extensions.Dump(obj, "將 JSON 反序列化成 dynamic 物件");

LINQPad.Extensions.Dump(obj.Username);
LINQPad.Extensions.Dump(obj.HostIp);
LINQPad.Extensions.Dump(obj.Platform);

string json = JsonSerializer.Serialize(obj, serializerOptions);

LINQPad.Extensions.Dump(json, "將 dynamic 物件序列化成 JSON 字串");

var node = JsonNode.Parse(jsonString);
node.Dump();
//node.ToJsonString().Dump();

//System.Console.WriteLine(node["Username"]);

public class DynamicJsonConverter : JsonConverter<dynamic>
{
	public override dynamic Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{

		if (reader.TokenType == JsonTokenType.True)
		{
			return true;
		}

		if (reader.TokenType == JsonTokenType.False)
		{
			return false;
		}

		if (reader.TokenType == JsonTokenType.Number)
		{
			if (reader.TryGetInt64(out long l))
			{
				return l;
			}

			return reader.GetDouble();
		}

		if (reader.TokenType == JsonTokenType.String)
		{
			if (reader.TryGetDateTime(out DateTime datetime))
			{
				return datetime;
			}

			return reader.GetString();
		}

		if (reader.TokenType == JsonTokenType.StartObject)
		{
			using JsonDocument documentV = JsonDocument.ParseValue(ref reader);
			return ReadObject(documentV.RootElement);
		}

		if (reader.TokenType == JsonTokenType.StartArray)
		{
			using JsonDocument documentV = JsonDocument.ParseValue(ref reader);
			return ReadList(documentV.RootElement);
		}

		using JsonDocument document = JsonDocument.ParseValue(ref reader);
		return document.RootElement.Clone();
	}

	private object ReadObject(JsonElement jsonElement)
	{
		IDictionary<string, object> expandoObject = new ExpandoObject();
		foreach (var obj in jsonElement.EnumerateObject())
		{
			var k = obj.Name;
			var value = ReadValue(obj.Value);
			expandoObject[k] = value;
		}
		return expandoObject;
	}

	private object ReadValue(JsonElement jsonElement)
	{
		object result = null;
		switch (jsonElement.ValueKind)
		{
			case JsonValueKind.Object:
				result = ReadObject(jsonElement);
				break;
			case JsonValueKind.Array:
				result = ReadList(jsonElement);
				break;
			case JsonValueKind.String:
				result = jsonElement.GetString();
				break;
			case JsonValueKind.Number:
				if (jsonElement.TryGetDecimal(out decimal d))
				{
					result = d;
				}
				else if (jsonElement.TryGetInt64(out long l))
				{
					result = l;
				}
				else
				{
					result = 0;
				}
				break;
			case JsonValueKind.True:
				result = true;
				break;
			case JsonValueKind.False:
				result = false;
				break;
			case JsonValueKind.Undefined:
			case JsonValueKind.Null:
				result = null;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		return result;
	}

	private object ReadList(JsonElement jsonElement)
	{
		IList<object> list = new List<object>();
		foreach (var item in jsonElement.EnumerateArray())
		{
			list.Add(ReadValue(item));
		}
		return list.Count == 0 ? null : list;
	}
	
	public override void Write(Utf8JsonWriter writer, dynamic value, JsonSerializerOptions options)
	{
		LINQPad.Extensions.Dump(value, "Write Value");

		// https://docs.microsoft.com/en-us/dotnet/api/system.typecode
		switch (Type.GetTypeCode((Type)value.GetType()))
		{
			case TypeCode.Boolean:
				writer.WriteBooleanValue(Convert.ToBoolean(value));
				break;
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
				writer.WriteNumberValue(Convert.ToInt64(value));
				break;
			case TypeCode.Decimal:
				writer.WriteNumberValue(Convert.ToDecimal(value));
				break;
			case TypeCode.Char:
			case TypeCode.Empty:
			case TypeCode.String:
				writer.WriteStringValue(Convert.ToString(value));
				break;
			case TypeCode.DateTime:
				writer.WriteStringValue(Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss"));
				break;
			case TypeCode.DBNull:
				writer.WriteNullValue();
				break;
			default:
				writer.WriteRawValue(JsonSerializer.Serialize(value, new JsonSerializerOptions() { WriteIndented = true }));
				break;
		}
	}
}
