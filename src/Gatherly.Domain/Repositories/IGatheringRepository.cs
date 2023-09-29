using Gatherly.Domain.Entities;

namespace Gatherly.Domain.Repositories;

public interface IGatheringRepository
{
    public ValueTask<Gathering?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public ValueTask<Gathering?> GetByIdWithCreatorAsync(Guid id, CancellationToken cancellationToken = default);
    public void Add(Gathering gathering);
}
