using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MaioInitializer : MonoBehaviour {
	/// maio から発行されるメディアIDに差し替えてください。
	public const string MEDIA_ID = "DemoPublisherMedia";
	
	void Start() {
		Button btnAdZone = GameObject.Find ("btnAdZone").GetComponent<Button>();
		btnAdZone.onClick.AddListener (() => {
			// 動画広告を表示
			if (Maio.CanShow()) {
				Maio.Show ();
			}
		});
		Text txtAdZone = btnAdZone.GetComponentsInChildren<Text> () [0];
		
		// maio 用のイベントハンドラを設定します。
		// 必要に応じて、下記コメントアウトを外してください。
		Maio.OnInitialized += () => {
			Debug.Log("Maio.OnInitialized");
		};
		Maio.OnChangedCanShow += (zoneId, newValue) => {
			Debug.Log(string.Format("Maio.OnChangedCanShow('{0}', {1})", zoneId, newValue));
			txtAdZone.text = "Play Ad";
			btnAdZone.interactable = newValue;
		};
		Maio.OnStartAd += (zoneId) => {
			Debug.Log(string.Format("Maio.OnStartAd('{0}')", zoneId));
		};
		Maio.OnFinishedAd += (zoneId, playtime, skipped, rewardParam) => {
			Debug.Log(string.Format ("Maio.OnFinishedAd('{0}', {1}, {2}, '{3}')", zoneId, playtime, skipped, rewardParam));
			if (!skipped) {
				// ここで、ユーザーへのリワード処理を行う事ができます。
			}
		};
		Maio.OnClickedAd += (zoneId) => {
			Debug.Log(string.Format("Maio.OnClickedAd('{0}')", zoneId));
		};
		Maio.OnClosedAd += (zoneId) => {
			Debug.Log(string.Format("Maio.OnClosedAd('{0}')", zoneId));
		};
		Maio.OnFailed += (zoneId, reason) => {
			Debug.Log(string.Format("Maio.OnFailed('{0}', {1}", zoneId, reason));
		};
		
		// 広告の配信テスト設定を行います。アプリをリリースする際にはコメントアウトして下さい。
		Maio.SetAdTestMode(true);
		
		// SDK のセットアップを開始します。
		Maio.Start(MEDIA_ID);
		
		txtAdZone.text = "Loading Ad";
	}
}
