using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using OctoberStudio;

namespace GameScripts
{
    public class DialogueModel
    {
        public int dialogueId;
        public float delay = default;
        public Action finishAction;
    }
    
    public class DialogueCommand
    {
        public string type { get; set; }        // 命令类型 (如 say, wait, jump)
        public string block { get; set; }       // 区块名称 (适用于 say, wait)
        public string text { get; set; }        // 对话文本 (适用于 say)
        public int characterId { get; set; }    // 角色 ID (适用于 say)
        public string sprite { get; set; }      // 角色精灵路径 (适用于 say)
        public float duration { get; set; }     // 等待时间 (适用于 wait)
        public string fromBlock { get; set; }   // 源区块 (适用于 jump)
        public string toBlock { get; set; }     // 目标区块 (适用于 jump)
    }

    public class PropChangeModel
    {
        public PropEnum PropType { get; set; }
        public int PropValue { get; set; }
    }
}