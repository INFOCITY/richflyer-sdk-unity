//
//  NotificationService.m
//  NotificationService
//
//  Copyright © 2019 INFOCITY,Inc. All rights reserved.
//

#import "NotificationService.h"
#import <RichFlyer/RichFlyer.h>
@interface NotificationService ()

@property (nonatomic, strong) void (^contentHandler)(UNNotificationContent *contentToDeliver);
@property (nonatomic, strong) UNMutableNotificationContent *bestAttemptContent;

@end

@implementation NotificationService

- (void)didReceiveNotificationRequest:(UNNotificationRequest *)request withContentHandler:(void (^)(UNNotificationContent * _Nonnull))contentHandler {
    self.contentHandler = contentHandler;
    self.bestAttemptContent = [request.content mutableCopy];

    NSDictionary* infoPlist = [[NSBundle mainBundle] infoDictionary];
    NSDictionary* rfItem = infoPlist[@"RichFlyer"];
    if (rfItem) {
      NSString* groupId = rfItem[@"groupId"];
      BOOL displayNavigate = NO;
      NSNumber* naviValue = rfItem[@"displayNavigate"];
      if (naviValue) {
        displayNavigate = [naviValue boolValue];
      }

      [RFNotificationService configureRFNotification:self.bestAttemptContent
                                          appGroupId:groupId
                                     displayNavigate:displayNavigate
                                     completeHandler:^(UNMutableNotificationContent *content){
        self.bestAttemptContent = content;
        self.contentHandler(self.bestAttemptContent);
      }];
    } else {
      self.contentHandler(self.bestAttemptContent);
    }
}

- (void)serviceExtensionTimeWillExpire {
    // Called just before the extension will be terminated by the system.
    // Use this as an opportunity to deliver your "best attempt" at modified content, otherwise the original push payload will be used.
    self.contentHandler(self.bestAttemptContent);
}

@end
