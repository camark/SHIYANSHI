﻿using System;
using System.Collections.Generic;
using System.Linq;

using Common;
using Langben.DAL;
using System.ServiceModel;

namespace Langben.IBLL
{
    /// <summary>
    /// 预备方案 接口
    /// </summary>
    public partial interface IPREPARE_SCHEMEBLL
    {
        /// <summary>
        /// 获取证书编号
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        string GetSerialNumber(string id);
        /// <summary>
        /// 修改编号
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        bool UPTSerialNumber(string id);
        /// <summary>
        /// 报告生成发送审核
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        bool EditField1(ref Common.ValidationErrors validationErrors, PREPARE_SCHEME entity, string CreatePerson = "");
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        bool EditField(ref Common.ValidationErrors validationErrors, PREPARE_SCHEME entity);

        /// <summary>
        /// 建立方案保存下一步
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        bool EditInst(ref Common.ValidationErrors validationErrors, PREPARE_SCHEME entity);
        /// <summary>
        /// 获取在该表一条数据中，出现的所有外键实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        List<METERING_STANDARD_DEVICE> GetRefMETERING_STANDARD_DEVICE(string id);
    }
}

