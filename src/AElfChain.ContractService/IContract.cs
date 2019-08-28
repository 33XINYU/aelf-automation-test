using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.CSharp.Core;
using AElf.Types;
using AElfChain.AccountService;
using AElfChain.SDK.Models;
using Google.Protobuf;

namespace AElfChain.ContractService
{
    public interface IContract
    {
        string GetContractFileName();
        TContract DeployContract<TContract>(AccountInfo accountInfo) where TContract: IContract;
        TContract GetContractService<TContract>(Address contract, AccountInfo accountInfo) where TContract : IContract;
        Task SetContractExecutor(string account, string password = AccountOption.DefaultPassword);
        Task<string> SendTransactionWithIdAsync(string method, IMessage input);
        Task<List<string>> SendBatchTransactionsWithIdAsync(List<Transaction> transactions);
        Task<TransactionResultDto> SendTransactionWithResultAsync(string method, IMessage input);
        TResult CallTransactionAsync<TResult>(string method, IMessage input) where TResult : IMessage<TResult>;
        TStub GetTestStub<TStub>(Address contract, AccountInfo accountInfo) where TStub : ContractStubBase;
    }
}