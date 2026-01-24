using System;
using System.Threading;
using System.Threading.Tasks;

namespace EduShelf.Api.Services.Background
{
    public interface IBackgroundJobQueue
    {
        ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);

        ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
    }
}
