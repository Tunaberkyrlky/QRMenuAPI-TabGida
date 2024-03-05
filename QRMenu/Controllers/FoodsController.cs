using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QRMenu.Data;
using QRMenu.Models;

namespace QRMenu.Controllers
{
	public class FoodsController : Controller
	{
		private readonly ApplicationContext _context;

		public FoodsController(ApplicationContext context)
		{
			_context = context;
		}
        //TÜM METHODLARDA INTERFACE YERİNE CLASS KULLANILDI
        // GET: Foods

		public ViewResult Index(bool admin = false)
		{
            IQueryable<Food> foods = _context.Foods!;
            if (admin == false) //Admin olmayanlara sadece Active statete olan sorgular döner
            {
                foods = foods.Where(f => f.StateId == 1);
            }
            ViewData["admin"] = admin;  //admin olanlara ayrı kullanıcılara ayrı view göndermek için frontend'e admini gönderdik.
            //where koşul ekler, ToList listeye dönüştürür, OrderBy(f=>f.Name) isme göre sıralama şartı koyar.
			return View(foods.ToList());
		}
		//view result sadece view döndürür actionresult ise view harici şeyler de döndürebilir. NotFound gibi.
		//Aşağıda Details methodunda hem notfound hem view döndürdüğümüz için ActionResult Kullanmak zorundayız.
		public ActionResult Details(int id)
		{
			Food? food = _context.Foods!.Where(f => f.Id == id).Include(f=>f.State).FirstOrDefault();

			if (food == null)
			{
				return NotFound();
			}
			return View(food);
		}

        // GET: Foods/Create
        public ViewResult Create()
		{ 	//
            ViewData["StateId"] = new SelectList(_context.Set<State>(), "Id", "Name");
            return View();
        }

        // POST: Foods/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]              //frontendten gelen veriler ile bir food objesi yapıldı.
        public ActionResult Create([Bind("Id,Name,Price,Description,StateId")] Food food)
        {   //async Task<IActionResult> yazıyordu sildik.
            //Modelde tanımlanan propertylerin koşulları(maxlength vb.) Create viewdan gelirken {await Html.RenderPartialAsync("_ValidationScriptsPartial") kodu ile kontrol edilir eğer her şey olması gerektiği gibiyse valid verilir.
            if (ModelState.IsValid)
            {
                //foodu context'e ekle
                _context.Add(food);
                //her şeyi kaydet
                _context.SaveChanges(); //await vardı sildik.
                //Index isimli actiona geri dön satır 21. Bu sayede yeni verileri gösterir.
                return RedirectToAction(nameof(Index));
            }
            ViewData["StateId"] = new SelectList(_context.Set<State>(), "Id", "Name", food.StateId);
            //kullanıcının yazdığı verileri silmeden göstermek için View içerisine food gönderiyoruz.
            return View(food);
        }

        // GET: Foods/Edit/5
        public ActionResult Edit(int id)
        {
            //_context.Foods tablosunun varlığından eminiz ve null olamaz anlamında ! koyuyoruz fakat verilen id değerinde food olmayabilir bu nedenle değişkenimiz nullable olacak Food?.
            Food? food = _context.Foods!.Find(id); //await ve asnc sildik. Ünlem işareti null olamaz anlamında. 

            if (food == null)
            {
                return NotFound();
            }
            ViewData["StateId"] = new SelectList(_context.Set<State>(), "Id", "Name", food.StateId);
            return View(food);
        }

        // POST: Foods/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind("Id,Name,Price,Description,StateId")] Food food)
        {
            if (id != food.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(food);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StateId"] = new SelectList(_context.Set<State>(), "Id", "Name", food.StateId);
            return View(food);
        }

        // GET: Foods/Delete/5
        public ActionResult Delete(int id)
        {
            Food? food = _context.Foods!.Where(f => f.Id == id).Include(f => f.State).FirstOrDefault();

            if (food == null)
            {
                return NotFound();
            }
            return View(food);
        }

        // POST: Foods/Delete/5
        [HttpPost, ActionName("Delete")] //method DeleteConfirmed olmasına rağmen post ile gelen delete komutu buraya gelsin diye ActionName atadık.
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)  //aynı signature sahip iki method olamaz yani delete(int id) tekrar atanamaz. bu nedenler DeleteConfirmed olarak isimlendirdik.
        {
            Food food = _context.Foods!.Find(id)!;

            // Hard Delete version
            //_context.Foods.Remove(food);

            //Soft Delete version
            food.StateId = 0;
            _context.Foods.Update(food);

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}

