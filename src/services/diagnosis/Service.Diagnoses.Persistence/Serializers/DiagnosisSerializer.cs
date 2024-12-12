using MongoDB.Bson.Serialization;
using Service.Diagnoses.Domain.Entities;
using Value.Objects.Helper.BsonSerializer.Mongo;

namespace Service.Diagnoses.Persistence.Serializers;

internal class DiagnosisSerializer
{
    public static void RegisterMappings()
    {
        BsonClassMap.RegisterClassMap<Diagnosis>(cm =>
        {
            cm.AutoMap();

            cm.SetIdMember(
                cm.GetMemberMap(c => c.Id)
                  .SetSerializer(new GuidObjectSerializer()));

            cm.MapMember(c => c.DoctorId)
              .SetSerializer(new GuidObjectSerializer());

            cm.MapMember(c => c.PatientId)
              .SetSerializer(new GuidObjectSerializer());

            cm.MapMember(c => c.Disease)
              .SetSerializer(new StringObjectSerializer());

            cm.MapMember(c => c.Medicine)
              .SetSerializer(new StringObjectSerializer());

            cm.MapMember(c => c.Indications)
              .SetSerializer(new StringObjectSerializer());
        });
    }
}
