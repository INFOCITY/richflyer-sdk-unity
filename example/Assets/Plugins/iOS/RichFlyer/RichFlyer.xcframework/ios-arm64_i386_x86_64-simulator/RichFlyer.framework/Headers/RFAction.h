//
//  RFAction.h
//  RichFlyer
//
//  Copyright © 2019年 INFOCITY,Inc. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_CLASS_AVAILABLE_IOS(10_0)
@interface RFAction : NSObject

- (nullable id)initWithTitle: (nullable NSString*)title type: (nullable NSString*)type value: (nullable NSString*)value index: (NSUInteger)index;

- (nullable NSString*)getTitle;
- (nullable NSString*)getType;
- (nullable NSString*)getValue;
- (NSUInteger)getIndex;

@end
