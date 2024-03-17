using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI_TabGida.Models;
using QRMenuAPI_TabGida.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace QRMenuAPI_TabGida.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public CompaniesController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Companies
        
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
        {
            if (_context.Companies == null)
            {
                return NotFound();
            }
            return await _context.Companies.ToListAsync();
        }

        // GET: api/Companies/5
        
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Company>> GetCompany(int id)
        {
            if (_context.Companies == null)
            {
                return NotFound();
            }
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
            {
                return NotFound();
            }

            return company;
        }

        // PUT: api/Companies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator,CompanyAdministrator")]
        [HttpPut("{id}")]
        public ActionResult PutCompany(int id, Company company)
        {
            if  (_context.Companies.FindAsync(id).Result == null)
            {
                return BadRequest();
            }
            else if (User.HasClaim("CompanyId", id.ToString()) == false)
            {
                return Unauthorized();
            }
            //if (id != company.Id)
            //{
                
            //}

            _context.Entry(company).State = EntityState.Modified;

            try
            {
                 _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Companies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public int PostCompany(Company company)
        {

            ApplicationUser applicationUser = new ApplicationUser();
            Claim claim;

            company.RegisterationDate = DateTime.Now;
            _context.Companies.Add(company);
            _context.SaveChanges();

            applicationUser.Name = company.Name + "Admin";
            applicationUser.UserName = company.Name + "Admin";
            applicationUser.CompanyId = company.Id;
            applicationUser.Email = "abc@def.com";
            applicationUser.PhoneNumber = "1112223344";
            applicationUser.RegisterationDate = DateTime.Today;
            applicationUser.StateId = 1;
            _userManager.CreateAsync(applicationUser, "Admin123!").Wait();
            claim = new Claim("CompanyId", company.Id.ToString());
            _userManager.AddClaimAsync(applicationUser, claim).Wait();

            ApplicationUser? AdministratorUser = _userManager.FindByNameAsync("TABGIDAAdmin").Result;
            _userManager.AddClaimAsync(AdministratorUser,claim).Wait();

            _userManager.AddToRoleAsync(applicationUser, "CompanyAdministrator").Wait();

            return company.Id;//CreatedAtAction("GetCompany", new { id = company.Id }, company);
        }

        // DELETE: api/Companies/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            if (_context.Companies == null)
            {
                return NotFound();
            }
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CompanyExists(int id)
        {
            return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
