//
//  RFAppController.m
//  UnityFramework
//
//  Created by 後藤武 on 2022/08/10.
//

#import "RFAppController.h"
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
  }

  
  return [super application:application didFinishLaunchingWithOptions:launchOptions];
}

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken {
  [RFApp registDevice:deviceToken completion:^(RFResult* result) {
    if (result.result) {
      // register device succeeded.
    } else {
      // register device failed.
    }
  }];

//  [super application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
}


#pragma mark - RFNotificationDelegate
-(void)didReceiveNotificationWithCenter:(UNUserNotificationCenter *)center response:(UNNotificationResponse *)response withCompletionHandler:(void (^)(void))completionHandler {
  [RFReceiverBridge receiveNotification:response];
  
  completionHandler();
}

-(void)willPresentNotificationWithCenter:(UNUserNotificationCenter *)center notification:(UNNotification *)notification withCompletionHandler:(void (^)(UNNotificationPresentationOptions))completionHandler {
}

-(void)dismissedContentDisplay:(RFAction *)action content:(RFContent *)content {
}
@end

IMPL_APP_CONTROLLER_SUBCLASS(RFAppController)
