using System.Runtime.CompilerServices;

namespace genshinbot.memo
{
    public static class Fn
    {
        public static Fn<T> Memo<T>(Fn<T> f)
        {
            Mem<T>? prev = null;

            [MethodImpl(MethodImplOptions.Synchronized)]
            Mem<T> memoized()
            {
                if (prev is null || prev.Token.IsCompleted)
                {
                    prev = f();
                }
                return prev;
            }

            return memoized;
        }
    }
}
