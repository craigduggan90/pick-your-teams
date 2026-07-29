using Teams.Common.Extensions;
using Teams.Common.Providers.Identifiers;
using Teams.Common.Providers.Temporal;
using System.Reflection;
using Teams.Domain.Exceptions;
using Teams.Domain.Interfaces;

namespace Teams.Domain.Entities.Abstract;

/// <summary>Base class for service-owned entities.</summary>
public abstract class EntityBase : IHasCursor, IHasCreatedTimestamp, IHasModifiedTimestamp, ISerializableEntity
{
    /// <summary>Initializes a new instance of the <see cref="EntityBase"/> class.</summary>
    protected EntityBase()
    {
        Id = IdentifierProvider.Generate;
        var created = DateTimeOffsetProvider.Now;
        DateCreated = created.UtcDateTime;
        DateModified = created.UtcDateTime;
        Cursor = (long)(created - DateTimeOffset.UnixEpoch).TotalMicroseconds;
    }

    /// <summary>The unique identifier of the entity.</summary>
    public string Id { get; }

    /// <inheritdoc />
    public long Cursor { get; }

    /// <inheritdoc />
    public DateTime DateCreated { get; }

    /// <inheritdoc />
    public DateTime DateModified { get; private set; }

    /// <summary>The deletion timestamp of the entity.</summary>
    public DateTime? DateDeleted { get; private set; }

    /// <summary>Flag indicating whether the entity has been changed.</summary>
    public bool IsDirty { get; private set; }

    /// <summary>Set the value of a property if it has changed.</summary>
    /// <param name="propertyName">The name of the property to update.</param>
    /// <param name="value">The type of the property to update.</param>
    /// <returns>A boolean indicating whether the property has been updated.</returns>
    /// <exception cref="EntityUpdateException">
    /// When the property does not exist, is not accessible, or is not of a compatible type.
    /// </exception>
    public virtual bool UpdateProperty(string propertyName, object? value)
    {
        // If the provided value is null, do nothing
        if (value is null)
            return false;

        // Otherwise, grab the property and check that value can be assigned to the property type
        var property = GetProperty(propertyName);
        if (!value.GetType().IsAssignableTo(property.PropertyType))
            throw EntityUpdateException.ForIncorrectType(GetType(), propertyName, property.PropertyType, value.GetType());

        // If the value has not changed, do nothing 
        var currentValue = property.GetValue(this);
        if (value.Equals(currentValue))
            return false;

        // If it has, set the property and return dirty.
        SetProperty(property, value);
        return true;
    }

    /// <summary>Set the modified date to the current timestamp and marks the entity as dirty.</summary>
    protected virtual void SetDateModified()
    {
        DateModified = DateTimeOffsetProvider.Now.UtcDateTime;
        IsDirty = true;
    }

    /// <summary>Marks the entity as deleted.</summary>
    public virtual void SoftDelete()
        => DateDeleted = DateTimeOffsetProvider.Now.UtcDateTime;

    /// <inheritdoc />
    public abstract object AsSerializable();

    /// <inheritdoc />
    public override string ToString() => AsSerializable().Serialize();

    /// <summary>Get the <see cref="PropertyInfo"/> representing a property of this entity.</summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <exception cref="EntityUpdateException">
    /// When the property does not exist, is not accessible, or is not of a compatible type.
    /// </exception>
    private PropertyInfo GetProperty(string propertyName)
        => GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
               .FirstOrDefault(p =>
                   p.Name.Equals(propertyName, StringComparison.Ordinal) &&
                   p is { CanRead: true, CanWrite: true })
           ?? throw EntityUpdateException.ForMissingProperty(GetType(), propertyName);

    /// <summary>Set a property on this entity.</summary>
    /// <param name="propertyInfo">A <see cref="PropertyInfo"/> instance representing the property.</param>
    /// <param name="value">The value to assign.</param>
    private void SetProperty(PropertyInfo propertyInfo, object value)
    {
        propertyInfo.SetValue(this, value);
        SetDateModified();
    }
}