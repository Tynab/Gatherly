using Gatherly.Domain.Entities;

namespace Gatherly.Domain.Repositories;

public interface IMemberRepository
{
    public ValueTask<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
