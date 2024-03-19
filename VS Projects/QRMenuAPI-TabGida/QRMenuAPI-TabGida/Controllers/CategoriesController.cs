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
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public CategoriesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Categories

        [HttpGet]   //Return Category list
        [Authorize]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategory()
        {
            if (_context.Categories == null)
            {
                return NotFound();
            }
            return await _context.Categories.ToListAsync();
        }

        // GET: api/Categories/5

        [HttpGet("{id}")]   //Find Category with given id and return it
        [Authorize]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (_context.Categories == null || category == null)
            {
                return NotFound();
            }
            return category;
        }

        // PUT: api/Categories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPut("{id}")]    //Change the given properties of the Category
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult PutCategory(int id, Category category)
        {
            if (User.HasClaim("RestaurantId", category.Menu.RestaurantId.ToString()) == false)
            {
                return Unauthorized();
            }
            else if (id != category.Id)
            {
                return BadRequest();
            }

            Category existingCategory = _context.Categories.Find(id);

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.StateId = category.StateId;


            _context.Categories.Update(existingCategory);
            try
            {
                _context.SaveChanges();
                return Ok("Changes has been updated.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/Categories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPost]  //Save given Category to database
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult PostCategory(Category category)
        {
            if (User.HasClaim("RestaurantId", category.Menu.RestaurantId.ToString()) == false)
            {
                return Unauthorized();
            }
            else if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationContext.Category'  is null.");
            }

            _context.Categories.Add(category);
            _context.SaveChanges();

            return Ok($"Category linked to {category.Menu.Name}/{category.Menu.Restaurant.Name} created and a CategoryId has assigned: {(category.Id).ToString()}");
        }

        // DELETE: api/Categories/5

        [HttpDelete("{id}")]    //Delete given Menu besides all the data linked to that Menu
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult DeleteCategory(int id)
        {
            Category category = _context.Categories.FindAsync(id).Result;
            var restaurantId = _context.Categories.Include(c => c.Menu).ThenInclude(m => m.RestaurantId);
            if (User.HasClaim("RestaurantId", restaurantId.ToString()) == false)
            {
                return Unauthorized();
            }
            else if (_context.Categories == null || category == null)
            {
                return Problem("Entity set 'ApplicationContext.Menus'  is null.");
            }

            if (category != null)
            {
                category.StateId = 0;
                _context.Categories.Update(category);
                IQueryable<Food> foods = _context.Foods.Where(f => f.CategoryId == category.Id);
                foreach (Food food in foods)
                {
                    food.StateId = 0;
                    _context.Foods.Update(food);
                }
                _context.SaveChanges();
                return Ok("Category with given Id and all the data linked to the Category has been deleted");
            }
            else
            {
                return NotFound();
            }
        }

        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}