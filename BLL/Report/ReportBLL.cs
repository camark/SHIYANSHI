﻿using Langben.DAL;
using Langben.DAL.shiyanshi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

//using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.HSSF.Util;
using NPOI.POIFS.FileSystem;
//using NPOI.SS.UserModel;
 
using Langben.BLL;
using System.IO;
using Langben.IBLL;
using Common;

namespace Langben.Report
{
    /// <summary>
    /// 报告业务逻辑
    /// </summary>
    public partial class  ReportBLL
    {
        /// <summary>
        /// 模板中所有的合并的单元格
        /// </summary>
        Dictionary<string, List<CellRangeAddress>> returnList = new Dictionary<string, List<CellRangeAddress>>();
        /// <summary>
        ///  获取合并区域信息
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private Dictionary<string, List<CellRangeAddress>> GetMergedCellRegion(ISheet sheet)
        {
            if (!returnList.ContainsKey(sheet.SheetName))
            {
                int mergedRegionCellCount = sheet.NumMergedRegions;
                List<CellRangeAddress> cellList = new List<CellRangeAddress>();

                for (int i = 0; i < mergedRegionCellCount; i++)
                {
                    cellList.Add(sheet.GetMergedRegion(i));
                }
                returnList.Add(sheet.SheetName, cellList);

            }


            return returnList;
        }
        /// <summary>
        /// 获取合并单元格的坐标范围
        /// </summary>
        /// <param name="sheet">sheet</param>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndex"></param> 
        /// <returns>合并单元格的范围</returns>
        public CellRangeAddress getMergedRegionCell(ISheet sheet, int columnIndex, int rowIndex)
        {
            List<CellRangeAddress> result = null;
            if (!returnList.ContainsKey(sheet.SheetName))
            {
                GetMergedCellRegion(sheet);

            }
            result = returnList[sheet.SheetName];
            return (from c in result
                    where columnIndex >= c.FirstColumn && columnIndex <= c.LastColumn && rowIndex >= c.FirstRow && rowIndex <= c.LastRow
                    select c).FirstOrDefault();

        }

        /// <summary>
        /// 获取特殊字符配置信息
        /// </summary>
        /// <returns></returns>
        public SpecialCharacters GetSpecialCharacters()
        {
            SpecialCharacters result = null;
            if (ReportStatic.SpecialCharacterXml != null && ReportStatic.SpecialCharacterXml.Trim() != "")
            {
                result = SpecialCharacters.XmlCovertObj(ReportStatic.SpecialCharacterXml);
            }
            return result;
        }
        /// <summary>
        /// 获取所有报告配置信息
        /// </summary>
        /// <param name="type">报告类型</param>
        /// <returns></returns>
        public TableTemplates GetTableTemplates(ExportType type = ExportType.OriginalRecord_JianDing)
        {
            TableTemplates result = null;
            if (ReportStatic.TableTemplateXml != null && ReportStatic.TableTemplateXml.Trim() != "")
            {
                switch (type)
                {
                    case ExportType.OriginalRecord_JianDing:
                    case ExportType.OriginalRecord_XiaoZhun:
                        result = TableTemplates.XmlCovertObj(ReportStatic.TableTemplateXml);
                        break;
                    case ExportType.Report_JianDing:
                        result = TableTemplates.XmlCovertObj(ReportStatic.TableTemplate_JianDingXml);
                        break;
                    case ExportType.Report_XiaoZhun:
                        result = TableTemplates.XmlCovertObj(ReportStatic.TableTemplate_JiaoZhunXml);
                        break;
                    case ExportType.Report_XiaoZhun_CNAS:
                        result = TableTemplates.XmlCovertObj(ReportStatic.TableTemplate_JiaoZhunXml);
                        break;
                    default:
                        result = TableTemplates.XmlCovertObj(ReportStatic.TableTemplateXml);
                        break;

                }

            }
            return result;
        }


