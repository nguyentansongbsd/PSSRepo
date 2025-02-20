// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Utilities.BufferUtils
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable enable
namespace Newtonsoft.Json.Utilities
{
  internal static class BufferUtils
  {
    public static char[] RentBuffer(IArrayPool<char>? bufferPool, int minSize)
    {
      return bufferPool == null ? new char[minSize] : bufferPool.Rent(minSize);
    }

    public static void ReturnBuffer(IArrayPool<char>? bufferPool, char[]? buffer)
    {
      bufferPool?.Return(buffer);
    }

    public static char[] EnsureBufferSize(IArrayPool<char>? bufferPool, int size, char[]? buffer)
    {
      if (bufferPool == null)
        return new char[size];
      if (buffer != null)
        bufferPool.Return(buffer);
      return bufferPool.Rent(size);
    }
  }
}
