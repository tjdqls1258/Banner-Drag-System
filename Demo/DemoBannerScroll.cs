using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoBannerData
{
    public int demoID;
}

public class DemoBannerScroll : BannerScroll<DemoBannerData>
{
    Vector2 m_posPageItem = new Vector2(30, -18);
    Vector2 m_posPageItemL = new Vector2(30, 0);
    protected override Vector2 GetPageItemPos => m_posPageItem;
    protected override Vector2 GetPageItemPosLayout => m_posPageItemL;

    public override void OnClickBanner(int index)
    {
        Debug.Log($"OnClick Demo ID {index}");
    }

    public override void UpdateBannerItem(List<DemoBannerData> bannerItem)
    {
        base.UpdateBannerItem(bannerItem);
    }

}
