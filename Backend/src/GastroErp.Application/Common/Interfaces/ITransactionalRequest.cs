namespace GastroErp.Application.Common.Interfaces;

/// <summary>
/// Marker for MediatR requests that must run inside a database transaction.
/// </summary>
public interface ITransactionalRequest;
