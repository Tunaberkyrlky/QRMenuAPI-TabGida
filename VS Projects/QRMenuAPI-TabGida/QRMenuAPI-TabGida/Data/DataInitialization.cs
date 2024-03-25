using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI_TabGida.Models;
using System.Data;
using System.Security.Claims;

namespace QRMenuAPI_TabGida.Data
{
    public class DataInitialization
    {
        private readonly ApplicationContext? _context;
        private readonly RoleManager<IdentityRole>? _roleManager;
        private readonly SignInManager<ApplicationUser>? _signInManager;
        public DataInitialization(ApplicationContext context, RoleManager<IdentityRole>? roleManager, UserManager<ApplicationUser>? userManager, SignInManager<ApplicationUser>? signInManager)
        {
            _context = context;
            _roleManager = roleManager;
            _signInManager = signInManager;

            State state;
            Company company = null;
            IdentityRole identityRole;
            ApplicationUser applicationUser;
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

                if (_roleManager != null)
                {
                    if (roleManager.Roles.Count() == 0)
                    {
                        identityRole = new IdentityRole("SystemAdministrator");
                        _roleManager.CreateAsync(identityRole).Wait();
                        identityRole = new IdentityRole("Administrator");
                        _roleManager.CreateAsync(identityRole).Wait();
                        identityRole = new IdentityRole("CompanyAdministrator");
                        _roleManager.CreateAsync(identityRole).Wait();
                        identityRole = new IdentityRole("RestaurantAdministrator");
                        _roleManager.CreateAsync(identityRole).Wait();
                    }
                }
                if (userManager != null)
                {
                    if (userManager.Users.Count() == 0)
                    {
                        if (company != null)
                        {
                            applicationUser = new ApplicationUser();
                            applicationUser.UserName = company.Name + "Admin";
                            applicationUser.CompanyId = company.Id;
                            applicationUser.Name = company.Name+ "Admin";
                            applicationUser.Email = "abc@def.com";
                            applicationUser.PhoneNumber = "1112223344";
                            applicationUser.RegisterationDate = DateTime.Today;
                            applicationUser.StateId = 1;
                            _signInManager.UserManager.CreateAsync(applicationUser, "Admin123!").Wait();
                            _signInManager.UserManager.AddToRoleAsync(applicationUser, "Administrator").Wait();

                            Claim userClaim = new Claim("UserId", applicationUser.Id.ToString());
                            _signInManager.UserManager.AddClaimAsync(applicationUser, userClaim).Wait();
                            
                            Claim CompanyClaim = new Claim("CompanyId", applicationUser.CompanyId.ToString());
                            _signInManager.UserManager.AddClaimAsync(applicationUser, CompanyClaim).Wait();

                            //applicationUser = new ApplicationUser();
                            //applicationUser.UserName = "SA";
                            //applicationUser.CompanyId = company.Id;
                            //applicationUser.Name = "SystemAdministrator";
                            //applicationUser.Email = "abc@def.com";
                            //applicationUser.PhoneNumber = "1112223344";
                            //applicationUser.RegisterationDate = DateTime.Today;
                            //applicationUser.StateId = 1;
                            //userManager.CreateAsync(applicationUser, "Admin123!").Wait();
                            //userManager.AddToRoleAsync(applicationUser, "SystemAdministrator").Wait();
                        }
                    }
                }
            }
        }
    }
}
