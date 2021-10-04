﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac;
using AutoMapper;
using DataImporter.Common.Utilities;
using DataImporter.Importing.BusinessObjects;
using DataImporter.Importing.Services;
using DataImporter.Web.Models.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace DataImporter.Web.Models.ExcelData
{
    public class ExcelFromDatabase
    {
        private readonly IExcelFileService _excelFileService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDateTimeUtility _dateTimeUtility;

        public DataTable DataTable {get; set;}
        public  string ExcelFileName {get; set;}
        public int ExcelLastId { get; set; }
        public ExcelFromDatabase()
        {
            _excelFileService = Startup.AutofacContainer.Resolve<IExcelFileService>(); 
            _mapper = Startup.AutofacContainer.Resolve<IMapper>();
            _httpContextAccessor = Startup.AutofacContainer.Resolve<IHttpContextAccessor>();
            _dateTimeUtility = Startup.AutofacContainer.Resolve<IDateTimeUtility>();
        }
        public ExcelFromDatabase(IExcelFileService excelFileService, IMapper mapper, IHttpContextAccessor httpContextAccessor,IDateTimeUtility dateTimeUtility)
        {
            _excelFileService = excelFileService;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _dateTimeUtility = dateTimeUtility;
        }

        internal byte[] GetExcelDatabase(int groupId)
        {
           var dataTableAndExcelData = _excelFileService.GetExcelDatabase(groupId);

           DataTable = dataTableAndExcelData.dataTable;
           ExcelLastId = dataTableAndExcelData.excelDataLastId;

            ExcelFileName = _excelFileService.GetExcelFileName(groupId);
           ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

           byte[] fileContents;
           using (var package = new ExcelPackage())
           {
               var workSheet = package.Workbook.Worksheets.Add(ExcelFileName);
               workSheet.Cells["A1"].LoadFromDataTable(DataTable, true);
               fileContents = package.GetAsByteArray();
           }
            return fileContents;
        }

        internal object GetExcelDatabaseToJson(int groupId)
        {
            var dataTableAndExcelData = _excelFileService.GetExcelDatabase(groupId);
            DataTable = dataTableAndExcelData.dataTable;
           

           var list= ConvertTable(DataTable);
           return new
           {
               
               data=list
           };
        }

        internal List<string> GetDataTableColumnName(int groupId)
        {
            var dataTableAndExcelData = _excelFileService.GetExcelDatabase(groupId);
            DataTable = dataTableAndExcelData.dataTable;

            List<string> list = new List<string>();

            foreach (DataColumn column  in DataTable.Columns)
            {
                list.Add($"{column.Caption}");
            }

            return list;
        }

        public List<string[]> ConvertTable(DataTable table)
        {
            return table.Rows.Cast<DataRow>()
                .Select(row => table.Columns.Cast<DataColumn>()
                    .Select(col => Convert.ToString(row[col]))
                    .ToArray())
                .ToList();
        }

        internal void CreateExportHistory(int groupId, int lastExcelFieldId,string email)
        {
            var exportFileHistory = new ExportFileHistory()
            {
                GroupId = groupId,
                ExportDate = _dateTimeUtility.Now,
                Email = email,
                ExportLastExcelFieldId = lastExcelFieldId
            };
            _excelFileService.ExportFileHistoryCreate(exportFileHistory);
        }

        public int GetGroupId(int excelLastId)
        {
           var groupid= _excelFileService.GetGroupId(excelLastId);

           return groupid;
        }

        internal byte[] GetExcelDatabaseForHistory(int groupId, int excelLastDataId)
        {
            var dataTableAndExcelData = _excelFileService.GetExcelDataForHistoryDownload(groupId, excelLastDataId);
            ExcelFileName = _excelFileService.GetExcelFileName(groupId);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            byte[] fileContents;
            using (var package = new ExcelPackage())
            {
                var workSheet = package.Workbook.Worksheets.Add(ExcelFileName);
                workSheet.Cells["A1"].LoadFromDataTable(dataTableAndExcelData, true);
                fileContents = package.GetAsByteArray();
            }
            return fileContents;
        }
    }
}