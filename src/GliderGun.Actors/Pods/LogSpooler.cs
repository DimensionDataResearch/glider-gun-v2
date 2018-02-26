using Akka;
using Akka.Actor;
using KubeClient;
using System;

namespace GliderGun.Actors.Pods
{
    /// <summary>
    ///     Actor that publishes log entries from a container in a Kubernetes Pod.
    /// </summary>
    public class LogSpooler
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
        ///     The actor to which log entries will be sent by the <see cref="LogSpooler"/>.
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
        ///     Create a new <see cref="LogSpooler"/> actor.
        /// </summary>
        /// <param name="kubeClient">
        ///     The Kubernetes API client.
        /// </param>
        public LogSpooler(KubeApiClient kubeClient)
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
        ///     Called when the <see cref="LogSpooler"/> is initialising.
        /// </summary>
        void Initializing()
        {
            Receive<Initialize>(initialize =>
            {
                IActorRef self = Self;

                _podName = initialize.PodName;
                _containerName = initialize.ContainerName;
                _podNamespace = initialize.PodNamespace;
                _subscriber = initialize.Subscriber;
                _logStream = KubeClient.PodsV1().StreamLogs(_podName, _containerName,
                    kubeNamespace: _podNamespace
                );
                _logStreamSubscription = _logStream.Subscribe(
                    onNext: logLine =>
                    {
                        _subscriber.Tell(new LogEntry(
                            _podName,
                            _containerName,
                            _podNamespace,
                            logLine
                        ));
                    },
                    onError: error =>
                    {
                        // TODO: Publish error to subscriber (?)
                    },
                    onCompleted: () =>
                    {
                        if (_logStreamSubscription != null)
                        {
                            // Like tears in rain, time to die.
                            self.Tell(PoisonPill.Instance);
                        }
                    }
                );

                Become(Streaming);
            });

            ReceiveAny(_ =>
            {
                Sender.Tell(new Status.Failure(
                    new InvalidOperationException("LogSpooler has not been initialised.")
                ));
            });
        }

        /// <summary>
        ///     Called when the <see cref="LogSpooler"/> is streaming log entries.
        /// </summary>
        void Streaming()
        {
            // TODO: Handle Cancel message.

            Receive<Initialize>(_ =>
            {
                Sender.Tell(new Status.Failure(
                    new InvalidOperationException("LogSpooler has already been initialised.")
                ));
            });
        }

        /// <summary>
        ///     Called when the actor is stopped.
        /// </summary>
        protected override void PostStop()
        {
            _logStreamSubscription?.Dispose();
            _logStreamSubscription = null;
        }

        /// <summary>
        ///     Request initialisation of a <see cref="LogSpooler"/> actor.
        /// </summary>
        public class Initialize
        {
            /// <summary>
            ///     The name of the target Pod.
            /// </summary>
            public string PodName { get; }

            /// <summary>
            ///     The name of the target container within the Pod.
            /// </summary>
            /// <remarks>
            ///     Not required if the Pod only has a single container.
            /// </remarks>
            public string ContainerName { get; }

            /// <summary>
            ///     The Kubernetes namespace in which the target Pod is located.
            /// </summary>
            public string PodNamespace { get; }

            /// <summary>
            ///     The actor to which log entries will be sent by the <see cref="LogSpooler"/>.
            /// </summary>
            public IActorRef Subscriber { get; }

            /// <summary>
            ///     Create a new <see cref="Initialize"/> message.
            /// </summary>
            /// <param name="podName">
            ///     The name of the target Pod.
            /// </param>
            /// <param name="podNamespace">
            ///     The Kubernetes namespace in which the target Pod is located.
            /// </param>
            /// <param name="subscriber">
            ///     The actor to which log entries will be sent by the <see cref="LogSpooler"/>.
            /// </param>
            public Initialize(string podName, string podNamespace, IActorRef subscriber)
                : this(podName, null, podNamespace, subscriber)
            {
            }

            /// <summary>
            ///     Create a new <see cref="Initialize"/> message.
            /// </summary>
            /// <param name="podName">
            ///     The name of the target Pod.
            /// </param>
            /// <param name="containerName">
            ///     The name of the target container within the Pod.
            /// 
            ///     Optional, if the Pod only has a single container.
            /// </param>
            /// <param name="podNamespace">
            ///     The Kubernetes namespace in which the target Pod is located.
            /// </param>
            /// <param name="subscriber">
            ///     The actor to which log entries will be sent by the <see cref="LogSpooler"/>.
            /// </param>
            public Initialize(string podName, string containerName, string podNamespace, IActorRef subscriber)
            {
                if (String.IsNullOrWhiteSpace(podName))
                    throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'podName'.", nameof(podName));
                
                if (String.IsNullOrWhiteSpace(podNamespace))
                    throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'podNamespace'.", nameof(podNamespace));
                
                if (subscriber == null)
                    throw new ArgumentNullException(nameof(subscriber));
                
                PodName = podName;
                ContainerName = containerName;
                PodNamespace = podNamespace;
                Subscriber = subscriber;
            }
        }

        /// <summary>
        ///     Represents a single line streamed from a Pod container log.
        /// </summary>
        public class LogEntry
        {
            /// <summary>
            ///     Create a new <see cref="LogEntry"/> message.
            /// </summary>
            /// <param name="podName">
            ///     The name of the target Pod.
            /// </param>
            /// <param name="containerName">
            ///     The name of the target container within the Pod.
            /// </param>
            /// <param name="podNamespace">
            ///     The Kubernetes namespace in which the target Pod is located.
            /// </param>
            /// <param name="line">
            ///     The line from the log.
            /// </param>
            public LogEntry(string podName, string containerName, string podNamespace, string line)
            {
                if (String.IsNullOrWhiteSpace(podName))
                    throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'podName'.", nameof(podName));
                
                if (String.IsNullOrWhiteSpace(podNamespace))
                    throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'podNamespace'.", nameof(podNamespace));
                
                if (line == null)
                    throw new ArgumentNullException(nameof(line));
                
                PodName = podName;
                ContainerName = containerName;
                PodNamespace = podNamespace;
                Line = line;
            }

            /// <summary>
            ///     The name of the target Pod.
            /// </summary>
            public string PodName { get; }

            /// <summary>
            ///     The name of the target container within the Pod.
            /// </summary>
            public string ContainerName { get; }

            /// <summary>
            ///     The Kubernetes namespace in which the target Pod is located.
            /// </summary>
            public string PodNamespace { get; }

            /// <summary>
            ///     The line from the log.
            /// </summary>
            public string Line { get; }
        }
    }
}