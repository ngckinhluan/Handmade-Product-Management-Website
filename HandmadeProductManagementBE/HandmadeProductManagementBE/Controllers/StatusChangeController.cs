﻿using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;

namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusChangeController : ControllerBase
    {
        private readonly IStatusChangeService _statusChangeService;

        public StatusChangeController(IStatusChangeService statusChangeService)
        {
            _statusChangeService = statusChangeService;
        }

       
        [HttpGet("page")]
        public async Task<IActionResult> GetByPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = new BaseResponse<IList<StatusChangeResponseModel>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get Status Change sucessfully!",
                Data = await _statusChangeService.GetByPage(page, pageSize)
            };
            return Ok(response);
        }

     
        [HttpGet("Order/{orderId}")]
        public async Task<IActionResult> GetStatusChangesByOrderId(string orderId)
        {
            var response = new BaseResponse<IList<StatusChangeResponseModel>>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Get Status Change sucessfully!",
                Data = await _statusChangeService.GetByOrderId(orderId)
            };
            return Ok(response);
        }

    
        [HttpPost]
        public async Task<IActionResult> CreateStatusChange([FromBody] StatusChangeForCreationDto statusChange)
        {
            var result = await _statusChangeService.Create(statusChange);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Created Status Change successfully!",
                Data = result
            };
            return Ok(response);
        }

      
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatusChange(string id, [FromBody] StatusChangeForUpdateDto updatedStatusChange)
        {
            var result = await _statusChangeService.Update(id, updatedStatusChange);
            var response = new BaseResponse<bool>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = "Updated Status Change successfully!",
                Data = result
            };
            return Ok(response);
        }

     
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStatusChange(string id)
        {
            await _statusChangeService.Delete(id);

            var response = new BaseResponse<string>
            {
                Code = "Success",
                StatusCode = StatusCodeHelper.OK,
                Message = $"Status Change with ID {id} has been successfully deleted.",
                Data = null
            };
            return Ok(response);
        }
    }
}
