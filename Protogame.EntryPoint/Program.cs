#if PLATFORM_WINDOWS || PLATFORM_MACOS || PLATFORM_LINUX || PLATFORM_WEB || PLATFORM_IOS

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Protogame;
using Protoinject;

#if PLATFORM_MACOS
#if PLATFORM_MACOS_LEGACY
using MonoMac.AppKit;
using MonoMac.Foundation;
#else
using AppKit;
using Foundation;
#endif

public static class Program
{
	public static void Main(string[] args)
	{
		NSApplication.Init();

		using (var p = new NSAutoreleasePool())
		{
			NSApplication.SharedApplication.Delegate = new AppDelegate();

			// TODO: Offer a way of setting the application icon.
			//NSImage appIcon = NSImage.ImageNamed("monogameicon.png");
			//NSApplication.SharedApplication.ApplicationIconImage = appIcon;

			NSApplication.Main(args);
		}
	}
}

public class AppDelegate : NSApplicationDelegate
{
	public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
	{
		return true;
	}

#if PLATFORM_MACOS_LEGACY
	public override void FinishedLaunching(NSObject notification)
#else
	public override void DidFinishLaunching(NSNotification notification)
#endif
	{
		var args = new string[0];
#elif PLATFORM_IOS
using Foundation;
using UIKit;

[Register("AppDelegate")]
public class Program : UIApplicationDelegate
{
    public static void Main(string[] args)
    {
        UIApplication.Main(args, null, "AppDelegate");
    }

    public override void FinishedLaunching(UIApplication app)
    {
        var args = new string[0];
#else
public static class Program
{
    public static void Main(string[] args)
    {
#endif
        var kernel = new StandardKernel();
        
		Func<System.Reflection.Assembly, Type[]> TryGetTypes = assembly =>
		{
			try
			{
				return assembly.GetTypes();
			}
			catch
			{
				return new Type[0];
			}
		};

        // Search the application domain for implementations of
        // the IGameConfiguration.
        var gameConfigurations =
            (from assembly in AppDomain.CurrentDomain.GetAssemblies()
				from type in TryGetTypes(assembly)
            where typeof (IGameConfiguration).IsAssignableFrom(type) &&
                  !type.IsInterface && !type.IsAbstract
            select Activator.CreateInstance(type) as IGameConfiguration).ToList();

#if PLATFORM_WINDOWS || PLATFORM_MACOS || PLATFORM_LINUX
        // Search the application domain for implementations of
        // the IServerConfiguration.
        var serverConfigurations =
            (from assembly in AppDomain.CurrentDomain.GetAssemblies()
             from type in TryGetTypes(assembly)
             where typeof(IServerConfiguration).IsAssignableFrom(type) &&
               !type.IsInterface && !type.IsAbstract
             select Activator.CreateInstance(type) as IServerConfiguration).ToList();

        if (gameConfigurations.Count == 0 && serverConfigurations.Count == 0)
        {
            throw new InvalidOperationException(
                "You must have at least one implementation of " +
                "IGameConfiguration or IServerConfiguration in your game.");
        }
#else
        if (gameConfigurations.Count == 0)
        {
            throw new InvalidOperationException(
                "You must have at least one implementation of " +
                "IGameConfiguration in your game.");
        }
#endif

        Game game = null;
#if PLATFORM_WINDOWS || PLATFORM_MACOS || PLATFORM_LINUX
        ICoreServer server = null;
#endif

        foreach (var gameConfiguration in gameConfigurations)
        {
            gameConfiguration.ConfigureKernel(kernel);

            // It is expected that the AssetManagerProvider architecture will
            // be refactored in future to just provide IAssetManager directly,
            // and this method call will be dropped.
            gameConfiguration.InitializeAssetManagerProvider(new AssetManagerProviderInitializer(kernel, args));

            // We only construct one game.  In the event there are
            // multiple game configurations (such as a third-party library
            // providing additional game tools, it's expected that libraries
            // will return null for ConstructGame).
            if (game == null)
            {
                game = gameConfiguration.ConstructGame(kernel);
            }
        }

#if PLATFORM_WINDOWS || PLATFORM_MACOS || PLATFORM_LINUX

        foreach (var serverConfiguration in serverConfigurations)
        {
            serverConfiguration.ConfigureKernel(kernel);

            // It is expected that the AssetManagerProvider architecture will
            // be refactored in future to just provide IAssetManager directly,
            // and this method call will be dropped.
            serverConfiguration.InitializeAssetManagerProvider(new AssetManagerProviderInitializer(kernel, args));

            // We only construct one server.  In the event there are
            // multiple server configurations (such as a third-party library
            // providing additional game tools, it's expected that libraries
            // will return null for ConstructServer).
            if (server == null)
            {
                server = serverConfiguration.ConstructServer(kernel);
            }
        }

        if (game == null && server == null)
        {
            throw new InvalidOperationException(
                "No implementation of IGameConfiguration provided " +
                "a game instance from ConstructGame, and " +
                "no implementation of IServerConfiguration provided " +
                "a server instance from ConstructServer.");
        }

        if (game != null && server != null)
        {
            throw new InvalidOperationException(
                "An implementation of IGameConfiguration provided " +
                "a game instance, and an implementation of " +
                "IServerConfiguration provided a server instance.  " +
                "It's not possible to determine whether the game or " +
                "server should be started with this configuration.  " +
                "Move one of the implementations so that it's not in " +
                "the default loaded application domain.");
        }

#else

        if (game == null)
        {
            throw new InvalidOperationException(
                "No implementation of IGameConfiguration provided " +
                "a game instance from ConstructGame.");
        }

#endif

#if PLATFORM_WINDOWS || PLATFORM_MACOS || PLATFORM_LINUX
        if (game != null)
        {
#endif

#if PLATFORM_MACOS || PLATFORM_IOS
		game.Run();
#else
        using (var runningGame = game)
        {
            runningGame.Run();
        }
#endif

#if PLATFORM_WINDOWS || PLATFORM_MACOS || PLATFORM_LINUX
        }
        else if (server != null)
        {
            server.Run();
        }
#endif
    }
}

#endif
