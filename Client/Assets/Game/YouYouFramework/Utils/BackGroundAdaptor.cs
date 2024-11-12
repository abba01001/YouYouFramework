using UnityEngine;
using UnityEngine.UI;
using System;

public class BackgroundAdaptor : MonoBehaviour
{
    public enum ModeEnum {
        SameRate = 0,
        ScaleWidth =1
    } //1等比例，2拉伸

    public ModeEnum mode = ModeEnum.SameRate;
    // void Awake()
    // {
    //     float r1 = Screen.width / (float)Screen.height;
    //     float r2 = Constants.ResolutionWidth / (float)Constants.ResolutionHeight;
    //     if (r1 > r2)
    //     {
    //         Image img = GetComponent<Image>();
    //         RawImage rawImg = GetComponent<RawImage>();
    //         SpriteRenderer sprite = GetComponent<SpriteRenderer>();
    //         if (mode ==  ModeEnum.SameRate)
    //         {
    //             if (img != null)
    //             {
    //                 img.transform.localScale = Vector3.one * (r1 / r2);
    //             }
    //             else if (rawImg != null)
    //             {
    //                 rawImg.transform.localScale = Vector3.one * (r1 / r2);
    //             }
    //             else if (sprite != null)
    //             {
    //                 sprite.transform.localScale = Vector3.one * (r1 / r2);
    //             }
    //         }
    //         else
    //         {
    //             if (img != null)
    //             {
    //                 img.transform.localScale = new Vector3(r1 / r2, 1, 1);
    //             }
    //             else if (rawImg != null)
    //             {
    //                 rawImg.transform.localScale = new Vector3(r1 / r2, 1, 1);
    //             }
    //             else if (sprite != null)
    //             {
    //                 sprite.transform.localScale = new Vector3(r1 / r2, 1, 1);
    //             }
    //         }
    //     }
    // }
}