using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework; // For IdentityRole
using Microsoft.AspNet.Identity.Owin; // For HttpContext.GetOwinContext().Get()
using Microsoft.Owin.Security;
using StudentPortalMVC.Models;
using System.Data.Entity.Infrastructure; // For DbUpdateException
using System.Data.Entity.Validation; // For DbEntityValidationException

namespace StudentPortalMVC.Controllers
{
    [Authorize] // This attribute should be on specific actions or the controller if most actions require auth
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager; // Declare the RoleManager field

        // Default constructor - essential if no custom DI container is set up for AccountController
        public AccountController()
        {
        }

        // Constructor for dependency injection (if you configure one)
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager; // Initialize RoleManager from the passed parameter
        }

        // UserManager property with lazy initialization from OwinContext
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // SignInManager property with lazy initialization from OwinContext
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        // RoleManager property with lazy initialization from OwinContext
        public ApplicationRoleManager RoleManager
        {
            get
            {
                // This is the critical line: try to get it from OwinContext if not set by constructor
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        // Action for Admin Login - no functional changes needed here from your provided code
        [AllowAnonymous]
        public ActionResult AdminLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdminLogin(LoginViewModel model, string adminCode, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string expectedAdminCode = ConfigurationManager.AppSettings["AdminSpecialLoginCode"];

            if (string.IsNullOrWhiteSpace(adminCode) || adminCode != expectedAdminCode)
            {
                ModelState.AddModelError("", "Invalid Admin Code.");
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    var user = await UserManager.FindByEmailAsync(model.Email);

                    if (user != null)
                    {
                        if (await UserManager.IsInRoleAsync(user.Id, "Admin"))
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        else
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            ModelState.AddModelError("", "This login method is only for administrators. Please use the regular login.");
                            return View(model);
                        }
                    }
                    else
                    {
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                    }

                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var user = await UserManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        // ===== START: ROLE-BASED REDIRECTION LOGIC (This will now handle redirection after manual login) =====
                        try
                        {
                            // Check if the user is an Admin
                            if (await UserManager.IsInRoleAsync(user.Id, "Admin"))
                            {
                                return RedirectToAction("Dashboard", "Admin"); // Redirect to Admin Dashboard
                            }
                            // Check if the user is a Student (or any other non-admin role)
                            else if (await UserManager.IsInRoleAsync(user.Id, "Student"))
                            {
                                // CORRECTED: Redirect to Home/Index as Student/Dashboard does not exist
                                return RedirectToAction("Index", "Home"); // Redirect to generic authenticated homepage
                            }
                            else
                            {
                                // Fallback if no specific role or unexpected role
                                return RedirectToAction("Index", "Home"); // Generic authenticated homepage
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error during role check on login for {user.Email}: {ex.Message}");
                            return RedirectToAction("Index", "Home"); // Fallback to generic homepage
                        }
                        // ===== END: ROLE-BASED REDIRECTION LOGIC =====
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var createResult = await UserManager.CreateAsync(user, model.Password);

                if (createResult.Succeeded)
                {
                    // Get the user object again after creation to ensure it's fully loaded for role checks
                    var registeredUser = await UserManager.FindByIdAsync(user.Id);

                    // ===== START: AUTOMATED ROLE ASSIGNMENT LOGIC =====
                    ApplicationRoleManager roleManager = null;
                    try
                    {
                        roleManager = HttpContext.GetOwinContext().Get<ApplicationRoleManager>();

                        if (roleManager == null)
                        {
                            System.Diagnostics.Debug.WriteLine("CRITICAL WARNING: ApplicationRoleManager is NULL during registration role assignment.");
                            ModelState.AddModelError("", "Role management service could not be fully initialized. User registered, but role not assigned automatically.");
                        }
                        else
                        {
                            string roleName = (model.Email == "inesh@gmail.com") ? "Admin" : "Student";

                            // Ensure the target role exists
                            if (!await roleManager.RoleExistsAsync(roleName))
                            {
                                var roleCreateResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                                if (!roleCreateResult.Succeeded)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error creating '{roleName}' role: {string.Join("; ", roleCreateResult.Errors)}");
                                    ModelState.AddModelError("", $"Failed to create '{roleName}' role. User registered, but role not assigned automatically.");
                                }
                            }

                            // Add the newly registered user to the determined role
                            var addToRoleResult = await UserManager.AddToRoleAsync(registeredUser.Id, roleName);
                            if (!addToRoleResult.Succeeded)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error adding user {registeredUser.Email} to '{roleName}' role: {string.Join("; ", addToRoleResult.Errors)}");
                                ModelState.AddModelError("", $"Failed to assign '{roleName}' role to user. User registered, but role not assigned automatically.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Exception during role assignment: {ex.Message}");
                        ModelState.AddModelError("", "An unexpected error occurred during role assignment. User registered, but role not assigned automatically.");
                    }
                    // ===== END: AUTOMATED ROLE ASSIGNMENT LOGIC =====


                    // ===== START: PROFILE CREATION AFTER SUCCESSFUL REGISTRATION =====
                    var dbContextForProfile = new ApplicationDbContext();
                    try
                    {
                        var newProfile = new StudentPortalMVC.Models.Profile
                        {
                            UserId = registeredUser.Id,
                            Email = registeredUser.Email,
                            FirstName = "",
                            LastName = "",
                            DateOfBirth = new DateTime(1900, 1, 1),
                            Phone = "",
                            Department = "",
                            Address = "",
                            ProfileImagePath = "~/Content/Images/default_profile.png"
                        };
                        dbContextForProfile.Profiles.Add(newProfile);
                        await dbContextForProfile.SaveChangesAsync();
                        System.Diagnostics.Debug.WriteLine($"Profile created for user {registeredUser.Email}. ProfileId: {newProfile.ProfileId}");
                    }
                    catch (DbUpdateException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"DbUpdateException creating profile for user {registeredUser.Email}: {ex.InnerException?.Message}");
                        ModelState.AddModelError("", "Error creating user profile (database issue). User registered, but profile not created.");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        foreach (var validationErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                            }
                        }
                        ModelState.AddModelError("", "Error creating user profile (validation issue). User registered, but profile not created.");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"General error creating profile for user {registeredUser.Email}: {ex.Message}");
                        ModelState.AddModelError("", "An unexpected error occurred during profile creation. User registered, but profile not created.");
                    }
                    finally
                    {
                        dbContextForProfile.Dispose();
                    }
                    // ===== END: PROFILE CREATION =====

                    // ===== CRITICAL CHANGE: REDIRECT TO LOGIN PAGE AFTER REGISTRATION =====
                    TempData["SuccessMessage"] = "Registration successful! Please log in with your new account.";
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    // User creation failed
                    AddErrors(createResult);
                }
            }

            // If ModelState is not valid or any operation failed, return the view with errors
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            // Redirect to the Home page or Login page after logging out.
            return RedirectToAction("Index", "Home"); // Or "Login", "Account" if you prefer to go directly to login
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
                // Ensure RoleManager is also disposed
                if (_roleManager != null)
                {
                    _roleManager.Dispose();
                    _roleManager = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Helpers
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}






















































