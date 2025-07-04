// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MovieManagement.Models;

namespace MovieManagement.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Email là bắt buộc")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Họ là bắt buộc")]
            [Display(Name = "Họ")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Tên là bắt buộc")]
            [Display(Name = "Tên")]
            public string LastName { get; set; }
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Lỗi từ nhà cung cấp bên ngoài: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Lỗi tải thông tin đăng nhập từ bên ngoài.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Kiểm tra xem có email từ provider không
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                ErrorMessage = "Không thể lấy thông tin email từ Google. Vui lòng thử lại.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Kiểm tra xem user đã tồn tại chưa
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                // Người dùng đã tồn tại, liên kết external login nếu chưa có
                var existingLogin = await _userManager.GetLoginsAsync(existingUser);
                var hasGoogleLogin = existingLogin.Any(x => x.LoginProvider == info.LoginProvider);
                
                if (!hasGoogleLogin)
                {
                    var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                    if (!addLoginResult.Succeeded)
                    {
                        foreach (var error in addLoginResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return Page();
                    }
                }                // Cập nhật EmailConfirmed = true và thêm role User nếu chưa có
                if (!existingUser.EmailConfirmed)
                {
                    existingUser.EmailConfirmed = true;
                    await _userManager.UpdateAsync(existingUser);
                }

                // Đảm bảo user có role User
                var roles = await _userManager.GetRolesAsync(existingUser);
                if (!roles.Contains("User"))
                {
                    await _userManager.AddToRoleAsync(existingUser, "User");
                }

                // Đăng nhập user
                await _signInManager.SignInAsync(existingUser, isPersistent: false, info.LoginProvider);
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                
                // Lấy thông tin từ Google
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";
                
                // Nếu không có given name và surname, thử lấy từ Name
                if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                {
                    var fullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "";
                    var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (nameParts.Length > 0)
                    {
                        firstName = nameParts[0];
                        lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";
                    }
                }

                Input = new InputModel
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Lỗi tải thông tin đăng nhập từ bên ngoài.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.EmailConfirmed = true; // Tự động xác nhận email cho Google login

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        // Thêm role mặc định
                        await _userManager.AddToRoleAsync(user, "User");

                        // Đăng nhập ngay lập tức
                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
