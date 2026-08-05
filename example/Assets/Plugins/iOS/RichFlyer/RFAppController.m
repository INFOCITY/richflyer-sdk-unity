//
//  RFAppController.m
//  UnityFramework
//
//  Created by 後藤武 on 2022/08/10.
//

#import "RFAppController.h"
#import "RFPlugin.h"
#import "RFReceiverBridge.h"

@implementation RFAppController

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary<UIApplicationLaunchOptionsKey,id> *)launchOptions {

  NSDictionary* infoPlist = [[NSBundle mainBundle] infoDictionary];
  NSDictionary* rfItem = infoPlist[@"RichFlyer"];
  if (rfItem) {
    NSString* serviceKey = rfItem[@"serviceKey"];
    NSString* groupId = rfItem[@"groupId"];
    BOOL sandbox = [rfItem[@"sandbox"] boolValue];
    NSNumber* launchMode = rfItem[@"launchMode"];

    [RFApp setServiceKey:serviceKey appGroupId:groupId sandbox:sandbox];

    [RFApp setRFNotificationDelegate:self];

    [RFApp requestAuthorization:[UIApplication sharedApplication]
            applicationDelegate:self];
    
    if (launchMode) {
      [RFApp setLaunchMode:[launchMode intValue]];
    }
  } else {
    [RFPlugin completeInitializationWithResult:NO code:604 message:@"RichFlyer settings were not found in Info.plist."];
  }

  
  return [super application:application didFinishLaunchingWithOptions:launchOptions];
}

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken {
  [RFApp registDevice:deviceToken completion:^(RFResult* result) {
    [RFPlugin completeInitializationWithResult:result.result code:result.code message:result.message];
  }];

#if UNITY_USES_REMOTE_NOTIFICATIONS
  [super application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
#endif
}

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error {
  [RFPlugin completeInitializationWithResult:NO code:601 message:error.localizedDescription];
#if UNITY_USES_REMOTE_NOTIFICATIONS
  [super application:application didFailToRegisterForRemoteNotificationsWithError:error];
#endif
}


#pragma mark - RFNotificationDelegate
-(void)didReceiveNotificationWithCenter:(UNUserNotificationCenter *)center response:(UNNotificationResponse *)response withCompletionHandler:(void (^)(void))completionHandler {
  [RFReceiverBridge receiveNotification:response];
  
  completionHandler();
}

-(void)willPresentNotificationWithCenter:(UNUserNotificationCenter *)center notification:(UNNotification *)notification withCompletionHandler:(void (^)(UNNotificationPresentationOptions))completionHandler {
  
  UNNotificationPresentationOptions options;
  if (@available(iOS 14.0, *)) {
    options = UNNotificationPresentationOptionBanner|UNNotificationPresentationOptionList|UNNotificationPresentationOptionBadge|UNNotificationPresentationOptionSound;
  } else {
    options = UNNotificationPresentationOptionAlert|UNNotificationPresentationOptionBadge|UNNotificationPresentationOptionSound;
  }
  [RFApp willPresentNotification:options completionHandler:completionHandler];
}

-(void)dismissedContentDisplay:(RFAction *)action content:(RFContent *)content {
}
@end

IMPL_APP_CONTROLLER_SUBCLASS(RFAppController)
