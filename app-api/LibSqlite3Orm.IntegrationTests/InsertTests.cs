using LibSqlite3Orm.IntegrationTests.TestDataModel;

namespace LibSqlite3Orm.IntegrationTests;

[TestFixture]
public class InsertTests : IntegrationTestBase<TestDbContext>
{
    [Test]
    public void Insert_WhenMaxFieldValues_RecordStoredAccurately()
    {
        var entity = CreateTestEntityMasterWithMaxValues();
        
        Assert.That(Orm.Insert(entity), Is.True);
        Assert.That(entity.Id, Is.EqualTo(1));

        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(entity, actual);
    }
    
    [Test]
    public void Insert_WhenMinFieldValues_RecordStoredAccurately()
    {
        var entity = CreateTestEntityMasterWithMinValues();
        
        Assert.That(Orm.Insert(entity), Is.True);
        Assert.That(entity.Id, Is.EqualTo(1));

        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(entity, actual);
    } 
    
    [Test]
    public void Insert_WhenRandomFieldValues_RecordStoredAccurately()
    {
        var entity = CreateTestEntityMasterWithRandomValues();
        
        Assert.That(Orm.Insert(entity), Is.True);
        Assert.That(entity.Id, Is.EqualTo(1));

        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(entity, actual);
    }  
}