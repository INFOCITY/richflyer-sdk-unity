//
//  RFReceiverBridge.m
//  UnityFramework
//
//  Created by 後藤武 on 2022/08/17.
//

#import "RFReceiverBridge.h"
#import "RFPlugin.h"

@implementation RFReceiverBridge

+ (void)receiveNotification:(UNNotificationResponse*)response {  
  [RFPlugin receiveNotification:response];
}

@end
