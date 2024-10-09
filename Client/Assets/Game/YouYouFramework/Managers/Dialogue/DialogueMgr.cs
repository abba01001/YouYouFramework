using System.Collections.Generic;
using Fungus;
using UnityEngine;


public class DialogueMgr
{
    private static DialogueMgr _instance;

    public static DialogueMgr Instance
    {
        get { return _instance ??= new DialogueMgr(); }
    }

    private DialogueMgr()
    {
        GameObject flowchartObject = new GameObject("DynamicFlowchart");
        CharacterParent = new GameObject("CharacterParent");
        Flowchart = flowchartObject.AddComponent<Flowchart>();
        CharacterDic = new Dictionary<int, Character>();
        BlockDic = new Dictionary<string, Block>();
        BlcokBranchDic = new Dictionary<string, List<string>>();
    }

    #region 变量

    private Flowchart Flowchart { get; set; }
    private GameObject CharacterParent;
    private Dictionary<string, List<string>> BlcokBranchDic { get; set; }//分支表
    private Dictionary<int, Character> CharacterDic { get; set; }
    private Dictionary<string, Block> BlockDic { get; set; }
    private int BlockIndex;
    private int BlockPosIndex;
    #endregion


    public void ExcuteBlock(string blockName)
    {
        Block block = GetBlockByName(blockName);
        Flowchart.ExecuteBlock(block);
    }

    //说话命令
    public Say CreateSayCommand(string blockName,string sayText = "默认文本",Character character = null,Sprite sprite = null)
    {
        Block block = GetBlockByName(blockName);
        Say say = block.gameObject.AddComponent<Say>();
        say.SetStandardText(sayText);
        say.SetCharacter(character);
        say.SetCharacterImage(sprite);
        block.InsertCommand(say);
        return say;
    }

    //等待命令
    public void AddWaitCommand(string blockName,float waitShowTime = 0f)
    {
        Block block = GetBlockByName(blockName);
        Wait wait = block.gameObject.AddComponent<Wait>();
        wait.SetDuration(waitShowTime);
        block.InsertCommand(wait);
    }

    //跳转到另一个块命令
    public Call CreateCallCommand(string fromBlock,string toBlock)
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
        Debug.LogError(result.TrimEnd(' ', '|')); // 去掉最后一个分隔符
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