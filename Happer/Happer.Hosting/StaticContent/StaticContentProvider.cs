using Happer.Http;

namespace Happer.StaticContent
{
    public class StaticContentProvider : IStaticContentProvider
    {
        private readonly IRootPathProvider rootPathProvider;
        private readonly StaticContentsConventions conventions;
        private string rootPath;

        public StaticContentProvider(IRootPathProvider rootPathProvider, StaticContentsConventions conventions)
        {
            this.rootPathProvider = rootPathProvider;
            this.rootPath = this.rootPathProvider.GetRootPath();
            this.conventions = conventions;
        }

        public Response GetContent(Context context)
        {
            foreach (var convention in this.conventions)
            {
                var result = convention.Invoke(context, this.rootPath);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
