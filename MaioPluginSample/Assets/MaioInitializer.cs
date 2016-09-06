using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System;

public class MaioInitializer : MonoBehaviour {
    /// maio から発行されるメディアIDに差し替えてください。
#if UNITY_IOS
    public const string MEDIA_ID = "DemoPublisherMedia";
#elif UNITY_ANDROID
    public const string MEDIA_ID = "DemoPublisherMediaForAndroid";
#else
    public const string MEDIA_ID = "DemoPublisherMedia";
#endif

    private readonly Queue<Action> _mainQueue = new Queue<Action>();

    void Start() {
        Debug.Log("Hello world!");
        Debug.Log(Maio.PluginVersion);

        var btnAdZone = GameObject.Find("btnAdZone").GetComponent<Button>();
        btnAdZone.onClick.AddListener(() => {
            // 動画広告を表示
            if (Maio.CanShow()) {
                Maio.Show();
            }
        });
        var txtAdZone = btnAdZone.GetComponentsInChildren<Text>().First();

        // maio 用のイベントハンドラを設定します。
        // 必要に応じて、下記コメントアウトを外してください。
        Maio.OnInitialized += () => {
            Debug.LogFormat("Maio.OnInitialized()");
        };
        Maio.OnChangedCanShow += (zoneId, newValue) => {
            Debug.LogFormat("Maio.OnChangedCanShow('{0}', {1})", zoneId, newValue);
            lock (_mainQueue) {
                _mainQueue.Enqueue(() => {
                    txtAdZone.text = "Play Ad";
                    btnAdZone.interactable = newValue;
                });
            }
        };
        Maio.OnStartAd += (zoneId) => {
            Debug.LogFormat("Maio.OnStartAd('{0}')", zoneId);
        };
        Maio.OnFinishedAd += (zoneId, playtime, skipped, rewardParam) => {
            Debug.LogFormat("Maio.OnFinishedAd('{0}', {1}, {2}, '{3}')", zoneId, playtime, skipped, rewardParam);
            if (!skipped) {
                // ここで、ユーザーへのリワード処理を行う事ができます。
            }
        };
        Maio.OnClickedAd += (zoneId) => {
            Debug.LogFormat("Maio.OnClickedAd('{0}')", zoneId);
        };
        Maio.OnClosedAd += (zoneId) => {
            Debug.LogFormat("Maio.OnClosedAd('{0}')", zoneId);
        };
        Maio.OnFailed += (zoneId, reason) => {
            Debug.LogFormat("Maio.OnFailed({0}, {1})", zoneId == null ? "null" : "'" + zoneId + "'", reason);
        };

        // 広告の配信テスト設定を行います。アプリをリリースする際にはコメントアウトして下さい。
        Maio.SetAdTestMode(true);

        // SDK のセットアップを開始します。
        Maio.Start(MEDIA_ID);

        txtAdZone.text = "Loading Ad";
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
}
