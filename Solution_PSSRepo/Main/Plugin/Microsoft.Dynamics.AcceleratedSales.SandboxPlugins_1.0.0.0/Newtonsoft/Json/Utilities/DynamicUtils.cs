// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Utilities.DynamicUtils
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

#nullable enable
namespace Newtonsoft.Json.Utilities
{
  internal static class DynamicUtils
  {
    public static IEnumerable<string> GetDynamicMemberNames(
      this IDynamicMetaObjectProvider dynamicProvider)
    {
      return dynamicProvider.GetMetaObject((Expression) Expression.Constant((object) dynamicProvider)).GetDynamicMemberNames();
    }

    internal static class BinderWrapper
    {
      public const string CSharpAssemblyName = "Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
      private const string BinderTypeName = "Microsoft.CSharp.RuntimeBinder.Binder, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
      private const string CSharpArgumentInfoTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
      private const string CSharpArgumentInfoFlagsTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
      private const string CSharpBinderFlagsTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
      private static object? _getCSharpArgumentInfoArray;
      private static object? _setCSharpArgumentInfoArray;
      private static MethodCall<object?, object?>? _getMemberCall;
      private static MethodCall<object?, object?>? _setMemberCall;
      private static bool _init;

      private static void Init()
      {
        if (DynamicUtils.BinderWrapper._init)
          return;
        if (Type.GetType("Microsoft.CSharp.RuntimeBinder.Binder, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false) == (Type) null)
          throw new InvalidOperationException("Could not resolve type '{0}'. You may need to add a reference to Microsoft.CSharp.dll to work with dynamic types.".FormatWith((IFormatProvider) CultureInfo.InvariantCulture, (object) "Microsoft.CSharp.RuntimeBinder.Binder, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
        DynamicUtils.BinderWrapper._getCSharpArgumentInfoArray = DynamicUtils.BinderWrapper.CreateSharpArgumentInfoArray(new int[1]);
        DynamicUtils.BinderWrapper._setCSharpArgumentInfoArray = DynamicUtils.BinderWrapper.CreateSharpArgumentInfoArray(0, 3);
        DynamicUtils.BinderWrapper.CreateMemberCalls();
        DynamicUtils.BinderWrapper._init = true;
      }

      private static object CreateSharpArgumentInfoArray(params int[] values)
      {
        Type type1 = Type.GetType("Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
        Type type2 = Type.GetType("Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
        Array instance = Array.CreateInstance(type1, values.Length);
        for (int index = 0; index < values.Length; ++index)
        {
          object obj = type1.GetMethod("Create", new Type[2]
          {
            type2,
            typeof (string)
          }).Invoke((object) null, new object[2]
          {
            (object) 0,
            null
          });
          instance.SetValue(obj, index);
        }
        return (object) instance;
      }

      private static void CreateMemberCalls()
      {
        Type type1 = Type.GetType("Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true);
        Type type2 = Type.GetType("Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true);
        Type type3 = Type.GetType("Microsoft.CSharp.RuntimeBinder.Binder, Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true);
        Type type4 = typeof (IEnumerable<>).MakeGenericType(type1);
        DynamicUtils.BinderWrapper._getMemberCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>((MethodBase) type3.GetMethod("GetMember", new Type[4]
        {
          type2,
          typeof (string),
          typeof (Type),
          type4
        }));
        DynamicUtils.BinderWrapper._setMemberCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>((MethodBase) type3.GetMethod("SetMember", new Type[4]
        {
          type2,
          typeof (string),
          typeof (Type),
          type4
        }));
      }

      public static CallSiteBinder GetMember(string name, Type context)
      {
        DynamicUtils.BinderWrapper.Init();
        return (CallSiteBinder) DynamicUtils.BinderWrapper._getMemberCall((object) null, (object) 0, (object) name, (object) context, DynamicUtils.BinderWrapper._getCSharpArgumentInfoArray);
      }

      public static CallSiteBinder SetMember(string name, Type context)
      {
        DynamicUtils.BinderWrapper.Init();
        return (CallSiteBinder) DynamicUtils.BinderWrapper._setMemberCall((object) null, (object) 0, (object) name, (object) context, DynamicUtils.BinderWrapper._setCSharpArgumentInfoArray);
      }
    }
  }
}
