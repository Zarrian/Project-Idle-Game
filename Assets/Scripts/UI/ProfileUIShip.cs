using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.UI
{
    public class ProfileUIShip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Panel Background")]
        [SerializeField] private Image panelBackground;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(0.8f, 0.9f, 1f, 1f);

        [Header("Profile UI")]
        [SerializeField] private Image profileImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text textNumber;

        public PanelUpgrade panelUpgrade;

        public Image barFilled;


        [Header("Options")]
        [SerializeField] private bool hideUnusedProgressBars = true;

        public HangarShip ship;


        private void OnEnable()
        {

            if (panelBackground == null)
                panelBackground = GetComponent<Image>();

            SetPanelColor(normalColor);
        }

        private void Start()
        {
            SetProfile();
            ship.OnChangeUnitList += UpdateInfoUI;
            ship.OnChangeUnitList += UpdateBar;
        }

        public void SetProfile()
        {
            if (profileImage != null)
                profileImage.sprite = ship.unit.uiSprite;

            if (nameText != null)
                nameText.text = ship.unit.tierName;


            UpdateInfoUI();
            UpdateBar();
        }

        public void UpdateInfoUI()
        {
            textNumber.text = ship.unitsList.Count.ToString() + "/" + ship.unit.maxUnits.ToString();
        }

        public void UpdateBar()
        {
            float percentage = (float)ship.unitsList.Count / ship.unit.maxUnits;

            barFilled.fillAmount = percentage;

            if (percentage < 0.33f)
                barFilled.color = Color.Lerp(new Color(0.3f, 0f, 0f), Color.red, percentage / 0.33f);
            else if (percentage < 0.5f)
                barFilled.color = Color.Lerp(Color.red, new Color(1f, 0.5f, 0f), (percentage - 0.33f) / 0.17f);
            else
                barFilled.color = Color.Lerp(new Color(1f, 0.5f, 0f), Color.green, (percentage - 0.5f) / 0.5f);
        }

        public void OpenPanelInfo()
        {
            panelUpgrade.InteractPanel();
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            SetPanelColor(hoverColor);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetPanelColor(normalColor);
        }

        private void SetPanelColor(Color color)
        {
            if (panelBackground != null)
                panelBackground.color = color;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OpenPanelInfo();
        }
    }
}
