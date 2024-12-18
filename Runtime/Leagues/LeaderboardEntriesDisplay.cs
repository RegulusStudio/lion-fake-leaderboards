using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LionStudios.Suite.Core.LeanTween;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeaderboardEntriesDisplay : MonoBehaviour
    {
        [SerializeField] private Transform leaderboardTopThreeContentTransform;
        [SerializeField] private Transform leaderboardScrollContentTransform;
        [SerializeField] private LeaderboardEntryDisplay prefab;
        [SerializeField] private GameObject promotionPrefab;
        [SerializeField] private GameObject demotionPrefab;

        private Dictionary<Transform, Vector3>
            scrollRanksPreviousSyncedPositions = new Dictionary<Transform, Vector3>();

        private Dictionary<Transform, Vector3> topRanksPreviousSyncedPositions = new Dictionary<Transform, Vector3>();
        private List<EntryData<LeaderboardEntryDisplay>> topThreeRankEntries;
        private List<EntryData<LeaderboardEntryDisplay>> scrollRankEntries;
        private Transform playerEntry;
        private GameObject promotionSeparator;
        private GameObject demotionSeparator;

        private int promoteCount;

        private ContentSizeFitter sizeFitter;
        private HorizontalOrVerticalLayoutGroup layoutGroup;
        private ScrollRect scrollRect;

        private const int TopRanksCount = 3;

        private bool _isDataAlreadyUpdating = false;
        private bool _animatePlayerOnly;
        private Vector3 _startSizeOfEntryDisplay;

        private int _siblingMovementIndex = -1;
        private RectTransform playerRectTransform;
        private RectTransform bottomPlayerRectTransform;
        private RectTransform viewport;

        private const float RANK_SCALING_ANIMATION = 0.2f;
        private const float ONE_RANK_OFFSET_VALUE_PERCENTAGE = 0.05f;

        //These variables will override from Advance settings
        private float _perRankTime = 0.15f;
        private float _maxPlayerAnimationTime = 3f;
        private float _minPlayerAnimationTime = 0.5f;
        
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private class EntryData<T>
        {
            public string participantName;
            public T leaderboardEntryDisplay;
        }

        public void Init(LeaderboardCalculatedData data, int promoteCount, bool hasPromotionZone, bool hasDemotionZone,
            bool animatePlayerOnly, float perRankSpeed, float maxPlayerAnimationTime, float minPlayerAnimationTime)
        {
            layoutGroup = leaderboardScrollContentTransform.GetComponent<HorizontalOrVerticalLayoutGroup>();
            sizeFitter = leaderboardScrollContentTransform.GetComponent<ContentSizeFitter>();
            scrollRect = leaderboardScrollContentTransform.GetComponentInParent<ScrollRect>();
            this.promoteCount = promoteCount;
            _animatePlayerOnly = animatePlayerOnly;
            _perRankTime = perRankSpeed;
            _maxPlayerAnimationTime = maxPlayerAnimationTime;
            _minPlayerAnimationTime = minPlayerAnimationTime;
            
            leaderboardScrollContentTransform.DestroyChildrenImmediate();

            topThreeRankEntries = new List<EntryData<LeaderboardEntryDisplay>>();
            scrollRankEntries = new List<EntryData<LeaderboardEntryDisplay>>();

            var displays = leaderboardTopThreeContentTransform.GetComponentsInChildren<LeaderboardEntryDisplay>();
            for (int i = 0; i < TopRanksCount; i++)
            {
                var display = displays[i];
                if (i > data.participantDatas.Count)
                {
                    display.gameObject.SetActive(false);
                    return;
                }

                ParticipantData participantData = data.participantDatas[i];
                bool isPlayer = i == data.playerIndex;
                display.Init(i, participantData, isPlayer);
                topThreeRankEntries.Add(new EntryData<LeaderboardEntryDisplay>()
                {
                    participantName = participantData.name,
                    leaderboardEntryDisplay = display
                });
            }
            
            scrollRect.onValueChanged.AddListener(OnScrollUpdate);
            viewport = leaderboardScrollContentTransform.parent.GetComponent<RectTransform>();

            RectTransform playerInstance = null;
            
            for (var i = 0; i < data.participantDatas.Count; i++)
            {
                if (hasPromotionZone && i == promoteCount)
                    promotionSeparator = Instantiate(promotionPrefab, leaderboardScrollContentTransform);
                if (hasDemotionZone && i == data.participantDatas.Count - promoteCount)
                    demotionSeparator = Instantiate(demotionPrefab, leaderboardScrollContentTransform);
                ParticipantData participantData = data.participantDatas[i];
                LeaderboardEntryDisplay instance = Instantiate(prefab, leaderboardScrollContentTransform);
                bool isPlayer = i == data.playerIndex;
                instance.Init(i, participantData, isPlayer);
                scrollRankEntries.Add(new EntryData<LeaderboardEntryDisplay>()
                {
                    participantName = data.participantDatas[i].name,
                    leaderboardEntryDisplay = instance
                });
                
                _startSizeOfEntryDisplay = instance.transform.localScale;
            }

            _isDataAlreadyUpdating = false;

            Canvas.ForceUpdateCanvases();
        }

        public void UpdateData(LeaderboardCalculatedData data, bool focusOnPlayer, bool animated)
        {
            if (_isDataAlreadyUpdating)
                return;

            _isDataAlreadyUpdating = true;
            
            //Reset Data
            LeanTween.cancelAll(true);
            scrollRanksPreviousSyncedPositions.Clear();
            topRanksPreviousSyncedPositions.Clear();
            ToggleLayoutActivation(true);
            Canvas.ForceUpdateCanvases();
            
            if (bottomPlayerRectTransform != null)
            {
                Destroy(bottomPlayerRectTransform.gameObject);
                bottomPlayerRectTransform = null;
            }

            var previousPlayerEntryData = scrollRankEntries.FirstOrDefault(x => x.leaderboardEntryDisplay._isPlayer);
            int previousPlayerRank = -1;

            if (previousPlayerEntryData != null)
            {
                previousPlayerRank = previousPlayerEntryData.leaderboardEntryDisplay._rank;
            }

            var participantList = data.GetParticipantList();

            //Set old initial position list first
            for (int i = 0; i < participantList.Count; i++)
            {
                var participant = participantList[i];

                //For all ranks
                EntryData<LeaderboardEntryDisplay> previousEntryDisplayData =
                    scrollRankEntries.Find(p => p.participantName == participant.name);

                //If previous entry was also in scroll rect content ranks
                if (previousEntryDisplayData != null)
                {
                    scrollRanksPreviousSyncedPositions[scrollRankEntries[i].leaderboardEntryDisplay.transform] =
                        previousEntryDisplayData.leaderboardEntryDisplay.transform.localPosition;
                }
            }

            //Update UI data
            for (int i = 0; i < participantList.Count; i++)
            {
                var participant = participantList[i];
                int rank = i;
                bool isPlayer = i == data.playerIndex;

                if (rank < TopRanksCount)
                {
                    topThreeRankEntries[i].participantName = participant.name;
                    topThreeRankEntries[i].leaderboardEntryDisplay.UpdateData(rank, participant, isPlayer);
                }

                int scrollRankIndex = rank;
                scrollRankEntries[scrollRankIndex].participantName = participant.name;
                scrollRankEntries[scrollRankIndex].leaderboardEntryDisplay.UpdateData(rank, participant, isPlayer);

                if (isPlayer)
                {
                    playerEntry = scrollRankEntries[scrollRankIndex].leaderboardEntryDisplay.transform;
                }
            }

            for (int i = 0; i < scrollRankEntries.Count; i++)
            {
                scrollRankEntries[i].leaderboardEntryDisplay.transform.SetSiblingIndex(i);
            }

            if (demotionSeparator != null)
            {
                int demotionIndex = scrollRankEntries.Count - promoteCount;
                demotionIndex = Mathf.Clamp(demotionIndex, 0, data.participantDatas.Count());
                demotionSeparator.transform.SetSiblingIndex(demotionIndex);
            }

            if (promotionSeparator != null)
            {
                // int promotionIndex = promoteCount - TopRanksCount;
                int promotionIndex = promoteCount;
                promotionIndex = Mathf.Clamp(promotionIndex, 0, data.participantDatas.Count());
                promotionSeparator.transform.SetSiblingIndex(promotionIndex);
            }

            ToggleLayoutActivation(true);
            Canvas.ForceUpdateCanvases();

            if (focusOnPlayer)
            {
                FocusOnPlayer();
            }

            if (animated)
            {
                if (_animatePlayerOnly)
                {
                    AnimateOnlyPlayerEntry(focusOnPlayer, previousPlayerRank);
                }
                else
                {
                    AnimateAllEntries(focusOnPlayer);
                }
            }
            else
            {
                _isDataAlreadyUpdating = false;
                CreateBottomPlayerEntry(playerEntry);
                CheckVisibility();
            }
        }

        async void AnimateOnlyPlayerEntry(bool focusOnPlayer, int previousPlayerRank)
        {
            if (focusOnPlayer)
            {
                FocusOnPlayer();
            }

            await Task.Yield();

            //To calculate difference of previous and new player rank 
            float extraRankChangeDelayTime = 0;
            float eachRankTimeAfterCalculation = 0.1f;
            LeaderboardEntryDisplay entryPlayerDis = null;
            LeaderboardEntryDisplay entryDisplay = null;

            if (playerEntry != null)
            {
                var playerKvPair =
                    scrollRanksPreviousSyncedPositions.FirstOrDefault(x => x.Key == playerEntry.transform);

                Vector3 targetNewPosition = playerEntry.localPosition;
                Vector3 previousPosition = playerKvPair.Value;
                
                entryDisplay = playerKvPair.Key.GetComponent<LeaderboardEntryDisplay>();
                entryPlayerDis = entryDisplay;

                if (entryDisplay == null) {
                    Firebase.Crashlytics.Crashlytics.Log("League.AnimateOnlyPlayerEntry: `entryDisplay` is null");
                }

                if (IsIncreasingPosition(previousPosition, playerEntry.localPosition))
                {
                    int numberOfPlayerRankChanged = 0;
                    //Just a precaution check that previous player rank is set, otherwise just assign any default value
                    //Here it is 3
                    if (previousPlayerRank == -1)
                    {
                        extraRankChangeDelayTime = 2;
                    }
                    else
                    {
                        numberOfPlayerRankChanged =
                            Mathf.Abs(previousPlayerRank) - Mathf.Abs(entryDisplay._rank);
                        numberOfPlayerRankChanged = Mathf.Abs(numberOfPlayerRankChanged);

                        extraRankChangeDelayTime = numberOfPlayerRankChanged * _perRankTime;
                        //limit max speed
                        extraRankChangeDelayTime = Math.Clamp(extraRankChangeDelayTime, _minPlayerAnimationTime, _maxPlayerAnimationTime);
                        eachRankTimeAfterCalculation = extraRankChangeDelayTime / numberOfPlayerRankChanged; 
                    }

                    if (numberOfPlayerRankChanged > 0)
                    {
                        ToggleLayoutActivation(false);
                        float offsetValueToNpcPositionDetection =
                            ((float)Screen.width / Screen.height) * ONE_RANK_OFFSET_VALUE_PERCENTAGE;

                        entryDisplay.CustomRank(previousPlayerRank);
                    
                        Dictionary<LeaderboardEntryDisplay, Vector3> movedSiblingsRealPosition = new Dictionary<LeaderboardEntryDisplay, Vector3>();
                        movedSiblingsRealPosition = MoveAllUpperSiblingNpcsOnePositionUp(numberOfPlayerRankChanged, previousPlayerRank);
                        _siblingMovementIndex = 0;
                        
                        playerEntry.localPosition = previousPosition;
                        
                        if (focusOnPlayer)
                        {
                            FocusOnPlayer();
                        }
                        
                        Canvas.ForceUpdateCanvases();
                        
                        entryDisplay.PutThisOnTopOfSortingOrder();

                        LeanTween.scale(playerKvPair.Key.gameObject, _startSizeOfEntryDisplay + Vector3.one * 0.1f,
                                RANK_SCALING_ANIMATION).setOnUpdate(FocusOnPlayer)
                            .setOnComplete(() =>
                            {
                                LeanTween.moveLocal(playerKvPair.Key.gameObject, targetNewPosition,
                                        extraRankChangeDelayTime)
                                    .setOnUpdate((float f) =>
                                    {
                                        FocusOnPlayer();
                                        OnPlayerMovingFromLowToHigh(entryDisplay, 
                                            movedSiblingsRealPosition, 
                                            offsetValueToNpcPositionDetection,
                                            eachRankTimeAfterCalculation);
                                    })
                                    .setEase(LeanTweenType.easeInOutQuad)
                                    .setOnComplete(
                                        () =>
                                        {
                                            playerEntry.gameObject.transform.localPosition = targetNewPosition;

                                            LeanTween.scale(playerEntry.gameObject, _startSizeOfEntryDisplay,
                                                    RANK_SCALING_ANIMATION)
                                                .setOnUpdate(FocusOnPlayer);
                                        });
                            });
                    }
                    else
                    {
                        entryDisplay.ResetSortingOrder();
                        //No need to disable these two if player score is decreased as no animation will be played.
                        ToggleLayoutActivation(true);
                    }
                }
                else
                {
                    ToggleLayoutActivation(true);
                }
            }
            else
            {
                ToggleLayoutActivation(true);
            }

            try
            {
                await Task.Delay((int)(RANK_SCALING_ANIMATION * 2 * 1000f + extraRankChangeDelayTime * 1000f + eachRankTimeAfterCalculation * 1000f), _cancellationTokenSource.Token);
            }
            catch(TaskCanceledException)
            {
                return;
            }
            
            ToggleLayoutActivation(true);
            CreateBottomPlayerEntry(entryPlayerDis.transform);

            if (entryDisplay != null)
            {
                entryDisplay.ResetSortingOrder();
            }

            _isDataAlreadyUpdating = false;
        }

        async void AnimateAllEntries(bool focusOnPlayer)
        {
            if (layoutGroup != null)
                layoutGroup.enabled = false;
            if (sizeFitter != null)
                sizeFitter.enabled = false;

            //For all rows animation
            for (int i = 0; i < scrollRanksPreviousSyncedPositions.Count; i++)
            {
                var kvp = scrollRanksPreviousSyncedPositions.ElementAt(i);

                Vector3 targetNewPosition = kvp.Key.localPosition;
                Vector3 previousPosition = kvp.Value;
                kvp.Key.localPosition = previousPosition;

                if (focusOnPlayer && playerEntry != null && kvp.Key == playerEntry.transform)
                {
                    LeaderboardEntryDisplay entryDisplay = kvp.Key.GetComponent<LeaderboardEntryDisplay>();

                    entryDisplay.PutThisOnTopOfSortingOrder();

                    LeanTween.moveLocal(kvp.Key.gameObject, targetNewPosition, 1f).setOnUpdate(FocusOnPlayer)
                        .setOnComplete(
                            () =>
                            {
                                entryDisplay.ResetSortingOrder();
                                kvp.Key.gameObject.transform.localPosition = targetNewPosition;
                            });
                }
                else
                {
                    LeaderboardEntryDisplay entryDisplay = kvp.Key.GetComponent<LeaderboardEntryDisplay>();
                    entryDisplay.ResetSortingOrder();
                    LeanTween.moveLocal(kvp.Key.gameObject, targetNewPosition, 0.7f).setOnComplete(() =>
                    {
                        kvp.Key.gameObject.transform.localPosition = targetNewPosition;
                    });
                }
            }

            try
            {
                await Task.Delay(1100, _cancellationTokenSource.Token);
            }
            catch(TaskCanceledException)
            {
                return;
            }
            
            CreateBottomPlayerEntry(playerEntry.GetComponent<LeaderboardEntryDisplay>().transform);

            if (layoutGroup != null)
                layoutGroup.enabled = true;
            if (sizeFitter != null)
                sizeFitter.enabled = true;

            _isDataAlreadyUpdating = false;
        }

        public void FocusOnPlayer()
        {
            if (playerEntry == null && scrollRankEntries != null)
            {
                if (scrollRankEntries.Count > 0)
                {
                    scrollRect.FocusOnItem(scrollRankEntries[0].leaderboardEntryDisplay.GetComponent<RectTransform>());
                }
            }
            else
            {
                scrollRect.FocusOnItem(playerEntry.GetComponent<RectTransform>());
            }
        }

        private void OnPlayerMovingFromLowToHigh(LeaderboardEntryDisplay playerEntryDisplay, 
            Dictionary<LeaderboardEntryDisplay, Vector3> movedSiblingsRealPosition,
            float offsetValueToNpcPositionDetection, float eachRankTimeAfterCalculation)
        {
            if (_siblingMovementIndex < 0 || _siblingMovementIndex >= movedSiblingsRealPosition.Count)
            {
                return;
            }
            
            var siblingData = movedSiblingsRealPosition.ElementAt(_siblingMovementIndex);
            LeaderboardEntryDisplay entryDisplay = siblingData.Key;
            Vector3 realPosition = siblingData.Value;

            if (playerEntryDisplay.ThisTransform.localPosition.y > entryDisplay.ThisTransform.localPosition.y - offsetValueToNpcPositionDetection)
            {
                _siblingMovementIndex++;
                
                entryDisplay.ResetRankToSavedRank();
                playerEntryDisplay.LowerRankByOne();
                
                if (!LeanTween.isTweening(entryDisplay.ThisTransform.gameObject))
                {
                    LeanTween.moveLocal(entryDisplay.ThisTransform.gameObject, realPosition, eachRankTimeAfterCalculation)
                        .setEase(LeanTweenType.easeOutQuad);
                }
            }
        }

        private Dictionary<LeaderboardEntryDisplay, Vector3> MoveAllUpperSiblingNpcsOnePositionUp(int numberOfPlayerRankChanged, int previousPlayerRank)
        {
            if (numberOfPlayerRankChanged == 0)
            {
                //Means player increasing rank is never changed.
                return null;
            }

            Dictionary<LeaderboardEntryDisplay, Vector3> realPositionOfSiblings = new Dictionary<LeaderboardEntryDisplay, Vector3>();

            List<LeaderboardEntryDisplay> allEntryDisplays =
                playerEntry.parent.GetComponentsInChildren<LeaderboardEntryDisplay>().ToList();

            for (int i = previousPlayerRank; i > previousPlayerRank - numberOfPlayerRankChanged; i--)
            {
                LeaderboardEntryDisplay currentChildEntryDisplay = null;
                LeaderboardEntryDisplay upperChildEntryDisplay = null;

                currentChildEntryDisplay = allEntryDisplays[i].GetComponent<LeaderboardEntryDisplay>();
                upperChildEntryDisplay = allEntryDisplays[i - 1].GetComponent<LeaderboardEntryDisplay>();
                
                realPositionOfSiblings.Add(currentChildEntryDisplay, currentChildEntryDisplay.ThisTransform.localPosition);
                currentChildEntryDisplay.ThisTransform.localPosition = upperChildEntryDisplay.ThisTransform.localPosition;
                currentChildEntryDisplay.LowerRankByOne();
            }

            return realPositionOfSiblings;
        }

        void FocusOnPlayer(float v)
        {
            FocusOnPlayer();
        }

        private bool IsIncreasingPosition(Vector3 oldPosition, Vector3 newPosition)
        {
            return newPosition.y > oldPosition.y + 1;
        }

        private void ToggleLayoutActivation(bool flag)
        {
            if (layoutGroup != null)
                layoutGroup.enabled = flag;
            if (sizeFitter != null)
                sizeFitter.enabled = flag;
        }

        private void OnScrollUpdate(Vector2 pos)
        {
            CheckVisibility();
        } 
        
        void CheckVisibility()
        {
            if (bottomPlayerRectTransform != null)
                bottomPlayerRectTransform.gameObject.SetActive(IsPlayerBelowViewport());
        } 

        bool IsPlayerBelowViewport()
        {
            // Get the world corners of the viewport
            Vector3[] viewportWorldCorners = new Vector3[4];
            viewport.GetWorldCorners(viewportWorldCorners);
            float viewportBottom = viewportWorldCorners[0].y;

            // Get the world corners of the playerRect element
            Vector3[] elementWorldCorners = new Vector3[4];
            CalculateWorldCorners(playerRectTransform, elementWorldCorners);
            float elementBottom = elementWorldCorners[0].y;

            // Check if the element is below the viewport
            bool isBelowViewport = elementBottom < viewportBottom;

            return isBelowViewport;
        }

        void CalculateWorldCorners(RectTransform rectTransform, Vector3[] corners)
        {
            Vector3[] localCorners = new Vector3[4];
            rectTransform.GetLocalCorners(localCorners);
            for (int i = 0; i < 4; i++)
            {
                corners[i] = rectTransform.TransformPoint(localCorners[i]);
            }
        }
        
        void CreateBottomPlayerEntry(Transform playerEntry)
        {
            playerRectTransform = (RectTransform)playerEntry;
            bottomPlayerRectTransform = Instantiate(playerRectTransform, viewport).GetComponent<RectTransform>();
            bottomPlayerRectTransform.sizeDelta = playerRectTransform.sizeDelta;
            bottomPlayerRectTransform.anchorMin = new Vector2(0.5f, 0);
            bottomPlayerRectTransform.anchorMax = new Vector2(0.5f, 0);
            bottomPlayerRectTransform.pivot = new Vector2(0.5f, 0);
            bottomPlayerRectTransform.anchoredPosition = Vector2.zero;
            bottomPlayerRectTransform.localScale = Vector3.one;
            bottomPlayerRectTransform.gameObject.SetActive(false);

            var actualPlayerDisplay = playerEntry.GetComponent<LeaderboardEntryDisplay>();
            var bottomPlayerDisplay = bottomPlayerRectTransform.GetComponent<LeaderboardEntryDisplay>();
            bottomPlayerDisplay.Init(actualPlayerDisplay._rank, actualPlayerDisplay._participantData, actualPlayerDisplay._isPlayer);
        }

        private void OnDisable()
        {
            ResetData();
        }

        private void ResetData()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            
            //Reset animation if already playing
            LeanTween.cancelAll(true);
            
            ToggleLayoutActivation(true);
            
            Canvas.ForceUpdateCanvases();
            _isDataAlreadyUpdating = false;
        }
    }
}