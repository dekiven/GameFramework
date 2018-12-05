#import "STRIAPManager.h"
#import <StoreKit/StoreKit.h>

@interface STRIAPManager()<SKPaymentTransactionObserver,SKProductsRequestDelegate>{
    NSString           *_purchID;
    NSString           *_externalData;
    IAPCompletionHandle _handle;
}
@end
@implementation STRIAPManager

#pragma mark - ♻️life cycle
+ (instancetype)shareSIAPManager{
    
    static STRIAPManager *IAPManager = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken,^{
        IAPManager = [[STRIAPManager alloc] init];
    });
    return IAPManager;
}
- (instancetype)init{
    self = [super init];
    if (self) {
        // 购买监听写在程序入口,程序挂起时移除监听,这样如果有未完成的订单将会自动执行并回调 paymentQueue:updatedTransactions:方法
        [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
    }
    return self;
}

- (void)dealloc{
    [[SKPaymentQueue defaultQueue] removeTransactionObserver:self];
}


#pragma mark - 🚪public
- (void)startPurchWithID:(NSString *)purchID externalData:(NSString *)data completeHandle:(IAPCompletionHandle)handle{
    if (purchID) {
        if ([SKPaymentQueue canMakePayments]) {
            // 开始购买服务
            _purchID = purchID;
            _externalData = data;
            _handle = handle;
            NSSet *nsset = [NSSet setWithArray:@[purchID]];
            SKProductsRequest *request = [[SKProductsRequest alloc] initWithProductIdentifiers:nsset];
            request.delegate = self;
            [request start];
        }else{
            [self handleActionWithType:SIAPPurchNotArrow data:nil];
        }
    }
}
#pragma mark - 🔒private
- (void)handleActionWithType:(SIAPPurchType)type data:(NSDictionary *)data{
#if DEBUG
    switch (type) {
        case SIAPPurchSuccess:
            NSLog(@"购买成功");
            break;
        case SIAPPurchFailed:
            NSLog(@"购买失败");
            break;
        case SIAPPurchCancle:
            NSLog(@"用户取消购买");
            break;
        case SIAPPurchVerFailed:
            NSLog(@"订单校验失败");
            break;
        case SIAPPurchVerSuccess:
            NSLog(@"订单校验成功");
            break;
        case SIAPPurchNotArrow:
            NSLog(@"不允许程序内付费");
            break;
        default:
            break;
    }
#endif
    if(_handle){
        _handle(type,data);
    }
}
#pragma mark - 🍐delegate
// 交易结束
- (void)completeTransaction:(SKPaymentTransaction *)transaction{
//    // Your application should implement these two methods.
//    NSString * productIdentifier = transaction.payment.productIdentifier;
//    NSString * receipt = [transaction.transactionReceipt base64EncodedStringWithOptions:0];
//    if ([productIdentifier length] > 0) {
//        // 向自己的服务器验证购买凭证
//    }
    [self verifyPurchaseWithPaymentTransaction:transaction isTestServer:NO];
    
}

// 交易失败
- (void)failedTransaction:(SKPaymentTransaction *)transaction{
    if (transaction.error.code != SKErrorPaymentCancelled) {
        [self handleActionWithType:SIAPPurchFailed data:nil];
    }else{
        [self handleActionWithType:SIAPPurchCancle data:nil];
    }
    
    [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
}

//本地校验交易
- (void)checkLocal:(BOOL)flag receipt:(NSData *)receipt transaction:(SKPaymentTransaction *)transaction {
    NSError *error;
    NSDictionary *requestContents = @{
                                      @"receipt-data": [receipt base64EncodedStringWithOptions:0]
                                      };
    NSData *requestData = [NSJSONSerialization dataWithJSONObject:requestContents
                                                          options:0
                                                            error:&error];
    
    if (!requestData) { // 交易凭证为空验证失败
        [self handleActionWithType:SIAPPurchVerFailed data:nil];
        return;
    }
    
    //In the test environment, use https://sandbox.itunes.apple.com/verifyReceipt
    //In the real environment, use https://buy.itunes.apple.com/verifyReceipt
    
    NSString *serverString = @"https://buy.itunes.apple.com/verifyReceipt";
    if (flag) {
        serverString = @"https://sandbox.itunes.apple.com/verifyReceipt";
    }
    NSURL *storeURL = [NSURL URLWithString:serverString];
    NSMutableURLRequest *storeRequest = [NSMutableURLRequest requestWithURL:storeURL];
    [storeRequest setHTTPMethod:@"POST"];
    [storeRequest setHTTPBody:requestData];
    
    NSOperationQueue *queue = [[NSOperationQueue alloc] init];
    [NSURLConnection sendAsynchronousRequest:storeRequest queue:queue
                           completionHandler:^(NSURLResponse *response, NSData *data, NSError *connectionError) {
                               if (connectionError) {
                                   // 无法连接服务器,购买校验失败
                                   [self handleActionWithType:SIAPPurchVerFailed data:nil];
                               } else {
                                   NSError *error;
                                   NSDictionary *jsonResponse = [NSJSONSerialization JSONObjectWithData:data options:0 error:&error];
                                   if (!jsonResponse) {
                                       // 苹果服务器校验数据返回为空校验失败
                                       [self handleActionWithType:SIAPPurchVerFailed data:nil];
                                   }
                                   
                                   // 先验证正式服务器,如果正式服务器返回21007再去苹果测试服务器验证,沙盒测试环境苹果用的是测试服务器
                                   NSString *status = [NSString stringWithFormat:@"%@",jsonResponse[@"status"]];
                                   if (status && [status isEqualToString:@"21007"]) {
                                       [self verifyPurchaseWithPaymentTransaction:transaction isTestServer:YES];
                                   }else if(status && [status isEqualToString:@"0"]){
                                       [self handleActionWithType:SIAPPurchVerSuccess data:data];
                                   }
#if DEBUG
                                   NSLog(@"----验证结果 %@",jsonResponse);
#endif
                               }
                           }];
}

//交易验证
- (void)verifyPurchaseWithPaymentTransaction:(SKPaymentTransaction *)transaction isTestServer:(BOOL)flag{
    //交易验证
    NSURL *recepitURL = [[NSBundle mainBundle] appStoreReceiptURL];
    NSData *receipt = [NSData dataWithContentsOfURL:recepitURL];
    
    if(!receipt){
        // 交易凭证为空验证失败
        [self handleActionWithType:SIAPPurchVerFailed data:nil];
        return;
    }
    
    NSDictionary* dic =
  @{
        @"receipt":[receipt base64EncodedStringWithOptions:0],
        @"ransactionId":transaction.transactionIdentifier,
        @"productId":transaction.payment.productIdentifier,
        @"externalData":_externalData,
    };
    
    // 购买成功将交易凭证发送给服务端进行再次校验
    [self handleActionWithType:SIAPPurchSuccess data:dic];
    
    
//    //本地校验交易
//    [self checkLocal:flag receipt:receipt transaction:transaction];
    
    // 验证成功与否都注销交易,否则会出现虚假凭证信息一直验证不通过,每次进程序都得输入苹果账号
    [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
}

#pragma mark - SKProductsRequestDelegate
- (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response{
    NSArray *product = response.products;
    if([product count] <= 0){
#if DEBUG
        NSLog(@"--------------没有商品------------------");
#endif
        return;
    }
    
    SKProduct *p = nil;
    for(SKProduct *pro in product){
        if([pro.productIdentifier isEqualToString:_purchID]){
            p = pro;
            break;
        }
    }
    
#if DEBUG
    NSLog(@"productID:%@", response.invalidProductIdentifiers);
    NSLog(@"产品付费数量:%lu",(unsigned long)[product count]);
    NSLog(@"%@",[p description]);
    NSLog(@"%@",[p localizedTitle]);
    NSLog(@"%@",[p localizedDescription]);
    NSLog(@"%@",[p price]);
    NSLog(@"%@",[p productIdentifier]);
    NSLog(@"发送购买请求");
#endif
    
    SKPayment *payment = [SKPayment paymentWithProduct:p];
    [[SKPaymentQueue defaultQueue] addPayment:payment];
}

//请求失败
- (void)request:(SKRequest *)request didFailWithError:(NSError *)error{
#if DEBUG
    NSLog(@"------------------错误-----------------:%@", error);
#endif
}

- (void)requestDidFinish:(SKRequest *)request{
#if DEBUG
    NSLog(@"------------反馈信息结束-----------------");
#endif
}

#pragma mark - SKPaymentTransactionObserver
- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray<SKPaymentTransaction *> *)transactions{
    for (SKPaymentTransaction *tran in transactions) {
        switch (tran.transactionState) {
            case SKPaymentTransactionStatePurchased:
                [self completeTransaction:tran];
                break;
            case SKPaymentTransactionStatePurchasing:
#if DEBUG
                NSLog(@"商品添加进列表");
#endif
                break;
            case SKPaymentTransactionStateRestored:
#if DEBUG
                NSLog(@"已经购买过商品");
#endif
                // 消耗型不支持恢复购买
                [[SKPaymentQueue defaultQueue] finishTransaction:tran];
                break;
            case SKPaymentTransactionStateFailed:
                [self failedTransaction:tran];
                break;
            default:
                break;
        }
    }
}
@end

//在控制器中调用，导入头文件
//调用方法
//- (void)purchaseAction{
//
//    if (!_IAPManager) {
//        _IAPManager = [STRIAPManager shareSIAPManager];
//    }
//    // iTunesConnect 苹果后台配置的产品ID
//    [_IAPManager startPurchWithID:@"com.bb.helper_advisory" completeHandle:^(SIAPPurchType type,NSData *data) {
//        //请求事务回调类型，返回的数据
//
//    }];
//}
