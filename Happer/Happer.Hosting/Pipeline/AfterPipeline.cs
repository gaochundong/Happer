using System;
using System.Threading;
using System.Threading.Tasks;
using Happer.Http;

namespace Happer.Pipelining
{
    public class AfterPipeline : AsyncNamedPipelineBase<Func<Context, CancellationToken, Task>, Action<Context>>
    {
        public AfterPipeline()
        {
        }

        public AfterPipeline(int capacity)
            : base(capacity)
        {
        }

        public static implicit operator Func<Context, CancellationToken, Task>(AfterPipeline pipeline)
        {
            return pipeline.Invoke;
        }

        public static implicit operator AfterPipeline(Func<Context, CancellationToken, Task> func)
        {
            var pipeline = new AfterPipeline();
            pipeline.AddItemToEndOfPipeline(func);
            return pipeline;
        }

        public static AfterPipeline operator +(AfterPipeline pipeline, Func<Context, CancellationToken, Task> func)
        {
            pipeline.AddItemToEndOfPipeline(func);
            return pipeline;
        }

        public static AfterPipeline operator +(AfterPipeline pipeline, Action<Context> action)
        {
            pipeline.AddItemToEndOfPipeline(action);
            return pipeline;
        }

        public static AfterPipeline operator +(AfterPipeline pipelineToAddTo, AfterPipeline pipelineToAdd)
        {
            foreach (var pipelineItem in pipelineToAdd.PipelineItems)
            {
                pipelineToAddTo.AddItemToEndOfPipeline(pipelineItem);
            }

            return pipelineToAddTo;
        }

        public async Task Invoke(Context context, CancellationToken cancellationToken)
        {
            foreach (var pipelineDelegate in this.PipelineDelegates)
            {
                await pipelineDelegate.Invoke(context, cancellationToken).ConfigureAwait(false);
            }
        }

        protected override PipelineItem<Func<Context, CancellationToken, Task>> Wrap(PipelineItem<Action<Context>> pipelineItem)
        {
            return new PipelineItem<Func<Context, CancellationToken, Task>>(pipelineItem.Name, (ctx, ct) =>
            {
                pipelineItem.Delegate(ctx);
                return Task.FromResult<object>(null);
            });
        }
    }
}
