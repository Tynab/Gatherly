using static System.DateTime;

namespace Gatherly.Domain.Entities;

public sealed class Attendee(Invitation invitation)
{
    public Guid GatheringId { get; private set; } = invitation.GatheringId;
    public Guid MemberId { get; private set; } = invitation.MemberId;
    public DateTime CreatedOnUtc { get; private set; } = UtcNow;
}
