
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace genshinbot.memo
{
    /// <summary>
    /// Represents a computed value from a memoized function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public record Mem<T>(
        ///the returned value
        T Val, 

        ///a task which is completed when the val is outdated
        Task Token
    );

    public delegate Mem<T> Fn<T>();
}
