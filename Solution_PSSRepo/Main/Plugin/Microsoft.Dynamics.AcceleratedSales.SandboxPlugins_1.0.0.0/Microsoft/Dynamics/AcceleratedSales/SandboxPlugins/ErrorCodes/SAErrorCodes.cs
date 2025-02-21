// Decompiled with JetBrains decompiler
// Type: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.ErrorCodes.SAErrorCodes
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using System.Diagnostics.CodeAnalysis;

#nullable disable
namespace Microsoft.Dynamics.AcceleratedSales.SandboxPlugins.ErrorCodes
{
  [ExcludeFromCodeCoverage]
  public static class SAErrorCodes
  {
    public const int MissingViewId = 1879441410;
    public const int MissingEntityName = 1879441411;
    public const int MissingFetchXml = 1879441412;
    public const int MissingEntityId = 1879441413;
    public const int MissingAdminConfig = 1879441414;
    public const int MissingUserConfig = 1879441415;
    public const int ConvertQueryExpressionTotFetchXmlFailure = 1879506962;
    public const int GetLayoutXmlFailure = 1879506963;
    public const int GetWorklistDataApiFailure = 1879506947;
    public const int FetchRecordsFailure = 1879506948;
    public const int SASettingsFetchFailure = 1879506949;
    public const int ConvertFetchXmlToQueryExpressionFailure = 1879506950;
    public const int IsStringNullOrEmptyErrorCode = 1879506951;
    public const int GetWorklistDataRequestPayloadParsingFailure = 1879506952;
    public const int UnknownErrorCode = 1879506953;
    public const int GetWorklistSettingsDataRequestPayloadParsingFailure = 1879506960;
    public const int UpdateWorklistSettingsDataRequestPayloadParsingFailure = 1879506961;
    public const int GetMergedViewXmlApiFailure = 1879506964;
    public const int GetValidAttributeValueError = 1879506951;
    public const int InvalidTargetEntity = -2138046461;
    public const int FeatureFCBNotEnabled = -2137980928;
    public const int AttributeMissingInEntityConfiguration = -2137980927;
    public const int RetrieveNotSupported = -2138046455;
    public const int EntityReferenceIsNull = -2137980921;
    public const int PermissionDeniedError = -2147220960;
    public const int PrimaryEntityFetchRecordsFailure = 1879506965;
    public const int QueryBuilderNoAttribute = -2147217149;
  }
}