//using System;
//using System.Configuration;
//using System.Globalization;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.EntityFramework; // For IdentityRole
//using Microsoft.AspNet.Identity.Owin; // For HttpContext.GetOwinContext().Get()
//using Microsoft.Owin.Security;
//using StudentPortalMVC.Models;
//using System.Data.Entity.Infrastructure; // For DbUpdateException
//using System.Data.Entity.Validation; // For DbEntityValidationException

//namespace StudentPortalMVC.Controllers
//{
//    [Authorize] // This attribute should be on specific actions or the controller if most actions require auth
//    public class AccountController : Controller
//    {
//        private ApplicationSignInManager _signInManager;
//        private ApplicationUserManager _userManager;
//        private ApplicationRoleManager _roleManager; // Declare the RoleManager field

//        // Default constructor - essential if no custom DI container is set up for AccountController
//        public AccountController()
//        {
//        }

//        // Constructor for dependency injection (if you configure one)
//        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
//        {
//            UserManager = userManager;
//            SignInManager = signInManager;
//            RoleManager = roleManager; // Initialize RoleManager from the passed parameter
//        }

//        // UserManager property with lazy initialization from OwinContext
//        public ApplicationUserManager UserManager
//        {
//            get
//            {
//                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
//            }
//            private set
//            {
//                _userManager = value;
//            }
//        }

