using System.Reflection;

namespace Teams.Core.Exceptions;

/// <summary>Represents errors that occur when a requested resource cannot be found.</summary>
public class NotFoundException : Exception
{
    /// <summary>The name of the type of the requested resource, if provided.</summary>
    public string? ResourceType { get; init; }

    /// <summary>The identifier of the requested resource, if provided.</summary>
    public string? ResourceIdentifier { get; init; }

    internal const string ExceptionMessage = "The requested resource was not found.";

    /// <summary>Initializes a new instance of the <see cref="NotFoundException"/> class.</summary>
    public NotFoundException()
        : base(ExceptionMessage)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="NotFoundException"/> class.</summary>
    /// <param name="type">The type of the requested resource.</param>
    public NotFoundException(MemberInfo type)
        : base(ExceptionMessage)
        => ResourceType = type.Name;

    /// <summary>Initializes a new instance of the <see cref="NotFoundException"/> class.</summary>
    /// <param name="type">The name of the type of the requested resource.</param>
    public NotFoundException(string type)
        : base(ExceptionMessage)
        => ResourceType = type;

    /// <summary>Initializes a new instance of the <see cref="NotFoundException"/> class.</summary>
    /// <param name="type">The type of the requested resource.</param>
    /// <param name="identifier">The identifier of the requested resource.</param>
    public NotFoundException(MemberInfo type, object identifier)
        : base(ExceptionMessage)
    {
        ResourceType = type.Name;
        ResourceIdentifier = identifier.ToString();
    }

    /// <summary>Initializes a new instance of the <see cref="NotFoundException"/> class.</summary>
    /// <param name="type">The name of the type of the requested resource.</param>
    /// <param name="identifier">The identifier of the requested resource.</param>
    public NotFoundException(string type, object identifier)
        : base(ExceptionMessage)
    {
        ResourceType = type;
        ResourceIdentifier = identifier.ToString();
    }
}