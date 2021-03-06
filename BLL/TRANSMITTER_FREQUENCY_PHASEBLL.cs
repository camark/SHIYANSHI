﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Langben.DAL;
using Common;

namespace Langben.BLL
{
    /// <summary>
    /// 变送器频率相位 
    /// </summary>
    public partial class TRANSMITTER_FREQUENCY_PHASEBLL :  IBLL.ITRANSMITTER_FREQUENCY_PHASEBLL, IDisposable
    {
        /// <summary>
        /// 私有的数据访问上下文
        /// </summary>
        protected SysEntities db;
        /// <summary>
        /// 变送器频率相位的数据库访问对象
        /// </summary>
        TRANSMITTER_FREQUENCY_PHASERepository repository = new TRANSMITTER_FREQUENCY_PHASERepository();
        /// <summary>
        /// 构造函数，默认加载数据访问上下文
        /// </summary>
        public TRANSMITTER_FREQUENCY_PHASEBLL()
        {
            db = new SysEntities();
        }
        /// <summary>
        /// 已有数据访问上下文的方法中调用
        /// </summary>
        /// <param name="entities">数据访问上下文</param>
        public TRANSMITTER_FREQUENCY_PHASEBLL(SysEntities entities)
        {
            db = entities;
        }
        /// <summary>
        /// 查询的数据
        /// </summary>
        /// <param name="id">额外的参数</param>
        /// <param name="page">页码</param>
        /// <param name="rows">每页显示的行数</param>
        /// <param name="order">升序asc（默认）还是降序desc</param>
        /// <param name="sort">排序字段</param>
        /// <param name="search">查询条件</param>
        /// <param name="total">结果集的总数</param>
        /// <returns>结果集</returns>
        public List<TRANSMITTER_FREQUENCY_PHASE> GetByParam(string id, int page, int rows, string order, string sort, string search, ref int total)
        {
            IQueryable<TRANSMITTER_FREQUENCY_PHASE> queryData = repository.GetData(db, order, sort, search);
            total = queryData.Count();
            if (total > 0)
            {
                if (page <= 1)
                {
                    queryData = queryData.Take(rows);
                }
                else
                {
                    queryData = queryData.Skip((page - 1) * rows).Take(rows);
                }
                
                    foreach (var item in queryData)
                    {
                        if (item.TRANSMITTER_HEADID != null && item.TRANSMITTER_HEAD != null)
                        { 
                                item.TRANSMITTER_HEADIDOld = item.TRANSMITTER_HEAD.类型.GetString();//                            
                        }                  

                    }
 
            }
            return queryData.ToList();
        }
        /// <summary>
        /// 查询的数据 /*在6.0版本中 新增*/
        /// </summary>
        /// <param name="id">额外的参数</param>
        /// <param name="page">页码</param>
        /// <param name="rows">每页显示的行数</param>
        /// <param name="order">升序asc（默认）还是降序desc</param>
        /// <param name="sort">排序字段</param>
        /// <param name="search">查询条件</param>
        /// <param name="total">结果集的总数</param>
        /// <returns>结果集</returns>
        public List<TRANSMITTER_FREQUENCY_PHASE> GetByParam(string id, string order, string sort, string search)
        {
            IQueryable<TRANSMITTER_FREQUENCY_PHASE> queryData = repository.GetData(db, order, sort, search);
            
            return queryData.ToList();
        }
        /// <summary>
        /// 创建一个变送器频率相位
        /// </summary>
        /// <param name="validationErrors">返回的错误信息</param>
        /// <param name="db">数据库上下文</param>
        /// <param name="entity">一个变送器频率相位</param>
        /// <returns></returns>
        public bool Create(ref ValidationErrors validationErrors, TRANSMITTER_FREQUENCY_PHASE entity)
        {
            try
            {
                repository.Create(entity);
                return true;
            }
            catch (Exception ex)
            {
                validationErrors.Add(ex.Message);
                ExceptionsHander.WriteExceptions(ex);                
            }
            return false;
        }
        /// <summary>
        ///  创建变送器频率相位集合
        /// </summary>
        /// <param name="validationErrors">返回的错误信息</param>
        /// <param name="entitys">变送器频率相位集合</param>
        /// <returns></returns>
        public bool CreateCollection(ref ValidationErrors validationErrors, IQueryable<TRANSMITTER_FREQUENCY_PHASE> entitys)
        {
            try
            {
                if (entitys != null)
                {
                    int count = entitys.Count();
                    if (count == 1)
                    {
                        return this.Create(ref validationErrors, entitys.FirstOrDefault());
                    }
                    else if (count > 1)
                    {
                        //using (TransactionScope transactionScope = new TransactionScope())
                        { 
                            repository.Create(db, entitys);
                            if (count == repository.Save(db))
                            {
                                //transactionScope.Complete();
                                return true;
                            }
                            else
                            {
                                //Transaction.Current.Rollback();
                            }                          
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                validationErrors.Add(ex.Message);
                ExceptionsHander.WriteExceptions(ex);                
            }
            return false;
        }
        /// <summary>
        /// 删除一个变送器频率相位
        /// </summary>
        /// <param name="validationErrors">返回的错误信息</param>
        /// <param name="id">一变送器频率相位的主键</param>
        /// <returns></returns>  
        public bool Delete(ref ValidationErrors validationErrors, string id)
        {
            try
            {
                return repository.Delete(id) == 1;
            }
            catch (Exception ex)
            {
                validationErrors.Add(ex.Message);
                ExceptionsHander.WriteExceptions(ex);                
            }
            return false;
        }
        /// <summary>
        /// 删除变送器频率相位集合
        /// </summary>
        /// <param name="validationErrors">返回的错误信息</param>
        /// <param name="deleteCollection">变送器频率相位的集合</param>
        /// <returns></returns>    
        public bool DeleteCollection(ref ValidationErrors validationErrors, string[] deleteCollection)
        {
            try
            {
                if (deleteCollection != null)
                { 
                        //using (TransactionScope transactionScope = new TransactionScope())
                        { 
                            repository.Delete(db, deleteCollection);
                            if (deleteCollection.Length == repository.Save(db))
                            {
                                //transactionScope.Complete();
                                return true;
                            }
                            else
                            {
                                //Transaction.Current.Rollback();
                            }
                        }
                    }
             
            }
            catch (Exception ex)
            {
                validationErrors.Add(ex.Message);
                ExceptionsHander.WriteExceptions(ex);                
            }
            return false;
        }
        /// <summary>
        ///  创建变送器频率相位集合
        /// </summary>
        /// <param name="validationErrors">返回的错误信息</param>
        /// <param name="entitys">变送器频率相位集合</param>
        /// <returns></returns>
        public bool EditCollection(ref ValidationErrors validationErrors, IQueryable<TRANSMITTER_FREQUENCY_PHASE> entitys)
        {
            try
            {
                if (entitys != null)
                {
                    int count = entitys.Count();
                    if (count == 1)
                    {
                        return this.Edit(ref validationErrors, entitys.FirstOrDefault());
                    }
                    else if (count > 1)
                    {
                        //using (TransactionScope transactionScope = new TransactionScope())
                        { 
                            repository.Edit(db, entitys);
                            if (count == repository.Save(db))
                            {
                                //transactionScope.Complete();
                                return true;
                            }
                            else
                            {
                                //Transaction.Current.Rollback();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                validationErrors.Add(ex.Message);
                ExceptionsHander.WriteExceptions(ex);                
            }
            return false;
        }
         /// <summary>
        /// 编辑一个变送器频率相位
        /// </summary>
        /// <param name="validationErrors">返回的错误信息</param>
        /// <param name="entity">一个变送器频率相位</param>
        /// <returns></returns>
        public bool Edit(ref ValidationErrors validationErrors, TRANSMITTER_FREQUENCY_PHASE entity)
        {
            try
            { 
                repository.Edit(db, entity);
                repository.Save(db);
                return true;
            }
            catch (Exception ex)
            {
                validationErrors.Add(ex.Message);
                ExceptionsHander.WriteExceptions(ex);                
            }
            return false;
        }
      
        public List<TRANSMITTER_FREQUENCY_PHASE> GetAll()
        {           
            return repository.GetAll(db).ToList();          
        }   
        
        /// <summary>
        /// 根据主键获取一个变送器频率相位
        /// </summary>
        /// <param name="id">变送器频率相位的主键</param>
        /// <returns>一个变送器频率相位</returns>
        public TRANSMITTER_FREQUENCY_PHASE GetById(string id)
        {           
            return repository.GetById(db, id);           
        }


        /// <summary>
        /// 根据TRANSMITTER_HEADIDId，获取所有变送器频率相位数据
        /// </summary>
        /// <param name="id">外键的主键</param>
        /// <returns></returns>
        public List<TRANSMITTER_FREQUENCY_PHASE> GetByRefTRANSMITTER_HEADID(string id)
        {
            return repository.GetByRefTRANSMITTER_HEADID(db, id).ToList();                      
        }

        public void Dispose()
        {
           
        }
    }
}

