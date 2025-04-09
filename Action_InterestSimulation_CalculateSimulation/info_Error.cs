using Microsoft.Xrm.Sdk;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Action_InterestSimulation_CalculateSimulation
{
    public class info_Error
    {
        public int index { get; set; }
        public string message { get; set; }

        public string Error_Data_Null = "Data Null";
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
       

        /// <summary>
        /// Add thêm nội dung thông báo
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public string createMessageAdd(string msg)
        {
            return this.message += "\n " + msg;
        }

        /// <summary>
        /// New mới nội dung thông báo
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public string createMessageNew(string msg)
        {
            return this.message = "\n " + msg;
        }

        /// <summary>
        /// Xóa nội dung thông báo
        /// </summary>
        /// <returns></returns>
        public string clearMessage()
        {
            return this.message = "";
        }

        public string JSONSerialize<T>(T obj)
        {
            string retVal = String.Empty;
            using (System.IO.MemoryStream ms = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                serializer.WriteObject(ms, obj);
                var byteArray = ms.ToArray();
                retVal = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
            }
            return retVal;
        }
    }
}
