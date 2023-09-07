using UnityEngine.UI;
using UnityEngine;

public class ItemCard : Card<Item> {

    public Image QualityBanner;
    public Image QualityBackground;
    public Image QualityBorder;

    [Space]
    public Text Name;
    public Image Icon;
    public Text Type;
    public Text FlavorText;
    public Text RequiredLevel;
    public Text SellValue;

    public override void Refresh(Item item) {

        if (QualityBanner)
            QualityBanner.sprite = item && item.Quality ? item.Quality.QualityBanner : ItemQuality.DefaultQualityBanner;

        if (QualityBackground)
            QualityBackground.sprite = item && item.Quality ? item.Quality.ItemSlotBackground : ItemQuality.DefaultQualityBackground;

        if (QualityBorder)
            QualityBorder.sprite = item && item.Quality ? item.Quality.ItemSlotBorder : ItemQuality.DefaultQualityBorder;

        if (Name) {
            if (!item) {
                Name.text = "";
            } else {
                var name = item.DisplayName != "" ? item.DisplayName : item.name;
                Name.text = name.Length > 23 ? name.Remove(20) + "..." : name;
            }
        }

        if (Icon)
            Icon.sprite = item ? item.Icon : null;

        if (Type) {
            if (!item)
                Type.text = "";
            else
                Type.text = item.Quality ? item.Quality.name + " " + item.TypeDescription : item.TypeDescription;
        }

        if (FlavorText) {
            FlavorText.text = item ? item.FlavorText : "";
            FlavorText.transform.parent.gameObject.SetActive(FlavorText.text != "");
        }

        if (RequiredLevel)
            RequiredLevel.text = item ? item.RequiredLevel.ToString() : "0";

        if (SellValue)
            SellValue.text = item ? item.SellValue.ToString() : "0";
    }
}