using Teams.Domain.Entities.Abstract;

namespace Teams.Domain.Entities;

public class Player(
    string name,
    string? userId = null,
    int rating = 1000)
    : EntityBase
{
    public string Name { get; private set; } = name;

    public string? UserId { get; } = userId;

    public int Rating { get; private set; } = rating;

    public void Update(string? name)
    {
        UpdateProperty(nameof(Name), name);
    }

    public void Delete()
    {
        if (DateDeleted.HasValue)
            return;

        SetDateModified();
        SoftDelete();
    }

    public override object AsSerializable()
        => new { Id, DateCreated, DateModified };
}