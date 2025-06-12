using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BannerScroll<T> : MonoBehaviour
{//데이터 수에 관계없이 3개의 페이지만 가지고 스크롤링 해줌 (데이터만 업데이트 해주는 방식)

    //배너 스크롤 이동 방향
    public enum MovePath
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop,
    }

    //페이지 아이템의 위치(더미 데이터)
    protected Vector2 m_pageItemPos = new Vector2(30 , -10);
    protected Vector2 m_pageItemLayoutPos = new Vector2(30 , 0);

    protected virtual Vector2 GetPageItemPos => m_pageItemPos;
    protected virtual Vector2 GetPageItemPosLayout => m_pageItemLayoutPos;

    // 1 , 2 , 3 순서대로 배너 위치
    private Vector2 m_lastPos;
    private Vector2 m_centerPos;
    private Vector2 m_firstPos;
    //현재 Index
    private int m_currentIndex;
    private List<ScrollBannerItem<T>> m_items = new();

    [InspectorName("Page Select Item"), SerializeField] 
    private RectTransform m_currentPageItem;

    [InspectorName("Page Off Item Content"), SerializeField] 
    private RectTransform m_pageItemContent;

    [InspectorName("Page Select Item"),SerializeField] 
    private GameObject m_pageItemPrefab;

    [InspectorName("Banner Item"),SerializeField] 
    protected GameObject m_BannerItemPrefab;

    private List<RectTransform> m_pageItem = new();
    protected List<T> m_dataList;
    [InspectorName("CustomScrollRect"), SerializeField] private CustomScrollRect m_scrollRect;
    private bool scrolling = false;

    //페이지 이동 속도 설정
    [SerializeField, Header("Page Move Speed")] 
    private float m_moveSpeed = 0.5f;
    [SerializeField] private MovePath m_pathOption = MovePath.LeftToRight;
    private float m_slideSpeed = 0.2f;

    //자동 이동 설정
    [SerializeField, Header("Auto Scrolling")] 
    private bool m_autoScrolling; //자동이동 사용 여부
    [SerializeField, Range(1, 5)] private float m_scrollingTime = 2f; //자동 이동 시간
    Coroutine m_scrollCo;
    WaitForSeconds m_waitForSeconds;

    private bool isHorizontal;
    private bool isSlideFoward;

    protected void Awake()
    {
        isHorizontal = m_pathOption == MovePath.LeftToRight || m_pathOption == MovePath.RightToLeft;
        isSlideFoward = m_pathOption == MovePath.LeftToRight || m_pathOption == MovePath.TopToBottom;
        m_scrollRect.horizontal = isHorizontal;
        m_scrollRect.vertical = !isHorizontal;

        m_waitForSeconds = new WaitForSeconds(m_scrollingTime + m_moveSpeed);
        m_scrollRect.enabled = false;
        m_BannerItemPrefab.SetActive(false);
        float size = isHorizontal ? 
            ((RectTransform)m_BannerItemPrefab.transform).sizeDelta.x : ((RectTransform)m_BannerItemPrefab.transform).sizeDelta.y;

        for (int i = 0; i < 3; i++)
        {
            var obj = Instantiate(m_BannerItemPrefab, m_scrollRect.content);
            var bannerItemCom = obj.GetComponent<ScrollBannerItem<T>>();
            switch (m_pathOption)
            {
                case MovePath.LeftToRight:
                    bannerItemCom.MyRT.anchoredPosition = new Vector2(-size + (size * i), 0);
                    break;
                case MovePath.RightToLeft:
                    bannerItemCom.MyRT.anchoredPosition = new Vector2(size - (size * i), 0);
                    break;
                case MovePath.TopToBottom:
                    bannerItemCom.MyRT.anchoredPosition = new Vector2(0, -size + (size * i));
                    break;
                case MovePath.BottomToTop:
                    bannerItemCom.MyRT.anchoredPosition = new Vector2(0, size - (size * i));
                    break;
            }

            
            bannerItemCom.BannerSetting(i);
            m_items.Add(bannerItemCom);
        }

        m_firstPos = m_items[0].MyRT.anchoredPosition;
        m_centerPos = m_items[1].MyRT.anchoredPosition;
        m_lastPos = m_items[2].MyRT.anchoredPosition;


        for (int i = 0; i < m_items.Count; i++)
        {
            m_items[i].SetPosition(i,m_lastPos, m_firstPos, m_centerPos, m_moveSpeed);
            m_items[i].SetAction(OnClickBanner, GetData);
        }

        m_scrollRect.AddOnDragAction(OnDrageBegin);

        m_scrollRect.AddEndDragAction(OnPointerUp);
    }

    protected void OnEnable()
    {
        SetAutoScrolling(true);
    }

    protected void OnDisable()
    {
        SetAutoScrolling(false);
    }

    public T GetData(int index)
    {
        return m_dataList[index];
    }

    public virtual void UpdateBannerItem(List<T> bannerItem)
    {
        m_dataList = bannerItem;

        if(bannerItem.Count <= 1)
        {
            m_scrollRect.enabled = false;
        }
        else
        {
            m_scrollRect.enabled = true;
        }

        foreach (var item in m_pageItem)
        {
            item.gameObject.SetActive(true);
        }
        if(m_pageItem.Count < m_dataList.Count)
        {
            for(int i = m_pageItem.Count; m_dataList.Count > i; i++) 
            {
                var ob = Instantiate(m_pageItemPrefab, m_pageItemContent);
                ob.SetActive(true);
                ((RectTransform)ob.transform).anchoredPosition = GetPageItemPos + (GetPageItemPosLayout * i);
                m_pageItem.Add((RectTransform)ob.transform);
            }
            m_currentPageItem.anchoredPosition = m_pageItem[m_currentIndex].anchoredPosition;
            m_currentPageItem.SetAsLastSibling();
        }
        else
        {
            for(int i = m_dataList.Count; i < m_pageItem.Count; i++)
            {
                m_pageItem[i].gameObject.SetActive(false);
            }
        }
        if (m_dataList.Count < 1) return;
        foreach (var item in m_items)
        {
            item.InitBannerItem(0, m_dataList.Count - 1, m_dataList[m_currentIndex]);
        }
    }

    //베너 클릭시 이벤트 (index는 넣어준 베너 리스트의 Index )
    public virtual void OnClickBanner(int index) {    }

    public void OnPointerUp()
    {
        if(!scrolling) return;
        if (m_dataList.Count <= 1) return;

        scrolling = false;
        SetAutoScrolling(true);

        float movePos = isHorizontal ? 
            m_scrollRect.horizontalNormalizedPosition : m_scrollRect.verticalNormalizedPosition;

        //현재 마우스가 이동한 거리 (슬라이드 방향)
        if(movePos <= 0)
        {
            if(isSlideFoward)
                SlideBeforePage();
            else
                SlideNextPage();
        }
        else if(movePos >= 1)
        {
            if(isSlideFoward)
                SlideNextPage();
            else
                SlideBeforePage();
        }
        else
        {
            foreach (var i in m_items)
            {
                i.MoveCurrent();
            }
        }
    }

    protected void OnDrageBegin()
    {
        scrolling = true;
        SetAutoScrolling(false);
        foreach(var i in m_items)
        {
            i.gameObject.SetActive(true);
            i.OnSlide();
        }
    }

    protected void SetAutoScrolling(bool active)
    {
        if(m_autoScrolling == false) return;

        if (m_scrollCo != null)
            StopCoroutine(m_scrollCo);

        if (active)
        {
            m_scrollCo = StartCoroutine(AutoScrolling());
        }
    }

    protected IEnumerator AutoScrolling()
    {
        while(scrolling==false) 
        {
            yield return m_waitForSeconds;
            MoveNextPage();
        }
    }

    //다음 페이지로 이동
    public void MoveNextPage()
    {
        NextPage(false);
    }

    //이전 페이지로 이동
    public void MoveBeforePage()
    {
        BeforePage(false);
    }

    //다음 페이지로 슬라이드 됨
    public void SlideNextPage()
    {
        NextPage(true);
    }

    //이전 페이지로 슬라이드 됨
    public void SlideBeforePage()
    {
        BeforePage(true);
    }

    private void NextPage(bool slide)
    {
        if (m_dataList == null) return;
        if (scrolling || m_dataList.Count <= 1) return;
        m_currentIndex++;
        if (m_currentIndex >= m_pageItem.Count)
        {
            m_currentIndex = 0;
        }
        m_currentPageItem.anchoredPosition = m_pageItem[m_currentIndex].anchoredPosition;
        foreach (var i in m_items)
        {
            if (slide)//슬라이드 여부에 따라 페이지 속도 상이하게 둠
                i.SlideNext(m_slideSpeed);
            else
                i.MoveNext();
            i.UpdateIndex(m_currentIndex, m_dataList.Count - 1, m_dataList[m_currentIndex]);
        }
    }

    private void BeforePage(bool slide)
    {
        if (scrolling || m_dataList.Count <= 1) return;
        m_currentIndex--;
        if (m_currentIndex < 0)
        {
            m_currentIndex = Mathf.Max(0, m_pageItem.Count - 1);
        }
        m_currentPageItem.anchoredPosition = m_pageItem[m_currentIndex].anchoredPosition;
        foreach (var i in m_items)
        {
            if (slide)
                i.SlidBeforePage(m_slideSpeed);
            else
                i.MoveBeforePage();
            i.UpdateIndex(m_currentIndex, m_dataList.Count - 1, m_dataList[m_currentIndex]);
        }
    }
}
