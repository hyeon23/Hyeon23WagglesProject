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
    [Range(1,30)]//OnDisable 함수에서 각종 변수, 트랜스폼, 물리 초기화
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
        //게임의 Frame을 부드럽게 하기
        //1. Application.targetFrameRage: 게임의 FPS를 설정하는 함수
        //어느 플랫폼에서나 60으로 통일
        Application.targetFrameRate = 60;//Prefab의 interpolate 속성을 none에서 interpolate로 바꾸면 움직임을 부드럽게 보정해줌

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

        //오브젝트 활성화
        foreach (GameObject backGround in backGrounds)
            backGround.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();//Audio Source 재생
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
        //이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab[Random.Range(0, 6)], effectGroup);
        //instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        //effectPool.Add(instantEffect);

        //Shapes 생성
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
        //이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab[Random.Range(0, 6)], targetPos, Quaternion.identity, effectGroup);
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();

        //Shapes 생성
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

    //Random Dongle 생성
    //Shapes GetShapes()
    //{
    //    for(int index = 0; index < shapesPool.Count; index++)
    //    {
    //        poolCursor = (poolCursor + 1) % shapesPool.Count; ;
    //        if (!shapesPool[poolCursor].gameObject.activeSelf)//해당 오브젝트가 활성화되었는지 알려주는 activeSelf
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
        //1. 장면 안에 있는 모든 동글 가져오기<중요>
        Shapes[] Shepes = FindObjectsOfType<Shapes>();

        //2. 지우기 전 붕괴 & 합성 방지를 위해 모든 동글의 물리효과 비활성화
        foreach (Shapes shape in Shepes)
        {
            shape.rigid.simulated = false;
        }

        //3. 모든 동글에 하나씩 접근해 지워주기
        foreach (Shapes shape in Shepes)
        {
            shape.Hide(Vector3.up * 100);//Tip: 해당 함수가 내가 원하는 로직대로 사용되지 않을 경우
            yield return new WaitForSeconds(0.1f);
        }
        //아예 사용되지 않을 값을 집어넣고, 본 함수에 if문을 추가해 제어하는 방식도 가능

        yield return new WaitForSeconds(1f);

        //4. 최고 점수 갱신
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        //5. 게임 오버 UI 표시
        subScoreText.text = "점수: " + scoreText.text;
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

    private void LateUpdate()//Update 종료 후 실행: 점수, 위치를 Update에서 계산하면, 이를 가지고 활용하는 곳은 LateUpdate
    {
        scoreText.text = score.ToString();
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();//모바일 환경에서 게임 나가기
        }
    }
}
