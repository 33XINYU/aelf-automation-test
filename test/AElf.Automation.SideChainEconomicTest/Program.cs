﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AElf.Automation.Common;
using AElf.Automation.Common.Helpers;
using AElf.Automation.Common.Managers;
using AElf.Automation.SideChainEconomicTest.EconomicTest;

namespace AElf.Automation.SideChainEconomicTest
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Log4NetHelper.LogInit();
            var logger = Log4NetHelper.GetLogger();
            var mainTest = new MainChainTests();
            var sideTest = new SideChainTests();
            sideTest.GetTokenInfo();
            
            var acs8Contract = "mkGKKat9jBFQa75Ty9QYiUnhssHJifYs9wPNafKZedx1TZx4s";
            if (acs8Contract == "")
            {
                await mainTest.MainManager.BuyResources(ChainConstInfo.ChainAccount, 2000);
                await mainTest.Transfer_From_Main_To_Side();

                //设置资源币价格
                sideTest.SideManager.SetResourceUnitPrice(sideTest.SideA);

                acs8Contract = await sideTest.DeployContract_And_Transfer_Resources();
            }

            var contract = new Acs8ContractTest(sideTest.SideA, acs8Contract);
            await contract.ExecutionTest();
            await Task.Delay(50);
            sideTest.SideA.GetTokenBalances(acs8Contract);

            logger.Info("Get side chain consensus resource tokens");
            var consensus = sideTest.SideA.ConsensusService;
            sideTest.SideA.GetTokenBalances(consensus.ContractAddress);

            //Query all main bp resources
            logger.Info("Get side chain bps resource tokens");
            var bps = NodeInfoHelper.Config.Nodes.Select(o => o.Account);
            foreach (var bp in bps)
            {
                sideTest.SideA.GetTokenBalances(bp);
            }
        }
    }
}