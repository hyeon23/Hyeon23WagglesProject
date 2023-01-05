using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shapes : MonoBehaviour
{
    public GameManager manager;
    public GameObject effectObj;
    public ParticleSystem effect;//Dongle ���鶧 �Ŀ� ����

    public int level;
    bool isDrag;
    bool isMerge;
    bool isAttach;
    public Rigidbody2D rigid;
    public PolygonCollider2D polygon;   
    //public TrailRenderer trailRenderer;

    Animator anime;
    SpriteRenderer spriteRenderer;

    float deadTime;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anime = GetComponent<Animator>();
        polygon = GetComponent<PolygonCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDisable()//������Ʈ ��Ȱ��ȭ �� ����Ǵ� �Լ�
    {
        //Shapes �Ӽ� �ʱ�ȭ
        isDrag = false;
        isMerge = false;
        isAttach = false;

        //Shapes Ʈ������ �ʱ�ȭ
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;//Quternion.identity�� ȸ������ Vector3.zero�� ���� ����
        transform.localScale = Vector3.zero;

        rigid.simulated = false;//rigid body ��Ȱ��ȭ
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0f;//ȸ���ӵ��� float ���̹Ƿ� 0
        polygon.enabled = true;
    }

    void Update()
    {
        if (isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //x axis limited
            float leftBorder = -4.2f + transform.localScale.x / 2f;//+-
            float rightBorder = 4.2f - transform.localScale.x / 2f;//+-

            if (mousePos.x < leftBorder)
                mousePos.x = leftBorder;
            else if (mousePos.x > rightBorder)
                mousePos.x = rightBorder;

            //y axis limited
            mousePos.y = 8;

            //z axis limited(Camera ����)
            mousePos.z = 0;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);
        }
    }

    //Drag & Drop
    public void Drag()
    {
        rigid.simulated = false;
        isDrag = true;
    }

    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(AttachRoutine());
    }

    IEnumerator AttachRoutine()
    {
        if (isAttach)
        {
            yield break;
        }

        isAttach = true;
        manager.SfxPlay(GameManager.SFX.Attach);

        yield return new WaitForSeconds(3f);

        isAttach = false;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Shapes" && !manager.isOver)
        {
            Shapes other = collision.gameObject.GetComponent<Shapes>();
            if (level == other.level && !isMerge && !other.isMerge && level < 5)
            {
                //Shapes ��ġ��: ���� ����� ��ġ
                float myX = transform.position.x;
                float myY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                //1. ���� �Ʒ��� ���� ��,
                //2. ������ ������ ��, ���� �����ʿ� ���� ��,
                if (myY < otherY || (myY == otherY && myX > otherX))
                {
                    //�� �� �����
                    Hide((other.transform.position + transform.position) / 2);
                    other.Hide((other.transform.position + transform.position) / 2);
                    //���� ������
                    LevelUp((other.transform.position + transform.position) / 2, other.level);
                }
            }
        }
    }

    public void Hide(Vector3 targetPos)//targetPos (= myPos)
    {
        isMerge = true;
        rigid.simulated = false;
        polygon.enabled = false;
        anime.SetTrigger("doMerge");
        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;
        while (frameCount < 20)
        {
            frameCount++;
            if (targetPos != Vector3.up * 100)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.1f);
            }
            else
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);//Lerp�� ����� õõ�� ���̱�
            }
            yield return null;//Essential
        }
        manager.score += (int)Mathf.Pow(2, level);// score = 2^level

        isMerge = false;
        Destroy(gameObject);
        Destroy(effectObj);
    }

    public void LevelUp(Vector3 targetPos, int level)
    {
        StartCoroutine(LevelUpRoutine1(targetPos, level));
    }

    IEnumerator LevelUpRoutine1(Vector3 targetPos, int level)
    {
        yield return new WaitForSeconds(0.1f);

        //manager.maxLevel = Mathf.Max(level, manager.maxLevel);//�� �� �� ū ���� ��ȯ�� ����
        StartCoroutine(manager.LevelUpRoutine2(targetPos, level));
        isMerge = false;
        yield return new WaitForSeconds(3f);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime += Time.deltaTime;

            if(deadTime > 2)
            {
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }
            if (deadTime > 5)
            {
                manager.GameOver();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }
}