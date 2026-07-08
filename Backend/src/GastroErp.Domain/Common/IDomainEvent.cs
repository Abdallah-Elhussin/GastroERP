namespace GastroErp.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
