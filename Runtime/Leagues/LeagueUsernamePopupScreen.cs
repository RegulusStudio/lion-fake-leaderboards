using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueUsernamePopupScreen : LeagueScreen
    {
        private const string LS_LEAGUES_USERNAME_KEY = "ls_leagues_username";
        
        public TMP_InputField inputField; 
        public Button continueButton; 
        public Button closeButton; 
        
        internal static event Action OnPopupClosed;

        public static string LeaguesUsername
        {
            get => PlayerPrefs.GetString(LS_LEAGUES_USERNAME_KEY, string.Empty);
            private set => PlayerPrefs.SetString(LS_LEAGUES_USERNAME_KEY, value);
        }
        
        private void Start()
        {
            continueButton.onClick.AddListener(ContinueButtonPressed);
            closeButton.onClick.AddListener(()=> OnPopupClosed?.Invoke());
        }

        private void ContinueButtonPressed()
        {
            if (!string.IsNullOrEmpty(inputField.text))
            {
                LeaguesUsername = inputField.text;
            }
            
            OnPopupClosed?.Invoke();
            Hide();
        }
    }
}