//        // SignInManager property with lazy initialization from OwinContext
//        public ApplicationSignInManager SignInManager
//        {
//            get
//            {
//                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
//            }
//            private set
//            {
//                _signInManager = value;
//            }
//        }

//        // RoleManager property with lazy initialization from OwinContext
//        public ApplicationRoleManager RoleManager
//        {
//            get
//            {
//                // This is the critical line: try to get it from OwinContext if not set by constructor
//                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
//            }
//            private set
//            {
//                _roleManager = value;
//            }
//        }

//        // Action for Admin Login - no functional changes needed here from your provided code
//        [AllowAnonymous]
//        public ActionResult AdminLogin(string returnUrl)
//        {
//            ViewBag.ReturnUrl = returnUrl;
//            return View();
//        }

//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> AdminLogin(LoginViewModel model, string adminCode, string returnUrl)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }

//            string expectedAdminCode = ConfigurationManager.AppSettings["AdminSpecialLoginCode"];

//            if (string.IsNullOrWhiteSpace(adminCode) || adminCode != expectedAdminCode)
//            {
//                ModelState.AddModelError("", "Invalid Admin Code.");
//                return View(model);
//            }

//            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

//            switch (result)
//            {
//                case SignInStatus.Success:
//                    var user = await UserManager.FindByEmailAsync(model.Email);

//                    if (user != null)
//                    {
//                        // This AdminLogin still relies on RoleManager to check "Admin" role.
//                        // Now that ConfigureAuth is running, this should work.
//                        if (await UserManager.IsInRoleAsync(user.Id, "Admin"))
//                        {
//                            return RedirectToAction("Dashboard", "Admin");
//                        }
//                        else
//                        {
//                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
//                            ModelState.AddModelError("", "This login method is only for administrators. Please use the regular login.");
//                            return View(model);
//                        }
//                    }
//                    else
//                    {
//                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
//                        ModelState.AddModelError("", "Invalid login attempt.");
//                        return View(model);
//                    }

//                case SignInStatus.LockedOut:
//                    return View("Lockout");
//                case SignInStatus.RequiresVerification:
//                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
//                case SignInStatus.Failure:
//                default:
//                    ModelState.AddModelError("", "Invalid login attempt.");
//                    return View(model);
//            }
//        }

//        // GET: /Account/Login
//        [AllowAnonymous]
//        public ActionResult Login(string returnUrl)
//        {
//            ViewBag.ReturnUrl = returnUrl;
//            return View();
//        }

//        // POST: /Account/Login
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }

