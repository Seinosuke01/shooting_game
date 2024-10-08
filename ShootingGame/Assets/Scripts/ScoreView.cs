using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreView : MonoBehaviour
{
    [SerializeField]
    public TextMeshPro textMeshPro;

    [SerializeField]
    int cnt = 0;
    const int MAXCNT = 60;

    // Start is called before the first frame update
    void Start()
    {
        textMeshPro.fontSize = 3; //フォントサイズを変更
    }

    // Update is called once per frame
    void Update()
    {
        cnt++;
        cnt%=60;
        if(cnt==0){
            Destroy(this.gameObject);
        }
        transform.position += Vector3.up*0.01f;
        
    }
}
