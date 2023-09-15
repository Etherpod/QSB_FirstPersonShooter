using UnityEngine;
using OWML.Common;
using UnityEngine.UI;

namespace qsbFPS;

public class GunController : MonoBehaviour
{
    bool fadeReticle = false;
    bool cooldownFire = false;
    bool canFire = true;
    bool autoFire;

    float currentReticleAlpha;
    float reticleFadeTime = 0.75f;
    float fireCooldownTimer;
    float fireCooldown = 0.2f;
    int damage;
    float stationaryAccuracyVar = 0.015f;
    float movingAccuracyVar = 0.03f;
    float autoAccuracyVarMult = 2f;

    public Image hitReticle;
    public GameObject gunObject;
    public GameObject gunFirePoint;
    public Transform particlesOffset;

    private void Update()
    {
        //Detect button press
        if (qsbFPS.inSolarSystem && canFire)
        {
            if (OWInput.IsNewlyPressed(InputLibrary.lockOn, InputMode.All))
            {
                damage = 20;
                fireCooldown = 0.2f;
                autoFire = false;
                FireRaycast();
            }
            else if (OWInput.IsPressed(InputLibrary.probeLaunch, InputMode.All))
            {
                damage = 8;
                fireCooldown = 0.08f;
                autoFire = true;
                FireRaycast();
            }
        }
        //Fade out hit marker
        if (fadeReticle)
        {
            currentReticleAlpha -= Time.deltaTime / reticleFadeTime;
            hitReticle.color = new Color(hitReticle.color.r, hitReticle.color.g, hitReticle.color.b, currentReticleAlpha);

            if (hitReticle.color.a <= 0)
            {
                qsbFPS.Instance.ModHelper.Console.WriteLine("Reticle disabled");
                hitReticle.enabled = false;
                fadeReticle = false;
            }
        }
        //Check if cooldown is finished
        if (cooldownFire)
        {
            if (Time.time - fireCooldownTimer > fireCooldown)
            {
                canFire = true;
                cooldownFire = false;
            }
        }
    }

    private void FireRaycast()
    {
        float raycastDist = 200f;
        PlayerCameraController player = FindObjectOfType<PlayerCameraController>();

        gunObject.GetComponentInChildren<Animator>().SetTrigger("fire");
        StartCooldown();

        Vector2 vector = OWInput.GetAxisValue(InputLibrary.moveXZ, InputMode.Character | InputMode.NomaiRemoteCam);
        float variable;

        if (vector.magnitude > 0.5f)
        {
            variable = movingAccuracyVar;
        }
        else
        {
            variable = stationaryAccuracyVar;
        }

        Vector3 randomVector = Random.insideUnitSphere * variable;

        if (autoFire)
        {
            randomVector *= autoAccuracyVarMult;
        }

        GameObject fireEffect = AssetBundleUtilities.LoadPrefab("Assets/qsbfps", "Assets/qsbFPS/GunFireEffect.prefab", qsbFPS.Instance);
        GameObject instantiatedFireEffect = Instantiate(fireEffect, gunFirePoint.transform.position, 
            Quaternion.LookRotation(gunObject.transform.GetChild(0).forward), gunObject.transform);
        instantiatedFireEffect.SetActive(true);

        Physics.queriesHitTriggers = false;
        if (Physics.Raycast(player.transform.position, player.transform.forward + randomVector, out RaycastHit hit, raycastDist))
        {
            Physics.queriesHitTriggers = true;

            if (!hit.collider.GetComponentInParent<PlayerCharacterController>() && hit.collider.gameObject.name == "REMOTE_PlayerDetector")
            {
                qsbFPS.Instance.ModHelper.Console.WriteLine("Shot hit another player!", MessageType.Success);
                qsbFPS.qsbAPI.SendMessage("deal-damage", damage, receiveLocally: false);

                GameObject prefab = AssetBundleUtilities.LoadPrefab("Assets/qsbfps", "Assets/qsbFPS/GunPlayerHitEffect.prefab", qsbFPS.Instance);
                GameObject instantiated = Instantiate(prefab, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), particlesOffset);
                instantiated.SetActive(true);

                ReticleFade();

                return;
            }
            else
            {
                /*qsbFPS.Instance.ModHelper.Console.WriteLine("Shot didn't hit a player", MessageType.Info);
                qsbFPS.Instance.ModHelper.Console.WriteLine("Shot reciever is my own character: " +
                    (hit.collider.GetComponentInParent<PlayerCharacterController>() != null));
                qsbFPS.Instance.ModHelper.Console.WriteLine("Shot reciever has a player detector: " +
                    (hit.collider.gameObject.name == "REMOTE_PlayerDetector"));*/
            }

            if (hit.collider != null)
            {
                GameObject prefab = AssetBundleUtilities.LoadPrefab("Assets/qsbfps", "Assets/qsbFPS/GunGroundHitEffect.prefab", qsbFPS.Instance);
                GameObject instantiated = Instantiate(prefab, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), particlesOffset);
                instantiated.SetActive(true);

                //ReticleFade();
            }
        }
    }

    private void ReticleFade()
    {
        qsbFPS.Instance.ModHelper.Console.WriteLine("Reticle enabled");
        qsbFPS.Instance.ModHelper.Console.WriteLine("Reticle object: " + hitReticle);
        hitReticle.color = new Color(hitReticle.color.r, hitReticle.color.g, hitReticle.color.b, 1f);
        currentReticleAlpha = 1f;
        hitReticle.enabled = true;
        fadeReticle = true;
    }

    private void StartCooldown()
    {
        fireCooldownTimer = Time.time;
        canFire = false;
        cooldownFire = true;
    }
}
