using System;
using System.Collections.Generic;
using System.Data;
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
    public class FoodsController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FoodsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Foods

        [HttpGet]    //Return Food list
        [Authorize]
        public async Task<ActionResult<IEnumerable<Food>>> GetFoods()
        {
            if (_context.Foods == null)
            {
                return NotFound();
            }
            return await _context.Foods.ToListAsync();
        }

        // GET: api/Foods/5

        [HttpGet("{id}")]   //Find Food with given id and return it
        [Authorize]
        public async Task<ActionResult<Food>> GetFood(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (_context.Foods == null || food == null)
            {
                return NotFound();
            }
            return food;
        }

        // PUT: api/Foods/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPut("{id}")]   //Change the given properties of the Food
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult PutFood(int id, Food food)
        {
            if (User.HasClaim("RestaurantId", food.Category.Menu.RestaurantId.ToString()) == false)
            {
                return Unauthorized(); 
            }
            if (id != food.Id)
            {
                return BadRequest();
            }

            Food existingFood = _context.Foods.Find(id);

            existingFood.Name = food.Name;
            existingFood.Price = food.Price;
            existingFood.Description = food.Description;
            existingFood.StateId = food.StateId;

            _context.Foods.Update(existingFood);
            try
            {
                _context.SaveChanges();
                return Ok("Changes has been updated.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoodExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/Foods
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPost]  //Save given Food to database
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult PostFood(Food food)
        {
            if (User.HasClaim("RestaurantId", food.Category.Menu.RestaurantId.ToString()) == false)
            {
                return Unauthorized();
            }
            if (_context.Foods == null)
            {
                return Problem("Entity set 'ApplicationContext.Foods'  is null.");
            }
            _context.Foods.Add(food);
            _context.SaveChanges();

            return Ok($"Food linked to {food.Category.Name}/{food.Category.Menu.Name}/{food.Category.Menu.Restaurant} created and a FoodId has assigned: {(food.Id).ToString()}");
        }

        // DELETE: api/Foods/5

        [HttpDelete("{id}")]    //Delete given Food 
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult DeleteFood(int id)
        {
            Food food = _context.Foods.FindAsync(id).Result;
            if (User.HasClaim("RestaurantId", food.Category.Menu.RestaurantId.ToString()) == false)
            {
                return Unauthorized();
            }
            else if (_context.Foods == null || food == null)
            {
                return NotFound();
            }
           
            food.StateId = 0;
            _context.Foods.Update(food);
            return Ok("Food with given Id has been deleted");
        }

        private bool FoodExists(int id)
        {
            return (_context.Foods?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}