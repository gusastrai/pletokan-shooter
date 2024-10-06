using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public Camera playerCamera;

    // Shooting
    public bool isShooting, readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 2f;

    // Spread
    public float spreadIntensity;

    public GameObject bulletPrefab;
    public Transform bulletPoint;
    public float bulletSpeed = 30;
    public float bulletLife = 3f;

    public GameObject muzzleEffect;
    private Animator animator;

    public enum ShootingMode {
        Single
    }

    public ShootingMode currentShootingMode;

    private void Awake() {
        readyToShoot = true;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentShootingMode == ShootingMode.Single) {
            // Clicking left mouse once
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        if (readyToShoot && isShooting) {
            Fire();
        }
    }

    private void Fire() {

        muzzleEffect.GetComponent<ParticleSystem>().Play();
        animator.SetTrigger("FIRE");

        SoundManager.instance.shootingSoundGun.Play();

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        // Instantiating the bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletPoint.position, Quaternion.identity);

        // Positioning the bullet to face the shooting
        bullet.transform.forward = shootingDirection;

        // Shoot the bullet
        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletSpeed, ForceMode.Impulse);

        // Destroy the bullet after a certain time
        StartCoroutine(DestroyBullet(bullet, bulletLife));

        // Checking if we are done shooting
        if (allowReset) {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }

    }

    private void ResetShot() {
        readyToShoot = true;
        allowReset = true;
    }

    public Vector3 CalculateDirectionAndSpread() {
        // Shooting from the middle of the screen where are we pointing at
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); 
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit)) {
            // Hitting something
            targetPoint = hit.point;
        } else {
            // Shooting in the air
            targetPoint = ray.GetPoint(100);
        }

        Vector3 direction = targetPoint - bulletPoint.position;

        float x = Random.Range(-spreadIntensity, spreadIntensity);
        float y = Random.Range(-spreadIntensity, spreadIntensity);

        // Returning the shooting direction and spread
        return direction + new Vector3(x, y, 0);
    }

    private IEnumerator DestroyBullet(GameObject bullet, float bulletLife) {
        yield return new WaitForSeconds(bulletLife);
        Destroy(bullet, bulletLife);
    }
}
