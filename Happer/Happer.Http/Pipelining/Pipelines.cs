using System.Linq;

namespace Happer.Http
{
    public class Pipelines : IPipelines
    {
        public Pipelines()
        {
            this.AfterRequest = new AfterPipeline();
            this.BeforeRequest = new BeforePipeline();
            this.OnError = new ErrorPipeline();
        }

        public Pipelines(IPipelines pipelines)
        {
            this.AfterRequest = new AfterPipeline(pipelines.AfterRequest.PipelineItems.Count());

            foreach (var pipelineItem in pipelines.AfterRequest.PipelineItems)
            {
                this.AfterRequest.AddItemToEndOfPipeline(pipelineItem);
            }

            this.BeforeRequest = new BeforePipeline(pipelines.BeforeRequest.PipelineItems.Count());

            foreach (var pipelineItem in pipelines.BeforeRequest.PipelineItems)
            {
                this.BeforeRequest.AddItemToEndOfPipeline(pipelineItem);
            }

            this.OnError = new ErrorPipeline(pipelines.OnError.PipelineItems.Count());

            foreach (var pipelineItem in pipelines.OnError.PipelineItems)
            {
                this.OnError.AddItemToEndOfPipeline(pipelineItem);
            }
        }

        public BeforePipeline BeforeRequest { get; set; }

        public AfterPipeline AfterRequest { get; set; }

        public ErrorPipeline OnError { get; set; }
    }
}