//            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
//            switch (result)
//            {
//                case SignInStatus.Success:
//                    var user = await UserManager.FindByEmailAsync(model.Email);
//                    if (user != null)
//                    {
//                        // ===== START: ROLE-BASED REDIRECTION LOGIC (RE-ENABLED) =====
//                        try
//                        {
//                            // Check if the user is an Admin
//                            if (await UserManager.IsInRoleAsync(user.Id, "Admin"))
//                            {
//                                return RedirectToAction("Dashboard", "Admin"); // Redirect to Admin Dashboard
//                            }
//                            // Check if the user is a Student (or any other non-admin role)
//                            else if (await UserManager.IsInRoleAsync(user.Id, "Student"))
//                            {
//                                return RedirectToAction("Index", "Home"); // Redirect to Student Portal
//                            }
//                            else
//                            {
//                                // If user has no specific role, or an unexpected role, redirect to a default authenticated page
//                                // This might happen for older users or if role assignment failed.
//                                return RedirectToAction("Index", "Home"); // Generic authenticated homepage
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            // Log this error, as it indicates RoleManager might still be an issue during login
//                            System.Diagnostics.Debug.WriteLine($"Error during role check on login for {user.Email}: {ex.Message}");
//                            // Fallback: Redirect to a default page if role check fails
//                            return RedirectToAction("Index", "Home"); // Fallback to generic homepage
//                        }
//                        // ===== END: ROLE-BASED REDIRECTION LOGIC =====
//                    }
//                    else
//                    {
//                        // This case should ideally not be hit if SignInStatus.Success, but defensive
//                        ModelState.AddModelError("", "Invalid login attempt.");
//                        return View(model);
//                    }
//                case SignInStatus.LockedOut:
//                    return View("Lockout");
//                case SignInStatus.RequiresVerification:
//                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
//                case SignInStatus.Failure:
//                default:
//                    ModelState.AddModelError("", "Invalid login attempt.");
//                    return View(model);
//            }
//        }

//        // GET: /Account/Register
//        [AllowAnonymous]
//        public ActionResult Register()
//        {
//            return View();
//        }

