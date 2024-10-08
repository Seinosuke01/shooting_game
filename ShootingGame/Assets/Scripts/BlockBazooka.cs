using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BlockType
{
    public int[,] shape; //形状を定義する配列
}

public class BlockBazooka : MonoBehaviour
{
    //ブロックのゲームオブジェクト
    [SerializeField]
    GameObject blockObj;

    GameObject shotObj;

        // ブロックタイプの作成
    [SerializeField]
    BlockType[] blockTypes;

    //ポインタオブジェクト
    [SerializeField]
    GameObject pointObj;

    [SerializeField]
    private AudioSource shootSound;

    // ブロックの種類を定義
    void CreateBlockType()
    {
        blockTypes = new BlockType[7];
        blockTypes[0].shape = new int[,] {
            {0,0,0,0},
            {0,0,0,0},
            {1,1,1,1},
            {0,0,0,0},
        };
        blockTypes[1].shape = new int[,] {
            {1,1},
            {1,1},
        };
        blockTypes[2].shape = new int[,] {
            {0,1,0},
            {1,1,1},
            {0,0,0},
        };
        blockTypes[3].shape = new int[,] {
            {0,0,1},
            {1,1,1},
            {0,0,0},
        };
        blockTypes[4].shape = new int[,] {
            {1,0,0},
            {1,1,1},
            {0,0,0},
        };
        blockTypes[5].shape = new int[,] {
            {1,1,0},
            {0,1,1},
            {0,0,0},
        };
        blockTypes[6].shape = new int[,] {
            {0,1,1},
            {1,1,0},
            {0,0,0},
        };
    }

    //ブロックを生成
    GameObject CreateBlock(int typeNum){
        int size = blockTypes[typeNum].shape.GetLength(0);
        GameObject blockUnits = new GameObject("BlockUnits");
        blockUnits.tag = "blockUnits";

        //物理演算を適用するためRigidbodyを追加
        var rb = blockUnits.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;
        //二次元配列をループで処理
        for(int i=0; i<size; i++){
            for(int j=0; j<size; j++){
                //ブロックを配置する位置であればブロックを生成
                if(blockTypes[typeNum].shape[j, i]==1){
                    GameObject go = Instantiate(blockObj);
                    go.transform.parent = blockUnits.transform;
                    //ブロックを生成する位置はオブジェクトの相対位置で決定
                    go.transform.localPosition = new Vector3(i-size/2, size/2-j, 0)*0.1f;
                    //修正：Blockコンポーネントを追加
                    if(!go.TryGetComponent<Block>(out var block)){
                        block = go.AddComponent<Block>();
                    }
                    go.GetComponent<BoxCollider>().enabled = false;
                }
            }
        }
        return blockUnits;
    }

    //左右どちらかのコントローラを取得するために使用するControllerクラス
    OVRInput.Controller controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<OVRControllerHelper>().m_controller;

        CreateBlockType();

        //ブロックユニットを事前に生成
        shotObj = CreateBlock(Random.Range(0, 7));
        shotObj.transform.parent = transform;
        shotObj.transform.localPosition = Vector3.zero;

        if(shootSound == null){
            shootSound = GetComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {

        switch(GameStatus.status){
            case "Shot":
            ViewPointer();
                if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller)){
                    Fire(transform.position, transform.forward, shotObj);
                    shootSound.Play();
                    shotObj = CreateBlock(Random.Range(0, 7));
                    shotObj.transform.parent = transform;
                    shotObj.transform.localPosition = Vector3.zero;
                }
                if(OVRInput.GetDown(OVRInput.Button.One, controller)){
                    RotateBlockUnit(true);
                }
                if(OVRInput.GetDown(OVRInput.Button.Two, controller)){
                    ChangeBlock();
                }
            break;
            case "Wait":
            break;
            case "Fall":
            break;
            case "Delete":
            break;
        }
    }

    //ブロックを発射する関数
    void Fire(Vector3 startPos, Vector3 direction, GameObject target)
    {
        var rb = target.GetComponent<Rigidbody>();
        // rb.useGravity = true;
        target.transform.parent = null;
        //ブロックの衝突判定を許可する
        for(int i=0; i<target.transform.childCount; i++){
            target.transform.GetChild(i).GetComponent<BoxCollider>().enabled = true;
        }

        //ブロックの発射位置を設定
        target.transform.position = startPos;
        //ブロックをコントローラ正面方向に放つ
        target.GetComponent<Rigidbody>().AddForce(direction*10f,ForceMode.Impulse);
        //状態を「Wait」に変更
        GameStatus.status = "Wait";
    }

    void RotateBlockUnit(bool isRight)
    {
        if(isRight)
        {
            for (int i = 0; i < shotObj.transform.childCount;i++)
            {
                            // 右回転
                Vector3 tmp = shotObj.transform.GetChild(i).transform.localPosition;
                int x = shotObj.transform.GetChild(i).GetComponent<Block>().x;
                int y = shotObj.transform.GetChild(i).GetComponent<Block>().y;
                shotObj.transform.GetChild(i).transform.localPosition = new Vector3(-tmp.y, tmp.x, 0);
                shotObj.transform.GetChild(i).GetComponent<Block>().x = -y;
                shotObj.transform.GetChild(i).GetComponent<Block>().y = x;
            }
        }
        else
        {
            for (int i = 0; i < shotObj.transform.childCount;i++)
            {
                            // 左回転
                Vector3 tmp = shotObj.transform.GetChild(i).transform.localPosition;
                int x = shotObj.transform.GetChild(i).GetComponent<Block>().x;
                int y = shotObj.transform.GetChild(i).GetComponent<Block>().y;
                shotObj.transform.GetChild(i).transform.localPosition = new Vector3(tmp.y, -tmp.x, 0);
                shotObj.transform.GetChild(i).GetComponent<Block>().x = y;
                shotObj.transform.GetChild(i).GetComponent<Block>().y = -x;
            }
        }
    }

    void ChangeBlock(){
        Destroy(shotObj);

        shotObj = CreateBlock(Random.Range(0, 7));
        shotObj.transform.parent = transform;
        shotObj.transform.localPosition = Vector3.zero;
    }

    void ViewPointer(){
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, 10f);
        if(hits.Length==0){
            pointObj.SetActive(false);
        }
        else{
            pointObj.SetActive(true);
            foreach(RaycastHit hit in hits){
                if(hit.collider.name == "Field"){
                    pointObj.transform.position = hit.point;
                }
            }
        }
    }
}
