namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Serialisation.PHP;

/// <summary>
///     Tests To Verify That <see cref="PHPPropertyAttribute"/> Works Correctly As A Drop-In Replacement For <see cref="PhpPropertyAttribute"/>
/// </summary>
public sealed class PHPPropertyAttributeParityTests
{
    [Test]
    public async Task Serialisation_And_Deserialisation_With_Standard_Attribute_Produces_Expected_Results()
    {
        TestClassWithPhpProperty @object = new ()
        {
            PropertyOne = "one",
            PropertyTwo = 2,
            PropertyThree = "three",
            PropertyFour = 4
        };

        string serialisedData = PhpSerialization.Serialize(@object);

        const string expectedSerialisationOutput = @"a:4:{s:3:""one"";s:3:""one"";s:3:""two"";i:2;i:3;s:5:""three"";i:4;i:4;}";

        await Assert.That(serialisedData).IsEqualTo(expectedSerialisationOutput);

        if (PhpSerialization.Deserialize(serialisedData) is not IDictionary deserialisedData)
        {
            Assert.Fail("Deserialised Data Is NULL");
        }

        else
        {
            using (Assert.Multiple())
            {
                await Assert.That(deserialisedData).IsNotNull();
                await Assert.That(deserialisedData["one"]).IsEqualTo("one");
                await Assert.That(deserialisedData["two"]).IsEqualTo(2);
                await Assert.That(deserialisedData[3]).IsEqualTo("three");
                await Assert.That(deserialisedData[4]).IsEqualTo(4);
            }
        }
    }

    [Test]
    public async Task Serialisation_And_Deserialisation_With_Custom_Attribute_Produces_Expected_Results()
    {
        TestClassWithPHPProperty @object = new ()
        {
            PropertyOne = "one",
            PropertyTwo = 2,
            PropertyThree = "three",
            PropertyFour = 4
        };

        string serialisedData = PhpSerialization.Serialize(@object);

        const string expectedSerialisationOutput = @"a:4:{s:3:""one"";s:3:""one"";s:3:""two"";i:2;i:3;s:5:""three"";i:4;i:4;}";

        await Assert.That(serialisedData).IsEqualTo(expectedSerialisationOutput);

        if (PhpSerialization.Deserialize(serialisedData) is not IDictionary deserialisedData)
        {
            Assert.Fail("Deserialised Data Is NULL");
        }

        else
        {
            using (Assert.Multiple())
            {
                await Assert.That(deserialisedData).IsNotNull();
                await Assert.That(deserialisedData["one"]).IsEqualTo("one");
                await Assert.That(deserialisedData["two"]).IsEqualTo(2);
                await Assert.That(deserialisedData[3]).IsEqualTo("three");
                await Assert.That(deserialisedData[4]).IsEqualTo(4);
            }
        }
    }

    [Test]
    public async Task PHP_Serialisation_Output_Is_Identical_Between_Standard_And_Custom_Attribute()
    {
        TestClassWithPhpProperty standardObject = new ()
        {
            PropertyOne = "one",
            PropertyTwo = 2,
            PropertyThree = "three",
            PropertyFour = 4
        };

        TestClassWithPHPProperty customObject = new ()
        {
            PropertyOne = "one",
            PropertyTwo = 2,
            PropertyThree = "three",
            PropertyFour = 4
        };

        string standardSerialised = PhpSerialization.Serialize(standardObject);
        string customSerialised = PhpSerialization.Serialize(customObject);

        const string expectedSerialisationOutput = @"a:4:{s:3:""one"";s:3:""one"";s:3:""two"";i:2;i:3;s:5:""three"";i:4;i:4;}";

        using (Assert.Multiple())
        {
            await Assert.That(standardSerialised).IsEqualTo(expectedSerialisationOutput);
            await Assert.That(customSerialised).IsEqualTo(expectedSerialisationOutput);
        }
    }

    [Test]
    public async Task NULL_Values_Are_Correctly_Serialised_To_PHP_For_Both_Standard_And_Custom_Attribute()
    {
        TestClassWithPhpProperty standardObject = new ()
        {
            PropertyOne = null!,
            PropertyTwo = 2,
            PropertyThree = null!,
            PropertyFour = 4
        };

        TestClassWithPHPProperty customObject = new ()
        {
            PropertyOne = null!,
            PropertyTwo = 2,
            PropertyThree = null!,
            PropertyFour = 4
        };

        string standardSerialised = PhpSerialization.Serialize(standardObject);
        string customSerialised = PhpSerialization.Serialize(customObject);

        const string expectedSerialisationOutput = @"a:4:{s:3:""one"";N;s:3:""two"";i:2;i:3;N;i:4;i:4;}";

        using (Assert.Multiple())
        {
            await Assert.That(standardSerialised).IsEqualTo(expectedSerialisationOutput);
            await Assert.That(customSerialised).IsEqualTo(expectedSerialisationOutput);
        }
    }
}

/// <summary>
///     Test Class Using Standard <see cref="PhpPropertyAttribute"/> Attribute
/// </summary>
file class TestClassWithPhpProperty
{
    [PhpProperty("one")]
    public required string PropertyOne { get; init; }

    [PhpProperty("two")]
    public required int PropertyTwo { get; init; }

    [PhpProperty(3)]
    public required string PropertyThree { get; init; }

    [PhpProperty(4)]
    public required int PropertyFour { get; init; }
}

/// <summary>
///     Test Class Using Custom <see cref="PHPPropertyAttribute"/> Attribute
/// </summary>
file class TestClassWithPHPProperty
{
    [PHPProperty("one")]
    public required string PropertyOne { get; init; }

    [PHPProperty("two")]
    public required int PropertyTwo { get; init; }

    [PHPProperty(3)]
    public required string PropertyThree { get; init; }

    [PHPProperty(4)]
    public required int PropertyFour { get; init; }
}
