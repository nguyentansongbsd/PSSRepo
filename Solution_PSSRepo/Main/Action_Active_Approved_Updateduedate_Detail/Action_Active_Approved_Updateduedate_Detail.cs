using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Action này là trigger. Khi Approve 1 Action, nếu người dùng thay đổi Due date thì cần cập nhật lại trường Due date ở Action Detail tương ứng.

namespace Action_Active_Approved_Updateduedate_Detail
{
    public class Action_Active_Approved_Updateduedate_Detail : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
        }
    }
}
