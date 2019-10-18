using AElf.Automation.Common.Helpers;
using AElf.Automation.Common.Managers;

namespace AElfChain.Console.Commands
{
    public class UpdateCommand : BaseCommand
    {
        public UpdateCommand(INodeManager nodeManager, ContractServices contractServices) 
            : base(nodeManager, contractServices)
        {
        }
        public override void RunCommand()
        {
            var parameters = InputParameters();
            if (parameters == null)
                return;

            Services.Authority.UpdateContractWithAuthority(parameters[0], parameters[1], parameters[2]);
        }

        public override CommandInfo GetCommandInfo()
        {
            return new CommandInfo
            {
                Name = "update",
                Description = "Update contract with authority permission"
            };
        }

        public override string[] InputParameters()
        {
            string from = "28Y8JA1i2cN6oHvdv7EraXJr9a1gY6D1PpJXw9QtRMRwKcBQMK";
            string contractAddress = "2F5C128Srw5rHCXoSY2C7uT5sAku48mkgiaTTp1Hiprhbb7ED9";
            string filename = "AElf.Contracts.MultiToken";
            
            "Parameter: [From] [ContractAddress] [ContractFileName]".WriteSuccessLine();
            $"eg: {from} {contractAddress} {filename}".WriteSuccessLine();
            
            return CommandOption.InputParameters(3);
        }   
    }
}