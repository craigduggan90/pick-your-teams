using System.Reflection;

namespace Teams.Core.CQRS;

public record MediatorConfiguration(IEnumerable<Assembly> Assemblies)
{
    public MediatorConfiguration()
        : this([Assembly.GetExecutingAssembly()])
    {

    }
}