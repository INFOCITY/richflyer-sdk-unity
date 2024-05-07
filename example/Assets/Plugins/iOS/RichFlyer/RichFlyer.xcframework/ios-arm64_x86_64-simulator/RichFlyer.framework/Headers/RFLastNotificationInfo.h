//
//  RFLastNotificationInfo.h
//  RichFlyer
//
//  Copyright © 2019年 INFOCITY,Inc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UserNotifications/UserNotifications.h>

@interface RFLastNotificationInfo : NSObject

@property (class, nullable, nonatomic, readonly) NSString* identifier;
@property (class, nullable, nonatomic, readonly) NSString* eventId;
@property (class, nullable, nonatomic, readonly) NSString* title;
@property (class, nullable, nonatomic, readonly) NSString* subTitle;

+ (void)reset;
+ (void)updateWithResponse:(nullable UNNotificationResponse*)response;

+ (void)saveLastNotificationId:(nonnull UNNotificationContent*)content appGroups:(nonnull NSString*)appGroups;
+ (nullable NSString*)loadLastNotificationId:(nonnull NSString*)appGroups;
+ (nullable NSString*)loadLastEventId:(nonnull NSString*)appGroups;


@end
