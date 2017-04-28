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





