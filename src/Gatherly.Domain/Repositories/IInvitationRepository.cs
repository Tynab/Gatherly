using Gatherly.Domain.Entities;

namespace Gatherly.Domain.Repositories;

public interface IInvitationRepository
{
    public ValueTask<Invitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public void Add(Invitation invitation);
}
