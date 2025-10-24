using LibElectronAppApi.OData;
using LibElectronAppApi.OData.FilterExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibApiTests;

[TestFixture]
public class ODataQueryParserTests
{
    [TestCaseSource(nameof(GetTestCases))]
    public void ParseFromUri_WhenInvoked_CorrectlyParsesQueryString(string queryString, ODataQueryOptions expected)
    { 
        var actual = ODataQueryParser.Parse(queryString);
        
        Console.WriteLine(queryString);
        Console.WriteLine(JsonConvert.SerializeObject(actual, Formatting.Indented));
        Console.WriteLine(JsonConvert.SerializeObject(actual));
        
        var expectedJson = JsonConvert.SerializeObject(expected);
        var actualJson = JsonConvert.SerializeObject(actual);
        Assert.That(actualJson, Is.EqualTo(expectedJson));
    }

    private static IEnumerable<object[]> GetTestCases()
    {
        yield return ["$filter=(department eq 'Sales' and revenue gt 100000) or (department eq 'Engineering' and projectCount gt 5)", GetExpected("{\"FilterRaw\":\"(department eq 'Sales' and revenue gt 100000) or (department eq 'Engineering' and projectCount gt 5)\",\"Filter\":{\"Left\":{\"Left\":{\"Left\":{\"PropertyName\":\"department\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"Equal\",\"Right\":{\"Value\":\"Sales\",\"Type\":\"String\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"Operator\":\"And\",\"Right\":{\"Left\":{\"PropertyName\":\"revenue\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"GreaterThan\",\"Right\":{\"Value\":100000,\"Type\":\"Number\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"TypeName\":\"BinaryExpression\"},\"Operator\":\"Or\",\"Right\":{\"Left\":{\"Left\":{\"PropertyName\":\"department\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"Equal\",\"Right\":{\"Value\":\"Engineering\",\"Type\":\"String\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"Operator\":\"And\",\"Right\":{\"Left\":{\"PropertyName\":\"projectCount\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"GreaterThan\",\"Right\":{\"Value\":5,\"Type\":\"Number\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"TypeName\":\"BinaryExpression\"},\"TypeName\":\"BinaryExpression\"},\"OrderBy\":[],\"Top\":null,\"Skip\":null,\"Count\":null,\"RawQuery\":\"$filter=(department eq 'Sales' and revenue gt 100000) or (department eq 'Engineering' and projectCount gt 5)\"}")];
        yield return ["$filter=age gt 25", GetExpected("{\"FilterRaw\":\"age gt 25\",\"Filter\":{\"Left\":{\"PropertyName\":\"age\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"GreaterThan\",\"Right\":{\"Value\":25,\"Type\":\"Number\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"OrderBy\":[],\"Top\":null,\"Skip\":null,\"Count\":null,\"RawQuery\":\"$filter=age gt 25\"}")];
        yield return ["$filter=name eq 'John' and (age gt 25 or status eq 'active')", GetExpected("{\"FilterRaw\":\"name eq 'John' and (age gt 25 or status eq 'active')\",\"Filter\":{\"Left\":{\"Left\":{\"PropertyName\":\"name\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"Equal\",\"Right\":{\"Value\":\"John\",\"Type\":\"String\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"Operator\":\"And\",\"Right\":{\"Left\":{\"Left\":{\"PropertyName\":\"age\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"GreaterThan\",\"Right\":{\"Value\":25,\"Type\":\"Number\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"Operator\":\"Or\",\"Right\":{\"Left\":{\"PropertyName\":\"status\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"Equal\",\"Right\":{\"Value\":\"active\",\"Type\":\"String\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"TypeName\":\"BinaryExpression\"},\"TypeName\":\"BinaryExpression\"},\"OrderBy\":[],\"Top\":null,\"Skip\":null,\"Count\":null,\"RawQuery\":\"$filter=name eq 'John' and (age gt 25 or status eq 'active')\"}")];
        yield return ["$filter=contains(name, 'Smith') and startswith(email, 'john')", GetExpected("{\"FilterRaw\":\"contains(name, 'Smith') and startswith(email, 'john')\",\"Filter\":{\"Left\":{\"FunctionName\":\"contains\",\"Arguments\":[{\"PropertyName\":\"name\",\"TypeName\":\"PropertyExpression\"},{\"Value\":\"Smith\",\"Type\":\"String\",\"TypeName\":\"LiteralExpression\"}],\"TypeName\":\"FunctionExpression\"},\"Operator\":\"And\",\"Right\":{\"FunctionName\":\"startswith\",\"Arguments\":[{\"PropertyName\":\"email\",\"TypeName\":\"PropertyExpression\"},{\"Value\":\"john\",\"Type\":\"String\",\"TypeName\":\"LiteralExpression\"}],\"TypeName\":\"FunctionExpression\"},\"TypeName\":\"BinaryExpression\"},\"OrderBy\":[],\"Top\":null,\"Skip\":null,\"Count\":null,\"RawQuery\":\"$filter=contains(name, 'Smith') and startswith(email, 'john')\"}")];
        yield return ["$filter=age ge 18 and age le 65 and status ne 'inactive'&$orderby=lastName, firstName desc&$top=50&$skip=100&$count=true", GetExpected("{\"FilterRaw\":\"age ge 18 and age le 65 and status ne 'inactive'\",\"Filter\":{\"Left\":{\"Left\":{\"Left\":{\"PropertyName\":\"age\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"GreaterThanOrEqual\",\"Right\":{\"Value\":18,\"Type\":\"Number\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"Operator\":\"And\",\"Right\":{\"Left\":{\"PropertyName\":\"age\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"LessThanOrEqual\",\"Right\":{\"Value\":65,\"Type\":\"Number\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"TypeName\":\"BinaryExpression\"},\"Operator\":\"And\",\"Right\":{\"Left\":{\"PropertyName\":\"status\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"NotEqual\",\"Right\":{\"Value\":\"inactive\",\"Type\":\"String\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"TypeName\":\"BinaryExpression\"},\"OrderBy\":[{\"Property\":\"lastName\",\"Direction\":\"Ascending\"},{\"Property\":\"firstName\",\"Direction\":\"Descending\"}],\"Top\":50,\"Skip\":100,\"Count\":true,\"RawQuery\":\"$filter=age ge 18 and age le 65 and status ne 'inactive'&$orderby=lastName, firstName desc&$top=50&$skip=100&$count=true\"}")];
        yield return ["$filter=status eq 'active'&$orderby=createdDate desc&$top=20&$skip=0", GetExpected("{\"FilterRaw\":\"status eq 'active'\",\"Filter\":{\"Left\":{\"PropertyName\":\"status\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"Equal\",\"Right\":{\"Value\":\"active\",\"Type\":\"String\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"OrderBy\":[{\"Property\":\"createdDate\",\"Direction\":\"Descending\"}],\"Top\":20,\"Skip\":0,\"Count\":null,\"RawQuery\":\"$filter=status eq 'active'&$orderby=createdDate desc&$top=20&$skip=0\"}")];
        yield return ["$filter=description eq 'It''s a test'", GetExpected("{\"FilterRaw\":\"description eq 'It''s a test'\",\"Filter\":{\"Left\":{\"PropertyName\":\"description\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"Equal\",\"Right\":{\"Value\":\"It\",\"Type\":\"String\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"OrderBy\":[],\"Top\":null,\"Skip\":null,\"Count\":null,\"RawQuery\":\"$filter=description eq 'It''s a test'\"}")];
        yield return ["$filter=price mul quantity gt 1000", GetExpected("{\"FilterRaw\":\"price mul quantity gt 1000\",\"Filter\":{\"Left\":{\"Left\":{\"PropertyName\":\"price\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"Multiply\",\"Right\":{\"PropertyName\":\"quantity\",\"TypeName\":\"PropertyExpression\"},\"TypeName\":\"BinaryExpression\"},\"Operator\":\"GreaterThan\",\"Right\":{\"Value\":1000,\"Type\":\"Number\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"OrderBy\":[],\"Top\":null,\"Skip\":null,\"Count\":null,\"RawQuery\":\"$filter=price mul quantity gt 1000\"}")];
        yield return ["$filter=not status eq 'deleted'", GetExpected("{\"FilterRaw\":\"not status eq 'deleted'\",\"Filter\":{\"Left\":{\"Operator\":\"Not\",\"Operand\":{\"PropertyName\":\"status\",\"TypeName\":\"PropertyExpression\"},\"TypeName\":\"UnaryExpression\"},\"Operator\":\"Equal\",\"Right\":{\"Value\":\"deleted\",\"Type\":\"String\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"OrderBy\":[],\"Top\":null,\"Skip\":null,\"Count\":null,\"RawQuery\":\"$filter=not status eq 'deleted'\"}")];
        yield return ["$filter=createdDate ge 2025-02-03T12:05:01.420Z", GetExpected("{\"FilterRaw\":\"createdDate ge 2025-02-03T12:05:01.420Z\",\"Filter\":{\"Left\":{\"PropertyName\":\"createdDate\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"GreaterThanOrEqual\",\"Right\":{\"Value\":\"2025-02-03T12:05:01.42Z\",\"Type\":\"DateTime\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"OrderBy\":[],\"Top\":null,\"Skip\":null,\"Count\":null,\"RawQuery\":\"$filter=createdDate ge 2025-02-03T12:05:01.420Z\"}")];
        yield return ["$filter=createdDate ge 2025-02-03t12:05:01z", GetExpected("{\"FilterRaw\":\"createdDate ge 2025-02-03t12:05:01z\",\"Filter\":{\"Left\":{\"PropertyName\":\"createdDate\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"GreaterThanOrEqual\",\"Right\":{\"Value\":\"2025-02-03T12:05:01Z\",\"Type\":\"DateTime\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"OrderBy\":[],\"Top\":null,\"Skip\":null,\"Count\":null,\"RawQuery\":\"$filter=createdDate ge 2025-02-03t12:05:01z\"}")];
        yield return ["$filter=status eq 'active'&$orderby=createdDate desc&$top=20&$skip=3&$count=true", GetExpected("{\"FilterRaw\":\"status eq 'active'\",\"Filter\":{\"Left\":{\"PropertyName\":\"status\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"Equal\",\"Right\":{\"Value\":\"active\",\"Type\":\"String\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"OrderBy\":[{\"Property\":\"createdDate\",\"Direction\":\"Descending\"}],\"Top\":20,\"Skip\":3,\"Count\":true,\"RawQuery\":\"$filter=status eq 'active'&$orderby=createdDate desc&$top=20&$skip=3&$count=true\"}")];
        yield return ["$filter=status eq 'active'&$orderby=createdDate desc&$top=20&$skip=3&$count=false", GetExpected("{\"FilterRaw\":\"status eq 'active'\",\"Filter\":{\"Left\":{\"PropertyName\":\"status\",\"TypeName\":\"PropertyExpression\"},\"Operator\":\"Equal\",\"Right\":{\"Value\":\"active\",\"Type\":\"String\",\"TypeName\":\"LiteralExpression\"},\"TypeName\":\"BinaryExpression\"},\"OrderBy\":[{\"Property\":\"createdDate\",\"Direction\":\"Descending\"}],\"Top\":20,\"Skip\":3,\"Count\":false,\"RawQuery\":\"$filter=status eq 'active'&$orderby=createdDate desc&$top=20&$skip=3&$count=false\"}")];
    }

    private static ODataQueryOptions GetExpected(string json)
    {
        return JsonConvert.DeserializeObject<ODataQueryOptions>(json,
                   new JsonSerializerSettings() { Converters = [new ODataQueryOptionsJsonConverter()] }) ??
               new ODataQueryOptions();
    }
}

public class ODataQueryOptionsJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        if (jo["TypeName"]?.Value<string>() == nameof(BinaryExpression))
        {
            return jo.ToObject<BinaryExpression>(serializer);
        }
        if (jo["TypeName"]?.Value<string>() == nameof(FunctionExpression))
        {
            return jo.ToObject<FunctionExpression>(serializer);
        }
        if (jo["TypeName"]?.Value<string>() == nameof(LiteralExpression))
        {
            return jo.ToObject<LiteralExpression>(serializer);
        }
        if (jo["TypeName"]?.Value<string>() == nameof(PropertyExpression))
        {
            return jo.ToObject<PropertyExpression>(serializer);
        }
        if (jo["TypeName"]?.Value<string>() == nameof(UnaryExpression))
        {
            return jo.ToObject<UnaryExpression>(serializer);
        }

        return null;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(FilterExpression);
    }
}