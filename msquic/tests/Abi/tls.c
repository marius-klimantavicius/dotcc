#include "quic_platform.h"
#include "quic_tls.h"
#include "observe.h"
static BOOLEAN abi_receive_tp(QUIC_CONNECTION *connection, uint16_t length, const uint8_t *buffer) {
    return connection != NULL && length == 3 && buffer[0] == 0x42 && buffer[2] == 0x19;
}
int main(void) {
    ABI_LAYOUT(CXPLAT_TLS_CONFIG);
    ABI_OFFSET(CXPLAT_TLS_CONFIG, Connection);
    ABI_OFFSET(CXPLAT_TLS_CONFIG, AlpnBuffer);
    ABI_OFFSET(CXPLAT_TLS_CONFIG, AlpnBufferLength);
    ABI_OFFSET(CXPLAT_TLS_CONFIG, LocalTPBuffer);
    ABI_OFFSET(CXPLAT_TLS_CONFIG, TlsSecrets);
    ABI_LAYOUT(CXPLAT_TLS_CALLBACKS);
    ABI_OFFSET(CXPLAT_TLS_CALLBACKS, ReceiveTP);
    ABI_OFFSET(CXPLAT_TLS_CALLBACKS, CertificateReceived);
    ABI_LAYOUT(CXPLAT_TLS_PROCESS_STATE);
    ABI_OFFSET(CXPLAT_TLS_PROCESS_STATE, EarlyDataState);
    ABI_OFFSET(CXPLAT_TLS_PROCESS_STATE, ReadKey);
    ABI_OFFSET(CXPLAT_TLS_PROCESS_STATE, AlertCode);
    ABI_OFFSET(CXPLAT_TLS_PROCESS_STATE, BufferLength);
    ABI_OFFSET(CXPLAT_TLS_PROCESS_STATE, BufferTotalLength);
    ABI_OFFSET(CXPLAT_TLS_PROCESS_STATE, BufferOffsetHandshake);
    ABI_OFFSET(CXPLAT_TLS_PROCESS_STATE, BufferOffset1Rtt);
    ABI_OFFSET(CXPLAT_TLS_PROCESS_STATE, Buffer);
    ABI_OFFSET(CXPLAT_TLS_PROCESS_STATE, SmallAlpnBuffer);
    ABI_OFFSET(CXPLAT_TLS_PROCESS_STATE, NegotiatedAlpn);
    ABI_OFFSET(CXPLAT_TLS_PROCESS_STATE, ReadKeys);
    ABI_OFFSET(CXPLAT_TLS_PROCESS_STATE, WriteKeys);
    ABI_OFFSET(CXPLAT_TLS_PROCESS_STATE, ClientAlpnListLength);
    CXPLAT_TLS_PROCESS_STATE state;
    memset(&state, 0, sizeof(state));
    state.HandshakeComplete = 1;
    state.SessionResumed = 1;
    state.BufferLength = 0x1234;
    state.BufferTotalLength = 0x01020304;
    state.BufferOffsetHandshake = 0x05060708;
    state.SmallAlpnBuffer[0] = 3;
    state.SmallAlpnBuffer[15] = 0x52;
    abi_bytes("CXPLAT_TLS_PROCESS_STATE", &state, sizeof(state));
    CXPLAT_TLS_CALLBACKS callbacks = {0};
    callbacks.ReceiveTP = abi_receive_tp;
    uint8_t buffer[3] = {0x42, 0x13, 0x19};
    printf("callback CXPLAT_TLS_CALLBACKS %d\n", callbacks.ReceiveTP((QUIC_CONNECTION *)&state, 3, buffer));
    return 0;
}
