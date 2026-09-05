#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using SDL2;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uoAvox.Assets;
using uoAvox.Configuration;
using uoAvox.Game;
using uoAvox.Game.Data;
using uoAvox.IO;
using uoAvox.Network;
using uoAvox.Network.Encryption;
using uoAvox.Resources;
using uoAvox.Utility;
using uoAvox.Utility.Logging;
using uoAvox.Utility.Platforms;

namespace uoAvox
{
    internal static class Client
    {
        public static ClientVersion Version { get; private set; }
        public static ClientFlags Protocol { get; set; }
        public static string ClientPath { get; private set; }
        public static GameController Game { get; private set; }

        public static Process LocalServer { get; private set; }

        private static bool _LocalServerContext, _LocalServerAttached;

        public static void Run()
        {
            Debug.Assert(Game == null);

            Load();

            _LocalServerContext = false;

            if (Dns.GetHostAddresses(Settings.GlobalSettings.IP) is IPAddress[] addresses)
            {
                foreach (IPAddress addr in addresses)
                {
                    if (IPAddress.IsLoopback(addr))
                    {
                        _LocalServerContext = true;
                        break;
                    }
                }
            }

            Log.Trace("Running game...");

            using (Game = new GameController())
            {
                // https://github.com/FNA-XNA/FNA/wiki/7:-FNA-Environment-Variables#fna_graphics_enable_highdpi
                UOAEnvironment.IsHighDPI = Environment.GetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI") == "1";

                if (UOAEnvironment.IsHighDPI)
                {
                    Log.Trace("HIGH DPI - ENABLED");
                }

                Log.Trace("Loading plugins...");

                foreach (string p in Settings.GlobalSettings.Plugins)
                {
                    Plugin.Create(p);
                }

                Log.Trace("Done!");

                UoAssist.Start();

                Game.Run();
            }

            //StopLocalServer();

            Log.Trace("Exiting game...");
        }

        public static void ShowErrorMessage(string msg)
        {
            SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "ERROR", msg, IntPtr.Zero);
        }

        private static Process FindExistingLocalServer(string serverExecutable)
        {
            try
            {
                string serverFileName = Path.GetFileNameWithoutExtension(serverExecutable);

                Process[] processes = Process.GetProcessesByName(serverFileName);

                foreach (Process process in processes)
                {
                    try
                    {
                        if (string.Equals(process.MainModule?.FileName, serverExecutable, StringComparison.OrdinalIgnoreCase))
                        {
                            return process;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"Failed to search for existing server process: {ex.Message}");
            }

            return null;
        }

        public static void StartLocalServer()
        {
            if (!_LocalServerContext || _LocalServerAttached)
            {
                return;
            }

            string serverDirectory;

            if (string.IsNullOrWhiteSpace(Settings.GlobalSettings.ServerDirectory))
            {
                serverDirectory = Path.Combine(UOAEnvironment.ExecutablePath, "server");
            }
            else
            {
                serverDirectory = Settings.GlobalSettings.ServerDirectory;
            }

            if (!Directory.Exists(serverDirectory))
            {
                Log.Warn($"Local server directory not found: {serverDirectory}");

                return;
            }

            string serverExecutable = Path.Combine(serverDirectory, "Server.exe");

            if (!File.Exists(serverExecutable))
            {
                Log.Warn($"Local server executable not found: {serverExecutable}");

                return;
            }

            Process existingServer = FindExistingLocalServer(serverExecutable);

            if (existingServer != null)
            {
                Log.Trace($"Local server process found: #{existingServer.Id}");

                LocalServer = existingServer;

                _LocalServerAttached = false;

                if (!WaitForLocalServer(10, 3000))
                {
                    Log.Warn($"Existing local server process #{existingServer.Id} is not responding to connection attempts");

                    LocalServer.Dispose();
                    LocalServer = null;
                }

                return;
            }

            try
            {
                Log.Trace($"Local server starting: {serverExecutable}");
                Log.Trace($"Expected connection: {Settings.GlobalSettings.IP}:{Settings.GlobalSettings.Port}");

                int clientPid = Environment.ProcessId;
                string args = $"-attached -service -usehrt -parentpid {clientPid}";

#if DEBUG
                args += " -debug";
#else
                if (UOAEnvironment.Debug)
                {
                    args += " -debug";
                }
#endif
                LocalServer = new Process()
                {
                    StartInfo =
                    {
                        FileName = serverExecutable,
                        WorkingDirectory = serverDirectory,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardInputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
                        StandardOutputEncoding = Encoding.UTF8,
                        Arguments = args,
                        ErrorDialog = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    },
                    EnableRaisingEvents = true
                };

                LocalServer.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Log.Info($"[Server] {e.Data}");
                    }
                };

                LocalServer.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Log.Error($"[Server] {e.Data}");
                    }
                };

