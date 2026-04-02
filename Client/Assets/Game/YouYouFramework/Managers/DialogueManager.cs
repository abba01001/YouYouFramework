using System;
using System.Collections.Generic;
using Fungus;
using Main;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;


public enum DialogueConfigId
{
    FirstEntryGame = 1
}

public class DialogueManager
{
    #region 变量
    private Flowchart Flowchart { get; set; }
    private GameObject CharacterParent;
    private Dictionary<string, List<string>> BlcokBranchDic { get; set; }//分支表
    private Dictionary<int, Character> CharacterDic { get; set; }
    private Dictionary<string, Block> BlockDic { get; set; }
    private int BlockIndex;
    private int BlockPosIndex;
    private bool IsParse = false;
    private bool IsHandleIng = false;
    private Sys_DialogueEntity curDialogueEntity;
    #endregion
    
    public void Init()
    {
        GameObject parent = new GameObject("DiagloueManager");
        GameObject flowchartObject = new GameObject("DynamicFlowchart");
        flowchartObject.transform.SetParent(parent.transform);
        CharacterParent = new GameObject("CharacterParent");
        CharacterParent.transform.SetParent(parent.transform);
        Flowchart = flowchartObject.AddComponent<Flowchart>();
        CharacterDic = new Dictionary<int, Character>();
        BlockDic = new Dictionary<string, Block>();
        BlcokBranchDic = new Dictionary<string, List<string>>();
        // GameEntry.Time.CreateTimerLoop(this, 1f, -1, (int loop) =>
        // {
        //     ParseDialogueTable();
        // });
        GameEntry.Event.AddEventListener(Constants.EventName.TriggerDialogue,OnTriggerDialogue);
    }

    private void OnTriggerDialogue(object user_data)
    {
        dialogueModel = user_data as DialogueModel;
        if (dialogueModel.delay != default)
        {
            Observable.Timer(TimeSpan.FromSeconds(dialogueModel.delay)).Subscribe(_ => ParseDialogueTable());
        }
        else
        {
            ParseDialogueTable();
        }
    }

    //解析对话配置表
    private void ParseDialogueTable()
    {
        if(!Constants.IsLoadDataTable || IsHandleIng || !Constants.IsEntryGame) return;
        if (dialogueModel == null) return;
        if (GameEntry.Data.PlayerRoleData.dialogueIds.Contains(dialogueModel.dialogueId))
        {
            dialogueModel.finishAction?.Invoke();
            return;
        }
        
        foreach (var pair in GameEntry.DataTable.Sys_DialogueDBModel.IdByDic)
        {
            if (dialogueModel.dialogueId == pair.Value.DialogueId)
            {
                HandleDialogue(pair.Value);
            }
        }
    }

    private DialogueModel dialogueModel = null;

    private void HandleDialogue(Sys_DialogueEntity entity)
    {
        curDialogueEntity = entity;
        IsHandleIng = true;

        List<DialogueCommand> commands = JsonConvert.DeserializeObject<List<DialogueCommand>>(entity.Content);
        foreach (var command in commands)
        {
            switch (command.type)
            {
                case "say":
                    // 调用对话命令逻辑
                    AddSayCommand(command.fromBlock, command.text, entity.ClickMode);
                    break;
                case "wait":
                    // 调用等待命令逻辑
                    AddWaitCommand(command.fromBlock, command.duration);
                    break;
                case "jump":
                    // 调用跳转命令逻辑
                    AddJumpCommand(command.fromBlock, command.toBlock);
                    break;
                case "finish":
                    AddFinishDialogueCommand(entity.DisableBlock);
                    break;
            }
        }

        EnableBlock(entity.EnableBlock);
    }
        
    private void OnDialogueComplete()
    {
        //保存数据
        GameEntry.Data.PlayerRoleData.dialogueIds.Add(dialogueModel.dialogueId);
        dialogueModel.finishAction?.Invoke();
        dialogueModel = null;
        IsHandleIng = false;
        RemoveAllBlocks();
    }

    public void OnUpdate()
    {

    }
    
    /// <summary>
    /// 启动区块
    /// </summary>
    /// <param name="blockName">区块名</param>
    public void EnableBlock(string blockName)
    {
        Block block = GetBlockByName(blockName);
        Flowchart.ExecuteBlock(block);
    }

    
    /// <summary>
    /// 对话窗口命令
    /// </summary>
    /// <param name="blockName">创建的区块名</param>
    /// <param name="sayText">对话文本</param>
    /// <param name="character">角色</param>
    /// <param name="sprite">角色精灵</param>
    /// <returns></returns>
    public Say AddSayCommand(string blockName,string sayText = "默认文本",int clickMode = 1,Character character = null,Sprite sprite = null)
    {
        Block block = GetBlockByName(blockName);
        Say say = block.gameObject.AddComponent<Say>();
        say.SetParams(GameEntry.Instance.GetSayItemParent(),clickMode);
        say.SetStandardText(sayText);
        say.SetCharacter(character);
        say.SetCharacterImage(sprite);
        block.InsertCommand(say);
        return say;
    }

