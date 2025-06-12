using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScrollBannerItem<T> : CacheObject
{//배너아이템 정보

    //현재 배너의 위치
    public enum BannerPosition
    {
        None = -1,
        FirstPos,
        Center,
        LastPos,
        Reset
    }

    //1,2,3번 위치, 속도, 페이지 인덱스 ( 배너 스크롤에서 설정해주는 값 )
    private Vector2 m_firstPos;
    private Vector2 m_lastPos;
    private Vector2 m_centerPos;
    private BannerPosition m_currentPosition;
    protected int m_pageIndex = 0;
    private float m_moveSpeed = 0.5f;

    private UnityAction<int> m_clickAction;
    private Func<int, T> getDataAction;
    [SerializeField] private Button m_btn;
    protected T m_currentData;
    private bool m_onSlide = false;

    public void Awake()
    {
        m_btn.onClick.AddListener(OnClickAction);
    }

    public virtual void InitBannerItem(int centerPageIndex, int Max, T data)
    {
        if (m_currentPosition == BannerPosition.Center)
            m_pageIndex = centerPageIndex;
        else if (m_currentPosition == BannerPosition.FirstPos)
            m_pageIndex = Max < centerPageIndex + 1 ? 0 : centerPageIndex + 1;
        else if (m_currentPosition == BannerPosition.LastPos)
            m_pageIndex = 1 > centerPageIndex ? Max : centerPageIndex - 1;

        if(getDataAction != null) 
            m_currentData = getDataAction.Invoke(m_pageIndex);
        UpdateBanner();
    }

    public void SetPosition(int index, Vector2 first, Vector2 last, Vector2 center, float moveSpeed)
    {
        m_pageIndex = index;
        m_currentPosition = index == 0 ? BannerPosition.LastPos : index == 1 ? BannerPosition.Center : BannerPosition.FirstPos;
        m_firstPos = first;
        m_lastPos = last; 
        m_centerPos = center;
        m_moveSpeed = moveSpeed;
    }
    public void SetSpeed(float slideSpeed)
    {
        m_moveSpeed = slideSpeed;
    }
    private void MoveBanner(BannerPosition position, float moveSpeed)
    {
        switch (position)
        {
            case BannerPosition.FirstPos:
                MoveLeft(moveSpeed);
                break;
            case BannerPosition.LastPos:
                MoveRight(moveSpeed);
                break;
            case BannerPosition.Center:
                MoveCenter(moveSpeed);
                break;
        }
    }

    private void MoveRight(float moveSpeed) => DoMove(m_lastPos, moveSpeed);
    private void MoveCenter(float moveSpeed) => DoMove(m_centerPos, moveSpeed);

    private void MoveLeft(float moveSpeed) => DoMove(m_firstPos, moveSpeed);

    private void DoMove(Vector2 Pos, float moveSpeed)
    {
        gameObject.SetActive(true);
        MyRT.DOKill();
        MyRT.DOAnchorPos(Pos, moveSpeed).OnComplete((ScrollEndEvent)).OnKill(DoKillCon);
    }

    public void MoveNext()
    {
        if(CheckNext())
            MoveBanner(m_currentPosition, m_moveSpeed);
    }

    public void MoveBeforePage()
    {
        if(CheckBefore())
            MoveBanner(m_currentPosition, m_moveSpeed);
    }

    public virtual void UpdateIndex(int centerPageIndex, int Max, T data)
    {
        if (m_currentPosition == BannerPosition.Center)
            m_pageIndex = centerPageIndex;
        else if (m_currentPosition == BannerPosition.FirstPos)
            m_pageIndex = Max < centerPageIndex + 1 ? 0 : centerPageIndex + 1;
        else if (m_currentPosition == BannerPosition.LastPos)
            m_pageIndex = 1 > centerPageIndex ? Max : centerPageIndex-1;

        if (getDataAction != null)
            m_currentData = getDataAction.Invoke(m_pageIndex);
        UpdateBanner();
    }

    public void SetAction(UnityAction<int> action, Func<int, T> data)
    {
        m_clickAction = action;
        getDataAction = data;
    }

    public void OnClickAction()
    {
        if(m_clickAction != null)
        {
            m_clickAction.Invoke(m_pageIndex);
        }
    }

    private void OnDestroy()
    {
        if(MyRT != null)
            MyRT.DOKill();
    }

    public void SlideNext(float speed)
    {
        m_onSlide = false;
        if (CheckNext())
            MoveBanner(m_currentPosition, speed);
    }

    public void SlidBeforePage(float speed)
    {
        m_onSlide = false; 
        if (CheckBefore())
            MoveBanner(m_currentPosition, speed);
    }

    public void OnSlide()
    {
        m_onSlide = true;
        MyRT.DOKill();

        if (getDataAction != null)
            m_currentData = getDataAction.Invoke(m_pageIndex);
        UpdateBanner();
    }

    public void MoveCurrent()
    {
        m_onSlide = false;
        gameObject.SetActive(m_currentPosition == BannerPosition.Center);
        MoveBanner(m_currentPosition, 0.1f);
    }

    protected virtual void ScrollEndEvent()
    {
        gameObject.SetActive(BannerPosition.Center == m_currentPosition);
    }

    protected virtual void UpdateBanner() { }

    //다음으로 이동하는데 Reset(마지막에서 스크롤링됨)일 경우 첫번째 위치로 이동 시켜주고
    //중앙으로 가는 아이템인 경우 배너를 업데이트 해줍니다.
    private bool CheckNext()
    {
        m_currentPosition = (BannerPosition)((int)m_currentPosition + 1);
        if (m_currentPosition == BannerPosition.Reset)
        {
            gameObject.SetActive(false);
            m_currentPosition = BannerPosition.FirstPos;
            MyRT.anchoredPosition = m_firstPos;
            return false;
        }
        if (m_currentPosition != BannerPosition.LastPos)
        {
            if (getDataAction != null)
                m_currentData = getDataAction.Invoke(m_pageIndex);
            UpdateBanner();
        }
        return true;
    }

    //이전으로 이동하는데 해당 아이템이 None(첫번째에서 뒤로 스크롤링됨) 인 경우 리스트의 마지막 위치로 이동시켜주고
    //중앙으로 가는 아이템인 경우 배너를 업데이트 해줍니다.
    private bool CheckBefore()
    {
        m_currentPosition = (BannerPosition)((int)m_currentPosition - 1);
        if (m_currentPosition == BannerPosition.None)
        {
            gameObject.SetActive(false);
            m_currentPosition = BannerPosition.LastPos;
            MyRT.anchoredPosition = m_lastPos;

            return false;
        }

        if (m_currentPosition != BannerPosition.FirstPos)
        {
            if (getDataAction != null)
                m_currentData = getDataAction.Invoke(m_pageIndex);
            UpdateBanner();
        }
        return true;
    }

    //DoTween이 완료되기 전에 Kill 당할경우 위치를 재정렬 시켜줍니다.
    private void DoKillCon()
    {
        if (m_onSlide) return;

        switch (m_currentPosition)
        {
            case BannerPosition.FirstPos:
                MyRT.anchoredPosition = m_firstPos;
                break;
            case BannerPosition.Center:
                MyRT.anchoredPosition = m_centerPos;
                break;
            case BannerPosition.LastPos:
                MyRT.anchoredPosition = m_lastPos;
                break;
        }

    }

    //3개의 배너를 전부 그려주는게 아닌 중앙에 있는 배너만 그려줍니다.
    //단 스크롤링 될때는 3개 전부 그려줍니다.
    public void BannerSetting(int index)
    {
        if ((BannerPosition)index == BannerPosition.Center)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }
}
