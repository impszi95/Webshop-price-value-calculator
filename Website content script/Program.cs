using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Website_content_script
{
    class Program
    {
        static List<List<string>> products_string;
        static List<string> product_urls;
        static List<Product> products;

        static void Main(string[] args)
        {

           
            products_string = new List<List<string>>();
            product_urls = new List<string>();
            products = new List<Product>();

            for (int i = 0; i < 4; i++)
            {
                Divide_Product_Links("site" + (i + 1) + ".txt");
                Product_Divide_String("site" + (i + 1) + ".txt");
            }
                        
            MakeProducts();
            

            WebClient client = new WebClient();
            List<string> egyeni_oldalak_product = new List<string>();
            Console.WriteLine("Start download");
            foreach (string url in product_urls)
            {
                string downloadString = client.DownloadString(url);
                Console.Clear();
                Console.WriteLine(egyeni_oldalak_product.Count);
                egyeni_oldalak_product.Add(downloadString);
            }
            Console.WriteLine("Ready download");

            for (int i = 0; i < egyeni_oldalak_product.Count; i++)
            {
                for (int j = 0; j < egyeni_oldalak_product[i].Length; j++)
                {
                    if (egyeni_oldalak_product[i][j] == 'H' && egyeni_oldalak_product[i][j+1] == 'a' && egyeni_oldalak_product[i][j + 2] == 't')
                    {
                        int k = j;
                        while (!Char.IsNumber(egyeni_oldalak_product[i][k]))
                        {
                            k++;
                        }
                        string gramm = "";
                        while (Char.IsNumber(egyeni_oldalak_product[i][k]))
                        {
                           gramm+= egyeni_oldalak_product[i][k];
                            k++;
                        }
                        products[i].gramm = double.Parse(gramm);
                    }
                }
            }

            products = products.Where(x => x.price != 0).ToList();            
            products = products.Where(x => x.caliber < 200).ToList();
            products = products.Where(x => x.gramm < 3000).ToList();

            foreach (Product item in products)
            {
                item.CalcGoodness();
            }
            var q = products.OrderBy(x =>x.goodness);

            foreach (var item in q)
            {
                Console.WriteLine(item.name.Substring(0,5) + " - Lövés:" + item.shoot + " - Kaliber:" + item.caliber + " - Ár:" + item.price + "Ft" + " - Gramm:" + item.gramm + " Ft/g:"+item.goodness);
            }

            Console.ReadLine();
        }

        static void Product_Divide_String(string file)
        {
            StreamReader sr = new StreamReader(file,Encoding.UTF8);
            List<string> actual_product_string = new List<string>();
            bool inProduct = false;

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Contains("product_loop_thumbnail_holder"))
                {
                    inProduct = true;
                }
                if (inProduct)
                {
                    actual_product_string.Add(line);
                }
                if (line.Contains("<a rel="))
                {
                    inProduct = false;
                    products_string.Add(actual_product_string);
                    actual_product_string = new List<string>();
                }
            }
        }
        static void Divide_Product_Links(string file)
        {
            StreamReader sr = new StreamReader(file, Encoding.UTF8);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                if (line.Contains("<a href=\"https://www.tuzijatekaruhaz.com/termek/"))
                {
                    int lastPerJel = 0;
                    int i = 10;
                    while (!(line[i] == 'c' && line[i + 1] == 'l' && line[i + 2] == 'a' && line[i + 3] == 's'))
                    {
                        lastPerJel = i - 1;
                        i ++;
                    }

                    string url = line.Substring(10, lastPerJel - 10);

                    product_urls.Add(url);
                }
            }
        }

        static void MakeProducts()
        {            
            foreach (List<string> product_string in products_string)
            {
                Product product = new Product();
                int product_price = 0;
                foreach (string line in product_string)
                {
                    //Name
                    if (line.Contains("<h2"))
                    {
                        string name = "";
                        bool inName = false;
                        for (int i = 1; i < line.Length-1; i++)
                        {                            
                            if (line[i-1] == '>' && Char.IsLetter(line[i]))
                            {
                                inName = true;
                            }
                            if (inName)
                            {
                                name += line[i];
                            }
                            if (inName && line[i+1] == '<')
                            {
                                inName = false;
                                product.name = name;
                            }
                        }
                    }
                    //Price
                    if (line.Contains("woocommerce-Price-amount"))
                    {
                        int start = 0;
                        int end = 0;
                        bool firstC = true;
                        bool first = true;
                        for (int i = 0; i < line.Length; i++)
                        {
                            if (firstC && Char.IsNumber(line[i]))
                            {
                                start = i;
                                firstC = false;
                            }
                            if (first && line[i] == '&')
                            {
                                end = i;
                                first = false;
                            }
                        }
                        string vmi = line.Substring(start, end);
                        product_price = int.Parse(line.Substring(start,end-start));
                    }
                    //Rate
                    if (line.Contains("Besorol"))
                    {
                        for (int i = line.Length-1; i >= 0; i--)
                        {
                            if (Char.IsNumber(line[i]))
                            {
                                product.rate = int.Parse(line[i].ToString());
                            }                          
                        }
                    }
                    //Shoot
                    if (line.Contains("s sz"))
                    {
                        string shoot_string = "";
                        for (int i = 0; i < line.Length; i++)
                        {
                            if (Char.IsNumber(line[i]))
                            {
                                shoot_string += line[i];
                                if (!Char.IsNumber(line[i + 1]))
                                {
                                    product.shoot = int.Parse(shoot_string);
                                }
                            }                            
                        }
                    }
                    //Kaliber
                    if (line.Contains("Kaliber"))
                    {
                        string caliber_string = "";
                        for (int i = 0; i < line.Length; i++)
                        {
                            if (Char.IsNumber(line[i]))
                            {
                                caliber_string += line[i];
                                if (!Char.IsNumber(line[i + 1]))
                                {
                                    product.caliber = int.Parse(caliber_string);
                                }
                            }
                        }
                    }
                }
                product.price = product_price;
                products.Add(product);
            }            
        } 
    }
}
