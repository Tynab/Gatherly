using Gatherly.Application.Abstractions;
using Gatherly.Domain.Entities;
using Gatherly.Domain.Repositories;
using MediatR;
using static Gatherly.Domain.Entities.InvitationStatus;
using static MediatR.Unit;
using static System.DateTime;
using static System.Guid;

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

        if (gathering.Creator?.Id == member.Id)
        {
            throw new Exception("Can't send invitation to the gathering creator.");
        }

        if (gathering.ScheduledAtUtc < UtcNow)
        {
            throw new Exception("Can't send invitation for gathering in the past.");
        }

        var invitation = new Invitation
        {
            Id = NewGuid(),
            MemberId = member.Id,
            GatheringId = gathering.Id,
            Status = Pending,
            CreatedOnUtc = UtcNow
        };

        gathering.Invitations?.Add(invitation);
        _invitationRepository.Add(invitation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _emailService.SendInvitationSentEmailAsync(member, gathering, cancellationToken);

        return Value;
    }
}
