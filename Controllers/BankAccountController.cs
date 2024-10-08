using Assignment2.Data;
using Assignment2.Models;
using Microsoft.AspNetCore.Mvc;

namespace Assignment2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankAccountController : Controller
    {
        [HttpPost("Create")]
        public IActionResult Create([FromBody] BankAccount bankAccount)
        {   //create validation for bankaccount object
            try
            {
                if (DBmanager.CreateBankAccount(bankAccount))
                {
                    return Ok("Bank Account created successfully.");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the account.", error = ex.Message });
            }
        }

        [HttpGet("GetByAccNum/{accNum}")]
        public IActionResult GetByAccNum(int accNum)
        {
            try
            {
                BankAccount bankAccount = DBmanager.GetAccountByAccNum(accNum);
                if (bankAccount == null)
                {
                    return NotFound();
                }
                return Ok(bankAccount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the account.", error = ex.Message });
            }
            
        }

        [HttpPost("Update")]
        public IActionResult Update([FromBody] BankAccount bankAccount)
        {
            // create validation
            try
            {
                if (DBmanager.UpdateBankAccount(bankAccount))
                {
                    return Ok("Bank Account updated successfully.");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the account.", error = ex.Message });
            }
        }

        [HttpDelete("delete/{AccountNumber}")]
        public IActionResult Delete(int AccountNumber)
        {
            try
            {
                if (DBmanager.DeleteBankAccount(AccountNumber))
                {
                    return Ok("Bank Account deleted successfully.");
                }
                return BadRequest();
            }catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the account.", error = ex.Message });
            }
        }
    }
}
