using Gatherly.Domain.Repositories;
using MediatR;
using static Gatherly.Domain.Entities.Gathering;
using static MediatR.Unit;

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

        var gathering = Create(member, request.Type, request.ScheduledAtUtc, request.Name, request.Location, request.MaximumNumberOfAttendees, request.InvitationsValidBeforeInHours);

        _gatheringRepository.Add(gathering);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Value;
    }
}
