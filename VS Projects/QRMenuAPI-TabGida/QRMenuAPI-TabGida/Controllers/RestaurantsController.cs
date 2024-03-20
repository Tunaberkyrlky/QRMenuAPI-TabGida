using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI_TabGida.Data;
using QRMenuAPI_TabGida.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace QRMenuAPI_TabGida.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantsController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RestaurantsController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Restaurants

        [HttpGet]   //Return Restaurant list
        [Authorize]
        public async Task<ActionResult<IEnumerable<Restaurant>>> GetRestaurants()
        {
            if (_context.Restaurants == null)
            {
                return NotFound();
            }
            return await _context.Restaurants.ToListAsync();
        }

        // GET: api/Restaurants/5

        [HttpGet("{id}")]   //Find Restaurant with given id and return it
        [Authorize]
        public async Task<ActionResult<Restaurant>> GetRestaurant(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (_context.Restaurants == null || restaurant == null)
            {
                return NotFound();
            }
            return restaurant;
        }

        // PUT: api/Restaurants/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPut("{id}")]    //Change the given properties of the Restaurant
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult PutRestaurant(int id, Restaurant restaurant)
        {
            if (User.HasClaim("RestaurantId", id.ToString()) == false)
            {
                return Unauthorized();
            }
            else if (id != restaurant.Id)
            {
                return BadRequest();
            }

            Restaurant existingRestaurant = _context.Restaurants.Find(id);

            existingRestaurant.Name = restaurant.Name;
            existingRestaurant.Phone = restaurant.Phone;
            existingRestaurant.EMail = restaurant.EMail;
            existingRestaurant.PostalCode = restaurant.PostalCode;
            existingRestaurant.AddressDetails = restaurant.AddressDetails;
            existingRestaurant.CompanyId = restaurant.CompanyId;
            existingRestaurant.StateId = restaurant.StateId;

            _context.Restaurants.Update(existingRestaurant);
            try
            {
                _context.SaveChanges();
                return Ok("Changes has been updated.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RestaurantExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/Restaurants
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPost]   //Save given Restaurant to database and create an admin user for Restaurant
        [Authorize(Roles = "CompanyAdministrator")]  //Restaurant Admin Login info: //UserName; (RestaurantName)Admin
                                                                                    //Password; Admin123!
        public ActionResult PostRestaurant(Restaurant restaurant)
        {
            if (User.HasClaim("CompanyId", restaurant.CompanyId.ToString()) == false)
            {
                return Unauthorized();
            }
            ApplicationUser applicationUser = new ApplicationUser();
            Claim claim;

            //Save Restaurant
            restaurant.RegisterationDate = DateTime.Now;
            _context.Restaurants.Add(restaurant);
            _context.SaveChanges();

            //Create an Admin user
            applicationUser.Name = restaurant.Name + "Admin";
            applicationUser.UserName = restaurant.Name + "Admin";
            applicationUser.CompanyId = restaurant.CompanyId;
            applicationUser.Email = "abc@def.com";
            applicationUser.PhoneNumber = "1112223344";
            applicationUser.RegisterationDate = DateTime.Today;
            applicationUser.StateId = 1;
            _userManager.CreateAsync(applicationUser, "Admin123!").Wait();

            //Create a claim for users that has type "RestaurantId" and value; RestaurantId
            claim = new Claim("RestaurantId", restaurant.Id.ToString());

            //Add claim and role to Restaurant Admin
            _userManager.AddClaimAsync(applicationUser, claim).Wait();
            _userManager.AddToRoleAsync(applicationUser, "RestaurantAdministrator").Wait();

            //Give claim  to Company Admin
            ApplicationUser? companyAdmin = _userManager.Users.Where(u => u.CompanyId == restaurant.CompanyId).FirstOrDefault();
            _userManager.AddClaimAsync(companyAdmin, claim).Wait();

            return Ok($"Restaurant created and a RestaurantId has assigned: {(restaurant.Id).ToString()}");
        }

        // DELETE: api/Restaurants/5

        [HttpDelete("{id}")]    //Delete given Restaurant besides all the data linked to that Restaurant
        [Authorize(Roles = "CompanyAdmin")]
        public ActionResult DeleteRestaurant(int id)
        {
            if (User.HasClaim("RestaurantId", id.ToString()) == false)
            {
                return Unauthorized();
            }
            else if (_context.Restaurants == null)
            {
                return Problem("Entity set 'ApplicationContext.Restaurants'  is null.");
            }

            var restaurant = _context.Restaurants.Find(id);
            if (restaurant != null)
            {

                restaurant.StateId = 0;
                _context.Restaurants.Update(restaurant);
                IQueryable<Menu>? menus = _context.Menus.Where(m => m.RestaurantId == restaurant.Id);
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
                _context.SaveChanges();
                return Ok("Restaurant with given Id and all the data linked to the Restaurant has been deleted");
            }
            else
            {
                return NotFound();
            }
        }

        private bool RestaurantExists(int id)
        {
            return (_context.Restaurants?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}