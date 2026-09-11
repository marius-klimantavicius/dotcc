/* Campaign-owned boundary helpers; the pinned TLS implementation is unchanged. */
#include "picotls.h"

void dotcc_ptls_buffer_dispose(ptls_buffer_t *buffer)
{
    ptls_buffer_dispose(buffer);
}

uint8_t *dotcc_ptls_encode_quicint(uint8_t *output, uint64_t value)
{
    return ptls_encode_quicint(output, value);
}

/* Forward the upstream length-block macros through the managed-callable ABI. */
int dotcc_ptls_buffer_push_block(ptls_buffer_t *buffer, size_t length_bytes, ptls_iovec_t data)
{
    int ret = 0;
    if (buffer == NULL || (data.base == NULL && data.len != 0) ||
        (length_bytes != 1 && length_bytes != 2 && length_bytes != 3 && length_bytes != SIZE_MAX))
        return PTLS_ALERT_ILLEGAL_PARAMETER;
    ptls_buffer_push_block(buffer, length_bytes, { ptls_buffer_pushv(buffer, data.base, data.len); });
Exit:
    return ret;
}

int dotcc_ptls_decode_block(ptls_iovec_t encoded, size_t length_bytes, ptls_iovec_t *decoded)
{
    int ret = 0;
    ptls_iovec_t result = {NULL, 0};
    if (decoded == NULL)
        return PTLS_ALERT_ILLEGAL_PARAMETER;
    *decoded = result;
    if (encoded.base == NULL || (length_bytes != 1 && length_bytes != 2 && length_bytes != 3 && length_bytes != SIZE_MAX))
        return PTLS_ALERT_ILLEGAL_PARAMETER;
    const uint8_t *src = encoded.base;
    const uint8_t *end = src + encoded.len;
    ptls_decode_block(src, end, length_bytes, {
        result = ptls_iovec_init(src, end - src);
        src = end;
    });
    if (src != end) {
        ret = PTLS_ALERT_DECODE_ERROR;
        goto Exit;
    }
    *decoded = result;
Exit:
    return ret;
}

void dotcc_ptls_client_properties(ptls_handshake_properties_t *properties, ptls_iovec_t *protocols,
                                  size_t protocol_count, ptls_iovec_t ticket, int negotiate_first)
{
    properties->client.negotiated_protocols.list = protocols;
    properties->client.negotiated_protocols.count = protocol_count;
    properties->client.session_ticket = ticket;
    properties->client.negotiate_before_key_exchange = negotiate_first != 0;
}

void dotcc_ptls_server_properties(ptls_handshake_properties_t *properties, int enforce_retry)
{
    properties->server.enforce_retry = enforce_retry != 0;
}
