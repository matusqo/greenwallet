using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using greenwallet.Logic;
using greenwallet.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace greenwallet.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletMovementController : ControllerBase
    {
        private readonly IWalletMovementsHandler _walletMovementsHandler;

        public WalletMovementController(IWalletMovementsHandler walletMovementsHandler)
        {
            _walletMovementsHandler = walletMovementsHandler;
        }
        
        [HttpPost("deposit")]
        public async Task<ActionResult<string>> CreditFunds(string playerEmail, string operationId, decimal amount)
        {
            try
            {
                TransactionStatus transactionStatus = await _walletMovementsHandler.DepositFunds(new WalletMovementRequest
                {
                    WalletExternalId = playerEmail,
                    MovementExternalId = operationId,
                    Amount = amount
                }).ConfigureAwait(false);
                return transactionStatus.ToString();
            }
            catch (Exception e) when (e.Message.Contains("exists"))
            {
                return Problem(e.Message, null, (int)HttpStatusCode.Conflict);
            }
            catch (Exception e) when (e.Message.Contains("exist"))
            {
                return NotFound(e.Message);
            }
        }
    }
}
