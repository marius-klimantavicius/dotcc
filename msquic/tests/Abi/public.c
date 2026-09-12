/* Include the unchanged pinned upstream public header, with no replacement declarations. */
#include "msquic.h"
#include "observe.h"
static void QUIC_API abi_set_context(HQUIC handle, void *context) { *(void **)handle = context; }
static void *QUIC_API abi_get_context(HQUIC handle) { return *(void **)handle; }
int main(void) {
    ABI_LAYOUT(QUIC_BUFFER);
    ABI_OFFSET(QUIC_BUFFER, Length);
    ABI_OFFSET(QUIC_BUFFER, Buffer);
    ABI_LAYOUT(QUIC_ADDR);
    ABI_OFFSET(QUIC_ADDR, Ipv4.sin_port);
    ABI_OFFSET(QUIC_ADDR, Ipv4.sin_addr);
    ABI_OFFSET(QUIC_ADDR, Ipv6.sin6_addr);
    ABI_OFFSET(QUIC_ADDR, Ipv6.sin6_scope_id);
    ABI_LAYOUT(QUIC_SETTINGS);
    ABI_OFFSET(QUIC_SETTINGS, IsSetFlags);
    ABI_OFFSET(QUIC_SETTINGS, MaxBytesPerKey);
    ABI_OFFSET(QUIC_SETTINGS, PeerBidiStreamCount);
    ABI_OFFSET(QUIC_SETTINGS, DestCidUpdateIdleTimeoutMs);
    ABI_OFFSET(QUIC_SETTINGS, Flags);
    ABI_OFFSET(QUIC_SETTINGS, StreamRecvWindowUnidiDefault);
    ABI_LAYOUT(QUIC_API_TABLE);
    ABI_OFFSET(QUIC_API_TABLE, SetContext);
    ABI_OFFSET(QUIC_API_TABLE, GetContext);
    ABI_OFFSET(QUIC_API_TABLE, StreamSend);
    ABI_OFFSET(QUIC_API_TABLE, ConnectionOpenInPartition);
    ABI_OFFSET(QUIC_API_TABLE, ConnectionExportKeyingMaterial);
    ABI_LAYOUT(QUIC_TLS_SECRETS);
    ABI_OFFSET(QUIC_TLS_SECRETS, ClientRandom);
    ABI_OFFSET(QUIC_TLS_SECRETS, ClientHandshakeTrafficSecret);
    ABI_OFFSET(QUIC_TLS_SECRETS, ServerTrafficSecret0);

    QUIC_ADDR address;
    memset(&address, 0, sizeof(address));
    address.Ipv6.sin6_family = QUIC_ADDRESS_FAMILY_INET6;
    address.Ipv6.sin6_port = 0x1234;
    address.Ipv6.sin6_scope_id = 0x10203040;
    address.Ipv6.sin6_addr.s6_addr[15] = 1;
    abi_bytes("QUIC_ADDR", &address, sizeof(address));

    QUIC_SETTINGS settings;
    memset(&settings, 0, sizeof(settings));
    settings.IsSet.MaxBytesPerKey = 1;
    settings.IsSet.PacingEnabled = 1;
    settings.IsSet.StreamMultiReceiveEnabled = 1;
    settings.MaxBytesPerKey = 0x0102030405060708ULL;
    settings.PeerBidiStreamCount = 0x1234;
    settings.SendBufferingEnabled = 1;
    settings.ServerResumptionLevel = 2;
    settings.HyStartEnabled = 1;
    settings.StreamMultiReceiveEnabled = 1;
    abi_bytes("QUIC_SETTINGS", &settings, sizeof(settings));

    QUIC_TLS_SECRETS secrets;
    memset(&secrets, 0, sizeof(secrets));
    secrets.SecretLength = 32;
    secrets.IsSet.ClientRandom = 1;
    secrets.IsSet.ServerHandshakeTrafficSecret = 1;
    secrets.IsSet.ServerTrafficSecret0 = 1;
    secrets.ClientRandom[31] = 0x91;
    secrets.ClientHandshakeTrafficSecret[0] = 0x52;
    secrets.ServerTrafficSecret0[63] = 0xa3;
    abi_bytes("QUIC_TLS_SECRETS", &secrets, sizeof(secrets));

    QUIC_API_TABLE api = {0};
    void *context = NULL;
    int marker = 73;
    api.SetContext = abi_set_context;
    api.GetContext = abi_get_context;
    api.SetContext((HQUIC)&context, &marker);
    printf("callback QUIC_API_TABLE %d %d\n", api.GetContext((HQUIC)&context) == &marker,
        *(int *)api.GetContext((HQUIC)&context));
    return 0;
}
