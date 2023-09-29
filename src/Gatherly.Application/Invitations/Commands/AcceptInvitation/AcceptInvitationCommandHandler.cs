using Gatherly.Application.Abstractions;
using Gatherly.Domain.Entities;
using Gatherly.Domain.Repositories;
using MediatR;
using static Gatherly.Domain.Entities.GatheringType;
using static Gatherly.Domain.Entities.InvitationStatus;
using static MediatR.Unit;
using static System.DateTime;

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

        var expired = gathering.Type is WithFixedNumberOfAttendees
            && gathering.NumberOfAttendees < gathering.MaximumNumberOfAttendees
            || gathering.Type is WithExpirationForInvitations
            && gathering.InvitationsExpireAtUtc < UtcNow;

        if (expired)
        {
            invitation.Status = Expired;
            invitation.ModifiedOnUtc = UtcNow;
        }
        else
        {
            invitation.Status = Accepted;
            invitation.ModifiedOnUtc = UtcNow;
        }

        var attendee = new Attendee
        {
            MemberId = invitation.MemberId,
            GatheringId = invitation.GatheringId,
            CreatedOnUtc = UtcNow
        };

        gathering.Attendees?.Add(attendee);
        gathering.NumberOfAttendees++;
        _attendeeRepository.Add(attendee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (invitation.Status is Accepted)
        {
            await _emailService.SendInvitationAcceptedEmailAsync(gathering, cancellationToken);
        }

        return Value;
    }
}
