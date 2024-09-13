using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueCloseButton : MonoBehaviour
    {
        private void Awake()
        {
            LeaguesUIManager leaguesUIManager = GetComponentInParent<LeaguesUIManager>();
            GetComponent<Button>().onClick.AddListener(leaguesUIManager.HideAll);
        }
    }
}
