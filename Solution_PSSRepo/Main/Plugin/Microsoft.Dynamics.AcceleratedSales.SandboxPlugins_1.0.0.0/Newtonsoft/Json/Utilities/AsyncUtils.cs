// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Utilities.AsyncUtils
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace Newtonsoft.Json.Utilities
{
  internal static class AsyncUtils
  {
    public static readonly Task<bool> False = Task.FromResult<bool>(false);
    public static readonly Task<bool> True = Task.FromResult<bool>(true);
    internal static readonly Task CompletedTask = Task.Delay(0);

    internal static Task<bool> ToAsync(this bool value)
    {
      return !value ? AsyncUtils.False : AsyncUtils.True;
    }

    public static Task? CancelIfRequestedAsync(this CancellationToken cancellationToken)
    {
      return !cancellationToken.IsCancellationRequested ? (Task) null : cancellationToken.FromCanceled();
    }

    public static Task<T>? CancelIfRequestedAsync<T>(this CancellationToken cancellationToken)
    {
      return !cancellationToken.IsCancellationRequested ? (Task<T>) null : cancellationToken.FromCanceled<T>();
    }

    public static Task FromCanceled(this CancellationToken cancellationToken)
    {
      return new Task((Action) (() => { }), cancellationToken);
    }

    public static Task<T> FromCanceled<T>(this CancellationToken cancellationToken)
    {
      return new Task<T>((Func<T>) (() => default (T)), cancellationToken);
    }

    public static Task WriteAsync(
      this TextWriter writer,
      char value,
      CancellationToken cancellationToken)
    {
      return !cancellationToken.IsCancellationRequested ? writer.WriteAsync(value) : cancellationToken.FromCanceled();
    }

    public static Task WriteAsync(
      this TextWriter writer,
      string? value,
      CancellationToken cancellationToken)
    {
      return !cancellationToken.IsCancellationRequested ? writer.WriteAsync(value) : cancellationToken.FromCanceled();
    }

    public static Task WriteAsync(
      this TextWriter writer,
      char[] value,
      int start,
      int count,
      CancellationToken cancellationToken)
    {
      return !cancellationToken.IsCancellationRequested ? writer.WriteAsync(value, start, count) : cancellationToken.FromCanceled();
    }

    public static Task<int> ReadAsync(
      this TextReader reader,
      char[] buffer,
      int index,
      int count,
      CancellationToken cancellationToken)
    {
      return !cancellationToken.IsCancellationRequested ? reader.ReadAsync(buffer, index, count) : cancellationToken.FromCanceled<int>();
    }

    public static bool IsCompletedSucessfully(this Task task)
    {
      return task.Status == TaskStatus.RanToCompletion;
    }
  }
}
