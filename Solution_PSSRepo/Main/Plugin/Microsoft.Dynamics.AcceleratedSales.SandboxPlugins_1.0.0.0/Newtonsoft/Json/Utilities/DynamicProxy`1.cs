// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Utilities.DynamicProxy`1
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Collections.Generic;
using System.Dynamic;

#nullable enable
namespace Newtonsoft.Json.Utilities
{
  internal class DynamicProxy<T>
  {
    public virtual IEnumerable<string> GetDynamicMemberNames(T instance)
    {
      return (IEnumerable<string>) CollectionUtils.ArrayEmpty<string>();
    }

    public virtual bool TryBinaryOperation(
      T instance,
      BinaryOperationBinder binder,
      object arg,
      out object? result)
    {
      result = (object) null;
      return false;
    }

    public virtual bool TryConvert(T instance, ConvertBinder binder, out object? result)
    {
      result = (object) null;
      return false;
    }

    public virtual bool TryCreateInstance(
      T instance,
      CreateInstanceBinder binder,
      object[] args,
      out object? result)
    {
      result = (object) null;
      return false;
    }

    public virtual bool TryDeleteIndex(T instance, DeleteIndexBinder binder, object[] indexes)
    {
      return false;
    }

    public virtual bool TryDeleteMember(T instance, DeleteMemberBinder binder) => false;

    public virtual bool TryGetIndex(
      T instance,
      GetIndexBinder binder,
      object[] indexes,
      out object? result)
    {
      result = (object) null;
      return false;
    }

    public virtual bool TryGetMember(T instance, GetMemberBinder binder, out object? result)
    {
      result = (object) null;
      return false;
    }

    public virtual bool TryInvoke(
      T instance,
      InvokeBinder binder,
      object[] args,
      out object? result)
    {
      result = (object) null;
      return false;
    }

    public virtual bool TryInvokeMember(
      T instance,
      InvokeMemberBinder binder,
      object[] args,
      out object? result)
    {
      result = (object) null;
      return false;
    }

    public virtual bool TrySetIndex(
      T instance,
      SetIndexBinder binder,
      object[] indexes,
      object value)
    {
      return false;
    }

    public virtual bool TrySetMember(T instance, SetMemberBinder binder, object value) => false;

    public virtual bool TryUnaryOperation(
      T instance,
      UnaryOperationBinder binder,
      out object? result)
    {
      result = (object) null;
      return false;
    }
  }
}
