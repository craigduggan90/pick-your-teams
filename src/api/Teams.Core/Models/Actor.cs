using Teams.Core.Exceptions;

namespace Teams.Core.Models;

public record Actor(string Id, string Tag, string DisplayName)
{
    public void ThrowIfNotOrganiser(string organiserId)
    {
        if (!Id.Equals(organiserId, StringComparison.OrdinalIgnoreCase))
            throw AccessDeniedException.ForOrganiserOnly();
    }

    public void ThrowIfNotUser(string userId)
    {
        if (!Id.Equals(userId, StringComparison.OrdinalIgnoreCase))
            throw AccessDeniedException.ForSelfOnly();
    }

    public void ThrowIfNotOrganiserOrUser(string userId, string organiserId)
    {
        if (!new[] { userId, organiserId }.Contains(Id, StringComparer.OrdinalIgnoreCase))
            throw AccessDeniedException.ForOrganiserOrSelfOnly();
    }
}