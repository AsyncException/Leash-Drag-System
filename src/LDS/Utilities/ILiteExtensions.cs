using LiteDB;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LDS.Utilities;
public static class ILiteExtensions
{
    public static bool TryGetFirst<T>(this IEnumerable<T> collection, [NotNullWhen(true)] out T? result) where T : class => (result = collection.FirstOrDefault()) is not null;
}
