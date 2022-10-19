//
//  RFNotificationService.h
//  RichFlyer
//
//  Copyright © 2019年 INFOCITY,Inc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UserNotifications/UserNotifications.h>

@interface RFNotificationService : NSObject

+ (void)configureRFNotification: (nonnull UNMutableNotificationContent*)content appGroupId:(nonnull NSString*)appGroupId completeHandler:(nullable void(^)(UNMutableNotificationContent* _Nonnull content)) completeHandler;

+ (void)configureRFNotification: (nonnull UNMutableNotificationContent*)content appGroupId:(nonnull NSString*)appGroupId displayNavigate:(BOOL)displayNavigate completeHandler:(nullable void(^)(UNMutableNotificationContent* _Nonnull content)) completeHandler;

@end
