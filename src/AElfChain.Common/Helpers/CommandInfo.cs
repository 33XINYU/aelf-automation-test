﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using log4net;
using Newtonsoft.Json.Linq;

namespace AElfChain.Common.Helpers
{
    public class CommandInfo
    {
        private readonly ILog Logger = Log4NetHelper.GetLogger();

        public string Category { get; set; }
        public string Cmd { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Parameter { get; set; }
        public string ContractMethod { get; set; }

        public bool Result { get; set; }
        public JObject JsonInfo { get; set; }
        public object InfoMsg { get; set; }
        public object ErrorMsg { get; set; }
    }

    public class CategoryRequest
    {
        public CategoryRequest()
        {
            Commands = new List<CommandInfo>();
            Count = 0;
            PassCount = 0;
            FailCount = 0;
        }

        public string Category { get; set; }
        public int Count { get; set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public List<CommandInfo> Commands { get; set; }
    }

    public class CategoryInfoSet
    {
        private static readonly ILog Logger = Log4NetHelper.GetLogger();

        public CategoryInfoSet(List<CommandInfo> commands)
        {
            CommandList = commands;
            CategoryList = new List<CategoryRequest>();
        }

        private List<CommandInfo> CommandList { get; }
        private List<CategoryRequest> CategoryList { get; }

        public void GetCategoryBasicInfo()
        {
            foreach (var item in CommandList)
            {
                var category = CategoryList.FindLast(x => x.Category == item.Category);
                if (category == null)
                {
                    category = new CategoryRequest {Category = item.Category};
                    CategoryList.Add(category);
                }

                category.Commands.Add(item);
            }
        }

        public void GetCategorySummaryInfo()
        {
            foreach (var item in CategoryList)
            {
                Logger.Info("Command Category: {0}", item.Category);
                item.Count = item.Commands.Count;
                item.PassCount = item.Commands.FindAll(x => x.Result).Count;
                item.FailCount = item.Commands.FindAll(x => x.Result == false).Count;
            }
        }

        public string SaveTestResultXml(int threadCount, int transactionCount)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null));
            var el = xmlDoc.CreateElement("WebApiResults");
            xmlDoc.AppendChild(el);

            var thread = xmlDoc.CreateAttribute("ThreadCount");
            thread.Value = threadCount.ToString();
            el.Attributes.Append(thread);
            var transactions = xmlDoc.CreateAttribute("TxCount");
            transactions.Value = transactionCount.ToString();
            el.Attributes.Append(transactions);

            foreach (var item in CategoryList)
            {
                var rpc = xmlDoc.CreateElement("WebApi");

                var category = xmlDoc.CreateAttribute("Category");
                category.Value = item.Category;

                rpc.Attributes.Append(category);

                var passCount = xmlDoc.CreateElement("PassCount");
                passCount.InnerText = item.PassCount.ToString();

                var failCount = xmlDoc.CreateElement("FailCount");
                failCount.InnerText = item.FailCount.ToString();

                rpc.AppendChild(passCount);
                rpc.AppendChild(failCount);

                el.AppendChild(rpc);
            }

            var fileName = "WebTh_" + threadCount + "_Tx_" + transactionCount + "_" +
                           DateTime.Now.ToString("MMddHHmmss") + ".xml";
            var fullPath = Path.Combine(CommonHelper.AppRoot, "logs", fileName);
            xmlDoc.Save(fullPath);
            return fullPath;
        }
    }
}