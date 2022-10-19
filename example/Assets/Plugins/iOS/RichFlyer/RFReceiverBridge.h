//
//  RFReceiverBridge.h
//  UnityFramework
//
//  Created by 後藤武 on 2022/08/17.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface RFReceiverBridge : NSObject

+ (void)receiveNotification:(UNNotificationResponse*)response;

@end

NS_ASSUME_NONNULL_END
