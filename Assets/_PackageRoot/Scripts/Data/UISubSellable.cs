using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Store
{
    public class UISubSellable
    {
        [Required]  public GameObject       root;
                    public TextMeshProUGUI  textTitle;
                    public TextMeshProUGUI  textQuantity;
                    public Image            sellableIcon;
    }
}
