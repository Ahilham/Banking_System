using Assignment2.Data;
using Assignment2.Models;
using Microsoft.AspNetCore.Mvc;

namespace Assignment2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        [HttpPost("Create")]
        public IActionResult Create([FromBody] UserProfile userProfile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                bool result = DBmanager.CreateUserProfile(userProfile);

                if (result)
                {
                    return StatusCode(201, "User profile created successfully.");
                }

                return BadRequest("Failed to create user profile.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the user profile.", error = ex.Message });
            }
        }

        // Get user profile by username
        [HttpGet("GetByUsername/{username}")]
        public IActionResult GetByUsername(string username)
        {
            try
            {
                var userProfile = DBmanager.GetProfileByUserName(username);

                if (userProfile == null)
                {
                    return NotFound(new { message = $"User profile with username '{username}' not found." });
                }

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the user profile.", error = ex.Message });
            }
        }

        // Get user profile by email
        [HttpGet("GetByEmail/{email}")]
        public IActionResult GetByEmail(string email)
        {
            try
            {
                var userProfile = DBmanager.GetProfileByEmail(email);

                if (userProfile == null)
                {
                    return NotFound(new { message = $"User profile with email '{email}' not found." });
                }

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the user profile.", error = ex.Message });
            }
        }

        [HttpPut("Update/{UserId}")]
        public IActionResult Update([FromBody] UserProfile userProfile, int UserId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Call the DBManager to update the profile
                bool result = DBmanager.UpdateUserProfile(userProfile, UserId);

                if (result)
                {
                    return Ok("User profile updated successfully.");
                }

                return NotFound(new { message = $"User profile with ID '{UserId}' not found or update failed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the user profile.", error = ex.Message });
            }
        }

        [HttpDelete("Delete/{UserId}")]
        public IActionResult Delete(int UserId)
        {
            try
            {
                bool result = DBmanager.DeleteUserProfile(UserId);

                if (result)
                {
                    return Ok("User profile deleted successfully.");
                }

                return NotFound(new { message = $"User profile with ID '{UserId}' not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user profile.", error = ex.Message });
            }
        }
    }
}
