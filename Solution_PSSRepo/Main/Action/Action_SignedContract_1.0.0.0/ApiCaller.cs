// Decompiled with JetBrains decompiler
// Type: BSDLibrary.ApiCaller
// Assembly: Action_SignedContract, Version=1.0.0.0, Culture=neutral, PublicKeyToken=91af1975bd46f505
// MVID: 64A057F8-04D7-4937-A84E-D4EF3DDC89DB
// Assembly location: C:\Users\ngoct\Downloads\Action_SignedContract_1.0.0.0.dll

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace BSDLibrary
{
  public class ApiCaller
  {
    public string postJson(string link, Dictionary<string, string> dataPost)
    {
      HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(link);
      byte[] bytes = Encoding.UTF8.GetBytes(new Json().createJSonString(dataPost));
      httpWebRequest.Method = "POST";
      httpWebRequest.ProtocolVersion = HttpVersion.Version10;
      httpWebRequest.ContentType = "application/json; charset=UTF-8";
      httpWebRequest.Accept = "application/json";
      httpWebRequest.ContentLength = (long) bytes.Length;
      using (Stream requestStream = httpWebRequest.GetRequestStream())
        requestStream.Write(bytes, 0, bytes.Length);
      HttpWebResponse response = (HttpWebResponse) httpWebRequest.GetResponse();
      string end = new StreamReader(response.GetResponseStream()).ReadToEnd();
      response.Close();
      return end;
    }

    public string get(string link)
    {
      HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(link);
      httpWebRequest.Method = "GET";
      WebResponse response = httpWebRequest.GetResponse();
      string end = new StreamReader(response.GetResponseStream()).ReadToEnd();
      response.Close();
      return end.Trim('"');
    }
  }
}
