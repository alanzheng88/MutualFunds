using System;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Collections;
using System.Globalization;
using System.Configuration;

namespace MutualFunds
{
    class MutualFunds
    {
        private string mutualFundsConnStr = ConfigurationManager.ConnectionStrings["mutualFundsConnStr"].ConnectionString;
        private string url = ConfigurationManager.AppSettings["url"];
        private decimal transUnit = Decimal.Parse(ConfigurationManager.AppSettings["transactionUnits"]);
        private decimal initialUnitPrice = Decimal.Parse(ConfigurationManager.AppSettings["initialUnitPrice"]);

        public Hashtable getInfo()
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            //Debug.WriteLine(responseString);

            var infoPattern = @"<div class=""td-layout-grid9 td-layout-column"">.*?As On:\s+(.*?)<br />.*?<strong>\$(\d+\.\d+)&nbsp;&nbsp;</strong>.*?<strong\s*(?:class=(?:""td-copy-green""|""td-copy-red""))?>\s*([-]?\s*\d*\.\d+).*?[\(]?\s*([-]?\s*\d*\.\d+)%[\)]?</strong>.*?</div>";
            var infoMatch = Regex.Match(responseString, infoPattern, RegexOptions.Singleline);
            //Debug.WriteLine(infoMatch);

            var date = infoMatch.Groups[1].Value;
            var datePattern = @"\s*(\w+)\s*(\d{0,2})\s*[,]?\s*(\d{4})\s*";
            var dateMatch = Regex.Match(date, datePattern);
            var month = dateMatch.Groups[1].Value;
            var day = dateMatch.Groups[2].Value;
            var year = dateMatch.Groups[3].Value;

            var unitPrice = infoMatch.Groups[2].Value;

            var decimalChange = infoMatch.Groups[3].Value;

            var percentChange = infoMatch.Groups[4].Value;

            var info = new Hashtable();
            info.Add("month", month.ToString());
            info.Add("day", day.ToString());
            info.Add("year", year.ToString());
            info.Add("unitPrice", unitPrice.ToString());
            info.Add("decimalChange", decimalChange.ToString());
            info.Add("percentChange", percentChange.ToString());
            return info;
        }

        private object executeQuery(string query)
        {
            using (SqlConnection conn = new SqlConnection(mutualFundsConnStr))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        private decimal getMinUnitPrice()
        {
            return (decimal)executeQuery("SELECT UnitPrice FROM MutualFundsInfo WHERE UnitPrice <= ALL(SELECT UnitPrice FROM MutualFundsInfo)");
        }

        private decimal getMaxUnitPrice()
        {
            return (decimal)executeQuery("SELECT UnitPrice FROM MutualFundsInfo WHERE UnitPrice >= ALL(SELECT UnitPrice FROM MutualFundsInfo)");
        }

        private decimal getNetPercentProfit(decimal initialUnits, decimal currentUnitPrice)
        {
            return (currentUnitPrice - initialUnits) / initialUnits;
        }

        private decimal getBalance(decimal unitPrice)
        {
            var transUnits = this.transUnit;
            var balance = (decimal)transUnits * unitPrice;
            return balance;
        }

        public void displayInfo(Hashtable info)
        {
            Console.WriteLine("date: {0} {1}, {2}", info["month"], info["day"], info["year"]);
            Console.WriteLine("unit price: ${0}", info["unitPrice"]);
            Console.WriteLine("change: {0} ({1}%)", info["decimalChange"], info["percentChange"]);
            Console.WriteLine("balance: ${0}", getBalance(Decimal.Parse((string)info["unitPrice"])).ToString("#.##"));
            Console.WriteLine("minimum unit price: ${0}", getMinUnitPrice());
            Console.WriteLine("maximum unit price: ${0}", getMaxUnitPrice());
            Console.WriteLine("net profit: {0}%", string.Format("{0:0.###}", getNetPercentProfit(initialUnitPrice, (Decimal.Parse((string)info["unitPrice"])))));
        }

        private DateTime getDateTimeObject(string month, string day, string year)
        {
            var intMonth = DateTime.ParseExact(month.ToString(), "MMM", CultureInfo.CurrentCulture).Month;
            string dateStr = intMonth + "/" + day + "/" + year;
            IFormatProvider culture = new System.Globalization.CultureInfo("en-US", true);
            DateTime date = DateTime.Parse(dateStr, culture, System.Globalization.DateTimeStyles.AssumeLocal);
            return date;
        }

        public Boolean dateExist(Hashtable info)
        {
            var dateTime = getDateTimeObject(info["month"].ToString(), info["day"].ToString(), info["year"].ToString());
            int tupleCount = (int)executeQuery(string.Format("SELECT COUNT(Date) FROM MutualFundsInfo WHERE Date='{0}'", dateTime));
            return tupleCount > 0;
        }

        public void saveInfo(Hashtable info)
        {
            var date = getDateTimeObject(info["month"].ToString(), info["day"].ToString(), info["year"].ToString());
            Debug.WriteLine(date.ToShortDateString());
            var unitPrice = Convert.ToDecimal(info["unitPrice"]);
            Debug.WriteLine(unitPrice);
            var decimalChange = Convert.ToDecimal(info["decimalChange"]);
            var percentChange = Convert.ToDecimal(info["percentChange"]);
            var balance = Convert.ToDecimal(getBalance(Decimal.Parse(info["unitPrice"].ToString())));

            using(SqlConnection conn = new SqlConnection(mutualFundsConnStr))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO MutualFundsInfo (Date, UnitPrice, DecimalChange, PercentChange, Balance) VALUES (@date, @unitPrice, @decimalChange, @percentChange, @balance)";
                conn.Open();
               
                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@unitPrice", unitPrice);
                cmd.Parameters.AddWithValue("@decimalChange", decimalChange);
                cmd.Parameters.AddWithValue("@percentChange", percentChange);
                cmd.Parameters.AddWithValue("@balance", balance);

                cmd.ExecuteNonQuery();
            }
                   
        }
    }
}
