using LibSqlite3Orm.IntegrationTests.TestDataModel;
using LibSqlite3Orm.PInvoke.Types.Exceptions;

namespace LibSqlite3Orm.IntegrationTests;

[TestFixture]
public class UpdateTests : IntegrationTestSeededBase<TestDbContext>
{
    [Test]
    public void Update_WhenMasterValuesChange_TheyAreStoredAccurately()
    {
        var entity = CreateTestEntityMasterWithRandomValues();
        entity.Id = SeededMasterRecords[1].Id;
        var ret = Orm.Update(entity);

        Assert.That(ret, Is.True);

        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(entity, actual);
    }
    
    [Test]
    public void Update_WhenTagValuesChange_TheyAreStoredAccurately()
    {
        var entity = CreateTestEntityTagWithRandomValues();
        entity.Id = SeededTagRecords[1].Id;
        var ret = Orm.Update(entity);

        Assert.That(ret, Is.True);
        
        var actual = Orm
            .Get<TestEntityTag>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(entity, actual);
    }

    [Test]
    public void Update_WhenLinkedTagIdChanges_ItIsStoredAccurately()
    {
        var linkEntity = SeededLinkRecords.Values.First();
        var usedTagIds = SeededLinkRecords.Where(x => x.Value.EntityId == linkEntity.EntityId)
            .Select(x => x.Value.TagId).ToArray();
        var availableTagId = SeededTagRecords.Keys.First(x => !usedTagIds.Contains(x));
        
        linkEntity.TagId = availableTagId;
        var ret = Orm.Update(linkEntity);

        Assert.That(ret, Is.True);
        
        var actual = Orm
            .Get<TestEntityTagLink>(loadNavigationProps: true)
            .Where(x => x.Id == linkEntity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(linkEntity, actual);
        AssertThatRecordsMatch(SeededTagRecords[linkEntity.TagId], actual.Tag.Value);
        AssertThatRecordsMatch(SeededMasterRecords[linkEntity.EntityId], actual.Entity.Value);
    }
    
    [Test]
    public void Update_WhenTagValueViolatesConstraint_Throws()
    {
        var entity = SeededTagRecords[1];
        
        entity.TagValue = SeededTagRecords[2].TagValue;
        
        var ex = Assert.Throws<SqliteException>(() => Orm.Update(entity));
        Assert.That(ex?.Message, Is.EqualTo("UNIQUE constraint failed: TestEntityTag.TagValue"));
    }
    
    [Test]
    public void Update_WhenTagDoesNotExist_ReturnsFalse()
    {
        var entity = SeededTagRecords[1];
        
        entity.TagValue = SeededTagRecords[2].TagValue;
        entity.Id = 0;

        var ret = Orm.Update(entity);
        
        Assert.That(ret, Is.False);
    }
}