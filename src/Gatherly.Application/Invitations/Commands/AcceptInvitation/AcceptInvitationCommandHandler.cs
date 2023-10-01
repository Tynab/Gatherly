using Gatherly.Application.Abstractions;
using Gatherly.Domain.Repositories;
using MediatR;
using static Gatherly.Domain.Entities.InvitationStatus;
using static MediatR.Unit;

namespace Gatherly.Application.Invitations.Commands.AcceptInvitation;

internal class AcceptInvitationCommandHandler(
    IInvitationRepository invitationRepository,
    IMemberRepository memberRepository,
    IGatheringRepository gatheringRepository,
    IAttendeeRepository attendeeRepository,
    IUnitOfWork unitOfWork,
    IEmailService emailService
) : IRequestHandler<AcceptInvitationCommand>
{
    private readonly IInvitationRepository _invitationRepository = invitationRepository;
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly IGatheringRepository _gatheringRepository = gatheringRepository;
    private readonly IAttendeeRepository _attendeeRepository = attendeeRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEmailService _emailService = emailService;

    public async Task<Unit> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId, cancellationToken);

        if (invitation is null || invitation.Status is not Pending)
        {
            return Value;
        }

        var member = await _memberRepository.GetByIdAsync(invitation.MemberId, cancellationToken);
        var gathering = await _gatheringRepository.GetByIdWithCreatorAsync(invitation.GatheringId, cancellationToken);

        if (member is null || gathering is null)
        {
            return Value;
        }

        var attendee = gathering.AcceptInvitation(invitation);

        if (attendee is not null)
        {
            _attendeeRepository.Add(attendee);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (invitation.Status is Accepted)
        {
            await _emailService.SendInvitationAcceptedEmailAsync(gathering, cancellationToken);
        }

        return Value;
    }
}
