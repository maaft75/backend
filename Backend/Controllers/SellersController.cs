using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellersController : ControllerBase
    {
        private readonly BackendContext _context;
        private readonly JWTSettings _jwtSettings;

        public SellersController(BackendContext context, IOptions<JWTSettings> jwtSettings)
        {
            this._context = context;
            this._jwtSettings = jwtSettings.Value;
        }

        //Check if email exists
        async Task<bool> checkIfEmailExists(string emailAddress)
        {
            return await _context.Sellers.AnyAsync(x => x.EmailAddress == emailAddress);
        }

        //Like the function name states
        private string generateToken(int Id)
        {
            //sign token here 
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.PrimarySid, Convert.ToString(Id))
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        //Register
        [HttpPost("register")]
        public async Task<ActionResult<Seller>> Register(Seller seller)
        {
            try
            {
                var checkEmail = await checkIfEmailExists(seller.EmailAddress);

                if (checkEmail == false)
                {
                    await _context.Sellers.AddAsync(seller);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction("GetSeller", new { Id = seller.Id }, seller);
                }
                else
                {
                    return BadRequest(new { error = "This email address already exists." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }

        //Get Seller
        [HttpGet("{Id}")]
        public async Task<ActionResult<Seller>> GetSeller(int Id)
        {
            try
            {
                var seller = await _context.Sellers.FindAsync(Id);

                if (seller != null)
                {
                    return seller;
                }
                else
                {
                    return NotFound(new { error = "This seller doesn't exist." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }

        //Get All Sellers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Seller>>> GetSellers()
        {
            return await _context.Sellers.ToListAsync();
        }

        //Delete
        //[Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteSeller(int Id)
        {
            try
            {
                var seller = await _context.Sellers.FindAsync(Id);

                if(seller != null)
                {
                    _context.Sellers.Remove(seller);

                    await _context.SaveChangesAsync();

                    return Content("This seller has been deleted.");
                }
                else
                {
                    return NotFound(new { error = "This user doesn't exist."});
                }
            }
            catch(Exception ex)
            {
                return BadRequest( new { error = ex.ToString() });
            }
        }

        //Update
        //[Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateSeller(Seller seller)
        {
            try
            {
                _context.Entry(seller).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return Content("Details updated.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }
        
        //Login
        [HttpPost("login")]
        public async Task<ActionResult<SellerWithToken>> Login(Login login)
        {
            try
            {
                var seller = await _context.Sellers
                                .Where(x => x.EmailAddress == login.EmailAddress
                                    && x.Password == login.Password)
                                .FirstOrDefaultAsync();

                if(seller != null)
                {
                    SellerWithToken sellerWithToken = new SellerWithToken(seller);

                    sellerWithToken.Token = generateToken(seller.Id);

                    return sellerWithToken;
                }
                else
                {
                    return NotFound(new { error = "This seller doesn't exist" });
                }
            }
            catch(Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }
    }
}