//        // POST: /Account/Register
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> Register(RegisterViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
//                var createResult = await UserManager.CreateAsync(user, model.Password);

//                if (createResult.Succeeded)
//                {
//                    // Sign in the user immediately after successful registration
//                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

//                    // ===== START: AUTOMATED ROLE ASSIGNMENT LOGIC (RE-ENABLED) =====
//                    ApplicationRoleManager roleManager = null;
//                    try
//                    {
//                        roleManager = HttpContext.GetOwinContext().Get<ApplicationRoleManager>();

//                        if (roleManager == null)
//                        {
//                            // This should ideally NOT happen now that ConfigureAuth is running
//                            System.Diagnostics.Debug.WriteLine("CRITICAL WARNING: ApplicationRoleManager is NULL during registration role assignment despite ConfigureAuth running.");
//                            ModelState.AddModelError("", "Role management service could not be fully initialized. User registered, but role not assigned automatically.");
//                        }
//                        else
//                        {
//                            string roleName = (model.Email == "inesh@gmail.com") ? "Admin" : "Student";

//                            // Ensure the target role exists
//                            if (!await roleManager.RoleExistsAsync(roleName))
//                            {
//                                var roleCreateResult = await roleManager.CreateAsync(new IdentityRole(roleName));
//                                if (!roleCreateResult.Succeeded)
//                                {
//                                    System.Diagnostics.Debug.WriteLine($"Error creating '{roleName}' role: {string.Join("; ", roleCreateResult.Errors)}");
//                                    ModelState.AddModelError("", $"Failed to create '{roleName}' role. User registered, but role not assigned automatically.");
//                                }
//                            }

//                            // Add the newly registered user to the determined role
//                            var addToRoleResult = await UserManager.AddToRoleAsync(user.Id, roleName);
//                            if (!addToRoleResult.Succeeded)
//                            {
//                                System.Diagnostics.Debug.WriteLine($"Error adding user {user.Email} to '{roleName}' role: {string.Join("; ", addToRoleResult.Errors)}");
//                                ModelState.AddModelError("", $"Failed to assign '{roleName}' role to user. User registered, but role not assigned automatically.");
//                            }
//                        }
//                    }
//                    catch (Exception ex) // General catch for any unexpected errors during role assignment
//                    {
//                        System.Diagnostics.Debug.WriteLine($"Exception during role assignment: {ex.Message}");
//                        ModelState.AddModelError("", "An unexpected error occurred during role assignment. User registered, but role not assigned automatically.");
//                    }
//                    // ===== END: AUTOMATED ROLE ASSIGNMENT LOGIC =====


//                    // ===== START: PROFILE CREATION AFTER SUCCESSFUL REGISTRATION (RE-ENABLED) =====
//                    var dbContextForProfile = new ApplicationDbContext(); // Use a new DbContext for this operation
//                    try
//                    {
//                        var newProfile = new StudentPortalMVC.Models.Profile
//                        {
//                            UserId = user.Id,
//                            Email = user.Email,
//                            FirstName = "", // Provide defaults as these are required in your Profile model
//                            LastName = "",
//                            DateOfBirth = new DateTime(1900, 1, 1), // Default valid date
//                            Phone = "",
//                            Department = "",
//                            Address = "",
//                            ProfileImagePath = "~/Content/Images/default_profile.png" // Default image path
//                        };
//                        dbContextForProfile.Profiles.Add(newProfile);
//                        await dbContextForProfile.SaveChangesAsync();
//                        System.Diagnostics.Debug.WriteLine($"Profile created for user {user.Email}. ProfileId: {newProfile.ProfileId}");
//                    }
//                    catch (DbUpdateException ex)
//                    {
//                        System.Diagnostics.Debug.WriteLine($"DbUpdateException creating profile for user {user.Email}: {ex.InnerException?.Message}");
//                        ModelState.AddModelError("", "Error creating user profile (database issue). User registered, but profile not created.");
//                    }
//                    catch (DbEntityValidationException ex)
//                    {
//                        foreach (var validationErrors in ex.EntityValidationErrors)
//                        {
//                            foreach (var validationError in validationErrors.ValidationErrors)
//                            {
//                                System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
//                            }
//                        }
//                        ModelState.AddModelError("", "Error creating user profile (validation issue). User registered, but profile not created.");
//                    }
//                    catch (Exception ex)
//                    {
//                        System.Diagnostics.Debug.WriteLine($"General error creating profile for user {user.Email}: {ex.Message}");
//                        ModelState.AddModelError("", "An unexpected error occurred during profile creation. User registered, but profile not created.");
//                    }
//                    finally
//                    {
//                        dbContextForProfile.Dispose(); // Dispose the DbContext
//                    }
//                    // ===== END: PROFILE CREATION =====

//                    // Redirect after registration (user is already signed in)
//                    // This will now use the role-based redirection from the Login action's logic
//                    return RedirectToAction("Index", "Home"); // Or wherever your main portal entry point is
//                }
//                else
//                {
//                    // User creation failed
//                    AddErrors(createResult);
//                }
//            }

//            // If ModelState is not valid or any operation failed, return the view with errors
//            return View(model);
//        }

//        // ... (rest of your controller actions and helpers like Dispose, AddErrors, RedirectToLocal etc. remain the same) ...

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                if (_userManager != null)
//                {
//                    _userManager.Dispose();
//                    _userManager = null;
//                }
//                if (_signInManager != null)
//                {
//                    _signInManager.Dispose();
//                    _signInManager = null;
//                }
//                // Ensure RoleManager is also disposed
//                if (_roleManager != null)
//                {
//                    _roleManager.Dispose();
//                    _roleManager = null;
//                }
//            }
//            base.Dispose(disposing);
//        }

//        #region Helpers
//        private const string XsrfKey = "XsrfId";

//        private IAuthenticationManager AuthenticationManager
//        {
//            get
//            {
//                return HttpContext.GetOwinContext().Authentication;
//            }
//        }

//        private void AddErrors(IdentityResult result)
//        {
//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError("", error);
//            }
//        }

//        private ActionResult RedirectToLocal(string returnUrl)
//        {
//            if (Url.IsLocalUrl(returnUrl))
//            {
//                return Redirect(returnUrl);
//            }
//            return RedirectToAction("Index", "Home");
//        }

//        internal class ChallengeResult : HttpUnauthorizedResult
//        {
//            public ChallengeResult(string provider, string redirectUri)
//                : this(provider, redirectUri, null)
//            {
//            }

//            public ChallengeResult(string provider, string redirectUri, string userId)
//            {
//                LoginProvider = provider;
//                RedirectUri = redirectUri;
//                UserId = userId;
//            }

//            public string LoginProvider { get; set; }
//            public string RedirectUri { get; set; }
//            public string UserId { get; set; }

//            public override void ExecuteResult(ControllerContext context)
//            {
//                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
//                if (UserId != null)
//                {
//                    properties.Dictionary[XsrfKey] = UserId;
//                }
//                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
//            }
//        }
//        #endregion
//    }
//}




























































































//using System;
//using System.Configuration;
//using System.Data.Entity.Infrastructure;
//using System.Data.Entity.Validation;
//using System.Globalization;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.EntityFramework;
//using Microsoft.AspNet.Identity.Owin;
//using Microsoft.Owin.Security;
//using StudentPortalMVC.Models;
//using static StudentPortalMVC.ApplicationSignInManager; // Make sure your ApplicationUser and ApplicationDbContext are here

//namespace StudentPortalMVC.Controllers
//{
//    [Authorize]
//    public class AccountController : Controller
//    {
//        private ApplicationSignInManager _signInManager;
//        private ApplicationUserManager _userManager;
//        private ApplicationRoleManager _roleManager; // Declare the RoleManager field

//        // Default constructor - essential if no custom DI container is set up for AccountController
//        public AccountController()
//        {
//        }

//        // Constructor for dependency injection (if you configure one)
//        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
//        {
//            UserManager = userManager;
//            SignInManager = signInManager;
//            RoleManager = roleManager; // Initialize RoleManager from the passed parameter
//        }

//        // UserManager property with lazy initialization from OwinContext
//        public ApplicationUserManager UserManager
//        {
//            get
//            {
//                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
//            }
//            private set
//            {
//                _userManager = value;
//            }
//        }

//        // SignInManager property with lazy initialization from OwinContext
//        public ApplicationSignInManager SignInManager
//        {
//            get
//            {
//                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
//            }
//            private set
//            {
//                _signInManager = value;
//            }
//        }

//        // RoleManager property with lazy initialization from OwinContext
//        public ApplicationRoleManager RoleManager
//        {
//            get
//            {
//                // This is the critical line: try to get it from OwinContext if not set by constructor
//                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
//            }
//            private set
//            {
//                _roleManager = value;
//            }
//        }

//        // Action for Admin Login - no functional changes needed here from your provided code
//        [AllowAnonymous]
//        public ActionResult AdminLogin(string returnUrl)
//        {
//            ViewBag.ReturnUrl = returnUrl;
//            return View();
//        }

//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> AdminLogin(LoginViewModel model, string adminCode, string returnUrl)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }

//            string expectedAdminCode = ConfigurationManager.AppSettings["AdminSpecialLoginCode"];

//            if (string.IsNullOrWhiteSpace(adminCode) || adminCode != expectedAdminCode)
//            {
//                ModelState.AddModelError("", "Invalid Admin Code.");
//                return View(model);
//            }

//            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

//            switch (result)
//            {
//                case SignInStatus.Success:
//                    var user = await UserManager.FindByEmailAsync(model.Email);

//                    if (user != null)
//                    {
//                        if (await UserManager.IsInRoleAsync(user.Id, "Admin"))
//                        {
//                            return RedirectToAction("Dashboard", "Admin");
//                        }
//                        else
//                        {
//                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
//                            ModelState.AddModelError("", "This login method is only for administrators. Please use the regular login.");
//                            return View(model);
//                        }
//                    }
//                    else
//                    {
//                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
//                        ModelState.AddModelError("", "Invalid login attempt.");
//                        return View(model);
//                    }

//                case SignInStatus.LockedOut:
//                    return View("Lockout");
//                case SignInStatus.RequiresVerification:
//                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
//                case SignInStatus.Failure:
//                default:
//                    ModelState.AddModelError("", "Invalid login attempt.");
//                    return View(model);
//            }
//        }

//        // GET: /Account/Login
//        [AllowAnonymous]
//        public ActionResult Login(string returnUrl)
//        {
//            ViewBag.ReturnUrl = returnUrl;
//            return View();
//        }

//        // POST: /Account/Login
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }

