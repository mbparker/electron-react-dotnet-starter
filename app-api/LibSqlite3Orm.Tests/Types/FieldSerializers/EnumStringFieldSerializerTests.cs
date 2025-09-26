using LibSqlite3Orm.Types.FieldSerializers;

namespace LibSqlite3Orm.Tests.Types.FieldSerializers;

[TestFixture]
public class EnumStringFieldSerializerTests
{
    private enum TestEnum
    {
        Value1,
        Value2,
        ValueWithSpecialName
    }

    private EnumStringFieldSerializer _serializer;

    [SetUp]
    public void SetUp()
    {
        _serializer = new EnumStringFieldSerializer(typeof(TestEnum));
    }

    [Test]
    public void Constructor_WithValidEnumType_SetsEnumType()
    {
        // Assert
        Assert.That(_serializer.EnumType, Is.EqualTo(typeof(TestEnum)));
        Assert.That(_serializer.RuntimeType, Is.EqualTo(typeof(TestEnum)));
    }

    [Test]
    public void SerializedType_ReturnsStringType()
    {
        // Act & Assert
        Assert.That(_serializer.SerializedType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void Serialize_WithEnumValue_ReturnsEnumNameString()
    {
        // Act
        var result = _serializer.Serialize(TestEnum.Value1);

        // Assert
        Assert.That(result, Is.EqualTo("Value1"));
        Assert.That(result, Is.TypeOf<string>());
    }

    [Test]
    public void Serialize_WithDifferentEnumValue_ReturnsCorrectName()
    {
        // Act
        var result = _serializer.Serialize(TestEnum.ValueWithSpecialName);

        // Assert
        Assert.That(result, Is.EqualTo("ValueWithSpecialName"));
    }

    [Test]
    public void Deserialize_WithValidEnumName_ReturnsEnumValue()
    {
        // Act
        var result = _serializer.Deserialize("Value1");

        // Assert
        Assert.That(result, Is.EqualTo(TestEnum.Value1));
        Assert.That(result, Is.TypeOf<TestEnum>());
    }

    [Test]
    public void Deserialize_WithCaseInsensitiveName_ReturnsEnumValue()
    {
        // Act
        var result = _serializer.Deserialize("value1");

        // Assert
        Assert.That(result, Is.EqualTo(TestEnum.Value1));
    }

    [Test]
    public void Deserialize_WithUpperCaseName_ReturnsEnumValue()
    {
        // Act
        var result = _serializer.Deserialize("VALUE1");

        // Assert
        Assert.That(result, Is.EqualTo(TestEnum.Value1));
    }

    [Test]
    public void Deserialize_WithInvalidEnumName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _serializer.Deserialize("InvalidValue"));
    }

    [Test]
    public void Deserialize_WithNullString_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _serializer.Deserialize(null));
    }

    [Test]
    public void Deserialize_WithEmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _serializer.Deserialize(""));
    }

    [Test]
    public void SerializeDeserialize_RoundTrip_PreservesValue()
    {
        // Arrange
        var enumValues = Enum.GetValues<TestEnum>();

        foreach (var originalValue in enumValues)
        {
            // Act
            var serialized = _serializer.Serialize(originalValue);
            var deserialized = _serializer.Deserialize(serialized);

            // Assert
            Assert.That(deserialized, Is.EqualTo(originalValue), $"Failed for value {originalValue}");
        }
    }

    [Test]
    public void Constructor_WithNullType_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EnumStringFieldSerializer(null));
    }

    [Test]
    public void Constructor_WithNonEnumType_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EnumStringFieldSerializer(typeof(string)));
    }

    [Test]
    public void Serialize_WithInvalidType_ThrowsException()
    {
        // Act & Assert  
        Assert.Throws<ArgumentException>(() => _serializer.Serialize("not an enum"));
    }

    // Test with a different enum type
    private enum AnotherEnum
    {
        First = 1,
        Second = 2
    }

    [Test]
    public void DifferentEnumType_WorksCorrectly()
    {
        // Arrange
        var anotherSerializer = new EnumStringFieldSerializer(typeof(AnotherEnum));

        // Act
        var serialized = anotherSerializer.Serialize(AnotherEnum.First);
        var deserialized = anotherSerializer.Deserialize("First");

        // Assert
        Assert.That(serialized, Is.EqualTo("First"));
        Assert.That(deserialized, Is.EqualTo(AnotherEnum.First));
        Assert.That(anotherSerializer.EnumType, Is.EqualTo(typeof(AnotherEnum)));
    }
}