﻿using System;
using System.Linq;
using AElfChain.Common.Helpers;
using log4net;

namespace AElf.Automation.NetworkTest
{
    class Program
    {
        private static readonly ILog Logger = Log4NetHelper.GetLogger();

        static void Main(string[] args)
        {
            #region Basic Preparation

            //Init Logger
            Log4NetHelper.LogInit("NetworkTest_");

            #endregion

            var operation = new Operation();
            var operatedNode = ConfigHelper.Config.Nodes.Select(o=>o.ListeningPort).ToList();

            var type = ConfigHelper.Config.Type;
            switch (type)
            {
                case "Remove":
                    Logger.Info("Remove peer");
                    operation.RemovePeer(operatedNode);
                    operation.GetPeer();
                    break;
                case "Get":
                    Logger.Info("Get peer");
                    operation.GetPeer();
                    break;
                case "Add":
                    Logger.Info("Add peer");
                    operation.AddPeer(operatedNode);
                    operation.GetPeer();
                    break;
            }

        }
    }
}