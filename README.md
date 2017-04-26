# IOSPushySDKBinding
Xamarin Ios bindings library for PushySDK

To get the callback working for SetNotificationHandler there needs to be some changes made to the code in App delegate of your ios project

The notification handler needs to be a insternal safe function so create the function like this inside of your appDelegate class

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

As well as an intermal class for the proxy which will invoke the callback

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






