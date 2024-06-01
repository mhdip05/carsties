using IdentityModel;
using IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace IdentityServer.Pages.Account.Register
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public RegisterViewModel Input { get; set; }

        [BindProperty]
        public bool RegisterSuccess { get; set; }


        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;   
        }
        public IActionResult OnGet(string returnUrl = null)
        {
            Input = new RegisterViewModel
            {
                ReturnUrl = returnUrl,
            };

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (Input.Button != "register") return Redirect("~");

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Username,
                    Email = Input.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddClaimsAsync(user, new Claim[]
                    {
                        new Claim(JwtClaimTypes.Name, Input.FullName)
                    });

                    RegisterSuccess = true;
                }
            }

            return Page();
        }
    }
}
