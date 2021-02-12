using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.TelegramWorker.Options
{
    public class Categories
    {
        public int CategoryInLine { get; set; }
        public Category[] OutMoneyCategories { get; set; }
        public Category[] IncomeMoneyCategories { get; set; }
    }
}
