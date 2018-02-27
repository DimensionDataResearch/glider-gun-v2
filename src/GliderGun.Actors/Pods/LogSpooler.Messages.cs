using Akka;
using Akka.Actor;
using KubeClient;
using System;

namespace GliderGun.Actors.Pods
{
    /// <summary>
    ///     Actor that publishes log entries from a container in a Kubernetes Pod.
    /// </summary>
    public partial class LogSpooler
    {
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
        ///     
        /// </summary>
        public class Cancel
        {

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

        /// <summary>
        ///     Message indicating that an error was encountered while streaming log entries.
        /// </summary>
        public class Error
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
            /// <param name="cause">
            ///     An <see cref="Exception"/> representing the cause of the error.
            /// </param>
            public Error(string podName, string containerName, string podNamespace, Exception cause)
            {
                if (String.IsNullOrWhiteSpace(podName))
                    throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'podName'.", nameof(podName));
                
                if (String.IsNullOrWhiteSpace(podNamespace))
                    throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'podNamespace'.", nameof(podNamespace));
                
                if (cause == null)
                    throw new ArgumentNullException(nameof(cause));
                
                PodName = podName;
                ContainerName = containerName;
                PodNamespace = podNamespace;
                Cause = cause;
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
            ///     An <see cref="Exception"/> representing the cause of the error.
            /// </summary>
            public Exception Cause { get; }
        }

        /// <summary>
        ///     Message indicating that the end of the log has been reached.
        /// </summary>
        public class EndOfLog
        {
            /// <summary>
            ///     The singleton instance of the <see cref="EndOfLog"/> message.
            /// </summary>
            public static readonly EndOfLog Instance = new EndOfLog();
            
            /// <summary>
            ///     Create a new <see cref="EndOfLog"/> message.
            /// </summary>
            EndOfLog()
            {
            }
        }
    }
}