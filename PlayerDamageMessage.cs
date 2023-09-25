using UnityEngine;
using QSB.Messaging;
using QSBFPS;

namespace QSB.Player.Messages;

public class PlayerDamageMessage : QSBMessage<(uint playerID, int damage)>
{
    public PlayerDamageMessage(uint playerID, int damage) : base((playerID, damage)) { }

    public override void OnReceiveLocal() => OnReceiveRemote();

    public override void OnReceiveRemote()
    {
        qsbFPS.Instance.ModHelper.Console.WriteLine($"Player ID: {Data.playerID}, Damage: {Data.damage}", OWML.Common.MessageType.Success);

        if (Data.playerID == QSBPlayerManager.LocalPlayerId)
        {
            qsbFPS.Instance.DealDamage(Data.damage);
        }
    }
}