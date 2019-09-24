using System.Collections.Generic;
using System.Threading.Tasks;
using Acs1;
using AElf.Automation.Common;
using AElf.Automation.Common.Contracts;
using AElf.Automation.Common.Helpers;
using AElf.Automation.Common.Managers;
using AElf.Automation.Common.Utils;
using AElf.Contracts.MultiToken;
using AElf.Contracts.TokenConverter;
using AElf.Kernel.SmartContract.ExecutionPluginForAcs8.Tests.TestContract;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Shouldly;

namespace AElf.Automation.Contracts.ScenarioTest
{
    [TestClass]
    public class TestChainTests
    {
        public INodeManager MainNode { get; set; }
        public INodeManager SideNode1 { get; set; }
        public INodeManager SideNode2 { get; set; }

        public string BpAccount { get; set; }

        public string TestSymbol = "STA";
        public ILog Logger { get; set; }

        public TestChainTests()
        {
            Log4NetHelper.LogInit();
            Logger = Log4NetHelper.GetLogger();

            MainNode = new NodeManager("18.212.240.254:8000");
            SideNode1 = new NodeManager("3.84.143.239:8000");
            SideNode2 = new NodeManager("34.224.27.242:8000");

            BpAccount = "2ZYyxEH6j8zAyJjef6Spa99Jx2zf5GbFktyAQEBPWLCvuSAn8D";
        }

        [TestMethod]
        [DataRow("", "TELF", 100_00000000)]
        public async Task TransferToken_Main(string to, string symbol, long amount)
        {
            var gensis = GenesisContract.GetGenesisContract(MainNode);
            var token = gensis.GetTokenContract();

            var beforeBalance = token.GetUserBalance(to, symbol);
            Logger.Info($"Before balance: {beforeBalance}");

            token.TransferBalance(BpAccount, to, amount, symbol);

            var afterBalance = token.GetUserBalance(to, symbol);
            Logger.Info($"After balance: {afterBalance}");
        }

        [TestMethod]
        [DataRow("TELF")]
        public async Task GetTokenConnector(string symbol)
        {
            var gensis = GenesisContract.GetGenesisContract(MainNode);
            var tokenConverter = gensis.GetTokenConverterStub();

            var result = await tokenConverter.GetConnector.CallAsync(new TokenSymbol
            {
                Symbol = symbol
            });

            Logger.Info($"Connector: {JsonConvert.SerializeObject(result)}");
        }

        [TestMethod]
        public async Task CreateConnector()
        {
            const long supply = 100_000_00000000;

            var gensis = GenesisContract.GetGenesisContract(MainNode);
            var tokenConverter = gensis.GetTokenConverterContract();

            var authority = new AuthorityManager(MainNode, BpAccount);
            var orgAddress = authority.GetGenesisOwnerAddress();
            var miners = authority.GetCurrentMiners();
            var connector = new Connector
            {
                Symbol = TestSymbol,
                IsPurchaseEnabled = true,
                IsVirtualBalanceEnabled = true,
                Weight = "0.5",
                VirtualBalance = supply
            };
            var transactionResult = authority.ExecuteTransactionWithAuthority(tokenConverter.ContractAddress,
                "SetConnector", connector, orgAddress, miners, BpAccount);
            transactionResult.Status.ShouldBe(TransactionResultStatus.Mined);

            await GetTokenConnector(TestSymbol);
        }

        [TestMethod]
        [DataRow(5000_00000000, "CPU")]
        [DataRow(5000_00000000, "STO")]
        [DataRow(5000_00000000, "NET")]
        [DataRow(5000_00000000, "RAM")]
        public async Task BuyResource(long amount, string symbol)
        {
            var gensis = GenesisContract.GetGenesisContract(MainNode, BpAccount);
            var tokenConverterAddress = gensis.GetTokenConverterContract().ContractAddress;
            var tokenConverter = gensis.GetTokenConverterStub();
            var token = gensis.GetTokenContract();
            Logger.Info($"Token converter token balance: {token.GetUserBalance(tokenConverterAddress, symbol)}");
            Logger.Info(
                $"Account: {BpAccount}, Before {NodeOption.NativeTokenSymbol}: {token.GetUserBalance(BpAccount)}");
            Logger.Info($"Account: {BpAccount}, Before {symbol}: {token.GetUserBalance(BpAccount, symbol)}");

            var transactionResult = await tokenConverter.Buy.SendAsync(new BuyInput
            {
                Symbol = symbol,
                Amount = amount,
                PayLimit = 0
            });
            CheckTransactionResult(transactionResult.TransactionResult);

            Logger.Info($"After {NodeOption.NativeTokenSymbol}: {token.GetUserBalance(BpAccount)}");
            Logger.Info($"After {TestSymbol}: {token.GetUserBalance(BpAccount, symbol)}");
        }

