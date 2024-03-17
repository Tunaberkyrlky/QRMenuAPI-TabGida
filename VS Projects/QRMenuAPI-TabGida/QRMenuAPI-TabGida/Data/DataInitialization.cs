using Microsoft.EntityFrameworkCore;
using QRMenuAPI_TabGida.Models;
using System.Data.SqlTypes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace QRMenuAPI_TabGida.Data
{
    public class DataInitialization
    {
        private readonly ApplicationContext? _context;
        public DataInitialization(ApplicationContext context)
        {
            _context = context;

            State state;
            Company company;
            if (_context != null)
            {
                _context.Database.Migrate();
                if (_context.States.Count() == 0)
                {
                    state = new State() { Id = 0, Name = "Deleted" };
                    _context.States.Add(state);
                    state = new State() { Id = 1, Name = "Active" };
                    _context.States.Add(state);
                    state = new State() { Id = 2, Name = "Passive" };
                    _context.States.Add(state);
                }
                _context.SaveChanges();
                if (_context.Companies.Count() == 0)
                {
                    company = new Company()
                    {
                        Name = "TABGIDA",
                        Phone = "EditData",
                        EMail = "EditEmail@mail.com",
                        TaxNumber = "EditData..",
                        PostalCode = "EditD",
                        AddressDetails = "EditData",
                        RegisterationDate = DateTime.Now,
                        StateId = 1
                    };
                    _context.Companies.Add(company);
                }
                _context.SaveChanges();
            }
        }
    }
}
