﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataImporter.Common.Utilities;
using DataImporter.Importing.BusinessObjects;
using DataImporter.Importing.Exceptions;
using DataImporter.Membership.Entities;
using DataImporter.Web.Models;
using DataImporter.Web.Models.Commons;
using DataImporter.Web.Models.Contact;
using DataImporter.Web.Models.ExcelData;
using DataImporter.Web.Models.Files;
using DataImporter.Web.Models.GroupModel;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace DataImporter.Web.Controllers
{
    [Authorize(Policy = "RestrictedArea")]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        public DashboardController(ILogger<DashboardController> logger, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _logger = logger;
            _userManager = userManager;
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            var model = new DashBoardModel();
            var applicationuser = Guid.Parse(_userManager.GetUserId(HttpContext.User));
            model.loadData(applicationuser);
            return View(model);
        }
        public IActionResult ManageGroup()
        {
            return View();
        }
        public JsonResult GetGroupData()
        {
            var DataTableModel = new DataTablesAjaxRequestModel(Request);
            var model = new GroupListModel();
            var applicationuser = Guid.Parse(_userManager.GetUserId(HttpContext.User));
            var data = model.GetGroupData(DataTableModel, applicationuser);
            return Json(data);
        }
        public IActionResult CreateGroup()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CreateGroup(CreateGroupModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var applicationuser = Guid.Parse(_userManager.GetUserId(HttpContext.User));
                    model.CreateGroup(applicationuser);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Failed to create Group");
                    _logger.LogError(ex, "Create Group Failed");
                }
            }
            return View(model);
        }
        public IActionResult GroupEdit(int id)
        {
            var model = new GroupEditModel();
            model.LoadModelData(id);
            return View(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult GroupEdit(GroupEditModel model)
        {
            if (ModelState.IsValid)
            {
                model.Update();
            }
            return RedirectToAction("ManageGroup");
        }
        public IActionResult GroupDelete(int id)
        {
            var model = new GroupListModel();
            model.Delete(id);
            return RedirectToAction("ManageGroup");
        }
        public IActionResult ViewContactsStatus()
        {
            return View();
        }
        public JsonResult ViewContactsGetData()
        {
            var dataTableModel = new DataTablesAjaxRequestModel(Request);
            var model = new ContactListModel();
            var applicationuserId = Guid.Parse(_userManager.GetUserId(HttpContext.User));
            var data = model.LoadData(dataTableModel, applicationuserId);
            return Json(data);
        }
        public IActionResult UploadContacts()
        {
            var model = new AllGroupForContacts();
            var applicationuserId = Guid.Parse(_userManager.GetUserId(HttpContext.User));
            model.LoadAllGroup(applicationuserId);
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult PreviewExcelFile(IFormFile fileSelect, AllGroupForContacts allGroupForContacts)
        {
            var model = new FileUploadModel();
            model.PreviewExcelLoad(fileSelect, allGroupForContacts);
            return View(model);
        }
        public IActionResult CreateExcelFileStatus(FileUploadModel fileUploadModel)
        {
            var model = fileUploadModel.ExcelFileUpload();
            if (model == false)
            {
                ViewBag.Error = "Group Contacts Column does not match.";

                return View();
            }
            return RedirectToAction("ViewContactsStatus", "Dashboard");
        }
        public IActionResult ClearExcelFile(FileUploadModel fileUploadModel)
        {
            fileUploadModel.ExcelFileCancel();
            return RedirectToAction("UploadContacts", "Dashboard");
        }

        public IActionResult ViewContacts()
        {
            var model = new AllGroupForContacts();
            var applicationuserId = Guid.Parse(_userManager.GetUserId(HttpContext.User));
            model.LoadAllGroupForViewData(applicationuserId);
            return View(model);
        }

        [HttpPost]
        public JsonResult ViewContactsForExcelSend(int id)
        {
            var model = new ExcelFromDatabase();
            var jsondata = model.GetExcelDatabaseToJson(id);
            return Json(jsondata);
        }

        [HttpPost]
        public List<string> ColumnName(int id)
        {
            var model = new ExcelFromDatabase();
          /*  var IsitFirstData = model.GroupCheek(id);
          
                if (IsitFirstData)
                {
                    
                }*/
            
            var coloumName = model.GetDataTableColumnName(id);
            return coloumName;
        }

        [HttpPost]
        public IActionResult ExcelFileDownload(AllGroupForContacts allGroupForContacts)
        {
            var model = new ExcelFromDatabase();
            var groupId = allGroupForContacts.GroupId;
            var fileContents = model.GetExcelDatabase(groupId);
            var excelFileName = model.ExcelFileName;
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"{excelFileName}.xlsx";
            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound();
            }
            return File(fileContents, contentType, fileName);
        }

        public IActionResult DownloadContacts()
        {
            return View();
        }

        public JsonResult DownloadHistoryData()
        {
            var dataTableModel = new DataTablesAjaxRequestModel(Request);
            var model = new ExportHistoryListModel();
            var applicationuserId = Guid.Parse(_userManager.GetUserId(HttpContext.User));
            var data = model.LoadDataExcelHistory(dataTableModel, applicationuserId);
            return Json(data);

        }
        public IActionResult Download(int id)
        {
            if (id == 0)
            {
                throw new InvalidParameterException("Download cant be found");
            }
            var model = new ExcelFromDatabase();
            var groupId = model.GetGroupId(id);
            var fileContents = model.GetExcelDatabaseForHistory(groupId, id);
            var excelFileName = model.ExcelFileName;
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"{excelFileName}.xlsx";
            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound();
            }
            return File(fileContents, contentType, fileName);
        }
        [HttpPost]
        public IActionResult SendMailContacts(AllGroupForContacts allGroupForContacts)
        {
            var model = new SendMailContactsModel();
            model.GroupId = allGroupForContacts.GroupId;
            return View(model);
        }
        [HttpPost]
        public IActionResult SendMailContactsFinal(SendMailContactsModel sendMailContactsModel)
        {
            var groupId = sendMailContactsModel.GroupId;
            var email = sendMailContactsModel.Email;
            if (groupId == 0)
            {
                throw new InvalidParameterException("Download cant be found");
            }
            var model = new ExcelFromDatabase();
            var fileContents = model.GetExcelDatabase(groupId);
            var excelFileName = model.ExcelFileName;
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"{excelFileName}.xlsx";
            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound();
            }
            var message = new Message(new string[] { $"{email}" }, "Attachment", $"Check the Attachments .", fileContents, fileName, contentType);
            _emailService.SendEmail(message);
            var lastExcelFile = model.ExcelLastId;
            model.CreateExportHistory(groupId, lastExcelFile, email);
            return RedirectToAction("DownloadContacts");
        }
    }
}