        [TestMethod]
        [DataRow(500)]
        public async Task SellResource(long amount)
        {
            var gensis = GenesisContract.GetGenesisContract(MainNode, BpAccount);
            var tokenConverter = gensis.GetTokenConverterStub();
            var token = gensis.GetTokenContract();
            var tokenStub = gensis.GetTokenStub();
            var tokenConverterAddress = gensis.GetTokenConverterContract().ContractAddress;

            Logger.Info($"Token converter token balance: {token.GetUserBalance(tokenConverterAddress, TestSymbol)}");
            Logger.Info($"Before {NodeOption.NativeTokenSymbol}: {token.GetUserBalance(BpAccount)}");
            Logger.Info($"Before {TestSymbol}: {token.GetUserBalance(BpAccount, TestSymbol)}");


            var allowanceResult = await tokenStub.Approve.SendAsync(new ApproveInput
            {
                Spender = tokenConverterAddress.ConvertAddress(),
                Symbol = TestSymbol,
                Amount = amount
            });
            CheckTransactionResult(allowanceResult.TransactionResult);

            var transactionResult = await tokenConverter.Sell.SendAsync(new SellInput
            {
                Symbol = TestSymbol,
                Amount = amount,
            });
            CheckTransactionResult(transactionResult.TransactionResult);

            Logger.Info($"After {NodeOption.NativeTokenSymbol}: {token.GetUserBalance(BpAccount)}");
            Logger.Info($"After {TestSymbol}: {token.GetUserBalance(BpAccount, TestSymbol)}");
            Logger.Info($"Token converter token balance: {token.GetUserBalance(tokenConverterAddress, TestSymbol)}");
        }

        [TestMethod]
        [DataRow("TELF", 100_0000)]
        public async Task Transfer_From_Main_To_Side(string symbol, long amount)
        {
        }

        [TestMethod]
        [DataRow("")]
        public async Task SideChain_Accept_MainTransfer(string rawTransaction)
        {
        }

        [TestMethod]
        [DataRow("STA", 100_0000)]
        public async Task Transfer_From_Side_To_Main(string symbol, long amount)
        {
        }

        [TestMethod]
        [DataRow("")]
        public async Task MainChain_Accept_SideTransfer(string rawTransaction)
        {
        }

        [TestMethod]
        public void CheckAllBpAccounts()
        {
            var bps = NodeInfoHelper.Config.Nodes;
            var genesis = GenesisContract.GetGenesisContract(MainNode);
            var token = genesis.GetTokenContract();

            foreach (var bp in bps)
            {
                var balance = token.GetUserBalance(bp.Account);
                Logger.Info($"Account: {bp.Account}, balance = {balance}");
            }

            var tokenConverterAddress = genesis.GetTokenConverterContract().ContractAddress;
            var tokenConverterTELF = token.GetUserBalance(tokenConverterAddress);
            var tokenConverterSTA = token.GetUserBalance(tokenConverterAddress, TestSymbol);
            Logger.Info($"TokenConverter: TELF={tokenConverterTELF}, STA={tokenConverterSTA}");
        }

        [TestMethod]
        public void SetTransactionFee_Main()
        {
            var authority = new AuthorityManager(MainNode, BpAccount);
            var miners = authority.GetCurrentMiners();
            var genesisOwner = authority.GetGenesisOwnerAddress();

            var genesis = MainNode.GetGenesisContract(BpAccount);
            var token = genesis.GetTokenContract();

            var input = new TokenAmounts
            {
                Method = "Approve",
                Amounts =
                {
                    new TokenAmount
                    {
                        Symbol = NodeOption.NativeTokenSymbol,
                        Amount = 1000
                    }
                }
            };
            var setTransactionFeeResult = authority.ExecuteTransactionWithAuthority(token.ContractAddress,
                "SetMethodFee", input, genesisOwner,
                miners, BpAccount);
            CheckTransactionResult(setTransactionFeeResult);
        }

