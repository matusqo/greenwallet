using greenwallet.Logic;

namespace greenwallet.TestsCommon
{
    public interface IPrepareDataService
    {
        WalletMovementRequest GetMovementRequestMissingPlayerId();
        WalletMovementRequest GetMovementRequestMissingExternalId();
    }
}