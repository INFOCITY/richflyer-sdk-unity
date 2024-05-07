//
//  RFPlugin.h
//  Unity-iPhone
//
//  Created by 後藤武 on 2022/08/17.
//

#ifndef RFPlugin_h
#define RFPlugin_h

#import <RichFlyer/RichFlyer.h>

@interface RFPlugin : NSObject

+ (void)receiveNotification:(UNNotificationResponse*)response;

+ (void)resetBadgeNumber;

+ (void)setBadgeNumber:(int)number;

+ (void)registSegments:(NSDictionary<NSString*, NSString*>*)segments completion:(void (^)(RFResult* result))completion;

+ (NSDictionary*)getSegments;

+ (NSDictionary*)getReceivedData;

+ (NSDictionary*)getLatestReceivedData;

+ (void)setLaunchMode:(RFLaunchModes)mode;

+ (void)displayContent:(NSString*)notificationId  completeHandler:(nullable void(^)(RFAction* _Nullable action))completeHandler;

+ (void)postMessage:(nonnull NSArray*)events variables:(nullable NSDictionary<NSString*, NSString*>*)variables standbyTime:(nullable NSNumber*)standbyTime completion:(nullable void (^)(RFResult* _Nonnull result, NSArray<NSString*>* _Nonnull eventPostIds))completion;

+ (void)cancelPosting:(nonnull NSString*)eventPostId completion:(nullable void (^)(RFResult* _Nonnull result))completion;

@end

#endif /* RFPlugin_h */
