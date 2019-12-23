using System;

namespace Klyte.Commons.Interfaces
{
    public interface IEnumerableIndex<T> where T : Enum
    {
        T Index { get; set; }
    }
}