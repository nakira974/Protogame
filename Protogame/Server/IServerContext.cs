using Microsoft.Xna.Framework;
using Protoinject;

namespace Protogame
{
    using System;

    public interface IServerContext
    {
        /// <summary>
        /// Gets the current server tick.
        /// </summary>
        /// <value>The current server tick.</value>
        int Tick { get; set; }

        /// <summary>
        /// Gets the current time-based server tick.
        /// </summary>
        /// <remarks>
        /// While <see cref="Tick" /> is based on the number
        /// of individual Update loops the server makes, the
        /// time-based tick is the number of milliseconds
        /// since the server started.  This allows synchronisation
        /// of events with clients regardless of tick-rate, but
        /// you must inform clients on the start time that is
        /// set in the <see cref="StartTime"/> property.
        /// </remarks>
        /// <value>The time tick.</value>
        int TimeTick { get; set; }

        /// <summary>
        /// Gets or sets the time that the server started.
        /// </summary>
        /// <value>The server start time.</value>
        DateTime StartTime { get; set; }

        /// <summary>
        /// Gets the server.
        /// </summary>
        /// <value>The server.</value>
        Server Server { get; }

        /// <summary>
        /// Gets the current server world.
        /// </summary>
        /// <value>The world.</value>
        IServerWorld World { get; }

        /// <summary>
        /// Gets the current server world manager.
        /// </summary>
        /// <value>The world manager.</value>
        IServerWorldManager WorldManager { get; }

        /// <summary>
        /// Gets the dependency injection hierarchy, which contains all worlds, entities and components.
        /// </summary>
        /// <value>
        /// The dependency injection hierarchy, which contains all worlds, entities and components.
        /// </value>
        IHierarchy Hierarchy { get; }
        
        /// <summary>
        /// Gets the amount of game time elapsed since the last update step.
        /// </summary>
        /// <value>
        /// The elapsed game time since the last update step.
        /// </value>
        GameTime GameTime { get; set; }

        /// <summary>
        /// Called by the server at the beginning of a step, immediately before any update
        /// logic is processed.  This method is used by the engine, and should not be called
        /// from user code.
        /// </summary>
        void Begin();

        /// <summary>
        /// The create world.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IWorld"/>.
        /// </returns>
        IServerWorld CreateWorld<T>() where T : IServerWorld;

        /// <summary>
        /// The create world.
        /// </summary>
        /// <param name="creator">
        /// The creator.
        /// </param>
        /// <typeparam name="TFactory">
        /// </typeparam>
        /// <returns>
        /// The <see cref="IWorld"/>.
        /// </returns>
        IServerWorld CreateWorld<TFactory>(Func<TFactory, IServerWorld> creator);

        /// <summary>
        /// The switch world.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        void SwitchWorld<T>() where T : IServerWorld;

        /// <summary>
        /// The switch world.
        /// </summary>
        /// <param name="creator">
        /// The creator.
        /// </param>
        /// <typeparam name="TFactory">
        /// </typeparam>
        void SwitchWorld<TFactory>(Func<TFactory, IServerWorld> creator);

        /// <summary>
        /// The switch world.
        /// </summary>
        /// <param name="world">
        /// The world.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        void SwitchWorld<T>(T world) where T : IServerWorld;
    }
}

