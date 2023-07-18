//
//  RFAlertController.h
//  RichFlyer
//
//  Copyright © 2019年 INFOCITY,Inc. All rights reserved.
//

#import <UIKit/UIKit.h>

@interface RFAlertController : UIViewController

- (nullable id)initWithApplication:(nonnull UIApplication*)application title:(nonnull NSString*)title message:(nonnull NSString*)message;

- (nullable RFAlertController*)addView:(nonnull UIView*)view;
- (nullable RFAlertController*)addImage:(nonnull NSString*)imageName;

- (void)present:(nonnull UIViewController*)parent completeHandler:(nullable void(^)(void))completeHandler;
- (void)present:(nullable void(^)(void))completeHandler;

@end
