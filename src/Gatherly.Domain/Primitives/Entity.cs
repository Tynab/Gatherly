using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Domain.Primitives;

public abstract class Entity : IEquatable<Entity>
{
    protected Entity(Guid id) => Id = id;

    public Guid Id { get; private init; }

    public static bool operator ==(Entity? first, Entity? second) => first is not null && second is not null && first.Equals(second);

    public static bool operator !=(Entity? first, Entity? second) => !(first == second);

    public override bool Equals(object? obj) => obj is not null && obj.GetType() == GetType() && obj is Entity entity && entity.Id == Id;

    public bool Equals(Entity? other) => other is not null && other.GetType() == GetType() && other.Id == Id;

    public override int GetHashCode() => Id.GetHashCode() * 41;
}
