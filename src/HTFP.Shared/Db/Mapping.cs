namespace HTFP.Shared.Models;


public sealed class Mappings
{
    public static void RegisterMappings()
    {
        MongoDB.Bson.Serialization.BsonClassMap.RegisterClassMap<SubFile>(cm =>
        {
            cm.AutoMap();
            cm.MapField(f => f._statusRecords).SetElementName("StatusRecords");
            cm.UnmapProperty(f => f.StatusRecords);
        });

        MongoDB.Bson.Serialization.BsonClassMap.RegisterClassMap<ReconciliationFile>(cm =>
        {
            cm.AutoMap();
            cm.MapField(f => f._statusRecords).SetElementName("StatusRecords");
            cm.UnmapProperty(f => f.StatusRecords);
        });
    }
}