//            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
//            switch (result)
//            {
//                case SignInStatus.Success:
//                    return RedirectToLocal(returnUrl);
//                case SignInStatus.LockedOut:
//                    return View("Lockout");
//                case SignInStatus.RequiresVerification:
//                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
//                case SignInStatus.Failure:
//                default:
//                    ModelState.AddModelError("", "Invalid login attempt.");
//                    return View(model);
//            }
//        }

//        // GET: /Account/Register
//        [AllowAnonymous]
//        public ActionResult Register()
//        {
//            return View();
//        }

//        // POST: /Account/Register
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> Register(RegisterViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
//                var createResult = await UserManager.CreateAsync(user, model.Password);

//                if (createResult.Succeeded)
//                {
//                    // Removed: Role assignment logic
//                    // Removed: Profile creation logic

//                    // Instead of signing in immediately, redirect to Login page
//                    // Optionally, add a TempData message to confirm registration
//                    TempData["SuccessMessage"] = "Registration successful! Please log in with your new account.";
//                    return RedirectToAction("Login", "Account");
//                }
//                else
//                {
//                    // User creation failed
//                    AddErrors(createResult);
//                }
//            }

//            // If ModelState is not valid or any operation failed, return the view with errors
//            return View(model);
//        }

//        // GET: /Account/VerifyCode
//        [AllowAnonymous]
//        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
//        {
//            if (!await SignInManager.HasBeenVerifiedAsync())
//            {
//                return View("Error");
//            }
//            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
//        }

//        // POST: /Account/VerifyCode
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }

