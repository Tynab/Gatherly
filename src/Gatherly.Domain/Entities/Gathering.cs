using Gatherly.Domain.Primitives;
using static Gatherly.Domain.Entities.GatheringType;
using static System.DateTime;
using static System.Guid;

namespace Gatherly.Domain.Entities;

public sealed class Gathering(Guid id, Member creator, GatheringType type, DateTime scheduledAtUtc, string name, string location) : Entity(id)
{
    private readonly List<Invitation> _invitations = new();
    private readonly List<Attendee> _attendees = new();

    public Member? Creator { get; private set; } = creator;
    public GatheringType Type { get; private set; } = type;
    public string? Name { get; private set; } = name;
    public DateTime ScheduledAtUtc { get; private set; } = scheduledAtUtc;
    public string? Location { get; private set; } = location;
    public int? MaximumNumberOfAttendees { get; private set; }
    public DateTime? InvitationsExpireAtUtc { get; private set; }
    public int NumberOfAttendees { get; private set; }
    public IReadOnlyCollection<Attendee>? Attendees => _attendees;
    public IReadOnlyCollection<Invitation>? Invitations => _invitations;

    public static Gathering Create(Member creator, GatheringType type, DateTime scheduledAtUtc, string name, string location, int? maximumNumberOfAttendees, int? invitationsValidBeforeInHours)
    {
        var gathering = new Gathering(NewGuid(), creator, type, scheduledAtUtc, name, location);

        switch (gathering.Type)
        {
            case WithFixedNumberOfAttendees:
                if (maximumNumberOfAttendees is null)
                {
                    throw new Exception($"{nameof(maximumNumberOfAttendees)} can't be null.");
                }

                gathering.MaximumNumberOfAttendees = maximumNumberOfAttendees;
                break;
            case WithExpirationForInvitations:
                if (invitationsValidBeforeInHours is null)
                {
                    throw new Exception($"{nameof(invitationsValidBeforeInHours)} can't be null.");
                }

                gathering.InvitationsExpireAtUtc = gathering.ScheduledAtUtc.AddHours(-invitationsValidBeforeInHours.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }

        return gathering;
    }

    public Invitation SendInvitation(Member member)
    {
        if (Creator?.Id == member.Id)
        {
            throw new Exception("Can't send invitation to the gathering creator.");
        }

        if (ScheduledAtUtc < UtcNow)
        {
            throw new Exception("Can't send invitation for gathering in the past.");
        }

        var invitation = new Invitation(NewGuid(), member, this);

        _invitations.Add(invitation);

        return invitation;
    }

    public Attendee? AcceptInvitation(Invitation invitation)
    {
        var expired = Type is WithFixedNumberOfAttendees && NumberOfAttendees == MaximumNumberOfAttendees || Type is WithExpirationForInvitations && InvitationsExpireAtUtc < UtcNow;

        if (expired)
        {
            invitation.Expire();

            return default;
        }

        var attendee = invitation.Accept();

        _attendees.Add(attendee);
        NumberOfAttendees++;

        return attendee;
    }
}
