using System;
using System.Threading.Tasks;

namespace GastroErp.Application.Common.Interfaces;

public interface IBackgroundJob
{
    string Enqueue(Func<Task> methodCall);
    string Schedule(Func<Task> methodCall, TimeSpan delay);
}
