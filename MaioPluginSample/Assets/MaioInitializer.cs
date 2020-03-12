using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class MaioInitializer : MonoBehaviour {
    /// maio から発行されるメディアIDに差し替えてください。
#if UNITY_IOS
    public const string MEDIA_ID = "DemoPublisherMedia";
    public const string ZONE_ID = "DemoPublisherZone";
#elif UNITY_ANDROID
    public const string MEDIA_ID = "DemoPublisherMediaForAndroid";
    public const string ZONE_ID = "DemoPublisherZoneForAndroid";
#else
    public const string MEDIA_ID = "DemoPublisherMedia";
    public const string ZONE_ID = "DemoPublisherZone";
#endif

    private readonly Queue<Action> _mainQueue = new Queue<Action>();

    [SerializeField] private Button showAdButton;
    [SerializeField] private Text showAdButtonText;

    void Start() {
        Debug.Log("Hello world!");
        Debug.Log(Maio.PluginVersion);

        // maio 用のイベントハンドラを設定します。
        Maio.OnInitialized += HandleOnInitialized;
        Maio.OnChangedCanShow += HandleOnChangedCanShow;
        Maio.OnStartAd += HandleOnStartAd;
        Maio.OnFinishedAd += HandleOnFinishedAd;
        Maio.OnClickedAd += HandleOnClickedAd;
        Maio.OnClosedAd += HandleOnClosedAd;
        Maio.OnFailed += HandleOnFailed;

        // 広告の配信テスト設定を行います。アプリをリリースする際にはコメントアウトして下さい。
        Maio.SetAdTestMode(true);

        // SDK のセットアップを開始します。
        Maio.Start(MEDIA_ID);

        showAdButtonText.text = "Loading Ad";
    }

    void Update() {
        while (_mainQueue.Count > 0) {
            Action action;
            lock (_mainQueue) {
                action = _mainQueue.Dequeue();
            }
            action();
        }
    }

    void OnDestroy()
    {
        // シーンが遷移する際にイベントハンドラを解除します。
        Maio.OnInitialized -= HandleOnInitialized;
        Maio.OnChangedCanShow -= HandleOnChangedCanShow;
        Maio.OnStartAd -= HandleOnStartAd;
        Maio.OnFinishedAd -= HandleOnFinishedAd;
        Maio.OnClickedAd -= HandleOnClickedAd;
        Maio.OnClosedAd -= HandleOnClosedAd;
        Maio.OnFailed -= HandleOnFailed;
    }

    public void ShowAd()
    {
        if(Maio.CanShow(ZONE_ID))
        {
            Maio.Show(ZONE_ID);
        }
    }

    /// <summary>
    /// 初期化が完了した際に呼ばれます。
    /// </summary>
    public void HandleOnInitialized()
    {
        Debug.Log("Maio.OnInitialized()");
    }

    /// <summary>
    /// 表示が可否が変更された際に呼ばれます。
    /// </summary>
    /// <param name="zoneId"></param>
    /// <param name="newValue"></param>
    public void HandleOnChangedCanShow(string zoneId, bool newValue)
    {
        Debug.LogFormat("Maio.OnChangedCanShow('{0}', {1})", zoneId, newValue);
        lock (_mainQueue) {
            _mainQueue.Enqueue(() => {
                showAdButtonText.text = "Play Ad";
                showAdButton.interactable = newValue;
            });
        }
    }

    /// <summary>
    /// 広告の再生が開始された際に呼ばれます。
    /// </summary>
    /// <param name="zoneId"></param>
    public void HandleOnStartAd(string zoneId)
    {
        Debug.LogFormat("Maio.OnStartAd('{0}')", zoneId);
    }

    /// <summary>
    /// 広告の視聴が終了した際に呼ばれます。
    /// </summary>
    /// <param name="zoneId"></param>
    /// <param name="playtime"></param>
    /// <param name="skipped"></param>
    /// <param name="rewardParam"></param>
    public void HandleOnFinishedAd(string zoneId, int playtime, bool skipped, string rewardParam)
    {
        Debug.LogFormat("Maio.OnFinishedAd('{0}', {1}, {2}, '{3}')", zoneId, playtime, skipped, rewardParam);
        if (!skipped) {
            // ここで、ユーザーへのリワード処理を行う事ができます。
        }
    }

    /// <summary>
    /// 広告がクリックされた際に呼ばれます。
    /// </summary>
    /// <param name="zoneId"></param>
    public void HandleOnClickedAd(string zoneId)
    {
        Debug.LogFormat("Maio.OnClickedAd('{0}')", zoneId);
    }

    /// <summary>
    /// 広告が閉じられたときに呼ばれます
    /// </summary>
    /// <param name="zoneId"></param>
    public void HandleOnClosedAd(string zoneId)
    {
        Debug.LogFormat("Maio.OnClosedAd('{0}')", zoneId);
    }

    /// <summary>
    /// 広告の取得・表示などに失敗した際に呼ばれます。
    /// </summary>
    /// <param name="zoneId"></param>
    /// <param name="reason"></param>
    public void HandleOnFailed(string zoneId, Maio.FailReason reason)
    {
        Debug.LogFormat("Maio.OnFailed({0}, {1})", zoneId == null ? "null" : "'" + zoneId + "'", reason);
    }

}
