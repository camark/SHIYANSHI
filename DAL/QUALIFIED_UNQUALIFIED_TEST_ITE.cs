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
    
    public partial class QUALIFIED_UNQUALIFIED_TEST_ITE
    {
        public string ID { get; set; }
        public string CONCLUSION { get; set; }
        public string PREPARE_SCHEMEID { get; set; }
        public string RULEID { get; set; }
        public string RULENAME { get; set; }
        public string RULENJOINAME { get; set; }
        public string HTMLVALUE { get; set; }
        public string REMARK { get; set; }
        public Nullable<System.DateTime> CREATETIME { get; set; }
        public string CREATEPERSON { get; set; }
        public Nullable<System.DateTime> UPDATETIME { get; set; }
        public string UPDATEPERSON { get; set; }
        public Nullable<decimal> SORT { get; set; }
        public string INPUTSTATE { get; set; }
    
        public virtual PREPARE_SCHEME PREPARE_SCHEME { get; set; }
    }
}
