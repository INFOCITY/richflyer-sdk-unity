//
//  RFPlugin.m
//  iOSPlugin
//
//  Created by 後藤武 on 2022/08/02.
//

#import "RFPlugin.h"

#import <Foundation/Foundation.h>

#ifdef __cplusplus
extern "C" {
#endif

  typedef void (*RFNotificationReceiverFunction)(const char* buttonTitle, const char* buttonValue, const char* buttonValueType, unsigned long buttonIndex, const char* extendedProperty);
  static RFNotificationReceiverFunction RFNotificationReceiver;

  typedef void (*RFCallbackFunction)(bool result, long status, const char* message);
  static RFCallbackFunction RFCompleted;

  typedef void (*RFContentDisplayCallbackFunction)(const char* buttonTitle, const char* buttonValue, const char* buttonValueType, unsigned long buttonIndex);
  static RFContentDisplayCallbackFunction RFContentDisplayCallback;

  void registReceiver(RFNotificationReceiverFunction callback) {
    RFNotificationReceiver = callback;
  }

  void resetBadgeNumber() {
    [RFPlugin resetBadgeNumber];
  }

  void setBadgeNumber(int number) {
    [RFPlugin setBadgeNumber:number];
  }

  void registSegments(const char* segmentsJson, RFCallbackFunction callback) {
        
    NSData *data = [[NSString stringWithUTF8String:segmentsJson] dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary* dictionary = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    
    NSArray* srcSegments = dictionary[@"Segments"];
    NSMutableDictionary* registabaleSegments = [NSMutableDictionary dictionary];
    for (NSDictionary* segment in srcSegments) {
      [registabaleSegments setObject:segment[@"Value"] forKey:segment[@"Name"]];
    }
    
    RFCompleted = callback;
    [RFPlugin registSegments:registabaleSegments completion:^(RFResult* result) {
      RFCompleted(result.result, result.code, [result.message UTF8String]);
    }];
  }

  const char* getSegments() {
    
    NSDictionary* srcSegments = [RFPlugin getSegments];
    
    NSMutableArray* segments = [NSMutableArray array];
    for (NSString* key in srcSegments.allKeys) {
      NSString* value = srcSegments[key];
      NSDictionary* segment = @{@"Name":key, @"Value":value};
      [segments addObject:segment];
    }

    NSDictionary* responseSegments = @{@"Segments": segments};

    NSData* data = [NSJSONSerialization dataWithJSONObject:responseSegments options:0 error:nil];
    
    NSString* json = [[NSString alloc]initWithData:data encoding:NSUTF8StringEncoding];
    
    const char* jsonStr = [json UTF8String];
    
    char* value = (char*)malloc(strlen(jsonStr)+1);
    strcpy(value, jsonStr);
    
    return value;
  }

  const char* getReceivedData() {
    NSDictionary* contents = [RFPlugin getReceivedData];
    if (!contents) return NULL;

    NSData* data = [NSJSONSerialization dataWithJSONObject:contents options:0 error:nil];
    NSString* json = [[NSString alloc]initWithData:data encoding:NSUTF8StringEncoding];
    
    const char* jsonStr = [json UTF8String];
    char* value = (char*)malloc(strlen(jsonStr)+1);
    strcpy(value, jsonStr);
    
    return value;

  }

  const char* getLatestReceivedData() {

    NSDictionary* content = [RFPlugin getLatestReceivedData];
    if (!content) return NULL;

    NSData* data = [NSJSONSerialization dataWithJSONObject:content options:0 error:nil];
    NSString* json = [[NSString alloc]initWithData:data encoding:NSUTF8StringEncoding];
    
    const char* jsonStr = [json UTF8String];
    char* value = (char*)malloc(strlen(jsonStr)+1);
    strcpy(value, jsonStr);
    
    return value;

  }

  void setLaunchMode(int mode) {
    [RFPlugin setLaunchMode:mode];
  }

  void displayContent(const char* notificationId, RFContentDisplayCallbackFunction callback) {
    
    RFContentDisplayCallback = callback;
    
    [RFPlugin displayContent:[NSString stringWithUTF8String:notificationId]
             completeHandler:^(RFAction* action) {
      
      const char* title = action.getTitle ? [action.getTitle UTF8String] : NULL;
      const char* value = action.getValue ? [action.getValue UTF8String] : NULL;
      const char* type = action.getType ? [action.getType UTF8String] : NULL;
      unsigned long index = action.getIndex;

      if (RFContentDisplayCallback) {
        RFContentDisplayCallback(title, value, type, index);
      }
    }];
  }

#ifdef __cplusplus
}
#endif


@implementation RFPlugin


+ (void)receiveNotification:(UNNotificationResponse*)response {

  if (!response) return;
  
  [RFApp didReceiveNotification:response handler:^(RFAction* action, NSString* extendedProperty){
    if (RFNotificationReceiver) {
      const char* title = action.getTitle ? [action.getTitle UTF8String] : NULL;
      const char* value = action.getValue ? [action.getValue UTF8String] : NULL;
      const char* type = action.getType ? [action.getType UTF8String] : NULL;
      unsigned long index = action.getIndex;
      const char* extendedPropertyCstr = extendedProperty ? [extendedProperty UTF8String] : NULL;
      RFNotificationReceiver(title, value, type, index, extendedPropertyCstr);
    }
  }];
}


+ (void)resetBadgeNumber {
  [RFApp resetBadgeNumber:[UIApplication sharedApplication]];
}

+ (void)setBadgeNumber:(int)number {
  [RFApp setBadgeNumber:[UIApplication sharedApplication] badge:number];
}

+ (void)registSegments:(NSDictionary<NSString*, NSString*>*)segments completion:(void (^)(RFResult* result))completion{
  [RFApp registSegments:segments completion:completion];
}

+ (NSDictionary*)getSegments {
  return [RFApp getSegments];
}


+ (NSDictionary*)getReceivedData {
  NSArray* contents = [RFApp getReceivedData];
  
  NSMutableArray* convertedContents = [NSMutableArray array];
  for (RFContent* content in contents) {
    NSDictionary* convertedContent = [RFPlugin convertDictionary:content];
    [convertedContents addObject:convertedContent];
  }
  
  return @{@"Contents": convertedContents};
}

+ (NSDictionary*)getLatestReceivedData {
  RFContent* content = [RFApp getLatestReceivedData];
  if (!content) return nil;
  
  return [RFPlugin convertDictionary:content];
}

+ (void)setLaunchMode:(RFLaunchModes)mode {
  [RFApp setLaunchMode:mode];
}

+ (void)displayContent:(NSString*)notificationId completeHandler:(nullable void(^)(RFAction* _Nullable action))completeHandler
{
  NSArray* contents = [RFApp getReceivedData];
  RFContent* useContent = nil;
  for (RFContent* content in contents) {
    if ([content.notificationId isEqualToString:notificationId]) {
      useContent = content;
      break;
    }
  }
  
  if (!useContent) {
    completeHandler(nil);
    return;
  }
  
  UIViewController *topController = [UIApplication sharedApplication].keyWindow.rootViewController;
  while (topController.presentedViewController) {
    topController = topController.presentedViewController;
  }

  RFContentDisplay* rfDisplay = [[RFContentDisplay alloc] initWithContent:useContent];
  [rfDisplay present:topController completeHandler:^(RFAction* action){
    completeHandler(action);
    [rfDisplay dismiss];
  }];
}

+ (NSDictionary*)convertDictionary:(RFContent*)content {

  if (!content) return nil;
  
  NSMutableDictionary* contentDictionary = [NSMutableDictionary dictionary];
  if (content.title) {
    [contentDictionary setObject:content.title forKey:@"Title"];
  }
  if (content.body) {
    [contentDictionary setObject:content.body forKey:@"Body"];
  }
  if (content.notificationId) {
    [contentDictionary setObject:content.notificationId forKey:@"NotificationId"];
  }
  if (content.imagePath) {
    [contentDictionary setObject:[content.imagePath absoluteString] forKey:@"ImagePath"];
  }
  if (content.receivedDate) {
    NSTimeInterval receivedDateUnix = [content.receivedDate timeIntervalSince1970];
    [contentDictionary setObject:[NSNumber numberWithDouble:receivedDateUnix] forKey:@"ReceivedDateUnixTime"];
  }
  
  if (content.notificationDate) {
    NSTimeInterval notificationDateUnix = [content.notificationDate timeIntervalSince1970];
    [contentDictionary setObject:[NSNumber numberWithDouble:notificationDateUnix] forKey:@"NotificationDateUnixTime"];
  }

  [contentDictionary setObject:[NSNumber numberWithUnsignedInteger:content.type] forKey:@"Type"];


  if (content.actionButtons) {
    
    NSMutableArray* actions = [NSMutableArray array];
    for (RFAction* actionButton in content.actionButtons) {
      NSString *title = [actionButton getTitle];
      NSString *type = [actionButton getType];
      NSString *value = [actionButton getValue];
      NSUInteger index = [actionButton getIndex];

      NSMutableDictionary* action = [NSMutableDictionary dictionary];
      if (title) {
        [action setObject:title forKey:@"Title"];
      }
      if (type) {
        [action setObject:type forKey:@"Type"];
      }
      if (value) {
        [action setObject:value forKey:@"Value"];
      }
      [action setObject:[NSNumber numberWithUnsignedInteger:index] forKey:@"Index"];
      [actions addObject:action];
    }
    [contentDictionary setObject:actions forKey:@"ActionButtons"];
  }

  return contentDictionary;
}

@end


#ifdef __cplusplus
extern "C" {
#endif

//  typedef void (*RFNotificationReceiverFunction)(const char* message);
//  static RFNotificationReceiverFunction RFNotificationReceiver;



#ifdef __cplusplus
}
#endif
