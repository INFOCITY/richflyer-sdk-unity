//
//  RFPlugin.h
//  Unity-iPhone
//
//  Created by 後藤武 on 2022/08/17.
//

#ifndef RFPlugin_h
#define RFPlugin_h

#import <RichFlyer/RichFlyer.h>

NS_ASSUME_NONNULL_BEGIN

@interface RFPlugin : NSObject

+ (void)receiveNotification:(nullable UNNotificationResponse*)response;

+ (void)completeInitializationWithResult:(BOOL)result code:(NSInteger)code message:(nullable NSString*)message;

+ (void)resetBadgeNumber;

+ (void)setBadgeNumber:(int)number;

+ (void)registSegments:(NSDictionary<NSString*, NSString*>*)segments completion:(void (^)(RFResult* result))completion;

+ (NSDictionary*)getSegments;

+ (NSDictionary*)getReceivedData;

+ (nullable NSDictionary*)getLatestReceivedData;

+ (void)setLaunchMode:(RFLaunchModes)mode;

+ (void)displayContent:(NSString*)notificationId  completeHandler:(nullable void(^)(RFAction* _Nullable action))completeHandler;

+ (void)postMessage:(nonnull NSArray*)events variables:(nullable NSDictionary<NSString*, NSString*>*)variables standbyTime:(nullable NSNumber*)standbyTime completion:(nullable void (^)(RFResult* _Nonnull result, NSArray<NSString*>* _Nonnull eventPostIds))completion;

+ (void)cancelPosting:(nonnull NSString*)eventPostId completion:(nullable void (^)(RFResult* _Nonnull result))completion;

@end

NS_ASSUME_NONNULL_END

#endif /* RFPlugin_h */
