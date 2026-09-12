using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Managed.Transport;
using Managed.Transport.Hosting;

// A test-only table over the real services, real core and translated TLS provider.
// Each process owns one connection. A bound UDP sink suppresses environmental
// ICMP responses while the test controls the selected failure and clock advance.
internal static unsafe class Program
{
    private static MSQUIC_HOST_TABLE services;
    private static long now = 1_000_000_000;
    private static int allocations, sends, socketCreates, closed, shutdowns, connected;
    private static uint shutdownStatus;
    internal static int SendFault, ReceiveFault, InjectedSendFaults, InjectedReceiveFaults;
    private static string scenario = "";

    private static void Require(bool condition, string reason)
    { if (!condition) throw new InvalidOperationException(reason); }
    private static void Check(uint status, string operation)
    { Require(!Status.Failed(status), operation + ": " + status); }
    private static ulong Clock(void* context) => (ulong)Interlocked.Read(ref now);
    private static uint Dequeue(void* context, CXPLAT_EVENTQ* queue, CXPLAT_CQE* result, uint count, uint timeout)
        // The real queue still owns delivery/draining. Short bounded waits cause
        // worker deadlines to be reconsidered after a virtual clock advance.
        => services.CxPlatEventQDequeue(context, queue, result, count, Math.Min(timeout, 10));
    private static CXPLAT_SEND_DATA* AllocateSend(void* context, CXPLAT_SOCKET* socket, CXPLAT_SEND_CONFIG* config)
    {
        int ordinal = Interlocked.Increment(ref allocations);
        if (scenario == "send-allocation" && ordinal == 1) return null;
        return services.CxPlatSendDataAlloc(context, socket, config);
    }
    private static void Send(void* context, CXPLAT_SOCKET* socket, CXPLAT_ROUTE* route, CXPLAT_SEND_DATA* data)
    {
        services.CxPlatSocketSend(context, socket, route, data);
        Interlocked.Increment(ref sends);
    }
    private static uint CreateSocket(void* context, CXPLAT_DATAPATH* path, CXPLAT_UDP_CONFIG* config, CXPLAT_SOCKET** output)
    {
        Interlocked.Increment(ref socketCreates);
        if (scenario == "socket-create") { *output = null; return Status.AddressNotAvailable; }
        return services.CxPlatSocketCreateUdp(context, path, config, output);
    }
    private static uint Callback(QUIC_HANDLE* connection, void* context, QUIC_CONNECTION_EVENT* notification)
    {
        switch (notification->Type)
        {
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_CONNECTED:
                Interlocked.Increment(ref connected); break;
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_TRANSPORT:
                shutdownStatus = notification->SHUTDOWN_INITIATED_BY_TRANSPORT.Status;
                Interlocked.Increment(ref shutdowns); break;
            case QUIC_CONNECTION_EVENT_TYPE.QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE:
                Interlocked.Increment(ref closed); break;
        }
        return Status.Success;
    }
    private static void Wait(Func<bool> condition, string operation)
    {
        long start = Stopwatch.GetTimestamp();
        while (!condition())
        {
            Require(Stopwatch.GetElapsedTime(start) < TimeSpan.FromSeconds(3), operation + " exceeded wall-clock bound");
            Thread.Sleep(1);
        }
    }

