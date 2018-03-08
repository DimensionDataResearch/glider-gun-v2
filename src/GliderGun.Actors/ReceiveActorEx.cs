using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;

namespace GliderGun.Actors
{
    /// <summary>
    ///     The base class for Receive-style actors.
    /// </summary>
    public abstract class ReceiveActorEx
        : ReceiveActor
    {
        /// <summary>
        ///     Create a new <see cref="ReceiveActorEx"/>.
        /// </summary>
        protected ReceiveActorEx()
        {
        }

        /// <summary>
        ///     The actor's logging facility.
        /// </summary>
        protected ILoggingAdapter Log { get; } = Context.GetLogger<SerilogLoggingAdapter>();
    }
}
