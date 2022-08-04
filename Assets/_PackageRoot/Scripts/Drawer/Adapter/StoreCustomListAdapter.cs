using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;


namespace Project.Store
{
    public class StoreCustomListAdapter : StoreAdapter
    {
	    protected bool        ValidateIDs       (List<string> x)    => x.All(id => StoreSO.Instance.ValidateID(id));
        protected string[]    AllSellableIDs    ()                  => StoreSO.Instance.AllSellableIDs();

        [ValidateInput("ValidateIDs")]
        [ValueDropdown("AllSellableIDs", IsUniqueList = true)]
	    public List<string> sellableIDs = new List<string>();

        protected override List<StoreSellable> GetSellables() => sellableIDs.Select(id => StoreSO.Instance.GetSellable(id)).ToList();
    }
}