    private static int Main(string[] arguments)
    {
        if (arguments.Length != 3 || arguments[1] is not ("ipv4" or "ipv6") || arguments[2] is not
            ("virtual-timeout" or "send-allocation" or "send-error" or "receive-error" or "socket-create")) return 2;
        scenario = arguments[2];
        var wall = Stopwatch.StartNew();
        QUIC_API_TABLE* api = null;
        QUIC_HANDLE* registration = null, configuration = null, connection = null;
        MsQuicHost.CredentialRegistration? credential = null;
        var host = new MsQuicHost(2);
        bool installed = false, passed = false;
        ulong retransmitted = 0;
        try
        {
            services = host.CreateTable(); var table = services;
            table.CxPlatTimeUs64 = &Clock; table.CxPlatEventQDequeue = &Dequeue;
            table.CxPlatSendDataAlloc = &AllocateSend; table.CxPlatSocketSend = &Send;
            table.CxPlatSocketCreateUdp = &CreateSocket;
            Check(MsQuic.MsQuicHostInstall(&table), "host install"); installed = true;
            void* opened = null;
            Check(MsQuic.MsQuicOpenVersion(2, &opened), "open"); api = (QUIC_API_TABLE*)opened;
            fixed (byte* name = "injected-host\0"u8)
            {
                QUIC_REGISTRATION_CONFIG config = new() { AppName = name };
                Check(api->RegistrationOpen(&config, &registration), "registration");
            }
            QUIC_SETTINGS settings = default; var flags = settings.IsSet;
            flags.HandshakeIdleTimeoutMs = 1; flags.InitialRttMs = 1;
            settings.IsSet = flags; settings.HandshakeIdleTimeoutMs = 10_000; settings.InitialRttMs = 100;
            fixed (byte* protocol = "dotcc-probe"u8)
            {
                QUIC_BUFFER alpn = new() { Buffer = protocol, Length = 11 };
                Check(api->ConfigurationOpen(registration, &alpn, 1, &settings, (uint)sizeof(QUIC_SETTINGS), null, &configuration), "configuration");
            }
            using var root = X509Certificate2.CreateFromPem(File.ReadAllText(arguments[0]));
            credential = host.CreateClientCredential(new[] { root }, 0x1301);
            Check(host.LoadCredential(configuration, credential), "credential");
            var address = arguments[1] == "ipv4" ? IPAddress.Loopback : IPAddress.IPv6Loopback;
            using var sink = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            sink.Bind(new IPEndPoint(address, 0));
            QUIC_ADDR remote = default; MsQuicHost.DatagramAddress((IPEndPoint)sink.LocalEndPoint!, &remote);
            Check(api->ConnectionOpen(registration, &Callback, null, &connection), "connection");
            Check(api->SetParam(connection, MsQuic.QUIC_PARAM_CONN_REMOTE_ADDRESS, (uint)sizeof(QUIC_ADDR), &remote), "remote address");
            SendFault = scenario == "send-error" ? 1 : 0;
            ReceiveFault = scenario == "receive-error" ? 1 : 0;
            fixed (byte* name = "localhost\0"u8)
                Check(api->ConnectionStart(connection, configuration, remote.Ip.sa_family, name, (ushort)((IPEndPoint)sink.LocalEndPoint!).Port), "start admission");

            if (scenario is "socket-create" or "send-error")
            {
                Wait(() => Volatile.Read(ref closed) != 0, "injected error shutdown");
                Require(shutdownStatus == (scenario == "socket-create" ? Status.AddressNotAvailable : Status.Unreachable), "Wrong injected error status: " + shutdownStatus);
            }
            else
            {
                Wait(() => Volatile.Read(ref sends) != 0, "initial send after admission/failure");
                if (scenario == "send-allocation") Require(allocations >= 2, "Failed allocation was not retried");
                if (scenario == "receive-error") Wait(() => Volatile.Read(ref InjectedReceiveFaults) == 1, "receive error injection");
                int initialSends = Volatile.Read(ref sends);
                Thread.Sleep(50);
                Require(closed == 0 && Interlocked.Read(ref now) == 1_000_000_000, "Frozen clock expired a deadline");
                Interlocked.Add(ref now, 1_000_000);
                Wait(() => Volatile.Read(ref sends) > initialSends, "PTO retransmission after virtual advance");
                QUIC_STATISTICS_V2 stats = default; uint size = (uint)sizeof(QUIC_STATISTICS_V2);
                Check(api->GetParam(connection, MsQuic.QUIC_PARAM_CONN_STATISTICS_V2, &size, &stats), "statistics");
                retransmitted = stats.SendTotalPackets;
                Require(retransmitted >= 2, "Core did not count retransmitted Initial packets");
                Interlocked.Add(ref now, 11_000_000);
                Wait(() => Volatile.Read(ref closed) != 0, "virtual handshake idle timeout");
                Require(shutdownStatus == Status.ConnectionIdle, "Wrong virtual idle status: " + shutdownStatus);
            }
            Require(shutdowns == 1 && closed == 1 && connected == 0, "Incorrect terminal callback cardinality");
            Require(InjectedSendFaults == (scenario == "send-error" ? 1 : 0), "Send fault count");
            Require(InjectedReceiveFaults == (scenario == "receive-error" ? 1 : 0), "Receive fault count");
            Require(host.DatagramSendErrors == (scenario == "send-error" ? 1 : 0), "Unexpected send errors");
            Require(host.DatagramReceiveErrors == (scenario == "receive-error" ? 1 : 0), "Unexpected receive errors");
            passed = true;
        }
        catch (Exception error) { Console.Error.WriteLine(error); }
        finally
        {
            try
            {
                if (connection != null)
                {
                    if (closed == 0)
                    {
                        api->ConnectionShutdown(connection, QUIC_CONNECTION_SHUTDOWN_FLAGS.QUIC_CONNECTION_SHUTDOWN_FLAG_SILENT, 0);
                        Wait(() => Volatile.Read(ref closed) != 0, "cleanup shutdown");
                    }
                    api->ConnectionClose(connection);
                }
                if (configuration != null) api->ConfigurationClose(configuration);
                if (registration != null) api->RegistrationClose(registration);
                if (api != null) MsQuic.MsQuicClose(api);
                credential?.Dispose();
                Require(host.OutstandingResources == 0 && host.OutstandingPlatformAllocations == 0 && host.OutstandingDatagramReceives == 0, "Host owners did not drain");
                if (installed) Check(MsQuic.MsQuicHostUninstall(), "host uninstall");
                host.Dispose();
            }
            catch (Exception error) { Console.Error.WriteLine("cleanup: " + error); passed = false; }
        }
        Console.WriteLine("{\"passed\":" + (passed ? "true" : "false") + ",\"scenario\":\"" + scenario +
            "\",\"family\":\"" + arguments[1] + "\",\"virtual_elapsed_us\":" + (Interlocked.Read(ref now) - 1_000_000_000) +
            ",\"wall_elapsed_ms\":" + wall.ElapsedMilliseconds + ",\"socket_creates\":" + socketCreates +
            ",\"send_allocations\":" + allocations + ",\"sends\":" + sends + ",\"core_packets_before_timeout\":" + retransmitted +
            ",\"shutdown_status\":" + shutdownStatus + ",\"shutdown_callbacks\":" + shutdowns + ",\"closed_callbacks\":" + closed +
            ",\"injected_send_errors\":" + InjectedSendFaults + ",\"injected_receive_errors\":" + InjectedReceiveFaults +
            ",\"host_resources\":" + host.OutstandingResources + ",\"host_allocations\":" + host.OutstandingPlatformAllocations +
            ",\"host_receive_leases\":" + host.OutstandingDatagramReceives + ",\"aot\":" + (!RuntimeFeature.IsDynamicCodeSupported ? "true" : "false") + "}");
        return passed ? 0 : 1;
    }
}
