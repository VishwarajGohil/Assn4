using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IEXTrading.Infrastructure.IEXTradingHandler;
using IEXTrading.Models;
using IEXTrading.Models.ViewModel;
using IEXTrading.DataAccess;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace MVCTemplate.Controllers
{
    public class HomeController : Controller
    {
        public ApplicationDbContext dbContext;
        private string SessionKeyName;
        public int TotalCompanies = 100;

        public HomeController(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        /****
         * The Symbols action calls the GetSymbols method that returns a list of Companies.
         * This list of Companies is passed to the Symbols View.
        ****/
        public IActionResult Symbols()
        {
            //Set ViewBag variable first
            ViewBag.dbSucessComp = 0;
            IEXHandler webHandler = new IEXHandler();
            List<Company> companies = webHandler.GetSymbols();

            //Save comapnies in TempData
            //TempData["Companies"] = JsonConvert.SerializeObject(companies);

            String companiesData = JsonConvert.SerializeObject(companies);
            
            HttpContext.Session.SetString("CompaniesData", companiesData);

            return View(companies);
        }

        /****
         * The Chart action calls the GetChart method that returns 1 year's equities for the passed symbol.
         * A ViewModel CompaniesEquities containing the list of companies, prices, volumes, avg price and volume.
         * This ViewModel is passed to the Chart view.
        ****/
        public IActionResult Chart(string symbol)
        {
            //Set ViewBag variable first
            ViewBag.dbSuccessChart = 0;
            List<Equity> equities = new List<Equity>();
            if (symbol != null)
            {
                IEXHandler webHandler = new IEXHandler();
                equities = webHandler.GetChart(symbol);
                equities = equities.OrderBy(c => c.date).ToList(); //Make sure the data is in ascending order of date.
            }

            CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);

            return View(companiesEquities);
        }

        /****
         * The Refresh action calls the ClearTables method to delete records from a or all tables.
         * Count of current records for each table is passed to the Refresh View.
        ****/
        public IActionResult Refresh(string tableToDel)
        {
            ClearTables(tableToDel);
            Dictionary<string, int> tableCount = new Dictionary<string, int>();
            tableCount.Add("Companies", dbContext.Companies.Count());
            tableCount.Add("Charts", dbContext.Equities.Count());
            return View(tableCount);
        }

        /****
         * Saves the Symbols in database.
        ****/
        public IActionResult PopulateSymbols()
        {
            // reading JSON from the Session
            string companiesData = HttpContext.Session.GetString("CompaniesData");
            List<Company> companies = null;
            if (companiesData != "")
            {
                companies = JsonConvert.DeserializeObject<List<Company>>(companiesData);
            }
            foreach (Company company in companies)
            {
                //Database will give PK constraint violation error when trying to insert record with existing PK.
                //So add company only if it doesnt exist, check existence using symbol (PK)
                if (dbContext.Companies.Where(c => c.symbol.Equals(company.symbol)).Count() == 0)
                {
                    dbContext.Companies.Add(company);
                }
            }
            dbContext.SaveChanges();
            ViewBag.dbSuccessComp = 1;
            return View("Symbols", companies);
        }

        /****
         * Saves the equities in database.
        ****/
        public IActionResult SaveCharts(string symbol)
        {
            IEXHandler webHandler = new IEXHandler();
            List<Equity> equities = webHandler.GetChart(symbol);
            //List<Equity> equities = JsonConvert.DeserializeObject<List<Equity>>(TempData["Equities"].ToString());
            foreach (Equity equity in equities)
            {
                if (dbContext.Equities.Where(c => c.date.Equals(equity.date)).Count() == 0)
                {
                    dbContext.Equities.Add(equity);
                }
            }

            dbContext.SaveChanges();
            ViewBag.dbSuccessChart = 1;

            CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);

            return View("Chart", companiesEquities);
        }

        /****
         * Deletes the records from tables.
        ****/
        public void ClearTables(string tableToDel)
        {
            if ("all".Equals(tableToDel))
            {
                //First remove equities and then the companies
                dbContext.Equities.RemoveRange(dbContext.Equities);
                dbContext.Companies.RemoveRange(dbContext.Companies);
            }
            else if ("Companies".Equals(tableToDel))
            {
                //Remove only those that don't have Equity stored in the Equitites table
                dbContext.Companies.RemoveRange(dbContext.Companies
                                                         .Where(c => c.Equities.Count == 0)
                                                                      );
            }
            else if ("Charts".Equals(tableToDel))
            {
                dbContext.Equities.RemoveRange(dbContext.Equities);
            }
            dbContext.SaveChanges();
        }

        /****
         * Returns the ViewModel CompaniesEquities based on the data provided.
         ****/
        public CompaniesEquities getCompaniesEquitiesModel(List<Equity> equities)
        {
            List<Company> companies = dbContext.Companies.ToList();

            if (equities.Count == 0)
            {
                return new CompaniesEquities(companies, null, "", "", "", 0, 0);
            }

            Equity current = equities.Last();
            string dates = string.Join(",", equities.Select(e => e.date));
            string prices = string.Join(",", equities.Select(e => e.high));
            string volumes = string.Join(",", equities.Select(e => e.volume / 1000000)); //Divide vol by million
            float avgprice = equities.Average(e => e.high);
            double avgvol = equities.Average(e => e.volume) / 1000000; //Divide volume by million
            return new CompaniesEquities(companies, equities.Last(), dates, prices, volumes, avgprice, avgvol);
        }

        public IActionResult Top5()
        {
            //Strategy1();

            //Strategy2();
            
            return View();

        }

        //public decimal CalculatePriceToEarnings(decimal latestPrice, decimal latestEPS)
        //{
        //    if(latestEPS!=0)
        //    {
        //        return (latestPrice/latestEPS);
        //    }
        //    else
        //    {
        //        return 0;
        //    }


        //}

        public CompanyDetails GetCompanyDetails(string symbol)
        {

            CompanyDetails _cd = new CompanyDetails();
            if (symbol != null)
            {
                IEXHandler webHandler = new IEXHandler();

                _cd =  webHandler.GetAll(symbol);
                webHandler = new IEXHandler();

                decimal _latestPrice = webHandler.GetLatestPrice(symbol);
                _cd.latestPrice = _latestPrice;

                if(_cd!=null&&_cd.latestPrice!=0)
                {
                    _cd.PTE = _cd.latestEPS / _cd.latestPrice;
                    _cd.variance = _cd.week52change / _cd.latestPrice;
                }

                //webHandler.GetDividends(symbol);
            }

            return _cd;
        }

        public IActionResult Strategy2()
        {
            List<Company> companies = dbContext.Companies.ToList();
            companies = companies.GetRange(0, TotalCompanies);
            CompanyDetails temp = new CompanyDetails();
            List<CompanyDetails> tempList = new List<CompanyDetails>();
            List<CompanyDetails> _model = new List<CompanyDetails>();

            if (companies != null)
            {
                foreach (var item in companies)
                {
                    temp = GetCompanyDetails(item.symbol);
                    if (temp != null)
                    {
                        tempList.Add(temp);
                    }
                }
            }

            if (tempList.Count != 0)
            {
                _model = tempList.OrderByDescending(x => x.revenuePerShare).ThenByDescending(x => x.returnOnEquity).ThenByDescending(x => x.PTE).ToList();
            }

            if (_model != null)
            {
                if (_model.Count < 5)
                {
                    return View(_model);
                }
                else
                {
                    return View(_model.GetRange(0, 5));
                }
            }
            else
            {
                return View(null);
            }

        }

        public IActionResult Strategy1()
        {
            List<Company> companies = dbContext.Companies.ToList();
            companies = companies.GetRange(0, TotalCompanies);
            CompanyDetails temp = new CompanyDetails();
            List<CompanyDetails> tempList = new List<CompanyDetails>();
            List<CompanyDetails> _model = new List<CompanyDetails>();
            bool _flagDividendGiven;

            if (companies != null)
            {
                foreach (var item in companies)
                {
                    temp = GetCompanyDetails(item.symbol);

                    _flagDividendGiven = IsDividendGiven(item.symbol);
                    if (temp != null&& _flagDividendGiven)
                    {
                        tempList.Add(temp);
                    }
                }
            }

            if (tempList.Count != 0)
            {
                _model = tempList.OrderByDescending(x => x.returnOnEquity).ThenByDescending(x => x.variance).ToList();
            }

            if (_model != null)
            {
                if (_model.Count < 5)
                {
                    return View(_model);
                }
                else
                {
                    return View(_model.GetRange(0, 5));
                }
            }
            else
            {
                return View(null);
            }

        }

        public IActionResult BothStrategies()
        {
            List<Company> companies = dbContext.Companies.ToList();
            companies = companies.GetRange(0, TotalCompanies);
            CompanyDetails temp = new CompanyDetails();
            List<CompanyDetails> tempList = new List<CompanyDetails>();
            List<CompanyDetails> _model = new List<CompanyDetails>();
            bool _flagDividendGiven;

            if (companies != null)
            {
                foreach (var item in companies)
                {
                    temp = GetCompanyDetails(item.symbol);

                    _flagDividendGiven = IsDividendGiven(item.symbol);
                    if (temp != null && _flagDividendGiven)
                    {
                        tempList.Add(temp);
                    }
                }
            }

            if (tempList.Count != 0)
            {
                _model = tempList.OrderByDescending(x => x.returnOnEquity).ThenByDescending(x => x.variance).ToList();
                _model = _model.OrderByDescending(x => x.revenuePerShare).ThenByDescending(x => x.returnOnEquity).ThenByDescending(x => x.PTE).ToList();
            }

            if (_model != null)
            {
                if (_model.Count < 5)
                {
                    return View(_model);
                }
                else
                {
                    return View(_model.GetRange(0, 5));
                }
            }
            else
            {
                return View(null);
            }

        }

        public bool IsDividendGiven(string symbol)
        {
            IEXHandler webHandler = new IEXHandler();

            List<Dividend> _listDiv = webHandler.GetDividends(symbol);

            if(_listDiv.Count!=0)
            {
                return _listDiv.Any(x => x.qualified == "Q");
                
            }

            return false;
        }

        //public float GetEPSbySymbol(string symbol)
        //{
        //    IEXHandler webHandler = new IEXHandler();

        //    float totalEPS=0;

        //    List<EPS> _listEPS = webHandler.GetEPS(symbol);

        //    if(_listEPS.Count!=0)
        //    {
        //        foreach(var item in _listEPS)
        //        {
        //            totalEPS += item.actualEPS; 
        //        }
        //    }

        //    return totalEPS/4;
        //}

        public IActionResult Strategy()
        {
            return View();
        }
    }
}
