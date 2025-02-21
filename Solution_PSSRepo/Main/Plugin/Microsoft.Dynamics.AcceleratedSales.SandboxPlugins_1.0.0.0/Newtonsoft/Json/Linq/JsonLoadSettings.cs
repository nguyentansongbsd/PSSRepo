// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Linq.JsonLoadSettings
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System;

#nullable disable
namespace Newtonsoft.Json.Linq
{
  internal class JsonLoadSettings
  {
    private CommentHandling _commentHandling;
    private LineInfoHandling _lineInfoHandling;
    private DuplicatePropertyNameHandling _duplicatePropertyNameHandling;

    public JsonLoadSettings()
    {
      this._lineInfoHandling = LineInfoHandling.Load;
      this._commentHandling = CommentHandling.Ignore;
      this._duplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace;
    }

    public CommentHandling CommentHandling
    {
      get => this._commentHandling;
      set
      {
        this._commentHandling = value >= CommentHandling.Ignore && value <= CommentHandling.Load ? value : throw new ArgumentOutOfRangeException(nameof (value));
      }
    }

    public LineInfoHandling LineInfoHandling
    {
      get => this._lineInfoHandling;
      set
      {
        this._lineInfoHandling = value >= LineInfoHandling.Ignore && value <= LineInfoHandling.Load ? value : throw new ArgumentOutOfRangeException(nameof (value));
      }
    }

    public DuplicatePropertyNameHandling DuplicatePropertyNameHandling
    {
      get => this._duplicatePropertyNameHandling;
      set
      {
        this._duplicatePropertyNameHandling = value >= DuplicatePropertyNameHandling.Replace && value <= DuplicatePropertyNameHandling.Error ? value : throw new ArgumentOutOfRangeException(nameof (value));
      }
    }
  }
}
