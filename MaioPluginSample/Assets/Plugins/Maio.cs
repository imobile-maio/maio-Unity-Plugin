using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public static class Maio {
    private const string PLUGIN_VERSION = "1.1.6";

    /// <summary>
    /// maio SDK のエラー種別（アプリ側への通知内容）
    /// </summary>
    public enum FailReason {
        /// 不明なエラー
        Unknown = 0,
        /// 広告在庫切れ
        AdStockOut,
        /// ネットワーク接続エラー
        NetworkConnection,
        /// HTTP status 4xx クライアントエラー
        NetworkClient,
        /// HTTP status 5xx サーバーエラー
        NetworkServer,
        /// SDK エラー
        Sdk,
        /// クリエイティブダウンロードのキャンセル
        DownloadCancelled,
        /// 動画再生エラー
        VideoPlayback,
    }

    public static event InitializedEventHandler OnInitialized;
    public static event ChangedCanShowEventHandler OnChangedCanShow;
    public static event StartAdEventHandler OnStartAd;
    public static event FinishedAdEventHandler OnFinishedAd;
    public static event ClickedAdEventHandler OnClickedAd;
    public static event ClosedAdEventHandler OnClosedAd;
    public static event FailedEventHandler OnFailed;

    public delegate void InitializedEventHandler();
    public delegate void ChangedCanShowEventHandler(string zoneId, bool newValue);
    public delegate void StartAdEventHandler(string zoneId);
    public delegate void FinishedAdEventHandler(string zoneId, int playtime, bool skipped, string rewardParam);
    public delegate void ClickedAdEventHandler(string zoneId);
    public delegate void ClosedAdEventHandler(string zoneId);
    public delegate void FailedEventHandler(string zoneId, FailReason reason);

#if UNITY_IOS && !UNITY_EDITOR
#elif UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass _maio = new AndroidJavaClass("jp.maio.sdk.android.MaioAds");
    private static MaioAdsListener _maioListener = MaioAdsListener.Instance;

    private class MaioAdsListener : AndroidJavaProxy {

        internal static MaioAdsListener Instance { get { return _instance; } }
        private static MaioAdsListener _instance = new MaioAdsListener();

        private MaioAdsListener() : base("jp.maio.sdk.android.MaioAdsListenerInterface") { }

        //public override AndroidJavaObject Invoke(string methodName, object[] args) {
        //    onHoge((int)args[0], (string)args[1], (string)args[2]);
        //    return null;
        //}

        //SDK準備完了の処理
        public void onInitialized() {
            Maio.OnInitializedEventHandler();
        }

        //変更時の処理
        public void onChangedCanShow(string zoneId, bool newValue) {
            Maio.OnChangedCanShowEventHandler(zoneId, newValue);
        }

        ////再生直前に呼ばれる処理
        //public void onOpenAd(string zoneId) {
        //    Maio.OnOpenAdEventHandler(zoneId);
        //}

        //再生時に呼ばれる処理
        public void onStartedAd(string zoneId) {
            Maio.OnStartAdEventHandler(zoneId);
        }

        //再生終了時に呼ばれる処理
        public void onFinishedAd(int playtime, bool skipped, int duration, string zoneId) {
            Maio.OnFinishedAdEventHandler(zoneId, playtime, skipped, null);
        }

        //広告クリック時に呼ばれる処理
        public void onClickedAd(string zoneId){
            Maio.OnClickedAdEventHandler(zoneId);
        }

        //広告が閉じられた際の処理
        public void onClosedAd(string zoneId) {
            Maio.OnClosedAdEventHandler(zoneId);
        }

        public override AndroidJavaObject Invoke(string methodName, object[] args) {
            switch (methodName) {
                case "onFailed":
                    onFailed((AndroidJavaObject)args[0], (string)args[1]);
                    return null;
            }
            return base.Invoke(methodName, args);
        }

        //エラー時に呼ばれる処理
        public void onFailed(AndroidJavaObject reason, string zoneId) {
            FailReason reasonVal;
            switch (reason.Call<int>("ordinal")) {
                // RESPONSE
                case 0:
                    reasonVal = FailReason.NetworkServer;
                    break;

                // NETWORK_NOT_READY
                case 1:
                    reasonVal = FailReason.NetworkConnection;
                    break;

                // NETWORK
                case 2:
                    reasonVal = FailReason.NetworkConnection;
                    break;

                // UNKNOWN
                case 3:
                    reasonVal = FailReason.Unknown;
                    break;

                // AD_STOCK_OUT
                case 4:
                    reasonVal = FailReason.AdStockOut;
                    break;

                // VIDEO
                case 5:
                    reasonVal = FailReason.VideoPlayback;
                    break;

                default:
                    reasonVal = FailReason.Unknown;
                    break;
            }
            Maio.OnFailedEventHandler(zoneId, reasonVal);
        }
    }
#else
#endif

    /// <summary>
    /// maio Unity Plugin のバージョンを返します。
    /// </summary>
    public static string PluginVersion {
        get {
#if UNITY_IOS && !UNITY_EDITOR
            if (Application.platform != RuntimePlatform.OSXEditor) {
                return string.Format("{0}(iOS SDK {1})", PLUGIN_VERSION, _SdkVersion());
            }
            else {
                return null;
            }
#elif UNITY_ANDROID && !UNITY_EDITOR
            return string.Format("{0}(Android SDK {1})", PLUGIN_VERSION, (string)_maio.CallStatic<string>("getSdkVersion"));
#else
            return null;
#endif
        }
    }

    /// <summary>
    /// maio SDK のバージョンを返します。
    /// </summary>
    /// <value>maio SDK のバージョン</value>
    [Obsolete("Use \"PluginVersion\" property instead.")]
    public static string SdkVersion {
        get {
#if UNITY_IOS && !UNITY_EDITOR
            if (Application.platform != RuntimePlatform.OSXEditor) {
                return _SdkVersion();
            }
            else {
                return null;
            }
#elif UNITY_ANDROID && !UNITY_EDITOR
            return (string)_maio.CallStatic<string>("getSdkVersion");
#else
            return null;
#endif
        }
    }

    /// <summary>
    /// 広告の配信テストを行うかどうかを設定します。
    /// </summary>
    /// <param name="adTestMode">広告のテスト配信を行う場合には <c>true</>、それ以外なら <c>false</c>。アプリ開発中は <c>true</c> にし、ストアに提出する際には <c>false</c> にして下さい（既定値は <c>false</c>）。</param>
    public static void SetAdTestMode(bool adTestMode) {
        // Debug.Log(adTestMode);
#if UNITY_IOS && !UNITY_EDITOR
        if (Application.platform != RuntimePlatform.OSXEditor) {
            _SetAdTestMode(adTestMode);
        }
#elif UNITY_ANDROID && !UNITY_EDITOR
        _maio.CallStatic("setAdTestMode", adTestMode);
#endif
    }

    /// <summary>
    /// SDK のセットアップを開始します。
    /// </summary>
    /// <param name="mediaId">管理画面にて発行されるアプリ識別子</param>
    public static void Start(string mediaId) {
        // Debug.Log(mediaId);
#if UNITY_IOS && !UNITY_EDITOR
        if (Application.platform != RuntimePlatform.OSXEditor) {
            _Start(mediaId
                ,OnInitializedEventHandler
                ,OnChangedCanShowEventHandler
                ,OnStartAdEventHandler
                ,OnFinishedAdEventHandler
                ,OnClickedAdEventHandler
                ,OnClosedAdEventHandler
                ,OnFailedEventHandler
           );
        }
#elif UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject activity;
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
             activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"); 
        }
        _maio.CallStatic("init", activity, mediaId, _maioListener);
#endif
    }

    /// <summary>
    /// 指定したゾーンの広告表示準備が整っていれば YES、そうでなければ NO を返します。
    /// </summary>
    /// <param name="zoneId">広告の表示準備が整っているか確認したいゾーンの識別子</param>
    /// <returns></returns>
    public static bool CanShow(string zoneId) {
        // Debug.Log(zoneId);
#if UNITY_IOS && !UNITY_EDITOR
        if (Application.platform != RuntimePlatform.OSXEditor) {
            return _CanShow(zoneId);
        }
        else {
            return false;
        }
#elif UNITY_ANDROID && !UNITY_EDITOR
        return (bool)_maio.CallStatic<bool>("canShow", zoneId);
#else
        return false;
#endif
    }
    /// <summary>
    /// 既定のゾーンの広告表示準備が整っていれば YES、そうでなければ NO を返します。
    /// </summary>
    /// <returns></returns>
    public static bool CanShow() {
        return CanShow(null);
    }

    /// <summary>
    /// 指定したゾーンの広告を表示します。
    /// </summary>
    /// <param name="zoneId">広告を表示したいゾーンの識別子</param>
    public static void Show(string zoneId) {
        // Debug.Log(zoneId);
#if UNITY_IOS && !UNITY_EDITOR
        if (Application.platform != RuntimePlatform.OSXEditor) {
            _Show(zoneId);
        }
#elif UNITY_ANDROID && !UNITY_EDITOR
        if(zoneId == null)
        {
            _maio.CallStatic("show");
        }
        else{
            _maio.CallStatic("show", zoneId);
        }
#endif
    }
    /// <summary>
    /// 既定のゾーンの広告を表示します。
    /// </summary>
    public static void Show() {
        Show(null);
    }

    #region Internal

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string _SdkVersion();
    [DllImport("__Internal")]
    private static extern void _SetAdTestMode(bool adTestMode);
    [DllImport("__Internal")]
    private static extern void _Start(string zoneId
        ,InitializedEventHandler e1
        ,ChangedCanShowEventHandler e2
        ,StartAdEventHandler e3
        ,FinishedAdEventHandler e4
        ,ClickedAdEventHandler e5
        ,ClosedAdEventHandler e6
        ,FailedEventHandler e7
    );
    [DllImport("__Internal")]
    private static extern bool _CanShow(string zoneId);
    [DllImport("__Internal")]
    private static extern void _Show(string zoneId);
