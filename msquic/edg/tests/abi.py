"""Generate native and managed observations of the *actual* product ABI.

Both probes derive their list of observations from this file. The native side
uses the selected staged headers; the managed side references unchanged EDG
generated types. It does not translate a second copy of the ABI declarations.
"""

LAYOUTS = {
    "QUIC_BUFFER": ["Length", "Buffer"],
    "QUIC_ADDR": ["Ipv4.sin_port", "Ipv4.sin_addr", "Ipv6.sin6_addr", "Ipv6.sin6_scope_id"],
    "QUIC_SETTINGS": ["IsSetFlags", "MaxBytesPerKey", "PeerBidiStreamCount", "DestCidUpdateIdleTimeoutMs", "Flags", "StreamRecvWindowUnidiDefault"],
    "QUIC_API_TABLE": ["SetContext", "GetContext", "StreamSend", "ConnectionOpenInPartition", "ConnectionExportKeyingMaterial"],
    "QUIC_TLS_SECRETS": ["ClientRandom", "ClientHandshakeTrafficSecret", "ServerTrafficSecret0"],
    "CXPLAT_TLS_CONFIG": ["Connection", "AlpnBuffer", "AlpnBufferLength", "LocalTPBuffer", "TlsSecrets"],
    "CXPLAT_TLS_CALLBACKS": ["ReceiveTP", "CertificateReceived"],
    "CXPLAT_TLS_PROCESS_STATE": ["EarlyDataState", "ReadKey", "AlertCode", "BufferLength", "BufferTotalLength", "BufferOffsetHandshake", "BufferOffset1Rtt", "Buffer", "SmallAlpnBuffer", "NegotiatedAlpn", "ReadKeys", "WriteKeys", "ClientAlpnListLength"],
    "MSQUIC_HOST_TABLE": ["Context", "TotalMemory", "CxPlatAlloc", "quic_bugcheck"],
    "CXPLAT_LOCK": [], "CXPLAT_RW_LOCK": [], "CXPLAT_EVENT": [],
    "CXPLAT_THREAD": [], "CXPLAT_EVENTQ": [], "CXPLAT_CQE": [],
    "CXPLAT_SQE": ["Completion"], "CXPLAT_RUNDOWN_REF": [],
    "CXPLAT_THREAD_CONFIG": [],
}

# Byte observations exercise union overlays and bitfield setters, in addition to
# sizes and offsets. No pointer values or uninitialized padding are compared.
VALUES = {
    "QUIC_SETTINGS": [
        ("IsSet.MaxBytesPerKey", "1"), ("IsSet.PacingEnabled", "1"),
        ("IsSet.StreamMultiReceiveEnabled", "1"),
        ("MaxBytesPerKey", "0x0102030405060708UL"),
        ("PeerBidiStreamCount", "0x1234"), ("SendBufferingEnabled", "1"),
        ("ServerResumptionLevel", "2"), ("HyStartEnabled", "1"),
        ("StreamMultiReceiveEnabled", "1"),
    ],
    "CXPLAT_TLS_PROCESS_STATE": [
        ("HandshakeComplete", "1"), ("SessionResumed", "1"),
        ("BufferLength", "0x1234"), ("BufferTotalLength", "0x01020304"),
        ("BufferOffsetHandshake", "0x05060708"),
        ("SmallAlpnBuffer[0]", "3"), ("SmallAlpnBuffer[15]", "0x52"),
    ],
}


def sources(types=None, fields=None):
    types, fields = types or {}, fields or {}
    def member(record, field):
        first, dot, rest = field.partition('.')
        return fields.get(record, {}).get(first, first) + dot + rest
    native = ['#include "msquic_host.h"', '#include "quic_tls.h"',
              '#include <stdio.h>', '#include <stddef.h>', '#include <string.h>',
              'int main(void) {']
    managed = ['using System;', 'using System.Runtime.InteropServices;',
               # The selected LP64 host spells its scalar thread handle as
               # uintptr_t; EDG erases scalar typedefs in signatures.
               'using CXPLAT_THREAD = System.UInt64;',
               'using static Managed.Transport.MsQuic;',
               'internal static unsafe class Program {']
    for name in LAYOUTS:
        managed.append(f'[StructLayout(LayoutKind.Sequential)] private struct Align_{name} {{ public byte Prefix; public {types.get(name,name)} Value; }}')
    managed.append('private static void Main() {')
    for index, (name, members) in enumerate(LAYOUTS.items()):
        native.append(f'printf("layout {name} %zu %zu\\n", sizeof({name}), _Alignof({name}));')
        native.append(f'{name} v{index};')
        managed.extend([f'Align_{name} a{index} = default;', f'{types.get(name,name)} v{index} = default;',
            f'Console.WriteLine($"layout {name} {{sizeof({types.get(name,name)})}} {{(byte*)&a{index}.Value - (byte*)&a{index}}}");'])
        for field in members:
            native.append(f'printf("offset {name}.{field} %zu\\n", offsetof({name}, {field}));')
            managed.append(f'Console.WriteLine($"offset {name}.{field} {{(byte*)&v{index}.{member(name,field)} - (byte*)&v{index}}}");')
    for index, (name, assignments) in enumerate(VALUES.items()):
        var = f'b{index}'
        native.extend([f'{name} {var};', f'memset(&{var}, 0, sizeof({var}));'])
        managed.append(f'{types.get(name,name)} {var} = default;')
        for field, value in assignments:
            native.append(f'{var}.{field} = {value};')
            managed.append(f'{var}.{member(name,field)} = {value};')
        native.extend([f'printf("bytes {name} ");',
            f'for (size_t i = 0; i < sizeof({var}); ++i) printf("%02x", ((unsigned char*)&{var})[i]);',
            'printf("\\n");'])
        managed.append(f'Console.WriteLine("bytes {name} " + Convert.ToHexStringLower(new ReadOnlySpan<byte>(&{var}, sizeof({types.get(name,name)}))));')
    native.append('return 0; }')
    managed.append('} }')
    return '\n'.join(native) + '\n', '\n'.join(managed) + '\n'
