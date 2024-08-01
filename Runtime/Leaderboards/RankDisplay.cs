using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LionStudios.Suite.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class RankDisplay : MonoBehaviour
    {
        public enum Type
        {
            CurrentTournament,
            CompletedTournament
        }

        [SerializeField] private Type target;
        [ShowWhen(nameof(target), Type.CompletedTournament)]
        [SerializeField] private LeagueEndScreen leaguesManagerEndScreen;
        [ShowWhen(nameof(target), Type.CurrentTournament)]
        [SerializeField] [Tooltip("In seconds")]
        private float autoUpdateInterval = 1f;
        [SerializeField] private TextMeshProUGUI rankTxt;
        [SerializeField] private Image rankImage;
        [Tooltip("The last sprite is used for all ranks after.")]
        [SerializeField] private List<Sprite> rankSprites;
        
        private CancellationTokenSource cancellerTokenSource = new CancellationTokenSource();
        private bool hasStarted = false;
        
        private void OnEnable()
        {
            if (target == Type.CompletedTournament && leaguesManagerEndScreen != null)
            {
                int currentRank = leaguesManagerEndScreen.rank;
                UpdateRank(currentRank);
            }
            else
            {
                UpdateRank(-1);
                // We don't call UpdateButton on 1st OnEnable because some things are initialized in Awake so we call the 1st one on Start instead.
                if (hasStarted)
                    AutoUpdateData();
            }
        }

        private void OnDisable()
        {
            StopAutoUpdateData();
        }
        private void Start()
        {
            // This gets checked before the leagues remote config variables have been processed, so check to see if dev by default has leaguesManager disabled
            if (!LeaguesManager.Instance.isEnabled)
            {
                gameObject.SetActive(false);
            }

            AutoUpdateData();
            hasStarted = true;
        }

        private void UpdateRank(int playerRank)
        {
            if (playerRank < 0)
            {
                rankImage.gameObject.SetActive(false);
                rankTxt.gameObject.SetActive(false);
            }
            else
            {
                rankImage.gameObject.SetActive(true);
                rankTxt.gameObject.SetActive(true);
                rankImage.sprite = (playerRank >= rankSprites.Count)
                    ? rankSprites[^1]
                    : rankSprites[playerRank];
                rankTxt.text = (playerRank + 1).ToString();
            }
        }
        
        
        async void AutoUpdateData()
        {
            try
            {
                cancellerTokenSource.Dispose();
                cancellerTokenSource = new CancellationTokenSource();
                await Task.Delay(100, cancellerTokenSource.Token);

                await TaskWaiter.WaitUntil(() => LeaguesManager.Instance.IsInitialized);
            
                while (!cancellerTokenSource.IsCancellationRequested)
                {
                    
                    if (!LeaguesManager.Instance.HasJoined)
                    {
                        UpdateRank(-1);
                    }
                    else if (LeaguesManager.Instance.HasOutdatedScores())
                    {
                        int rank = LeaguesManager.Instance.GetStoredScores().playerIndex;
                        UpdateRank(rank);
                    }
                    else
                    {
                        int currentRank = LeaguesManager.Instance.GetCurrentScores().playerIndex;
                        UpdateRank(currentRank);
                    }
                    
                    await Task.Delay((int)(autoUpdateInterval * 1000), cancellerTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        void StopAutoUpdateData()
        {
            cancellerTokenSource.Cancel(false);
        }
    }
}