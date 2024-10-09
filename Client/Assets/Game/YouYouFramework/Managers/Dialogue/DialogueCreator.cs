using Fungus;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueCreator : MonoBehaviour
{
    public Flowchart flowchart;

    void Start()
    {
        //CreateDialogue("Hello, World!", "This is a sample dialogue.");
        Sprite sprite = Resources.Load<Sprite>("calling-neutral");
        Debug.LogError(sprite.name);
        DialogueMgr.Instance.CreateSayCommand("对话组件1", "你好呀,\n我的朋友,\n今天过的怎么样？\n开心还是难过？",DialogueMgr.Instance.GetCharacterById(1),sprite);
        DialogueMgr.Instance.AddWaitCommand("对话组件1",5f);
        DialogueMgr.Instance.CreateSayCommand("对话组件1").SetStandardText("你好，你叫啥");
        DialogueMgr.Instance.CreateSayCommand("对话组件1").SetStandardText("我叫李明，你呢");
        DialogueMgr.Instance.CreateSayCommand("对话组件4").SetStandardText("我是小红，很高心认识你");
        DialogueMgr.Instance.CreateCallCommand("对话组件1", "对话组件3");
        DialogueMgr.Instance.CreateCallCommand("对话组件1", "对话组件4");
        DialogueMgr.Instance.CreateCallCommand("对话组件1", "对话组件5");
        DialogueMgr.Instance.CreateCallCommand("对话组件1", "对话组件6");
        DialogueMgr.Instance.CreateCallCommand("对话组件1", "对话组件7");
        DialogueMgr.Instance.CreateCallCommand("对话组件1", "对话组件8");
        DialogueMgr.Instance.CreateCallCommand("对话组件8", "对话组件9");
        DialogueMgr.Instance.CreateSayCommand("对话组件10").SetStandardText("你好，你叫啥");
        DialogueMgr.Instance.CreateSayCommand("对话组件11").SetStandardText("你好，你叫啥");
        DialogueMgr.Instance.PrintGraph();
        DialogueMgr.Instance.ExcuteBlock("对话组件1");
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