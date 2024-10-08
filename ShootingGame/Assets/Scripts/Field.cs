using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class Field : MonoBehaviour
{
    [SerializeField]
    GameObject scoreViewObj;

    [SerializeField]
    List<GameObject> blockList;

    //スコア用のTextMeshPro
    [SerializeField]
    TextMeshPro scoreTextMeshPro;

    [SerializeField]
    GameObject deleteEffect;

    int score;

    //ブロックを一列消去した際に追加するポイント
    const int DELETE_POINT = 100;

    bool[,] blocks = new bool[10, 20];

    // Wait状態の待ちフレームワーク
    int waitCnt = 0;

    // 最大待ちフレーム数
    const int MAXWAIT = 60;

    void InitBlocks(){
        for(int i=0; i<blocks.GetLength(0); i++){
            for(int j=0; j<blocks.GetLength(1); j++){
                blocks[i, j] = false;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitBlocks();
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch)){
            ResetGame();
            InitBlocks();
        }

		switch (GameStatus.status)
		{
			case "Shot":
                waitCnt = MAXWAIT;
			    break;
			case "Wait":
                waitCnt--;
                //1秒経過したらShot状態に遷移
                if(waitCnt<=0){
                    GameStatus.status = "Shot";
                }
			    break;
			case "Fall":
                while (FallBlocks()){}
                GameStatus.status = "Delete";
                break;
			case "Delete":
                CheckLines();
                GameStatus.status = "Shot";
			    break;
		}
    }

    void ResetGame()
    {
        // 全てのブロックを削除
        GameObject[] blocksToDelete = GameObject.FindGameObjectsWithTag("block");
        foreach (GameObject block in blocksToDelete)
        {
            Destroy(block);
        }

        // blocks 配列のリセット
        InitBlocks();

        // スコアをリセット
        score = 0;
        scoreTextMeshPro.text = "0";

        // ブロックリストをリセット
        blockList.Clear();
    }

        //座標の位置を変換する関数(座標を基に配列の番号を返す関数)
    int GetPosition(float value, float distance, int minRange, int maxRange)
    {
        for(int i = minRange; i<maxRange; i++)
        {
            if(Mathf.Abs(value-(float)(i)) < distance)
            {
                return i;
            }
        }
        return minRange-1;
    }

    void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.tag != "blockUnits")
			return;
        
        GameStatus.status = "Fall";

		var b = other.gameObject.transform.position;
		var u = 0.1f;
		var g = new Vector3(
			((int)(b.x / u)) * u,           //x座標
			((int)(b.y / u)) * u,           //y座標
			transform.position.z        //z座標
			);

		//向きを一定にする
		other.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

		//左下座標を定義
		float px = -0.4f;
		float py = 0.1f;

		//blocksの要素番号を取得
		int bx = GetPosition((g.x - px) / u, 0.01f, 0, 10);
		int by = GetPosition((g.y - py) / u, 0.01f, 0, 20);

		//範囲外ならエラーを帰す
		if (bx < 0 || by < 0 || bx >= 10 || by >= 20 || CheckExistBlock(other.gameObject, bx, by))
		{
            GameStatus.status = "Shot";
            for(int i=0; i<other.gameObject.transform.childCount; i++){
                Destroy(other.gameObject.transform.GetChild(i).transform.GetComponent<BoxCollider>());
            }
            other.gameObject.transform.GetComponent<Rigidbody>().AddForce(Vector3.back * 10f, ForceMode.Impulse);

		}
		else
		{
			//座標を反映
			other.gameObject.transform.position = new Vector3(bx * u + px, by * u + py, transform.position.z);

			//配置された位置の配列をtrueにする
			ApplyBlockUnits(other.gameObject, bx, by);

			// 位置を表示
			GameObject sv = Instantiate(scoreViewObj);
			sv.transform.position = other.gameObject.transform.position;
			sv.GetComponent<ScoreView>().textMeshPro.text = $"({bx},{by})";

			// 親オブジェクトを削除
			int cnt = other.transform.childCount;
			for (int i = 0; i < cnt; i++)
			{
				other.transform.GetChild(0).parent = null;
			}
			other.transform.tag = "Untagged";
			other.transform.position = Vector3.forward * 1000f;

		}
	}

    // その位置にブロックが存在するかチェックする関数
    bool CheckExistBlock(GameObject target, int x, int y)
    {
        for (int i = 0; i < target.transform.childCount; i++)
        {
            Vector3 g = target.transform.GetChild(i).localPosition;
            int bx = (int)(g.x * 10);
            int by = (int)(g.y * 10);
    　　　　　　　　//枠外にはブロックが存在することにしておく
            if( x + bx < 0 || y + by < 0 || x + bx >= 10 || y + by >= 20)
            {
                return true;
            }
    　　　　　　　　// （x+bx,y+by）の位置にブロックが存在しているかどうかチェック
            if (!(x + bx < 0 || x + bx >= 10 || y + by < 0 || y + by >= 20) && blocks[x + bx, y + by])
            {
                return true;
            }
        }
        return false;
    }

    void ApplyBlockUnits(GameObject target, int x, int y)
    {
        blockList = new List<GameObject>();
        for (int i = 0; i < target.transform.childCount; i++)
        {
            Transform child = target.transform.GetChild(i);
            Vector3 g = child.localPosition;
            int bx = (int)(g.x * 10);
            int by = (int)(g.y * 10);
            blocks[x + bx, y + by] = true;
            child.name = $"name:{x + bx},{y + by}";
            
            // Block コンポーネントを取得または追加
            if (!child.TryGetComponent<Block>(out var block))
            {
                block = child.gameObject.AddComponent<Block>();
            }
            // x と y を設定
            block.x = x + bx;
            block.y = y + by;

            blockList.Add(child.gameObject);
        }
    }


	void SortBlockList()
	{
		for (int i = 0; i < blockList.Count; i++)
		{
			for (int j = i; j < blockList.Count; j++)
			{
				if (blockList[i].GetComponent<Block>().y > blockList[j].GetComponent<Block>().y)
				{
					var tmp = blockList[i];
					blockList[i] = blockList[j];
					blockList[j] = tmp;
				}
			}
		}
	}

    // ブロックを一段だけ落下させる処理
	bool FallBlocks(){
		var retblocks = blocks.Clone() as bool[,];
		// ソート
		SortBlockList();
 
		// ブロックが落とせる状態にあるかチェック
		bool isFall = true;
 
		// 一個下にブロックがあるかチェック
		foreach (GameObject go in blockList)
		{
            Block block = go.GetComponent<Block>();
			int x = block.x;
			int y = block.y;
			if (y - 1 < 0 || retblocks[x, y - 1])
			{
				isFall = false;
				break;
			}
            else{
                retblocks[x, y] = false;
                retblocks[x, y - 1] = true;
            }
		}
 
		if (isFall)
		{
			// ブロックを配置
			foreach (GameObject go in blockList)
			{
				int x = go.GetComponent<Block>().x;
				int y = go.GetComponent<Block>().y;
				go.transform.position += Vector3.down * 0.1f;
 
				blocks[x, y] = false;
				blocks[x, y - 1] = true;
 
				go.GetComponent<Block>().y -= 1;
				go.name = $"name:{x},{y - 1}";
			}
		}
 
		return isFall;
	}

    void CheckLines(){
        for(int i=0; i<20; i++){
            bool isDelete = true;
            for(int j=0; j<10; j++){
                //上から順に調べたいため、iではなくて19-i
                if(!blocks[j, 19-i]){
                    isDelete = false;
                    break;
                }
            }
            if(isDelete){
                DeleteBlocks(19-i);
                DropBlocks(19-i);

                score += DELETE_POINT;
                scoreTextMeshPro.text = "" + score;
            }
        }
    }

    void DeleteBlocks(int h){
        //一列ブロックのGameObjectを削除する
        GameObject[] glist = GameObject.FindGameObjectsWithTag("block");
        foreach(GameObject go in glist){
            if(go.GetComponent<Block>().y == h){
                GameObject deleteEffectClone = Instantiate(deleteEffect);
                deleteEffectClone.transform.position = go.transform.position + Vector3.back*0.1f;
                deleteEffectClone.transform.localScale = Vector3.one * 0.1f;
                blocks[go.GetComponent<Block>().x, h] = false;
                Destroy(go);
            }
        }
    }

    void DropBlocks(int h){
        List<GameObject> glist = new List<GameObject>(GameObject.FindGameObjectsWithTag("block"));
        glist.OrderBy((x) => x.GetComponent<Block>().y);

        foreach(GameObject go in glist){
            if(go.GetComponent<Block>().y > h){
                // blocksを更新
                int x = go.GetComponent<Block>().x;
                int y = go.GetComponent<Block>().y;
                blocks[x, y] = false;
                blocks[x, y-1] = true;

                //y座標をずらす
                go.GetComponent<Block>().y -= 1;

                //GameObjectの座標をずらす
                go.transform.position += Vector3.down * 0.1f;
            }
        }
    }

    public void ResetField(){
        GameObject[] blocksToDelete = GameObject.FindGameObjectsWithTag("block");
        foreach (GameObject block in blocksToDelete)
        {
            Destroy(block);
        }

        InitBlocks();

        score = 0;
        scoreTextMeshPro.text = "" + score;

        blockList.Clear();
    }
}
