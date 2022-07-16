using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace VerifyEmailAndForgotPasswordApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;

        public UserController(DataContext context)
        {
            _context = context;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            if(_context.Users.Any(u=>u.Email == request.Email))
            {
                return BadRequest("Email Already Exist!!!");
            }
            CreatePasswordHash(request.Password,
                out byte[] passwordHash,
                out byte[] passowrdSalt);
            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passowrdSalt,
                VerificationToken = CreateRandomToken()
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("User Successfully Registered!!!");
        }
        [HttpPost("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user == null)
            {
                return BadRequest("Invalid Token!!!");
            }
            user.VerifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok("User Verified!!!");
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var user=await _context.Users.FirstOrDefaultAsync(u=>u.Email==request.Email);
            if (user == null)
            {
                return BadRequest("User Not Found!!!");
            }
            if(user.VerifiedAt==null)
            {
                return BadRequest("User Is Not Verified!!!");
            }
            if(!VerifyPasswordHash(request.Password,user.PasswordHash,user.PasswordSalt))
            {
                return BadRequest("Password Does Not Match!!!");
            }
            return Ok($"Welcome Back, {user.Email}!!!");
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassowrd(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return BadRequest("User Not Found!!!");
            }
            user.PasswordResetToken=CreateRandomToken();
            user.ResetTokenExpires=DateTime.UtcNow.AddDays(1);
            await _context.SaveChangesAsync();
            return Ok("You May Now Reset Your Password!!!");
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassowrd(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);
            if (user == null  || user.ResetTokenExpires<DateTime.Now)
            {
                return BadRequest("Invalid Token!!!");
            }
            CreatePasswordHash(request.Password,
                out byte[] passwordHash,
                out byte[] passowrdSalt);
            user.PasswordHash=passwordHash;
            user.PasswordSalt=passowrdSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;
            await _context.SaveChangesAsync();
            return Ok("Password Succesfully Reset!!!");
        }
        private static void CreatePasswordHash(string password,out byte[] passwordHash,out byte[] passowrdSalt)
        {
            using (var hmac=new HMACSHA512())
            {
                passowrdSalt = hmac.Key;
                passwordHash=hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                 
            }
        }
        private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passowrdSalt)
        {
            using (var hmac = new HMACSHA512(passowrdSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        private static string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }
    }
}