//            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
//            switch (result)
//            {
//                case SignInStatus.Success:
//                    return RedirectToLocal(model.ReturnUrl);
//                case SignInStatus.LockedOut:
//                    return View("Lockout");
//                case SignInStatus.Failure:
//                default:
//                    ModelState.AddModelError("", "Invalid code.");
//                    return View(model);
//            }
//        }

//        // GET: /Account/ConfirmEmail
//        [AllowAnonymous]
//        public async Task<ActionResult> ConfirmEmail(string userId, string code)
//        {
//            if (userId == null || code == null)
//            {
//                return View("Error");
//            }
//            var result = await UserManager.ConfirmEmailAsync(userId, code);
//            return View(result.Succeeded ? "ConfirmEmail" : "Error");
//        }

//        // GET: /Account/ForgotPassword
//        [AllowAnonymous]
//        public ActionResult ForgotPassword()
//        {
//            return View();
//        }

//        // POST: /Account/ForgotPassword
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                var user = await UserManager.FindByNameAsync(model.Email);
//                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
//                {
//                    return View("ForgotPasswordConfirmation");
//                }
//            }
//            return View(model);
//        }

//        // GET: /Account/ForgotPasswordConfirmation
//        [AllowAnonymous]
//        public ActionResult ForgotPasswordConfirmation()
//        {
//            return View();
//        }

//        // GET: /Account/ResetPassword
//        [AllowAnonymous]
//        public ActionResult ResetPassword(string code)
//        {
//            return code == null ? View("Error") : View();
//        }

//        // POST: /Account/ResetPassword
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }
//            var user = await UserManager.FindByNameAsync(model.Email);
//            if (user == null)
//            {
//                return RedirectToAction("ResetPasswordConfirmation", "Account");
//            }
//            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
//            if (result.Succeeded)
//            {
//                return RedirectToAction("ResetPasswordConfirmation", "Account");
//            }
//            AddErrors(result);
//            return View();
//        }

//        // GET: /Account/ResetPasswordConfirmation
//        [AllowAnonymous]
//        public ActionResult ResetPasswordConfirmation()
//        {
//            return View();
//        }

