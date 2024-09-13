using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public abstract class LeagueScreen : MonoBehaviour
    {
        protected LeaguesManager leaguesManager;
        protected LeaguesUIManager uiManager;
        
        public virtual void Init(LeaguesUIManager uiManager)
        {
            this.leaguesManager = LeaguesManager.Instance;
            this.uiManager = uiManager;
        }
        
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }
        
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
        
    }
}
