using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Model;
using WebAPI.DTOs;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Text;

namespace WebAPI.Controllers
{
  

    public class AuthController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration, UserDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // User Registration
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDTO request)
        {
            if (_context.Users.Any(u => u.email == request.email))
            {
                return BadRequest("User with this email already exists.");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                first_name = request.first_name,
                last_name = request.last_name,
                email = request.email,
                passwordHash = passwordHash,
                passwordSalt = passwordSalt,
                isActive = request.isActive
            };

            if (user.email == "admin@gmail.com" && request.Password == "admin")
            {
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.role_name == "Admin");
                if (adminRole == null)
                {
                    adminRole = new Role { role_name = "admin" };
                    _context.Roles.Add(adminRole);
                    await _context.SaveChangesAsync();
                }

                user.UserRoles = new List<UserRole>
                {
                    new UserRole { role_id = adminRole.role_id }
                };
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        // User Login
        [HttpPost("login")]
        public async Task<IActionResult> Login(Login request)
        {
            var user = await _context.Users
               .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Roles)
                .FirstOrDefaultAsync(u => u.email == request.email);

            if (user == null)
            {
                return BadRequest("Email not found.");
            }

            if (!VerifyPasswordHash(request.Password, user.passwordHash, user.passwordSalt))
            {
                return BadRequest("Wrong password.");
            }

            var userRoles = user.UserRoles.Select(ur => ur.Roles.role_name).ToList();
            if (user.email == "admin@gmail.com" && request.Password == "admin")
            {
                userRoles.Add("admin"); // Ensure the admin role is included
            }

            string token = CreateToken(user);
            return Ok(new { token, message = "Logged in successfully", role = user.UserRoles.FirstOrDefault()?.Roles.role_name });
        }


        [HttpPost("CreateRole")]


       [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateRole(Role request)
        {
            var role = new Role
            {
                role_name = request.role_name
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return Ok(role);
        }

        [Authorize(AuthenticationSchemes="Bearer", Roles = "admin")]
        [HttpPost("AssignRoles")]
        public async Task<IActionResult> AssignRole(AssignRole request)
        {
            // Check if the user exists
            var user = await _context.Users
                .Include(u=>u.UserRoles)
                .FirstOrDefaultAsync(u => u.user_id == request.user_id);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // Check if the role exists
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.role_id == request.role_id);
            if (role == null)
            {
                return BadRequest("Role not found.");
            }

            // Check if the user-role assignment already exists
            var existingAssignment = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.user_id == request.user_id && ur.role_id == request.role_id);
            if (existingAssignment != null)
            {
                return BadRequest("User already assigned to this role.");
            }

            // Create and add the new UserRole assignment
            var userRole = new UserRole
            {
                UserRoleID = request.UserRoleID,
                user_id = request.user_id,
                role_id = request.role_id
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Admin '{user.email}' assigned role '{role.role_name}' to user with ID {request.user_id}");

            return Ok(userRole);
        }

        [HttpGet("User with Role")]
        public IActionResult GetUsersWithRoles()
        {
            var usersWithRoles = _context.UserRoles
                .Select(u => new AssignRole
                {
                    UserRoleID = u.UserRoleID,
                    user_id = u.user_id,
                    role_id = u.role_id
               
                })
                .ToList();
            if (usersWithRoles == null || !usersWithRoles.Any())
            {
                return NotFound("No user roles found.");
            }

            return Ok(usersWithRoles);
        }

        [HttpGet("Users")]
        public IActionResult User()
        {
            var users = _context.Users
                .Select(u => new User
                {
               
                    user_id = u.user_id,
                    first_name= u.first_name,
                    last_name = u.last_name,
                    email = u.email
                

                })
                .ToList();
           

            return Ok(users);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("Role")]
        public IActionResult Roles()
        {
            var Roles = _context.Roles
                .Select(u => new Role
                {
               
                    role_id = u.role_id,
                    role_name= u.role_name

                })
                .ToList();
           

            return Ok(Roles);
        }




        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Email, user.email),
    };

            // Include roles in the token
            var userRoles = user.UserRoles.Select(ur => ur.Roles.role_name).ToList();
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role)); // Add role to token claims
            }


            var key = _configuration["JWT:Key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Decode email from token
        public string DecodeEmailFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var decodedToken = handler.ReadJwtToken(token.Substring(7)); // Skip 'Bearer ' prefix

            return decodedToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeRole(string email, string role)
        {
            var user = GetByEmail(email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.role_name == role);
            if (roleEntity == null)
            {
                return BadRequest("Role not found.");
            }

            user.UserRoles = new List<UserRole>
            {
                new UserRole { role_id = roleEntity.role_id, user_id = (int)user.user_id }
            };

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        // Helper methods
        public User GetByEmail(string email)
        {
            return _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Roles)
                .FirstOrDefault(c => c.email == email);
        }

        public User GetById(int id)
        {
            return _context.Users.FirstOrDefault(c => c.user_id == id);
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users.Include(u => u.UserRoles).ToList();
        }
    }
}