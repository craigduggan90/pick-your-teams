using Teams.Core.Models;

namespace Teams.Core.Services;

public interface IActorAccessor
{
    Actor Current { get; }
}