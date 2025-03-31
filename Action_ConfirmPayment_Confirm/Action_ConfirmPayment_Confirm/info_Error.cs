using Microsoft.Xrm.Sdk;

namespace Action_ConfirmPayment_Confirm
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
    }
}
