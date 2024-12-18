using System;
using System.Threading.Tasks;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueView : MonoBehaviour
    {
        [SerializeField] private LeaderboardEntriesDisplay entriesDisplay;
        [SerializeField] private CurrentLeagueNameDisplay _currentLeagueNameDisplay;

        public LeaderboardEntriesDisplay EntriesDisplay
        {
            get { return entriesDisplay; }
        }
        
        private LeaguesManager leaguesManager => LeaguesManager.Instance;
        private bool isInitializing;

        private bool _isAlreadyDataUpdated = false;

        private void Awake()
        {
            if (leaguesManager.IsInitialized)
            {
                Init();
            }
            else
            {
                leaguesManager.OnLeagueInitialized += Init;
            }
        }

        private void OnEnable()
        {
            if (!isInitializing && leaguesManager != null)
            {
                UpdateData(true, true);
            }
        }

        private void Update()
        {
            if (!_isAlreadyDataUpdated && !leaguesManager.IsAnyLeagueActiveInCurrentTime())
            {
                _isAlreadyDataUpdated = true;
                UpdateData(true, false);
            }
        }

        private void Init()
        {
            InitLeaderboard();
        }

        private void InitLeaderboard()
        {
            isInitializing = true;
            bool hasOutdatedScores = leaguesManager.HasOutdatedScores();
            LeaderboardCalculatedData scores = hasOutdatedScores
                ? leaguesManager.GetStoredScores()
                : (leaguesManager.HasJoined ? leaguesManager.GetCurrentScores() : leaguesManager.GetInitialScores());
            entriesDisplay.Init(scores, 
                leaguesManager.promoteCount, 
                leaguesManager.CurrentLeague < leaguesManager.leagues.Count - 1, 
                leaguesManager.CurrentLeague > 0, 
                leaguesManager.animatePlayerOnly,
                leaguesManager.perRankTime,
                leaguesManager.maxPlayerAnimationTime,
                leaguesManager.minPlayerAnimationTime);
            isInitializing = false;

            _currentLeagueNameDisplay.Init(leaguesManager);
            UpdateData(true, false);
            entriesDisplay.FocusOnPlayer();
        }

        public void UpdateData(bool focusOnPlayer, bool animated)
        {
            if (leaguesManager == null) return;

            Firebase.Crashlytics.Crashlytics.Log("League: Update data on LeagueView script");

            Debug.Log("Updating Leaderboard Data");
            bool hasOutdatedScores = leaguesManager.HasOutdatedScores();
            LeaderboardCalculatedData scores =
                hasOutdatedScores ? leaguesManager.GetStoredScores() : leaguesManager.GetCurrentScores();
            entriesDisplay.UpdateData(scores, focusOnPlayer, animated);
        }

        private void OnDisable()
        {
            _isAlreadyDataUpdated = false;
        }
    }
}