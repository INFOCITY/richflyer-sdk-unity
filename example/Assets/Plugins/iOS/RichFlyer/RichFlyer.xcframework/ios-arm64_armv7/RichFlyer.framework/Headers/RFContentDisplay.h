//
//  RFContentDisplay.h
//  RichFlyer
//
//  Copyright © 2019年 INFOCITY,Inc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

#import "RFContent.h"

NS_ASSUME_NONNULL_BEGIN

@interface RFContentDisplay : NSObject

- (id)init;
- (id)initWithContent:(nonnull RFContent*)content;

- (void)setContent:(nonnull RFContent*)content;
- (void)setButtonColor:(nonnull UIColor*)color;

- (void)present:(nonnull UIViewController*)parent completeHandler:(nullable void(^)(RFAction* _Nullable action))completeHandler;

- (void)dismiss;

@end

NS_ASSUME_NONNULL_END
