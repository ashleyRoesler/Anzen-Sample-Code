using UnityEngine.UI;
using UnityEngine;

public class WeaponCard : Card<Weapon> {

    public GameObject WeaponInfo;

    public Text DamageRange;
    public Image DamageIcon;
    public Text Speed;
    public Text Handling;
    public Text Resource;

    public override void Refresh(Weapon weapon) {

        if (WeaponInfo)
            WeaponInfo.SetActive(weapon);

        /// damage range
        if (DamageRange)
            DamageRange.text = weapon ? weapon.DamageRange.Start + "-" + weapon.DamageRange.End + " " + weapon.DamageType.name + " Damage" : "";

        /// damage icon
        if (DamageIcon) {
            DamageIcon.sprite = weapon ? weapon.DamageType.Icon : null;
            DamageIcon.gameObject.SetActive(weapon);
        }

        /// speed
        if (Speed) {
            if (!weapon)
                Speed.text = "";
            else
                Speed.text = weapon.Speed.ToString().Contains("Very") ? weapon.Speed.ToString().Insert(4, " ") : weapon.Speed.ToString();
        }

        /// handling
        if (Handling) {
            if (!weapon)
                Handling.text = "";
            else
                Handling.text = weapon.Handling == WeaponHandlingType.OneHanded ? "One-Handed" : "Two-Handed";
        }

        /// resource
        if (Resource) {
            Resource.text = weapon ? weapon.ResourceType.ToString() : "";
            Resource.transform.parent.gameObject.SetActive(weapon);
        }
    }
}