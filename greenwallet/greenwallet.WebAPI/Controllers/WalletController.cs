using System;
using System.Threading.Tasks;
using greenwallet.Logic;
using Microsoft.AspNetCore.Mvc;

namespace greenwallet.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletHandler _walletHandler;
        private readonly IWalletMovementsHandler _walletMovementsHandler;

        public WalletController(IWalletHandler walletHandler, IWalletMovementsHandler walletMovementsHandler)
        {
            _walletHandler = walletHandler;
            _walletMovementsHandler = walletMovementsHandler;
        }

        [HttpPost]
        public Task Register(string playerEmail) => _walletHandler.RegisterNew(playerEmail);

        [HttpGet("balance")]
        public async Task<ActionResult<decimal>> GetBalance(string playerEmail)
        {
            try
            {
                return await _walletHandler.GetWalletBalance(playerEmail);
            }
            catch (Exception e) when(e.Message.Contains("exist"))
            {
                return NotFound();
            }
        }
    }
}