        [TestMethod]
        public async Task VerifyTransactionFee_Main()
        {
            var genesis = MainNode.GetGenesisContract(BpAccount);
            var token = genesis.GetTokenContract();
            var tokenStub = genesis.GetTokenStub();

            var beforeBalance = token.GetUserBalance(BpAccount);

            var transactionResult = await tokenStub.Approve.SendAsync(new ApproveInput
            {
                Symbol = NodeOption.NativeTokenSymbol,
                Amount = 5000,
                Spender = genesis.Contract
            });
            CheckTransactionResult(transactionResult.TransactionResult);

            var afterBalance = token.GetUserBalance(BpAccount);
            Logger.Info($"Bp token: before={beforeBalance}, after={afterBalance}");
        }

        [TestMethod]
        public void SetTransactionFee_Side()
        {
        }

        [TestMethod]
        [DataRow("KDSkLLtkvKcAmFppPfRUGWdgtrPYVPRzYmCSA56tTaNcjgF7n")]
        public void TransferResource(string contract)
        {
            var symbols = new List<string> {"CPU", "NET", "STO", "RAM"};
            var genesis = MainNode.GetGenesisContract(BpAccount);
            var token = genesis.GetTokenContract();
            foreach (var symbol in symbols)
            {
                token.TransferBalance(BpAccount, contract, 5000_00000000, symbol);
            }
        }

        [TestMethod]
        [DataRow("KDSkLLtkvKcAmFppPfRUGWdgtrPYVPRzYmCSA56tTaNcjgF7n")]
        public void GetContractResource(string contract)
        {
            var symbols = new List<string> {"CPU", "NET", "STO", "RAM"};
            var genesis = MainNode.GetGenesisContract(BpAccount);
            var token = genesis.GetTokenContract();
            
            foreach (var symbol in symbols)
            {
                var balance = token.GetUserBalance(contract, symbol);
                Logger.Info($"Contract: {symbol}={balance}");
            }
        }

        [TestMethod]
        public async Task ExecuteAcs8Contract()
        {
            var contract = "KDSkLLtkvKcAmFppPfRUGWdgtrPYVPRzYmCSA56tTaNcjgF7n";
            var acs8Contract = new ExecutionPluginForAcs8Contract(MainNode, BpAccount, contract);
            var acs8Stub = acs8Contract.GetTestStub<ContractContainer.ContractStub>(BpAccount);

            var cpuResult = await acs8Stub.CpuConsumingMethod.SendAsync(new Empty());
            CheckTransactionResult(cpuResult.TransactionResult);

            var netResult = await acs8Stub.NetConsumingMethod.SendAsync(new NetConsumingMethodInput
            {
                Blob = ByteString.CopyFrom(CommonHelper.GenerateRandombytes(1024))
            });
            CheckTransactionResult(netResult.TransactionResult);

            var stoResult = await acs8Stub.StoConsumingMethod.SendAsync(new Empty());
            CheckTransactionResult(stoResult.TransactionResult);

            await Task.Delay(3000);
            
            GetContractResource(contract);
        }

        [TestMethod]
        public async Task CheckTransaction_Fee()
        {
            var nodeUrls = new List<string>
            {
                "18.212.240.254:8000",
                "54.183.221.226:8000",
                "13.230.195.6:8000",
                "35.183.35.159:8000",
                "34.255.1.143:8000",
                "18.163.40.216:8000",
                "3.1.211.78:8000",
                "13.210.243.191:8000",
                "18.231.115.220:8000",
                "35.177.181.31:8000"
            };
            foreach (var url in nodeUrls)
            {
                Logger.Info($"Test endpoint: {url}");
                
                var nodeManager = new NodeManager(url);
                var genesis = nodeManager.GetGenesisContract();
                var token = genesis.GetTokenContract();

                var tokenAmount = token.CallViewMethod<TokenAmounts>(TokenMethod.GetMethodFee, new MethodName
                {
                    Name = "Transfer"
                });
                Logger.Info(tokenAmount);
            }
        }

        private void CheckTransactionResult(TransactionResult result)
        {
            if (!result.Status.Equals(TransactionResultStatus.Mined))
                Logger.Error(result.Error);
        }
    }
}