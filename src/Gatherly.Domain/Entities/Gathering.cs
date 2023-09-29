namespace Gatherly.Domain.Entities;

public sealed class Gathering
{
    public Guid Id { get; set; }
    public Member? Creator { get; set; }
    public GatheringType Type { get; set; }
    public string? Name { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public string? Location { get; set; }
    public int? MaximumNumberOfAttendees { get; set; }
    public DateTime? InvitationsExpireAtUtc { get; set; }
    public int NumberOfAttendees { get; set; }
    public List<Attendee>? Attendees { get; set; }
    public List<Invitation>? Invitations { get; set; }
}
