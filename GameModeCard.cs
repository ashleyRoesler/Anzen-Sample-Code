using ReimEnt.Core;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class GameModeCard : Card<GameMode> {

    public bool RefreshOnAwake = false;

    [Space]
    public Text NameText;
    public Image IconImage;
    public GameObject ComingSoon;

    [Space]
    public Text SubHeaderText;
    public Text DescriptionText;

    [Space]
    public Button Button;
    public Image Image;

    [Space]
    public Sprite NormalSprite;
    public Sprite EmptySprite;

    [Button("Refresh Card")]
    private void RefreshCard() {
        Refresh();
    }

    private void Awake() {
        if (RefreshOnAwake)
            Refresh();
    }

    public override void Refresh(GameMode target) {

        // if there is no target, set the card to empty
        if (Image) {
            if (!target)
                Image.sprite = EmptySprite;
            else
                Image.sprite = NormalSprite;
        }

        if (NameText) {
            NameText.text = target ? target.DisplayName : "";

            var color = NameText.color;
            color.a = target && target.IsComingSoon ? 0.1f : 1f;
            NameText.color = color;
        }

        if (ComingSoon)
            ComingSoon.SetActive(target && target.IsComingSoon);

        if (SubHeaderText)
            SubHeaderText.text = target ? target.SubHeader : "";

        if (DescriptionText)
            DescriptionText.text = target ? target.Description : "";

        if (IconImage) {
            IconImage.sprite = target ? target.Icon : null;

            var color = IconImage.color;
            color.a = target && target.IsComingSoon ? 0.1f : 1f;
            IconImage.color = color;

            IconImage.gameObject.SetActive(target);
        }

        // if the game mode is unavailable, disable button
        if (target && Button) {
            if (target.IsComingSoon || !target.IsAvailable)
                Button.interactable = false;
            else
                Button.interactable = true;
        }
        else if (Button)
            Button.enabled = false;
    }
}