// Decompiled with JetBrains decompiler
// Type: Newtonsoft.Json.Converters.DataSetConverter
// Assembly: Microsoft.Dynamics.AcceleratedSales.SandboxPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 7BBE7132-7E30-4424-B38D-27667CFBCEFA
// Assembly location: C:\Users\ngoct\Downloads\New folder\Microsoft.Dynamics.AcceleratedSales.SandboxPlugins_1.0.0.0.dll

using Newtonsoft.Json.Serialization;
using System;
using System.Data;

#nullable enable
namespace Newtonsoft.Json.Converters
{
  internal class DataSetConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
      if (value == null)
      {
        writer.WriteNull();
      }
      else
      {
        DataSet dataSet = (DataSet) value;
        DefaultContractResolver contractResolver = serializer.ContractResolver as DefaultContractResolver;
        DataTableConverter dataTableConverter = new DataTableConverter();
        writer.WriteStartObject();
        foreach (DataTable table in (InternalDataCollectionBase) dataSet.Tables)
        {
          writer.WritePropertyName(contractResolver != null ? contractResolver.GetResolvedPropertyName(table.TableName) : table.TableName);
          dataTableConverter.WriteJson(writer, (object) table, serializer);
        }
        writer.WriteEndObject();
      }
    }

    public override object? ReadJson(
      JsonReader reader,
      Type objectType,
      object? existingValue,
      JsonSerializer serializer)
    {
      if (reader.TokenType == JsonToken.Null)
        return (object) null;
      DataSet dataSet = objectType == typeof (DataSet) ? new DataSet() : (DataSet) Activator.CreateInstance(objectType);
      DataTableConverter dataTableConverter = new DataTableConverter();
      reader.ReadAndAssert();
      while (reader.TokenType == JsonToken.PropertyName)
      {
        DataTable table1 = dataSet.Tables[(string) reader.Value];
        int num = table1 != null ? 1 : 0;
        DataTable table2 = (DataTable) dataTableConverter.ReadJson(reader, typeof (DataTable), (object) table1, serializer);
        if (num == 0)
          dataSet.Tables.Add(table2);
        reader.ReadAndAssert();
      }
      return (object) dataSet;
    }

    public override bool CanConvert(Type valueType) => typeof (DataSet).IsAssignableFrom(valueType);
  }
}