        /// <summary>
        /// 导出原始记录及报告并保存路径
        /// </summary>
        /// <param name="ID">预备方案ID</param>
        /// <param name="Message">返回信息</param>
        /// <param name="Person">操作人</param>
        /// <returns></returns>
        public bool ExportAndSavePath(string ID, out string Message, string Person = "")
        {

            ValidationErrors validationErrors = new ValidationErrors();
            //fBll.Create(ref validationErrors, fEntity);       
            FILE_UPLOADERBLL fBll = new FILE_UPLOADERBLL();
            FILE_UPLOADER oEntity = null;
            FILE_UPLOADER rEntity = null;
            FILE_UPLOADER Entity = null;
            bool oSuccess = ExportOriginalRecord(ID, out Message, out oEntity);
            if (oSuccess)
            {
                Entity = oEntity;
            }
            bool rSuccess = ExportReport(ID, out Message, out rEntity);
            if (!oSuccess && rSuccess)
            {
                Entity = rEntity;
            }
            else if (oSuccess && rSuccess)
            {
                Entity.PATH = rEntity.PATH;
                Entity.FULLPATH = rEntity.FULLPATH;
                Entity.NAME = rEntity.NAME;
                Entity.SUFFIX = rEntity.SUFFIX;
                Entity.STATE = rEntity.STATE;
            }
            if (Entity != null)
            {
                Entity.CREATEPERSON = Person;
                fBll.Create(ref validationErrors, Entity);
                return true;
            }
            return false;

        }
        /// <summary>
        /// 导出报告Excel
        /// </summary>
        /// <param name="ID">预备方案ID</param>
        /// <param name="Message">返回消息</param>
        /// <param name="CreatePerson">创建人</param>
        /// <returns></returns>
        public bool ExportReport(string ID, out string Message, out FILE_UPLOADER fEntity)
        {
            fEntity = new FILE_UPLOADER();
            IBLL.IPREPARE_SCHEMEBLL m_BLL = new PREPARE_SCHEMEBLL();
            PREPARE_SCHEME entity = m_BLL.GetById(ID);
            string saveFileName = "";
            if (entity != null)
            {
                ExportType type = GetExportType(entity, "Report");
                string xlsPath = GetTemplatePath(type);


                HSSFWorkbook _book = new HSSFWorkbook();
                FileStream file = new FileStream(xlsPath, FileMode.Open, FileAccess.Read);
                IWorkbook hssfworkbook = new HSSFWorkbook(file);

                //设置封皮                
                if (type == ExportType.Report_JianDing)
                {
                    SetFengPi_BaoGaoJianDing(hssfworkbook, entity);
                }
                else
                {
                    SetFengPi_BaoGaoXiaoZhun(hssfworkbook, entity, type);
                }
                //设置数据

                //第二页数据
                SetSecond_BaoGao(hssfworkbook, entity, type);

                SetShuJu(hssfworkbook, entity, type);
                string fileName = SetFileName(type);
                //saveFileName = "../up/Report/" + entity.CERTIFICATE_CATEGORY + "_" + Result.GetNewId() + ".xls";
                saveFileName = "~/up/Report/" + fileName + ".xls";
                string saveFileNamePath = System.Web.HttpContext.Current.Server.MapPath(saveFileName);
                using (FileStream fileWrite = new FileStream(saveFileNamePath, FileMode.Create))
                {
                    hssfworkbook.Write(fileWrite);
                }
                Message = "../up/Report/" + fileName + ".xls";
                //if (IsSavePath)
                //{

                //FILE_UPLOADERBLL fBll = new FILE_UPLOADERBLL();
                //FILE_UPLOADER fEntity = new FILE_UPLOADER();                  

                fEntity.CONCLUSION = entity.CONCLUSION;
                fEntity.CREATETIME = DateTime.Now;
                fEntity.PATH = saveFileName;
                fEntity.FULLPATH = saveFileNamePath;
                fEntity.NAME = fileName;
                fEntity.SUFFIX = ".xls";
                fEntity.PREPARE_SCHEMEID = entity.ID;
                fEntity.STATE = "已上传";
                //fEntity.CREATEPERSON = CreatePerson;
                fEntity.ID = Result.GetNewId();
                //ValidationErrors validationErrors = new ValidationErrors();
                //fBll.Create(ref validationErrors, fEntity);
                //}


                return true;
            }
            Message = "未找到预备方案ID为【" + ID + "】的数据";
            return false;
        }
        private string GetTemplatePath(ExportType type = ExportType.OriginalRecord_JianDing)
        {
            string result = ReportStatic.YuanShiJiLuJianDingPath;
            switch (type)
            {
                case ExportType.OriginalRecord_JianDing:
                    result = ReportStatic.YuanShiJiLuJianDingPath;
                    break;
                case ExportType.OriginalRecord_XiaoZhun:
                    result = ReportStatic.YuanShiJiLuXiaoZhunPath;
                    break;
                case ExportType.Report_JianDing:
                    result = ReportStatic.BaoGaoJianDingPath;
                    break;
                case ExportType.Report_XiaoZhun:
                    result = ReportStatic.BaoGaoXiaoZhunPath;
                    break;
                case ExportType.Report_XiaoZhun_CNAS:
                    result = ReportStatic.BaoGaoXiaoZhunCNASPath;
                    break;
            }
            return result;

        }
        /// <summary>
        /// 设置文件名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string SetFileName(ExportType type = ExportType.OriginalRecord_JianDing)
        {
            string result = "检定原始记录";
            switch (type)
            {
                case ExportType.OriginalRecord_JianDing:
                    result = "检定原始记录";
                    break;
                case ExportType.OriginalRecord_XiaoZhun:
                    result = "校准原始记录";
                    break;
                case ExportType.Report_JianDing:
                    result = "检定报告";
                    break;
                case ExportType.Report_XiaoZhun:
                    result = "校准报告";
                    break;
                case ExportType.Report_XiaoZhun_CNAS:
                    result = "校准报告_CNAS";
                    break;
            }
            result = result + "_" + Result.GetNewId();
            return result;
        }
        /// <summary>
        /// 获取报告类型
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type">Report：报告、OriginalRecord：原始记录</param>
        /// <returns></returns>
        private ExportType GetExportType(PREPARE_SCHEME entity, string type = "Report")
        {
            ExportType result = ExportType.Report_JianDing;
            if (type == "Report")
            {
                if (entity.CERTIFICATE_CATEGORY == ZhengShuLeiBieEnums.校准.ToString())
                {
                    if (entity.CNAS != null && entity.CNAS.Trim() == ShiFouCNAS.Yes.ToString())
                    {
                        result = ExportType.Report_XiaoZhun_CNAS;
                    }
                    else
                    {
                        result = ExportType.Report_XiaoZhun;
                    }
                }
                else
                {
                    result = ExportType.Report_JianDing;
                }
            }
            else
            {
                if (entity.CERTIFICATE_CATEGORY == ZhengShuLeiBieEnums.校准.ToString())
                {
                    result = ExportType.OriginalRecord_XiaoZhun;
                }
                else
                {
                    result = ExportType.OriginalRecord_JianDing;
                }
            }
            return result;
        }
        /// <summary>
        /// 设置校准报告封皮信息
        /// </summary>
        /// <param name="hssfworkbook"></param>
        /// <param name="entity"></param>
        /// <param name="type">报告类型</param>
        private void SetFengPi_BaoGaoXiaoZhun(IWorkbook hssfworkbook, PREPARE_SCHEME entity, ExportType type = ExportType.Report_XiaoZhun)
        {
            if (type == ExportType.Report_XiaoZhun_CNAS)
            {
                return;//待修改成Word
            }
            string sheetName_Destination = "封皮";
            ISheet sheet_Destination = hssfworkbook.GetSheet(sheetName_Destination);
            #region 封皮
            //单元格从0开始
            //证书编号
            sheet_Destination.GetRow(12).GetCell(9).SetCellValue(entity.REPORTNUMBER);
            if (entity.APPLIANCE_LABORATORY != null && entity.APPLIANCE_LABORATORY.Count > 0)
            {
                IAPPLIANCE_DETAIL_INFORMATIONBLL infBll = new APPLIANCE_DETAIL_INFORMATIONBLL();
                APPLIANCE_DETAIL_INFORMATION infEntity = infBll.GetById(entity.APPLIANCE_LABORATORY.FirstOrDefault().APPLIANCE_DETAIL_INFORMATIONID);
                if (infEntity != null)
                {
                    //器具名称
                    if (infEntity.APPLIANCE_NAME != null && infEntity.APPLIANCE_NAME.Trim() != "")
                    {
                        sheet_Destination.GetRow(17).GetCell(7).SetCellValue(infEntity.APPLIANCE_NAME);
                    }
                    else
                    {
                        sheet_Destination.GetRow(17).GetCell(7).SetCellValue("/");
                    }
                    //型 号/规 格(有型号显示型号，没有显示规格)
                    if (infEntity.VERSION != null && infEntity.VERSION.Trim() != "")//器具型号
                    {
                        sheet_Destination.GetRow(19).GetCell(7).SetCellValue(infEntity.VERSION);
                    }
                    else if (infEntity.APPLIANCE_NAME != null && infEntity.APPLIANCE_NAME.Trim() != "")//计量器具名称
                    {
                        sheet_Destination.GetRow(19).GetCell(7).SetCellValue(infEntity.APPLIANCE_NAME);
                    }
                    else
                    {
                        sheet_Destination.GetRow(19).GetCell(7).SetCellValue("/");
                    }

                    //出厂编号
                    if (infEntity.FACTORY_NUM != null && infEntity.FACTORY_NUM.Trim() != "")
                    {
                        sheet_Destination.GetRow(21).GetCell(7).SetCellValue(infEntity.FACTORY_NUM);
                    }
                    else
                    {
                        sheet_Destination.GetRow(21).GetCell(7).SetCellValue("/");
                    }
                    //生产厂家/制 造 单 位
                    if (infEntity.MAKE_ORGANIZATION != null && infEntity.MAKE_ORGANIZATION.Trim() != "")
                    {
                        sheet_Destination.GetRow(23).GetCell(7).SetCellValue(infEntity.MAKE_ORGANIZATION);
                    }
                    else
                    {
                        sheet_Destination.GetRow(23).GetCell(7).SetCellValue("/");
                    }
                    IORDER_TASK_INFORMATIONBLL taskBll = new ORDER_TASK_INFORMATIONBLL();
                    ORDER_TASK_INFORMATION taskEntity = taskBll.GetById(infEntity.ORDER_TASK_INFORMATIONID);
                    if (taskEntity != null)
                    {
                        //受理单位
                        if (taskEntity.ACCEPT_ORGNIZATION != null && taskEntity.ACCEPT_ORGNIZATION.Trim() != "")
                        {
                            sheet_Destination.GetRow(3).GetCell(0).SetCellValue(taskEntity.ACCEPT_ORGNIZATION);
                        }
                        //委托单位 /送 检 单 位       
                        if (taskEntity.INSPECTION_ENTERPRISE != null && taskEntity.INSPECTION_ENTERPRISE.Trim() != "")
                        {
                            sheet_Destination.GetRow(15).GetCell(7).SetCellValue(taskEntity.INSPECTION_ENTERPRISE);
                        }
                        else
                        {
                            sheet_Destination.GetRow(15).GetCell(7).SetCellValue("/");
                        }
                        //受理单位信息
                        SetShouLiDangWeiXinXi(sheet_Destination, taskEntity.ACCEPT_ORGNIZATION);
                    }
                }
            }



            //批 准 人
            if (entity.APPROVALID == null || entity.APPROVALID.Trim() == "")
            {
                sheet_Destination.GetRow(33).GetCell(13).SetCellValue("/");
            }
            else
            {
                sheet_Destination.GetRow(33).GetCell(13).SetCellValue(entity.APPROVALID);
            }
            //核验员
            if (entity.DETECTERID != null && entity.DETECTERID.Trim() != "")
            {
                sheet_Destination.GetRow(35).GetCell(13).SetCellValue(entity.DETECTERID);
            }
            else
            {
                sheet_Destination.GetRow(35).GetCell(13).SetCellValue("/");
            }
            //检定员\校 准 员
            if (entity.CHECKERID != null && entity.CHECKERID.Trim() != "")
            {
                sheet_Destination.GetRow(37).GetCell(13).SetCellValue(entity.CHECKERID);
            }
            else
            {
                sheet_Destination.GetRow(37).GetCell(13).SetCellValue("/");
            }
            //检定日期\校 准 日 期
            if (entity.CALIBRATION_DATE.HasValue)
            {
                sheet_Destination.GetRow(42).GetCell(9).SetCellValue(entity.CALIBRATION_DATE.Value.ToString("yyyy年MM月dd日"));
            }
            else
            {
                sheet_Destination.GetRow(42).GetCell(9).SetCellValue("/");
            }
            #region 暂时没有数据，不做
            ////检定所使用的计量标准装置
            ////HideRow(sheet, RowIndex, 6);
            //RowIndex = RowIndex + 6;



            ////检定所使用的主要计量器具
            ////HideRow(sheet, RowIndex, 6);
            //RowIndex = RowIndex + 6;
            ////比对和匝比试验使用的中间试品
            ////HideRow(sheet, RowIndex, 6);
            //RowIndex = RowIndex + 6;
            ////空白
            //RowIndex = RowIndex + 8;
            #endregion
            #endregion

            sheet_Destination.ForceFormulaRecalculation = true;
        }
        /// <summary>
        /// 设置检定报告通知书封皮信息
        /// </summary>
        /// <param name="hssfworkbook"></param>
        /// <param name="entity"></param>
        private void SetFengPi_BaoGaoJianDing_TongZhiShu(IWorkbook hssfworkbook, PREPARE_SCHEME entity)
        {
            string sheetName_Destination = "通知书封皮";
            ISheet sheet_Destination = hssfworkbook.GetSheet(sheetName_Destination);
            #region 封皮
            //单元格从0开始
            //证书编号
            sheet_Destination.GetRow(12).GetCell(9).SetCellValue(entity.REPORTNUMBER);
            if (entity.APPLIANCE_LABORATORY != null && entity.APPLIANCE_LABORATORY.Count > 0)
            {
                IAPPLIANCE_DETAIL_INFORMATIONBLL infBll = new APPLIANCE_DETAIL_INFORMATIONBLL();
                APPLIANCE_DETAIL_INFORMATION infEntity = infBll.GetById(entity.APPLIANCE_LABORATORY.FirstOrDefault().APPLIANCE_DETAIL_INFORMATIONID);
                if (infEntity != null)
                {
                    //器具名称
                    if (infEntity.APPLIANCE_NAME != null && infEntity.APPLIANCE_NAME.Trim() != "")
                    {
                        sheet_Destination.GetRow(17).GetCell(7).SetCellValue(infEntity.APPLIANCE_NAME);
                    }
                    else
                    {
                        sheet_Destination.GetRow(17).GetCell(7).SetCellValue("/");
                    }
                    //型 号/规 格(有型号显示型号，没有显示规格)
                    if (infEntity.VERSION != null && infEntity.VERSION.Trim() != "")//器具型号
                    {
                        sheet_Destination.GetRow(19).GetCell(7).SetCellValue(infEntity.VERSION);
                    }
                    else if (infEntity.APPLIANCE_NAME != null && infEntity.APPLIANCE_NAME.Trim() != "")//计量器具名称
                    {
                        sheet_Destination.GetRow(19).GetCell(7).SetCellValue(infEntity.APPLIANCE_NAME);
                    }
                    else
                    {
                        sheet_Destination.GetRow(19).GetCell(7).SetCellValue("/");
                    }

                    //出厂编号
                    if (infEntity.FACTORY_NUM != null && infEntity.FACTORY_NUM.Trim() != "")
                    {
                        sheet_Destination.GetRow(21).GetCell(7).SetCellValue(infEntity.FACTORY_NUM);
                    }
                    else
                    {
                        sheet_Destination.GetRow(21).GetCell(7).SetCellValue("/");
                    }
                    //生产厂家/制 造 单 位
                    if (infEntity.MAKE_ORGANIZATION != null && infEntity.MAKE_ORGANIZATION.Trim() != "")
                    {
                        sheet_Destination.GetRow(23).GetCell(7).SetCellValue(infEntity.MAKE_ORGANIZATION);
                    }
                    else
                    {
                        sheet_Destination.GetRow(23).GetCell(7).SetCellValue("/");
                    }
                    IORDER_TASK_INFORMATIONBLL taskBll = new ORDER_TASK_INFORMATIONBLL();
                    ORDER_TASK_INFORMATION taskEntity = taskBll.GetById(infEntity.ORDER_TASK_INFORMATIONID);
                    if (taskEntity != null)
                    {
                        ////证书单位
                        //if (taskEntity.CERTIFICATE_ENTERPRISE != null && taskEntity.CERTIFICATE_ENTERPRISE.Trim() != "")
                        //{
                        //    sheet_Destination.GetRow(3).GetCell(0).SetCellValue(taskEntity.CERTIFICATE_ENTERPRISE);
                        //}
                        //受理单位
                        if (taskEntity.ACCEPT_ORGNIZATION != null && taskEntity.ACCEPT_ORGNIZATION.Trim() != "")
                        {
                            sheet_Destination.GetRow(3).GetCell(0).SetCellValue(taskEntity.ACCEPT_ORGNIZATION);
                        }
                        //委托单位 /送 检 单 位       
                        if (taskEntity.INSPECTION_ENTERPRISE != null && taskEntity.INSPECTION_ENTERPRISE.Trim() != "")
                        {
                            sheet_Destination.GetRow(15).GetCell(7).SetCellValue(taskEntity.INSPECTION_ENTERPRISE);
                        }
                        else
                        {
                            sheet_Destination.GetRow(15).GetCell(7).SetCellValue("/");
                        }
                        //受理单位信息
                        SetShouLiDangWeiXinXi(sheet_Destination, taskEntity.ACCEPT_ORGNIZATION);
                    }
                }
            }

            #region 检 定 依 据 当规程2个以上时，该处没有“检定依据”隐藏该行，该位置直接显示“检定结论”
            //检定所依据技术文件（代号、名称）
            IVRULEBLL rBll = new VRULEBLL();
            List<VRULE> rList = rBll.GetBySCHEMEID(entity.SCHEMEID);
            if (rList != null && rList.Count == 1)//一个规程
            {
                sheet_Destination.GetRow(25).GetCell(7).SetCellValue(rList[0].NAME);
            }
            else
            {
                HideRow(sheet_Destination, 25, 2);
            }
            #endregion

            //检定结论   
            if (entity.CONCLUSION_EXPLAIN == null || entity.CONCLUSION_EXPLAIN.Trim() == "")
            {
                sheet_Destination.GetRow(27).GetCell(7).SetCellValue("/");
            }
            else
            {
                sheet_Destination.GetRow(27).GetCell(7).SetCellValue(entity.CONCLUSION);
            }

            //批 准 人
            if (entity.APPROVALID == null || entity.APPROVALID.Trim() == "")
            {
                sheet_Destination.GetRow(33).GetCell(13).SetCellValue("/");
            }
            else
            {
                sheet_Destination.GetRow(33).GetCell(13).SetCellValue(entity.APPROVALID);
            }
            //核验员
            if (entity.DETECTERID != null && entity.DETECTERID.Trim() != "")
            {
                sheet_Destination.GetRow(35).GetCell(13).SetCellValue(entity.DETECTERID);
            }
            else
            {
                sheet_Destination.GetRow(35).GetCell(13).SetCellValue("/");
            }
            //检定员
            if (entity.CHECKERID != null && entity.CHECKERID.Trim() != "")
            {
                sheet_Destination.GetRow(37).GetCell(13).SetCellValue(entity.CHECKERID);
            }
            else
            {
                sheet_Destination.GetRow(37).GetCell(13).SetCellValue("/");
            }
            //检定日期
            if (entity.CALIBRATION_DATE.HasValue)
            {
                sheet_Destination.GetRow(42).GetCell(9).SetCellValue(entity.CALIBRATION_DATE.Value.ToString("yyyy年MM月dd日"));
            }
            else
            {
                sheet_Destination.GetRow(42).GetCell(9).SetCellValue("/");
            }
            ////有效期，通知书不要有效期
            //if (entity.VALIDITY_PERIOD.HasValue && entity.CALIBRATION_DATE.HasValue)
            //{
            //    //sheet_Destination.GetRow(43).GetCell(9).SetCellValue(entity.VALIDITY_PERIOD.Value.ToString("yyyy年MM月dd日"));                
            //    sheet_Destination.GetRow(43).GetCell(9).SetCellValue(entity.CALIBRATION_DATE.Value.AddYears(entity.VALIDITY_PERIOD.Value).ToString("yyyy年MM月dd日"));
            //}
            //else
            //{
            //    sheet_Destination.GetRow(43).GetCell(9).SetCellValue("/");
            //}
            #region 暂时没有数据，不做
            ////检定所使用的计量标准装置
            ////HideRow(sheet, RowIndex, 6);
            //RowIndex = RowIndex + 6;



            ////检定所使用的主要计量器具
            ////HideRow(sheet, RowIndex, 6);
            //RowIndex = RowIndex + 6;
            ////比对和匝比试验使用的中间试品
            ////HideRow(sheet, RowIndex, 6);
            //RowIndex = RowIndex + 6;
            ////空白
            //RowIndex = RowIndex + 8;
            #endregion
            #endregion

            sheet_Destination.ForceFormulaRecalculation = true;
        }
        /// <summary>
        /// 设置报告第二页信息
        /// </summary>
        /// <param name="hssfworkbook"></param>
        /// <param name="entity"></param>
        private void SetSecond_BaoGao(IWorkbook hssfworkbook, PREPARE_SCHEME entity, ExportType type = ExportType.OriginalRecord_JianDing)
        {

            string sheetName_Destination = "第二页";
            ISheet sheet_Destination = hssfworkbook.GetSheet(sheetName_Destination);
            #region 第二页
            //单元格从0开始
            //资质说明
            if (entity.QUALIFICATIONS != null && entity.QUALIFICATIONS.Trim() != "")
            {
                sheet_Destination.GetRow(3).GetCell(2).SetCellValue(entity.QUALIFICATIONS);
            }
            else
            {
                sheet_Destination.GetRow(3).GetCell(2).SetCellValue("/");
            }

            //温度
            if (entity.TEMPERATURE != null && entity.TEMPERATURE.Trim() != "")
            {
                sheet_Destination.GetRow(5).GetCell(6).SetCellValue(entity.TEMPERATURE+ "℃");
            }
            else
            {
                sheet_Destination.GetRow(5).GetCell(5).SetCellValue("/");
            }
            //相对湿度
            if (entity.HUMIDITY != null && entity.HUMIDITY.Trim() != "")
            {
                sheet_Destination.GetRow(5).GetCell(20).SetCellValue(entity.HUMIDITY+"%");
            }
            else
            {
                sheet_Destination.GetRow(5).GetCell(20).SetCellValue("/");
            }

            //检定地点
            if (entity.CHECK_PLACE != null && entity.CHECK_PLACE.Trim() != "")
            {
                sheet_Destination.GetRow(6).GetCell(6).SetCellValue(entity.CHECK_PLACE);
            }
            else
            {
                sheet_Destination.GetRow(6).GetCell(6).SetCellValue("/");
            }
            //其他
            if (entity.OTHER != null && entity.OTHER.Trim() != "")
            {
                sheet_Destination.GetRow(6).GetCell(20).SetCellValue(entity.OTHER);
            }
            else
            {
                sheet_Destination.GetRow(6).GetCell(20).SetCellValue("/");
            }
            #region 检 定 依 据 多个时:顺序显示，一行一个，仅一个时：“依据的技术文件”部分不显示，直接显示下边
            //检定所依据技术文件（代号、名称）
            IVRULEBLL rBll = new VRULEBLL();
            List<VRULE> rList = rBll.GetBySCHEMEID(entity.SCHEMEID);
            int RowIndex = 0;
            if (rList != null && rList.Count > 1)//两个以上规程
            {

                IRow GCTemplateRow = sheet_Destination.GetRow(8);//获取源格式行
                int GCTemplateIndex = 8;//规程模板行号   
                RowIndex = 8;
                CopyRow(sheet_Destination, GCTemplateIndex + 1, GCTemplateIndex, rList.Count - 1, false);
                foreach (VRULE r in rList)
                {
                    sheet_Destination.GetRow(RowIndex).GetCell(2).SetCellValue(r.NAME);
                    RowIndex++;
                }
            }
            else
            {
                HideRow(sheet_Destination, 7, 2);
                RowIndex = 9;
            }
            #endregion
            //RowIndex++;
            //各类装置
            SetZhuangZhis(hssfworkbook, sheet_Destination, ref RowIndex, entity, type);
            #region 校准说明            
            RowIndex++;
            if (entity.CONCLUSION_EXPLAIN == null || entity.CONCLUSION_EXPLAIN.Trim() == "")
            {
                sheet_Destination.GetRow(RowIndex).GetCell(2).SetCellValue("/");
            }
            else
            {
                sheet_Destination.GetRow(RowIndex).GetCell(2).SetCellValue(entity.CONCLUSION_EXPLAIN);
            }
            #endregion

            if (type == ExportType.Report_XiaoZhun_CNAS)
            {
                RowIndex = RowIndex + 7;
                //检定员\校准员
                if (entity.CHECKERID != null && entity.CHECKERID.Trim() != "")
                {
                    sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue(entity.CHECKERID);
                }
                else
                {
                    sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue("/");
                }
                //核验员
                if (entity.DETECTERID != null && entity.DETECTERID.Trim() != "")
                {
                    sheet_Destination.GetRow(RowIndex).GetCell(15).SetCellValue(entity.DETECTERID);
                }
                else
                {
                    sheet_Destination.GetRow(RowIndex).GetCell(15).SetCellValue("/");
                }
            }
            #endregion

            //设置页面页脚
            SetHeaderAndFooter(sheet_Destination, entity, type);
            sheet_Destination.ForceFormulaRecalculation = true;
        }
        /// <summary>
        /// 设置检定报告封皮信息
        /// </summary>
        /// <param name="hssfworkbook"></param>
        /// <param name="entity"></param>
        private void SetFengPi_BaoGaoJianDing(IWorkbook hssfworkbook, PREPARE_SCHEME entity)
        {

            if (entity.CONCLUSION == "不合格")//不合格只有通知书封皮
            {
                SetFengPi_BaoGaoJianDing_TongZhiShu(hssfworkbook, entity);
                return;
            }

            string sheetName_Destination = "封皮";
            ISheet sheet_Destination = hssfworkbook.GetSheet(sheetName_Destination);
            #region 封皮
            //单元格从0开始
            //证书编号
            sheet_Destination.GetRow(12).GetCell(9).SetCellValue(entity.REPORTNUMBER);
            if (entity.APPLIANCE_LABORATORY != null && entity.APPLIANCE_LABORATORY.Count > 0)
            {
                IAPPLIANCE_DETAIL_INFORMATIONBLL infBll = new APPLIANCE_DETAIL_INFORMATIONBLL();
                APPLIANCE_DETAIL_INFORMATION infEntity = infBll.GetById(entity.APPLIANCE_LABORATORY.FirstOrDefault().APPLIANCE_DETAIL_INFORMATIONID);
                if (infEntity != null)
                {
                    //器具名称
                    if (infEntity.APPLIANCE_NAME != null && infEntity.APPLIANCE_NAME.Trim() != "")
                    {
                        sheet_Destination.GetRow(17).GetCell(7).SetCellValue(infEntity.APPLIANCE_NAME);
                    }
                    else
                    {
                        sheet_Destination.GetRow(17).GetCell(7).SetCellValue("/");
                    }
                    //型 号/规 格(有型号显示型号，没有显示规格)
                    if (infEntity.VERSION != null && infEntity.VERSION.Trim() != "")//器具型号
                    {
                        sheet_Destination.GetRow(19).GetCell(7).SetCellValue(infEntity.VERSION);
                    }
                    else if (infEntity.APPLIANCE_NAME != null && infEntity.APPLIANCE_NAME.Trim() != "")//计量器具名称
                    {
                        sheet_Destination.GetRow(19).GetCell(7).SetCellValue(infEntity.APPLIANCE_NAME);
                    }
                    else
                    {
                        sheet_Destination.GetRow(19).GetCell(7).SetCellValue("/");
                    }

                    //出厂编号
                    if (infEntity.FACTORY_NUM != null && infEntity.FACTORY_NUM.Trim() != "")
                    {
                        sheet_Destination.GetRow(21).GetCell(7).SetCellValue(infEntity.FACTORY_NUM);
                    }
                    else
                    {
                        sheet_Destination.GetRow(21).GetCell(7).SetCellValue("/");
                    }
                    //生产厂家/制 造 单 位
                    if (infEntity.MAKE_ORGANIZATION != null && infEntity.MAKE_ORGANIZATION.Trim() != "")
                    {
                        sheet_Destination.GetRow(23).GetCell(7).SetCellValue(infEntity.MAKE_ORGANIZATION);
                    }
                    else
                    {
                        sheet_Destination.GetRow(23).GetCell(7).SetCellValue("/");
                    }
                    IORDER_TASK_INFORMATIONBLL taskBll = new ORDER_TASK_INFORMATIONBLL();
                    ORDER_TASK_INFORMATION taskEntity = taskBll.GetById(infEntity.ORDER_TASK_INFORMATIONID);
                    if (taskEntity != null)
                    {
                        //受理单位
                        if (taskEntity.ACCEPT_ORGNIZATION != null && taskEntity.ACCEPT_ORGNIZATION.Trim() != "")
                        {
                            sheet_Destination.GetRow(3).GetCell(0).SetCellValue(taskEntity.ACCEPT_ORGNIZATION);
                        }
                        //委托单位 /送 检 单 位       
                        if (taskEntity.INSPECTION_ENTERPRISE != null && taskEntity.INSPECTION_ENTERPRISE.Trim() != "")
                        {
                            sheet_Destination.GetRow(15).GetCell(7).SetCellValue(taskEntity.INSPECTION_ENTERPRISE);
                        }
                        else
                        {
                            sheet_Destination.GetRow(15).GetCell(7).SetCellValue("/");
                        }
                        //受理单位信息
                        SetShouLiDangWeiXinXi(sheet_Destination, taskEntity.ACCEPT_ORGNIZATION);
                    }
                }
            }

            #region 检 定 依 据 当规程2个以上时，该处没有“检定依据”隐藏该行，该位置直接显示“检定结论”
            //检定所依据技术文件（代号、名称）
            IVRULEBLL rBll = new VRULEBLL();
            List<VRULE> rList = rBll.GetBySCHEMEID(entity.SCHEMEID);
            if (rList != null && rList.Count == 1)//一个规程
            {
                sheet_Destination.GetRow(25).GetCell(7).SetCellValue(rList[0].NAME);
            }
            else
            {
                HideRow(sheet_Destination, 25, 2);
            }
            #endregion

            //检定结论   
            if (entity.CONCLUSION_EXPLAIN == null || entity.CONCLUSION_EXPLAIN.Trim() == "")
            {
                sheet_Destination.GetRow(27).GetCell(7).SetCellValue("/");
            }
            else
            {
                sheet_Destination.GetRow(27).GetCell(7).SetCellValue(entity.CONCLUSION);
            }

            //批 准 人
            if (entity.APPROVALID == null || entity.APPROVALID.Trim() == "")
            {
                sheet_Destination.GetRow(33).GetCell(13).SetCellValue("/");
            }
            else
            {
                sheet_Destination.GetRow(33).GetCell(13).SetCellValue(entity.APPROVALID);
            }
            //核验员
            if (entity.DETECTERID != null && entity.DETECTERID.Trim() != "")
            {
                sheet_Destination.GetRow(35).GetCell(13).SetCellValue(entity.DETECTERID);
            }
            else
            {
                sheet_Destination.GetRow(35).GetCell(13).SetCellValue("/");
            }
            //检定员
            if (entity.CHECKERID != null && entity.CHECKERID.Trim() != "")
            {
                sheet_Destination.GetRow(37).GetCell(13).SetCellValue(entity.CHECKERID);
            }
            else
            {
                sheet_Destination.GetRow(37).GetCell(13).SetCellValue("/");
            }
            //检定日期
            if (entity.CALIBRATION_DATE.HasValue)
            {
                sheet_Destination.GetRow(42).GetCell(9).SetCellValue(entity.CALIBRATION_DATE.Value.ToString("yyyy年MM月dd日"));
            }
            else
            {
                sheet_Destination.GetRow(42).GetCell(9).SetCellValue("/");
            }
            //有效期
            //if (entity.VALIDITY_PERIOD.HasValue && entity.CALIBRATION_DATE.HasValue)
            //{
            //    //sheet_Destination.GetRow(43).GetCell(9).SetCellValue(entity.VALIDITY_PERIOD.Value.ToString("yyyy年MM月dd日"));
            //    sheet_Destination.GetRow(43).GetCell(9).SetCellValue(entity.CALIBRATION_DATE.Value.AddYears((int)entity.VALIDITY_PERIOD.Value).AddDays(-1).ToString("yyyy年MM月dd日"));
            //}
            if (entity.VALIDITYEND.HasValue)//有效期改为直接取数据库值
            {
                sheet_Destination.GetRow(43).GetCell(9).SetCellValue(entity.VALIDITYEND.Value.ToString("yyyy年MM月dd日"));

            }
            else
            {
                sheet_Destination.GetRow(43).GetCell(9).SetCellValue("/");
            }
            #region 暂时没有数据，不做
            ////检定所使用的计量标准装置
            ////HideRow(sheet, RowIndex, 6);
            //RowIndex = RowIndex + 6;



            ////检定所使用的主要计量器具
            ////HideRow(sheet, RowIndex, 6);
            //RowIndex = RowIndex + 6;
            ////比对和匝比试验使用的中间试品
            ////HideRow(sheet, RowIndex, 6);
            //RowIndex = RowIndex + 6;
            ////空白
            //RowIndex = RowIndex + 8;
            #endregion
            #endregion           
            sheet_Destination.ForceFormulaRecalculation = true;
        }
        /// <summary>
        /// 受理单位信息
        /// </summary>
        /// <param name="sheet_Destination">目标sheet</param>
        /// <param name="ShouLiDangWei">受理单位名称</param>
        private void SetShouLiDangWeiXinXi(ISheet sheet_Destination, string ShouLiDangWei)
        {
            #region 受理单位信息
            //地址
            string dizhi = "地址：北京市西城区复兴门外地藏庵南巷1号";
            //邮编
            string youbian = "邮编：100045";
            //电话
            string dianhua = "电话：010-88071523";
            //传真
            string chuanzhen = "传真：010-88071504";
            if (ShouLiDangWei != null && ShouLiDangWei.Trim() != "" && ShouLiDangWei.Trim() == "冀北电力有限公司计量中心")
            {
                dizhi = "地址：北京市昌平区回龙观镇二拨子村";
                youbian = "邮编：102208";
                dianhua = "电话：010-56585812";
                chuanzhen = "传真：010-56585804";

            }
            //地址
            sheet_Destination.GetRow(46).GetCell(2).SetCellValue(dizhi);
            //邮编
            sheet_Destination.GetRow(46).GetCell(14).SetCellValue(youbian);
            //电话
            sheet_Destination.GetRow(47).GetCell(2).SetCellValue(dianhua);
            //传真
            sheet_Destination.GetRow(47).GetCell(14).SetCellValue(chuanzhen);
            #endregion
        }
     
