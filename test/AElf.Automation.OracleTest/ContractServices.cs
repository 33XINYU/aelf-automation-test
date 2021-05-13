using AElf.Types;
using AElfChain.Common.Contracts;
using AElfChain.Common.DtoExtension;
using AElfChain.Common.Managers;

namespace AElf.Automation.OracleTest
{
    public class ContractServices
    {
        public readonly INodeManager NodeManager;
        public AuthorityManager AuthorityManager;

        public ContractServices(string url, string callAddress, string password, string oracleContract, string aggregatorContract)
        {
            NodeManager = new NodeManager(url);
            AuthorityManager = new AuthorityManager(NodeManager, callAddress, password);
            CallAddress = callAddress.ConvertAddress();
            CallAccount = callAddress;
            
            OracleContract = oracleContract;
            AggregatorContract = aggregatorContract;
            NodeManager.UnlockAccount(CallAccount, password);
            GetContractServices();
        }

        public GenesisContract GenesisService { get; set; }
        public TokenContract TokenService { get; set; }
        public ParliamentContract ParliamentContract { get; set; }
        public OracleContract OracleService { get; set;}
        
        public string CallAccount { get; set; }
        public Address CallAddress { get; set; }
        public string OracleContract { get; set; }
        public string AggregatorContract { get; set; }

        private void GetContractServices()
        {
            GenesisService = GenesisContract.GetGenesisContract(NodeManager, CallAccount);

            //Token contract
            TokenService = GenesisService.GetTokenContract();
            ParliamentContract = GenesisService.GetParliamentContract();
            
            if(OracleContract == "")
                OracleService = new OracleContract(NodeManager, CallAccount);
            else
                OracleService = new OracleContract(NodeManager, CallAccount, OracleContract);
            if (AggregatorContract == "")
                AggregatorContract =
                    (AuthorityManager.DeployContractWithAuthority(CallAccount, "AElf.Contracts.IntegerAggregator"))
                    .ToBase58();
        }
    }
}