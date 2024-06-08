using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveBotData {
    public BotData botData;
    public BaseBotType baseType;
    public int count;
}

public enum BaseBotType {
    Medium, Tall, Baller
}
