﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataImporter.Data;
using DataImporter.Importing.Entities;

namespace DataImporter.Importing.Repositories
{
    public interface IExcelFieldDataRepository : IRepository<ExcelFieldData, int>
    {
    }
}
