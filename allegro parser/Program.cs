using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;
using System.IO;
using System.Net;

namespace test_parser
{
    class Program
    {
        /// <summary>
        /// Parse categories in subsection
        /// </summary>
        /// <returns></returns>
        public static List<string> CategoryParser()
        {
            string url = "https://allegro.pl/kategoria/smartfony-i-telefony-komorkowe-165";
            var web = new HtmlWeb();
            var doc = web.Load(url);

            HtmlNodeCollection categories = doc.DocumentNode.SelectNodes("//a[contains(@class, '_w7z6o _uj8z7 _1h7wt')]");

            if (categories == null)
            {
                throw new Exception("Check XPath");
            }

            List<string> categoryData = new List<string>();

            //Console.WriteLine($"Total {categories.Count} categories\n");

            foreach (HtmlNode category in categories)
            {
                string categoryLink = "https://allegro.pl" + category.Attributes["href"].Value;

                categoryData.Add(category.InnerText + ";" + categoryLink);
            }

            return categoryData;
        }

        /// <summary>
        /// Parse items in category
        /// </summary>
        /// <param name="category">category list</param>
        /// <returns></returns>
        public static List<string> ItemsParser(List<string> category)
        {
            var web = new HtmlWeb();

            List<string> itemData = new List<string>();
            itemData.Add("Model;Color;Display;Storage;RAM;Price");

            int pageIndex = 1;

            // Parse items in selected category
            for (int i = 0; i < category.Count; i++)
            {
                string url = category[i].Split(';')[1];     // Category link
                string pageUrl;      // Pagination link

                var doc = web.Load(url);

                itemData.AddRange(ItemPageParse(doc));          // Parse 1st page in category

                while (true)
                {
                    HtmlNode nextPage = doc.DocumentNode.SelectSingleNode("//a[contains(@data-role, 'next-page')]");    // Check pagination

                    if (nextPage == null)
                    {
                        break;
                    }
                    pageIndex++;
                    pageUrl = url + "?p=" + pageIndex;
                    doc = web.Load(pageUrl);
                    itemData.AddRange(ItemPageParse(doc));
                }
            }
            return itemData;
        }

        /// <summary>
        /// Parse selected page with items
        /// </summary>
        public static List<string> ItemPageParse(HtmlDocument doc)
        {
            List<string> data = new List<string>();
            HtmlNodeCollection items = doc.DocumentNode.SelectNodes("//div[contains(@class, '_9c44d_1LBF0')]");

            string itemColor, itemDisplay, itemStorage, itemRam;
            itemColor = itemDisplay = itemStorage = itemRam = string.Empty;

            if (items == null)
            {
                throw new Exception("Check XPath");
            }

            //Console.WriteLine($"Total {items.Count} items\n");

            foreach (HtmlNode item in items)
            {
                string itemName = item.SelectSingleNode(".//h2[contains(@class, '_9c44d_LUA1k')]").InnerText.Replace("&#x27;", "\"").Replace("&quot;", "\"").Replace(";", ",");
                string itemPrice = item.SelectSingleNode(".//span[contains(@class, '_9c44d_1zemI')]").InnerText;

                HtmlNodeCollection advConfiguration = item.SelectNodes(".//dd");

                if (advConfiguration != null)
                {
                    try
                    {
                        itemColor = advConfiguration[0].InnerText;
                    }
                    catch (Exception e) { }

                    try
                    {
                        itemDisplay = advConfiguration[1].InnerText.Replace("&quot;", "\"");
                    }
                    catch (Exception e) { }

                    try
                    {
                        itemStorage = advConfiguration[2].InnerText;
                    }
                    catch (Exception e) { }

                    try
                    {
                        itemRam = advConfiguration[3].InnerText;
                    }
                    catch (Exception e) { }
                }

                data.Add(itemName + ";" + itemColor + ";" + itemDisplay + ";" + itemStorage + ";" + itemRam + ";" + itemPrice);
            }

            return data;
        }

        /// <summary>
        /// Save results
        /// </summary>
        public static void SaveFile(string path, List<string> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                string line = data[i];

                using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
                {
                    sw.WriteLineAsync(line);
                }
            }
            
        }

        static void Main(string[] args)
        {
            string filePath = "data.csv";
            List<string> data = new List<string>();
            List<string> category = new List<string>();

            category = CategoryParser();
            data = ItemsParser(category);

            SaveFile(filePath, data);
        }
    }
}
