//
//  RFContent.h
//  RichFlyer
//
//  Copyright © 2019年 INFOCITY,Inc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UserNotifications/UserNotifications.h>

#import "RFAction.h"

NS_ASSUME_NONNULL_BEGIN

typedef NS_ENUM(NSUInteger, RFContentType) {
	RFContentTypeText,	//テキストのみ
	RFContentTypeImage, //静止画像
	RFContentTypeGif,   //GIF画像
	RFContentTypeMovie  //動画
} NS_ENUM_AVAILABLE_IOS(9_0);

@interface RFContent : NSObject

@property (nonatomic, readonly) NSString* title;
@property (nonatomic, readonly) NSString* body;
@property (nonatomic, readonly) NSString* notificationId;
@property (nonatomic, readonly) NSString* eventId;
@property (nonatomic, readonly) NSURL* imagePath;
@property (nonatomic, readonly) NSDate* receivedDate;
@property (nonatomic, readonly) NSDate* notificationDate;

@property (nonatomic, readonly) NSArray<RFAction*>* actionButtons;

@property (readonly) RFContentType type;

- (id)initWithUNNotificationContent:(nonnull UNNotificationContent*)content receivedDate:(nullable NSDate*)receivedDate;
- (nullable UNNotificationContent*)convertToUNNotificationContent;

- (BOOL)hasImage;
- (BOOL)hasGif;
- (BOOL)hasMovie;

- (BOOL)isEvent;

@end

NS_ASSUME_NONNULL_END
