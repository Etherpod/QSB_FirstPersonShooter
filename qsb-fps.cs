using OWML.Common;
using OWML.ModHelper;
using System;
using UnityEngine;

namespace qsbFPS;
public class qsbFPS : ModBehaviour
{
    public static qsbFPS Instance;
    private bool inSolarSystem = false;
    IQSBAPI qsbAPI;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Initialize();

        LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
        {
            if (loadScene != OWScene.SolarSystem) return;
            inSolarSystem = true;
            qsbAPI.RegisterHandler<int>("deal-damage", MessageHandler);
        };
    }

    private void Update()
    {
        if (inSolarSystem && OWInput.IsNewlyPressed(InputLibrary.probeLaunch, InputMode.All))
        {
            FireRaycast();
        }
    }

    /*private void FireProjectile()
    {
        GameObject prefab = AssetBundleUtilities.LoadPrefab("Assets/testbundle", "Assets/TestPrefab.prefab", this);
        PlayerCharacterController player = FindObjectOfType<PlayerCharacterController>();
        GameObject prefabInstance = Instantiate(prefab, player.transform.position + player.transform.forward * 3, Quaternion.identity);
        prefabInstance.SetActive(true);
        prefabInstance.GetComponent<OWRigidbody>().AddImpulse(player.transform.forward * 50);
    }*/

    private void FireRaycast()
    {
        PlayerCameraController player = FindObjectOfType<PlayerCameraController>();
        if (Physics.Raycast(player.transform.position, player.transform.forward, out RaycastHit hit, 50))
        {
            if (!hit.collider.GetComponentInParent<PlayerCharacterController>() && hit.collider.gameObject.name == "REMOTE_PlayerDetector")
            {
                qsbAPI.SendMessage("deal-damage", 10, receiveLocally: false);
            }
            else
            {
                print("Didn't hit a player");
                print("Reciever is my own character:" + hit.collider.GetComponentInParent<PlayerCharacterController>());
                print("Reciever has a player detector:" + hit.collider.gameObject.name == "REMOTE_PlayerDetector");
            }
        }
    }

    private void Initialize()
    {
        qsbAPI = ModHelper.Interaction.TryGetModApi<IQSBAPI>("Raicuparta.QuantumSpaceBuddies");
        var menuFrameworkAPI = ModHelper.Interaction.TryGetModApi<IMenuAPI>("_nebula.MenuFramework");

        LoadManager.OnCompleteSceneLoad += (oldScene, newScene) =>
        {
            if (newScene != OWScene.SolarSystem)
            {
                return;
            }

            //var button = menuFrameworkAPI.PauseMenu_MakeSimpleButton("QSB Api Test");

            qsbAPI.OnPlayerJoin().AddListener((uint playerId) => ModHelper.Console.WriteLine($"{playerId} joined the game!", MessageType.Success));
            qsbAPI.OnPlayerLeave().AddListener((uint playerId) => ModHelper.Console.WriteLine($"{playerId} left the game!", MessageType.Success));

           /* button.onClick.AddListener(() =>
            {
                ModHelper.Console.WriteLine("TESTING QSB API!");

                ModHelper.Console.WriteLine($"Local Player ID : {qsbAPI.GetLocalPlayerID()}");

                ModHelper.Console.WriteLine("Player IDs :");

                foreach (var playerID in qsbAPI.GetPlayerIDs())
                {
                    ModHelper.Console.WriteLine($" - id:{playerID} name:{qsbAPI.GetPlayerName(playerID)}");
                }

                ModHelper.Console.WriteLine("Setting custom data as \"QSB TEST STRING\"");
                qsbAPI.SetCustomData(qsbAPI.GetLocalPlayerID(), "APITEST.TESTSTRING", "QSB TEST STRING");
                ModHelper.Console.WriteLine($"Retreiving custom data : {qsbAPI.GetCustomData<string>(qsbAPI.GetLocalPlayerID(), "APITEST.TESTSTRING")}");

                ModHelper.Console.WriteLine("Sending string message test...");
                qsbAPI.RegisterHandler<string>("apitest-string", MessageHandler);
                qsbAPI.SendMessage("apitest-string", "STRING MESSAGE", receiveLocally: true);

                ModHelper.Console.WriteLine("Sending int message test...");
                qsbAPI.RegisterHandler<int>("apitest-int", MessageHandler);
                qsbAPI.SendMessage("apitest-int", 123, receiveLocally: true);

                ModHelper.Console.WriteLine("Sending float message test...");
                qsbAPI.RegisterHandler<float>("apitest-float", MessageHandler);
                qsbAPI.SendMessage("apitest-float", 3.14f, receiveLocally: true);
            });*/
        };
    }

    private void MessageHandler<T>(uint from, T data)
    {
        if (data is int)
        {
            int damage = Convert.ToInt32(data);
            PlayerResources pr = FindObjectOfType<PlayerResources>();
            pr.ApplyInstantDamage(damage, InstantDamageType.Impact);
        }
    }
}

