using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly BackendContext _context;
        public ProductsController(BackendContext context)
        {
            _context = context;
        }

        //Create Product
        //[Authorize]
        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct(Product product)
        {
            try
            {
                product.Category = await _context.Categories
                            .Where(x => x.Id == product.Category.Id)
                            .FirstOrDefaultAsync();

                product.Seller = await _context.Sellers
                            .Where(x => x.Id == product.Seller.Id)
                            .FirstOrDefaultAsync();

                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetProduct", new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }


        //Get Product
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                                                .Include(x => x.Category)
                                                .Include(x => x.Seller)
                                                .Where(x => x.Id == id)
                                                .FirstOrDefaultAsync();

                if (product == null)
                {
                    return NotFound(new { error = "This product doesn't exist" });
                }

                return product;
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }


        //Get All Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                return await _context.Products
                                .Include(x => x.Category)
                                .Include(x => x.Seller)
                                .ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }


        //Update Product
        //[Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    return BadRequest(new { error = "Kindly check details of product to be updated." });
                }
                else
                {
                    _context.Entry(product).State = EntityState.Modified;

                    await _context.SaveChangesAsync();

                    return Content("Product successfully updated.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }


        //Delete Product
        //[Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product != null)
                {
                    _context.Products.Remove(product);

                    await _context.SaveChangesAsync();

                    return Content("Product successfully deleted");
                }
                else
                {
                    return NotFound(new { error = "This product doesn't exist." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.ToString() });
            }
        }
    }
}
