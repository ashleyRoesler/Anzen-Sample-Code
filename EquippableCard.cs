using UnityEngine;
using UnityEngine.UI;
using ReimEnt.Anzen;
using ReimEnt.Utils;

public class EquippableCard : Card<Equippable> {

    public GameObject AttributeList;
    public GameObject AttributeTemplate;

    public GameObject StatList;
    public GameObject StatTemplate;

    public override void Refresh(Equippable target) {

        /// attributes
        if (AttributeList) {
            if (AttributeTemplate && target.Attributes.Count > 0) {

                Clear(AttributeList, AttributeTemplate);
                AttributeTemplate.SetActive(false);

                foreach (Attribute a in target.Attributes.GetKeys()) {
                    InstantiateInfo(a.ToString(), target.Attributes[a], AttributeList, AttributeTemplate);
                }

                AttributeList.transform.parent.gameObject.SetActive(true);
            }
            else {
                AttributeList.transform.parent.gameObject.SetActive(false);
            }

            AttributeList.SetActive(target);
        }

        /// stats
        if (StatList) {
            if (StatTemplate && target.Stats.Count > 0) {

                Clear(StatList, StatTemplate);
                StatTemplate.SetActive(false);

                foreach (Stat s in target.Stats.GetKeys()) {
                    InstantiateInfo(Names.NickifyStat(s.ToString()), target.Stats[s], StatList, StatTemplate);
                }

                StatList.transform.parent.gameObject.SetActive(true);
            }
            else {
                StatList.transform.parent.gameObject.SetActive(false);
            }

            StatList.SetActive(target);
        }
    }

    private void Clear(GameObject target, GameObject template) {
        foreach (Transform child in target.transform) {
            if (child.gameObject != template)
                Destroy(child.gameObject);
        }
    }

    private void InstantiateInfo(string key, object value, GameObject list, GameObject template) {
        GameObject info = Instantiate(template);

        info.GetComponent<Text>().text = "+" + value + " " + key;

        info.transform.SetParent(list.transform);
        info.SetActive(true);
    }
}