using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TenmoServer.DAO;
using TenmoServer.Models;
using TenmoServer.Security;

namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private IUserDAO userDao;

        private int GetCurrentUserId()
        {
            string userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userId)) return -1;
            int.TryParse(userId, out int userIdInt);
            return userIdInt;
        }
        public UserController(IUserDAO dao)
        {
            userDao = dao;
        }
      

        [HttpGet]

        public List<User> GetUsers()
        {
            return userDao.GetUsers();
        }

        [HttpGet("/user/{userName}")]

        public User GetUser(string userName)
        {
            return userDao.GetUser(userName);
        }
        
        

        [HttpGet("/user/balance")]

        public decimal GetUserBalance()
        {
            
            return userDao.GetCurrentBalance(GetCurrentUserId());
        }
        [HttpGet("/user/transfers")]
        public List<Transfer>ShowTransfers ()
        {

            return userDao.ShowUserTransfers(GetCurrentUserId());
        }
        [HttpGet("/user/transfers/{transferId}")]
        public Transfer ShowTransfer(int transferId)
        {
            return userDao.GetTransfer(transferId);
        }


        [HttpPut("/user/{transactionAmount}/{recipient}")]
        public ActionResult<decimal> TransferFunds(decimal transactionAmount, int recipient)
        {
            
            int transactionType = 1;
            int transferStatusID = 2;
            decimal currBalance = GetUserBalance();
            if (transactionAmount < 0)
            {
                if (currBalance < Math.Abs(transactionAmount))
                {
                    return BadRequest("Insufficient funds.");
                    transferStatusID = 3;
                }
                transactionType = 2;
            }
            

            //User existing = GetUser(userName);
            //if (existing == null)
            //{
            //    return NotFound("User doesn't exist.");
                
            //}

            decimal result = userDao.TransferFunds(transactionAmount, GetCurrentUserId(), recipient);
            userDao.AddTransfer(Math.Abs(transactionAmount), transactionType, transferStatusID, GetCurrentUserId(), recipient);
            return Ok(result);
        }
    }
}
