using OWML.Common;
using OWML.ModHelper;
using System;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Collections;
using UnityEngine.UI;

namespace qsbFPS;

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

    private void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        Initialize();

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

        GameObject prefab = AssetBundleUtilities.LoadPrefab("Assets/qsbfps", "Assets/qsbFPS/Particles Offset Parent.prefab", this);
        particlesOffset = Instantiate(prefab, Vector3.zero, Quaternion.identity).transform;
        particlesOffset.gameObject.SetActive(true);
        scriptHandler.GetComponent<GunController>().particlesOffset = particlesOffset;

        qsbAPI.RegisterHandler<int>("deal-damage", MessageHandler);

        SpawnArena();
        SpawnGunHUD();
        StartCoroutine(EquipSuitDelay());
    }

    private void Update()
    {

    }

    private IEnumerator EquipSuitDelay()
    {
        yield return new WaitForSeconds(2f);
        FindObjectOfType<PlayerSpacesuit>().SuitUp(false, false, true);
    }

    private void SpawnArena()
    {
        GameObject prefabArena = AssetBundleUtilities.LoadPrefab("Assets/qsbfps", "Assets/qsbFPS/OW_PvpArena.prefab", this);
        GameObject instantiatedArena = Instantiate(prefabArena, new Vector3(10000, 10000, 0), Quaternion.identity);
        instantiatedArena.SetActive(true);
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

    private void Initialize()
    {
        qsbAPI = ModHelper.Interaction.TryGetModApi<IQSBAPI>("Raicuparta.QuantumSpaceBuddies");
        menuFrameworkAPI = ModHelper.Interaction.TryGetModApi<IMenuAPI>("_nebula.MenuFramework");

        LoadManager.OnCompleteSceneLoad += (oldScene, newScene) =>
        {
            if (newScene != OWScene.SolarSystem)
            {
                return;
            }

            qsbAPI.OnPlayerJoin().AddListener((uint playerId) => ModHelper.Console.WriteLine($"{playerId} joined the game!", MessageType.Success));
            qsbAPI.OnPlayerLeave().AddListener((uint playerId) => ModHelper.Console.WriteLine($"{playerId} left the game!", MessageType.Success));
        };
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

