// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Automation
{
    /// <summary>
    /// Embedded HTTP server for external test automation.
    /// Listens for commands on a configurable port and dispatches them
    /// to the Unity main thread for execution.
    /// </summary>
    public class AutomationServer : MonoBehaviour
    {
        /// <summary>
        /// A queued command awaiting execution on the main thread.
        /// </summary>
        private class QueuedCommand
        {
            /// <summary>
            /// The HTTP method (GET, POST, etc.).
            /// </summary>
            public string method;

            /// <summary>
            /// The request path (e.g., "/api/v1/status").
            /// </summary>
            public string path;

            /// <summary>
            /// The request body content, if any.
            /// </summary>
            public string body;

            /// <summary>
            /// Signal to notify the listener thread that execution is complete.
            /// </summary>
            public ManualResetEventSlim completionSignal;

            /// <summary>
            /// The response body to send back to the client.
            /// </summary>
            public string responseBody;

            /// <summary>
            /// The response content type.
            /// </summary>
            public string responseContentType;

            /// <summary>
            /// The response byte data (for binary responses like screenshots).
            /// </summary>
            public byte[] responseBytes;

            /// <summary>
            /// The HTTP status code for the response.
            /// </summary>
            public int responseStatusCode;
        }

        /// <summary>
        /// The port the server is listening on.
        /// </summary>
        private int port;

        /// <summary>
        /// The underlying HTTP listener.
        /// </summary>
        private HttpListener httpListener;

        /// <summary>
        /// The listener thread.
        /// </summary>
        private Thread listenerThread;

        /// <summary>
        /// Whether the server is running.
        /// </summary>
        private volatile bool isRunning;

        /// <summary>
        /// Queue of commands awaiting main-thread execution.
        /// </summary>
        private ConcurrentQueue<QueuedCommand> commandQueue;

        /// <summary>
        /// Reference to the command dispatcher.
        /// </summary>
        private CommandDispatcher dispatcher;

        /// <summary>
        /// Initialize the automation server.
        /// </summary>
        /// <param name="port">Port to listen on.</param>
        public void Initialize(int port)
        {
            this.port = port;
            commandQueue = new ConcurrentQueue<QueuedCommand>();
            dispatcher = new CommandDispatcher();

            isRunning = true;

            listenerThread = new Thread(ListenerLoop);
            listenerThread.IsBackground = true;
            listenerThread.Start();

            Logging.Log("[AutomationServer->Initialize] Automation server started on port " + port + ".");
        }

        /// <summary>
        /// Terminate the automation server.
        /// </summary>
        public void Terminate()
        {
            isRunning = false;

            if (httpListener != null)
            {
                try
                {
                    httpListener.Stop();
                    httpListener.Close();
                }
                catch (Exception e)
                {
                    Logging.LogWarning("[AutomationServer->Terminate] Error stopping listener: " + e.Message);
                }
            }

            if (listenerThread != null && listenerThread.IsAlive)
            {
                listenerThread.Join(2000);
            }

            Logging.Log("[AutomationServer->Terminate] Automation server stopped.");
        }

        /// <summary>
        /// Background thread loop that accepts HTTP connections.
        /// </summary>
        private void ListenerLoop()
        {
            try
            {
                httpListener = new HttpListener();
                httpListener.Prefixes.Add("http://localhost:" + port + "/");
                httpListener.Start();

                while (isRunning)
                {
                    HttpListenerContext context;
                    try
                    {
                        context = httpListener.GetContext();
                    }
                    catch (HttpListenerException)
                    {
                        // Listener was stopped.
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }

                    HandleRequest(context);
                }
            }
            catch (Exception e)
            {
                Logging.LogError("[AutomationServer->ListenerLoop] Fatal error: " + e.Message);
            }
        }

        /// <summary>
        /// Handle an incoming HTTP request by queuing it for main-thread execution.
        /// </summary>
        /// <param name="context">The HTTP listener context.</param>
        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                // Read request body.
                string body = "";
                if (context.Request.HasEntityBody)
                {
                    using (StreamReader reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        body = reader.ReadToEnd();
                    }
                }

                // Create queued command.
                QueuedCommand command = new QueuedCommand
                {
                    method = context.Request.HttpMethod,
                    path = context.Request.Url.AbsolutePath,
                    body = body,
                    completionSignal = new ManualResetEventSlim(false),
                    responseBody = "",
                    responseContentType = "application/json",
                    responseBytes = null,
                    responseStatusCode = 200
                };

                // Enqueue for main thread processing.
                commandQueue.Enqueue(command);

                // Wait for main thread to process (timeout after 30 seconds).
                if (!command.completionSignal.Wait(30000))
                {
                    command.responseStatusCode = 504;
                    command.responseBody = "{\"error\": \"Command timed out.\"}";
                }

                // Send response.
                context.Response.StatusCode = command.responseStatusCode;
                context.Response.ContentType = command.responseContentType;

                // Add CORS headers.
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                if (command.responseBytes != null)
                {
                    context.Response.ContentLength64 = command.responseBytes.Length;
                    context.Response.OutputStream.Write(command.responseBytes, 0, command.responseBytes.Length);
                }
                else
                {
                    byte[] responseBuffer = Encoding.UTF8.GetBytes(command.responseBody);
                    context.Response.ContentLength64 = responseBuffer.Length;
                    context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
                }

                context.Response.OutputStream.Close();
                command.completionSignal.Dispose();
            }
            catch (Exception e)
            {
                Logging.LogError("[AutomationServer->HandleRequest] Error: " + e.Message);
                try
                {
                    context.Response.StatusCode = 500;
                    byte[] errorBuffer = Encoding.UTF8.GetBytes("{\"error\": \"Internal server error.\"}");
                    context.Response.ContentLength64 = errorBuffer.Length;
                    context.Response.OutputStream.Write(errorBuffer, 0, errorBuffer.Length);
                    context.Response.OutputStream.Close();
                }
                catch (Exception)
                {
                    // Response may already be sent.
                }
            }
        }

        /// <summary>
        /// Unity Update loop — processes queued commands on the main thread.
        /// </summary>
        private void Update()
        {
            while (commandQueue != null && commandQueue.TryDequeue(out QueuedCommand command))
            {
                try
                {
                    dispatcher.Dispatch(command.method, command.path, command.body,
                        out command.responseBody, out command.responseContentType,
                        out command.responseBytes, out command.responseStatusCode);
                }
                catch (Exception e)
                {
                    command.responseStatusCode = 500;
                    command.responseBody = "{\"error\": \"" + EscapeJson(e.Message) + "\"}";
                    command.responseContentType = "application/json";
                    command.responseBytes = null;
                    Logging.LogError("[AutomationServer->Update] Error dispatching command: " + e.Message);
                }
                finally
                {
                    command.completionSignal.Set();
                }
            }
        }

        private void OnDestroy()
        {
            Terminate();
        }

        /// <summary>
        /// Escape a string for safe inclusion in a JSON value.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The escaped string.</returns>
        public static string EscapeJson(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    }
}
