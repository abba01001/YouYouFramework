using Fungus;
using Unity.VisualScripting;
using UnityEngine;
using YouYou;

public class DialogueCreator : MonoBehaviour
{
    public Flowchart flowchart;

    void Start()
    {
        return;
        //CreateDialogue("Hello, World!", "This is a sample dialogue.");
        Sprite sprite = Resources.Load<Sprite>("calling-neutral");
        //Debug.LogError(sprite.name);
        GameEntry.Dialogue.AddSayCommand("对话组件1", "你好呀,\n我的朋友,\n今天过的怎么样？\n开心还是难过？",GameEntry.Dialogue.GetCharacterById(1),sprite);
        GameEntry.Dialogue.AddWaitCommand("对话组件1",5f);
        GameEntry.Dialogue.AddSayCommand("对话组件1").SetStandardText("你好，你叫啥");
        GameEntry.Dialogue.AddSayCommand("对话组件1").SetStandardText("我叫李明，你呢");
        GameEntry.Dialogue.AddSayCommand("对话组件4").SetStandardText("我是小红，很高心认识你");
        GameEntry.Dialogue.AddJumpCommand("对话组件1", "对话组件3");
        GameEntry.Dialogue.AddJumpCommand("对话组件1", "对话组件4");
        GameEntry.Dialogue.AddJumpCommand("对话组件1", "对话组件5");
        GameEntry.Dialogue.AddJumpCommand("对话组件1", "对话组件6");
        GameEntry.Dialogue.AddJumpCommand("对话组件1", "对话组件7");
        GameEntry.Dialogue.AddJumpCommand("对话组件1", "对话组件8");
        GameEntry.Dialogue.AddJumpCommand("对话组件8", "对话组件9");
        GameEntry.Dialogue.AddSayCommand("对话组件10").SetStandardText("你好，你叫啥");
        GameEntry.Dialogue.AddSayCommand("对话组件11").SetStandardText("你好，你叫啥");
        GameEntry.Dialogue.PrintGraph();
        GameEntry.Dialogue.EnableBlock("对话组件1");
    }

    void CreateDialogue(string characterName, string dialogueText)
    {
        // Create a new Block
        var block = flowchart.CreateBlock(Vector2.zero);
        var block1 = flowchart.CreateBlock(new Vector2(100, 0));
        Say say = block.AddComponent<Say>();

        Menu menu = block.AddComponent<Menu>();
        menu.SetStandardText("按钮测试");
        menu.SetTargetBlock(block1);

        block.InsertCommand(say);
        block.InsertCommand(menu);
        
        // Link block to start
        flowchart.AddSelectedBlock(block1);
        flowchart.AddSelectedBlock(block);
        flowchart.ExecuteBlock(block);
        //
        Debug.LogError(flowchart.SelectedBlock.State);
    }

}