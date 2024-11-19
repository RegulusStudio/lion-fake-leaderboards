using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [RequireComponent(typeof(TMP_Text))]
    public class CurrentLeagueNameDisplay : MonoBehaviour
    {

        [SerializeField] protected string suffix;
        [SerializeField] protected bool setTextMaterial = true;
        [SerializeField] protected Image textBackground;

        protected TMP_Text text;

        protected LeaguesManager leaguesManager;

        public void Init(LeaguesManager league)
        {
            leaguesManager = league;
            text = GetComponent<TMP_Text>();
            InitializeValues();
        }

        protected virtual void InitializeValues()
        {
            League currentLeague = leaguesManager.leagues[leaguesManager.CurrentLeague];
            text.text = $"{currentLeague.name}{suffix}";
            if (setTextMaterial && currentLeague.nameMaterial != null) text.fontSharedMaterial = currentLeague.nameMaterial;
            if (textBackground != null) textBackground.sprite = currentLeague.nameBackground;
        }
    }
}