        /// <summary>
        /// 导出原始记录Excel
        /// </summary>
        /// <param name="ID">预备方案ID</param>
        /// <returns></returns>
        public bool ExportOriginalRecord(string ID, out string Message, out FILE_UPLOADER fEntity)
        {
            fEntity = new FILE_UPLOADER();
            IBLL.IPREPARE_SCHEMEBLL m_BLL = new PREPARE_SCHEMEBLL();
            PREPARE_SCHEME entity = m_BLL.GetById(ID);
            string saveFileName = "";
            if (entity != null)
            {
                ExportType type = GetExportType(entity, "ExportOriginal");
                //string xlsPath = ReportStatic.YuanShiJiLuJianDingPath;
                //if (entity.CERTIFICATE_CATEGORY == ZhengShuLeiBieEnums.校准.ToString())
                //{
                //    xlsPath = ReportStatic.YuanShiJiLuXiaoZhunPath;
                //}
                string xlsPath = GetTemplatePath(type);
                HSSFWorkbook _book = new HSSFWorkbook();
                FileStream file = new FileStream(xlsPath, FileMode.Open, FileAccess.Read);
                IWorkbook hssfworkbook = new HSSFWorkbook(file);

                //设置封皮
                SetFengPi(hssfworkbook, entity, type);

                //设置数据
                SetShuJu(hssfworkbook, entity, type);

                //设置不确定度
                SetBuQueDingDu(hssfworkbook, entity, type);

                //saveFileName = "../up/Report/" + entity.CERTIFICATE_CATEGORY + "_" + Result.GetNewId() + ".xls";                
                string fileName = SetFileName(type);
                saveFileName = "~/up/Report/" + fileName + ".xls";

                string saveFileNamePath = System.Web.HttpContext.Current.Server.MapPath(saveFileName);
                using (FileStream fileWrite = new FileStream(saveFileNamePath, FileMode.Create))
                {
                    hssfworkbook.Write(fileWrite);
                }
                Message = "../up/Report/" + fileName + ".xls";
                fEntity.CONCLUSION = entity.CONCLUSION;
                fEntity.CREATETIME = DateTime.Now;
                fEntity.PATH2 = saveFileName;
                fEntity.FULLPATH2 = saveFileNamePath;
                fEntity.NAME2 = fileName;
                fEntity.SUFFIX2 = ".xls";
                fEntity.PREPARE_SCHEMEID = entity.ID;
                fEntity.STATE2 = "已上传";
                //fEntity.CREATEPERSON = CreatePerson;
                fEntity.ID = Result.GetNewId();
                //ValidationErrors validationErrors = new ValidationErrors();
                //fBll.Create(ref validationErrors, fEntity);                
                return true;
            }
            Message = "未找到预备方案ID为【" + ID + "】的数据";
            return false;
        }
        /// <summary>
        /// 设置不确定度
        /// </summary>
        /// <param name="hssfworkbook">workbook</param>
        /// <param name="entity"></param>
        /// <param name="type"></param>        
        private void SetBuQueDingDu(IWorkbook hssfworkbook, PREPARE_SCHEME entity, ExportType type = ExportType.OriginalRecord_JianDing)
        {
            if (type == ExportType.OriginalRecord_JianDing || type == ExportType.OriginalRecord_XiaoZhun)
            {
                string sheetName_Source = "不确定度模板";
                string sheetName_Destination = "不确定度";
                ISheet sheet_Source = hssfworkbook.GetSheet(sheetName_Source);
                ISheet sheet_Destination = hssfworkbook.GetSheet(sheetName_Destination);
                #region 检测项目            
                if (entity.QUALIFIED_UNQUALIFIED_TEST_ITE != null &&
                    entity.QUALIFIED_UNQUALIFIED_TEST_ITE.Count > 0)
                {
                    SpecialCharacters allSpecialCharacters = GetSpecialCharacters();
                    entity.QUALIFIED_UNQUALIFIED_TEST_ITE = entity.QUALIFIED_UNQUALIFIED_TEST_ITE.OrderBy(p => p.SORT).ToList();

                    int rowIndex_Destination = 0;
                    int ruleCount = 0;
                    foreach (QUALIFIED_UNQUALIFIED_TEST_ITE iEntity in entity.QUALIFIED_UNQUALIFIED_TEST_ITE)
                    {

                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(iEntity.HTMLVALUE);
                        Dictionary<int, List<BuDueDingDu>> buQueDingDuDic = AnalyticHTML.GetBuDueDingDu(doc);
                        if (buQueDingDuDic != null && buQueDingDuDic.Count > 0)
                        {
                            foreach (List<BuDueDingDu> buQueDingDus in buQueDingDuDic.Values)
                            {
                                foreach (BuDueDingDu buQueDingDu in buQueDingDus)
                                {
                                    #region 不确定度的评定
                                    ruleCount++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 0, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(0).SetCellValue(ruleCount.ToString() + "." + iEntity.RULENAME + "不确定度的评定");
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(35).SetCellValue(buQueDingDu.pdDDL);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(41).SetCellValue(buQueDingDu.pdText);
                                    rowIndex_Destination++;
                                    #endregion

                                    #region 评定点                               
                                    int pingdingIndex = 1;
                                    if (buQueDingDu.pingding != null && buQueDingDu.pingding.Count > 0)
                                    {

                                        while (buQueDingDu.pingding != null && buQueDingDu.pingding.Count > 0)
                                        {
                                            Dictionary<string, CellRangeAddress> cellAddressList = CopyRow_1(sheet_Source, sheet_Destination, 1, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                            int cIndex = 1;
                                            if (cellAddressList != null && cellAddressList.Count > 0)
                                            {
                                                foreach (CellRangeAddress c in cellAddressList.Values)
                                                {
                                                    MYData d = buQueDingDu.pingding.FirstOrDefault();
                                                    if (cIndex == 1 && pingdingIndex == 1)
                                                    {
                                                        sheet_Destination.GetRow(rowIndex_Destination).GetCell(c.FirstColumn).SetCellValue("评定点：");
                                                    }
                                                    else if (cIndex == 1)
                                                    {
                                                        sheet_Destination.GetRow(rowIndex_Destination).GetCell(c.FirstColumn).SetCellValue("");
                                                    }
                                                    else
                                                    {
                                                        sheet_Destination.GetRow(rowIndex_Destination).GetCell(c.FirstColumn).SetCellValue(d.value);
                                                        buQueDingDu.pingding.Remove(d);
                                                    }
                                                    cIndex++;
                                                }
                                                pingdingIndex++;
                                            }
                                            rowIndex_Destination++;

                                        }
                                    }
                                    #endregion

                                    #region 不确定度的A类评定
                                    CopyRow_1(sheet_Source, sheet_Destination, 2, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 3, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 4, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(5).SetCellValue(buQueDingDu.txtBuQueDingA);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 5, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 6, rowIndex_Destination, 1, false, null, allSpecialCharacters, null);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(0).SetCellValue(buQueDingDu.A_1_1);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(12).SetCellValue(buQueDingDu.A_1_2);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(23).SetCellValue(buQueDingDu.A_1_3);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(34).SetCellValue(buQueDingDu.A_1_4);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(45).SetCellValue(buQueDingDu.A_1_5);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 7, rowIndex_Destination, 1, false, null, allSpecialCharacters, null);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(0).SetCellValue(buQueDingDu.A_2_1);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(12).SetCellValue(buQueDingDu.A_2_2);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(23).SetCellValue(buQueDingDu.A_2_3);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(34).SetCellValue(buQueDingDu.A_2_4);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(45).SetCellValue(buQueDingDu.A_2_5);
                                    rowIndex_Destination++;
                                    #endregion

                                    #region 不确定度的B类评定
                                    CopyRow_1(sheet_Source, sheet_Destination, 2, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 9, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 10, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(5).SetCellValue(buQueDingDu.txtBuQueDingB);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 11, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    rowIndex_Destination++;
                                    if (buQueDingDu.buDueDingDuB != null && buQueDingDu.buDueDingDuB.Count > 0)
                                    {

                                        while (buQueDingDu.buDueDingDuB != null && buQueDingDu.buDueDingDuB.Count > 0)
                                        {
                                            Dictionary<string, CellRangeAddress> cellAddressList = CopyRow_1(sheet_Source, sheet_Destination, 12, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                            if (cellAddressList != null && cellAddressList.Count > 0)
                                            {
                                                foreach (CellRangeAddress c in cellAddressList.Values)
                                                {
                                                    MYData d = buQueDingDu.buDueDingDuB.FirstOrDefault();
                                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(c.FirstColumn).SetCellValue(d.value);
                                                    buQueDingDu.buDueDingDuB.Remove(d);
                                                }
                                            }
                                            rowIndex_Destination++;

                                        }
                                    }
                                    #endregion

                                    #region 合成不确定度评定Uc
                                    CopyRow_1(sheet_Source, sheet_Destination, 2, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 14, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 15, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(5).SetCellValue(buQueDingDu.txtBuQueDingC);
                                    rowIndex_Destination++;
                                    #endregion

                                    #region 扩展不确定度评定
                                    CopyRow_1(sheet_Source, sheet_Destination, 2, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 17, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(20).SetCellValue(buQueDingDu.ddlSelectD);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 18, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(5).SetCellValue(buQueDingDu.txtvalueD);
                                    rowIndex_Destination++;
                                    #endregion

                                    #region 测量结果报告
                                    CopyRow_1(sheet_Source, sheet_Destination, 2, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 20, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    rowIndex_Destination++;
                                    CopyRow_1(sheet_Source, sheet_Destination, 21, rowIndex_Destination, 1, true, null, allSpecialCharacters, null);
                                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(11).SetCellValue(buQueDingDu.txtValueE);
                                    rowIndex_Destination++;
                                    #endregion
                                }
                            }
                        }



                    }
                }
                #endregion
            }

        }
        /// <summary>
        /// 设置封皮信息
        /// </summary>
        /// <param name="hssfworkbook"></param>
        /// <param name="entity"></param>
        private void SetFengPi(IWorkbook hssfworkbook, PREPARE_SCHEME entity, ExportType type = ExportType.OriginalRecord_JianDing)
        {
            string sheetName_Source = "封皮模板";
            string sheetName_Destination = "封皮";
            ISheet sheet_Source = hssfworkbook.GetSheet(sheetName_Source);
            ISheet sheet_Destination = hssfworkbook.GetSheet(sheetName_Destination);
            int RowIndex = 0;
            #region 封皮
            //单元格从0开始
            //准确度等级
            sheet_Destination.GetRow(11).GetCell(7).SetCellValue(entity.ACCURACY_GRADE);
            //额定频率
            if (entity.RATED_FREQUENCY == null || entity.RATED_FREQUENCY.Trim() == "")
            {
                sheet_Destination.GetRow(11).GetCell(19).SetCellValue("");
                sheet_Destination.GetRow(11).GetCell(23).SetCellValue("");
                sheet_Destination.GetRow(11).GetCell(23).CellStyle.BorderBottom = BorderStyle.None;//底部边框去掉                    
            }
            else
            {
                sheet_Destination.GetRow(11).GetCell(23).SetCellValue(entity.RATED_FREQUENCY);
            }
            //脉冲常数
            if (entity.PULSE_CONSTANT == null || entity.PULSE_CONSTANT.Trim() == "")
            {
                HideRow(sheet_Destination, 12, 1);
            }
            else
            {
                sheet_Destination.GetRow(12).GetCell(7).SetCellValue(entity.PULSE_CONSTANT);
            }

            if (entity.APPLIANCE_LABORATORY != null && entity.APPLIANCE_LABORATORY.Count > 0)
            {
                IAPPLIANCE_DETAIL_INFORMATIONBLL infBll = new APPLIANCE_DETAIL_INFORMATIONBLL();
                APPLIANCE_DETAIL_INFORMATION infEntity = infBll.GetById(entity.APPLIANCE_LABORATORY.FirstOrDefault().APPLIANCE_DETAIL_INFORMATIONID);
                if (infEntity != null)
                {
                    //器具名称
                    if (infEntity.APPLIANCE_NAME != null && infEntity.APPLIANCE_NAME.Trim() != "")
                    {
                        sheet_Destination.GetRow(9).GetCell(7).SetCellValue(infEntity.APPLIANCE_NAME);
                    }
                    else
                    {
                        sheet_Destination.GetRow(9).GetCell(7).SetCellValue("/");
                    }
                    //器具型号
                    if (infEntity.VERSION != null && infEntity.VERSION.Trim() != "")
                    {
                        sheet_Destination.GetRow(9).GetCell(23).SetCellValue(infEntity.VERSION);
                    }
                    else
                    {
                        sheet_Destination.GetRow(9).GetCell(23).SetCellValue("/");
                    }
                    //器具规格
                    if (infEntity.FORMAT != null && infEntity.FORMAT.Trim() != "")
                    {
                        sheet_Destination.GetRow(10).GetCell(7).SetCellValue(infEntity.FORMAT);
                    }
                    else
                    {
                        sheet_Destination.GetRow(10).GetCell(7).SetCellValue("/");
                    }
                    //出厂编号
                    if (infEntity.FACTORY_NUM != null && infEntity.FACTORY_NUM.Trim() != "")
                    {
                        sheet_Destination.GetRow(10).GetCell(23).SetCellValue(infEntity.FACTORY_NUM);
                    }
                    else
                    {
                        sheet_Destination.GetRow(10).GetCell(23).SetCellValue("/");
                    }
                    //生产厂家
                    if (infEntity.MAKE_ORGANIZATION != null && infEntity.MAKE_ORGANIZATION.Trim() != "")
                    {
                        sheet_Destination.GetRow(13).GetCell(7).SetCellValue(infEntity.MAKE_ORGANIZATION);
                    }
                    else
                    {
                        sheet_Destination.GetRow(13).GetCell(7).SetCellValue("/");
                    }
                    IORDER_TASK_INFORMATIONBLL taskBll = new ORDER_TASK_INFORMATIONBLL();
                    ORDER_TASK_INFORMATION taskEntity = taskBll.GetById(infEntity.ORDER_TASK_INFORMATIONID);
                    if (taskEntity != null)
                    {
                        //委托单位（证书单位）        
                        //if (taskEntity.INSPECTION_ENTERPRISE != null && taskEntity.INSPECTION_ENTERPRISE.Trim() != "")
                        //{
                        //    sheet_Destination.GetRow(6).GetCell(5).SetCellValue(taskEntity.INSPECTION_ENTERPRISE);
                        //}
                        if (taskEntity.CERTIFICATE_ENTERPRISE != null && taskEntity.CERTIFICATE_ENTERPRISE.Trim() != "")
                        {
                            sheet_Destination.GetRow(6).GetCell(5).SetCellValue(taskEntity.CERTIFICATE_ENTERPRISE);
                        }
                        else
                        {
                            sheet_Destination.GetRow(6).GetCell(5).SetCellValue("/");
                        }
                    }
                }
            }

            #region 检定所依据技术文件（代号、名称）
            IVRULEBLL rBll = new VRULEBLL();
            List<VRULE> rList = rBll.GetBySCHEMEID(entity.SCHEMEID);
            if (rList != null && rList.Count > 0)
            {
                IRow GCTemplateRow = sheet_Destination.GetRow(16);//获取源格式行
                int GCTemplateIndex = 16;//规程模板行号
                if (rList.Count > 1)
                {
                    int RowCount = rList.Count - 1;
                    CopyRow(sheet_Destination, GCTemplateIndex + 1, GCTemplateIndex, RowCount, false);
                }
                RowIndex = 16;
                foreach (VRULE rEntity in rList)
                {
                    sheet_Destination.GetRow(RowIndex).GetCell(2).SetCellValue(rEntity.NAME);
                    RowIndex++;
                }
            }
            #endregion
            //温度
            RowIndex++;
            if (entity.TEMPERATURE != null && entity.TEMPERATURE.Trim() != "")
            {
                sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue(entity.TEMPERATURE);
            }
            else
            {
                sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue("/");
            }
            //相对湿度
            if (entity.HUMIDITY != null && entity.HUMIDITY.Trim() != "")
            {
                sheet_Destination.GetRow(RowIndex).GetCell(18).SetCellValue(entity.HUMIDITY);
            }
            else
            {
                sheet_Destination.GetRow(RowIndex).GetCell(18).SetCellValue("/");
            }

            RowIndex = RowIndex + 2;
            //检定地点
            if (entity.CHECK_PLACE != null && entity.CHECK_PLACE.Trim() != "")
            {
                sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue(entity.CHECK_PLACE);
            }
            else
            {
                sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue("/");
            }

            RowIndex++;
            //检定员
            if (entity.CHECKERID != null && entity.CHECKERID.Trim() != "")
            {
                sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue(entity.CHECKERID);
            }
            else
            {
                sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue("/");
            }
            //核验员
            if (entity.DETECTERID != null && entity.DETECTERID.Trim() != "")
            {
                sheet_Destination.GetRow(RowIndex).GetCell(23).SetCellValue(entity.DETECTERID);
            }
            else
            {
                sheet_Destination.GetRow(RowIndex).GetCell(23).SetCellValue("/");
            }
            RowIndex++;
            //检定日期
            if (entity.CALIBRATION_DATE.HasValue)
            {
                sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue(entity.CALIBRATION_DATE.Value.ToString("yyyy年MM月dd日"));
            }
            else
            {
                sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue("/");
            }
            //有效期
            if (entity.VALIDITY_PERIOD.HasValue)
            {
                sheet_Destination.GetRow(RowIndex).GetCell(23).SetCellValue(entity.VALIDITY_PERIOD.Value.ToString() + "年");
                //sheet_Destination.GetRow(RowIndex).GetCell(23).SetCellValue(entity.VALIDITY_PERIOD.Value.ToString("yyyy年MM月dd日"));
            }
            else
            {
                sheet_Destination.GetRow(RowIndex).GetCell(23).SetCellValue("/");
            }
            //RowIndex = RowIndex + 2;
            //if (entity.CERTIFICATE_CATEGORY == ZhengShuLeiBieEnums.校准.ToString())
            if (type == ExportType.OriginalRecord_XiaoZhun)
            {
                //校准说明   
                RowIndex = RowIndex + 1;
                if (entity.CONCLUSION_EXPLAIN == null || entity.CONCLUSION_EXPLAIN.Trim() == "")
                {
                    sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue("/");
                }
                else
                {
                    sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue(entity.CONCLUSION_EXPLAIN);
                }

            }
            else
            {
                //检定结论  
                RowIndex = RowIndex + 2;
                if (entity.CALIBRATION_INSTRUCTIONS == null || entity.CALIBRATION_INSTRUCTIONS.Trim() == "")
                {
                    sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue("/");
                }
                else
                {
                    sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue(entity.CALIBRATION_INSTRUCTIONS);
                }
                RowIndex++;
                //检定说明  
                if (entity.CONCLUSION_EXPLAIN == null || entity.CONCLUSION_EXPLAIN.Trim() == "")
                {
                    sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue("/");
                }
                else
                {
                    sheet_Destination.GetRow(RowIndex).GetCell(5).SetCellValue(entity.CONCLUSION_EXPLAIN);
                }
                RowIndex++;
            }
            RowIndex++;
            //各类装置
            SetZhuangZhis(hssfworkbook, sheet_Destination, ref RowIndex, entity, type);
            #endregion
            //设置页面页脚
            SetHeaderAndFooter(sheet_Destination, entity);
            sheet_Destination.ForceFormulaRecalculation = true;
        }
        /// <summary>
        /// 设置标准装置/计量标准器信息
        /// </summary>
        /// <param name="sheet_Source">源sheet</param>
        /// <param name="sheet_Destination">目标sheet</param>
        /// <param name="rowIndex_Destination">目标行号</param>
        /// <param name="PREPARE_SCHEMEID">预备方案ID</param>
        /// <param name="type">报告类型</param>
        private void SetZhuangZhis(IWorkbook hssfworkbook, ISheet sheet_Destination, ref int rowIndex_Destination, PREPARE_SCHEME entity, ExportType type)
        {
            //int rowIndex = rowIndex_Destination;
            //标准装置
            int rowIndex_Source_ZhuangZhi = 29;
            //计量器具
            int rowIndex_Source_QiJu = 32;
            //中间试品
            int rowIndex_Source_ShiPin = 35;
            string sheetName_Source = "封皮模板";
            switch (type)
            {
                case ExportType.OriginalRecord_JianDing:
                case ExportType.OriginalRecord_XiaoZhun:
                    sheetName_Source = "封皮模板";
                    rowIndex_Source_ZhuangZhi = 29;
                    rowIndex_Source_QiJu = 32;
                    rowIndex_Source_ShiPin = 35;
                    break;
                case ExportType.Report_JianDing:
                case ExportType.Report_XiaoZhun:
                case ExportType.Report_XiaoZhun_CNAS:
                    sheetName_Source = "第二页模板";
                    rowIndex_Source_ZhuangZhi = 11;
                    rowIndex_Source_QiJu = 14;
                    rowIndex_Source_ShiPin = 17;
                    break;
            }
            ISheet sheet_Source = hssfworkbook.GetSheet(sheetName_Source);
            IMETERING_STANDARD_DEVICEBLL bll = new METERING_STANDARD_DEVICEBLL();
List<METERING_STANDARD_DEVICE> list = bll.GetPREPARE_SCHEME(entity.ID);
            //List<METERING_STANDARD_DEVICE> list =    bll.GetAll();//测试
            //if (entity.METERING_STANDARD_DEVICE != null)            
            //{
                //List<METERING_STANDARD_DEVICE> list = entity.METERING_STANDARD_DEVICE.ToList();
                
                if (list != null && list.Count > 0)
                {
                    //空三行
                    //CopyRow(sheet_Source, sheet_Destination, 2, rowIndex_Destination, 3, true);
                    //rowIndex_Destination = rowIndex_Destination + 3;

                    //标准装置
                    List<METERING_STANDARD_DEVICE> listZhuanZhi = list.FindAll(p => p.CATEGORY == "标准装置");
                    SetZhuangZhi(sheet_Source, sheet_Destination, rowIndex_Source_ZhuangZhi, ref rowIndex_Destination, CATEGORYType.标准装置, listZhuanZhi);
                    //标准器
                    List<METERING_STANDARD_DEVICE> listQiJu = list.FindAll(p => p.CATEGORY == "标准器");
                    SetZhuangZhi(sheet_Source, sheet_Destination, rowIndex_Source_QiJu, ref rowIndex_Destination, CATEGORYType.标准器, listQiJu);
                    //中间试品
                    List<METERING_STANDARD_DEVICE> listShiPin = list.FindAll(p => p.CATEGORY == "中间试品");
                    SetZhuangZhi(sheet_Source, sheet_Destination, rowIndex_Source_ShiPin, ref rowIndex_Destination, CATEGORYType.中间试品, listShiPin);
                }
            //}

        }
        /// <summary>
        /// 设置标准装置/计量标准器信息
        /// </summary>
        /// <param name="sheet_Source">源sheet</param>
        /// <param name="sheet_Destination">目标sheet</param>
        /// <param name="rowIndex_Source">源行号</param>
        /// <param name="rowIndex_Destination">目标行号</param>
        /// <param name="type">装置类型</param>
        /// <param name="listZhuanZhi">装置实体</param>
        private void SetZhuangZhi(ISheet sheet_Source, ISheet sheet_Destination, int rowIndex_Source, ref int rowIndex_Destination, CATEGORYType type = CATEGORYType.标准装置, List<METERING_STANDARD_DEVICE> listZhuanZhi = null)
        {
            if (listZhuanZhi != null && listZhuanZhi.Count > 0)
            {
                //标题
                CopyRow(sheet_Source, sheet_Destination, rowIndex_Source, rowIndex_Destination, 1, true);
                rowIndex_Source++;
                rowIndex_Destination++;
                //表头
                CopyRow(sheet_Source, sheet_Destination, rowIndex_Source, rowIndex_Destination, 1, true);
                rowIndex_Source++;
                rowIndex_Destination++;

                #region 数据
                foreach (METERING_STANDARD_DEVICE item in listZhuanZhi)
                {
                    CopyRow(sheet_Source, sheet_Destination, rowIndex_Source, rowIndex_Destination, 1, false);
                    //名称
                    sheet_Destination.GetRow(rowIndex_Destination).GetCell(0).SetCellValue(item.NAME);
                    if (type == CATEGORYType.标准装置)
                    {
                        //测量范围
                        item.TEST_RANGE = item.TEST_RANGE.Replace(";", Environment.NewLine);
                        sheet_Destination.GetRow(rowIndex_Destination).GetCell(7).SetCellValue(item.TEST_RANGE);
                    }
                    else
                    {
                        //型号
                        sheet_Destination.GetRow(rowIndex_Destination).GetCell(7).SetCellValue(item.XINGHAO);
                        //编号
                        sheet_Destination.GetRow(rowIndex_Destination).GetCell(10).SetCellValue(item.FACTORY_NUM);
                    }

                    #region 不确定度/准确度等级/最大允许误差
                    //不确定度/准确度等级/最大允许误差
                    List<ALLOWABLE_ERROR> aList = item.ALLOWABLE_ERROR.ToList();
                    if (aList != null && aList.Count > 0)
                    {
                        //rowIndex_Source++;
                        string aValue = "";
                        foreach (ALLOWABLE_ERROR aItem in aList)
                        {
                            if (!string.IsNullOrWhiteSpace(aItem.THEACCURACYLEVEL))
                            {

                                aValue += aItem.THEACCURACYLEVEL + ",";
                            }
                            else if (!string.IsNullOrWhiteSpace(aItem.MAXCATEGORIES) || !string.IsNullOrWhiteSpace(aItem.MAXVALUE))
                            {

                                aValue += aItem.MAXCATEGORIES + aItem.MAXVALUE + ",";
                            }
                            else if (!string.IsNullOrWhiteSpace(aItem.THEUNCERTAINTY) || !string.IsNullOrWhiteSpace(aItem.THEUNCERTAINTYVALUE) || !string.IsNullOrWhiteSpace(aItem.THEUNCERTAINTYNDEXL) || !string.IsNullOrWhiteSpace(aItem.THEUNCERTAINTYVALUEK))
                            {

                                // aValue += aItem.THEUNCERTAINTY + aItem.THEUNCERTAINTYVALUE + aItem.THEUNCERTAINTYNDEXL +"|"+ aItem.THEUNCERTAINTYVALUEK + ",";
                                aValue += aItem.THEUNCERTAINTY + aItem.THEUNCERTAINTYVALUE + aItem.THEUNCERTAINTYNDEXL + "|,";
                            }

                        }
                        if (!string.IsNullOrEmpty(aValue) && aValue.Trim() != "")
                        {
                            if (aValue.IndexOf("|,") > 0)
                            {
                                aValue = aValue.Trim().Remove(aValue.Trim().Length - 2);
                            }
                            else
                            {
                                aValue = aValue.Trim().Remove(aValue.Trim().Length - 1);
                            }

                            //aValue = aValue.Replace(",", Environment.NewLine);
                            HSSFRichTextString value = SetSub((HSSFWorkbook)sheet_Destination.Workbook, null, aValue);
                            sheet_Destination.GetRow(rowIndex_Destination).GetCell(13).SetCellValue(value);
                        }
                        else
                        {
                            sheet_Destination.GetRow(rowIndex_Destination).GetCell(13).SetCellValue(aValue);
                        }
                    }
                    #endregion

                    #region 证书编号 有效期至
                    List<METERING_STANDARD_DEVICE_CHECK> mList = item.METERING_STANDARD_DEVICE_CHECK.ToList();
                    if (mList != null && mList.Count > 0)
                    {
                        string cValue = "";//证书编号
                        string vValue = "";//有效期
                        foreach (METERING_STANDARD_DEVICE_CHECK mItem in mList)
                        {
                            if (mItem != null && !string.IsNullOrEmpty(mItem.CERTIFICATE_NUM) && mItem.CERTIFICATE_NUM.Trim() != "")
                            {
                                cValue += mItem.CERTIFICATE_NUM + ",";
                            }
                            if (mItem != null && mItem.VALID_TO.HasValue)
                            {
                                vValue += mItem.VALID_TO.Value.ToString("yyyy/MM/dd") + ",";
                            }

                        }
                        if (!string.IsNullOrEmpty(cValue) && cValue.Trim() != "")
                        {
                            cValue = cValue.Trim().Remove(cValue.Trim().Length - 1);
                            cValue = cValue.Replace(",", Environment.NewLine);
                        }
                        if (!string.IsNullOrEmpty(vValue) && vValue.Trim() != "")
                        {
                            vValue = vValue.Trim().Remove(vValue.Trim().Length - 1);
                            vValue = vValue.Replace(",", Environment.NewLine);
                        }
                        sheet_Destination.GetRow(rowIndex_Destination).GetCell(20).SetCellValue(cValue);
                        sheet_Destination.GetRow(rowIndex_Destination).GetCell(27).SetCellValue(vValue);
                    }
                    #endregion
                    rowIndex_Destination++;
                }

                #endregion
            }
        }

        /// <summary>
        /// 设置数据信息
        /// </summary>
        /// <param name="hssfworkbook">工作文件</param>
        /// <param name="entity">预备方案对象</param>
        /// <param name="type">导出类型</param>
        private void SetShuJu(IWorkbook hssfworkbook, PREPARE_SCHEME entity, ExportType type = ExportType.OriginalRecord_JianDing)
        {
            List<VTEST_ITE> vList = null;
            if (entity != null)
            {
                IBLL.IVTEST_ITEBLL vBLL = new VTEST_ITEBLL();
                vList = vBLL.GetByPREPARE_SCHEMEID(entity.ID);
            }
            int RowIndex = 1;
            int JWTemplateIndex = 0;//规程标题获取源格式行   
            int ruleTitleTemplateIndex = 1;//检测项目名称
            string sheetName_Source = "数据模板";
            string sheetName_Destination = "数据";
            ISheet sheet_Source = hssfworkbook.GetSheet(sheetName_Source);
            ISheet sheet_Destination = hssfworkbook.GetSheet(sheetName_Destination);
            #region 检测项目            
            if (vList != null && vList.Count > 0)
            {

                TableTemplates allTableTemplates = GetTableTemplates(type);
                SpecialCharacters allSpecialCharacters = GetSpecialCharacters();


                entity.QUALIFIED_UNQUALIFIED_TEST_ITE = entity.QUALIFIED_UNQUALIFIED_TEST_ITE.OrderBy(p => p.SORT).ToList();
                int i = 1;
                string SameRuleName = "";
                List<string> SameRuleNameList = GetSameRuleName();
                QUALIFIED_UNQUALIFIED_TEST_ITE iEntity = null;
                foreach (VTEST_ITE iVTEST_ITE in vList)
                {
                    if (string.IsNullOrWhiteSpace(iVTEST_ITE.PARENTID))//一级检测项不打印
                    {
                        continue;
                    }
                    if (entity.QUALIFIED_UNQUALIFIED_TEST_ITE != null && entity.QUALIFIED_UNQUALIFIED_TEST_ITE.Count > 0 && entity.QUALIFIED_UNQUALIFIED_TEST_ITE.FirstOrDefault(p => p.RULEID == iVTEST_ITE.RULEID) != null)
                    {
                        iEntity = entity.QUALIFIED_UNQUALIFIED_TEST_ITE.FirstOrDefault(p => p.RULEID == iVTEST_ITE.RULEID);
                    }
                    else
                    {
                        iEntity = null;
                    }

                    #region 检测项目标题     
                    //相同检测项只展示一个标题      

                    CopyRow(sheet_Source, sheet_Destination, ruleTitleTemplateIndex, RowIndex, 1, false);
                    string celStr = i.ToString() + "、";

                    if (iVTEST_ITE.NAME != null && iVTEST_ITE.NAME.Trim() != "")
                    {
                        celStr = celStr + iVTEST_ITE.NAME.Trim() + "：";
                    }
                    //结论,只有非表格的才需要打结论
                    if (iEntity != null && (iVTEST_ITE.INPUTSTATE==InputStateEnums.HGBHG.ToString() || iVTEST_ITE.INPUTSTATE== InputStateEnums.WBK.ToString()) && iEntity.CONCLUSION != null && iEntity.CONCLUSION.Trim() != "")
                    {
                        celStr = celStr + iEntity.CONCLUSION.Trim();
                    }
                    else if (iEntity == null)
                    {
                        celStr = celStr + "/";
                    }
                    sheet_Destination.GetRow(RowIndex).GetCell(0).SetCellValue(celStr);
                    RowIndex++;

                    //相同检测项只展示一个标题   
                    if (SameRuleNameList != null && SameRuleNameList.Count > 0 && SameRuleNameList.FirstOrDefault(p => p == iVTEST_ITE.NAME) != null && SameRuleName == iVTEST_ITE.NAME)

                    {
                        HideRow(sheet_Destination, RowIndex - 2, 2);
                    }
                    else
                    {
                        i++;
                    }

                    #endregion

                    #region 检测项目表格                    

                    //if (TableTemplateDic != null && TableTemplateDic.ContainsKey(iEntity.RULEID))
                    //{
                    //    TableTemplateExt temp = TableTemplateDic[iEntity.INPUTSTATE];                       
                    if (iEntity != null 
                        && allTableTemplates != null && allTableTemplates.TableTemplateList != null && allTableTemplates.TableTemplateList.Count > 0 && allTableTemplates.TableTemplateList.FirstOrDefault(p => p.RuleID == iEntity.RULEID) != null)
                    {

                        if (SameRuleNameList != null && SameRuleNameList.Count > 0 && SameRuleNameList.FirstOrDefault(p => p == iVTEST_ITE.NAME) != null && SameRuleName == iVTEST_ITE.NAME)

                        {
                            //为了相同项表格底部没有线
                            CopyRow(sheet_Source, sheet_Destination, 4, RowIndex, 1, true);
                            HideRow(sheet_Destination, RowIndex, 1);
                            RowIndex++;
                        }

                        //&&iEntity.RULEID== "126-1995_2_4_1"
                        TableTemplate temp = allTableTemplates.TableTemplateList.FirstOrDefault(p => p.RuleID == iEntity.RULEID);
                        //解析html表格数据    

                        //RowIndex = paserData(iEntity.HTMLVALUE, sheet_Source, sheet_Destination, RowIndex, temp, allSpecialCharacters);
                        RowIndex = paserData_1(iEntity.HTMLVALUE, sheet_Source, sheet_Destination, RowIndex, temp, allSpecialCharacters);


                        //表格注
                        if (iEntity.REMARK != null && iEntity.REMARK.Trim() != "")
                        {
                            CopyRow(sheet_Source, sheet_Destination, temp.RemarkRowIndex, RowIndex, 1, true);
                            sheet_Destination.GetRow(RowIndex).GetCell(0).SetCellValue("注：" + iEntity.REMARK);
                            RowIndex++;
                        }

                        //表格结论
                        if (iEntity.CONCLUSION != null && iEntity.CONCLUSION.Trim() != "")
                        {
                            CopyRow(sheet_Source, sheet_Destination, temp.ConclusionRowIndex, RowIndex, 1, true);
                            sheet_Destination.GetRow(RowIndex).GetCell(0).SetCellValue("结论：" + iEntity.CONCLUSION);
                            RowIndex++;
                        }
                        //为了表格底部没有线
                        CopyRow(sheet_Source, sheet_Destination, 4, RowIndex, 1, true);
                    }
                    else
                    {
                        //增加一行空行
                        CopyRow(sheet_Source, sheet_Destination, 5, RowIndex, 1, true);
                    }


                    RowIndex++;
                    SameRuleName = iVTEST_ITE.NAME;

                    #endregion
                    //i++;

                }
            }
            #endregion
            //结尾             
            CopyRow(sheet_Source, sheet_Destination, JWTemplateIndex, RowIndex, 1, true);
            //设置页面页脚
            SetHeaderAndFooter(sheet_Destination, entity, type);
            sheet_Destination.ForceFormulaRecalculation = true;
        }
        /// <summary>
        /// 设置页眉页脚
        /// </summary>
        /// <param name="sheet_Destination">目标sheet</param>
        /// <param name="entity">预备方案</param>
        private void SetHeaderAndFooter(ISheet sheet_Destination, PREPARE_SCHEME entity, ExportType type = ExportType.OriginalRecord_JianDing)
        {
            string REPORTNUMBER = entity.REPORTNUMBER;
            //页眉
            string header = "原始记录编号：";
            if (type != ExportType.OriginalRecord_JianDing && type != ExportType.OriginalRecord_XiaoZhun)
            {
                header = "证书编号：";
            }
            else if (REPORTNUMBER != null && REPORTNUMBER.Trim() != "")
            {
                int index = entity.REPORTNUMBER.IndexOf("-");
                REPORTNUMBER = REPORTNUMBER.Split('-')[0] + "原始" + REPORTNUMBER.Substring(index);
            }
            if (REPORTNUMBER != null && REPORTNUMBER.Trim() != "")
            {
                sheet_Destination.Header.Left = header + REPORTNUMBER;
            }
            else
            {
                sheet_Destination.Header.Left = header;
            }
            //页脚
            if (entity.CERTIFICATE_CATEGORY == ZhengShuLeiBieEnums.校准.ToString() && entity.CNAS == ShiFouCNAS.Yes.ToString() && entity.REPORTNUMBER != null && entity.REPORTNUMBER.Trim() != "")
            {
                if (REPORTNUMBER != null && REPORTNUMBER.Trim() != "")
                {
                    sheet_Destination.Footer.Left = REPORTNUMBER;
                }
            }
        }
        #region 复制行


        /// <summary>
        /// 复制行格式并插入指定行数(返回动态区域)
        /// </summary>
        /// <param name="sheet_Source">源sheet</param>
        /// <param name="sheet_Destination">目标sheet</param>
        /// <param name="rowIndex_Source">源行号</param>
        /// <param name="rowIndex_Destination">目标行号</param>
        /// <param name="insertCount">插入行数</param>
        /// <param name="IsCopyContent">是否拷贝内容</param>
        /// <param name="rowInfoList">需要替换的动态模板数据</param>
        /// <param name="allSpecialCharacters">特殊字符配置信息</param>
        /// <param name="DongTaiShuJuList">需要替换的动态数据</param>
        private Dictionary<string, CellRangeAddress> CopyRow_1(ISheet sheet_Source, ISheet sheet_Destination, int rowIndex_Source, int rowIndex_Destination, int insertCount, bool IsCopyContent, List<RowInfo> rowInfoList, SpecialCharacters allSpecialCharacters, List<MYDataHead> DongTaiShuJuList)
        {
            //key：//第几行_第几列 
            Dictionary<string, CellRangeAddress> result = new Dictionary<string, CellRangeAddress>();
            string key = "";//第几行_第几列 
            int colCount = 0;
            IRow row_Source = sheet_Source.GetRow(rowIndex_Source);
            int sourceCellCount = row_Source.Cells.Count;
            if (insertCount <= 0)
            {
                insertCount = 1;
            }
            //1. 批量移动行,清空插入区域
            sheet_Destination.ShiftRows(rowIndex_Destination, //开始行
                             sheet_Destination.LastRowNum, //结束行
                             insertCount, //插入行总数
                             true,        //是否复制行高
                             false        //是否重置行高
                             );

            int startMergeCell = -1; //记录每行的合并单元格起始位置
            //int endMergeCell = -1;//记录每行的合并单元结束位置     
            //给目标行正式处理格式及插入多少行数据       
            for (int i = rowIndex_Destination; i < rowIndex_Destination + insertCount; i++)
            {
                startMergeCell = -1;
                IRow targetRow = null;
                ICell sourceCell = null;
                ICell targetCell = null;

                targetRow = sheet_Destination.CreateRow(i);
                targetRow.Height = row_Source.Height;//复制行高 
                //每行单元格处理               
                for (int m = row_Source.FirstCellNum; m < row_Source.LastCellNum; m++)
                {
                    if (m < 57)
                    {
                        sourceCell = row_Source.GetCell(m);
                        row_Source.Cells[m].SetCellType(CellType.String);
                        if (m + 1 != row_Source.LastCellNum)
                        {
                            row_Source.Cells[m + 1].SetCellType(CellType.String);
                        }
                        if (sourceCell == null)
                            continue;
                        targetCell = targetRow.CreateCell(m);
                        targetCell.CellStyle = sourceCell.CellStyle;//赋值单元格格式
                        targetCell.SetCellType(sourceCell.CellType);

                        //列合并，以下为复制模板行的单元格合并格式                    
                        #region 新合并方式                   
                        if (sourceCell.IsMergedCell)
                        {
                            CellRangeAddress cellAddress = getMergedRegionCell(sheet_Source, m, rowIndex_Source);
                            if (cellAddress != null && cellAddress.LastColumn > startMergeCell && (cellAddress.LastRow > cellAddress.FirstRow || cellAddress.LastColumn > cellAddress.FirstColumn))
                            {
                                if (rowIndex_Source == cellAddress.LastRow)
                                {
                                    sheet_Destination.AddMergedRegion(new CellRangeAddress(i - (cellAddress.LastRow - cellAddress.FirstRow), i, cellAddress.FirstColumn, cellAddress.LastColumn));
                                    startMergeCell = cellAddress.LastColumn + 1;
                                    if (m == 0)
                                    {
                                        colCount = 1;
                                    }
                                    else
                                    {
                                        colCount++;
                                    }
                                    key = (i - (cellAddress.LastRow - cellAddress.FirstRow)).ToString() + "_" + colCount.ToString();//第几行_第几列 
                                                                                                                                    //result.Add(new CellRangeAddress(i - (cellAddress.LastRow - cellAddress.FirstRow), i, cellAddress.FirstColumn, cellAddress.LastColumn));
                                    result.Add(key, new CellRangeAddress(i - (cellAddress.LastRow - cellAddress.FirstRow), i, cellAddress.FirstColumn, cellAddress.LastColumn));
                                }
                                if (IsCopyContent && rowIndex_Source == cellAddress.FirstRow)
                                {
                                    HSSFRichTextString value = GetDongTaiShuJu(DongTaiShuJuList, rowInfoList, row_Source.Cells[m], targetRow.Cells[m], allSpecialCharacters);
                                    targetRow.Cells[m].SetCellValue(value);

                                }
                            }

                        }
                        else
                        {
                            //colIndex++;
                            //result.Add(new CellRangeAddress(targetRow.RowNum, targetRow.RowNum, m, m));
                            if (m == 0)
                            {
                                colCount = 1;
                            }
                            else
                            {
                                colCount++;
                            }
                            key = targetRow.RowNum.ToString() + "_" + colCount.ToString();//第几行_第几列 
                            result.Add(key, new CellRangeAddress(targetRow.RowNum, targetRow.RowNum, m, m));
                            if (IsCopyContent)
                            {
                                HSSFRichTextString value = GetDongTaiShuJu(DongTaiShuJuList, rowInfoList, row_Source.Cells[m], targetRow.Cells[m], allSpecialCharacters);
                                targetRow.Cells[m].SetCellValue(value);
                            }
                        }
                        #endregion
                    }
                }
            }
            return result;
        }




        /// <summary>
        /// 复制行格式并插入指定行数
        /// </summary>
        /// <param name="sheet_Source">源sheet</param>
        /// <param name="sheet_Destination">目标sheet</param>
        /// <param name="rowIndex_Source">源行号</param>
        /// <param name="rowIndex_Destination">目标行号</param>
        /// <param name="insertCount">插入行数</param>
        /// <param name="IsCopyContent">是否拷贝内容</param>
        /// <param name="DongTaiShuJuList">需要替换的动态数据</param>
        /// <param name="rowInfoList">需要替换的动态数据位置</param>
        /// <param name="allSpecialCharacters">特殊字符配置信息</param>
        private void CopyRow(ISheet sheet_Source, ISheet sheet_Destination, int rowIndex_Source, int rowIndex_Destination, int insertCount, bool IsCopyContent = false, Dictionary<string, string> DongTaiShuJuList = null, List<RowInfo> rowInfoList = null, SpecialCharacters allSpecialCharacters = null, List<Cell> CellsTemplate = null)
        {
            IRow row_Source = sheet_Source.GetRow(rowIndex_Source);
            int sourceCellCount = row_Source.Cells.Count;

            //1. 批量移动行,清空插入区域
            sheet_Destination.ShiftRows(rowIndex_Destination, //开始行
                             sheet_Destination.LastRowNum, //结束行
                             insertCount, //插入行总数
                             true,        //是否复制行高
                             false        //是否重置行高
                             );

            int startMergeCell = -1; //记录每行的合并单元格起始位置
            int endMergeCell = -1;//记录每行的合并单元结束位置     
            //给目标行正式处理格式及插入多少行数据       
            for (int i = rowIndex_Destination; i < rowIndex_Destination + insertCount; i++)
            {
                IRow targetRow = null;
                ICell sourceCell = null;
                ICell targetCell = null;

                targetRow = sheet_Destination.CreateRow(i);
                targetRow.Height = row_Source.Height;//复制行高 
                //每行单元格处理               
                for (int m = row_Source.FirstCellNum; m < row_Source.LastCellNum; m++)
                {
                    if (m < 57)
                    {
                        sourceCell = row_Source.GetCell(m);
                        row_Source.Cells[m].SetCellType(CellType.String);
                        if (m + 1 != row_Source.LastCellNum)
                        {
                            row_Source.Cells[m + 1].SetCellType(CellType.String);
                        }
                        if (sourceCell == null)
                            continue;
                        targetCell = targetRow.CreateCell(m);
                        targetCell.CellStyle = sourceCell.CellStyle;//赋值单元格格式
                        targetCell.SetCellType(sourceCell.CellType);

                        //列合并，以下为复制模板行的单元格合并格式
                        #region 原合并方式
                        //if (sourceCell.IsMergedCell)
                        //{
                        //    //起始位置
                        //    if (startMergeCell < 0 || sourceCell.CellStyle.BorderLeft != BorderStyle.None || sourceCell.StringCellValue != "")
                        //    {
                        //        startMergeCell = m;
                        //    }
                        //    //结束位置
                        //    if (m + 1 == sourceCellCount || sourceCell.CellStyle.BorderRight != BorderStyle.None
                        //        || row_Source.Cells[m + 1].StringCellValue != "" || row_Source.Cells[m + 1].IsMergedCell == false
                        //        || (CellsTemplate != null && CellsTemplate.Count > 0 && CellsTemplate.FirstOrDefault(p => p.ColIndex == startMergeCell && p.ColCount == (m - startMergeCell)) != null))
                        //    {
                        //        endMergeCell = m;
                        //    }

                        //    if (startMergeCell < endMergeCell)
                        //    {
                        //        //int firstRow, int lastRow, int firstCol, int lastCol
                        //        sheet_Destination.AddMergedRegion(new CellRangeAddress(i, i, startMergeCell, endMergeCell));
                        //        //CellRangeAddress cellAddress = getMergedRegionCell(m, i);
                        //        //if (cellAddress != null)
                        //        //{
                        //        //    sheet_Destination.AddMergedRegion(new CellRangeAddress(i, cellAddress.LastRow-cellAddress.FirstRow+i, cellAddress.FirstColumn, cellAddress.LastColumn));
                        //        //}
                        //        //else
                        //        //{
                        //        //    sheet_Destination.AddMergedRegion(new CellRangeAddress(i, i, startMergeCell, endMergeCell));
                        //        //}


                        //        if (IsCopyContent)
                        //        {
                        //            HSSFRichTextString value = GetDongTaiShuJu(DongTaiShuJuList, rowInfoList, row_Source.Cells[startMergeCell], targetRow.Cells[startMergeCell], allSpecialCharacters);
                        //            targetRow.Cells[startMergeCell].SetCellValue(value);
                        //            startMergeCell = -1;

                        //        }
                        //    }
                        //}
                        //else
                        //{                       
                        //    if (IsCopyContent)
                        //    {

                        //        HSSFRichTextString value = GetDongTaiShuJu(DongTaiShuJuList, rowInfoList, row_Source.Cells[m], targetRow.Cells[m], allSpecialCharacters);
                        //        targetRow.Cells[m].SetCellValue(value);

                        //    }
                        //    startMergeCell = -1;                       
                        //}
                        #endregion
                        #region 新合并方式                   
                        if (sourceCell.IsMergedCell)
                        {
                            CellRangeAddress cellAddress = getMergedRegionCell(sheet_Source, m, rowIndex_Source);
                            if (cellAddress != null && cellAddress.LastColumn > startMergeCell && (cellAddress.LastRow > cellAddress.FirstRow || cellAddress.LastColumn > cellAddress.FirstColumn))
                            {
                                if (rowIndex_Source == cellAddress.LastRow)
                                {
                                    sheet_Destination.AddMergedRegion(new CellRangeAddress(i - (cellAddress.LastRow - cellAddress.FirstRow), i, cellAddress.FirstColumn, cellAddress.LastColumn));
                                    startMergeCell = cellAddress.LastColumn + 1;
                                }
                                if (IsCopyContent && rowIndex_Source == cellAddress.FirstRow)
                                {
                                    HSSFRichTextString value = GetDongTaiShuJu(DongTaiShuJuList, rowInfoList, row_Source.Cells[m], targetRow.Cells[m], allSpecialCharacters);
                                    targetRow.Cells[m].SetCellValue(value);
                                }
                            }

                        }
                        else
                        {
                            if (IsCopyContent)
                            {
                                HSSFRichTextString value = GetDongTaiShuJu(DongTaiShuJuList, rowInfoList, row_Source.Cells[m], targetRow.Cells[m], allSpecialCharacters);
                                targetRow.Cells[m].SetCellValue(value);
                            }
                        }
                        #endregion
                    }
                }
            }
        }

        /// <summary>
        /// 获取单元格数据及动态数据组合
        /// </summary>
        /// <param name="DongTaiShuJuList">动态数据值</param>
        /// <param name="rowInfoList">动态数据位置</param>
        /// <param name="sourceCell">单元格</param>
        /// <param name="targetCell">目标单元格</param>
        /// <param name="allSpecialCharacters">特殊字符配置信息</param>
        /// <returns></returns>
        private HSSFRichTextString GetDongTaiShuJu(List<MYDataHead> DongTaiShuJuList = null, List<RowInfo> rowInfoList = null, ICell sourceCell = null, ICell targetCell = null, SpecialCharacters allSpecialCharacters = null)
        {
            HSSFWorkbook workbook = null;
            if (targetCell != null && targetCell.Sheet != null && targetCell.Sheet.Workbook != null)
            {
                workbook = (HSSFWorkbook)targetCell.Sheet.Workbook;
            }
            HSSFRichTextString result = null;
            if (sourceCell == null)
            {
                return new HSSFRichTextString("");
            }

            int speStartIndex = sourceCell.StringCellValue.IndexOf("{0}");//动态字符位置
            string value = "";
            string SpecialStr = "";
            //有动态数据
            if (speStartIndex >= 0 && DongTaiShuJuList != null && DongTaiShuJuList.Count > 0 && rowInfoList != null && rowInfoList.Count > 0 &&
                rowInfoList.FirstOrDefault().Cells != null)
            {
                while (DongTaiShuJuList != null && DongTaiShuJuList.Count > 0 && rowInfoList.FirstOrDefault().Cells.FirstOrDefault(p => p.Code == DongTaiShuJuList.FirstOrDefault().name) == null)
                {
                    DongTaiShuJuList.RemoveAt(0);
                }
                if (DongTaiShuJuList != null && DongTaiShuJuList.Count > 0)
                {
                    value = string.Format(sourceCell.StringCellValue, DongTaiShuJuList.FirstOrDefault().value).Trim();
                    SpecialStr = DongTaiShuJuList.FirstOrDefault().value;
                    DongTaiShuJuList.RemoveAt(0);
                }
                else
                {
                    value = string.Format(sourceCell.StringCellValue, "").Trim();
                    speStartIndex = 0;
                    SpecialStr = value;
                }
            }
            //无动态数据
            else
            {
                value = string.Format(sourceCell.StringCellValue, "").Trim();
                speStartIndex = 0;
                SpecialStr = value;
            }
            if (value != null && value.Trim() != "" && value.Trim().ToUpper().IndexOf("U(K") >= 0)
            {
                speStartIndex = value.Trim().ToUpper().IndexOf("U(K");
                SpecialStr = "U(K";
            }
            else if (value != null && value.Trim() != "" && value.Trim().ToUpper().IndexOf("UI") >= 0)
            {
                speStartIndex = value.Trim().ToUpper().IndexOf("UI");
                SpecialStr = "UI";
            }
            //处理特殊字符下标上标斜体
            result = new HSSFRichTextString(value);

            if (!string.IsNullOrEmpty(SpecialStr) && SpecialStr.Trim() != "" && speStartIndex >= 0)
            {
                //特殊字符是否配置
                if (workbook != null && allSpecialCharacters != null && allSpecialCharacters.SpecialCharacterList != null &&
                        allSpecialCharacters.SpecialCharacterList.Count > 0 &&
                        allSpecialCharacters.SpecialCharacterList.FirstOrDefault(p => p.Code.Trim().ToUpper() == SpecialStr.Trim().ToUpper()) != null)
                {
                    SpecialCharacter spec = allSpecialCharacters.SpecialCharacterList.FirstOrDefault(p => p.Code.Trim().ToUpper() == SpecialStr.Trim().ToUpper());
                    #region 将字符设置成斜体

                    HSSFFont normalFont = (HSSFFont)workbook.CreateFont();
                    normalFont.IsItalic = true;
                    normalFont.FontName = "宋体";
                    int startIndex = speStartIndex;
                    if (startIndex < 0)
                    {
                        startIndex = 0;
                    }
                    int endIndex = speStartIndex + spec.Code.Trim().Length - spec.SubscriptLastCount;
                    if (endIndex < 0)
                    {
                        endIndex = 0;
                    }
                    result.ApplyFont(startIndex, endIndex, normalFont);
                    #endregion

                    #region 设置下标
                    if (spec.SubscriptLastCount > 0)
                    {
                        //result = new HSSFRichTextString(value);
                        // superscript = (HSSFFont)workbook.CreateFont();
                        //superscript.TypeOffset = FontSuperScript.Super;//上标
                        //superscript.Color = HSSFColor.RED.index;

                        HSSFFont subscript = (HSSFFont)workbook.CreateFont();
                        subscript.TypeOffset = FontSuperScript.Sub; //下标  
                        //subscript.IsItalic = true;
                        subscript.FontName = "宋体";
                        //subscript.Color = HSSFColor.Red.Index;
                        //HSSFFont normalFont = (HSSFFont)workbook.CreateFont();
                        startIndex = speStartIndex + spec.Code.Trim().Length - spec.SubscriptLastCount;
                        if (startIndex < 0)
                        {
                            startIndex = 0;
                        }
                        endIndex = speStartIndex + spec.Code.Trim().Length;
                        if (endIndex < 0)
                        {
                            endIndex = 0;
                        }
                        result.ApplyFont(startIndex, endIndex, subscript);
                    }
                    #endregion

                }
                return result;
            }

            return new HSSFRichTextString(string.Format(sourceCell.StringCellValue, ""));
        }


        /// <summary>
        /// 获取单元格数据及动态数据组合
        /// </summary>
        /// <param name="DongTaiShuJuList">动态数据值</param>
        /// <param name="rowInfoList">动态数据位置</param>
        /// <param name="sourceCell">单元格</param>
        /// <param name="targetCell">目标单元格</param>
        /// <param name="allSpecialCharacters">特殊字符配置信息</param>
        /// <returns></returns>
        private HSSFRichTextString GetDongTaiShuJu(Dictionary<string, string> DongTaiShuJuList = null, List<RowInfo> rowInfoList = null, ICell sourceCell = null, ICell targetCell = null, SpecialCharacters allSpecialCharacters = null)
        {

            HSSFWorkbook workbook = null;
            if (targetCell != null && targetCell.Sheet != null && targetCell.Sheet.Workbook != null)
            {
                workbook = (HSSFWorkbook)targetCell.Sheet.Workbook;
            }
            HSSFRichTextString result = null;
            if (sourceCell == null)
            {
                return new HSSFRichTextString("");
            }
            string key = "";
            //xml是否配置了信息
            //< Cell >

            //                < !--字段代码与模板中设置的字段保持一致-- >

            //                < Code > sssss </ Code >

            //                < !--字段名称含义-- >

            //                < Name > 显示值 </ Name >

            //                < !--模板中单元格开始列号-- >

            //                < ColIndex > 21 </ ColIndex >

            //                < !--所占列数-- >

            //                < ColCount > 13 </ ColCount >

            //            </ Cell >
            if (rowInfoList != null && rowInfoList.Count > 0 && rowInfoList.FirstOrDefault(p => p.RowIndex == sourceCell.RowIndex && p.Cells != null && p.Cells.Count > 0) != null)
            {
                RowInfo r = rowInfoList.FirstOrDefault(p => p.RowIndex == sourceCell.RowIndex && p.Cells != null && p.Cells.Count > 0);
                if (r != null)
                {
                    Cell c = r.Cells.FirstOrDefault(p => p.ColIndex == sourceCell.ColumnIndex);
                    if (c != null && DongTaiShuJuList.ContainsKey(c.Code))
                    {
                        key = c.Code;
                    }
                }
            }
            int speStartIndex = sourceCell.StringCellValue.IndexOf("{0}");//动态字符位置
            string value = "";
            string SpecialStr = "";
            //有动态数据
            if (key != null && key.Trim() != "")
            {
                value = string.Format(sourceCell.StringCellValue, DongTaiShuJuList[key]).Trim();
                SpecialStr = DongTaiShuJuList[key];
            }
            //无动态数据
            else
            {
                value = string.Format(sourceCell.StringCellValue, "").Trim();
                speStartIndex = 0;
                SpecialStr = value;
            }
            if (value != null && value.Trim() != "" && value.Trim().ToUpper().IndexOf("U(K") >= 0)
            {
                speStartIndex = value.Trim().ToUpper().IndexOf("U(K");
                SpecialStr = "U(K";
            }
            //处理特殊字符下标上标斜体
            result = new HSSFRichTextString(value);

            if (!string.IsNullOrEmpty(SpecialStr) && SpecialStr.Trim() != "" && speStartIndex >= 0)
            {
                //特殊字符是否配置
                if (workbook != null && allSpecialCharacters != null && allSpecialCharacters.SpecialCharacterList != null &&
                        allSpecialCharacters.SpecialCharacterList.Count > 0 &&
                        allSpecialCharacters.SpecialCharacterList.FirstOrDefault(p => p.Code.Trim().ToUpper() == SpecialStr.Trim().ToUpper()) != null)
                {
                    SpecialCharacter spec = allSpecialCharacters.SpecialCharacterList.FirstOrDefault(p => p.Code.Trim().ToUpper() == SpecialStr.Trim().ToUpper());
                    #region 将字符设置成斜体

                    HSSFFont normalFont = (HSSFFont)workbook.CreateFont();
                    normalFont.IsItalic = true;
                    normalFont.FontName = "宋体";
                    int startIndex = speStartIndex;
                    if (startIndex < 0)
                    {
                        startIndex = 0;
                    }
                    int endIndex = speStartIndex + spec.Code.Trim().Length - spec.SubscriptLastCount;
                    if (endIndex < 0)
                    {
                        endIndex = 0;
                    }
                    result.ApplyFont(startIndex, endIndex, normalFont);
                    #endregion

                    #region 设置下标
                    if (spec.SubscriptLastCount > 0)
                    {
                        //result = new HSSFRichTextString(value);
                        // superscript = (HSSFFont)workbook.CreateFont();
                        //superscript.TypeOffset = FontSuperScript.Super;//上标
                        //superscript.Color = HSSFColor.RED.index;

                        HSSFFont subscript = (HSSFFont)workbook.CreateFont();
                        subscript.TypeOffset = FontSuperScript.Sub; //下标  
                        //subscript.IsItalic = true;
                        subscript.FontName = "宋体";
                        //subscript.Color = HSSFColor.Red.Index;
                        //HSSFFont normalFont = (HSSFFont)workbook.CreateFont();
                        startIndex = speStartIndex + spec.Code.Trim().Length - spec.SubscriptLastCount;
                        if (startIndex < 0)
                        {
                            startIndex = 0;
                        }
                        endIndex = speStartIndex + spec.Code.Trim().Length;
                        if (endIndex < 0)
                        {
                            endIndex = 0;
                        }
                        result.ApplyFont(startIndex, endIndex, subscript);
                    }
                    #endregion

                }
                if (!string.IsNullOrEmpty(key) && key.Trim() != "" && DongTaiShuJuList != null)
                {
                    DongTaiShuJuList.Remove(key);
                }
                return result;
            }

            return new HSSFRichTextString(string.Format(sourceCell.StringCellValue, ""));
        }
        /// <summary>
        /// 设置下标\斜体\宋体
        /// </summary>
        /// <param name="workbook">工作文件</param>
        /// <param name="allSpecialCharacters">特殊字符配置信息</param>
        /// <param name="value">特殊字符</param>
        /// <returns></returns>
        private HSSFRichTextString SetSub(HSSFWorkbook workbook = null, SpecialCharacters allSpecialCharacters = null, string value = "")
        {
            if (value == null)
            {
                value = "";
            }
            //HSSFWorkbook workbook = null;
            //if (targetCell != null && targetCell.Sheet != null && targetCell.Sheet.Workbook != null)
            //{
            //    workbook = (HSSFWorkbook)targetCell.Sheet.Workbook;
            //}
            HSSFRichTextString result = new HSSFRichTextString(value.Trim());

           
            if (workbook != null && value != null && value.Trim() != "" )
            {
                #region 设置上标
                #region 处理*10
                if(value.IndexOf("*10")>0 || value.IndexOf("×10") > 0)
                {
                    value = value.Replace(",", Environment.NewLine);
                    result = new HSSFRichTextString(value.Trim().Replace("|", ""));

                    string[] vArray = value.Trim().Split('|');
                    int length = 0;
                    int startIndex = 0;
                    int endIndex = 0;                    
                    foreach(string v in vArray)
                    {
                                               
                        if(v.IndexOf("*10") >= 0 || value.IndexOf("×10")>0)
                        {
                            if (v.IndexOf("*10") >= 0)
                            {
                                startIndex = length + v.IndexOf("*10") + 3;
                            }
                            else
                            {
                                startIndex = length + v.IndexOf("×10") + 3;
                            }
                            endIndex = length + v.Length;                            
                            HSSFFont superscript = (HSSFFont)workbook.CreateFont();
                            superscript.TypeOffset = FontSuperScript.Super;//上标
                            superscript.FontName = "宋体";
                            result.ApplyFont(startIndex, endIndex, superscript);
                        }
                        length = length + v.Length;

                    }
                }
                #endregion 
                #endregion

                if (allSpecialCharacters != null && allSpecialCharacters.SpecialCharacterList != null &&
                    allSpecialCharacters.SpecialCharacterList.Count > 0 &&
                    allSpecialCharacters.SpecialCharacterList.FirstOrDefault(p => p.Code.Trim().ToUpper() == value.Trim().ToUpper()) != null)
                {
                    SpecialCharacter spec = allSpecialCharacters.SpecialCharacterList.FirstOrDefault(p => p.Code.Trim().ToUpper() == value.Trim().ToUpper());
                    #region 将字符设置成斜体

                    HSSFFont normalFont = (HSSFFont)workbook.CreateFont();
                    normalFont.IsItalic = true;
                    normalFont.FontName = "宋体";
                    int startIndex = 0;
                    int endIndex = spec.Code.Trim().Length - 1;
                    if (endIndex < 0)
                    {
                        endIndex = 0;
                    }
                    result.ApplyFont(startIndex, endIndex, normalFont);
                    #endregion

                    #region 设置下标
                    if (spec.SubscriptLastCount > 0)
                    {
                        //result = new HSSFRichTextString(value);
                        // superscript = (HSSFFont)workbook.CreateFont();
                        //superscript.TypeOffset = FontSuperScript.Super;//上标
                        //superscript.Color = HSSFColor.RED.index;

                        HSSFFont subscript = (HSSFFont)workbook.CreateFont();
                        subscript.TypeOffset = FontSuperScript.Sub; //下标  
                        //subscript.IsItalic = true;
                        subscript.FontName = "宋体";
                        //subscript.Color = HSSFColor.Red.Index;
                        //HSSFFont normalFont = (HSSFFont)workbook.CreateFont();
                        startIndex = spec.Code.Trim().Length - spec.SubscriptLastCount;
                        if (startIndex < 0)
                        {
                            startIndex = 0;
                        }
                        endIndex = spec.Code.Trim().Length;
                        if (endIndex < 0)
                        {
                            endIndex = 0;
                        }
                        result.ApplyFont(startIndex, endIndex, subscript);
                    }
                    #endregion
                }
            }
            return result;

        }
        /// <summary>
        /// 获取单元格数据及动态数据组合
        /// </summary>
        /// <param name="DongTaiShuJuList">动态数据值</param>
        /// <param name="rowInfoList">动态数据位置</param>
        /// <param name="sourceCell">单元格</param>
        /// <returns></returns>
        private string GetDongTaiShuJu(List<string> DongTaiShuJuList = null, List<RowInfo> rowInfoList = null, ICell sourceCell = null)
        {
            if (sourceCell == null)
            {
                return "";
            }
            if (DongTaiShuJuList == null || DongTaiShuJuList.Count == 0)
            {
                return string.Format(sourceCell.StringCellValue, "");
            }
            if (rowInfoList == null || rowInfoList.Count == 0)
            {
                return string.Format(sourceCell.StringCellValue, "");
            }

            if (rowInfoList.FirstOrDefault(p => p.RowIndex == sourceCell.RowIndex && p.Cells != null && p.Cells.Count > 0) != null)
            {
                RowInfo r = rowInfoList.FirstOrDefault(p => p.RowIndex == sourceCell.RowIndex && p.Cells != null && p.Cells.Count > 0);
                if (r.Cells.Count(p => p.ColIndex == sourceCell.ColumnIndex) > 0)
                {
                    string value = string.Format(sourceCell.StringCellValue, DongTaiShuJuList[0]);
                    DongTaiShuJuList.RemoveAt(0);
                    return value;
                }
            }
            return string.Format(sourceCell.StringCellValue, "");
        }
        /// <summary>
        /// 复制行格式并插入指定行数
        /// </summary>
        /// <param name="sheet">当前sheet</param>
        /// <param name="startRowIndex">起始行位置</param>
        /// <param name="sourceRowIndex">模板行位置</param>
        /// <param name="insertCount">插入行数</param>
        /// <param name="IsCopyContent">是否复制内容</param>
        private void CopyRow(ISheet sheet, int startRowIndex, int sourceRowIndex, int insertCount, bool IsCopyContent = false)
        {
            IRow sourceRow = sheet.GetRow(sourceRowIndex);
            int sourceCellCount = sourceRow.Cells.Count;

            //1. 批量移动行,清空插入区域
            sheet.ShiftRows(startRowIndex, //开始行
                             sheet.LastRowNum, //结束行
                             insertCount, //插入行总数
                             true,        //是否复制行高
                             false        //是否重置行高
                             );

            int startMergeCell = -1; //记录每行的合并单元格起始位置
            for (int i = startRowIndex; i < startRowIndex + insertCount; i++)
            {
                IRow targetRow = null;
                ICell sourceCell = null;
                ICell targetCell = null;

                targetRow = sheet.CreateRow(i);
                targetRow.Height = sourceRow.Height;//复制行高

                for (int m = sourceRow.FirstCellNum; m < sourceRow.LastCellNum; m++)
                {
                    if (m < 57)
                    {
                        sourceCell = sourceRow.GetCell(m);
                        if (sourceCell == null)
                            continue;
                        targetCell = targetRow.CreateCell(m);
                        targetCell.CellStyle = sourceCell.CellStyle;//赋值单元格格式
                        targetCell.SetCellType(sourceCell.CellType);

                        //以下为复制模板行的单元格合并格式                                        
                        if (sourceCell.IsMergedCell)
                        {
                            if (startMergeCell <= 0)
                                startMergeCell = m;
                            else if (startMergeCell > 0 && sourceCellCount == m + 1)
                            {
                                sheet.AddMergedRegion(new CellRangeAddress(i, i, startMergeCell, m));
                                startMergeCell = -1;
                            }
                        }
                        else
                        {
                            if (startMergeCell >= 0)
                            {
                                sheet.AddMergedRegion(new CellRangeAddress(i, i, startMergeCell, m - 1));
                                startMergeCell = -1;
                            }
                        }
                    }
                    if (IsCopyContent)
                    {
                        sheet.CopyRow(sourceRowIndex, targetRow.RowNum);
                    }
                }
            }
            if (IsCopyContent)
            {
                #region 移除
                int StartIndex = startRowIndex + insertCount;
                int EndIndex = StartIndex + insertCount;
                for (int i = StartIndex; i <= EndIndex; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null)
                    {
                        continue;
                    }
                    sheet.RemoveRow(row);
                }

                #endregion
            }
        }

        #endregion
        /// <summary>
        /// 设置表格
        /// </summary>
        /// <param name="html"></param>
        /// <param name="sheet_Source">源sheet</param>     
        /// <param name="sheet_Destination">目标sheet</param>
        /// <param name="rowIndex_Destination">目标开始行号</param>
        /// <param name="temp">模板信息</param>                 
        /// <param name="allSpecialCharacters">特殊字符配置信息</param>
        /// <returns></returns>
        private int paserData_1(string html, ISheet sheet_Source, ISheet sheet_Destination, int rowIndex_Destination, TableTemplate temp, SpecialCharacters allSpecialCharacters = null)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            Dictionary<int, DataValue> dataDic = AnalyticHTML.GetData(doc);//数据
            Dictionary<int, List<MYDataHead>> headDic = AnalyticHTML.GetHeadData(doc);//表头
            Dictionary<int, List<MYDataHead>> footDic = AnalyticHTML.GetFootData(doc);//表尾

            #region 处理下一行数据为空需要单元格合并数据
            if (temp != null && temp.Cells != null && temp.Cells.Count > 0 && dataDic != null && dataDic.Count > 0)
            {
                List<Cell> cList = temp.Cells.FindAll(p => p.IsMergeNullValue == "Y");
                if (cList != null && cList.Count > 0)
                {
                    foreach (Cell c in cList)
                    {
                        foreach (DataValue d in dataDic.Values)
                        {
                            if (d != null && d.Data != null && d.Data.Count > 0)
                            {
                                List<MYData> cData = d.Data.FindAll(p => p.name == c.Code);
                                
                                MYData mdd = null;
                                List<MYData> removeList = new List<MYData>();
                                int indexCount = 1;
                                if (cData != null && cData.Count > 0)
                                {
                                    foreach (MYData md in cData)
                                    {
                                        if (indexCount == 1)
                                        {
                                            mdd = md;                                          
                                        }
                                        else
                                        {
                                            if (md != null && !string.IsNullOrWhiteSpace(md.value))
                                            {                                               
                                                mdd = md;
                                                mdd.mergedRowNum = md.mergedRowNum;
                                            }
                                            else if (md != null && string.IsNullOrWhiteSpace(md.value))
                                            {                                                
                                                mdd.mergedRowNum = mdd.mergedRowNum + md.mergedRowNum;
                                                removeList.Add(md);
                                            }
                                        }                                       
                                        indexCount++;
                                    }
                                    foreach (MYData md in removeList)
                                    {
                                        d.Data.Remove(md);
                                    }
                                }
                            }
                        }
                    }
                }

            }
            #endregion 

            int rowIndex = rowIndex_Destination;

            //循环通道
            if (headDic != null && headDic.Count > 0)
            {
                foreach (int tongDaoID in headDic.Keys)
                {
                    #region 画表头
                    #region 画格子 同时填充数据                   

                    if (temp != null && temp.TableTitleList != null && temp.TableTitleList.Count > 0)
                    {
                        RowInfo t = temp.TableTitleList.FirstOrDefault();
                        if (t.RowIndex >= 0)
                        {
                            //数据与创建行同时进行 
                            for (int k = 0; k < t.RowNumber; k++)
                            {
                                CopyRow_1(sheet_Source, sheet_Destination, t.RowIndex + k, rowIndex_Destination, 1, true, temp.TableTitleList, allSpecialCharacters, headDic[tongDaoID]);

                                rowIndex_Destination++;
                            }
                        }
                    }

                    #endregion
                    #endregion
                    #region 画数据部分
                    int startRowIndex = rowIndex_Destination;
                    if (dataDic != null && dataDic.ContainsKey(tongDaoID) && dataDic[tongDaoID] != null && dataDic[tongDaoID].Count > 0 && dataDic[tongDaoID].Data != null && dataDic[tongDaoID].Data.Count > 0)
                    {
                        #region 画数据  
                        #region 画格子                       
                        Dictionary<string, CellRangeAddress> cellAddressList = CopyRow_1(sheet_Source, sheet_Destination, temp.DataRowIndex, rowIndex_Destination, dataDic[tongDaoID].Count, true, null, allSpecialCharacters, null);
                        rowIndex_Destination = rowIndex_Destination + dataDic[tongDaoID].Count;
                        #endregion
                        #region 填充数据

                        foreach (MYData d in dataDic[tongDaoID].Data)
                        {
                            if (temp.Cells.Count(p => p.Code == d.name) > 0)//配置中存在说明需要打印
                            {
                                //如果模板中有数据表示固定数据，否则是动态数据，固定数据跳过
                                string key = cellAddressList.Keys.FirstOrDefault();
                                int colCount = Convert.ToInt32(key.Split('_')[1]);
                                CellRangeAddress c = cellAddressList[key];
                                string cValue = sheet_Destination.GetRow(c.FirstRow).GetCell(c.FirstColumn).StringCellValue;
                                while (!string.IsNullOrWhiteSpace(cValue))
                                {
                                    if (temp.Cells.Count >= colCount && temp.Cells[colCount - 1].IsMergeSameValue == "Y")//固定值是否需要合并
                                    {
                                        sheet_Destination.AddMergedRegion(new CellRangeAddress(c.FirstRow, c.FirstRow + dataDic[tongDaoID].Count - 1, c.FirstColumn, c.LastColumn));
                                        int ccCount = 0;
                                        for (int j = 0; j < dataDic[tongDaoID].Count; j++)//将已合并或者已使用的区域移除
                                        {

                                            KeyValuePair<string, CellRangeAddress> cc = cellAddressList.FirstOrDefault(p => p.Value.FirstColumn == c.FirstColumn && p.Value.LastColumn == c.LastColumn);
                                            if (!string.IsNullOrWhiteSpace(cc.Key) && cc.Value != null && cc.Value.FirstColumn == c.FirstColumn && cc.Value.LastColumn == c.LastColumn)
                                            {
                                                if (ccCount > 0)
                                                {
                                                    sheet_Destination.GetRow(cc.Value.FirstRow).GetCell(cc.Value.FirstColumn).SetCellValue(new HSSFRichTextString(""));
                                                }
                                                cellAddressList.Remove(cc.Key);
                                                ccCount++;
                                            }

                                        }

                                    }
                                    else
                                    {
                                        cellAddressList.Remove(key);
                                    }
                                    key = cellAddressList.Keys.FirstOrDefault();
                                    c = cellAddressList[key];
                                    cValue = sheet_Destination.GetRow(c.FirstRow).GetCell(c.FirstColumn).StringCellValue;
                                }
                                HSSFRichTextString value = SetSub((HSSFWorkbook)sheet_Destination.Workbook, allSpecialCharacters, d.value);

                                if ((value == null || string.IsNullOrWhiteSpace(value.String)) && temp.Cells.FirstOrDefault(p => p.Code == d.name) != null && temp.Cells.FirstOrDefault(p => p.Code == d.name).IsHideRowNull == "Y")
                                {
                                    HideRow(sheet_Destination, c.FirstRow, 1);
                                }
                                else if ((value == null || string.IsNullOrWhiteSpace(value.String)) && d.name.IndexOf("_UNIT")<0)
                                {
                                    value = new HSSFRichTextString("/");
                                }
                                sheet_Destination.GetRow(c.FirstRow).GetCell(c.FirstColumn).SetCellValue(value);
                                if (d.mergedRowNum > 1)//多行单元格合并
                                {
                                    sheet_Destination.AddMergedRegion(new CellRangeAddress(c.FirstRow, c.FirstRow + d.mergedRowNum - 1, c.FirstColumn, c.LastColumn));
                                }

                                for (int j = 0; j < d.mergedRowNum; j++)//将已合并或者已使用的区域移除
                                {
                                    KeyValuePair<string, CellRangeAddress> cc = cellAddressList.FirstOrDefault(p => p.Value.FirstColumn == c.FirstColumn && p.Value.LastColumn == c.LastColumn);
                                    if (!string.IsNullOrWhiteSpace(cc.Key) && cc.Value != null && cc.Value.FirstColumn == c.FirstColumn && cc.Value.LastColumn == c.LastColumn)
                                    {
                                        cellAddressList.Remove(cc.Key);
                                    }
                                }

                            }
                        }

                        #endregion
                        #endregion
                    }
                    #endregion

                    #region 画表尾
                    #region 画格子 同时填充数据                   

                    if (temp != null && temp.TableFooterList != null && temp.TableFooterList.Count > 0)
                    {
                        RowInfo t = temp.TableFooterList.FirstOrDefault();
                        if (t.RowIndex >= 0)
                        {
                            //数据与创建行同时进行 
                            for (int k = 0; k < t.RowNumber; k++)
                            {
                                CopyRow_1(sheet_Source, sheet_Destination, t.RowIndex + k, rowIndex_Destination, 1, true, temp.TableFooterList, allSpecialCharacters, footDic[tongDaoID]);

                                rowIndex_Destination++;
                            }
                        }
                    }

                    #endregion
                    #endregion

                }

            }
            return rowIndex_Destination;
        }

        /// <summary>
        /// 隐藏行
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="startRowIndex">开始行号</param>
        /// <param name="rowCount">行数</param>
        private void HideRow(ISheet sheet, int startRowIndex, int rowCount)
        {
            for (int i = 0; i < rowCount; i++)
            {
                IRow sourceRow = sheet.GetRow(startRowIndex);
                sourceRow.Height = 0;
                startRowIndex++;
            }
        }
       
        /// <summary>
        /// 相同规则名称，需要合并只有展示一个标题
        /// </summary>
        /// <returns></returns>
        private List<string> GetSameRuleName()
        {
            List<string> result = new List<string>();
            result.Add("有功功率测量");
            result.Add("有功功率输出");
            return result;

        }


    }
}
