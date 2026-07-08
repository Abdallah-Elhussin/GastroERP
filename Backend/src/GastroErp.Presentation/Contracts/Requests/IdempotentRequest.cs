namespace GastroErp.Presentation.Contracts.Requests;

public abstract record IdempotentRequest
{
    // The idempotency key provided by the client (via header, but model bound if needed)
    // Actually, usually this comes from a header (e.g. Idempotency-Key). 
    // We can keep it here if we want to pass it explicitly in DTOs, but middleware is better.
    // As per instruction 14, just preparing the passive structure.
}
