namespace Happer.Http
{
    public interface IPipelines
    {
        BeforePipeline BeforeRequest { get; set; }
        AfterPipeline AfterRequest { get; set; }
        ErrorPipeline OnError { get; set; }
    }
}
