using Assignment2.Data;
using Assignment2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace Assignment2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : Controller
    {
        [HttpGet("GetTransactionHist/{AccountNumber}")]
        public IActionResult GetTransactionHist(int AccountNumber)
        {
            try
            {
                List<Transactions> transactions = DBmanager.GetTransactionHistoryByBankAcc(AccountNumber);

                if (transactions == null || transactions.Count == 0)
                {
                    return NotFound(new { message = $"No transaction history found for account number {AccountNumber}." });
                }

                return Ok(transactions);  
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = "An error occurred while retrieving the transaction history.", error = ex.Message });
            }
        }

        [HttpGet("Transfer/{sender}/{recipient}/{amount}/{description}")]
        public IActionResult Transfer(int sender, int recipient, decimal amount, string description)
        {
            try
            {
                // Validate the transfer amount
                if (amount <= 0)
                {
                    return BadRequest(new { message = "Transfer amount must be greater than zero." });
                }

                // Attempt the transfer using the DBmanager.Transfer method
                bool success = DBmanager.Transfer(sender, recipient, amount, description);

                if (success)
                {
                    return Ok(new { message = "Transfer successful", sender = sender, recipient = recipient, amount = amount });
                }
                else
                {
                    return BadRequest(new { message = "Transfer failed. Please check the account numbers and try again." });
                }
            }
            catch (Exception ex)
            {
                // Return a 500 Internal Server Error with the exception message
                return StatusCode(500, new { message = "An error occurred during the transfer.", error = ex.Message });
            }
        }

        [HttpGet("Deposit/{AccountNumber}/{Amount}")]
        public IActionResult Deposit(int AccountNumber, decimal amount)
        {
            try
            {
                // Validate the amount
                if (amount <= 0)
                {
                    return BadRequest(new { message = "Deposit amount must be greater than zero." });
                }

                // Attempt to perform the deposit
                if (DBmanager.Deposit(AccountNumber, amount))
                {
                    return Ok(new { message = "Deposit is successful", accountNumber = AccountNumber, amountDeposited = amount });
                }

                // If deposit failed
                return BadRequest(new { message = "Deposit failed. Please check the account number and try again." });
            }
            catch (Exception ex)
            {
                // Return a 500 Internal Server Error with details
                return StatusCode(500, new { message = "An error occurred while depositing.", error = ex.Message });
            }
        }

        [HttpGet("Withdrawal/{AccountNumber}/{Amount}")]
        public IActionResult Withdrawal(int AccountNumber, decimal amount)
        {
            try
            {
                // Validate the amount
                if (amount <= 0)
                {
                    return BadRequest(new { message = "Withdrawal amount must be greater than zero." });
                }

                // Attempt to perform the withdrawal
                if (DBmanager.Withdrawal(AccountNumber, amount))
                {
                    return Ok(new { message = "Withdrawal is successful", accountNumber = AccountNumber, amountWithdrawn = amount });
                }

                // If withdrawal failed (insufficient funds or invalid account number)
                return BadRequest(new { message = "Withdrawal failed. Insufficient funds or invalid account number." });
            }
            catch (Exception ex)
            {
                // Return a 500 Internal Server Error with details
                return StatusCode(500, new { message = "An error occurred while withdrawing.", error = ex.Message });
            }
        }
    }
}
