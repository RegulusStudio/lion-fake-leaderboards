using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueOfferScreen : LeagueScreen
    {
        
        [SerializeField] private Button continueBtn;

        [SerializeField] private LeaguesIconsDisplay leaguesDisplay;

        public event Action OnContinue;
        
        public override void Init(LeaguesUIManager uiManager)
        {
            base.Init(uiManager);
            List<League> leagues = leaguesManager.leagues; 
            bool joinOverride = leaguesManager.overrideJoin;
            if (leaguesDisplay != null) leaguesDisplay.Init(leagues, -1);
            continueBtn.onClick.RemoveAllListeners();
            if (!joinOverride)
            {
                continueBtn.onClick.AddListener(Hide);
                continueBtn.onClick.AddListener(() => OnContinue?.Invoke());
            }
        }

        public override void Show()
        {
            base.Show();

            if (leaguesDisplay != null) leaguesDisplay.Show();
        }

    }
}
