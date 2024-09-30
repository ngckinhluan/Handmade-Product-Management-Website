﻿using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Repositories.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Order : BaseEntity
    {
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Note { get; set; }
        public string? CancelReasonId { get; set; }
        [JsonIgnore]
        public ApplicationUser? User { get; set; }
        [JsonIgnore]
        public CancelReason? CancelReason { get; set; }
        [JsonIgnore]
        public Payment? Payment { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<StatusChange> StatusChanges { get; set; } = [];
    }
}
