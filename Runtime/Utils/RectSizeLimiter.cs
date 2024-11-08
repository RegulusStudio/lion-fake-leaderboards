using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    
    // Based on https://discussions.unity.com/t/rect-transform-size-limiter/730374/4
    [ExecuteInEditMode]
    public class RectSizeLimiter : UIBehaviour, ILayoutSelfController
    {
        
        RectTransform rectTransform => (RectTransform)transform;
        
        private DrivenRectTransformTracker m_Tracker;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        public void SetLayoutHorizontal()
        {
            HorizontalLayoutGroup container = rectTransform.parent.GetComponent<HorizontalLayoutGroup>();
            if (container == null)
                return;
            float containerWidth = container.GetComponent<RectTransform>().rect.width - container.padding.left - container.padding.right;
            float remainingWidthForSelf = containerWidth;
            
            for (int i = 0; i < container.transform.childCount; i++)
            {
                Transform child = container.transform.GetChild(i);
                if (child != this.transform && child.gameObject.activeInHierarchy)
                    remainingWidthForSelf -= child.GetComponent<RectTransform>().rect.width + container.spacing;
            }
            
            if (rectTransform.rect.width > remainingWidthForSelf)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, remainingWidthForSelf);
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
            }
            container.enabled = false;
            container.enabled = true;
        }

        public void SetLayoutVertical()
        {
            VerticalLayoutGroup container = rectTransform.parent.GetComponent<VerticalLayoutGroup>();
            if (container == null)
                return;
            float containerHeight = container.GetComponent<RectTransform>().rect.height - container.padding.top - container.padding.bottom;
            float remainingHeightForSelf = containerHeight;
            
            for (int i = 0; i < container.transform.childCount; i++)
            {
                Transform child = container.transform.GetChild(i);
                if (child != this.transform && child.gameObject.activeInHierarchy)
                    remainingHeightForSelf -= child.GetComponent<RectTransform>().rect.height + container.spacing;
            }

            if (rectTransform.rect.height > remainingHeightForSelf)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, remainingHeightForSelf);
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
            }
            container.enabled = false;
            container.enabled = true;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
#endif

    }

}