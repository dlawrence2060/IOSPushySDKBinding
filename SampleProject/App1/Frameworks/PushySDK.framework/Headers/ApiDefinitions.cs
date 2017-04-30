using System;
using Foundation;
using UIKit;

// @interface Pushy : NSObject
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
interface Pushy
{
	// -(instancetype _Nonnull)init:(UIResponder * _Nonnull)appDelegate application:(UIApplication * _Nonnull)application __attribute__((objc_designated_initializer));
	[Export ("init:application:")]
	[DesignatedInitializer]
	IntPtr Constructor (UIResponder appDelegate, UIApplication application);

	// -(void)setNotificationHandler:(void (^ _Nonnull)(NSDictionary * _Nonnull, void (^ _Nonnull)(UIBackgroundFetchResult)))notificationHandler;
	[Export ("setNotificationHandler:")]
	void SetNotificationHandler (Action<NSDictionary, Action<UIBackgroundFetchResult>> notificationHandler);

	// -(void)register:(void (^ _Nonnull)(NSError * _Nullable, NSString * _Nonnull))registrationHandler;
	[Export ("register:")]
	void Register (Action<NSError, NSString> registrationHandler);
}
