using System;

namespace Happer.Http
{
    public class ErrorPipeline : NamedPipelineBase<Func<Context, Exception, dynamic>>
    {
        public ErrorPipeline()
        {
        }

        public ErrorPipeline(int capacity) 
            : base(capacity)
        {
        }

        public static implicit operator Func<Context, Exception, dynamic>(ErrorPipeline pipeline)
        {
            return pipeline.Invoke;
        }

        public static implicit operator ErrorPipeline(Func<Context, Exception, dynamic> func)
        {
            var pipeline = new ErrorPipeline();
            pipeline.AddItemToEndOfPipeline(func);
            return pipeline;
        }

        public static ErrorPipeline operator +(ErrorPipeline pipeline, Func<Context, Exception, dynamic> func)
        {
            pipeline.AddItemToEndOfPipeline(func);
            return pipeline;
        }

        public static ErrorPipeline operator +(ErrorPipeline pipelineToAddTo, ErrorPipeline pipelineToAdd)
        {
            foreach (var pipelineItem in pipelineToAdd.PipelineItems)
            {
                pipelineToAddTo.AddItemToEndOfPipeline(pipelineItem);
            }

            return pipelineToAddTo;
        }

        public dynamic Invoke(Context context, Exception ex)
        {
            dynamic returnValue = null;

            using (var enumerator = this.PipelineDelegates.GetEnumerator())
            {
                while (returnValue == null && enumerator.MoveNext())
                {
                    returnValue = enumerator.Current.Invoke(context, ex);
                }
            }

            return returnValue;
        }
    }
}
