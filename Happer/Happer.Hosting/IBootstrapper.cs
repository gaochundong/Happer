namespace Happer
{
    public interface IBootstrapper
    {
        IEngine BootWith(IModuleContainer container);
    }
}
