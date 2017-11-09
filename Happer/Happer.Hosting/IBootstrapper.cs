using Happer.Http;

namespace Happer
{
    public interface IBootstrapper
    {
        IEngine BootWith(IModuleContainer container);
        IEngine BootWith(IModuleContainer container, IPipelines pipelines);
    }
}
