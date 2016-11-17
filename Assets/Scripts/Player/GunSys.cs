using UnityEngine;
using System.Collections;

public class GunSys : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single };
    public FireMode fireMode;
    public Transform[] projectileSpawn;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;
    public int burstCount;

    [Header("Effects")]
    public Transform shell;
    public Transform shellEjection;
    MuzzleFlash muzzleflash;
    float nextShotTime;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;

    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;
    public int magSize;
    int ammoRemainingInMag;
    bool isReloading;
    public float reloadTime = 0.3f;
  //  Vector3 initialRot = transform.localEulerAngles;
    public float maxReloadAngle = 30; // Currently not hooked up <<<<===============================================

    [Header("Recoil")]
    public float kickback = 0.2f;
    public float kickbackReturn = 0.1f; // Time to get back from kickback
    public Vector2 kickMinMax = new Vector2(.05f, .2f); //Kickback
    public Vector2 recoilAngleMinMax = new Vector2(3,5); // Angle up from recoil
    float recoilAngle;
    float recoilRotSmoothDampVelocity;
    Vector3 recoilSmoothDampVelocity;

    void Start()
    {
        muzzleflash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
        ammoRemainingInMag = magSize;
    }

    void LateUpdate() 
    {
        //Animate recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, kickbackReturn);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, 0.1f);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;

        if(!isReloading && ammoRemainingInMag == 0){ // Auto Reload
            Reload();
        }
    }

    void Shoot()
    {

        if (!isReloading && Time.time > nextShotTime && ammoRemainingInMag > 0)
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                {
                    return;
                }
                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }

            for (int i = 0; i < projectileSpawn.Length; i++)
            {
                if(ammoRemainingInMag == 0)
                {
                    break;
                }
                ammoRemainingInMag --;
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectile;
                newProjectile.SetSpeed(muzzleVelocity);
            }

            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleflash.Activate();
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y); // Recoil kickback
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            AudioManager.instance.PlaySound(shootAudio, transform.position);
        }
    }

    public void Reload()
    {
        if (!isReloading && ammoRemainingInMag != magSize)
        {
            Debug.Log("Animate");
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
        }
    }

    IEnumerator AnimateReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(0.2f);

        float reloadSpeed = 1f / reloadTime;
        float percent = 0;
        float maxReloadAngle = 30;
        Vector3 initialRot = transform.localEulerAngles;

        while(percent > 1)
        {
            percent += Time.deltaTime * reloadSpeed;

            //Reload anim
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        ammoRemainingInMag = magSize;
    }

    public void Aim(Vector3 aimpoint)
    {
        if (!isReloading)
        {
            transform.LookAt(aimpoint);
        }
    }


public void OnTriggerHold()
    {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }
}