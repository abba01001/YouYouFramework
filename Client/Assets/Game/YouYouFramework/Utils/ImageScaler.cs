using UnityEngine;

public class ImageScaler : MonoBehaviour
{
    public RectTransform imageRectTransform;

    // 参考分辨率
    private float referenceWidth = 1920f;
    private float referenceHeight = 1080f;

    void Start()
    {
        // 调整图片的缩放
        AdjustImageScale();
    }

    void AdjustImageScale()
    {
        // 获取当前屏幕的分辨率
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 计算水平和垂直的缩放因子
        float scaleX = screenWidth / referenceWidth;
        float scaleY = screenHeight / referenceHeight;

        // 选择更合适的缩放比例来避免拉伸
        float scaleFactor = Mathf.Min(scaleX, scaleY);

        // 调整RectTransform的localScale
        imageRectTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }
}