// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess.SequenceStepType
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.Core.DataAccess
{
  public enum SequenceStepType
  {
    SimpleCondition = 1,
    AutomatedEmail = 3,
    AutoAction = 4,
    LinkedInAction = 5,
    AutomatedSMS = 6,
    Appointment = 4201, // 0x00001069
    Email = 4202, // 0x0000106A
    PhoneCall = 4210, // 0x00001072
    Task = 4212, // 0x00001074
    SMS = 4213, // 0x00001075
  }
}
