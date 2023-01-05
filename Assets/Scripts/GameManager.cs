using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    [Header("@[Core]")]
    public int score;
    public bool isOver;

    [Header("@[Object Pooling]")]
    public Transform shapesGroup;
    public Transform bulletGroup;
    public Transform enemyGroup;
    public Transform effectGroup;
    public GameObject[] shapesPrefab;
    //public List<Shapes> shapesPool;
    public GameObject[] effectPrefab;
    //public List<ParticleSystem> effectPool;
    [Range(1,30)]//OnDisable �Լ����� ���� ����, Ʈ������, ���� �ʱ�ȭ
    public int poolSize;
    public int poolCursor;
    public Shapes lastShapes;

    [Header("@[Spawn]")]
    public bool[] isSpawned;
    public GameObject[] enemyPrefabs;
    public GameObject[] enemyBulletPrefabs;
    public Transform[] respawnPos;

    [Header("@[Audio]")]
    //Sounds Variables
    int sfxCursor;

    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum SFX { LevelUp, Next, Attach, Button, GameOver };
    

    [Header("@[UI]")]
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;
    public GameObject endGroup;
    public GameObject startGroup;
    public ParticleSystem[] startEffect;

    [Header("@[ETC]")]
    public GameObject[] backGrounds;

    private void Awake()
    {
        StartCoroutine(EffectPlay());
        //������ Frame�� �ε巴�� �ϱ�
        //1. Application.targetFrameRage: ������ FPS�� �����ϴ� �Լ�
        //��� �÷��������� 60���� ����
        Application.targetFrameRate = 60;//Prefab�� interpolate �Ӽ��� none���� interpolate�� �ٲٸ� �������� �ε巴�� ��������

        //shapesPool = new List<Shapes>();
        //effectPool = new List<ParticleSystem>();

        //for(int index = 0; index < poolSize; index++)
        //{
        //    MakeShapes();
        //}
        if (!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }
        maxScoreText.text = "MaxScore: " + PlayerPrefs.GetInt("MaxScore").ToString();
    }

    public IEnumerator EffectPlay()
    {
        foreach (ParticleSystem effect in startEffect)
        {
            effect.Play();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator EffectStop()
    {
        foreach (ParticleSystem effect in startEffect)
        {
            effect.Stop();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SpawnEnemy(float waitTime)
    {
        StartCoroutine(SpawnEnemyLogic(waitTime));
    }

    public IEnumerator SpawnEnemyLogic(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        int randomEnemyIndex = Random.Range(0, 2);
        int randomEnemySpawnPos = Random.Range(0, 25);
        if (isSpawned[randomEnemySpawnPos] == true)
        {
            SpawnEnemy(0.0f);
        }
        else
        {
            isSpawned[randomEnemySpawnPos] = true;
            GameObject instantEnemyObj = Instantiate(enemyPrefabs[randomEnemyIndex], respawnPos[randomEnemySpawnPos].position, respawnPos[randomEnemySpawnPos].rotation, enemyGroup);
            Enemy instantEnemy = instantEnemyObj.GetComponent<Enemy>();
            instantEnemy.spawnIndex = randomEnemySpawnPos;
            instantEnemy.manager = this;
            SpawnEnemy(5.0f);
        }
    }

    public void GameStart()
    {
        StartCoroutine(EffectStop());

        //������Ʈ Ȱ��ȭ
        foreach (GameObject backGround in backGrounds)
            backGround.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();//Audio Source ���
        SfxPlay(SFX.Button);
        Invoke("NextShape", 1.0f);
        SpawnEnemy(5.0f);
    }


    void NextShape()
    {
        if (isOver)
            return;

        //lastShapes = GetShapes();
        lastShapes = MakeShapes();
        lastShapes.gameObject.SetActive(true);
        lastShapes.gameObject.GetComponent<Animator>().SetTrigger("doCreate");

        SfxPlay(GameManager.SFX.Next);
        StartCoroutine(WaitNext());
    }

    IEnumerator WaitNext()
    {
        while (lastShapes != null)
        {
            yield return null;
        }
        yield return new WaitForSeconds(1.0f);

        NextShape();
    }

    Shapes MakeShapes()
    {
        //����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab[Random.Range(0, 6)], effectGroup);
        //instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        //effectPool.Add(instantEffect);

        //Shapes ����
        GameObject instantShapesObj = Instantiate(shapesPrefab[Random.Range(0, 0)], shapesGroup);
        //instantShapesObj.name = "Shapes " + shapesPool.Count;
        Shapes instantShapes = instantShapesObj.GetComponent<Shapes>();
        instantShapes.manager = this;
        instantShapes.effect = instantEffect;
        instantShapes.effectObj = instantEffectObj;

        //shapesPool.Add(instantShapes);

        return instantShapes;
    }

    public IEnumerator LevelUpRoutine2(Vector3 targetPos, int level)
    {
        //����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab[Random.Range(0, 6)], targetPos, Quaternion.identity, effectGroup);
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();

        //Shapes ����
        GameObject instantShapesObj = Instantiate(shapesPrefab[level + 1], targetPos, Quaternion.identity, shapesGroup);
        Shapes instantShapes = instantShapesObj.GetComponent<Shapes>();

        instantShapes.manager = this;
        instantShapes.effect = instantEffect;
        instantShapes.effectObj = instantEffectObj;

        instantShapes.rigid.simulated = true;
        instantShapes.gameObject.SetActive(true);
        SfxPlay(GameManager.SFX.LevelUp);
        instantShapes.effect.Play();
        yield return new WaitForSeconds(3f);
    }

    //Random Dongle ����
    //Shapes GetShapes()
    //{
    //    for(int index = 0; index < shapesPool.Count; index++)
    //    {
    //        poolCursor = (poolCursor + 1) % shapesPool.Count; ;
    //        if (!shapesPool[poolCursor].gameObject.activeSelf)//�ش� ������Ʈ�� Ȱ��ȭ�Ǿ����� �˷��ִ� activeSelf
    //            return shapesPool[poolCursor];
    //    }
    //    return MakeShapes();
    //}

    public void TouchDown()
    {
        if (lastShapes == null)
            return;

        lastShapes.Drag();
    }

    public void TouchUp()
    {
        if (lastShapes == null)
            return;

        lastShapes.Drop();
        lastShapes = null;
    }

    public void GameOver()
    {
        if (isOver)
            return;

        isOver = true;

        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        //1. ��� �ȿ� �ִ� ��� ���� ��������<�߿�>
        Shapes[] Shepes = FindObjectsOfType<Shapes>();

        //2. ����� �� �ر� & �ռ� ������ ���� ��� ������ ����ȿ�� ��Ȱ��ȭ
        foreach (Shapes shape in Shepes)
        {
            shape.rigid.simulated = false;
        }

        //3. ��� ���ۿ� �ϳ��� ������ �����ֱ�
        foreach (Shapes shape in Shepes)
        {
            shape.Hide(Vector3.up * 100);//Tip: �ش� �Լ��� ���� ���ϴ� ������� ������ ���� ���
            yield return new WaitForSeconds(0.1f);
        }
        //�ƿ� ������ ���� ���� ����ְ�, �� �Լ��� if���� �߰��� �����ϴ� ��ĵ� ����

        yield return new WaitForSeconds(1f);

        //4. �ְ� ���� ����
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        //5. ���� ���� UI ǥ��
        subScoreText.text = "����: " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(GameManager.SFX.GameOver);
    }

    public void Reset()
    {
        SfxPlay(SFX.Button);
        StartCoroutine(ResetCoroutine());
    }

    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(0);

    }

    public void SfxPlay(SFX type)
    {
        switch (type)
        {
            case SFX.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case SFX.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case SFX.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case SFX.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case SFX.GameOver:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }
        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;// sfxCursor = 0, 1, 2
    }

    private void LateUpdate()//Update ���� �� ����: ����, ��ġ�� Update���� ����ϸ�, �̸� ������ Ȱ���ϴ� ���� LateUpdate
    {
        scoreText.text = score.ToString();
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();//����� ȯ�濡�� ���� ������
        }
    }
}
