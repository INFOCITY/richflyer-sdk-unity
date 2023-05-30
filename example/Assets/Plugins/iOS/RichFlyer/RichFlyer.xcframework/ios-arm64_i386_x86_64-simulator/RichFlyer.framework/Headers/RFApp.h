//
//  RFApp.h
//  RichFlyer
//
//  Copyright © 2019年 INFOCITY,Inc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "RFNotificationDelegate.h"
#import "RFAction.h"
#import "RFContent.h"

#define RICHFLYER_USERINFO @"richflyer_userInfo"
#define RICHFLYER_NOTIFICATIONIDS @"richflyer_notificationids"
#define FORCE_TOUCH_CAPABILITY @"force_Touch_Capability"


typedef NS_OPTIONS(NSUInteger, RFAuthorizationOptions) {
  RFAuthorizationOptionNone    = 0,
	RFAuthorizationOptionBadge   = (1 << 0),
	RFAuthorizationOptionSound   = (1 << 1),
	RFAuthorizationOptionAlert   = (1 << 2),
	RFAuthorizationOptionCarPlay = (1 << 3),
};

typedef NS_OPTIONS(NSUInteger, RFLaunchModes) {
	RFLaunchModeNone    = 0,
	RFLaunchModeText   = (1 << 0),	//テキストのみ
	RFLaunchModeImage  = (1 << 1), //静止画像
	RFLaunchModeGif    = (1 << 2),   //GIF画像
	RFLaunchModeMovie  = (1 << 3)  //動画
};

@interface RFResult : NSObject

@property (readonly) BOOL result;
@property (readonly, nonatomic, nullable) NSString* message;
@property (readonly) NSInteger code;

- (nonnull id)initWithResult:(BOOL)result message:(nullable NSString*)message code:(NSInteger)code;
@end

@interface RFApp : NSObject

#if STAGING || SANDBOX
@property (class, assign) BOOL useProductionAPI;
#endif

+ (BOOL)isSandbox;

+ (void)setServiceKey:(nonnull NSString*)serviceKey appGroupId:(nonnull NSString*)appGroupId sandbox:(BOOL)sandbox;
+ (nullable NSString*)serviceKey;

+ (void)registDevice:(nonnull NSData*)deviceToken __attribute__((deprecated("Use registDevice:completion: instead")));
+ (void)registDevice:(nonnull NSData*)deviceToken completion:(nullable void(^)(RFResult* _Nonnull result))completion;

+ (void)setBadgeNumber:(nonnull UIApplication*)application badge:(int)badge;
+ (void)resetBadgeNumber:(nonnull UIApplication*)application;

+ (void)requestAuthorization:(nonnull UIApplication*)application applicationDelegate: (nullable id<UIApplicationDelegate>)applicationDelegate;
+ (void)requestAuthorizationWithOptions:(nonnull UIApplication*)application options:(RFAuthorizationOptions)options applicationDelegate: (nullable id<UIApplicationDelegate>)applicationDelegate;

+ (void)setRFNotificationDelegate:(nullable id <RFNotificationDelegate>)delegate NS_AVAILABLE_IOS(10_0);

+ (void)willPresentNotification:(UNNotificationPresentationOptions)options completionHandler:(nullable void( ^)(UNNotificationPresentationOptions options))completionHandler NS_AVAILABLE_IOS(10_0);

+ (void)didReceiveNotification:(nonnull UNNotificationResponse*)response handler: (nullable void(^)(RFAction* _Nullable action,  NSString* _Nullable extendedProperty))handler;

+ (void)registSegments:(nonnull NSDictionary<NSString*, NSString*>*)segments completion:(nullable void (^)(RFResult* _Nonnull result))completion;

+ (void)registDateSegments:(nonnull NSDictionary<NSString*, NSDate*>*)segments completion:(nullable void (^)(RFResult* _Nonnull result))completion;

+ (void)registIntegerSegments:(nonnull NSDictionary<NSString*, NSNumber*>*)segments completion:(nullable void (^)(RFResult* _Nonnull result))completion;

+ (void)registBooleanSegments:(nonnull NSDictionary<NSString*, NSNumber*>*)segments completion:(nullable void (^)(RFResult* _Nonnull result))completion;

+ (void)registSegments:(nullable NSDictionary<NSString*, NSString*>*)stringSegments
		   intSegments:(nullable NSDictionary<NSString*, NSNumber*>*)intSegments
		   boolSegments:(nullable NSDictionary<NSString*, NSNumber*>*)boolSegments
		   dateSegments:(nullable NSDictionary<NSString*, NSDate*>*)dateSegments
			completion:(nullable void (^)(RFResult* _Nonnull result))completion;


+ (nullable NSDictionary<NSString*, NSString*>*)getSegments;

+ (nullable NSArray<RFContent*>*)getReceivedData;

+ (nullable RFContent*)getLatestReceivedData;

+ (void)setLaunchMode:(RFLaunchModes)modes;

+ (BOOL)isRichFlyerNotification:(nonnull NSDictionary*)userInfo;

@end
