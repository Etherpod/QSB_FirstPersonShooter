using OWML.Common;
using OWML.ModHelper;
using System;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using QSB.RespawnSync;
using System.Linq;

namespace QSBFPS;

public class qsbFPS : ModBehaviour
{
    public static qsbFPS Instance;
    public static IQSBAPI qsbAPI;
    public static IMenuAPI menuFrameworkAPI;
    public static bool inSolarSystem = false;

    public bool disableProbeLauncher = true;
    public bool customSpawn = true;

    GameObject scriptHandler;
    Transform particlesOffset;
    Canvas overlayHUD;
    Image hitReticle;
    GameObject gunObject;

    public Dictionary<uint, GameObject> idToGameObjects = new Dictionary<uint, GameObject>();
    public GameObject lastJoinedObject;
    public uint lastJoinedID;

    private void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        qsbAPI = ModHelper.Interaction.TryGetModApi<IQSBAPI>("Raicuparta.QuantumSpaceBuddies");
        menuFrameworkAPI = ModHelper.Interaction.TryGetModApi<IMenuAPI>("_nebula.MenuFramework");
        qsbAPI.RegisterRequiredForAllPlayers(this);

        LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
        {
            if (loadScene != OWScene.SolarSystem) return;
            OnSystemLoad();
        };
    }

    private void OnSystemLoad()
    {
        inSolarSystem = true;

        scriptHandler = Instantiate(new GameObject("Script Handler"), Vector3.zero, Quaternion.identity);
        scriptHandler.AddComponent<GunController>();

        qsbAPI.RegisterHandler<int>("deal-damage", MessageHandler);

        GameObject prefab = AssetBundleUtilities.LoadPrefab("Assets/qsbfps", "Assets/qsbFPS/Particles Offset Parent.prefab", this);
        particlesOffset = Instantiate(prefab, Vector3.zero, Quaternion.identity).transform;
        particlesOffset.gameObject.SetActive(true);
        scriptHandler.GetComponent<GunController>().particlesOffset = particlesOffset;

        QSB.QSBCore.DebugSettings.DisableLoopDeath = true;

        qsbAPI.OnPlayerJoin().AddListener((uint playerID) =>
        {
            ModHelper.Console.WriteLine($"{playerID} joined the game!", MessageType.Success);
            //StartCoroutine(WaitForLocalInstance(playerID));
        });

        qsbAPI.OnPlayerLeave().AddListener((uint playerId) => ModHelper.Console.WriteLine($"{playerId} left the game!", MessageType.Success));

        SpawnArena();
        SpawnGunHUD();
        StartCoroutine(EquipSuitDelay());
    }

    private IEnumerator WaitForLocalInstance(uint playerID)
    {
        ModHelper.Console.WriteLine("Waiting for local instance...");
        yield return new WaitUntil(() => QSB.Player.TransformSync.PlayerTransformSync.LocalInstance != null);

        ModHelper.Console.WriteLine("Local instance found!");

        if (playerID == qsbAPI.GetLocalPlayerID())
        {
            ModHelper.Console.WriteLine($"It was just me ({playerID}) who joined... :(");
            yield break;
        }

        ModHelper.Console.WriteLine("New player joined! ID: " + playerID);
       /* new UpdateDictMessage(playerID, new KeyValuePair<uint, GameObject>(qsbAPI.GetLocalPlayerID(),
            idToGameObjects[qsbAPI.GetLocalPlayerID()])).Send();*/

        //lastJoinedID = playerID;
        //StartCoroutine(RegisterIdObjectPair());
    }

    private IEnumerator RegisterIdObjectPair()
    {
        ModHelper.Console.WriteLine("Saved playerID");

        if (lastJoinedObject == null)
        {
            ModHelper.Console.WriteLine("Player object is null");
            yield return new WaitUntil(() => lastJoinedObject != null);
        }

        idToGameObjects.Add(lastJoinedID, lastJoinedObject);
        ModHelper.Console.WriteLine("ID-object added");
        lastJoinedObject = null;
    }

    private IEnumerator EquipSuitDelay()
    {
        yield return new WaitForSeconds(2f);
        Locator.GetPlayerSuit().SuitUp(false, false, true);
    }

    public IEnumerator RespawnDelay()
    {
        FieldInfo field = typeof(RespawnManager).GetField("_playersPendingRespawn", BindingFlags.NonPublic | BindingFlags.Instance);
        List<QSB.Player.PlayerInfo> _playersPendingRespawn = (List<QSB.Player.PlayerInfo>)field.GetValue(RespawnManager.Instance);

        if (_playersPendingRespawn == null) 
        {
            ModHelper.Console.WriteLine("Didn't find the list somehow?", MessageType.Error);
            yield break;
        }

        yield return new WaitUntil(() => qsbAPI.GetPlayerIDs().Count() - _playersPendingRespawn.Count <= 1);
        yield return new WaitForSeconds(5f);
        RespawnManager.Instance.RespawnSomePlayer();
    }

    private void SpawnArena()
    {
        GameObject prefabArena = AssetBundleUtilities.LoadPrefab("Assets/qsbfps", "Assets/qsbFPS/OW_PvpArena.prefab", this);
        GameObject instantiatedArena = Instantiate(prefabArena, new Vector3(10000, 10000, 0), Quaternion.identity);
        instantiatedArena.SetActive(true);

        GameObject respawner = instantiatedArena.GetComponentInChildren<MultiInteractReceiver>().gameObject;
        respawner.AddComponent<ShipRecoveryPoint>();
    }

    private void SpawnGunHUD()
    {
        GameObject prefabHUD = AssetBundleUtilities.LoadPrefab("Assets/qsbfps", "Assets/qsbFPS/GunHUD.prefab", this);
        overlayHUD = Instantiate(prefabHUD, Vector3.zero, Quaternion.identity).GetComponent<Canvas>();
        hitReticle = overlayHUD.transform.GetChild(0).GetComponent<Image>();
        scriptHandler.GetComponent<GunController>().hitReticle = hitReticle;

        overlayHUD.gameObject.SetActive(true);
        hitReticle.enabled = false;

        GameObject prefabGun = AssetBundleUtilities.LoadPrefab("Assets/qsbfps", "Assets/qsbFPS/GunPivot.prefab", this);
        PlayerCameraController playerCam = FindObjectOfType<PlayerCameraController>();
        gunObject = Instantiate(prefabGun, playerCam.transform.position, playerCam.transform.rotation, playerCam.transform);
        scriptHandler.GetComponent<GunController>().gunObject = gunObject;
        scriptHandler.GetComponent<GunController>().gunFirePoint = gunObject.transform.GetChild(1).gameObject;

        gunObject.SetActive(true);
    }

    public void DealDamage(int damage)
    {
        PlayerResources pr = FindObjectOfType<PlayerResources>();
        pr.ApplyInstantDamage(damage, InstantDamageType.Impact);
    }

    private void MessageHandler<T>(uint from, T data)
    {
        if (data is int)
        {
            int damage = Convert.ToInt32(data);
            ModHelper.Console.WriteLine("Damage recieved: " + damage, MessageType.Success);
            PlayerResources pr = FindObjectOfType<PlayerResources>();
            pr.ApplyInstantDamage(damage, InstantDamageType.Impact);
        }
    }
}

