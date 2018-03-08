using Akka;
using Akka.Actor;
using System;
using Xunit;
using Xunit.Abstractions;

namespace GliderGun.Actors.Tests
{
    /// <summary>
    ///     Tests for actor logging.
    /// </summary>
    public class LoggingTests
        : ActorTestBase
    {
		/// <summary>
		///		Create a new actor-logging test suite.
		/// </summary>
		/// <param name="testOutput">
		///		Output for the current test.
		/// </param>
        public LoggingTests(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

		/// <summary>
		///		Actor can log messages using classic-style format string.
		/// </summary>
		/// <remarks>
		///		AF: Currently, Serilog-style format strings are not properly supported by the TestKit infrastructure.
		/// </remarks>
        [Fact(DisplayName = "Actor can log messages using classic-style format string")]
        public void Can_Log_ClassicFormat()
        {
            IActorRef loggingTestActor = ActorOf(Props.Create(
                () => new LoggingTestActor()
            ));

            loggingTestActor.Tell("Test1");
            Within(TimeSpan.FromSeconds(2), () =>
            {
                EventFilter.Info("Recieved message 'Test1'.").Match();
                ExpectMsg<string>("Message 1 Received");
            });

            loggingTestActor.Tell("Test Two");
            Within(TimeSpan.FromSeconds(2), () =>
            {
                EventFilter.Info("Recieved message 'Test Two'.").Match();
                ExpectMsg<string>("Message 2 Received");
            });
        }
    }

	/// <summary>
	///		Actor used to test logging from actors.
	/// </summary>
    class LoggingTestActor
        : ReceiveActorEx
    {
		/// <summary>
		///		The number of messages that the actor has received.
		/// </summary>
        int _messageCount;
        
		/// <summary>
		///		Create a new <see cref="LoggingTestActor"/>.
		/// </summary>
        public LoggingTestActor()
        {
            Receive<string>(message =>
            {
                Log.Info("Recieved message '{0}'.", message);
                
                _messageCount++;
                Sender.Tell($"Message {_messageCount} Received");
            });
        }
    }
}