#elif UNITY_ANDROID && !UNITY_EDITOR
#endif

    [AOT.MonoPInvokeCallback(typeof(InitializedEventHandler))]
    static void OnInitializedEventHandler() {
        if (OnInitialized != null) {
            OnInitialized();
        }
    }
    [AOT.MonoPInvokeCallback(typeof(ChangedCanShowEventHandler))]
    static void OnChangedCanShowEventHandler(string zoneId, bool newValue) {
        if (OnChangedCanShow != null) {
            OnChangedCanShow(zoneId, newValue);
        }
    }
    [AOT.MonoPInvokeCallback(typeof(StartAdEventHandler))]
    static void OnStartAdEventHandler(string zoneId) {
        if (OnStartAd != null) {
            OnStartAd(zoneId);
        }
    }
    [AOT.MonoPInvokeCallback(typeof(FinishedAdEventHandler))]
    static void OnFinishedAdEventHandler(string zoneId, int playtime, bool skipped, string rewardParam) {
        if (OnFinishedAd != null) {
            OnFinishedAd(zoneId, playtime, skipped, rewardParam);
        }
    }
    [AOT.MonoPInvokeCallback(typeof(ClickedAdEventHandler))]
    static void OnClickedAdEventHandler(string zoneId) {
        if (OnClickedAd != null) {
            OnClickedAd(zoneId);
        }
    }
    [AOT.MonoPInvokeCallback(typeof(ClosedAdEventHandler))]
    static void OnClosedAdEventHandler(string zoneId) {
        if (OnClosedAd != null) {
            OnClosedAd(zoneId);
        }
    }
    [AOT.MonoPInvokeCallback(typeof(FailedEventHandler))]
    static void OnFailedEventHandler(string zoneId, FailReason reason) {
        if (OnFailed != null) {
            OnFailed(zoneId, reason);
        }
    }

    #endregion Internal
}