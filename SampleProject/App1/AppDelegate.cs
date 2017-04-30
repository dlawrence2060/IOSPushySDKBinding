using Foundation;
using UIKit;
using System.Runtime.InteropServices;
using ObjCRuntime;
using System;
using PushySDK;

namespace App1
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIResponder, IUIApplicationDelegate
    {
        // class-level declarations
        public Pushy pushy;

        public UIWindow Window
        {
            get;
            set;
        }

        [DllImport("/usr/lib/libobjc.dylib")]
        static extern IntPtr _Block_copy(IntPtr ptr);

        [DllImport("/usr/lib/libobjc.dylib")]
        static extern void _Block_release(IntPtr ptr);

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
                invoker(blockPtr, (nuint)(UInt64)result);
            }
        }

        [Export("application:didFinishLaunchingWithOptions:")]
        public bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method
            var settings = UIUserNotificationSettings.GetSettingsForTypes(UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, null);
            UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);

            Window = new UIWindow();

            var viewController = new UIViewController();
            viewController.View.Frame = UIScreen.MainScreen.Bounds;
            viewController.View.BackgroundColor = UIColor.Orange;
            Window.RootViewController = viewController;

            Window.MakeKeyAndVisible();

            InitPushy(application);

            return true;
        }

        [Export("applicationWillResignActive:")]
        public void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        [Export("applicationDidEnterBackground:")]
        public void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        [Export("applicationWillEnterForeground:")]
        public void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        [Export("applicationDidBecomeActive:")]
        public void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
        }

        [Export("applicationWillTerminate:")]
        public void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }

        private void InitPushy(UIApplication application)
        {
            pushy = new Pushy(this, application);

            RegisterPushy();

            pushy.SetNotificationHandler(HandleNotification);
        }

        private void RegisterPushy()
        {
            pushy.Register((error, deviceToken) =>
            {
                if (error != null)
                {
                    Console.WriteLine("Registration failed:" + error.ToString());
                    return;
                }
                Console.WriteLine("Pushy token:" + deviceToken);
                NSUserDefaults.StandardUserDefaults.SetString(deviceToken, "PushyDeviceToken");
            });
        }

        [Export("handleNotification:completionHandler:")]
        internal unsafe void HandleNotification(NSDictionary info, IntPtr actionPtr)
        {
            string title = info["title"] != null ? info["title"].ToString() : "";

            var proxy = new BackgroundFetchResultHandlerProxy((BlockLiteral*)actionPtr);
            proxy.Invoke(UIBackgroundFetchResult.NewData);
        }
    }
}