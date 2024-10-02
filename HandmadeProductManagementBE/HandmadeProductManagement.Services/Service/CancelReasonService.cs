﻿using AutoMapper;
using FluentValidation;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class CancelReasonService : ICancelReasonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CancelReasonForCreationDto> _creationValidator;
        private readonly IValidator<CancelReasonForUpdateDto> _updateValidator;

        public CancelReasonService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CancelReasonForCreationDto> creationValidator, IValidator<CancelReasonForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _creationValidator = creationValidator;
            _updateValidator = updateValidator;
        }

        // Get all cancel reasons (only active records)
        public async Task<IList<CancelReasonResponseModel>> GetAll()
        {
            IQueryable<CancelReason> query = _unitOfWork.GetRepository<CancelReason>().Entities
                .Where(cr => !cr.DeletedTime.HasValue || cr.DeletedBy == null);

            var result = await query.Select(cancelReason => new CancelReasonResponseModel
            {
                Id = cancelReason.Id.ToString(),
                Description = cancelReason.Description, 
                RefundRate = cancelReason.RefundRate,
            }).ToListAsync();
            return result;
        }

        // Get cancel reasons by page (only active records)
        public async Task<IList<CancelReasonResponseModel>> GetByPage(int page, int pageSize)
        {
            if (page <= 0)
            {
                throw new BaseException.BadRequestException("invalid_input", "Page must be greater than 0.");
            }

            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException("invalid_input", "Page size must be greater than 0.");
            }

            IQueryable<CancelReason> query = _unitOfWork.GetRepository<CancelReason>().Entities
                .Where(cr => !cr.DeletedTime.HasValue || cr.DeletedBy == null);

            var cancelReasons = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cancelReason => new CancelReasonResponseModel
                {
                    Id = cancelReason.Id.ToString(),
                    Description = cancelReason.Description,
                    RefundRate = cancelReason.RefundRate,
                })
                .ToListAsync();

            var cancelReasonDto = _mapper.Map<IList<CancelReasonResponseModel>>(cancelReasons);
            return cancelReasonDto;
        }

        // Create a new cancel reason
        public async Task<CancelReasonResponseModel> Create(CancelReasonForCreationDto cancelReason)
        {
            // Validate
            var result = _creationValidator.ValidateAsync(cancelReason);
            if (!result.Result.IsValid)
            {
                throw new ValidationException(result.Result.Errors);
            }

            var cancelReasonEntity = _mapper.Map<CancelReason>(cancelReason);

            // Set metadata
            cancelReason.CreatedBy = "currentUser"; // Update with actual user info
            cancelReason.LastUpdatedBy = "currentUser"; // Update with actual user info

            await _unitOfWork.GetRepository<CancelReason>().InsertAsync(cancelReasonEntity);
            await _unitOfWork.SaveAsync();

            var cancelReasonToReturn = _mapper.Map<CancelReasonResponseModel>(cancelReasonEntity);

            return cancelReasonToReturn;
        }

        // Update an existing cancel reason
        public async Task<CancelReasonResponseModel> Update(string id, CancelReasonForCreationDto updatedCancelReason)
        {
            var result = _updateValidator.ValidateAsync(updatedCancelReason);
            if (!result.Result.IsValid)
            {
                throw new ValidationException(result.Result.Errors); 
            }
            var cancelReasonEntity = await _unitOfWork.GetRepository<CancelReason>().Entities
                .FirstOrDefaultAsync(p => p.Id == id);
            if (cancelReasonEntity == null)
            {
                throw new BaseException.NotFountException("Cancel Reason not found");
            }
            _mapper.Map(updatedCancelReason, cancelReasonEntity);

            cancelReasonEntity.LastUpdatedTime = DateTime.UtcNow;
            cancelReasonEntity.LastUpdatedBy = "user";

            await _unitOfWork.GetRepository<CancelReason>().UpdateAsync(cancelReasonEntity);
            await _unitOfWork.SaveAsync();
            var cancelReasonToReturn = _mapper.Map<CancelReasonResponseModel>(cancelReasonEntity);
            return cancelReasonToReturn;
        }

        // Soft delete 
        public async Task<bool> Delete(string id)
        {
            var cancelReasonRepo = _unitOfWork.GetRepository<CancelReason>();
            var cancelReasonEntity = await cancelReasonRepo.Entities.FirstOrDefaultAsync(x => x.Id == id);
            if (cancelReasonEntity == null)
            {
                throw new BaseException.NotFountException("Cancel Reason not found");
            }
            cancelReasonEntity.DeletedTime = DateTime.UtcNow;
            await cancelReasonRepo.UpdateAsync(cancelReasonEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