                if (!LocalServer.Start())
                {
                    Log.Error("Local server start failed");

                    return;
                }

                _LocalServerAttached = true;

                Log.Trace($"Local server started: #{LocalServer.Id}");

                try
                {
                    LocalServer.PriorityClass = ProcessPriorityClass.High;
                }
                catch
                {
                }

                LocalServer.StandardInput.AutoFlush = true;

                LocalServer.BeginOutputReadLine();
                LocalServer.BeginErrorReadLine();

                if (!WaitForLocalServer(60, 1000))
                {
                    Log.Error("Local server failed to respond to connection attempts, terminating...");

                    LocalServer.Kill();
                    LocalServer = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Local server start failed: {ex.Message}");
            }
        }

        public static void StopLocalServer()
        {
            try
            {
                if (!_LocalServerAttached || !SendCommandToLocalServer("shutdown"))
                {
                    return;
                }

                Log.Info("Local server exiting...");

                if (LocalServer.WaitForExit(-1))
                {
                    Log.Info($"Local server exited with code: {LocalServer.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Local server exit failed: {ex.Message}");
            }
            finally
            {
                _LocalServerAttached = false;
            }
        }

        public static bool SendCommandToLocalServer(string command)
        {
            if (!_LocalServerAttached || LocalServer == null)
            {
                return false;
            }

            if (LocalServer.HasExited)
            {
                Log.Error($"Local server exited with code: {LocalServer.ExitCode}");

                return false;
            }

            try
            {
                LocalServer.StandardInput.WriteLine(command);

                Log.Info($"Local server command sent: {command}");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Local server command failed: {ex.Message}");

                return false;
            }
        }

        private static bool WaitForLocalServer(int attempts, int timeout)
        {
            if (LocalServer == null || attempts <= 0)
            {
                return false;
            }

            if (LocalServer.HasExited)
            {
                Log.Error($"Local server exited with code: {LocalServer.ExitCode}");

                return false;
            }

            string host = Settings.GlobalSettings.IP;
            ushort port = Settings.GlobalSettings.Port;

            Log.Trace($"Waiting for local server PID={LocalServer.Id} on {host}:{port} ({attempts} attempts, {timeout}ms timeout per attempt)");

            Stopwatch stopwatch = Stopwatch.StartNew();
            int attemptCount = 0;
            Exception lastException = null;
            string lastErrorType = null;

            try
            {
                while (attemptCount < attempts)
                {
                    attemptCount++;

                    if (LocalServer.HasExited)
                    {
                        Log.Error($"Local server exited with code: {LocalServer.ExitCode} after {attemptCount} attempts");

                        return false;
                    }

                    if (ConnectToLocalServer(host, port, timeout, out lastException))
                    {
                        Log.Trace($"Local server ready ({stopwatch.Elapsed.TotalSeconds:F2}s)");

                        return true;
                    }

                    if (lastException != null)
                    {
                        if (lastException is SocketException socketEx)
                        {
                            lastErrorType = $"SocketException({socketEx.SocketErrorCode})";
                        }
                        else if (lastException is TimeoutException)
                        {
                            lastErrorType = "Timeout";
                        }
                        else
                        {
                            lastErrorType = lastException.GetType().Name;
                        }
                    }

                    if (attemptCount < attempts)
                    {
                        Thread.Sleep(1000);
                    }
                }

                Log.Warn($"Local server connection failed after {attemptCount} attempts ({stopwatch.Elapsed.TotalSeconds:F2}s)");
                Log.Warn($"Last error type: {lastErrorType ?? "unknown"}, Message: {lastException?.Message ?? "no error captured"}");

                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Local server connection failed after {attemptCount} attempts: {ex.Message}");

                return false;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        private static bool ConnectToLocalServer(string host, ushort port, int timeout, out Exception lastException)
        {
            lastException = null;

            TcpClient client = null;

            try
            {
                IPAddress ipAddress;

                if (IPAddress.TryParse(host, out ipAddress))
                {
                    client = new TcpClient(ipAddress.AddressFamily);
                }
                else
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(host);

                    IPAddress ipv4Address = Array.Find(hostEntry.AddressList, addr => addr.AddressFamily == AddressFamily.InterNetwork);

                    if (ipv4Address != null)
                    {
                        ipAddress = ipv4Address;
                        client = new TcpClient(AddressFamily.InterNetwork);
                    }
                    else
                    {
                        ipAddress = hostEntry.AddressList[0];
                        client = new TcpClient(ipAddress.AddressFamily);
                    }
                }

                using CancellationTokenSource cts = new(timeout);

                ValueTask connectTask = client.ConnectAsync(ipAddress, port, cts.Token);

                if (connectTask.AsTask().Wait(timeout))
                {
                    bool connected = client.Connected;

                    if (!connected)
                    {
                        lastException = new InvalidOperationException("Connection completed but client not connected");
                    }

                    return connected;
                }

                lastException = new TimeoutException($"Connection attempt timed out after {timeout}ms");

                return false;
            }
            catch (SocketException socketEx)
            {
                lastException = socketEx;

                return false;
            }
            catch (AggregateException aggEx) when (aggEx.InnerException is SocketException innerSocketEx)
            {
                lastException = innerSocketEx;

                return false;
            }
            catch (Exception ex)
            {
                lastException = ex;

                return false;
            }
            finally
            {
                client?.Close();
                client?.Dispose();
            }
        }

        private static void Load()
        {
            string clientPath = Settings.GlobalSettings.GameDirectory;
            Log.Trace($"Ultima Online game folder: {clientPath}");

            string patchPath = Settings.GlobalSettings.PatchDirectory;
            Log.Trace($"Ultima Online patch folder: {patchPath}");

            Log.Trace("Loading files...");

            // check if directory is good
            if (!Directory.Exists(clientPath))
            {
                Log.Error("Invalid client directory: " + clientPath);
                ShowErrorMessage(string.Format(ResErrorMessages.ClientPathIsNotAValidUODirectory, clientPath));

                throw new InvalidClientDirectory($"'{clientPath}' is not a valid directory");
            }

            if (!string.IsNullOrWhiteSpace(Settings.GlobalSettings.ClientVersion))
            {
                // sanitize client version
                Settings.GlobalSettings.ClientVersion = Settings.GlobalSettings.ClientVersion.Replace(",", ".").Replace(" ", "").ToLower();
            }

            string clientVersionText = Settings.GlobalSettings.ClientVersion;

            // try to load the client version
            if (!ClientVersionHelper.IsClientVersionValid(clientVersionText, out ClientVersion clientVersion))
            {
                Log.Warn($"Client version [{clientVersionText}] is invalid, let's try to read the client.exe");

                // mmm something bad happened, try to load from client.exe
                if (!ClientVersionHelper.TryParseFromFile(Path.Combine(clientPath, "client.exe"), out clientVersionText) || !ClientVersionHelper.IsClientVersionValid(clientVersionText, out clientVersion))
                {
                    Log.Error("Invalid client version: " + clientVersionText);
                    ShowErrorMessage(string.Format(ResGumps.ImpossibleToDefineTheClientVersion0, clientVersionText));

                    throw new InvalidClientVersion($"Invalid client version: '{clientVersionText}'");
                }

                Log.Trace($"Found a valid client.exe [{clientVersionText} - {clientVersion}]");

                // update the wrong/missing client version in settings.json
                Settings.GlobalSettings.ClientVersion = clientVersionText;
            }

            Version = clientVersion;
            ClientPath = clientPath;

            Protocol = ClientFlags.CF_T2A;

            if (Version >= ClientVersion.CV_200)
            {
                Protocol |= ClientFlags.CF_RE;
            }

            if (Version >= ClientVersion.CV_300)
            {
                Protocol |= ClientFlags.CF_TD;
            }

            if (Version >= ClientVersion.CV_308)
            {
                Protocol |= ClientFlags.CF_LBR;
            }

            if (Version >= ClientVersion.CV_308Z)
            {
                Protocol |= ClientFlags.CF_AOS;
            }

            if (Version >= ClientVersion.CV_405A)
            {
                Protocol |= ClientFlags.CF_SE;
            }

            if (Version >= ClientVersion.CV_60144)
            {
                Protocol |= ClientFlags.CF_SA;
            }

            Log.Trace($"Client path: '{clientPath}'");
            Log.Trace($"Client version: {clientVersion}");
            Log.Trace($"Protocol: {Protocol}");

            UOFilesOverrideMap.OverrideFile = patchPath;

            // ok now load uo files
            UOFileManager.Load(Version, clientPath, Settings.GlobalSettings.UseVerdata, Settings.GlobalSettings.Language);
            StaticFilters.Load();

            BuffTable.Load();
            ChairTable.Load();

            Log.Trace("Network calibration...");
            //ATTENTION: you will need to enable ALSO ultimalive server-side, or this code will have absolutely no effect!
            UltimaLive.Enable();
            PacketsTable.AdjustPacketSizeByVersion(Version);

            if (Settings.GlobalSettings.Encryption != 0)
            {
                Log.Trace("Calculating encryption by client version...");
                EncryptionHelper.CalculateEncryption(Version);
                Log.Trace($"encryption: {EncryptionHelper.Type}");

                if (EncryptionHelper.Type != (ENCRYPTION_TYPE)Settings.GlobalSettings.Encryption)
                {
                    Log.Warn($"Encryption found: {EncryptionHelper.Type}");
                    Settings.GlobalSettings.Encryption = (byte)EncryptionHelper.Type;
                }
            }
        }
    }
}