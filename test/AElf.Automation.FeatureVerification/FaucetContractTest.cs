﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AElf.Contracts.Faucet;
using AElf.Contracts.MultiToken;
using AElf.Types;
using AElfChain.Common;
using AElfChain.Common.Contracts;
using AElfChain.Common.DtoExtension;
using AElfChain.Common.Helpers;
using AElfChain.Common.Managers;
using Google.Protobuf.WellKnownTypes;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace AElf.Automation.Contracts.ScenarioTest
{
    [TestClass]
    public class FaucetContractTest
    {
        private ILog Logger { get; set; }
        private INodeManager NodeManager { get; set; }
        private GenesisContract _genesisContract;
        private TokenContract _tokenContract;
        private FaucetContract _faucetContract;
        private FaucetContract _faucetContract2;
        private FaucetContract _faucetContract3;
        private string InitAccount { get; } = "J6zgLjGwd1bxTBpULLXrGVeV74tnS2n74FFJJz7KNdjTYkDF6";
        private string Others { get; } = "2bs2uYMECtHWjB57RqgqQ3X2LrxgptWHtzCqGEU11y45aWimh4";
        private string Account1 { get; } = "2bs2uYMECtHWjB57RqgqQ3X2LrxgptWHtzCqGEU11y45aWimh4";
        private string Target { get; } = "YUW9zH5GhRboT5JK4vXp5BLAfCDv28rRmTQwo418FuaJmkSg8";

        private static string RpcUrl { get; } = "192.168.66.9:8000";
        private string Symbol { get; } = "ELF";

        private string faucet = "2M24EKAecggCnttZ9DUUMCXi4xC67rozA87kFgid9qEwRUMHTs";

        // private string faucet2 = "2dKF3svqDXrYtA5mYwKfADiHajo37mLZHPHVVuGbEDoD9jSgE8";
        private string faucet2 = "";
        private string faucet3 = "";

        [TestInitialize]
        public void Initialize()
        {
            Log4NetHelper.LogInit("FaucetContractTest");
            Logger = Log4NetHelper.GetLogger();
            NodeInfoHelper.SetConfig("nodes-env2-main");

            NodeManager = new NodeManager(RpcUrl);
            if (faucet.Equals(""))
                _faucetContract = new FaucetContract(NodeManager, InitAccount);
            else
                _faucetContract = new FaucetContract(NodeManager, InitAccount, faucet);

            if (faucet2.Equals(""))
                _faucetContract2 = new FaucetContract(NodeManager, InitAccount);
            else
                _faucetContract2 = new FaucetContract(NodeManager, InitAccount, faucet2);

            if (faucet3.Equals(""))
                _faucetContract3 = new FaucetContract(NodeManager, InitAccount);
            else
                _faucetContract3 = new FaucetContract(NodeManager, InitAccount, faucet3);
        }

        [TestMethod]
        public void Initialize_Default()
        {
            var result =
                _faucetContract.ExecuteMethodWithResult(FaucetContractMethod.Initialize, new InitializeInput());
            result.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            // get owner
            var owner = _faucetContract.GetOwner("ELF");
            Logger.Info($"owner is {owner}");
            owner.ShouldBe(InitAccount.ConvertAddress());

            // get limit amount and interval minutes
            var limitAmount = _faucetContract.GetLimitAmount("ELF");
            var intervalMinutes = _faucetContract.GetIntervalMinutes("ELF");
            Logger.Info($"limitAmount is {limitAmount}");
            Logger.Info($"intervalMinutes is {intervalMinutes}");
            limitAmount.ShouldBe(100_00000000);
            intervalMinutes.ShouldBe(180);

            // get faucet status
            var faucetStatus = _faucetContract.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatus.is_on is {faucetStatus.IsOn}");
            Logger.Info($"faucetStatus.turn_at is {faucetStatus.TurnAt}");
            faucetStatus.IsOn.ShouldBeFalse();
            faucetStatus.TurnAt.ShouldBeNull();
        }

        [TestMethod]
        public void ErrorTest()
        {
            // others call some methods(turnOn、turnOff、NewFaucet、Pour、SetLimit、Ban、Send)
            _faucetContract.SetAccount(Others);
            var turnOn = _faucetContract.TurnOn("ELF");
            turnOn.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.NodeValidationFailed);
            Logger.Info($"turnOn.Error is {turnOn.Error}");
            turnOn.Error.ShouldContain("No permission to operate faucet of ELF.");

            var turnOff = _faucetContract.TurnOff(symbol: "ELF");
            turnOff.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.NodeValidationFailed);
            Logger.Info($"turnOff.Error is {turnOff.Error}");
            turnOff.Error.ShouldContain("No permission to operate faucet of ELF.");

            var newFaucet = _faucetContract.NewFaucet("ELF", Others, 0, 0);
            newFaucet.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.NodeValidationFailed);
            Logger.Info($"newFaucet.Error is {newFaucet.Error}");
            newFaucet.Error.ShouldContain("No permission.");

            var pour = _faucetContract.Pour("ELF", 1000_00000000);
            pour.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.NodeValidationFailed);
            Logger.Info($"pour.Error is {pour.Error}");
            pour.Error.ShouldContain("No permission to operate faucet of ELF.");

            var setLimit = _faucetContract.SetLimit("ELF", 0, 0);
            setLimit.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.NodeValidationFailed);
            Logger.Info($"setLimit.Error is {setLimit.Error}");
            setLimit.Error.ShouldContain("No permission to operate faucet of ELF.");

            var ban = _faucetContract.Ban("ELF", Target, true);
            ban.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.NodeValidationFailed);
            Logger.Info($"ban.Error is {ban.Error}");
            ban.Error.ShouldContain("No permission to operate faucet of ELF.");

            var send = _faucetContract.Send(Target, "ELF", 10_00000000);
            send.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.NodeValidationFailed);
            Logger.Info($"send.Error is {send.Error}");
            send.Error.ShouldContain("No permission to operate faucet of ELF.");
        }

        [TestMethod]
        public void Test()
        {
            // init
            var initialize = _faucetContract2.Initialize(Account1, 50_00000000, 300);
            initialize.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            _faucetContract2.SetAccount(Account1);
            // get owner
            var owner = _faucetContract2.GetOwner("ELF");
            Logger.Info($"owner is {owner}");
            owner.ShouldBe(Account1.ConvertAddress());

            // get limit amount and interval minutes
            var limitAmount = _faucetContract2.GetLimitAmount("ELF");
            var intervalMinutes = _faucetContract2.GetIntervalMinutes("ELF");
            Logger.Info($"limitAmount is {limitAmount}");
            Logger.Info($"intervalMinutes is {intervalMinutes}");
            limitAmount.ShouldBe(50_00000000);
            intervalMinutes.ShouldBe(300);

            // turn on(init)
            var turnOn = _faucetContract2.ExecuteMethodWithResult(FaucetContractMethod.TurnOn, new TurnInput());
            var faucetStatusTurnOn = _faucetContract2.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatusTurnOn.is_on is {faucetStatusTurnOn.IsOn}");
            Logger.Info($"faucetStatusTurnOn.turn_at is {faucetStatusTurnOn.TurnAt}");
            faucetStatusTurnOn.IsOn.ShouldBeTrue();
            faucetStatusTurnOn.TurnAt.ShouldNotBeNull();

            // turn off(init)
            var turnOff = _faucetContract2.ExecuteMethodWithResult(FaucetContractMethod.TurnOff, new TurnInput());
            var faucetStatusTurnOff = _faucetContract2.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatusTurnOff.is_on is {faucetStatusTurnOff.IsOn}");
            Logger.Info($"faucetStatusTurnOff.turn_at is {faucetStatusTurnOff.TurnAt}");
            faucetStatusTurnOff.IsOn.ShouldBeFalse();
            faucetStatusTurnOff.TurnAt.ShouldNotBeNull();

            // turn on
            Thread.Sleep(60 * 1000);
            var turnOn1 = _faucetContract2.ExecuteMethodWithResult(FaucetContractMethod.TurnOn, new TurnInput());
            var faucetStatusTurnOn1 = _faucetContract2.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatusTurnOn1.is_on is {faucetStatusTurnOn1.IsOn}");
            Logger.Info($"faucetStatusTurnOn1.turn_at is {faucetStatusTurnOn1.TurnAt}");
            faucetStatusTurnOn1.IsOn.ShouldBeTrue();
            faucetStatusTurnOn1.TurnAt.ShouldNotBeNull();

            // faucet'balance before pour
            _genesisContract = GenesisContract.GetGenesisContract(NodeManager, Account1);
            _tokenContract = _genesisContract.GetTokenContract(Account1);
            var faucetBalanceBeforePour = _tokenContract.GetUserBalance(_faucetContract2.ContractAddress, "ELF");
            Logger.Info($"faucetBalanceBeforePour is {faucetBalanceBeforePour}");
            // faucetBalanceBeforePour.ShouldBe(0);

            // approve
            _faucetContract2.SetAccount(Account1);
            _genesisContract = GenesisContract.GetGenesisContract(NodeManager, Account1);
            _tokenContract = _genesisContract.GetTokenContract(Account1);
            var approve1 =
                _tokenContract.ApproveToken(Account1, _faucetContract2.ContractAddress, 10000_00000000);
            approve1.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            // pour
            var pour = _faucetContract2.Pour("ELF", 100_00000000);
            pour.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var faucetBalanceAfterPour = _tokenContract.GetUserBalance(_faucetContract2.ContractAddress, "ELF");
            Logger.Info($"faucetBalanceAfterPour is {faucetBalanceAfterPour}");
            var targetBalanceBeforeTake = _tokenContract.GetUserBalance(Target, "ELF");
            Logger.Info($"targetBalanceBeforeTake is {targetBalanceBeforeTake}");
            // faucetBalanceAfterPour.ShouldBe(100_00000000);
            // targetBalanceBeforeTake.ShouldBe(0);

            // set isBan to true
            var banTrue = _faucetContract2.Ban("ELF", Target, true);
            banTrue.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var isBanTrue = _faucetContract2.IsBannedByOwner(Target, "ELF");
            Logger.Info($"isBan(True) is {isBanTrue}");

            // take(is_ban = true)
            _faucetContract2.SetAccount(Target);
            var takeIsBanTrue = _faucetContract2.Take("ELF", 10_00000000);
            takeIsBanTrue.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.NodeValidationFailed);
            takeIsBanTrue.Error.ShouldContain("Sender is banned by faucet owner of ELF");

            // set isBan to false
            _faucetContract2.SetAccount(Account1);
            var banFalse = _faucetContract2.Ban("ELF", Target, false);
            banFalse.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var isBanFalse = _faucetContract2.IsBannedByOwner(Target, "ELF");
            Logger.Info($"isBan(False) is {isBanFalse}");

            // take(is_ban = false)
            _faucetContract2.SetAccount(Target);
            var take = _faucetContract2.Take("ELF", 10_00000000);
            take.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var targetBalanceAfterTake = _tokenContract.GetUserBalance(Target, "ELF");
            Logger.Info($"targetBalanceAfterTake is {targetBalanceAfterTake}");
            var faucetBalanceAfterTake = _tokenContract.GetUserBalance(_faucetContract2.ContractAddress, "ELF");
            Logger.Info($"faucetBalanceAfterTake is {faucetBalanceAfterTake}");
            targetBalanceAfterTake.ShouldBe(10_00000000);

            // return
            _tokenContract.SetAccount(Target);
            var approve2 = _tokenContract.ApproveToken(Target, _faucetContract2.ContractAddress, 10000000000_00000000);
            approve2.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var targetReturn = _faucetContract2.Return("ELF", 2_00000000);
            targetReturn.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var targetBalanceAfterReturn = _tokenContract.GetUserBalance(Target, "ELF");
            Logger.Info($"targetBalanceAfterReturn is {targetBalanceAfterReturn}");
            var faucetBalanceAfterReturn = _tokenContract.GetUserBalance(_faucetContract2.ContractAddress, "ELF");
            Logger.Info($"faucetBalanceAfterReturn is {faucetBalanceAfterReturn}");
            // targetBalanceAfterReturn.ShouldBe(8_00000000);

            // return all
            var targetReturnAll =
                _faucetContract2.ExecuteMethodWithResult(FaucetContractMethod.Return, new ReturnInput {Symbol = "ELF"});
            targetReturnAll.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var targetBalanceAfterReturnAll = _tokenContract.GetUserBalance(Target, "ELF");
            Logger.Info($"targetBalanceAfterReturnAll is {targetBalanceAfterReturnAll}");
            var faucetBalanceAfterReturnAll = _tokenContract.GetUserBalance(_faucetContract2.ContractAddress, "ELF");
            Logger.Info($"faucetBalanceAfterReturnAll is {faucetBalanceAfterReturnAll}");
            targetBalanceAfterReturnAll.ShouldBe(0);
        }

        [TestMethod]
        public void TakeReturnTest()
        {
            // init
            _faucetContract3.SetAccount(InitAccount);
            var initialize = _faucetContract3.Initialize(InitAccount, 50_00000000, 50);
            initialize.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            // get owner
            var ownerInitAccount = _faucetContract3.GetOwner("ELF");
            Logger.Info($"owner is {ownerInitAccount}");
            ownerInitAccount.ShouldBe(InitAccount.ConvertAddress());
            // get limit amount and interval minutes
            var limitAmount = _faucetContract3.GetLimitAmount("ELF");
            var intervalMinutes = _faucetContract3.GetIntervalMinutes("ELF");
            Logger.Info($"limitAmount is {limitAmount}");
            Logger.Info($"intervalMinutes is {intervalMinutes}");
            limitAmount.ShouldBe(50_00000000);
            intervalMinutes.ShouldBe(50);
            // get faucet'balance
            _genesisContract = GenesisContract.GetGenesisContract(NodeManager, InitAccount);
            _tokenContract = _genesisContract.GetTokenContract(InitAccount);
            var faucetBalance = _tokenContract.GetUserBalance(_faucetContract3.ContractAddress, "ELF");
            Logger.Info($"faucetBalance is {faucetBalance}");

            // new faucet
            var newFaucet = _faucetContract3.NewFaucet("ELF", Account1, 20_00000000, 1);
            newFaucet.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            // get owner
            var ownerAccount1 = _faucetContract3.GetOwner("ELF");
            Logger.Info($"owner is {Account1}");
            ownerAccount1.ShouldBe(Account1.ConvertAddress());
            // get limit amount and interval minutes
            var limitAmountAfter = _faucetContract3.GetLimitAmount("ELF");
            var intervalMinutesAfter = _faucetContract3.GetIntervalMinutes("ELF");
            Logger.Info($"limitAmountAfter is {limitAmountAfter}");
            Logger.Info($"intervalMinutesAfter is {intervalMinutesAfter}");
            limitAmountAfter.ShouldBe(20_00000000);
            intervalMinutesAfter.ShouldBe(1);
            // get faucet'balance
            _genesisContract = GenesisContract.GetGenesisContract(NodeManager, InitAccount);
            _tokenContract = _genesisContract.GetTokenContract(InitAccount);
            var faucetBalanceAfter = _tokenContract.GetUserBalance(_faucetContract3.ContractAddress, "ELF");
            Logger.Info($"faucetBalanceAfter is {faucetBalanceAfter}");

            // get faucet status
            var faucetStatusInit = _faucetContract3.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatusInit.is_on is {faucetStatusInit.IsOn}");
            Logger.Info($"faucetStatusInit.turn_at is {faucetStatusInit.TurnAt}");
            faucetStatusInit.IsOn.ShouldBeFalse();
            faucetStatusInit.TurnAt.ShouldBeNull();

            // takeAmount = 0
            _faucetContract3.SetAccount(Target);
            var take1 = _faucetContract3.Take("ELF", 0);
            take1.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.NodeValidationFailed);
            take1.Error.ShouldContain(
                "Cannot take 0 from ELF faucet due to either limit amount (2000000000) or input amount (0) is negative or zero.");

            Thread.Sleep(60 * 1000);
            // takeAmount > 0
            _faucetContract3.SetAccount(Target);
            var take2 = _faucetContract3.Take("ELF", 10_00000000);
            take2.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.NodeValidationFailed);
            take2.Error.ShouldContain("Insufficient balance of ELF. Need balance: 1000000000; Current balance: 0");

            // approve
            _faucetContract3.SetAccount(Account1);
            var approve =
                _tokenContract.ApproveToken(Account1, _faucetContract3.ContractAddress, 10000000000_00000000);
            approve.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            // pourAmount = 0
            var pour = _faucetContract3.Pour("ELF", 0);
            pour.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.NodeValidationFailed);
            Logger.Info($"pour.Error is {pour.Error}");
            pour.Error.ShouldContain("Invalid amount.");

            // pourAmount > 0
            var pour2 = _faucetContract3.Pour("ELF", 200_00000000);
            pour2.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            // takeAmount = 0
            Thread.Sleep(60 * 1000);
            _faucetContract3.SetAccount(Target);
            var take3 = _faucetContract3.Take("ELF", 0);
            take3.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.NodeValidationFailed);
            take3.Error.ShouldContain(
                "Cannot take 0 from ELF faucet due to either limit amount (2000000000) or input amount (0) is negative or zero.");

            // turn on
            _faucetContract3.SetAccount(Account1);
            var turnOn = _faucetContract3.ExecuteMethodWithResult(FaucetContractMethod.TurnOn, new TurnInput());
            var faucetStatusTurnOn = _faucetContract3.GetFaucetStatus("ELF");
            // get faucet status
            var faucetStatus = _faucetContract3.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatusTurnOn.is_on is {faucetStatusTurnOn.IsOn}");
            Logger.Info($"faucetStatusTurnOn.turn_at is {faucetStatusTurnOn.TurnAt}");
            faucetStatus.IsOn.ShouldBeTrue();
            faucetStatus.TurnAt.ShouldNotBeNull();

            // takeAmount > limit(20_00000000)
            Thread.Sleep(60 * 1000);
            _faucetContract3.SetAccount(Target);
            var take4 = _faucetContract3.Take("ELF", 30_00000000);
            take4.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.Mined);
            var targetBalance = _tokenContract.GetUserBalance(Target, "ELF");
            Logger.Info($"targetBalance is {targetBalance}");
            // targetBalance.ShouldBe(20_00000000);

            // set limit
            _faucetContract3.SetAccount(Account1);
            var setLimit = _faucetContract3.SetLimit("ELF", 1000_00000000, 1);
            // get limit amount and interval minutes
            var limitAmountReset = _faucetContract3.GetLimitAmount("ELF");
            var intervalMinutesReset = _faucetContract3.GetIntervalMinutes("ELF");
            Logger.Info($"limitAmountReset is {limitAmountReset}");
            Logger.Info($"intervalMinutesReset is {intervalMinutesReset}");
            limitAmountReset.ShouldBe(1000_00000000);
            intervalMinutesReset.ShouldBe(1);

            // takeAmount > pourAmount(200_00000000)
            Thread.Sleep(60 * 1000);
            _faucetContract3.SetAccount(Target);
            var take5 = _faucetContract3.Take("ELF", 300_00000000);
            take5.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.NodeValidationFailed);
            take5.Error.ShouldContain("Insufficient balance of ELF.");

            // return amount > takeAmount
            _genesisContract = GenesisContract.GetGenesisContract(NodeManager, Target);
            _tokenContract = _genesisContract.GetTokenContract(Target);
            _tokenContract.SetAccount(Target);
            _faucetContract3.SetAccount(Target);
            var approve2 = _tokenContract.ApproveToken(Target, _faucetContract3.ContractAddress, 10000000000_00000000);
            approve2.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var targetReturn1 =
                _faucetContract3.ExecuteMethodWithResult(FaucetContractMethod.Return,
                    new ReturnInput {Symbol = "ELF", Amount = 30_00000000});
            targetReturn1.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.NodeValidationFailed);
            targetReturn1.Error.ShouldContain("Insufficient balance of ELF.");

            // return all
            _faucetContract3.SetAccount(Target);
            var approve3 = _tokenContract.ApproveToken(Target, _faucetContract3.ContractAddress, 10000000000_00000000);
            approve3.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var targetReturnAll =
                _faucetContract3.ExecuteMethodWithResult(FaucetContractMethod.Return, new ReturnInput {Symbol = "ELF"});
            targetReturnAll.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var targetBalanceAfterReturnAll = _tokenContract.GetUserBalance(Target, "ELF");
            targetBalanceAfterReturnAll.ShouldBe(0);
        }

        [TestMethod]
        public void NewFaucetTest()
        {
            // new faucet
            var newFaucet = _faucetContract3.NewFaucet("ELF_TEST", InitAccount, 20_00000000, 20);
            newFaucet.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.NodeValidationFailed);
            newFaucet.Error.ShouldContain("No permission.");

            // init
            var initialize = _faucetContract3.Initialize(InitAccount, 50_00000000, 300);
            initialize.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            // turn on(ELF)
            _faucetContract3.SetAccount(InitAccount);
            var turnOnTimeBefore = _faucetContract3.TurnOn("ELF");
            turnOnTimeBefore.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var faucetStatusTurnOnTimeBefore = _faucetContract3.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatusTurnOnTimeBefore.is_on is {faucetStatusTurnOnTimeBefore.IsOn}");
            Logger.Info($"faucetStatusTurnOnTimeBefore.turn_at is {faucetStatusTurnOnTimeBefore.TurnAt}");
            faucetStatusTurnOnTimeBefore.IsOn.ShouldBeTrue();
            faucetStatusTurnOnTimeBefore.TurnAt.ShouldNotBeNull();

            // turn off(ELF)
            var turnOffTimeBefore = _faucetContract3.TurnOff("ELF");
            turnOffTimeBefore.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var faucetStatusTurnOffTimeBefore = _faucetContract3.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatusTurnOffTimeBefore.is_on is {faucetStatusTurnOffTimeBefore.IsOn}");
            Logger.Info($"faucetStatusTurnOffTimeBefore.turn_at is {faucetStatusTurnOffTimeBefore.TurnAt}");
            faucetStatusTurnOffTimeBefore.IsOn.ShouldBeFalse();
            faucetStatusTurnOffTimeBefore.TurnAt.ShouldNotBeNull();

            // new faucet for the first time
            var newFaucet1 = _faucetContract3.NewFaucet("ELF_TEST", Others, 20_00000000, 20);
            newFaucet1.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            var owner1 = _faucetContract3.GetOwner("ELF_TEST");
            Logger.Info($"owner1 is {owner1}");
            owner1.ShouldBe(Others.ConvertAddress());

            var limitAmount1 = _faucetContract3.GetLimitAmount("ELF_TEST");
            var intervalMinutes1 = _faucetContract3.GetIntervalMinutes("ELF_TEST");
            Logger.Info($"limitAmount1 is {limitAmount1}");
            Logger.Info($"intervalMinutes1 is {intervalMinutes1}");
            limitAmount1.ShouldBe(20_00000000);
            intervalMinutes1.ShouldBe(20);

            // new faucet for the second time
            var newFaucet2 = _faucetContract3.NewFaucet("ELF_TEST1", Others, 0, 0);
            newFaucet2.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            var owner2 = _faucetContract3.GetOwner("ELF_TEST1");
            Logger.Info($"owner2 is {owner2}");
            owner2.ShouldBe(Others.ConvertAddress());

            var limitAmount2 = _faucetContract3.GetLimitAmount("ELF_TEST1");
            var intervalMinutes2 = _faucetContract3.GetIntervalMinutes("ELF_TEST1");
            Logger.Info($"limitAmount2 is {limitAmount2}");
            Logger.Info($"intervalMinutes2 is {intervalMinutes2}");
            limitAmount2.ShouldBe(100_00000000);
            intervalMinutes2.ShouldBe(180);

            // turn on(ELF_TEST)
            _faucetContract3.SetAccount(Others);
            var turnOnTimeBefore1 = _faucetContract3.TurnOn("ELF_TEST");
            turnOnTimeBefore1.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var faucetStatusTurnOnTimeBefore1 = _faucetContract3.GetFaucetStatus("ELF_TEST");
            Logger.Info($"faucetStatusTurnOnTimeBefore1.is_on is {faucetStatusTurnOnTimeBefore1.IsOn}");
            Logger.Info($"faucetStatusTurnOnTimeBefore1.turn_at is {faucetStatusTurnOnTimeBefore1.TurnAt}");
            faucetStatusTurnOnTimeBefore1.IsOn.ShouldBeTrue();
            faucetStatusTurnOnTimeBefore1.TurnAt.ShouldNotBeNull();

            // turn off(ELF_TEST)
            var turnOffTimeBefore1 = _faucetContract3.TurnOff("ELF_TEST");
            turnOffTimeBefore1.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var faucetStatusTurnOffTimeBefore1 = _faucetContract3.GetFaucetStatus("ELF_TEST");
            Logger.Info($"faucetStatusTurnOffTimeBefore1.is_on is {faucetStatusTurnOffTimeBefore1.IsOn}");
            Logger.Info($"faucetStatusTurnOffTimeBefore1.turn_at is {faucetStatusTurnOffTimeBefore1.TurnAt}");
            faucetStatusTurnOffTimeBefore1.IsOn.ShouldBeFalse();
            faucetStatusTurnOffTimeBefore1.TurnAt.ShouldNotBeNull();

            // pour
            _faucetContract3.SetAccount(InitAccount);
            _genesisContract = GenesisContract.GetGenesisContract(NodeManager, Target);
            _tokenContract = _genesisContract.GetTokenContract(Target);
            var approve =
                _tokenContract.ApproveToken(InitAccount, _faucetContract3.ContractAddress, 10000000000_00000000);
            approve.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var pour = _faucetContract3.Pour("ELF", 100_00000000);
            pour.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var faucetBalanceAfterPour = _tokenContract.GetUserBalance(_faucetContract3.ContractAddress, "ELF");
            Logger.Info($"faucetBalanceAfterPour is {faucetBalanceAfterPour}");
            var targetBalanceBeforeTake = _tokenContract.GetUserBalance(Target, "ELF");
            Logger.Info($"targetBalanceBeforeTake is {targetBalanceBeforeTake}");

            // send
            var initAccountBalance = _tokenContract.GetUserBalance(InitAccount, "ELF");
            Logger.Info($"initAccountBalance is {initAccountBalance}");
            var send = _faucetContract3.Send(Target, "ELF", 100);
            send.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            var faucetBalance = _tokenContract.GetUserBalance(Target, "ELF");
            Logger.Info($"faucetBalance is {faucetBalance}");
            var targetBalance = _tokenContract.GetUserBalance(Target, "ELF");
            Logger.Info($"targetBalanceBefore is {targetBalance}");
            targetBalance.ShouldBe(100 + targetBalanceBeforeTake);
        }

        [TestMethod]
        public void TakeReturnLimitTest()
        {
            // init
            _faucetContract3.SetAccount(InitAccount);
            var initialize = _faucetContract3.Initialize(InitAccount, 50_00000000, 1);
            initialize.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            // get owner
            var ownerInitAccount = _faucetContract3.GetOwner("ELF");
            Logger.Info($"owner is {ownerInitAccount}");
            ownerInitAccount.ShouldBe(InitAccount.ConvertAddress());
            // get limit amount and interval minutes
            var limitAmount = _faucetContract3.GetLimitAmount("ELF");
            var intervalMinutes = _faucetContract3.GetIntervalMinutes("ELF");
            Logger.Info($"limitAmount is {limitAmount}");
            Logger.Info($"intervalMinutes is {intervalMinutes}");
            limitAmount.ShouldBe(50_00000000);
            intervalMinutes.ShouldBe(1);

            // approve
            _faucetContract3.SetAccount(InitAccount);
            _genesisContract = GenesisContract.GetGenesisContract(NodeManager, InitAccount);
            _tokenContract = _genesisContract.GetTokenContract(InitAccount);
            var approve =
                _tokenContract.ApproveToken(InitAccount, _faucetContract3.ContractAddress, 1000_00000000);
            approve.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            // pourAmount > 0
            var pour2 = _faucetContract3.Pour("ELF", 200_00000000);
            pour2.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            // take(never turned on)
            _faucetContract3.SetAccount(Target);
            var takeFirst = _faucetContract3.Take("ELF", 1_00000000);
            takeFirst.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.NodeValidationFailed);
            takeFirst.Error.ShouldContain("Faucet of ELF never turned on.");

            // turn on(ELF)
            _faucetContract3.SetAccount(InitAccount);
            var turnOn = _faucetContract3.TurnOn("ELF");
            turnOn.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var faucetStatus = _faucetContract3.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatus.is_on is {faucetStatus.IsOn}");
            Logger.Info($"faucetStatus.turn_at is {faucetStatus.TurnAt}");
            faucetStatus.IsOn.ShouldBeTrue();
            faucetStatus.TurnAt.ShouldNotBeNull();

            // take for the first time
            _faucetContract3.SetAccount(Target);
            var takeFirst1 = _faucetContract3.Take("ELF", 2_00000000);
            takeFirst1.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.Mined);

            // take for the second time
            var takeSecond = _faucetContract3.Take("ELF", 3_00000000);
            takeSecond.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.NodeValidationFailed);
            Logger.Info($"takeSecond.Error is {takeSecond.Error}");
            takeSecond.Error.ShouldContain("Can take ELF again after");
            Thread.Sleep(60 * 1000);

            // take for the third time
            var takeThird = _faucetContract3.Take("ELF", 3_00000000);
            takeThird.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.Mined);

            // return for the first time
            _faucetContract3.SetAccount(Target);
            _genesisContract = GenesisContract.GetGenesisContract(NodeManager, Target);
            _tokenContract = _genesisContract.GetTokenContract(Target);
            var approve2 = _tokenContract.ApproveToken(Target, _faucetContract3.ContractAddress, 10000000000_00000000);
            var targetReturnFirst =
                _faucetContract3.ExecuteMethodWithResult(FaucetContractMethod.Return,
                    new ReturnInput {Symbol = "ELF", Amount = 1_00000000});
            targetReturnFirst.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.Mined);

            // return for the second time
            var targetReturnSecond =
                _faucetContract3.ExecuteMethodWithResult(FaucetContractMethod.Return,
                    new ReturnInput {Symbol = "ELF", Amount = 2_00000000});
            targetReturnSecond.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.Mined);

            // return amount = 0
            var targetReturnThird =
                _faucetContract3.ExecuteMethodWithResult(FaucetContractMethod.Return,
                    new ReturnInput {Symbol = "ELF", Amount = 0});
            targetReturnSecond.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.Mined);
            var targetBalance = ViewTargetBalance();
            targetBalance.ShouldBe(0);
        }

        [TestMethod]
        public void TakeReturnTwiceTest()
        {
            // init
            var initialize = _faucetContract3.Initialize(InitAccount, 50_00000000, 300);
            initialize.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            // turn on(ELF)
            _faucetContract3.SetAccount(InitAccount);
            var turnOnTimeBefore = _faucetContract3.TurnOn("ELF");
            turnOnTimeBefore.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var faucetStatusTurnOnTimeBefore = _faucetContract3.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatusTurnOnTimeBefore.is_on is {faucetStatusTurnOnTimeBefore.IsOn}");
            Logger.Info($"faucetStatusTurnOnTimeBefore.turn_at is {faucetStatusTurnOnTimeBefore.TurnAt}");
            faucetStatusTurnOnTimeBefore.IsOn.ShouldBeTrue();
            faucetStatusTurnOnTimeBefore.TurnAt.ShouldNotBeNull();

            // turn on for the second time(ELF)
            Thread.Sleep(60 * 1000);
            var turnOnTimeBefore2 = _faucetContract3.TurnOn("ELF");
            turnOnTimeBefore2.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.NodeValidationFailed);
            turnOnTimeBefore2.Error.ShouldContain("Faucet of ELF is on.");

            // turn off(ELF)
            Thread.Sleep(60 * 1000);
            var turnOffTimeBefore = _faucetContract3.TurnOff("ELF");
            turnOffTimeBefore.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var faucetStatusTurnOffTimeBefore = _faucetContract3.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatusTurnOffTimeBefore.is_on is {faucetStatusTurnOffTimeBefore.IsOn}");
            Logger.Info($"faucetStatusTurnOffTimeBefore.turn_at is {faucetStatusTurnOffTimeBefore.TurnAt}");
            faucetStatusTurnOffTimeBefore.IsOn.ShouldBeFalse();
            faucetStatusTurnOffTimeBefore.TurnAt.ShouldNotBeNull();

            // turn off for the second time(ELF)
            Thread.Sleep(60 * 1000);
            var turnOffTimeBefore2 = _faucetContract3.TurnOff("ELF");
            turnOffTimeBefore2.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.NodeValidationFailed);
            turnOffTimeBefore2.Error.ShouldContain("Faucet of ELF is off.");
        }

        [TestMethod]
        public void TakeReturnFaucetOffTest()
        {
            // init
            _faucetContract3.SetAccount(InitAccount);
            var initialize = _faucetContract3.Initialize(InitAccount, 50_00000000, 50);
            initialize.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            // approve
            _genesisContract = GenesisContract.GetGenesisContract(NodeManager, InitAccount);
            _tokenContract = _genesisContract.GetTokenContract(InitAccount);
            _tokenContract.SetAccount(InitAccount);
            _faucetContract3.SetAccount(InitAccount);
            var approve =
                _tokenContract.ApproveToken(InitAccount, _faucetContract3.ContractAddress, 1000_00000000);
            approve.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            // pour
            var pour2 = _faucetContract3.Pour("ELF", 100_00000000);
            pour2.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);

            // turn off failed
            _faucetContract3.SetAccount(InitAccount);
            var turnOff = _faucetContract3.ExecuteMethodWithResult(FaucetContractMethod.TurnOff, new TurnInput());
            turnOff.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.NodeValidationFailed);
            turnOff.Error.ShouldContain("Faucet of ELF never turned on.");
            var faucetStatus = _faucetContract3.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatus.is_on is {faucetStatus.IsOn}");
            Logger.Info($"faucetStatus.turn_at is {faucetStatus.TurnAt}");
            faucetStatus.IsOn.ShouldBeFalse();
            faucetStatus.TurnAt.ShouldBeNull();

            // turn on(ELF)
            _faucetContract3.SetAccount(InitAccount);
            var turnOn = _faucetContract3.TurnOn("ELF");
            turnOn.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var faucetStatusTurnOn = _faucetContract3.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatusTurnOn.is_on is {faucetStatusTurnOn.IsOn}");
            Logger.Info($"faucetStatusTurnOn.turn_at is {faucetStatusTurnOn.TurnAt}");
            faucetStatusTurnOn.IsOn.ShouldBeTrue();
            faucetStatusTurnOn.TurnAt.ShouldNotBeNull();

            // take
            _faucetContract3.SetAccount(Target);
            var takeIsBanTrue = _faucetContract3.Take("ELF", 10_00000000);
            takeIsBanTrue.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.Mined);

            // turn off successed
            Thread.Sleep(60 * 1000);
            _faucetContract3.SetAccount(InitAccount);
            var turnOff1 = _faucetContract3.ExecuteMethodWithResult(FaucetContractMethod.TurnOff, new TurnInput());
            turnOff1.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.Mined);
            var faucetStatusFalse = _faucetContract3.GetFaucetStatus("ELF");
            Logger.Info($"faucetStatusFalse.is_on is {faucetStatusFalse.IsOn}");
            Logger.Info($"faucetStatusFalse.turn_at is {faucetStatusFalse.TurnAt}");
            faucetStatusFalse.IsOn.ShouldBeFalse();
            faucetStatusFalse.TurnAt.ShouldNotBeNull();

            // take
            _faucetContract3.SetAccount(Target);
            var take = _faucetContract3.Take("ELF", 10_00000000);
            take.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.NodeValidationFailed);
            take.Error.ShouldContain("Faucet of ELF is off.");

            // return
            _genesisContract = GenesisContract.GetGenesisContract(NodeManager, Target);
            _tokenContract = _genesisContract.GetTokenContract(Target);
            _tokenContract.SetAccount(Target);
            _faucetContract3.SetAccount(Target);
            var approve2 = _tokenContract.ApproveToken(Target, _faucetContract3.ContractAddress, 1000_00000000);
            approve2.Status.ConvertTransactionResultStatus().ShouldBe(TransactionResultStatus.Mined);
            var targetReturn1 =
                _faucetContract3.ExecuteMethodWithResult(FaucetContractMethod.Return,
                    new ReturnInput {Symbol = "ELF", Amount = 5_00000000});
            targetReturn1.Status.ConvertTransactionResultStatus()
                .ShouldBe(TransactionResultStatus.Mined);
        }

        [TestMethod]
        public long ViewTargetBalance()
        {
            _genesisContract = GenesisContract.GetGenesisContract(NodeManager, Target);
            _tokenContract = _genesisContract.GetTokenContract(Target);
            var targetBalance = _tokenContract.GetUserBalance(Target, "ELF");
            Logger.Info($"targetBalance is {targetBalance}");
            return targetBalance;
        }
    }
}