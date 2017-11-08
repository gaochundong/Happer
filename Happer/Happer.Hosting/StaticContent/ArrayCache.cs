namespace Happer.StaticContent
{
    public class ArrayCache
    {
        public static T[] Empty<T>()
        {
            return EmptyArray<T>.Value;
        }

        private static class EmptyArray<T>
        {
            public static readonly T[] Value = new T[0];
        }
    }
}
