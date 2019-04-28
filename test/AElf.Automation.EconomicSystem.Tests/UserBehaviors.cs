using AElf.Automation.Common.Contracts;
using AElf.Automation.Common.Helpers;
using AElf.Contracts.MultiToken.Messages;
using AElf.Kernel;

namespace AElf.Automation.EconomicSystem.Tests
{
    public class UserBehaviors
    {
        public readonly RpcApiHelper ApiHelper;
        public readonly ContractServices ContractServices;
        
        public readonly ElectionContract ElectionService;
        public readonly VoteContract VoteService;
        public readonly ProfitContract ProfitService;
        public readonly TokenContract TokenService;
        public readonly ConsensusContract ConsensusService;

        public UserBehaviors(ContractServices contractServices)
        {
            ApiHelper = contractServices.ApiHelper;
            ContractServices = contractServices;
            
            ElectionService = ContractServices.ElectionService;
            VoteService = ContractServices.VoteService;
            ProfitService = ContractServices.ProfitService;
            TokenService = ContractServices.TokenService;
            ConsensusService = ContractServices.ConsensusService;
        }
        
        //action
        public CommandInfo UserVote(string account,string candidate, int lockTime, long amount)
        {
            ElectionService.SetAccount(account);
            var vote = ElectionService.ExecuteMethodWithResult(ElectionMethod.Vote, new VoteMinerInput
            {
                CandidatePublicKey = ApiHelper.GetPublicKeyFromAddress(candidate),
                LockTime = lockTime,
                LockTimeUnit = LockTimeUnit.Days,
                Amount = amount,
            });

            return vote;
        }

        public CommandInfo TransferToken(string from, string to, long amount, string symbol = "ELF")
        {
            TokenService.SetAccount(from);
            
            return TokenService.ExecuteMethodWithResult(TokenMethod.Transfer, new TransferInput
            {
                Symbol = symbol,
                Amount = amount,
                To = Address.Parse(to),
                Memo = $"transfer {from}=>{to} with amount {amount}."
            });
        }
    }
}