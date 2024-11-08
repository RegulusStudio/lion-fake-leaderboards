using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeaderboardEntryDisplay : MonoBehaviour
    {

        [SerializeField] private Image backgroundImg;
        [SerializeField] private TMP_Text rankLbl;
        [SerializeField] private Image rankImg;
        [SerializeField] private Sprite[] rankSprites;
        [SerializeField] private Image iconImg;
        [SerializeField] private TMP_Text nameLbl;
        [SerializeField] private TMP_Text scoreLbl;
        
        [SerializeField] private Sprite playerBgSprite;
        [SerializeField] private Color playerLblColor = new Color(0.8f, 0.7f, 0f);
        [SerializeField] private bool updateRankLabelColor = true;
        [SerializeField] private Button editUsernameButton;

        [SerializeField] private RankRewardsDisplay rewardsDisplay;

        private Sprite _normalBgSprite;
        private Color _normalRankLblColor;
        private Color _normalNameLbkColor;

        private bool firstInit = true;
        private Canvas _parentCanvas;
        private RectTransform _nameLblRectTransform;

        internal int _rank { private set; get; }
        private int _savedRank;
        internal ParticipantData _participantData { private set; get; }
        internal bool _isPlayer { private set; get; }

        public Transform ThisTransform { private set; get; }
        
        internal static Action OnEditUsernameButtonClick;

        public void Init(int rank, ParticipantData participantData, bool isPlayer)
        {
            ThisTransform = this.transform;
            _nameLblRectTransform = nameLbl.GetComponent<RectTransform>();
            
            if (_parentCanvas == null)
            {
                _parentCanvas = transform.parent.GetComponentInParent<Canvas>();
            }
            
            if (firstInit)
            {
                _normalBgSprite = backgroundImg.sprite;
                if (rankLbl != null)
                    _normalRankLblColor = rankLbl.color;
                _normalNameLbkColor = nameLbl.color;
            }
            else
            {
                backgroundImg.sprite = _normalBgSprite;
                if (rankLbl != null)
                    rankLbl.color = _normalRankLblColor;
                nameLbl.color = _normalNameLbkColor;
            }

            firstInit = false;

            LeagueUsernamePopupScreen.OnPopupClosed += UpdateDataOnUsernamePopupClose;
            SetEditUsernameListener();
            UpdateData(rank, participantData, isPlayer);
            UpdateDataOnUsernamePopupClose();
        }

        private void OnEnable()
        {
            UpdateDataOnUsernamePopupClose();
        }

        public void UpdateData(int rank, ParticipantData participantData, bool isPlayer)
        {
            _rank = rank;
            _savedRank = rank;
            _participantData = participantData;

            if (isPlayer)
            {
                _isPlayer = true;
                
                if (playerBgSprite != null)
                    backgroundImg.sprite = playerBgSprite;
                if (rankLbl != null && updateRankLabelColor)
                    rankLbl.color = playerLblColor;
                nameLbl.color = playerLblColor;

                //Listeners
                if (editUsernameButton != null)
                {
                    editUsernameButton.gameObject.SetActive(true);
                    SetEditUsernameListener();
                }
                LeagueUsernamePopupScreen.OnPopupClosed -= UpdateDataOnUsernamePopupClose;
                LeagueUsernamePopupScreen.OnPopupClosed += UpdateDataOnUsernamePopupClose;
            }
            else
            {
                _isPlayer = false;
                
                backgroundImg.sprite = _normalBgSprite;
                if (rankLbl != null)
                    rankLbl.color = _normalRankLblColor;
                nameLbl.color = _normalNameLbkColor;
                
                //Listeners
                if (editUsernameButton != null)
                {
                    editUsernameButton.gameObject.SetActive(false);
                    editUsernameButton.onClick.RemoveAllListeners();
                }
                LeagueUsernamePopupScreen.OnPopupClosed -= UpdateDataOnUsernamePopupClose;
            }
            
            if (rankImg != null && rank < rankSprites.Length || rankLbl == null)
            {
                rankImg.gameObject.SetActive(true);
                if (rankLbl != null)
                    rankLbl.gameObject.SetActive(false);
                rankImg.sprite = rankSprites[rank];
            }
            else
            {
                rankImg.gameObject.SetActive(false);
                rankLbl.gameObject.SetActive(true);
                rankLbl.text = (rank + 1).ToString();
            }
            if (iconImg != null)
                iconImg.sprite = participantData.icon;
            nameLbl.text = participantData.name;

            if (isPlayer)
            {
                UpdatePlayerName();
            }
            
            scoreLbl.text = participantData.score.ToString();
            
            if (rewardsDisplay != null)
            {
                LeaguesManager leaguesManager = LeaguesManager.Instance;

                RankRewards rankRewards = leaguesManager.leagues[leaguesManager.CurrentLeague].GetRankRewardsCopy(rank);
                rewardsDisplay.Init(rankRewards, true);
            }
        }

        private void UpdateDataOnUsernamePopupClose()
        {
            if (_isPlayer)
            {
                UpdateData(_rank, _participantData, _isPlayer);
            }
        }

        internal void PutThisOnTopOfSortingOrder()
        {
            Canvas canvas = GetComponent<Canvas>();

            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = _parentCanvas.sortingOrder + 1;
        }

        internal void ResetSortingOrder()
        {
            Canvas canvas = GetComponent<Canvas>();

            if (canvas != null)
            {
                Destroy(canvas);
            }
        }

        internal void LowerRankByOne()
        {
            int localSavedRank = _rank;
            UpdateData(_rank - 1, _participantData, _isPlayer);
            _savedRank = localSavedRank;
        }
        
        internal void ResetRankToSavedRank()
        {
            UpdateData(_savedRank, _participantData, _isPlayer);
        }

        internal void CustomRank(int rank)
        {
            int localSavedRank = _rank;
            UpdateData(rank, _participantData, _isPlayer);
            _savedRank = localSavedRank;
        }

        private void UpdatePlayerName()
        {
            if (!string.IsNullOrEmpty(LeagueUsernamePopupScreen.LeaguesUsername))
            {
                nameLbl.text = LeagueUsernamePopupScreen.LeaguesUsername;
            }
        }

        private void SetEditUsernameListener()
        {
            if (editUsernameButton != null)
            {
                editUsernameButton.onClick.RemoveAllListeners();
                editUsernameButton.onClick.AddListener(() =>
                {
                    OnEditUsernameButtonClick?.Invoke();
                });
            }
        }

        private void OnDestroy()
        {
            LeagueUsernamePopupScreen.OnPopupClosed -= UpdateDataOnUsernamePopupClose;
        }
    }
}
