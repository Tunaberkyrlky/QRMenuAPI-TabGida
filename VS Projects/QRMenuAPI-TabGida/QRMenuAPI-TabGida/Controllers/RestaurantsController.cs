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
        private readonly SignInManager<ApplicationUser> _signInManager;

        public RestaurantsController(ApplicationContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
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
            var restaurant = await _context.Restaurants!.FindAsync(id);
            if (_context.Restaurants == null || restaurant == null)
            {
                return NotFound();
            }
            return restaurant;
        }

        // GET: api/RestaurantMenu/5
        [HttpGet("GetRestaurantMenu")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public async Task<ActionResult<Restaurant>> GetMenu(int RestaurantId)
        {
            var restaurant = await _context.Restaurants!.FindAsync(RestaurantId);
            if (User.HasClaim("RestaurantId", RestaurantId.ToString()) == false)
            {
                return Unauthorized();
            }
            else if (RestaurantId != restaurant.Id)
            {
                return BadRequest();
            }
            IQueryable<Menu> menu = _context.Menus.Include(m => m.Categories).ThenInclude(c => c.Foods).Where(m => m.RestaurantId == RestaurantId);
            return Ok(menu);
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

            
            RestaurantUser restaurantUser = new RestaurantUser()
            {
                UserId = applicationUser.Id,
                RestaurantId = restaurant.Id
            };

            //Give claim  to Company Admin
            ApplicationUser? companyAdmin = _userManager.Users.Where(u => u.CompanyId == restaurant.CompanyId).FirstOrDefault();
            _userManager.AddClaimAsync(companyAdmin, claim).Wait();

            return Ok($"Restaurant created and a RestaurantId has assigned: {(restaurant.Id).ToString()}");
        }

        // POST: api/User
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("NewRestaurantUser")]
        [Authorize(Roles = "RestaurantAdmin")]
        public ActionResult PostApplicationUser(ApplicationUser applicationUser, string password, int restaurantId)
        {
            var restaurant = _context.Restaurants.Where(r => r.Id == restaurantId).FirstOrDefault();
            if (User.HasClaim("RestaurantId", restaurant.Id.ToString()) == false)
            {
                return Unauthorized();
            }
            var UserId = applicationUser.Id;
            Claim userClaim = new Claim("UserId", UserId.ToString());
            _signInManager.UserManager.AddClaimAsync(applicationUser, userClaim).Wait();
            Claim RestaurantClaim = new Claim("RestaurantId", restaurantId.ToString());
            _signInManager.UserManager.AddClaimAsync(applicationUser, RestaurantClaim).Wait();
            Claim CompanyClaim = new Claim("CompanyId", restaurant.CompanyId.ToString());
            _signInManager.UserManager.AddClaimAsync(applicationUser, CompanyClaim).Wait();

            _signInManager.UserManager.CreateAsync(applicationUser, password).Wait();

            RestaurantUser restaurantUser = new RestaurantUser()
            {
                UserId = applicationUser.Id,
                RestaurantId = restaurant.Id
            };

            return Ok(applicationUser.Id);
        }

        [HttpDelete("{DeleteRestaurantUser}")]
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