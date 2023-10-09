using Gatherly.Domain.Primitives;
using static Gatherly.Domain.Entities.InvitationStatus;
using static System.DateTime;

namespace Gatherly.Domain.Entities;

public sealed class Invitation(Guid id, Member member, Gathering gathering) : Entity(id)
{
    public Guid GatheringId { get; private set; } = gathering.Id;
    public Guid MemberId { get; private set; } = member.Id;
    public InvitationStatus Status { get; private set; } = Pending;
    public DateTime CreatedOnUtc { get; private set; } = UtcNow;
    public DateTime? ModifiedOnUtc { get; private set; }

    internal void Expire()
    {
        Status = Expired;
        ModifiedOnUtc = UtcNow;
    }

    internal Attendee Accept()
    {
        Status = Accepted;
        ModifiedOnUtc = UtcNow;

        var attendee = new Attendee(this);

        return attendee;
    }
}
