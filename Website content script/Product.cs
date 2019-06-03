using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website_content_script
{
    public class Product
    {
         public string name { get; set; }
         public int price { get; set; }
         public int rate { get; set; }
         public int shoot { get; set; }
         public int caliber { get; set; }
         public double goodness { get; set; }
         public double gramm { get; set; }

        public Product()
        {
            name = "";
            price = 0;
            rate = 0;
            shoot = 0;
            caliber = 0;
            goodness = 0;
            gramm = 0;
        }
        public void CalcGoodness()
        {
            goodness =  gramm/price;
        }
    }
}
