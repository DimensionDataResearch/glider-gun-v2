using Akka;
using Akka.Actor;
using KubeClient;
using System;

namespace GliderGun.Actors.Pods
{
    /// <summary>
    ///     Actor that publishes log entries from a container in a Kubernetes Pod.
    /// </summary>
    public partial class LogStreamer
        : ReceiveActorEx
    {
        /// <summary>
        ///     The name of the target Pod.
        /// </summary>
        string _podName;

        /// <summary>
        ///     The name of the target container within the Pod.
        /// </summary>
        /// <remarks>
        ///     Not required if the Pod only has a single container.
        /// </remarks>
        string _containerName;

        /// <summary>
        ///     The Kubernetes namespace in which the target Pod is located.
        /// </summary>
        string _podNamespace;

        /// <summary>
        ///     The actor to which log entries will be sent by the <see cref="LogStreamer"/>.
        /// </summary>
        IActorRef _subscriber;

        /// <summary>
        ///     The sequence of log entries being streamed.
        /// </summary>
        IObservable<string> _logStream;

        /// <summary>
        ///     An <see cref="IDisposable"/> representing the subscription to the stream of log entries.
        /// </summary>
        IDisposable _logStreamSubscription;

        /// <summary>
        ///     Create a new <see cref="LogStreamer"/> actor.
        /// </summary>
        /// <param name="kubeClient">
        ///     The Kubernetes API client.
        /// </param>
        public LogStreamer(KubeApiClient kubeClient)
        {
            if (kubeClient == null)
                throw new ArgumentNullException(nameof(kubeClient));
            
            KubeClient = kubeClient;

            Become(Initializing);
        }

        /// <summary>
        ///     The Kubernetes API client.
        /// </summary>
        KubeApiClient KubeClient { get; }

        /// <summary>
        ///     Called when the <see cref="LogStreamer"/> is initialising.
        /// </summary>
        void Initializing()
        {
            Receive<Initialize>(initialize =>
            {
                _podName = initialize.PodName;
                _containerName = initialize.ContainerName;
                _podNamespace = initialize.PodNamespace;
                _subscriber = initialize.Subscriber;
                _logStream = KubeClient.PodsV1().StreamLogs(_podName, _containerName,
                    kubeNamespace: _podNamespace
                );
                StartStreaming();

                Become(Streaming);
            });

            ReceiveAny(message =>
            {
                Sender.Tell(new Status.Failure(
                    new InvalidOperationException($"{nameof(LogStreamer)} has not been initialised.")
                ));
                Unhandled(message);
            });
        }

        /// <summary>
        ///     Called when the <see cref="LogStreamer"/> is streaming log entries.
        /// </summary>
        void Streaming()
        {
            Receive<Cancel>(_ =>
            {
                StopStreaming();

                Self.Tell(PoisonPill.Instance);
            });

            Receive<Initialize>(initialize =>
            {
                Sender.Tell(new Status.Failure(
                    new InvalidOperationException($"{nameof(LogStreamer)} has already been initialised.")
                ));
                Unhandled(initialize);
            });
        }

        /// <summary>
        ///     Called when the actor is stopped.
        /// </summary>
        protected override void PostStop() => StopStreaming();

        /// <summary>
        ///     Start streaming log entries.
        /// </summary>
        void StartStreaming()
        {
            // Capture a reference to this actor for closure purposes (the Self property is thread-local).
            IActorRef self = Self;

            _logStreamSubscription = _logStream.Subscribe(
                onNext: logLine =>
                {
                    _subscriber.Tell(
                        new LogEntry(_podName, _containerName, _podNamespace, logLine)
                    );
                },
                onError: error =>
                {
                    _subscriber.Tell(
                        new Error(_podName, _containerName, _podNamespace, error)
                    );
                },
                onCompleted: () =>
                {
                    _subscriber.Tell(EndOfLog.Instance);

                    if (_logStreamSubscription != null)
                    {
                        // Like tears in rain, time to die.
                        self.Tell(PoisonPill.Instance);
                    }
                }
            );
        }

        /// <summary>
        ///     Stop streaming log entries.
        /// </summary>
        void StopStreaming()
        {
            _logStreamSubscription?.Dispose();
            _logStreamSubscription = null;
        }
    }
}