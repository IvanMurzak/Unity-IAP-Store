using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Store
{
	public class UIPrice
    {
        [Required]  public TextMeshProUGUI  textPrice;
                    public Image            iconCurrency;

                    public Vector3          textPriceOriginalScale { get; set; }
                    public Color            textPriceOriginalColor { get; set; }
    }
}