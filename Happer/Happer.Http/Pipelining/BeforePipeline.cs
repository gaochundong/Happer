using System;
using System.Threading;
using System.Threading.Tasks;

namespace Happer.Http
{
    public class BeforePipeline : AsyncNamedPipelineBase<Func<Context, CancellationToken, Task<Response>>, Func<Context, Response>>
    {
        public BeforePipeline()
        {
        }

        public BeforePipeline(int capacity)
            : base(capacity)
        {
        }

        public static implicit operator Func<Context, CancellationToken, Task<Response>>(BeforePipeline pipeline)
        {
            return pipeline.Invoke;
        }

        public static implicit operator BeforePipeline(Func<Context, CancellationToken, Task<Response>> func)
        {
            var pipeline = new BeforePipeline();
            pipeline.AddItemToEndOfPipeline(func);
            return pipeline;
        }

        public static BeforePipeline operator +(BeforePipeline pipeline, Func<Context, CancellationToken, Task<Response>> func)
        {
            pipeline.AddItemToEndOfPipeline(func);
            return pipeline;
        }

        public static BeforePipeline operator +(BeforePipeline pipeline, Func<Context, Response> action)
        {
            pipeline.AddItemToEndOfPipeline(action);
            return pipeline;
        }

        public static BeforePipeline operator +(BeforePipeline pipelineToAddTo, BeforePipeline pipelineToAdd)
        {
            foreach (var pipelineItem in pipelineToAdd.PipelineItems)
            {
                pipelineToAddTo.AddItemToEndOfPipeline(pipelineItem);
            }

            return pipelineToAddTo;
        }

        public async Task<Response> Invoke(Context context, CancellationToken cancellationToken)
        {
            foreach (var pipelineDelegate in this.PipelineDelegates)
            {
                var response = await pipelineDelegate.Invoke(context, cancellationToken).ConfigureAwait(false);
                if (response != null)
                {
                    return response;
                }
            }

            return null;
        }

        protected override PipelineItem<Func<Context, CancellationToken, Task<Response>>> Wrap(PipelineItem<Func<Context, Response>> pipelineItem)
        {
            return new PipelineItem<Func<Context, CancellationToken, Task<Response>>>(pipelineItem.Name, (ctx, ct) => Task.FromResult(pipelineItem.Delegate(ctx)));
        }
    }
}
