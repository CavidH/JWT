    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using IdentityJWT.Models;
    using IdentityJWT.ViewModels;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;

    namespace IdentityJWT.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class AuthController : ControllerBase
        {
            private UserManager<AppUser> _userManager { get; }
            private SignInManager<AppUser> _signInManager { get; }
            private RoleManager<IdentityRole> _roleManager;
            private IConfiguration _configuration;

            #region Ctor

            public AuthController(SignInManager<AppUser> signInManager,
                UserManager<AppUser> userManager,
                RoleManager<IdentityRole> roleManager,
                IConfiguration configuration)
            {
                _signInManager = signInManager;
                _userManager = userManager;
                _roleManager = roleManager;
                _configuration = configuration;
            }

            #endregion

            private string GenerateJWtToken(AppUser user)
            {
               // var c = _configuration["Jwt:SigninKey"];
                var tokenhandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SigninKey"]);
                var tokenDescriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier,user.Id),
                        new Claim(ClaimTypes.Name,user.UserName),
                        new Claim(ClaimTypes.Email,user.Email),
                    }),
                    Expires = DateTime.UtcNow.AddHours(2),//2 saat
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenhandler.CreateToken(tokenDescriptor);
                return tokenhandler.WriteToken(token);
            }

            #region Login

            [HttpPost]
            [Route("api/[controller]/Login")]
            public async Task<IActionResult> Login(UserLoginVM userLogin, string ReturnUrl)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(userLogin.Email);
                    if (user == null)
                    {
                        return BadRequest("Email tapılmadı");
                    }

                    if (await _userManager.CheckPasswordAsync(user, userLogin.Password) == false)
                    {
                        return BadRequest("Etibarsız parol");
                    }

                    var result =
                        await _signInManager.PasswordSignInAsync(user.UserName, userLogin.Password, userLogin.RememberMe,
                            true);
                    if (result.Succeeded)
                    {
                        // await _userManager.AddClaimAsync(user, new Claim("UserRole", "Admin"));

                        var token = GenerateJWtToken(user);

                        return Ok(new LoginResultVM() { Token = token, UserName = user.UserName });
                        //if (ReturnUrl != null)
                        // {
                        //     return Redirect(ReturnUrl);
                        // }

                        // return Redirect("/adminpanel");
                        // return RedirectToAction("index", "Home");
                    }
                    else
                    {
                        //ModelState.AddModelError(" ", "Yanlış giriş cəhdi");
                        return BadRequest(userLogin);
                    }
                }

                return BadRequest(userLogin);
            }

            #endregion
            #region GetAllUser
            [HttpGet]
            public async Task<List<AppUser>> Users()
            {
                return await _userManager
              .Users
              .ToListAsync();
            }
            #endregion
            #region Register

            [HttpPost]
            [Route("api/[controller]/Register")]
            public async Task<IActionResult> Register(UserPostVM userregVM)
            {
                // CreateRoll();
                if (ModelState.IsValid)
                {
                    var user = new AppUser()
                    {
                        UserName = userregVM.UserName,
                        EmailConfirmed = true,
                        Email = userregVM.EMail,
                        FirstName = userregVM.FirstName,
                        LastName = userregVM.LastName

                    };
                    bool isExistUsername = _userManager.Users.Any(us => us.UserName == user.UserName);
                    if (isExistUsername)
                    {
                        return BadRequest("Bu İstifadəçi adı artıq mövcuddur. Başqa İstifadəçi adı istifadə edin");
                    }

                    bool isExistEmail = _userManager.Users.Any(us => us.Email == user.Email);
                    if (isExistEmail)
                    {
                        return BadRequest("Bu Email artıq mövcuddur. Başqa Email istifadə edin");
                    }

                    var result = await _userManager.CreateAsync(user, userregVM.Password);


                    if (result.Succeeded)
                    {
                        // await _userManager.AddToRoleAsync(user, "Member");
                        //await _userManager.AddToRoleAsync(user, "ADMIN");
                        return Ok($"{userregVM.UserName} adinda user yaradildi");
                    }

                    {
                        string errors = String.Empty;
                        if (result.Errors.Count() > 0)
                        {

                            foreach (var error in result.Errors)
                            {
                                errors += error.Description;
                            }
                        }

                        return BadRequest(errors);
                    }
                }

                return BadRequest();
            }

            #endregion

            #region Logout

            [HttpGet]
            [Route("api/[controller]/Logout")]
            public async Task<IActionResult> Logout()
            {
                await _signInManager.SignOutAsync();
                return Ok();
            }

            #endregion
            #region CreateRoll

            // public async Task CreateRoll()
            // {
            //     foreach (var UserRole in Enum.GetValues(typeof(RoleHelper.UserRoles)))
            //     {
            //         if (!await _roleManager.RoleExistsAsync(UserRole.ToString()))
            //         {
            //             await _roleManager.CreateAsync(new IdentityRole { Name = UserRole.ToString() });
            //         }
            //     }
            //
            //     Console.WriteLine("role done");
            // }

            #endregion
        }
    }
