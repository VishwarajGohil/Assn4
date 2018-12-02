using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using IEXTrading.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IEXTrading.Infrastructure.IEXTradingHandler
{
    public class IEXHandler
    {
        static string BASE_URL = "https://api.iextrading.com/1.0/"; //This is the base URL, method specific URL is appended to this.
        HttpClient httpClient;

        public IEXHandler()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        /****
         * Calls the IEX reference API to get the list of symbols. 
        ****/
        public List<Company> GetSymbols()
        {
            string IEXTrading_API_PATH = BASE_URL + "ref-data/symbols";
            string companyList = "";

            List<Company> companies = null;

            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                companyList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            if (!companyList.Equals(""))
            {
                companies = JsonConvert.DeserializeObject<List<Company>>(companyList);
                
                companies = companies.GetRange(0, 4000);
            }
            return companies;
        }

        /****
         * Calls the IEX stock API to get 1 year's chart for the supplied symbol. 
        ****/
        public List<Equity> GetChart(string symbol)
        {
            //Using the format method.
            //string IEXTrading_API_PATH = BASE_URL + "stock/{0}/batch?types=chart&range=1y";
            //IEXTrading_API_PATH = string.Format(IEXTrading_API_PATH, symbol);

            string IEXTrading_API_PATH = BASE_URL + "stock/" + symbol + "/batch?types=chart&range=1y";

            string charts = "";
            List<Equity> Equities = new List<Equity>();
            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                charts = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            if (!charts.Equals(""))
            {
                ChartRoot root = JsonConvert.DeserializeObject<ChartRoot>(charts, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                Equities = root.chart.ToList();
            }
            //make sure to add the symbol the chart
            foreach (Equity Equity in Equities)
            {
                Equity.symbol = symbol;
            }

            return Equities;
        }

        public List<Dividend> GetDividends(string symbol)
        {
            string IEXTrading_API_PATH = BASE_URL + "stock/" + symbol + "/dividends/1y";
            string dividendsList = "";

            List<Dividend> _listDividend = null;

            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                dividendsList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            if (!dividendsList.Equals(""))
            {
                _listDividend = JsonConvert.DeserializeObject<List<Dividend>>(dividendsList, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                //_listDividend = _listDividend.GetRange(0, 9);
            }

            return _listDividend;
        }

        public CompanyDetails GetAll(string symbol)
        {
            string IEXTrading_API_PATH = BASE_URL + "stock/" + symbol + "/stats";
            string _list = "";

            CompanyDetails _all = new CompanyDetails();

            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                _list = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            var jsonObject = (JObject)JsonConvert.DeserializeObject(_list);

            if(jsonObject.Property("revenuePerShare")!=null)
            {
                _all.revenuePerShare = ((jsonObject.Property("revenuePerShare").Value.ToString().Length == 0) || (jsonObject.Property("revenuePerShare").Value.ToString() == "NaN")) ? 0 : Convert.ToDecimal(jsonObject.Property("revenuePerShare").Value.ToString());
            }
            if (jsonObject.Property("returnOnEquity") != null)
            {
                _all.returnOnEquity = ((jsonObject.Property("returnOnEquity").Value.ToString().Length == 0) || (jsonObject.Property("returnOnEquity").Value.ToString() == "NaN")) ? 0 : Convert.ToDecimal(jsonObject.Property("returnOnEquity").Value.ToString());
            }
            if (jsonObject.Property("latestEPS") != null)
            {
                _all.latestEPS = ((jsonObject.Property("latestEPS").Value.ToString().Length == 0) || (jsonObject.Property("latestEPS").Value.ToString() == "NaN")) ? 0 : Convert.ToDecimal(jsonObject.Property("latestEPS").Value.ToString());
            }
            if(jsonObject.Property("companyName") != null)
            {
                _all.companyName = (jsonObject.Property("companyName").Value.ToString().Length == 0) ? "" : jsonObject.Property("companyName").Value.ToString();
            }
            if (jsonObject.Property("week52change") != null)
            {
                _all.week52change = ((jsonObject.Property("week52change").Value.ToString().Length == 0)) ? 0 : Convert.ToDecimal(jsonObject.Property("week52change").Value.ToString());
            }



            _all.symbol = symbol;

            return _all;

        }

        public decimal GetLatestPrice(string symbol)
        {
            string IEXTrading_API_PATH = BASE_URL + "stock/" + symbol + "/price";
            string EPSList = "";

            decimal latestPrice=0;

            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                EPSList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            if (!EPSList.Equals(""))
            {
                latestPrice = JsonConvert.DeserializeObject<decimal>(EPSList);
            }

            return latestPrice;

        }

        public List<EPS> GetEPS(string symbol)
        {
            string IEXTrading_API_PATH = BASE_URL + "stock/" + symbol + "/earnings";
            string EPSList = "";

            List<EPS> _listEPS = null;

            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);

            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                EPSList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            var contentJo = (JObject)JsonConvert.DeserializeObject(EPSList, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var organizationsJArray = contentJo["earnings"]
                .Value<JArray>();

            var organizations = organizationsJArray.ToObject<List<EPS>>();

            var _1listEPS = JsonConvert.DeserializeObject<List<EPS>>(EPSList, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            //if (jsonObject.Property("revenuePerShare") != null)
            //{
            //    _listEPS = JsonConvert.DeserializeObject<List<EPS>>(EPSList, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            //}

            //if (response.IsSuccessStatusCode)
            //{
            //    EPSList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //}

            //var jsonObject = (JObject)JsonConvert.DeserializeObject(EPSList);
            //var unashamedohio = (JArray)(jsonObject.Property("earnings").Value);


            //List<EPS> listEPS = unashamedohio.ToObject<List<EPS>>();

            if (!EPSList.Equals(""))
            {
                _listEPS = JsonConvert.DeserializeObject<List<EPS>>(EPSList);
                //_listDividend = _listDividend.GetRange(0, 9);
            }

            return _listEPS;
        }

        public void GetData(string symbol)
        {

        }
    }
}
