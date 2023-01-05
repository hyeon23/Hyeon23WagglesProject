using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    bool isDead = false;
    bool isTab = false;
    
    float bulletPower;
    float curReloadTime;

    public int tabHp;
    public int noHitTime;
    public float maxReloadTime;
    public string type;
    public int spawnIndex;

    public int[] randomAngleArray = { 0, 15, 30, 45, 60, 75, 90, 105, 120, 135, 150, 165, 180 };
    public int[] randomflipX = { -1, 1 };

    public Animator anime;
    public GameManager manager;
    public Transform BulletPos;
    public GameObject bulletPrefab;
    

    void Awake()
    {
        anime = GetComponent<Animator>();
        anime.SetTrigger("doCreate");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isDead)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f);
            if (hit.collider != null)
            {
                if (hit.transform.gameObject == gameObject && !isTab)
                {
                    onTab();
                }
            }
        }

        curReloadTime += Time.deltaTime;

        if (curReloadTime >= maxReloadTime && !isDead && !isTab)
        {
            BeforeFire();
        }
    }

    public void BeforeFire()
    {
        transform.Rotate(Vector3.back, randomflipX[Random.Range(0, 2)] * randomAngleArray[Random.Range(0, 12)]);
        anime.SetTrigger("doShot");

        switch (type)
        {
            case "Doodle":
                StartCoroutine(AnimeShotOffset(1.0f));
                break;
            case "Pack":
                StartCoroutine(AnimeShotOffset(0.5f));
                break;
        }

        curReloadTime = 0;
    }

    public IEnumerator AnimeShotOffset(float time)
    {
        yield return new WaitForSeconds(time);
        Fire();
    }

    public void Fire()
    {
        if (!isTab && !isDead)
        {
            switch (type)
            {
                case "Doodle":
                    bulletPower = Random.Range(5, 9);
                    GameObject doodleBullet = Instantiate(bulletPrefab, BulletPos.position, BulletPos.rotation, manager.bulletGroup);
                    doodleBullet.GetComponent<Rigidbody2D>().AddForce((BulletPos.position - transform.position).normalized * bulletPower, ForceMode2D.Impulse);
                    break;
                case "Pack":
                    StartCoroutine(PackFire());
                    break;
            }
        }
    }

    public IEnumerator PackFire()
    {
        for (int index = 0; index < 5; index++)
        {
            if (isTab)
                break;
            bulletPower = Random.Range(1, 4);
            GameObject packBullet = Instantiate(bulletPrefab, BulletPos.position, BulletPos.rotation, manager.enemyGroup);
            packBullet.GetComponent<Rigidbody2D>().AddForce((BulletPos.position - transform.position).normalized * bulletPower, ForceMode2D.Impulse);
            yield return new WaitForSeconds(0.4f);
        }
    }

    public void onTab()
    {
        tabHp -= 1;
        curReloadTime = 0;

        if (tabHp >= 1)
        {
            isTab = true;
            anime.SetTrigger("doTab");
            StartCoroutine(TabRoutine());
        }
        else
        {
            switch (type)
            {
                case "Doodle":
                    manager.score += 1;
                    break;
                case "Pack":
                    manager.score += 3;
                    break;
            }
            isDead = true;
            anime.SetTrigger("doDie");
            StartCoroutine(DeadRoutine());
        }
    }

    public IEnumerator TabRoutine()//무적시간
    {
        yield return new WaitForSeconds(1f);
        isTab = false;
    }

    public IEnumerator DeadRoutine()
    {
        yield return new WaitForSeconds(1f);
        manager.isSpawned[spawnIndex] = false;
        Destroy(gameObject);
    }
}
