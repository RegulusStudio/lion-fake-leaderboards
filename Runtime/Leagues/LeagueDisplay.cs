using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueDisplay : MonoBehaviour
    {
        [SerializeField] private Image iconImg;
        [SerializeField] private Image leagueIconBgImg;
        [SerializeField] private List<Image> additionalIconImgs;
        [SerializeField] private bool colorizeIcon = false;
        [SerializeField] private Color currentIconColor = Color.white;
        [SerializeField] private Color normalIconColor = Color.white;
        [SerializeField] private float currentLeagueScale = 1f;
        [SerializeField] private float normalScale = 1f;
        [SerializeField] private Image backgroundImg;
        [SerializeField] private Sprite currentBackgroundSprite;
        [SerializeField] private Sprite normalBackgroundSprite;
        [SerializeField] private RectTransform iconContainer;
        [SerializeField] private string suffix;
        [SerializeField] private GameObject currentExtraGO;
        [SerializeField] private Sprite higherIconSpriteOverride;
        [SerializeField] private RankRewardsDisplay promotionRewardDisplay;
        
        public void Init(League league, int current, int index, bool showPromotionRewardsButton)
        {
            bool isCurrent = index >= 0 && current >= 0 && index == current;
            bool isHigher = index >= 0 && current >= 0 && index > current;
            if (iconImg != null)
            {
                iconImg.sprite = league.icon;
                if (colorizeIcon)
                    iconImg.color = isCurrent ? currentIconColor : normalIconColor;
                if (higherIconSpriteOverride != null && isHigher)
                {
                    iconImg.color = normalIconColor;
                    iconImg.sprite = higherIconSpriteOverride;
                }

                SetNextLeagueIcon(iconImg.sprite);
            }

            if (leagueIconBgImg != null)
            {
                leagueIconBgImg.sprite = league.endScreenLeagueBg;
            }
            
            GetComponent<RectTransform>().sizeDelta *= (isCurrent ? currentLeagueScale : normalScale);
            if (currentExtraGO != null)
                currentExtraGO.SetActive(isCurrent);
            if (backgroundImg != null)
            {
                if (isCurrent && currentBackgroundSprite != null)
                    backgroundImg.sprite = currentBackgroundSprite;
                else if (!isCurrent && normalBackgroundSprite != null)
                    backgroundImg.sprite = normalBackgroundSprite;
            }

            if(promotionRewardDisplay != null)
            {
                if (showPromotionRewardsButton && league.promotionRewards.Rewards.Count > 0 && index > current)
                {
                    promotionRewardDisplay.gameObject.SetActive(true);
                    promotionRewardDisplay.Init(league.promotionRewards, false);
                }
                else
                {
                    promotionRewardDisplay.gameObject.SetActive(false);
                }
            }
        }

        public void ResizeBackground(float width)
        {
            RectTransform background = GetComponent<RectTransform>();
            background.sizeDelta = new Vector2(width, background.sizeDelta.y);
        }

        public void RescaleIcon(float scale)
        {
            iconContainer.localScale = Vector3.one * scale;
        }

        private void SetNextLeagueIcon(Sprite nextLeagueIcon)
        {
            foreach (var nextIcon in additionalIconImgs)
            {
                nextIcon.sprite = nextLeagueIcon;
            }
        }
    }
}
