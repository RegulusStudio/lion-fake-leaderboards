using System;
using System.Collections;
using System.Threading.Tasks;
using LionStudios.Suite.UiCommons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueEndScreen : LeagueScreen
    {
        
        [SerializeField] private TMP_Text recapTitleLbl;
        [SerializeField] private TMP_Text recapMessageLbl;

        [SerializeField] private Button continueBtn;

        [SerializeField] private TMP_Text continueLbl;

        [SerializeField] private RankRewardsDisplay rankRewardsDisplay;

        [SerializeField] private LeagueDisplay leagueDisplay;

        // For animation
        [SerializeField] private Button openBtn;
        [SerializeField] private GameObject endingAnimation;
        [SerializeField] private RankRewardsDisplay openedBoxRankRewardsDisplay;
        [SerializeField] private GameObject openedParentChestObject;
        [SerializeField] private GameObject rankingInfoSection;
        
        [SerializeField] private bool flyOutRewards = true;
        
        private Canvas sourceCanvas;
        private bool ChestOpened;
        internal int rank;

        public void Show(LeaderboardCalculatedData scores)
        // Init(LeaguesManager manager,LeaguesUIManager uiManager, LeaderboardCalculatedData scores, int promoteCount)
        {
            base.Show();
            int promoteCount = leaguesManager.promoteCount;
            sourceCanvas = transform.GetComponent<Canvas>();
            rank = scores.GetPlayerIndex();
            bool hasPromotionZone = leaguesManager.CurrentLeague < leaguesManager.leagues.Count - 1;
            bool hasDemotionZone = leaguesManager.CurrentLeague > 0;

            League completedLeague = leaguesManager.leagues[leaguesManager.CurrentLeague];
            leagueDisplay.Init(completedLeague, -1, -1, false);

            RankRewards rewards = leaguesManager.GetRankAndPromotionRewards(rank);

            rankRewardsDisplay.Init(rewards, true);
            openedBoxRankRewardsDisplay.Init(rewards, false);

            if (hasPromotionZone && rank < promoteCount)
            {
                leaguesManager.LeagueUp();
                recapTitleLbl.text = $"You finished {rank+1}{StatUtils.OrdinalSuffix(rank+1)}! ";
                recapMessageLbl.text = $"Congratulations! You're promoted to the {leaguesManager.leagues[leaguesManager.CurrentLeague].name} league!";
                LeaguesAnalytics.FireLeagueEndEvents(LeaguesAnalytics.MissionType.Completed, leaguesManager, scores);
            }
            else if (hasDemotionZone && rank >= scores.participantDatas.Count - promoteCount)
            {
                leaguesManager.LeagueDown();
                recapTitleLbl.text = $"You finished {rank+1}{StatUtils.OrdinalSuffix(rank+1)}! ";
                recapMessageLbl.text = $"Sorry! You're demoted to the {leaguesManager.leagues[leaguesManager.CurrentLeague].name} league. \n Don't let this pull you down!";
                LeaguesAnalytics.FireLeagueEndEvents(LeaguesAnalytics.MissionType.Failed, leaguesManager, scores);
            }
            else
            {
                if ((leaguesManager.CurrentLeague + 1) == leaguesManager.leagues.Count)
                {
                    recapTitleLbl.text = $"You finished {rank + 1}{StatUtils.OrdinalSuffix(rank + 1)}! ";
                    recapMessageLbl.text = $"You stayed in the {leaguesManager.leagues[leaguesManager.CurrentLeague].name} league! Keep up the work, champion!";
                }
                else
                {
                    recapTitleLbl.text = $"You finished {rank + 1}{StatUtils.OrdinalSuffix(rank + 1)}! ";
                    recapMessageLbl.text = $"You stayed in the {leaguesManager.leagues[leaguesManager.CurrentLeague].name} league! \n Better luck next time!";
                }

                LeaguesAnalytics.FireLeagueEndEvents(LeaguesAnalytics.MissionType.Abandoned, leaguesManager, scores);
            }
            
            leaguesManager.UpdateLeaderboardData();

            if (rewards != null && rewards.isBoxed)
            {
                continueBtn.gameObject.SetActive(false);
                openBtn.gameObject.SetActive(true);
            }
            else
            {
                continueLbl.text = rewards == null ? "CONTINUE" : "CLAIM";
            }

            if (rewards != null)
            {
                leaguesManager.ClaimRewards(rewards.Rewards);
            }

            continueBtn.onClick.RemoveAllListeners();
            openBtn.onClick.RemoveAllListeners();

            continueBtn.onClick.AddListener(async () =>
            {
                if (!openedParentChestObject.gameObject.activeInHierarchy)
                {
                    uiManager.ResetLeaderboard();
                    uiManager.ShowLeagueUI();
                }
                else
                {
                    if (ChestOpened)
                    {
                        
                        if (!flyOutRewards)
                        {
                            ScreenAnimations();
                        }
                        else
                        {
                            for (var i = 0; i < rewards.Rewards.Count; i++)
                            {
                                var reward = rewards.Rewards[i];
                                RewardFlyAnimation.Spawn(
                                    openedBoxRankRewardsDisplay.chestRewards[i],
                                    reward.amount,
                                    openedBoxRankRewardsDisplay.chestRewards[i].transform,
                                    sourceCanvas,
                                    reward.id,
                                    ScreenAnimations);
                                await Task.Delay(150);
                            }
                        }
                    }
                }

                async void ScreenAnimations()
                {
                    ChangeBgScreenStatus(false);
                    await Task.Delay(TimeSpan.FromSeconds(2f));
                    uiManager.ResetLeaderboard();
                    uiManager.ShowLeagueUI();
                    Debug.Log("RWD Should Add!");
                    ChestOpened = false;
                    ChangeBgScreenStatus(true);
                }

                void ChangeBgScreenStatus(bool state)
                {
                    transform.GetChild(0).gameObject.SetActive(state);
                    transform.GetChild(1).gameObject.SetActive(state);
                }

                openBtn.gameObject.SetActive(false);
                openedBoxRankRewardsDisplay.gameObject.SetActive(false);
                rankingInfoSection.SetActive(true);
                openedParentChestObject.SetActive(false);
            });

            openBtn.onClick.AddListener(() =>
            {
                if (rewards != null)
                {
                    if (rewards.isBoxed == true)
                    {
                        Animator animator = endingAnimation.GetComponent<Animator>();
                        if (animator != null)
                        {
                            animator.SetBool("OpenChest", true);
                            openBtn.gameObject.SetActive(false);
                            continueLbl.text = "CLAIM";
                            openedBoxRankRewardsDisplay.Init(rewards, false);

                            RankRewards updatedRewardObject = leaguesManager.GetRankAndPromotionRewards(rank);
                            var cachedBoxSprite = updatedRewardObject.boxSprite;
                            updatedRewardObject.boxSprite = updatedRewardObject.openedBoxSprite;
                            rankRewardsDisplay.Init(rewards, false);
                            updatedRewardObject.boxSprite = cachedBoxSprite;
                            ChestOpened = true;
                            return;
                        }
                    }
                }
                uiManager.ResetLeaderboard();
                uiManager.ShowLeagueUI();
            });

        }
    }
}
