using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web.Security;
using savnmore;
using savnmore.Services;
namespace savnmore.Models
{
    /// <summary>
    /// The entities in this project
    /// </summary>
    public partial class savnmoreEntities : DbContext
    {
        /// <summary>
        /// Constructor, uses the connection string
        /// </summary>
        public savnmoreEntities() : base("ApplicationServices") { }
        /// <summary>
        /// The users in the database
        /// </summary>
        public DbSet<User> Users { get; set; }
        /// <summary>
        /// The roles in the database
        /// </summary>
        public DbSet<Role> Roles { get; set; }
        /// <summary>
        /// Roles and their users
        /// </summary>
        public DbSet<UserRole> UserRoles { get; set; }
        /// <summary>
        /// Password reset requests
        /// </summary>
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }

        //Add your models here

        public DbSet<Chain> Chains { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<WeeklySale> WeeklySales { get; set; }
        public DbSet<Item> SaleItems { get; set; }
        public DbSet<ZipCodeEntry> ZipCodes { get; set; }
        //public DbSet<Category> Categories { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chain>().HasMany(u => u.Stores);
            modelBuilder.Entity<Store>().HasMany(u => u.WeeklySales);
            modelBuilder.Entity<WeeklySale>().HasMany(u => u.SaleItems);
            //modelBuilder.Entity<Item>().HasOptional(u => u.Category);
        } 
    }
    /// <summary>
    /// Class that represents a password request for a user
    /// </summary>
    public partial class PasswordResetRequest
    {
        /// <summary>
        /// The identity of the record
        /// </summary>
        [Key]
        public int RequestId { get; set; }
        /// <summary>
        /// The users id for this request
        /// </summary>
        [Required]
        public int UserId { get; set; }
        /// <summary>
        /// Unique has for this request
        /// </summary>
        [Required]
        public string HashId { get; set; }
        /// <summary>
        /// When the resquest expires
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpiresOn { get; set; }


    }

    public partial class ResetPasswordModel
    {
        /// <summary>
        /// The email address for the user
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        [RegularExpression(Constants.EmailRegex, ErrorMessage = ErrorConstants.NotAValidEmail)]
        public string Email { get; set; }


    }

    public partial class Role : IReadOnly
    {
        /// <summary>
        /// The identity of the record
        /// </summary>
        [Key]
        public int RoleId { get; set; }
        /// <summary>
        /// The role name
        /// </summary>
        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// The description of the role
        /// </summary>
        [Display(Name = "Description")]
        [Required]
        public string Description { get; set; }
        /// <summary>
        /// The role photo file name
        /// </summary>
        [Display(Name = "Photo")]
        public string Photo { get; set; }

        [Display(Name = "Member Count")]
        public int? MemberCount { get; set; }
        [Required]
        public string AppName { get; set; }
        /// <summary>
        /// Readonly roles cannot be renamed or deleted. Readonly members cannot be removed.
        /// </summary>
        [Required]
        [Display(Name = "Is Read Only")]
        public bool IsReadOnly { get; set; }
    }
    public partial class UserRole
    {
        /// <summary>
        /// The identity of the record
        /// </summary>
        [Key]
        public int UserRoleId { get; set; }
        /// <summary>
        /// The role id
        /// </summary>
        [Required]
        public int RoleId { get; set; }
        /// <summary>
        /// The user id
        /// </summary>
        [Required]
        public int UserId { get; set; }

    }
    /// <summary>
    /// Class that represents a user
    /// </summary>
    public partial class User : IReadOnly
    {
        /// <summary>
        /// The identity of the record
        /// </summary>
        [Key]
        public int UserId { get; set; }
        /// <summary>
        /// User's first name
        /// </summary>
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        /// <summary>
        /// Last name
        /// </summary>
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        /// <summary>
        /// Username, required and must be unique
        /// </summary>
        [Display(Name = "Username")]
        [RegularExpression(Constants.ValidEntries, ErrorMessage = ErrorConstants.EntryInvalid)]
        public string UserName { get; set; }

        /// <summary>
        /// Email address of the user, required and must be unique
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        [RegularExpression(Constants.EmailRegex, ErrorMessage = ErrorConstants.NotAValidEmail)]
        public string Email { get; set; }
        /// <summary>
        /// The users password, must be of minimum length set
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = Constants.MinimuPasswordLength)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        /// <summary>
        /// The users photo filename
        /// </summary>
        [Display(Name = "Photo")]
        public string Photo { get; set; }
        /// <summary>
        /// If the user is online or not
        /// </summary>
        [Display(Name = "Is Online")]
        public bool IsOnline { get; set; }

        /// <summary>
        /// The last time the user logged on
        /// </summary>
        [Display(Name = "Last Logon")]
        [DataType(DataType.DateTime)]
        public DateTime? LastLogon { get; set; }
        /// <summary>
        /// Returns the users first and last name, and thier photo.
        /// </summary>
        /// <returns></returns>
        public string Tooltip()
        {
            string photo = string.Empty;
            if (!string.IsNullOrEmpty(Photo))
                photo = " (" + Photo + ")";
            return "[" + UserName + "] " + FirstName + " " + LastName + photo;
        }
        [Required]
        public string AppName { get; set; }

        /// <summary>
        /// Readonly users cannot be renamed or deleted. They, themselves, can edit only certain properties.
        /// </summary>
        [Required]
        [Display(Name = "Is Read Only")]
        public bool IsReadOnly { get; set; }
    }
    /// <summary>
    /// Drops and recreates the database with new entities
    /// </summary>
    public class savnmoreInitializer : DropCreateDatabaseIfModelChanges<savnmoreEntities>
    {
        private readonly string _appName;

        public savnmoreInitializer(string applicationName)
        {
            _appName = applicationName;
        }

        /// <summary>
        /// Seeds the data base with table values
        /// </summary>
        /// <param name="context"></param>
        protected override void Seed(savnmoreEntities context)
        {
            //add the admin
            var admin = UserService.CreateAdmin(_appName);
            context.Users.Add(admin);
            context.SaveChanges();

            //Add the Administrators Role
            var adminRol = EfRoleProvider.CreateAdminRole(_appName);
            var userRole = EfRoleProvider.CreateUsersRole(_appName);
            context.Roles.Add(adminRol);
            context.SaveChanges();
            context.Roles.Add(userRole);
            context.SaveChanges();
            //only one admin
            var adminuser = context.Users.Single(p => p.UserName == Constants.Admin && p.AppName == _appName);
            var usersRole = context.Roles.Single(t => t.Name == Constants.UsersRole && t.AppName == _appName);
            var adminRole = context.Roles.Single(p => p.Name == Constants.AdministratorsRole && p.AppName == _appName);
            context.UserRoles.Add(new UserRole
            {
                RoleId = adminRole.RoleId,
                UserId = adminuser.UserId,
            });
            Logger.WriteLine(MessageType.Information, Constants.Adding + adminuser.UserName + Constants.To + adminRole.Name);
            context.SaveChanges();
            context.UserRoles.Add(new UserRole
            {
                RoleId = usersRole.RoleId,
                UserId = adminuser.UserId,
            });
            Logger.WriteLine(MessageType.Information, Constants.Adding + adminuser.UserName + Constants.To + usersRole.Name);
            context.SaveChanges();
            if (!Convert.ToBoolean(ConfigurationManager.AppSettings[Constants.CreateSampleRolesAndUsersKey])) //add sample roles and users if sepcified
            {
                Logger.WriteLine(MessageType.Information, "Sample users and roles not requested.");
                base.Seed(context);
                return;
            }
            Logger.WriteLine(MessageType.Information, "Sample users and roles creation requested.");
            //now add the roles
            var sampleRoles = new List<Role>
                                             {                                                
                                                 new Role
                                                     {
                                                          
                                                         Name = Constants.Level1Role,
                                                         Description = Constants.Level1RoleDescription,
                                                         AppName = _appName,
                                                          IsReadOnly = false
                                                     },
                                                 new Role
                                                     {
                                                          
                                                         Name = Constants.Level2Role,
                                                         Description = Constants.Level2RoleDescription,
                                                         AppName = _appName,
                                                          IsReadOnly = false
                                                     },
                                                 new Role
                                                     {
                                                         
                                                         Name = Constants.PremiumRole,
                                                         Description = Constants.PremiumRoleRoleDescription,
                                                         AppName = _appName,
                                                          IsReadOnly = false
                                                     }
                                             };
            foreach (var sampleRole in sampleRoles)
            {
                context.Roles.Add(sampleRole);
                Logger.WriteLine(MessageType.Information, Constants.Adding + sampleRole.Name);

            }
            context.SaveChanges();

            //add sample users
            int sampleUserCount = Convert.ToInt32(ConfigurationManager.AppSettings[Constants.NumberOfSampleUsersKey]);
            if (sampleUserCount < 1)
            {
                sampleUserCount = 1;
            }
            for (var i = 1; i < sampleUserCount; i++)
            {
                var newUser = UserService.CreateUser(_appName,
                                              i.ToString(CultureInfo.InvariantCulture) + Constants.SampleUserName,
                                              i.ToString(CultureInfo.InvariantCulture),
                                              Constants.SampleUserName,
                                                FormsAuthentication.HashPasswordForStoringInConfigFile(Constants.SampleUserPassword, Constants.HashMethod),
                                                Constants.SampleUserName + i.ToString(CultureInfo.InvariantCulture) + ConfigurationManager.AppSettings[Constants.DomainEmailSuffixKey],
                                               false);
                context.Users.Add(newUser);

                Logger.WriteLine(MessageType.Information, Constants.Adding + newUser.UserName);
            }
            context.SaveChanges();
            //everyone is a user

            foreach (User usr1 in context.Users)
            {
                if (usr1.UserName == Constants.Admin) { continue; } //admin already added
                var usrRole = new UserRole
                {
                    RoleId = usersRole.RoleId,
                    UserId = usr1.UserId
                };
                context.UserRoles.Add(usrRole);
                Logger.WriteLine(MessageType.Information, Constants.Adding + usr1.UserName + Constants.To + usersRole.Name);
            }
            context.SaveChanges();
            //add random users to roles
            int j = 1;
            foreach (var r in sampleRoles)
            {
                int t = j;
                var randomName = t.ToString(CultureInfo.InvariantCulture);
                var users = from p in context.Users where p.UserName.Contains(randomName) && p.AppName == _appName select p;
                foreach (var user in users)
                {
                    var usrRole = new UserRole
                    {
                        RoleId = r.RoleId,
                        UserId = user.UserId
                    };
                    context.UserRoles.Add(usrRole);
                    Logger.WriteLine(MessageType.Information, Constants.Adding + user.UserName + Constants.To + r.Name);
                }
                j++;
            }
            context.SaveChanges();
            //Add Categories
            //Category bkry= new Category();
            //bkry.Name = "Bakery";
            //context.Categories.Add(bkry);
            //context.SaveChanges();
             
            //CategoryService catsvc = new CategoryService();
            //catsvc.Add("Baby Store");
            //catsvc.Add("Bakery");
            //catsvc.Add("Beverages");
            //catsvc.Add("Breakfast");
            //catsvc.Add("Canned & Packaged");
            //catsvc.Add("Cleaning Products");
            //catsvc.Add("Condiment & Sauces");
            //catsvc.Add("Dairy");
            //catsvc.Add("Deli");
            //catsvc.Add("Floral");
            //catsvc.Add("Frozen");
            //catsvc.Add("General");
            //catsvc.Add("Health & Beauty");
            //catsvc.Add("Ingredients");
            //catsvc.Add("International");
            //catsvc.Add("Meat & Seafood");
            //catsvc.Add("Paper & Plastics");
            //catsvc.Add("Pasta, Sauces, Grain");
            //catsvc.Add("Pet Shop");
            //catsvc.Add("Produce");
            //catsvc.Add("Snacks");
            //create a new address
           
            try
            {
                #region old
            //Address sa = new Address();
            //sa.Address1 = "611 Bordentown Avenue";
            //sa.City = "South Amboy";
            //sa.State = "New Jersey";
            //sa.Zip = "08879";
            //context.Addresses.Add(sa);
            //context.SaveChanges();
            
                //var breadCat = catsvc.Get("Bakery");

            //add a sale item of type bakery to the weekly sale
            //Item breadSale = new Item();
           
            //breadSale.Name = "Wonder White Bread";
            //breadSale.Description = "Family size";
            //breadSale.Price = "$1.00";

            //context.Categories.Attach(breadCat);
            //breadSale.Category = breadCat;

            //context.SaleItems.Add(breadSale);
            //context.SaveChanges();
           
            //context.Entry(breadSale).State = EntityState.Modified;
            //context.SaveChanges();
            //create a weekly special for a store
            //WeeklySale ws = new WeeklySale();
            //ws.StartsOn = DateTime.Now.AddDays(-21);
            //ws.EndsOn = ws.StartsOn.AddDays(7);
            //ws.SaleItems = new List<Item>();
            //ws.SaleItems.Add(breadSale);
            //context.WeeklySales.Add(ws);
            //context.SaveChanges();

            //give foodtwon one store
            //Store foodtownSa = new Store();
            //foodtownSa.Address = sa;//add an address
            //foodtownSa.Name = "Foodtown of South Amboy";
            //foodtownSa.Url = "http://foodtown.mywebgrocer.com/CircularMain.aspx?st=621E304";
            //    foodtownSa.WeeklySales = new List<WeeklySale>();
                
            //    foodtownSa.WeeklySales.Add(ws); //add a weekly sale
                
            //    context.Stores.Add(foodtownSa);
            //    context.SaveChanges();

            //    //create a chain with one store 
            //    Chain foodtwn = new Chain();
            //    foodtwn.Name = "FoodTown";
            //    foodtwn.Url = "http://www.foodtown.com/";
            //    foodtwn.Stores =new List<Store>();
            //    foodtwn.Stores.Add(foodtownSa);
            //    context.Chains.Add(foodtwn);
                //    context.SaveChanges();
                #endregion
                Chain fdtwn = new Chain();
                fdtwn.Name = "FoodTown";
                fdtwn.Url = "http://www.foodtown.com/";
                fdtwn.Stores = new List<Store>();

                //create each store
                Store s1 = new Store();
                s1.Name = "Foodtown of South Amboy";
                s1.Url = "http://foodtown.mywebgrocer.com/CircularMain.aspx?st=621E304";
                s1.WeeklySales = new List<WeeklySale>();

                Address a = new Address();
                a.Address1 = "611 Bordentown Avenue";
                a.City = "South Amboy";
                a.State = "NJ";
                a.Zip = "08879";
                s1.Address = a;

                Store s2 = new Store();

                s2.Name = "Foodtown of Atlantic Highlands";
                s2.Url = "http://foodtown.mywebgrocer.com/Circular.aspx?st=CA8D846";
                s2.WeeklySales = new List<WeeklySale>();

                Address a2 = new Address();
                a2.Address1 = "3 Bayshore Plaza";
                a2.City = "Atlantic Highlands";
                a2.State = "NJ";
                a2.Zip = "07716";
                s2.Address = a2;

                Store s3 = new Store();

                s3.Name = "Foodtown of Caldwell";
                s3.Url = "http://foodtown.mywebgrocer.com/CircularMain.aspx?st=DBAA565";
                s3.WeeklySales = new List<WeeklySale>();

                Address a3 = new Address();
                a3.Address1 = "370 Bloomfield";
                a3.City = "Caldwell";
                a3.State = "NJ";
                a3.Zip = "07006";
                s3.Address = a3;

                fdtwn.Stores.Add(s1);
                fdtwn.Stores.Add(s2);
                fdtwn.Stores.Add(s3);

                context.Chains.Add(fdtwn);
                context.SaveChanges(); 

                //create shoprite
                Chain shprite = new Chain();
                shprite.Name = "ShopRite";
                shprite.Url = "http://www.shoprite.com/";
                shprite.Stores = new List<Store>();

                //create each store
                Store sh1 = new Store();
                sh1.Name = "ShopRite of Old Bridge";
                sh1.Url = "http://shoprite.mywebgrocer.com/CircularMain.aspx?s=215744292&g=c3eacc48-bd72-449b-8a2e-4443bd8db9ed&uc=8EE7C1&st=9913726";
                sh1.WeeklySales = new List<WeeklySale>();

                Address adr = new Address();
                adr.Address1 = "2239 US Route 9";
                adr.City = "Old Bridge";
                adr.State = "NJ";
                adr.Zip = "08857";
                sh1.Address = adr;
                shprite.Stores.Add(sh1);

                //create each store
                Store sh2 = new Store();
                sh2.Name = "ShopRite of Edison";
                sh2.Url = "http://shoprite.mywebgrocer.com/CircularMain.aspx?s=215744292&g=c3eacc48-bd72-449b-8a2e-4443bd8db9ed&uc=8EE7C1&st=027F776";
                sh2.WeeklySales = new List<WeeklySale>();

                Address adr2 = new Address();
                adr2.Address1 = "Rt 1 & Old Post Rd";
                adr2.City = "Edison";
                adr2.State = "NJ";
                adr2.Zip = "08817";
                sh2.Address = adr2;
                shprite.Stores.Add(sh2);

                //create each store
                Store sh3 = new Store();
                sh3.Name = "ShopRite of Perth Amboy";
                sh3.Url = "http://shoprite.mywebgrocer.com/CircularMain.aspx?s=215744292&g=c3eacc48-bd72-449b-8a2e-4443bd8db9ed&uc=8EE7C1&st=6EBE716";
                sh3.WeeklySales = new List<WeeklySale>();

                Address adr3= new Address();
                adr3.Address1 = "365 Convery Blvd";
                adr3.City = "Perth Amboy";
                adr3.State = "NJ";
                adr3.Zip = "08861";
                sh3.Address = adr3;
                shprite.Stores.Add(sh3);

                //create each store
                Store sh4 = new Store();
                sh4.Name = "ShopRite of E. Brunswick";
                sh4.Url = "http://shoprite.mywebgrocer.com/CircularMain.aspx?s=215744292&g=c3eacc48-bd72-449b-8a2e-4443bd8db9ed&uc=8EE7C1&st=3D65777";
                sh4.WeeklySales = new List<WeeklySale>();

                Address adr4 = new Address();
                adr4.Address1 = "14-22 W. Prospect Street";
                adr4.City = "East Brunswick";
                adr4.State = "NJ";
                adr4.Zip = "08816";
                sh4.Address = adr4;
                shprite.Stores.Add(sh4);

                context.Chains.Add(shprite);
                context.SaveChanges(); 
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            } 
            base.Seed(context);
        }




    }

}
