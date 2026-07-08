using System;

namespace GastroErp.Application.Common.Interfaces;

public interface IDateTime
{
    DateTime UtcNow { get; }
}
