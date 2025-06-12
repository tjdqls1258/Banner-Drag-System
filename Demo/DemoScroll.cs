using System.Collections.Generic;
using UnityEngine;

public class DemoScroll : MonoBehaviour
{
    public DemoBannerScroll scroll;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var data = new List<DemoBannerData>();
        data.Add(new DemoBannerData() { demoID = 1 });
        data.Add(new DemoBannerData() { demoID = 2 });
        data.Add(new DemoBannerData() { demoID = 3 });
        data.Add(new DemoBannerData() { demoID = 4 });
        scroll.UpdateBannerItem(data);
    }
}
