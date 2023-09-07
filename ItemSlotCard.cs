using UnityEngine;
using UnityEngine.UI;

public class ItemSlotCard : Card<ItemSlot> {

    public ItemCard ItemCard;
    public WeaponCard WeaponCard;
    public EquippableCard EquippableCard;

    [Space]
    public RectTransform End;

    public override void Refresh(ItemSlot slot) {
        if (ItemCard) {
            ItemCard.Target = slot.Item ? slot.Item : null;
        }

        if (WeaponCard) {
            WeaponCard.Target = slot.Item is Weapon w ? w : null;
        }

        if (EquippableCard) {
            EquippableCard.Target = slot.Item is Equippable e ? e : null;
        }

        // resize tooltip so it fits the information (otherwise it might have too much empty space)
        if (End) {
            var end = new Vector2(End.GetWorldRect().xMax, End.GetWorldRect().yMin);
            var max = ((RectTransform)gameObject.transform).GetWorldRect().max;
            var height = max.y - end.y;

            gameObject.GetComponent<LayoutElement>().preferredHeight = height;
        }
    }
}