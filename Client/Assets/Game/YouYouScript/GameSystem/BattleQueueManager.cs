using System;
using System.Collections;
using System.Collections.Generic;
using Main;
using YouYou;

public class BattleQueueManager :Singleton<BattleQueueManager>
{
    private Queue<IRenderCommand> renderQueue = new Queue<IRenderCommand>(); // 顺序战斗队列
    private float renderDuration;
    private bool renderBlock;

    // 将战斗动画操作加入顺序队列
    public void AddToSequentialQueue(IRenderCommand command)
    {
        renderQueue.Enqueue(command);
    }

    public void Update()
    {
        while (renderQueue.Count > 0 && !renderBlock)
        {
            IRenderCommand command = renderQueue.Dequeue();
            renderBlock = command.block;
            var duration = 0f;
            switch (command)
            {
                case AttackCommand cmd:
                    GameUtil.LogError("攻击");
                    break;
                default:
                    GameUtil.LogError("未知命令");
                    break;
            }
            renderDuration =  Math.Max(renderDuration, duration);
        }
    }
}


public class IRenderCommand
{
    public bool block = false;
}

public class AttackCommand : IRenderCommand
{
}
