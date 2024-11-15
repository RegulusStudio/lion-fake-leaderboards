using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeaguesUIManager : MonoBehaviour
    {
        [Header("Prefab References (Do not change)")]
        [SerializeField] private LeagueOfferScreen offerScreen;
        [SerializeField] private LeagueUsernamePopupScreen usernamePopupScreen;
        [SerializeField] private LeagueLeaderboardScreen leaderboardScreen;
        [SerializeField] private LeagueEndScreen endScreen;
        [SerializeField] private LeagueInfoScreen infoScreen;

        public LeagueLeaderboardScreen LeaderboardScreen {
            get { return leaderboardScreen; }
        }
        
        private LeaguesManager leaguesManager;

        protected virtual void Start()
        {
            leaguesManager = LeaguesManager.Instance;
            if (leaguesManager.IsInitialized)
            {
                Init();
            }
            else
            {
                leaguesManager.OnLeagueInitialized += Init;
            }
        }

        protected virtual void Init()
        {
            endScreen.Init(this);
            leaderboardScreen.Init(this);
            offerScreen.Init(this);
            offerScreen.OnContinue += ShowUsernameScreen;
            LeaderboardEntryDisplay.OnEditUsernameButtonClick += ShowUsernameScreen;
            LeagueUsernamePopupScreen.OnPopupClosed += JoinLeague;
        }

        private void ShowUsernameScreen()
        {
            offerScreen.Hide();
            usernamePopupScreen.Show();
        }
        
        protected virtual void JoinLeague()
        {
            if (leaguesManager.HasJoined)
            {
                Debug.Log("League already joined!");
                return;
            }
            
            leaguesManager.HasJoined = true;
            LeaguesAnalytics.FireLeagueJoinedEvent(leaguesManager.leagues, leaguesManager.leagues[leaguesManager.CurrentLeague].name, leaguesManager.CurrentLeague.ToString());
            ShowLeagueUI();
        }
        
        public virtual void ShowLeagueUI()
        {
            if (!leaguesManager.isEnabled)
            {
                Debug.Log("League is disabled! Not showing");
                return;
            }

            if (ShowOfferScreen() || HandleStoredScores()) return;

            leaderboardScreen.Show();
            FireLeagueCheckEvent();
        }
        
        protected virtual bool ShowOfferScreen()
        {
            if (!leaguesManager.HasJoined)
            {
                offerScreen.Show();
                return true;
            }
            return false;
        }
        
        protected virtual bool HandleStoredScores()
        {
            TournamentProgress storedScores = leaguesManager.Leaderboard.scoresStorage.GetLastOutdatedScores(leaguesManager.LastStartTime);
            if (storedScores != null)
            {
                var scores = leaguesManager.GetPastScores(storedScores);
                leaguesManager.Leaderboard.scoresStorage.ClearPastScores(leaguesManager.LastStartTime);
                endScreen.Show(scores);
                return true;
            }
            return false;
        }
        
        public virtual void ResetLeaderboard()
        {
            leaderboardScreen.InitLeaderboard();
        }
        
        public virtual void HideAll()
        {
            offerScreen.Hide();
            leaderboardScreen.Hide();
            endScreen.Hide();
        }
        
        protected void FireLeagueCheckEvent()
        {
            LeaguesAnalytics.FireLeagueCheckEvent(leaguesManager.leagues, leaguesManager.CurrentLeague, leaguesManager.GetCurrentScores(), leaguesManager.promoteCount);
        }
    }
}
