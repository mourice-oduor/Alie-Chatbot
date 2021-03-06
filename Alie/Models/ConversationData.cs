﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alie.Models
{
    public class ConversationData
    {
        //State Management properties
        public string ChannelId { get; set; }

        public bool HasWelcomed { get; set; } = false;

        public bool PromptedUserForName { get; set; } = false;

        public bool HasSelectedProduct { get; set; } = false;

        //public bool HasSelectedProductType{ get; set; } = false;

        //Data
        public bool  HasAppliedLoan { get; set; } = false;

        //public ProductType SelectedProductType { get; set; } = ProductType.NONE;
    }

    public enum ProductType
    {
        NONE = 0,

        AutoLogBookLoan = 1,

        AssetFinance = 2,

        LoanAgainstShares = 3,

        MasomoBoost = 4,

        JijengeLoan = 5,

        ImportFinance = 6
    }
}
