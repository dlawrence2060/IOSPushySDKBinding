# IOSPushySDKBinding
Xamarin Ios bindings library for PushySDK

To get the callback working for SetNotificationHandler there needs to be some changes made to the code in App delegate of your ios project

The notification handler needs to be a internal safe function so create the function like this inside of your appDelegate class

	[Export("handleNotification:completionHandler:")]
	internal unsafe void HandleNotification(NSDictionary info, IntPtr actionPtr)
	{
		//Handle notification info

		//Call callback
		var proxy = new BackgroundFetchResultHandlerProxy((BlockLiteral *)actionPtr);
		proxy.Invoke(UIBackgroundFetchResult.NewData);
	}

You will also need to delcare the callback delegate inside of the appDelegate class

	[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
	internal delegate void DBackgroundFetchResultHandler(IntPtr blockPtr, nuint result);

As well as an internal class for the proxy which will invoke the callback

	internal class BackgroundFetchResultHandlerProxy
	{
		IntPtr blockPtr;
		DBackgroundFetchResultHandler invoker;

		[Preserve(Conditional = true)]
		public unsafe BackgroundFetchResultHandlerProxy(BlockLiteral* block)
		{
			blockPtr = _Block_copy((IntPtr)block);
			invoker = block->GetDelegateForBlock<DBackgroundFetchResultHandler>();
		}

		[Preserve(Conditional = true)]
		~BackgroundFetchResultHandlerProxy()
		{
			_Block_release(blockPtr);
		}

		[Preserve(Conditional = true)]
		internal unsafe void Invoke(UIBackgroundFetchResult result)
		{
			invoker(blockPtr, (nuint) (UInt64) result);
		}
	}

Then inside your didFinishLaunching method change your Pushy.setNotifcicationHandler((info,action) => {} to

	Pushy.SetNotificationHandler(HandleNotification); 

You will also need to include these in the appDelegate class

	using System.Runtime.InteropServices;
	using ObjCRuntime;

and declare these to AppDelegateClass as well

	[DllImport("/usr/lib/libobjc.dylib")]
	static extern IntPtr _Block_copy(IntPtr ptr);

	[DllImport("/usr/lib/libobjc.dylib")]
	static extern void _Block_release(IntPtr ptr);

Your Project using this dll needs to include some swift libraries as the PushySDK was written in swift. 

    Xamarin.Swift3, Version=3.0.0.0, 
    Xamarin.Swift3.Core, Version=3.0.2.0, 
    Xamarin.Swift3.CoreAudio" Version=3.0.2.0>
    Xamarin.Swift3.CoreData, Version=3.0.2.0, 
    Xamarin.Swift3.CoreGraphics, Version=3.0.2.0, 
    Xamarin.Swift3.CoreImage, Version=3.0.2.0, 
    Xamarin.Swift3.CoreMedia, Version=3.0.2.0, 
    Xamarin.Swift3.Darwin, Version=3.0.2.0, 
    Xamarin.Swift3.Dispatch, Version=3.0.2.0, 
    Xamarin.Swift3.Foundation, Version=3.0.2.0, 
    Xamarin.Swift3.Intents, Version=3.0.2.0, 
    Xamarin.Swift3.ObjectiveC, Version=3.0.2.0, 
    Xamarin.Swift3.OS, Version=3.0.2.0, 
    Xamarin.Swift3.QuartzCore, Version=3.0.2.0, 
    Xamarin.Swift3.UIKit, Version=3.0.2.0, 

These can be found at https://www.nuget.org

Sample AppDelegate.cs below

	using Foundation;
	using UIKit;
	using PushySDK;
	using System;
	using System.Runtime.InteropServices;
	using ObjCRuntime;
	using UserNotifications;
	using Firebase.Analytics;

	namespace ugs_mobile_app.iOS
	{
		[Register("AppDelegate")]
		public class AppDelegate : UIResponder, IUIApplicationDelegate, IDisposable
		{
			[DllImport("/usr/lib/libobjc.dylib")]
			static extern IntPtr _Block_copy(IntPtr ptr);

			[DllImport("/usr/lib/libobjc.dylib")]
			static extern void _Block_release(IntPtr ptr);

			public AppDelegate(IntPtr p) : base(p) { }

			public UIWindow Window
			{
				get; set;
			}

			Pushy pushy;
			[Export("application:didFinishLaunchingWithOptions:")]
			public bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
			{
				Console.WriteLine("FinishedLaunching()");
				App.Configure();
				Analytics.LogEvent(EventNamesConstants.AppOpen, null);

				if (GetVersion() >= 10)
				{
					UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) => {
						// Handle approval
					});
					UNUserNotificationCenter.Current.Delegate = new NotificationManager();
				}
				else
				{
					var settings = UIUserNotificationSettings.GetSettingsForTypes(UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, null);
					UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
				}

				NotificationManager.HandleNotificationLaunchOptions(launchOptions);
			
				Window = new UIWindow();
				UIViewController controller = UIStoryboard.FromName("Main",null).InstantiateViewController("mainViewController");
				controller.View.Frame = UIScreen.MainScreen.Bounds;
				Window.RootViewController = controller;
				Window.MakeKeyAndVisible();

				Pushy pushy = new Pushy(this, application);

				pushy.Register((error, deviceToken) =>
				{
					if (error != null)
					{
						Console.WriteLine("Registration failed:" + error.ToString());
						return;
					}

					Console.WriteLine("Pushy token:" + deviceToken);
					NSUserDefaults.StandardUserDefaults.SetString(deviceToken, "pushyToken");
				});

				pushy.SetNotificationHandler(HandleNotification);


				return true;
			}

			[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
			internal delegate void DBackgroundFetchResultHandler(IntPtr blockPtr, nuint result);

			internal class BackgroundFetchResultHandlerProxy
			{
				IntPtr blockPtr;
				DBackgroundFetchResultHandler invoker;

				[Preserve(Conditional = true)]
				public unsafe BackgroundFetchResultHandlerProxy(BlockLiteral* block)
				{
					blockPtr = _Block_copy((IntPtr)block);
					invoker = block->GetDelegateForBlock<DBackgroundFetchResultHandler>();
				}

				[Preserve(Conditional = true)]
				~BackgroundFetchResultHandlerProxy()
				{
					_Block_release(blockPtr);
				}

				[Preserve(Conditional = true)]
				internal unsafe void Invoke(UIBackgroundFetchResult result)
				{
					invoker(blockPtr, (nuint) (UInt64) result);
				}
			}

			[Export("handleNotification:completionHandler:")]
			internal unsafe void HandleNotification(NSDictionary info, IntPtr actionPtr)
			{

				//Handle notification info

				//callback
				var proxy = new BackgroundFetchResultHandlerProxy((BlockLiteral *)actionPtr);
				proxy.Invoke(UIBackgroundFetchResult.NewData);
			}

			[Export("applicationWillResignActive:")]
			public void OnResignActivation(UIApplication application)
			{
				// Invoked when the application is about to move from active to inactive state.
				// This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
				// or when the user quits the application and it begins the transition to the background state.
				// Games should use this method to pause the game.
				Console.WriteLine("OnResignActivation()");
			}

			[Export("applicationDidEnterBackground:")]
			public void DidEnterBackground(UIApplication application)
			{
				// Use this method to release shared resources, save user data, invalidate timers and store the application state.
				// If your application supports background exection this method is called instead of WillTerminate when the user quits.
				Console.WriteLine("DidEnterBackground()");
			}

			[Export("applicationWillEnterForeground:")]
			public void WillEnterForeground(UIApplication application)
			{
				// Called as part of the transiton from background to active state.
				// Here you can undo many of the changes made on entering the background.
				Console.WriteLine("WillEnterForeground()");
			}

			[Export("applicationDidBecomeActive:")]
			public void OnActivated(UIApplication application)
			{
				// Restart any tasks that were paused (or not yet started) while the application was inactive. 
				// If the application was previously in the background, optionally refresh the user interface.
				Console.WriteLine("OnActivated()");
			}

			[Export("applicationWillTerminate:")]
			public void WillTerminate(UIApplication application)
			{
				// Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
				Console.WriteLine("WillTerminate()");
			}

			public static int GetVersion()
			{
				string version = UIDevice.CurrentDevice.SystemVersion.Split('.')[0];
				return Int32.Parse(version);
			}

		}
	}










