using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AElf.Automation.Common.Helpers;
using AElf.Automation.Common.Managers;
using AElf.CSharp.Core;
using AElf.Types;
using AElfChain.SDK;
using AElfChain.SDK.Models;
using Google.Protobuf;
using log4net;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;

namespace AElf.Automation.Common.Contracts
{
    public class MethodStubFactory : IMethodStubFactory, ITransientDependency
    {
        private static readonly ILog Logger = Log4NetHelper.GetLogger();

        public MethodStubFactory(INodeManager nodeManager)
        {
            NodeManager = nodeManager;
        }

        public Address Contract { private get; set; }
        public string SenderAddress { private get; set; }
        public Address Sender => AddressHelper.Base58StringToAddress(SenderAddress);
        public INodeManager NodeManager { get; }
        public IApiService ApiService => NodeManager.ApiService;

        public IMethodStub<TInput, TOutput> Create<TInput, TOutput>(Method<TInput, TOutput> method)
            where TInput : IMessage<TInput>, new() where TOutput : IMessage<TOutput>, new()
        {
            async Task<IExecutionResult<TOutput>> SendAsync(TInput input)
            {
                var transaction = new Transaction
                {
                    From = Sender,
                    To = Contract,
                    MethodName = method.Name,
                    Params = ByteString.CopyFrom(method.RequestMarshaller.Serializer(input))
                };
                transaction.AddBlockReference(NodeManager.GetApiUrl(), NodeManager.GetChainId());
                transaction = NodeManager.TransactionManager.SignTransaction(transaction);

                var transactionOutput = await ApiService.SendTransactionAsync(transaction.ToByteArray().ToHex());

                var checkTimes = 0;
                TransactionResultDto resultDto;
                TransactionResultStatus status;
                while (true)
                {
                    checkTimes++;
                    resultDto = await ApiService.GetTransactionResultAsync(transactionOutput.TransactionId);
                    status = resultDto.Status.ConvertTransactionResultStatus();
                    if (status != TransactionResultStatus.Pending && status != TransactionResultStatus.NotExisted)
                    {
                        if (status == TransactionResultStatus.Mined)
                            Logger.Info(
                                $"TransactionId: {resultDto.TransactionId}, Method: {resultDto.Transaction.MethodName}, Status: {status}");
                        else
                            Logger.Error(
                                $"TransactionId: {resultDto.TransactionId}, Status: {status}\r\nDetail message: {JsonConvert.SerializeObject(resultDto)}");

                        break;
                    }

                    if (checkTimes % 20 == 0)
                        $"TransactionId: {resultDto.TransactionId}, Method: {resultDto.Transaction?.MethodName}, Status: {status}"
                            .WriteWarningLine();

                    if (checkTimes == 360) //max wait time 3 minutes
                        throw new Exception(
                            $"Transaction {resultDto.TransactionId} in '{status}' status more than three minutes.");

                    Thread.Sleep(500);
                }

                var transactionResult = resultDto.Logs == null
                    ? new TransactionResult
                    {
                        TransactionId = HashHelper.HexStringToHash(resultDto.TransactionId),
                        BlockHash = resultDto.BlockHash == null
                            ? null
                            : Hash.FromString(resultDto.BlockHash),
                        BlockNumber = resultDto.BlockNumber,
                        Bloom = ByteString.CopyFromUtf8(resultDto.Bloom),
                        Error = resultDto.Error ?? "",
                        Status = status,
                        ReadableReturnValue = resultDto.ReadableReturnValue ?? ""
                    }
                    : new TransactionResult
                    {
                        TransactionId = HashHelper.HexStringToHash(resultDto.TransactionId),
                        BlockHash = resultDto.BlockHash == null
                            ? null
                            : Hash.FromString(resultDto.BlockHash),
                        BlockNumber = resultDto.BlockNumber,
                        Logs =
                        {
                            resultDto.Logs.Select(o => new LogEvent
                            {
                                Address = AddressHelper.Base58StringToAddress(o.Address),
                                Name = o.Name,
                                NonIndexed = ByteString.FromBase64(o.NonIndexed)
                            }).ToArray()
                        },
                        Bloom = ByteString.CopyFromUtf8(resultDto.Bloom),
                        Error = resultDto.Error ?? "",
                        Status = status,
                        ReadableReturnValue = resultDto.ReadableReturnValue ?? ""
                    };

                return new ExecutionResult<TOutput>
                {
                    Transaction = transaction,
                    TransactionResult = transactionResult,
                    Output = method.ResponseMarshaller.Deserializer(ByteArrayHelper.HexStringToByteArray(resultDto.ReturnValue))
                };
            }

            async Task<TOutput> CallAsync(TInput input)
            {
                var transaction = new Transaction
                {
                    From = Sender,
                    To = Contract,
                    MethodName = method.Name,
                    Params = ByteString.CopyFrom(method.RequestMarshaller.Serializer(input))
                };
                transaction = NodeManager.TransactionManager.SignTransaction(transaction);

                var returnValue = await ApiService.ExecuteTransactionAsync(transaction.ToByteArray().ToHex());
                return method.ResponseMarshaller.Deserializer(ByteArrayHelper.HexStringToByteArray(returnValue));
            }

            return new MethodStub<TInput, TOutput>(method, SendAsync, CallAsync);
        }
    }
}