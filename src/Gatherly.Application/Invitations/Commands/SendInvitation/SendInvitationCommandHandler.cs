using Gatherly.Application.Abstractions;
using Gatherly.Domain.Repositories;
using MediatR;
using static MediatR.Unit;

namespace Gatherly.Application.Invitations.Commands.SendInvitation;

internal sealed class SendInvitationCommandHandler(
    IMemberRepository memberRepository,
    IGatheringRepository gatheringRepository,
    IInvitationRepository invitationRepository,
    IUnitOfWork unitOfWork,
    IEmailService emailService
) : IRequestHandler<SendInvitationCommand>
{
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly IGatheringRepository _gatheringRepository = gatheringRepository;
    private readonly IInvitationRepository _invitationRepository = invitationRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEmailService _emailService = emailService;

    public async Task<Unit> Handle(SendInvitationCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
        var gathering = await _gatheringRepository.GetByIdWithCreatorAsync(request.GatheringId, cancellationToken);

        if (member is null || gathering is null)
        {
            return Value;
        }

        var invitation = gathering.SendInvitation(member);

        _invitationRepository.Add(invitation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _emailService.SendInvitationSentEmailAsync(member, gathering, cancellationToken);

        return Value;
    }
}
