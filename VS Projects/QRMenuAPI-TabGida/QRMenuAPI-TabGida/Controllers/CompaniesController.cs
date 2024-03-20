using System;
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

        [HttpGet]   //Return company list
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

        [HttpGet("{id}")]   //Find company with given id and return it
        [Authorize]
        public async Task<ActionResult<Company>> GetCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (_context.Companies == null || company == null)
            {
                return NotFound();
            }

            return company;
        }

        // PUT: api/Companies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPut("{id}")]   //Change the given properties of the Company
        [Authorize(Roles = "CompanyAdministrator")]
        public ActionResult PutCompany(int id, Company company)
        {
            if (User.HasClaim("CompanyId", id.ToString()) == false)
            {
                return Unauthorized("Unauthorized User");
            }
            else if (_context.Companies.FindAsync(id).Result == null)
            {
                return BadRequest();
            }

            Company existingCompany = _context.Companies.Find(id);
            
            existingCompany.Name = company.Name;
            existingCompany.Phone = company.Phone;
            existingCompany.EMail = company.EMail;
            existingCompany.TaxNumber = company.TaxNumber;
            existingCompany.PostalCode = company.PostalCode;
            existingCompany.AddressDetails = company.AddressDetails;
            existingCompany.WebAddress = company.WebAddress;
            existingCompany.StateId = company.StateId;

            _context.Update(existingCompany);
            try
            {
                _context.SaveChanges();
                return Ok("Changes has been updated.");
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
        }

        // POST: api/Companies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPost]  //Save given Company to database and create an admin user for Company
        [Authorize(Roles = "Administrator")]    //Company Admin Login info: //UserName; (CompanyName)Admin
                                                                            //Password; Admin123!
        public ActionResult PostCompany(Company company)
        {
            ApplicationUser applicationUser = new ApplicationUser();
            Claim claim;

            //Save Company
            company.RegisterationDate = DateTime.Now;
            _context.Companies.Add(company);
            _context.SaveChanges();

            //Create an Admin user
            applicationUser.Name = company.Name + "Admin";
            applicationUser.UserName = company.Name + "Admin";
            applicationUser.CompanyId = company.Id;
            applicationUser.Email = "EditData@def.com";
            applicationUser.PhoneNumber = "1112223344";
            applicationUser.RegisterationDate = DateTime.Today.Date;
            applicationUser.StateId = 1;
            _userManager.CreateAsync(applicationUser, "Admin123!").Wait();

            //Create a claim for users that has type "CompanyId" and value; CompanyId
            claim = new Claim("CompanyId", company.Id.ToString());

            //Add claim and role to Company Admin
            _userManager.AddClaimAsync(applicationUser, claim).Wait();
            _userManager.AddToRoleAsync(applicationUser, "CompanyAdministrator").Wait();

            //Also give a claim to Administrator
            ApplicationUser? AdministratorUser = _userManager.FindByNameAsync("TABGIDAAdmin").Result;
            _userManager.AddClaimAsync(AdministratorUser, claim).Wait();

            return Ok($"Company created and a CompanyId has assigned: {(company.Id).ToString()}");
        }

        // DELETE: api/Companies/5

        [HttpDelete("{id}")]    //Delete given Company besides all the data linked to that Company
        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteCompany(int id)
        {
            if (User.HasClaim("CompanyId", id.ToString()) == false)
            {
                return Unauthorized();
            }
            else if (_context.Companies == null)
            {
                return Problem("Entity set 'ApplicationContext.Companies'  is null.");
            }

            var company = _context.Companies.Find(id);
            if (company != null)
            {
                company.StateId = 0;
                _context.Companies.Update(company);
                IQueryable<Restaurant> restaurants = _context.Restaurants.Where(r => r.CompanyId == id);
                foreach (Restaurant restaurant in restaurants)
                {
                    restaurant.StateId = 0;
                    _context.Restaurants.Update(restaurant);
                    IQueryable<Menu> menus = _context.Menus.Where(m => m.RestaurantId == restaurant.Id);
                    foreach (Menu menu in menus)
                    {
                        menu.StateId = 0;
                        _context.Menus.Update(menu);
                        IQueryable<Category> categories = _context.Categories.Where(c => c.MenuId == menu.Id);
                        foreach (Category category in categories)
                        {
                            category.StateId = 0;
                            _context.Categories.Update(category);
                            IQueryable<Food> foods = _context.Foods.Where(f => f.CategoryId == category.Id);
                            foreach (Food food in foods)
                            {
                                food.StateId = 0;
                                _context.Foods.Update(food);
                            }
                        }
                    }
                }
                _context.SaveChanges();
                return Ok("Company with given Id and all the data linked to the Company has been deleted");
            }
            else
            {
                return NotFound();
            }
        }

        private bool CompanyExists(int id)
        {
            return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}