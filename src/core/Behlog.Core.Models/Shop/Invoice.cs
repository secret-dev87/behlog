﻿using System;
using System.Collections.Generic;
using System.Text;
using Behlog.Core.Models.Enum;

namespace Behlog.Core.Models.Shop
{
    public class Invoice
    {
        public Invoice() {
            Orders = new HashSet<Order>();
            Payments = new HashSet<Payment>();
        }

        #region Properties
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public string InvoiceNumnber { get; set; }

        public DateTime InvoiceDate { get; set; }

        public InvoiceStatus Status { get; set; }

        public decimal TotalPrice { get; set; }

        public int? ShippingId { get; set; }

        public int? ShippingAddressId { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ModifyDate { get; set; }
        #endregion

        #region Navigations
        public Customer Customer { get; set; }
        public Shipping Shipping { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Payment> Payments { get; set; }
        #endregion
    }
}
