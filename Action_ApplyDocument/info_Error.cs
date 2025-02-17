using Microsoft.Xrm.Sdk;

namespace Action_ApplyDocument
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
