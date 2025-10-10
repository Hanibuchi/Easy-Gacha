using UnityEngine;
using System.Collections; // Coroutineのために必要

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    // --- インスペクターから設定するパラメータ ---
    
    // カメラの初期位置（Awake時に現在の位置で設定されることが多いが、今回はインスペクターで指定可能にする）
    [Header("カメラ設定")]
    [Tooltip("カメラがズームインする前の初期位置")]
    public Vector3 initialPosition = new Vector3(0f, 1f, -10f); 
    
    // 最終的なズームイン位置
    [Tooltip("カメラが最終的に到達するズームイン位置")]
    public Vector3 zoomedPosition = new Vector3(0f, 1f, -5f);
    
    // 移動時間
    [Tooltip("ズームイン/ズームアウトにかける時間（秒）")]
    public float moveDuration = 0.8f;

    // 移動の挙動を定義するカーブ（グラフで速度変化を視覚的に設定可能）
    [Tooltip("移動のイージングを定義するカーブ")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // 現在実行中のコルーチンを保持するための変数
    private Coroutine currentMoveCoroutine;

    // --- シングルトンと初期設定 ---
    
    private void Awake()
    {
        // シングルトンのインスタンス設定
        if (Instance == null)
        {
            Instance = this;
            // シーンを跨いでも破棄されないようにする（必要に応じて）
            DontDestroyOnLoad(gameObject);
            
            // CameraManagerがアタッチされているGameObjectの位置を初期位置に設定
            transform.position = initialPosition;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- カメラ演出メソッド ---
    
    /// <summary>
    /// カメラを指定されたズームイン位置へ移動させる。
    /// </summary>
    public void ZoomInCamera()
    {
        // 既存の移動コルーチンがあれば停止する
        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
        }
        
        // ズームインコルーチンを開始
        currentMoveCoroutine = StartCoroutine(MoveCameraRoutine(zoomedPosition));
    }

    /// <summary>
    /// カメラを初期位置に戻す。
    /// </summary>
    public void ResetCameraPosition()
    {
        // 既存の移動コルーチンがあれば停止する
        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
        }
        
        // 初期位置に戻すコルーチンを開始
        currentMoveCoroutine = StartCoroutine(MoveCameraRoutine(initialPosition));
    }

    // --- カメラ移動のコルーチン ---
    
    /// <summary>
    /// カメラを指定された目標位置へ滑らかに移動させるコルーチン。
    /// </summary>
    /// <param name="targetPosition">移動先の目標位置</param>
    private IEnumerator MoveCameraRoutine(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float startTime = Time.time;
        
        // 移動時間全体にわたって実行
        while (Time.time < startTime + moveDuration)
        {
            // 経過時間 / 全移動時間 = 0.0 ~ 1.0 の値
            float elapsedRatio = (Time.time - startTime) / moveDuration;
            
            // AnimationCurveを使用して、イージングを適用した進捗度（0.0 ~ 1.0）を計算
            float curveValue = easeCurve.Evaluate(elapsedRatio);

            // Lerp（線形補間）を使ってカメラの位置を滑らかに更新
            transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);

            yield return null; // 1フレーム待機
        }

        // 終了時に目標位置に完全に固定する
        transform.position = targetPosition;
        
        // コルーチンが終了したことを記録
        currentMoveCoroutine = null;
    }
}