using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Teams.Core.CQRS;

[ExcludeFromCodeCoverage]
public record MediatorConfiguration(IEnumerable<Assembly> Assemblies)
{
    public MediatorConfiguration()
        : this([Assembly.GetExecutingAssembly()])
    {

    }
}