using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [RequireComponent(typeof(TMP_Text))]
    public class CurrentLeagueNameDisplay : MonoBehaviour
    {

        [SerializeField] private string suffix;
        [SerializeField] private bool setTextMaterial = true;
        [SerializeField] private Image textBackground;

        private TMP_Text text;

        private LeaguesManager leaguesManager;

        public void Init(LeaguesManager league)
        {
            leaguesManager = league;
            text = GetComponent<TMP_Text>();
            InitializeValues();
        }

        private void InitializeValues()
        {
            League currentLeague = leaguesManager.leagues[leaguesManager.CurrentLeague];
            text.text = $"{currentLeague.name}{suffix}";
            if (setTextMaterial) text.fontSharedMaterial = currentLeague.nameMaterial;
            if (textBackground != null) textBackground.sprite = currentLeague.nameBackground;
        }
    }
}
