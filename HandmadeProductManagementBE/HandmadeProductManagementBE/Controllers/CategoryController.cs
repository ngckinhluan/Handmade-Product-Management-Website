﻿using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService) => _categoryService = categoryService;

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _categoryService.GetAll();
            return Ok(BaseResponse<IList<CategoryDto>>.OkResponse(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(string id)
        {
            var result = await _categoryService.GetById(id);
            return Ok(BaseResponse<CategoryDto>.OkResponse(result));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryForCreationDto categoryForCreation)
        {
            var result = await _categoryService.Create(categoryForCreation);
            return Ok(BaseResponse<CategoryDto>.OkResponse(result));
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategory(string categoryId, CategoryForUpdateDto categoryForUpdate)
        {
            var result = await _categoryService.Update(categoryId, categoryForUpdate);
            return Ok(BaseResponse<CategoryDto>.OkResponse(result));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var result = await _categoryService.SoftDelete(id);
            return Ok(BaseResponse<bool>.OkResponse(result));
        }
    }
}