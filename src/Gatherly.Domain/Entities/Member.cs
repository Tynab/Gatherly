using Gatherly.Domain.Primitives;

namespace Gatherly.Domain.Entities;

public sealed class Member(Guid id, string firstName, string lastName, string email) : Entity(id)
{
    public string? FirstName { get; set; } = firstName;
    public string? LastName { get; set; } = lastName;
    public string? Email { get; set; } = email;
}
