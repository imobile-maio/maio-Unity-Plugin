#import <Maio/Maio.h>

// Converts C style string to NSString
NSString* MaioCreateNSString (const char* string) {
  if (string)
    return [NSString stringWithUTF8String: string];
  else
    return [NSString stringWithUTF8String: ""];
}

// Helper method to create C string copy
char* MaioMakeStringCopy (const char* string) {
  if (string == NULL)
    return NULL;

  char* res = (char*)malloc(strlen(string) + 1);
  strcpy(res, string);
  return res;
}


#ifdef __cplusplus
extern "C" {
#endif
  typedef void (*MaioPluginInitializedCallback)();
  typedef void (*MaioPluginChangedCanShowCallback)(const char* zoneId, bool newValue);
  typedef void (*MaioPluginStartAdCallback)(const char* zoneId);
  typedef void (*MaioPluginFinishedAdCallback)(const char* zoneId, int playtime, bool skipped, const char* rewardParam);
  typedef void (*MaioPluginClickedAdCallback)(const char* zoneId);
  typedef void (*MaioPluginClosedAdCallback)(const char* zoneId);
  typedef void (*MaioPluginFailedCallback)(const char* zoneId, MaioFailReason reason);
  
  
  // typedef NS_ENUM(NSInteger, MaioEventId) {
  //   MaioEventIdOnInitialized = 1,
  //   MaioEventIdOnChangedCanShow = 2,
  //   MaioEventIdOnStartAd    = 3,
  //   MaioEventIdOnFinishedAd = 4,
  //   MaioEventIdOnClickedAd  = 5,
  //   MaioEventIdOnClosedAd   = 6,
  //   MaioEventIdOnFailed     = 0xff,
  // };

  @interface MaioDelegateImpl : NSObject<MaioDelegate>
  @property (nonatomic) MaioPluginInitializedCallback initializedCallback;
  @property (nonatomic) MaioPluginChangedCanShowCallback changedCanShowCallback;
  @property (nonatomic) MaioPluginStartAdCallback startAdCallback;
  @property (nonatomic) MaioPluginFinishedAdCallback finishedAdCallback;
  @property (nonatomic) MaioPluginClickedAdCallback clickedAdCallback;
  @property (nonatomic) MaioPluginClosedAdCallback closedAdCallback;
  @property (nonatomic) MaioPluginFailedCallback failedCallback;
  @end
  
  @implementation MaioDelegateImpl

  #pragma mark MaioDelegate

  - (void)maioDidInitialize {
    if (_initializedCallback) {
      _initializedCallback();
    }
  }

  - (void)maioDidChangeCanShow:(NSString *)zoneId newValue:(BOOL)newValue {
    if (_changedCanShowCallback) {
      _changedCanShowCallback(MaioMakeStringCopy([zoneId UTF8String]), newValue);
    }
  }

  - (void)maioWillStartAd:(NSString *)zoneId {
    if (_startAdCallback) {
      _startAdCallback([zoneId UTF8String]);
    }
  }

  - (void)maioDidFinishAd:(NSString *)zoneId playtime:(NSInteger)playtime skipped:(BOOL)skipped rewardParam:(NSString *)rewardParam {
    if (_finishedAdCallback) {
      _finishedAdCallback([zoneId UTF8String], (int)playtime, skipped, [rewardParam UTF8String]);
    }
  }

  - (void)maioDidClickAd:(NSString *)zoneId {
    if (_clickedAdCallback) {
      _clickedAdCallback([zoneId UTF8String]);
    }
  }

  - (void)maioDidCloseAd:(NSString *)zoneId {
    if (_closedAdCallback) {
      _closedAdCallback([zoneId UTF8String]);
    }
  }

  - (void)maioDidFail:(NSString *)zoneId reason:(MaioFailReason)reason {
    if (_failedCallback) {
      _failedCallback([zoneId UTF8String], reason);
    }
  }

  @end
  

  static MaioDelegateImpl *s_maioDelegate;

  void _SetAdTestMode (bool adTestMode) {
    [Maio setAdTestMode: adTestMode];
  }

  void _Start (const char* mediaId
    ,MaioPluginInitializedCallback initializedCallback
    ,MaioPluginChangedCanShowCallback changedCanShowCallback
    ,MaioPluginStartAdCallback startAdCallback
    ,MaioPluginFinishedAdCallback finishedAdCallback
    ,MaioPluginClickedAdCallback clickedAdCallback
    ,MaioPluginClosedAdCallback closedAdCallback
    ,MaioPluginFailedCallback failedCallback
    ) {
    if (!s_maioDelegate) {
        s_maioDelegate = [MaioDelegateImpl new];
    }
    s_maioDelegate.initializedCallback = initializedCallback;
    s_maioDelegate.changedCanShowCallback = changedCanShowCallback;
    s_maioDelegate.startAdCallback = startAdCallback;
    s_maioDelegate.finishedAdCallback = finishedAdCallback;
    s_maioDelegate.clickedAdCallback = clickedAdCallback;
    s_maioDelegate.closedAdCallback = closedAdCallback;
    s_maioDelegate.failedCallback = failedCallback;

    [Maio startWithMediaId:MaioCreateNSString(mediaId) delegate:s_maioDelegate];
  }

  bool _CanShow (const char* zoneId) {
    if (zoneId) {
      return [Maio canShowAtZoneId:MaioCreateNSString(zoneId)];
    }
    else {
      return [Maio canShow];
    }
  }

  void _Show (const char* zoneId) {
    if (zoneId) {
      [Maio showAtZoneId:MaioCreateNSString(zoneId)];
    }
    else {
      [Maio show];
    }
  }
#ifdef __cplusplus
}
#endif


