namespace GastroErp.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected init; } = Guid.NewGuid();
    
    public byte[] RowVersion { get; protected set; } = Array.Empty<byte>();

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
