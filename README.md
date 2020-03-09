![](https://github.com/imobile-maio/maio-iOS-SDK/blob/wiki/doc/images/logo.png)

# maio-Unity-Plugin

* Unity Plugin Version: 1.1.6

## サンプルとして同梱しているSDK

* iOS SDK Version: 1.5.3
* Android SDK Version: 1.1.11

## 注意
- MaioPluginSample サンプルプロジェクトは Unity 5.5.4p5 以降で実行できます。
    - 開発環境がmacOS High Sierraに移行した都合により。
    - Unity 5.2.1 ~ 5.5.4p4 で開けるプロジェクトは、tag:`v1.1.5_20180712`をご利用ください。
        - ※iOS SDK 1.3.0以降、WebKit.frameworkに依存する都合により。
    - Unity 5.0 ~ 5.2.0 で開けるプロジェクトは、tag:`v1.1.4_20171114`をご利用ください。
- SDKはサンプルとして同梱しています。
    - iOS SDKは最新版を配布ページから取得し、差し替えるようにしてください。
        - [maio iOS SDK](https://github.com/imobile-maio/maio-iOS-SDK/releases)
    - Android SDK 1.1.3以降をお使いの場合、CanShow()、Show()にゾーンIDを指定してください。<br>
    ゾーンの指定がないと正しく広告が再生できません。<br>
    ※Android SDK 1.1.2以前をお使いの場合はゾーンの指定が無くても広告の再生が可能です。
    <pre><code>if(Maio.CanShow(”DemoPublisherZoneForAndroid”))
        {
            Maio.Show(”DemoPublisherZoneForAndroid”);
        }</code></pre>

## Get Started
日本語 [wiki/Get-Started](https://github.com/imobile-maio/maio-Unity-Plugin/wiki/Get-Started)

English [wiki/Get-Started](https://github.com/imobile-maio/maio-Unity-Plugin/wiki/Get-Started-(EN))

## API Reference
日本語 [wiki/API-Reference](https://github.com/imobile-maio/maio-Unity-Plugin/wiki/API-Reference)

English [wiki/API-Reference](https://github.com/imobile-maio/maio-Unity-Plugin/wiki/API-Reference-(EN))
