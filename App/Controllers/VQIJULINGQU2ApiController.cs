﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Text;
using System.EnterpriseServices;
using System.Configuration;
using Models;
using Common;
using Langben.DAL;
using Langben.BLL;
using System.Web.Http;
using Langben.App.Models;

namespace Langben.App.Controllers
{
    /// <summary>
    /// 器具领取2
    /// </summary>
    public class VQIJULINGQU2ApiController : BaseApiController
    {
        /// <summary>
        /// 异步加载数据
        /// </summary>
        /// <param name="getParam"></param>
        /// <returns></returns>
        public Common.ClientResult.DataResult PostData([FromBody]GetDataParam getParam)
        {
            int total = 0;
            List<VQIJULINGQU2> queryData = m_BLL.GetByParam(getParam.id, getParam.page, getParam.rows, getParam.order, getParam.sort, getParam.search, ref total);
            var data = new Common.ClientResult.DataResult
            {
                total = total,
                rows = queryData.Select(s => new
                {
                    ID = s.ID
                    ,
                    APPLIANCE_NAME = s.APPLIANCE_NAME
                    ,
                    VERSION = s.VERSION
                    ,
                    FACTORY_NUM = s.FACTORY_NUM
                    ,
                    NUM = s.NUM
                    ,
                    ATTACHMENT = s.ATTACHMENT
                    ,
                    UNDERTAKE_LABORATORYID = s.UNDERTAKE_LABORATORYID
                    ,
                    APPLIANCE_RECIVE = s.APPLIANCE_RECIVE
                    ,
                    REPORTNUMBER = s.REPORTNUMBER
                    ,
                    REMARKS = s.REMARKS
                    ,
                    ORDER_TASK_INFORMATIONID = s.ORDER_TASK_INFORMATIONID


                })
            };
            return data;
        }

        /// <summary>
        /// 根据ID获取数据模型
        /// </summary>
        /// <param name="id">编号</param>
        /// <returns></returns>
        public VQIJULINGQU2 Get(string id)
        {
            VQIJULINGQU2 item = m_BLL.GetById(id);
            return item;
        }

        // PUT api/<controller>/5
        /// <summary>
        /// 报告，器具领取
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>  
        [System.Web.Http.HttpPut]
        public Common.ClientResult.Result Put(string baogaoid, string qijuid, string yemian)
        {
            Common.ClientResult.Result result = new Common.ClientResult.Result();
            if (baogaoid != null || qijuid != null )
            {   //数据校验          
                string currentPerson = GetCurrentPerson();
                string returnValue = string.Empty;
                #region 报告领取操作
                if (!string.IsNullOrWhiteSpace(baogaoid))
                {
                    foreach (var item in baogaoid.Split('|'))
                    {
                        
                        PREPARE_SCHEME prep = new PREPARE_SCHEME();//预备方案
                        if (!string.IsNullOrEmpty(item))
                        {
                           
                            prep.ID = item.Split('~')[0];
                            prep.REPORTSTATUS = Common.REPORTSTATUS.报告已领取.ToString();//报告领取状态
                            prep.REPORTSTATUSZI = Common.REPORTSTATUS.报告已领取.GetHashCode().ToString();//报告领取状态
                            if ( m_BLL5.EditField(ref validationErrors, prep))
                            {      
                                result.Code = Common.ClientCode.Succeed;
                                result.Message = Suggestion.InsertSucceed;
                            }
                            else
                            {
                                if (validationErrors != null && validationErrors.Count > 0)
                                {
                                    validationErrors.All(a =>
                                    {
                                        returnValue += a.ErrorMessage;
                                        return true;
                                    });
                                }
                                result.Code = Common.ClientCode.Fail;
                                result.Message = Suggestion.InsertFail + returnValue;
                                return result; //提示创建失败
                            }
                        }
                    }
                }
                #endregion
                #region 器具领取操作
                if (!string.IsNullOrWhiteSpace(qijuid))
                {
                    foreach (var item in qijuid.Split('|'))
                    {
                    
                        APPLIANCE_LABORATORY appry = new APPLIANCE_LABORATORY();//器具明细信息_承接实验室
                        APPLIANCE_DETAIL_INFORMATION appion = new APPLIANCE_DETAIL_INFORMATION();//器具明细
                        if (!string.IsNullOrEmpty(item))
                        {
                           
                            appion.APPLIANCE_PROGRESS = null;//所在实验室
                            appion.ID = item.Split('~')[0];//id
                            if (!m_BLL6.EditField(ref validationErrors, appion))//修改器具所在实验室数据
                            {
                                LogClassModels.WriteServiceLog(Suggestion.UpdateSucceed + "，器具明细信息的Id为" + appion.ID, "器具领取");//写入日志       
                                result.Code = Common.ClientCode.Succeed;
                                result.Message = Suggestion.UpdateFail;
                                return result; //提示创建失败
                            }
               
                            List<APPLIANCE_LABORATORY> list = m_BLL4.GetByRefAPPLIANCE_DETAIL_INFORMATIOID(item.Split('~')[0]);
                            foreach (var item2 in list)
                            {
                                appry.ID = item2.ID;
                                appry.ORDER_STATUS = Common.ORDER_STATUS.器具已领取.ToString();
                                appry.EQUIPMENT_STATUS_VALUUMN = Common.ORDER_STATUS.器具已领取.GetHashCode().ToString();
                                if (!m_BLL4.EditField(ref validationErrors, appry))
                                {
                                    LogClassModels.WriteServiceLog(Suggestion.UpdateSucceed + "，器具明细信息_承接实验室的Id为" + appry.ID, "器具领取");//写入日志  
                                    result.Code = Common.ClientCode.Succeed;
                                    result.Message = Suggestion.UpdateFail;
                                    return result; //提示创建失败
                                }
                            }
                           
                        }
                    }
                }
                #endregion
                result.Code = Common.ClientCode.Succeed;
                result.Message = Suggestion.InsertSucceed;
                return result;
            }
            result.Code = Common.ClientCode.FindNull;
            result.Message = Suggestion.InsertFail + "请核对输入的数据的格式";
            return result; //提示输入的数据的格式不对         
        }

