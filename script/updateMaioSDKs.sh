#!/bin/bash

readonly ROOT=$(cd $(dirname $0)/.. && pwd)
readonly SAMPLE_ROOT=$ROOT/MaioPluginSample
readonly API_BASE="https://api.github.com"
readonly OWNER="imobile-maio"

if [ $(cd $ROOT && git checkout master >/dev/null && git status -s | wc -l) -ne 0 ]; then
    echo "Exists diff. Abort." >&2
    exit 1
fi

readonly TMP=$(mktemp -d)

echo $(cd $ROOT && git checkout -B updateSDKs) > /dev/null

update_sdk_ios(){
    local readonly repoName="maio-iOS-SDK"
    local readonly json=$(curl $API_BASE/repos/$OWNER/$repoName/releases/latest)
    local readonly version=$(echo $json | jq '.name' | tr -d "\"")
    local readonly url=$(echo $json | jq '.tarball_url' | tr -d "\"")

    curl -L $url -o $TMP/sdk_ios
    tar xfz $TMP/sdk_ios -C $TMP

    # NOTE: Maio.frameworkを削除すると中身に生成された.metaも喪失してしまうため、とりいそぎ削除しない
    # rm -rf $ROOT/MaioPluginSample/Assets/Plugins/iOS/Maio.framework
    cp -r $TMP/$OWNER-$repoName-*/Maio.framework $SAMPLE_ROOT/Assets/Plugins/iOS/

    if [ $(cd $ROOT && git status -s | wc -l) -ne 0 ]; then
        local readonly readme=$(mktemp)
        sed -E "s/(iOS.*Version[^0-9.]*)[0-9.]*/\1${version#v}/" $ROOT/README.md > $readme
        mv -f $readme $ROOT/README.md
        echo $(cd $ROOT && git commit -a -m "maio iOS SDKを $version に更新") > /dev/null
    fi
}

update_sdk_android(){
    local readonly repoName="maio-Android-SDK"
    local readonly json=$(curl $API_BASE/repos/$OWNER/$repoName/releases/latest)
    local readonly version=$(echo $json | jq '.name' | tr -d "\"")
    local readonly url=$(echo $json | jq '.tarball_url' | tr -d "\"")

    curl -L $url -o $TMP/sdk_android
    tar xfz $TMP/sdk_android -C $TMP

    # NOTE: Maio.frameworkを削除すると中身に生成された.metaも喪失してしまうため、とりいそぎ削除しない

    cp -r $TMP/$OWNER-$repoName-*/maio.aar $SAMPLE_ROOT/Assets/Plugins/Android/

    if [ $(cd $ROOT && git status -s | wc -l) -ne 0 ]; then
        local readonly readme=$(mktemp)
        sed -E "s/(Android.*Version[^0-9.]*)[0-9.]*/\1${version#v}/" $ROOT/README.md > $readme
        mv -f $readme $ROOT/README.md
        echo $(cd $ROOT && git commit -a -m "maio Android SDKを $version に更新") > /dev/null
    fi
}

make_unity_package(){
    local readonly masterHash=$(cd $ROOT && git rev-parse master)
    local readonly updateSDKsHash=$(cd $ROOT && git rev-parse updateSDKs)

    if [ "$masterHash" = "$updateSDKsHash" ]; then
        # 何も差分が無いので、終了
        return 0;
    fi

    echo $(/Applications/Unity\ 5.0/Unity\ 5.0.app/Contents/MacOS/Unity \
        -quit -batchmode -projectPath $SAMPLE_ROOT -exportPackage Assets/Plugins $ROOT/maio.unitypackage) > /dev/null
    echo $(cd $ROOT && git commit -a -m "Unity packageを更新") > /dev/null
}

release_if_needed(){
    local readonly masterHash=$(cd $ROOT && git rev-parse master)
    local readonly updateSDKsHash=$(cd $ROOT && git rev-parse updateSDKs)

    if [ "$masterHash" = "$updateSDKsHash" ]; then
        # 何も差分が無いので、終了
        return 0;
    fi

    local pluginVersion=$(grep "PLUGIN_VERSION\s*=[^=]*;" $SAMPLE_ROOT/Assets/Plugins/Maio.cs |
        sed -E "s/.*\"([0-9.]*)\".*/\1/")
    local currentDay=$(date +"%Y%m%d")
    echo $(cd $ROOT &&
        git checkout master &&
        git merge --no-ff updateSDKs &&
        git tag "v${pluginVersion}_${currentDay}"
    ) > /dev/null
}

update_sdk_ios
update_sdk_android
make_unity_package
release_if_needed

echo $(cd $ROOT && git checkout master && git branch -d updateSDKs) > /dev/null

rm -rf $TMP