//        // POST: /Account/ExternalLogin
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public ActionResult ExternalLogin(string provider, string returnUrl)
//        {
//            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
//        }

//        // GET: /Account/SendCode
//        [AllowAnonymous]
//        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
//        {
//            var userId = await SignInManager.GetVerifiedUserIdAsync();
//            if (userId == null)
//            {
//                return View("Error");
//            }
//            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
//            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
//            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
//        }

//        // POST: /Account/SendCode
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> SendCode(SendCodeViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View();
//            }

//            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
//            {
//                return View("Error");
//            }
//            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
//        }

//        // GET: /Account/ExternalLoginCallback
//        [AllowAnonymous]
//        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
//        {
//            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
//            if (loginInfo == null)
//            {
//                return RedirectToAction("Login");
//            }

//            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
//            switch (result)
//            {
//                case SignInStatus.Success:
//                    return RedirectToLocal(returnUrl);
//                case SignInStatus.LockedOut:
//                    return View("Lockout");
//                case SignInStatus.RequiresVerification:
//                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
//                case SignInStatus.Failure:
//                default:
//                    ViewBag.ReturnUrl = returnUrl;
//                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
//                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
//            }
//        }

//        // POST: /Account/ExternalLoginConfirmation
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
//        {
//            if (User.Identity.IsAuthenticated)
//            {
//                return RedirectToAction("Index", "Manage");
//            }

//            if (ModelState.IsValid)
//            {
//                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
//                if (info == null)
//                {
//                    return View("ExternalLoginFailure");
//                }
//                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
//                var result = await UserManager.CreateAsync(user);
//                if (result.Succeeded)
//                {
//                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
//                    if (result.Succeeded)
//                    {
//                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
//                        return RedirectToLocal(returnUrl);
//                    }
//                }
//                AddErrors(result);
//            }

//            ViewBag.ReturnUrl = returnUrl;
//            return View(model);
//        }

//        // POST: /Account/LogOff
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult LogOff()
//        {
//            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
//            return RedirectToAction("Index", "Home");
//        }

//        // GET: /Account/ExternalLoginFailure
//        [AllowAnonymous]
//        public ActionResult ExternalLoginFailure()
//        {
//            return View();
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                if (_userManager != null)
//                {
//                    _userManager.Dispose();
//                    _userManager = null;
//                }
//                if (_signInManager != null)
//                {
//                    _signInManager.Dispose();
//                    _signInManager = null;
//                }
//                // Ensure RoleManager is also disposed
//                if (_roleManager != null)
//                {
//                    _roleManager.Dispose();
//                    _roleManager = null;
//                }
//            }
//            base.Dispose(disposing);
//        }

//        #region Helpers
//        private const string XsrfKey = "XsrfId";

//        private IAuthenticationManager AuthenticationManager
//        {
//            get
//            {
//                return HttpContext.GetOwinContext().Authentication;
//            }
//        }

//        private void AddErrors(IdentityResult result)
//        {
//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError("", error);
//            }
//        }

//        private ActionResult RedirectToLocal(string returnUrl)
//        {
//            if (Url.IsLocalUrl(returnUrl))
//            {
//                return Redirect(returnUrl);
//            }
//            return RedirectToAction("Index", "Home");
//        }

//        internal class ChallengeResult : HttpUnauthorizedResult
//        {
//            public ChallengeResult(string provider, string redirectUri)
//                : this(provider, redirectUri, null)
//            {
//            }

//            public ChallengeResult(string provider, string redirectUri, string userId)
//            {
//                LoginProvider = provider;
//                RedirectUri = redirectUri;
//                UserId = userId;
//            }

//            public string LoginProvider { get; set; }
//            public string RedirectUri { get; set; }
//            public string UserId { get; set; }

//            public override void ExecuteResult(ControllerContext context)
//            {
//                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
//                if (UserId != null)
//                {
//                    properties.Dictionary[XsrfKey] = UserId;
//                }
//                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
//            }
//        }
//        #endregion
//    }
//}