using System.ComponentModel.DataAnnotations;
using AjeboCustomerPortal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class RegisterModel : PageModel
{
    private readonly UserManager<UserDetailes> _userManager;
    private readonly SignInManager<UserDetailes> _signInManager;

    public RegisterModel(UserManager<UserDetailes> userManager, SignInManager<UserDetailes> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = default!;

        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = default!;
    }

    public void OnGet(string returnUrl = null)
    {
        ReturnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    && !returnUrl.StartsWith("/Identity/Account", StringComparison.OrdinalIgnoreCase)
            ? returnUrl
            : Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return Page();

        var user = new UserDetailes
        {
            UserName = Input.Email,
            Email = Input.Email,
            FullName = Input.FullName
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Normalize and sanitize the returnUrl
            var target = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                         && !returnUrl.StartsWith("/Identity/Account", StringComparison.OrdinalIgnoreCase)
                ? returnUrl
                : Url.Content("~/"); // or RedirectToAction("Index","Apartments")

            return LocalRedirect(target);
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return Page();
    }


}
