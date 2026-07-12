using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Common;

namespace SharpCompress;

public static class AsyncEnumerableEx
{
    public static async IAsyncEnumerable<T> Empty<T>()
        where T : notnull
    {
        await Task.Yield();
        yield break;
    }
}

public static class EnumerableExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
        }
    }
}

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<TResult> CastAsync<TResult>(
        this IAsyncEnumerable<object?> source
    )
        where TResult : class
    {
        await foreach (var item in source.ConfigureAwait(false))
        {
            yield return (item as TResult).NotNull();
        }
    }

    public static async ValueTask<TAccumulate> AggregateAsync<TAccumulate, T>(
        this IAsyncEnumerable<T> source,
        TAccumulate seed,
        Func<TAccumulate, T, TAccumulate> func
    )
    {
        var result = seed;
        await foreach (var element in source.ConfigureAwait(false))
        {
            result = func(result, element);
        }
        return result;
    }
}
