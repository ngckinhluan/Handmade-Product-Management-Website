﻿using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ProductDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagementAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ControllerTests
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly ProductController _productController;

        public ProductControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _productController = new ProductController(_productServiceMock.Object);
        }

        [Fact]
        public async Task GetProducts_ReturnsOkResult_WithListOfProducts()
        {
            var products = new List<ProductDto>
            {
                new ProductDto { Id = "1", Name = "Product 1" },
                new ProductDto { Id = "2", Name = "Product 2" }
            };
            _productServiceMock.Setup(service => service.GetAll())
                .ReturnsAsync(products);
            var result = await _productController.GetProducts();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponse<IList<ProductDto>>>(okResult.Value);
            Assert.Equal("200", response.Code);
            Assert.Equal(products, response.Data);
        }

        [Fact]
        public async Task GetProduct_ReturnsOkResult_WithSingleProduct()
        {
            var product = new ProductDto { Id = "1", Name = "Product 1" };
            _productServiceMock.Setup(service => service.GetById("1"))
                .ReturnsAsync(product);
            var result = await _productController.GetProduct("1");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponse<ProductDto>>(okResult.Value);
            Assert.Equal("200", response.Code);
            Assert.Equal(product, response.Data);
        }

        //[Fact]
        //public async Task CreateProduct_ReturnsOkResult_WithCreatedProduct()
        //{
        //    var newProduct = new ProductForCreationDto { Name = "New Product" };
        //    var createdProduct = new ProductDto { Id = "1", Name = "New Product" };
        //    _productServiceMock.Setup(service => service.Create(newProduct))
        //        .ReturnsAsync(createdProduct);
        //    var result = await _productController.CreateProduct(newProduct);
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var response = Assert.IsType<BaseResponse<ProductDto>>(okResult.Value);
        //    Assert.Equal("200", response.Code);
        //    Assert.Equal(createdProduct, response.Data);
        //    Assert.Equal("Product created successfully", response.Message);
        //}
        //[Fact]
        //public async Task UpdateProduct_ReturnsOkResult_WithSuccessMessage()
        //{
        //    var updatedProduct = new ProductForUpdateDto { Name = "Updated Product" };
        //    var productDto = new ProductDto { Id = "1", Name = "Updated Product" }; 
        //    _productServiceMock.Setup(service => service.Update("1", updatedProduct))
        //        .ReturnsAsync(productDto);
        //    var result = await _productController.UpdateProduct("1", updatedProduct);
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var response = Assert.IsType<BaseResponse<string>>(okResult.Value);
        //    Assert.Equal("200", response.Code);
        //    Assert.Equal("Product updated successfully", response.Data);
        //}

        //[Fact]
        //public async Task SoftDeleteProduct_ReturnsOkResult_WithSuccessMessage()
        //{
        //    _productServiceMock.Setup(service => service.SoftDelete("1"))
        //        .ReturnsAsync(true); 
        //    var result = await _productController.SoftDeleteProduct("1");
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var response = Assert.IsType<BaseResponse<string>>(okResult.Value);
        //    Assert.Equal("200", response.Code);
        //    Assert.Equal("Product soft-deleted successfully", response.Data);
        //}

        [Fact]
        public async Task GetProductDetails_ReturnsOkResult_WithProductDetails()
        {
            var productDetails = new ProductDetailResponseModel { Id = "1", Description = "Product 1 details" };
            _productServiceMock.Setup(service => service.GetProductDetailsByIdAsync("1"))
                .ReturnsAsync(productDetails);
            var result = await _productController.GetProductDetails("1");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponse<ProductDetailResponseModel>>(okResult.Value);
            Assert.Equal("200", response.Code);
            Assert.Equal(productDetails, response.Data);
        }

        [Fact]
        public async Task CalculateAverageRating_ReturnsOkResult_WithAverageRating()
        {
            var averageRating = 4.5m;
            _productServiceMock.Setup(service => service.CalculateAverageRatingAsync("1"))
                .ReturnsAsync(averageRating);
            var result = await _productController.CalculateAverageRating("1");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponse<decimal>>(okResult.Value);
            Assert.Equal("200", response.Code);
            Assert.Equal(averageRating, response.Data);
        }

        [Fact]
        public async Task SearchProducts_ReturnsOkResult_WithSearchResults()
        {
            var searchFilter = new ProductSearchFilter { Name = "Test" };
            var searchResults = new List<ProductSearchVM>
            {
                new ProductSearchVM { Id = "1", Name = "Test Product 1" },
                new ProductSearchVM { Id = "2", Name = "Test Product 2"}
            };
            _productServiceMock.Setup(service => service.SearchProductsAsync(searchFilter))
                .ReturnsAsync(searchResults);
            var result = await _productController.SearchProducts(searchFilter);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponse<IEnumerable<ProductSearchVM>>>(okResult.Value);
            Assert.Equal("200", response.Code);
            Assert.Equal(searchResults, response.Data);
        }

    }
}
