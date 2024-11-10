using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Project.Store
{
    public class StoreCategoryAdapter : StoreAdapter
    {
        protected   bool        ValidateCategory    (string x) => StoreSO.Instance.ValidateCategory(x);
        protected   string[]    AllCategories       ()         => StoreSO.Instance.AllCategories();

        [ValidateInput("ValidateCategory")]
        [ValueDropdown("AllCategories", IsUniqueList = true)]
        public      string      category;
    
        protected override List<StoreSellable> GetSellables() => StoreSO.Instance.categories[category];
    }
}
