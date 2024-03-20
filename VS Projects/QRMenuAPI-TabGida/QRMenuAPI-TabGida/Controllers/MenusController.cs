using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI_TabGida.Data;
using QRMenuAPI_TabGida.Models;

namespace QRMenuAPI_TabGida.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenusController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public MenusController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Menus

        [HttpGet]   //Return Menu list
        [Authorize]
        public async Task<ActionResult<IEnumerable<Menu>>> GetMenus()
        {
            if (_context.Menus == null)
            {
                return NotFound();
            }
            return await _context.Menus.ToListAsync(); 
        }

        // GET: api/Menus/5

        [HttpGet("{id}")]   //Find Menu with given id and return it
        [Authorize]
        public async Task<ActionResult<Menu>> GetMenu(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (_context.Menus == null || menu == null)
            {
                return NotFound();
            }
            return menu;
        }

        // PUT: api/Menus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPut("{id}")]   //Change the given properties of the Menu
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult PutMenu(int id, Menu menu)
        {
            if (User.HasClaim("RestaurantId", menu.RestaurantId.ToString()) == false)
            {
                return Unauthorized();
            }
            else if (id != menu.Id)
            {
                return BadRequest();
            }

            Menu existingMenu= _context.Menus.Find(id);

            existingMenu.Name = menu.Name;
            existingMenu.Description = menu.Description;
            existingMenu.StateId = menu.StateId;
           

            _context.Menus.Update(existingMenu);
            try
            {
                _context.SaveChanges();
                return Ok("Changes has been updated.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/Menus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPost]  //Save given Menu to database
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult PostMenu(Menu menu)
        {
            if (User.HasClaim("RestaurantId", menu.RestaurantId.ToString()) == false)
            {
                return Unauthorized();
            }
            else if (_context.Menus == null)
            {
                return Problem("Entity set 'ApplicationContext.Menus'  is null.");
            }

            _context.Menus.Add(menu);
            _context.SaveChanges();
            Restaurant parentRestaurant = _context.Restaurants.Where(r => r.Id == menu.RestaurantId).FirstOrDefault();
            return Ok($"Menu linked to {parentRestaurant.Name} created and a MenuId has assigned: {(menu.Id).ToString()}");
        }

        // DELETE: api/Menus/5
        [HttpDelete("{id}")]    //Delete given Menu besides all the data linked to that Menu
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult DeleteMenu(int id)
        {
            var menu = _context.Menus.Find(id);

            if (User.HasClaim("RestaurantId", menu.RestaurantId.ToString()) == false)
            {
                return Unauthorized();
            }
            else if (_context.Menus == null || menu == null)
            {
                return Problem("Entity set 'ApplicationContext.Menus'  is null.");
            }

            if (menu != null)
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
                _context.SaveChanges();
                return Ok("Menu with given Id and all the data linked to the Menu has been deleted");
            }
            else
            {
                return NotFound();
            }
        }

        private bool MenuExists(int id)
        {
            return (_context.Menus?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}