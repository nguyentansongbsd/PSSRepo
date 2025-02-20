// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Utilities.ThreadSafeStore`2
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;
using System.Collections.Concurrent;

#nullable enable
namespace Newtonsoft.Json.Utilities
{
  internal class ThreadSafeStore<TKey, TValue>
  {
    private readonly ConcurrentDictionary<TKey, TValue> _concurrentStore;
    private readonly Func<TKey, TValue> _creator;

    public ThreadSafeStore(Func<TKey, TValue> creator)
    {
      ValidationUtils.ArgumentNotNull((object) creator, nameof (creator));
      this._creator = creator;
      this._concurrentStore = new ConcurrentDictionary<TKey, TValue>();
    }

    public TValue Get(TKey key) => this._concurrentStore.GetOrAdd(key, this._creator);
  }
}
