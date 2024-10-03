using Azure;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.InteropServices;
using System.ComponentModel.DataAnnotations;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService) => _productService = productService;

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchFilter searchFilter)
        {
            var products = await _productService.SearchProductsAsync(searchFilter);
            var response = new BaseResponse<IEnumerable<ProductSearchVM>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Search Product Successfully",
                Data = products
            };
            return Ok(response);
        }


        //[httpget("search")]
        //public async task<iactionresult> searchproducts(productsearchmodel searchmodel)
        //{
        //    var response = new baseresponse<productsearchvm>
        //    {
        //        code = ,
        //        statuscode = statuscodehelper.ok,
        //        message = "success",
        //        data = _productservice.searchproductsasync(searchmodel)
        //    };
        //    return ok(response);
        //}

        [HttpGet("sort")]
        public async Task<IActionResult> SortProducts([FromQuery] ProductSortFilter sortModel)
        {
            var products = await _productService.SortProductsAsync(sortModel);
            var response = new BaseResponse<IEnumerable<ProductSearchVM>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Sort Product Successfully",
                Data = products
            };
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            try
            {
                IList<ProductDto> products = await _productService.GetAll();
                return Ok(BaseResponse<IList<ProductDto>>.OkResponse(products));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(string id)
        {
            try
            {
                ProductDto product = await _productService.GetById(id);
                return Ok(BaseResponse<ProductDto>.OkResponse(product));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Product not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductForCreationDto productForCreation)
        {
            try
            {
                ProductDto createdProduct = await _productService.Create(productForCreation);
                return Ok(BaseResponse<ProductDto>.OkResponse(createdProduct));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(string id, ProductForUpdateDto productForUpdate)
        {
            try
            {
                await _productService.Update(id, productForUpdate);
                return Ok(BaseResponse<string>.OkResponse("Product updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Product not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(string id)
        {
            try
            {
                await _productService.Delete(id);
                return Ok(new BaseResponse<bool>(StatusCodeHelper.OK, "Product deleted successfully.", true));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Product not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpDelete("soft-delete/{id}")]
        public async Task<ActionResult> SoftDeleteProduct(string id)
        {
            try
            {
                await _productService.SoftDelete(id);
                return Ok(BaseResponse<string>.OkResponse("Product soft-deleted successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(BaseResponse<string>.FailResponse("Product not found"));
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, BaseResponse<string>.FailResponse(ex.Message));
            }
        }

        [HttpGet("GetProductDetails/{id}")]
        public async Task<IActionResult> GetProductDetails([Required] string id)
        {
            var productDetails = await _productService.GetProductDetailsByIdAsync(id);
            var response = new BaseResponse<ProductDetailResponseModel>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Product details retrieved successfully.",
                Data = productDetails
            };
            return Ok(response);
        }

        [HttpGet("CalculateAverageRating/{id}")]
        public async Task<IActionResult> CalculateAverageRating([Required] string id)
        {
            var averageRating = await _productService.CalculateAverageRatingAsync(id);
            var response = new BaseResponse<decimal>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Average rating calculated successfully.",
                Data = averageRating
            };
            return Ok(response);
        }
    }
}
