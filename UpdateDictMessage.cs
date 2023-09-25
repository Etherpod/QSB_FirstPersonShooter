using QSB.Messaging;
using QSBFPS;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.Player.Messages;

public class UpdateDictMessage : QSBMessage<(uint, KeyValuePair<uint, GameObject>)>
{
    public UpdateDictMessage(uint playerID, KeyValuePair<uint, GameObject> dictPair) : base((playerID, dictPair)) { }

    public override void OnReceiveLocal() => OnReceiveRemote();


    public override void OnReceiveRemote()
    {
        qsbFPS.Instance.ModHelper.Console.WriteLine("Recieved dictionary data!");

        if (Data.Item1 == QSBPlayerManager.LocalPlayerId)
        {
            qsbFPS.Instance.idToGameObjects.Add(Data.Item2.Key, Data.Item2.Value);
        }
    }
}