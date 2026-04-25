using Sirenix.OdinInspector;

namespace OctoberStudio
{
    using Sirenix.OdinInspector; // 必须加这个

    public enum DropType
    {
        [LabelText("小型宝石")] SmallGem = 0,
        [LabelText("中型宝石")] MediumGem = 1,
        [LabelText("大型宝石")] BigGem = 2,
        [LabelText("巨型宝石")] LargeGem = 3,
        [LabelText("磁铁")] Magnet = 10,
        [LabelText("炸弹")] Bomb = 20,
        [LabelText("食物")] Food = 30,
        [LabelText("金币")] Coin = 40,
        [LabelText("寂冻时环")] TimeStop = 50,
        [LabelText("宝箱")] Chest = 100,
    }
}