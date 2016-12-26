﻿using System.Collections.Generic;

namespace Langben.Report
{
    ///// <summary>
    ///// 所有通道数据（便于扩展)
    ///// </summary>
    //public class ALLData
    //{
    //    /// <summary>
    //    /// int:通道编号、OneData：一个通道的所有数据
    //    /// </summary>
    //    public Dictionary<int, OneData> data{get;set;}        
    //}

    ///// <summary>
    ///// 一个通道的所有数据（便于扩展)
    ///// </summary>
    //public class OneData
    //{        
    //    /// <summary>
    //    /// 表头数据
    //    /// </summary>
    //    public List<MYDataHead> DataHead { get; set; }
    //    /// <summary>
    //    /// 表数据
    //    /// </summary>
    //     public DataValue Data { get; set; }
    //}
    public class MYDataHead
    {
        public string id { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }
    /// <summary>
    /// id name value hangshu(合并几行单元格) 通道 量程 行号 第几列
    /// </summary>
    public class MYData
    {
        public string id { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        /// <summary>
        /// 合并几行单元格
        /// </summary>
        public int mergedRowNum { get; set; }
        /// <summary>
        /// 行号
        /// </summary>
        public int rowNum { get; set; }
        /// <summary>
        /// 第几列
        /// </summary>
        public int columnNum { get; set; }
    }
}
