﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockData.Worker.Models
{
    public interface ICreateCompanyDataScrape
    {
        void LoadDataToCompany();
        bool IsCompanyDataEmpty();
    }
}