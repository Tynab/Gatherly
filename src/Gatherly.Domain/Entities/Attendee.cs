namespace Gatherly.Domain.Entities;

public sealed class Attendee
{
    public Guid GatheringId { get; set; }
    public Guid MemberId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
