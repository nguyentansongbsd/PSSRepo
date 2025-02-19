// Decompiled with JetBrains decompiler
// Type: Plugin_CollectionMeeting_GenerateTermination.info_Error
// Assembly: Plugin_CollectionMeeting_GenerateTermination, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f7afacec0aa430c5
// MVID: 48B5B8C3-1D78-484D-B78A-C63DDA7C8A96
// Assembly location: C:\Users\ngoct\Downloads\Plugin_CollectionMeeting_GenerateTermination_1.0.0.0.dll

using Microsoft.Xrm.Sdk;

namespace Plugin_CollectionMeeting_GenerateTermination2
{
  public class info_Error
  {
    public string Error_Data_Null = "Data Null";

    public int index { get; set; }

    public string message { get; set; }

    public int count { get; set; }

    public bool result { get; set; }

    public EntityCollection entc { get; set; }

    public Entity ent_First { get; set; }

    public info_Error()
    {
      this.index = 0;
      this.message = "";
      this.entc = new EntityCollection();
      this.ent_First = new Entity();
      this.count = 0;
      this.result = false;
    }

    public string createMessageAdd(string msg) => this.message = this.message + "\n " + msg;

    public string createMessageNew(string msg) => this.message = "\n " + msg;

    public string clearMessage() => this.message = "";
  }
}
