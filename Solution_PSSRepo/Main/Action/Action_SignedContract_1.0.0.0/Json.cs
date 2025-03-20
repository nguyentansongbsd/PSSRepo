// Decompiled with JetBrains decompiler
// Type: BSDLibrary.Json
// Assembly: Action_SignedContract, Version=1.0.0.0, Culture=neutral, PublicKeyToken=91af1975bd46f505
// MVID: 64A057F8-04D7-4937-A84E-D4EF3DDC89DB
// Assembly location: C:\Users\ngoct\Downloads\Action_SignedContract_1.0.0.0.dll

using System.Collections;
using System.Collections.Generic;

namespace BSDLibrary
{
  public class Json
  {
    public string createJSonString(Dictionary<string, string> arr)
    {
      ArrayList arrayList = new ArrayList();
      foreach (KeyValuePair<string, string> keyValuePair in arr)
        arrayList.Add((object) ("\"" + keyValuePair.Key + "\":\"" + keyValuePair.Value + "\""));
      return "{" + string.Join(",", arrayList.ToArray()) + "}";
    }
  }
}