    /// <summary>
    /// 等待命令
    /// </summary>
    /// <param name="blockName">区块名</param>
    /// <param name="waitShowTime">等待时间</param>
    public void AddWaitCommand(string blockName,float waitShowTime = 0f)
    {
        Block block = GetBlockByName(blockName);
        Wait wait = block.gameObject.AddComponent<Wait>();
        wait.SetDuration(waitShowTime);
        block.InsertCommand(wait);
    }

    /// <summary>
    /// 跳转到另一个区块命令
    /// </summary>
    /// <param name="fromBlock">源区块名</param>
    /// <param name="toBlock">跳转区块名</param>
    /// <returns></returns>
    public Call AddJumpCommand(string fromBlock,string toBlock)
    {
        Block fBlock = GetBlockByName(fromBlock);
        Block tBlock = GetBlockByName(toBlock);
        Call call = fBlock.gameObject.AddComponent<Call>();
        call.SetTargetBlock(tBlock);
        BlockPosIndex--;
        AddBranch(fBlock.BlockName, tBlock.BlockName);
        tBlock.RefreshBlockPos(GetBlockPos(fBlock));
        fBlock.InsertCommand(call);
        return call;
    }

    /// <summary>
    /// 结束对话回调
    /// </summary>
    /// <param name="fromBlock">源区块名</param>
    public void AddFinishDialogueCommand(string fromBlock)
    {
        Block fBlock = GetBlockByName(fromBlock);
        fBlock.OnBlockComplete = OnDialogueComplete;
    }
    #region block

    //通过name获取block
    public Block GetBlockByName(string name)
    {
        return BlockDic.TryGetValue(name, out Block value) ? value : CreateBlock(name);
    }

    //创建block
    private Block CreateBlock(string name)
    {
        var block = Flowchart.CreateBlock(Vector2.zero + BlockPosIndex * new Vector2(150, 0));
        block.BlockName = name;
        AddToBranchList(name);
        BlockIndex++;
        BlockPosIndex++;
        BlockDic.TryAdd(name, block);
        return block;
    }

    private void RemoveAllBlocks()
    {
        foreach (var pair in BlockDic)
        {
            pair.Value.RemoveAllCommands();
        }
    }
    
    #endregion

    #region Block分支
    private Vector2 GetBlockPos(Block fblock)
    {
        BlcokBranchDic.TryGetValue(fblock.BlockName, out List<string> info);
        Vector2 orignPos = fblock.GetBlockPos();
        return orignPos + new Vector2(150,GetYPosition(info.Count));
    }
    
    private int GetYPosition(int count)
    {
        return 50 * ((count + 1) / 2) * (count % 2 == 0 ? 1 : -1);
    }
    
    private void AddToBranchList(string blockName)
    {
        if (!BlcokBranchDic.ContainsKey(blockName))
        {
            BlcokBranchDic[blockName] = new List<string>();
        }
    }
    
    private void AddBranch(string from, string to)
    {
        if (BlcokBranchDic.ContainsKey(from) && BlcokBranchDic.ContainsKey(to))
        {
            BlcokBranchDic[from].Add(to);
        }
    }

    public void PrintGraph()
    {
        string result = "";
        foreach (var node in BlcokBranchDic)
        {
            result += node.Key + " -> ";
            if (node.Value.Count > 0)
            {
                result += string.Join(", ", node.Value);
            }
            else
            {
                result += "No edges";
            }
            result += " | "; // 分隔符，用于分隔不同的节点
        }
        // 打印最终结果
        GameUtil.LogError(result.TrimEnd(' ', '|')); // 去掉最后一个分隔符
    }
    #endregion
    
    #region 角色

    //通过id获取角色
    public Character GetCharacterById(int id)
    {
        return CharacterDic.TryGetValue(id, out Character value) ? value : CreateCharacter(id);
    }

    //创建角色
    private Character CreateCharacter(int id)
    {
        GameObject characterObj = new GameObject($"Character{id}");
        characterObj.transform.SetParent(CharacterParent.transform);
        Character comp = characterObj.AddComponent<Character>();
        comp.SetStandardText($"Character{id}");
        CharacterDic.TryAdd(id, comp);
        return comp;
    }
    #endregion
}