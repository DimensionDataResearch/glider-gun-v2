using Xunit.Abstractions;

using TestKit = Akka.TestKit.Xunit2.TestKit;

namespace GliderGun.Actors.Tests
{
    /// <summary>
    ///     The base class for actor test suites.
    /// </summary>
    public abstract class ActorTestBase
        : TestKit
    {
		/// <summary>
        ///     Create a new <see cref="ActorTestBase"/>.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        protected ActorTestBase(ITestOutputHelper testOutput)
            : base(output: testOutput)
        {
		}
    }
}
