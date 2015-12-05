using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockExchangeQuotes
{
    public class Quotation
    {
        public string Symbol { get; set; }

        public string Name { get; set; }

        public double Value { get; set; }

        public bool IsMain { get; set; }

        //value to be used to pass parameters to SetLimitDialog
        public string LimitType { get; set; }

        //value to be used to pass parameters to SetLimitDialog
        public string Date { get; set; }

        //value to be used to pass parameters to SetLimitDialog
        public string Time { get; set; }

        public override string ToString()
        {
            return Symbol;
        }
    }
}
