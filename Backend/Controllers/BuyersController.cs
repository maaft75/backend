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
    public class BuyersController : ControllerBase
    {
        private readonly BackendContext _context;
        private readonly JWTSettings _jwtSettings;

        public BuyersController(BackendContext context, IOptions<JWTSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }


        //check if Email exists
        async Task<bool> checkIfEmailExists(string emailAddress)
        {
            return await _context.Buyers.AnyAsync(x => x.EmailAddress == emailAddress);
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
        public async Task<ActionResult<Buyer>> Register(Buyer buyer)
        {
            try
            {
                var checkBuyer = await checkIfEmailExists(buyer.EmailAddress);

                if (checkBuyer == false)
                {
                    await _context.Buyers.AddAsync(buyer);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction("GetBuyer", new { Id = buyer.Id }, buyer);
                }
                else
                {
                    return BadRequest(new { error = "This email address already exists." });
                }
            }
            catch(Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }

        //Get Buyer
        [HttpGet("{Id}")]
        public async Task<ActionResult<Buyer>> GetBuyer(int Id)
        {
            try
            {
                var buyer = await _context.Buyers.FindAsync(Id);

                if (buyer != null)
                {
                    return buyer;
                }
                else
                {
                    return NotFound(new { error = "This buyer doesn't exist." });
                }
            }
            catch (Exception ex)
            {

                return BadRequest(new { error = ex.ToString() });
            }
        }

        //Get All Buyers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Buyer>>> GetBuyers()
        {
            return await _context.Buyers.ToListAsync();
        }
    
        //Delete Buyer
        //[Authorize]
        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteBuyer(int Id)
        {
            try
            {
                var buyer = await _context.Buyers
                                        .Where(x => x.Id == Id)
                                        .FirstOrDefaultAsync();

                if (buyer != null)
                {
                    _context.Buyers.Remove(buyer);
                    await _context.SaveChangesAsync();
                    return Content("This buyer has been deleted.");
                }
                else
                {
                    return BadRequest(new { error = "This buyer doesn't exist." });
                }
            }
            catch(Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }

        //Update Buyer's Details
        //[Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateBuyer(Buyer buyer)
        {
            try
            {
                _context.Entry(buyer).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Content("Details updated.");
            }
            catch(Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }

        //Login
        [HttpPost("login")]
        public async Task<ActionResult<BuyerWithToken>> Login(Login login)
        {
            try
            {
                var buyer = await _context.Buyers
                                        .Where(x => x.EmailAddress == login.EmailAddress
                                            && x.Password == login.Password)
                                        .FirstOrDefaultAsync();

                if(buyer != null)
                {
                    BuyerWithToken buyerWithToken = new BuyerWithToken(buyer);

                    buyerWithToken.Token = generateToken(buyer.Id);

                    return buyerWithToken;
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
