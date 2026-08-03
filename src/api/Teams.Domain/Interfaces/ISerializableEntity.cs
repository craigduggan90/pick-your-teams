namespace Teams.Domain.Interfaces;

/// <summary>Describes an entity which defines its own serialization method.</summary>
public interface ISerializableEntity
{
    /// <summary>Gets an anonymous, serializable object representing the entity.</summary>
    object AsSerializable();
}