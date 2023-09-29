using Gatherly.Domain.Entities;
using Gatherly.Domain.Repositories;
using MediatR;
using static Gatherly.Domain.Entities.GatheringType;
using static MediatR.Unit;
using static System.DateTime;
using static System.Guid;

namespace Gatherly.Application.Gatherings.Commands.CreateGathering;

internal class CreateGatheringCommandHandler(
    IMemberRepository memberRepository,
    IGatheringRepository gatheringRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateGatheringCommand>
{
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly IGatheringRepository _gatheringRepository = gatheringRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Unit> Handle(CreateGatheringCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);

        if (member is null)
        {
            return Value;
        }

        var gathering = new Gathering
        {
            Id = NewGuid(),
            Creator = member,
            Type = request.Type,
            ScheduledAtUtc = UtcNow,
            Name = request.Name,
            Location = request.Location
        };

        switch (gathering.Type)
        {
            case WithFixedNumberOfAttendees:
                if (request.MaximumNumberOfAttendees is null)
                {
                    throw new Exception($"{nameof(request.MaximumNumberOfAttendees)} can't be null.");
                }

                break;
            case WithExpirationForInvitations:
                if (request.InvitationsValidBeforeInHours is null)
                {
                    throw new Exception($"{nameof(request.InvitationsValidBeforeInHours)} can't be null.");
                }

                gathering.InvitationsExpireAtUtc = gathering.ScheduledAtUtc.AddHours(-request.InvitationsValidBeforeInHours.Value);

                break;
            default:
                throw new ArgumentException(nameof(GatheringType));
        }

        _gatheringRepository.Add(gathering);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Value;
    }
}
