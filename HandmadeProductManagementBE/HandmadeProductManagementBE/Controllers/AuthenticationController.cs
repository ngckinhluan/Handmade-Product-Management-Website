using System.IdentityModel.Tokens.Jwt;
using System.Web;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.AuthModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Repositories.Entity;
using HandmadeProductManagement.Services.Service;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagementAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(
    UserManager<ApplicationUser> userManager,
    TokenService tokenService,
    IEmailService emailService,
    IAuthenticationService authenticationService
)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<BaseResponse<UserLoginResponseModel>>> Login(LoginModelView loginModelView)
    {
        if (string.IsNullOrWhiteSpace(loginModelView.PhoneNumber) &&
            string.IsNullOrWhiteSpace(loginModelView.Email) &&
            string.IsNullOrWhiteSpace(loginModelView.UserName) ||
            string.IsNullOrWhiteSpace(loginModelView.Password)
           )
        {
            return new BaseResponse<UserLoginResponseModel>()
            {
                StatusCode = StatusCodeHelper.Unauthorized,
                Message = "At least one of Phone Number, Email, or Username is required for login.",
            };
        }

        var user = await userManager.Users
            .Include(u => u.UserInfo)
            .Include(u => u.Cart)
            .FirstOrDefaultAsync(u => u.Email == loginModelView.Email
                                      || u.PhoneNumber == loginModelView.PhoneNumber
                                      || u.UserName == loginModelView.UserName);

        if (user is null)
        {
            return new BaseResponse<UserLoginResponseModel>()
            {
                StatusCode = StatusCodeHelper.Unauthorized,
                Message = "Incorrect user login credentials"
            };
        }

        if (user.Status != Constants.UserActiveStatus)
        {
            return new BaseResponse<UserLoginResponseModel>()
            {
                StatusCode = StatusCodeHelper.Unauthorized,
                Message = "This account has been disabled."
            };
        }

        var success = await userManager.CheckPasswordAsync(user, loginModelView.Password);

        if (success)
        {
            var userResponse = await CreateUserResponse(user); // Await async call
            return BaseResponse<UserLoginResponseModel>.OkResponse(userResponse);
        }

        return new BaseResponse<UserLoginResponseModel>()
        {
            StatusCode = StatusCodeHelper.Unauthorized,
            Message = "Incorrect password",
        };
    }

    private async Task<UserLoginResponseModel> CreateUserResponse(ApplicationUser user)
    {
        var token = await tokenService.CreateToken(user);
        return new UserLoginResponseModel()
        {
            FullName = user.UserInfo.FullName,
            UserName = user.UserName,
            DisplayName = user.UserInfo.DisplayName,
            Token = token
        };
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<BaseResponse<string>>> Register(RegisterModelView registerModelView)
    {
        //throw new BaseException.BadRequestException("bad_request", "this is a very bad request");
        if (!ValidationHelper.IsValidNames(CustomRegex.UsernameRegex, registerModelView.UserName) ||
            !ValidationHelper.IsValidNames(CustomRegex.FullNameRegex, registerModelView.FullName)
           )
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.Unauthorized,
                Message = "Username or Full Name contains invalid characters.",
            };

        if (await userManager.Users.AnyAsync(x => x.UserName == registerModelView.UserName))
        {
            ModelState.AddModelError("username", "Username is already taken");
        }

        if (await userManager.Users.AnyAsync(x => x.Email == registerModelView.Email))
        {
            ModelState.AddModelError("email", "Email is already taken");
        }

        if (await userManager.Users.AnyAsync(x => x.PhoneNumber == registerModelView.PhoneNumber))
        {
            ModelState.AddModelError("phone", "Phone is already taken");
        }

        //Return validation errors if any
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return new BaseResponse<string>
            {
                StatusCode = StatusCodeHelper.BadRequest,
                Message = "Validation failed: " + string.Join("; ", errors),
                Data = null
            };
        }

        var user = registerModelView.Adapt<ApplicationUser>();

        var result = await userManager.CreateAsync(user, registerModelView.Password);

        if (result.Succeeded)
        {
            await emailService.SendEmailConfirmationAsync(user.Email!, registerModelView.ClientUri);
            await authenticationService.AssignRoleToUser(user.Id.ToString(), "Seller");

            return BaseResponse<string>.OkResponse(user.Id.ToString());
        }

        var errorMessages = result.Errors
            .Select(e => e.Description)
            .ToList();

        return new BaseResponse<string>()
        {
            StatusCode = StatusCodeHelper.BadRequest,
            Message = "User creation failed: " + string.Join("; ", errorMessages),
            Data = null
        };
    }

    [Authorize(Roles = "Admin")]
    [AllowAnonymous]
    [HttpPost("admin/register")]
    public async Task<ActionResult<BaseResponse<string>>> RegisterForAdmin(RegisterModelView registerModelView)
    {
        //throw new BaseException.BadRequestException("bad_request", "this is a very bad request");
        if (!ValidationHelper.IsValidNames(CustomRegex.UsernameRegex, registerModelView.UserName) ||
            !ValidationHelper.IsValidNames(CustomRegex.FullNameRegex, registerModelView.FullName)
           )
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.Unauthorized,
                Message = "Username or Full Name contains invalid characters.",
            };

        if (await userManager.Users.AnyAsync(x => x.UserName == registerModelView.UserName))
        {
            ModelState.AddModelError("username", "Username is already taken");
        }

        if (await userManager.Users.AnyAsync(x => x.Email == registerModelView.Email))
        {
            ModelState.AddModelError("email", "Email is already taken");
        }

        if (await userManager.Users.AnyAsync(x => x.PhoneNumber == registerModelView.PhoneNumber))
        {
            ModelState.AddModelError("phone", "Phone is already taken");
        }

        //Return validation errors if any
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return new BaseResponse<string>
            {
                StatusCode = StatusCodeHelper.BadRequest,
                Message = "Validation failed: " + string.Join("; ", errors),
                Data = null
            };
        }

        var user = registerModelView.Adapt<ApplicationUser>();

        var result = await userManager.CreateAsync(user, registerModelView.Password);

        if (result.Succeeded)
        {
            await emailService.SendEmailConfirmationAsync(user.Email!, registerModelView.ClientUri);
            await authenticationService.AssignRoleToUser(user.Id.ToString(), "Admin");

            return BaseResponse<string>.OkResponse(user.Id.ToString());
        }

        return new BaseResponse<string>()
        {
            StatusCode = StatusCodeHelper.BadRequest,
            Message = result.Errors.ToString(),
        };
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<BaseResponse<string>>> ForgotPassword(
        ForgotPasswordModelView forgotPasswordModelView)
    {
        var user = await userManager.FindByEmailAsync(forgotPasswordModelView.Email);
        if (user == null
            // || !user.EmailConfirmed
           )
        {
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.BadRequest,
                Message = "Email is invalid or not confirmed."
            };
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(resetToken); //encode the token for URL safety

        var passwordResetLink = $"{forgotPasswordModelView.ClientUri}?email={user.Email}&token={encodedToken}";

        await emailService.SendPasswordRecoveryEmailAsync(user.Email!, passwordResetLink);

        return new BaseResponse<string>()
        {
            StatusCode = StatusCodeHelper.OK,
            Message = "Password reset link has been sent to your email."
        };
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult<BaseResponse<string>>> ResetPassword(ResetPasswordModelView resetPasswordModelView)
    {
        var user = await userManager.FindByEmailAsync(resetPasswordModelView.Email);
        if (user == null)
        {
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.BadRequest,
                Message = "Invalid request."
            };
        }

        var decodedToken = HttpUtility.UrlDecode(resetPasswordModelView.Token); //decode the token from the request
        var result = await userManager.ResetPasswordAsync(user, decodedToken, resetPasswordModelView.NewPassword);

        if (result.Succeeded)
        {
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.OK,
                Message = "Password has been reset successfully."
            };
        }

        return new BaseResponse<string>()
        {
            StatusCode = StatusCodeHelper.BadRequest,
            Message = "Error resetting the password.",
        };
    }

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<BaseResponse<string>> ConfirmEmail(ConfirmEmailModelView confirmEmailModelView)
    {
        var user = await userManager.FindByEmailAsync(confirmEmailModelView.Email);
        if (user is null)
        {
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.BadRequest,
                Message = "User not found."
            };
        }

        var decodedToken = HttpUtility.UrlDecode(confirmEmailModelView.Token);
        var result = await userManager.ConfirmEmailAsync(user, decodedToken);

        if (result.Succeeded)
        {
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.OK,
                Message = StatusCodeHelper.OK.Name()
            };
        }

        return BaseResponse<string>.FailResponse(statusCode: StatusCodeHelper.BadRequest,
            message: "Error confirming the email.");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok("This is an Admin only endpoint");
    }

    [Authorize(Roles = "Seller")]
    [HttpGet("seller-only")]
    public IActionResult SellerOnlyEndpoint()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(new { Message = "This is a Seller only endpoint", Claims = claims });
    }

    [Authorize]
    [HttpGet("test-claims")]
    public IActionResult TestClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        //var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        //var nameIdentifier = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        //var emailAddress = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        //var role = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

        return Ok(claims);
    }


    [HttpPost]
    [Route("google-login")]
    public async Task<IActionResult> GoogleLogin(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        if (jsonToken == null)
        {
            return BadRequest("Invalid token.");
        }

        var email = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value;
        var name = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value;
        var picture = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "picture")?.Value;

        if (email == null || name == null)
        {
            return BadRequest("Token is missing necessary claims.");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return BadRequest("Failed to create a new user.");
            }
            var userInfo = new UserInfo
            {
                Id = (user.Id).ToString(),                   
                FullName = name,                   
                AvatarUrl = picture,              
                DisplayName = name,               
                Bio = string.Empty,                 
                BankAccount = null,                 
                BankAccountName = null,            
                Bank = null,
                Address = null,                  
                UserInfoImages = new List<UserInfoImage>() 
            };
            await userManager.AddToRoleAsync(user, "Customer");
        }
        var roles = await userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault();
        var userToken = await tokenService.CreateToken(user);

        return Ok(new
        {
            Token = userToken
        });
    }

    [HttpPost]
    [Route("facebook-login")]
    public async Task<IActionResult> FacebookLogin(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        if (jsonToken == null)
        {
            return BadRequest("Invalid token.");
        }

        var email = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value;
        var name = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value;
        var picture = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "picture")?.Value;

        if (email == null || name == null)
        {
            return BadRequest("Token is missing necessary claims.");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return BadRequest("Failed to create a new user.");
            }

            var userInfo = new UserInfo
            {
                Id = (user.Id).ToString(),                   
                FullName = name,                   
                AvatarUrl = picture,              
                DisplayName = name,               
                Bio = string.Empty,                 
                BankAccount = null,                 
                BankAccountName = null,            
                Bank = null,
                Address = null,                  
                UserInfoImages = new List<UserInfoImage>() 
            };
            await userManager.AddToRoleAsync(user, "Customer");
        }
        var roles = await userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault();
        var userToken = await tokenService.CreateToken(user);
        return Ok(new
        {
            Token = userToken
        });
    }
}