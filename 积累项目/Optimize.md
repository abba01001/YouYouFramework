
# DrawCall

- 关闭不需要的Raycast选项，降低每帧检测射线的性能消耗，使用Empty4Raycast 代替空的Image接受射线检测，可以减少Drawcall
- 使用Mask会额外增加一个Drawcall,并且会切割子UI不能与其他UI层级合并,矩形使用RectMask2D,圆形和多边形可以自定义mask

# CanvasRebuild

- 需要SetActive的物体，可以缩放scale，移出屏幕外，使用CanvasGroup代替，降低Canvas重建
- Content Size Fitter、VerticalLayoutGroup、HorizontalLayoutGroup、 AspectRatioFitter、GridLayoutGroup组件效率是很低的，它们势必会导致所有元素的Rebuild()执行两次,最好可以考虑自行封装一套布局组件


# OverDrall
- UI边框，sliced模式的sprite节省纹理尺寸，中空的边框不要勾选FillCenter，减低Fill Rate
