using Happer.Http;

namespace Happer.StaticContent
{
    public interface IStaticContentProvider
    {
        Response GetContent(Context context);
    }
}