        // PUT api/<controller>/5
        /// <summary>
        ///报告回收
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>  
        [System.Web.Http.HttpPut]
        public Common.ClientResult.Result BaoGaoHuiShou(string id)
        {
            Common.ClientResult.OrderTaskGong result = new Common.ClientResult.OrderTaskGong();
            if (id != null && ModelState.IsValid)
            {   //数据校验          
                string currentPerson = GetCurrentPerson();
                string returnValue = string.Empty;
                PREPARE_SCHEME prep = new PREPARE_SCHEME();//预备方案
                prep.ID = id;
                prep.REPORTSTATUS = Common.REPORTSTATUS.报告已回收.ToString();//报告回收状态
                prep.REPORTSTATUSZI = Common.REPORTSTATUS.报告已回收.GetHashCode().ToString();//报告回收状态
                if (m_BLL5.EditField(ref validationErrors, prep))
                {
                    LogClassModels.WriteServiceLog(Suggestion.UpdateSucceed + "，报告回收信息的Id为" + prep.ID, "报告回收");//写入日志        
                    result.Code = Common.ClientCode.Succeed;
                    result.Message = Suggestion.InsertSucceed;
                    result.Id = prep.ID;
                    return result;
                }
                else
                {
                    if (validationErrors != null && validationErrors.Count > 0)
                    {
                        validationErrors.All(a =>
                        {
                            returnValue += a.ErrorMessage;
                            return true;
                        });
                    }
                    LogClassModels.WriteServiceLog(Suggestion.UpdateFail + "，报告回收信息的Id为" + prep.ID + "," + returnValue, "报告回收");//写入日志
                    result.Code = Common.ClientCode.Fail;
                    result.Message = Suggestion.InsertFail + returnValue;
                    return result; //提示创建失败
                }
            }
            result.Code = Common.ClientCode.FindNull;
            result.Message = Suggestion.InsertFail + "请核对输入的数据的格式";
            return result; //提示输入的数据的格式不对         
        }
        IBLL.IVQIJULINGQU2BLL m_BLL;
        IBLL.IAPPLIANCECOLLECTIONBLL m_BLL2;
        IBLL.IREPORTCOLLECTIONBLL m_BLL3;
        IBLL.IAPPLIANCE_LABORATORYBLL m_BLL4;
        IBLL.IPREPARE_SCHEMEBLL m_BLL5;
        IBLL.IAPPLIANCE_DETAIL_INFORMATIONBLL m_BLL6;

        ValidationErrors validationErrors = new ValidationErrors();

        public VQIJULINGQU2ApiController()
            : this(new VQIJULINGQU2BLL(), new APPLIANCECOLLECTIONBLL(), new REPORTCOLLECTIONBLL(), new APPLIANCE_LABORATORYBLL(), new PREPARE_SCHEMEBLL(), new APPLIANCE_DETAIL_INFORMATIONBLL()) { }

        public VQIJULINGQU2ApiController(VQIJULINGQU2BLL bll, APPLIANCECOLLECTIONBLL bll2, REPORTCOLLECTIONBLL bll3, APPLIANCE_LABORATORYBLL bll4, PREPARE_SCHEMEBLL bll5, APPLIANCE_DETAIL_INFORMATIONBLL bll6)
        {
            m_BLL = bll;
            m_BLL2 = bll2;
            m_BLL3 = bll3;
            m_BLL4 = bll4;
            m_BLL5 = bll5;
            m_BLL6 = bll6;
        }

    }
}


