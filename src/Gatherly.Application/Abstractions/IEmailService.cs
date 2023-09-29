using Gatherly.Domain.Entities;

namespace Gatherly.Application.Abstractions;

public interface IEmailService
{
    public Task SendInvitationSentEmailAsync(Member member, Gathering gathering, CancellationToken cancellationToken = default);
    public Task SendInvitationAcceptedEmailAsync(Gathering gathering, CancellationToken cancellationToken = default);
}
