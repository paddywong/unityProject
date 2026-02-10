using UnityEngine;
using System.Collections.Generic;

public class BulletPoolShooter : MonoBehaviour
{
    [Header("子弹设置")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float fireRate = 0.1f;
    public int poolSize = 20;

    [Header("枪口火焰效果")]
    public GameObject muzzleFlashPrefab;  // 枪口火焰预制体
    public float muzzleFlashDuration = 0.1f; // 火焰显示时间
    public Vector3 muzzleFlashOffset = Vector3.zero; // 火焰位置偏移
    public Vector3 muzzleFlashRotation = Vector3.zero; // 火焰旋转角度

    [Header("发射设置")]
    public bool isAutomatic = false;
    public string fireButton = "Fire1";

    [Header("声音效果")]
    public AudioClip fireSound;
    public AudioSource audioSource;

    private List<GameObject> bulletPool;
    private float nextFireTime = 0f;
    private GameObject currentMuzzleFlash; // 当前枪口火焰实例

    void Start()
    {
        InitializePool();

        // 如果没有指定AudioSource，尝试获取
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f; // 3D音效
                audioSource.playOnAwake = false;
            }
        }
    }

    void InitializePool()
    {
        bulletPool = new List<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bullet.transform.SetParent(transform);
            bulletPool.Add(bullet);
        }
    }

    void Update()
    {
        if (isAutomatic)
        {
            if (Input.GetButton(fireButton) && Time.time >= nextFireTime)
            {
                ShootFromPool();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            if (Input.GetButtonDown(fireButton))
            {
                ShootFromPool();
            }
        }
    }

    void ShootFromPool()
    {
        GameObject bullet = GetPooledBullet();

        if (bullet != null)
        {
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = firePoint.rotation;
            bullet.SetActive(true);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = firePoint.forward * bulletSpeed;
            }

            // 生成枪口火焰效果
            SpawnMuzzleFlash();

            // 播放射击音效
            PlayFireSound();
        }
    }

    GameObject GetPooledBullet()
    {
        foreach (GameObject bullet in bulletPool)
        {
            if (!bullet.activeInHierarchy)
            {
                return bullet;
            }
        }

        // 如果池子满了，创建新的子弹
        GameObject newBullet = Instantiate(bulletPrefab);
        newBullet.SetActive(false);
        newBullet.transform.SetParent(transform);
        bulletPool.Add(newBullet);
        poolSize++;

        return newBullet;
    }

    void SpawnMuzzleFlash()
    {
        if (muzzleFlashPrefab == null)
        {
            Debug.LogWarning("请指定枪口火焰预制体！");
            return;
        }

        // 计算枪口火焰的位置和旋转
        Vector3 spawnPosition = firePoint.position +
                               firePoint.right * muzzleFlashOffset.x +
                               firePoint.up * muzzleFlashOffset.y +
                               firePoint.forward * muzzleFlashOffset.z;

        Quaternion spawnRotation = firePoint.rotation * Quaternion.Euler(muzzleFlashRotation);

        // 生成枪口火焰
        currentMuzzleFlash = Instantiate(muzzleFlashPrefab, spawnPosition, spawnRotation);

        // 如果火焰有粒子系统，播放它
        ParticleSystem ps = currentMuzzleFlash.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }

        // 定时销毁枪口火焰
        Destroy(currentMuzzleFlash, muzzleFlashDuration);
    }

    void PlayFireSound()
    {
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }

    // 手动触发射击（可从其他脚本调用）
    public void ManualShoot()
    {
        ShootFromPool();
    }

    // 设置射击间隔
    public void SetFireRate(float newRate)
    {
        fireRate = Mathf.Max(0.01f, newRate);
    }

    // 设置子弹速度
    public void SetBulletSpeed(float newSpeed)
    {
        bulletSpeed = Mathf.Max(0.1f, newSpeed);
    }
}