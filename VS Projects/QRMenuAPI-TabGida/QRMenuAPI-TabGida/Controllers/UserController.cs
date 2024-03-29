using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI_TabGida.Data;
using QRMenuAPI_TabGida.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace QRMenuAPI_TabGida.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(ApplicationContext context, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: api/User
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetUsers()
        {

            if (_context.Users == null)
            {
                return NotFound();
            }
            foreach (var user in _signInManager.UserManager.Users)
            {
                user.State = _context.States.FindAsync(user.StateId).Result;
            }
            return await _signInManager.UserManager.Users.ToListAsync();
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ApplicationUser>> GetApplicationUser(string id)
        {

            var applicationUser = await _signInManager.UserManager.FindByIdAsync(id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            return applicationUser;
        }

        // PUT: api/User/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public ActionResult PutApplicationUser(string id, ApplicationUser applicationUser)
        {
            if (User.HasClaim("UserId", applicationUser.Id))
            {
                return Unauthorized();
            }
            ApplicationUser existingApplicationUser = _signInManager.UserManager.FindByIdAsync(id).Result;
            existingApplicationUser.UserName = applicationUser.UserName;
            existingApplicationUser.Email = applicationUser.Email;
            existingApplicationUser.Name = applicationUser.Name;
            existingApplicationUser.PhoneNumber = applicationUser.PhoneNumber;
            existingApplicationUser.StateId = applicationUser.StateId;

            _signInManager.UserManager.UpdateAsync(existingApplicationUser).Wait();
            return Ok();
        }

        //// POST: api/User
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public string PostApplicationUser(ApplicationUser applicationUser, string password)
        //{
        //    var UserId = applicationUser.Id;
        //    Claim claim = new Claim("UserId", UserId.ToString());
        //    _signInManager.UserManager.AddClaimAsync(applicationUser, claim).Wait();
        //    _signInManager.UserManager.CreateAsync(applicationUser, password).Wait();

        //    return applicationUser.Id;
        //}
        // DELETE: api/User/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "RestaurantAdmin")]
        public async Task<IActionResult> DeleteApplicationUser(string id)
        {
            var user = _context.RestaurantUsers.Where(u => u.UserId == id).FirstOrDefault();
            
            if (User.HasClaim("RestaurantId", user.RestaurantId.ToString()) == false)
            {
                return Unauthorized();
            }
            else if (User.HasClaim("UserId", user.UserId.ToString()))
            {
                return Unauthorized();
            }
            var applicationUser = await _signInManager.UserManager.FindByIdAsync(id);
            if (applicationUser == null)
            {
                return NotFound();
            }
            //Hard delete
            //await _userManager.DeleteAsync(applicationUser);

            //Soft delete
            applicationUser.StateId = 0;
            await _signInManager.UserManager.UpdateAsync(applicationUser);
            return NoContent();
        }

        // POST: /Account/Login
        [HttpPost("Login")]
        public bool Login(string userName, string password)
        {
            SignInResult signInResult;
            ApplicationUser applicationUser = _signInManager.UserManager.FindByNameAsync(userName).Result;
            Claim claim;
            if (applicationUser == null)
            {
                return false;
            }
            else if (applicationUser.StateId !=1)
            {
                return false;
            }
            signInResult = _signInManager.PasswordSignInAsync(applicationUser, password, false, false).Result;
            if (signInResult.Succeeded == true)
            {
                claim = new Claim("CompanyId", applicationUser.CompanyId.ToString());
                _signInManager.UserManager.AddClaimAsync(applicationUser, claim);
                return true;
            }
            return signInResult.Succeeded;
        }

        [HttpPost("Logout")]
        public void Logout()
        {
            _signInManager.SignOutAsync();
        }

        [HttpPost("TokenlessResetPassword")]
        public void ResetPassword(string userName, string newPassword)
        {
            ApplicationUser applicationUser = _signInManager.UserManager.FindByNameAsync(userName).Result;

            if (applicationUser == null)
            {
                return;
            }
            _signInManager.UserManager.RemovePasswordAsync(applicationUser).Wait();
            _signInManager.UserManager.AddPasswordAsync(applicationUser, newPassword).Wait();
        }

        [HttpPost("ResetPassword")]
        public string? ResetPassword(string userName)
        {
            ApplicationUser applicationUser = _signInManager.UserManager.FindByNameAsync(userName).Result;

            if (applicationUser == null)
            {
                return null;
            }
            return _signInManager.UserManager.GeneratePasswordResetTokenAsync(applicationUser).Result;
        }
        [HttpPost("ValidateToken")]
        public ActionResult<string?> ValidatePasswordResetToken(string userName, string token, string newPassWord)
        {
            ApplicationUser applicationUser = _signInManager.UserManager.FindByNameAsync(userName).Result;

            if (applicationUser == null)
            {
                return null;
            }
            IdentityResult identityResult = _signInManager.UserManager.ResetPasswordAsync(applicationUser, token, newPassWord).Result;
            if (identityResult.Succeeded == false)
            {
                return identityResult.Errors.First().Description;
            }
            return Ok();
        }
        
        //[HttpPost("AssignRole")]
        //public ActionResult AssignRole(string userId, string roleId)
        //{
        //    ApplicationUser applicationUser = _signInManager.UserManager.FindByIdAsync(userId).Result;
        //    IdentityRole identityRole = _roleManager.FindByIdAsync(roleId).Result;

        //    _signInManager.UserManager.AddToRoleAsync(applicationUser, identityRole.Name).Wait();
        //    return Ok($"{identityRole.Name} Role with RoleId: {identityRole.Id} is assigned to {applicationUser.Name}");
        //}
    }
}