//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Langben.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class SysEmail
    {
        public string Id { get; set; }
        public string SysMailId { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Reply_email { get; set; }
        public string Mail_type { get; set; }
        public string Remark { get; set; }
        public string State { get; set; }
        public Nullable<System.DateTime> ReadTime { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string CreatePerson { get; set; }
    
        public virtual SysEmailTemp SysEmailTemp { get; set; }
    }
}
