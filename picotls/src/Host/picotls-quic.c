/* Authored QUIC boundary, compiled in the same unit as the unchanged pinned core.
 * The include resolves exclusively through the verified reference root. */
#include <lib/picotls.c>

/* Produce a ticket from the completed transcript. The upstream pre-Finished
 * send_session_ticket helper deliberately adds a hypothetical client Finished;
 * it cannot be called here. Reuse its existing session encoder and buffer macros
 * without changing the transcript, record keys, epochs, or handshake state. */
int dotcc_ptls_send_quic_ticket(ptls_t *tls, ptls_buffer_t *output)
{
    int ret = 0;
    size_t original_offset;
    ptls_buffer_t session;
    uint8_t session_small[256], nonce[32];
    uint32_t age_add;
    if (tls == NULL || output == NULL || !ptls_is_server(tls) ||
        tls->state != PTLS_STATE_SERVER_POST_HANDSHAKE || tls->key_schedule == NULL ||
        tls->key_share == NULL || tls->ctx->encrypt_ticket == NULL ||
        tls->ctx->ticket_lifetime == 0 || tls->ctx->max_early_data_size != 0 ||
        tls->server.num_tickets_to_send == 0 || output->off > output->capacity)
        return PTLS_ALERT_UNEXPECTED_MESSAGE;
    original_offset = output->off;
    ptls_buffer_init(&session, session_small, sizeof(session_small));
    tls->ctx->random_bytes(nonce, sizeof(nonce));
    tls->ctx->random_bytes(&age_add, sizeof(age_add));
    if ((ret = encode_session_identifier(tls->ctx, &session, age_add,
                                        ptls_iovec_init(nonce, sizeof(nonce)),
                                        tls->key_schedule, tls->server_name, tls->key_share->id,
                                        tls->cipher_suite->id, tls->negotiated_protocol)) != 0)
        goto Exit;
    ptls_buffer_push_message_body(output, NULL, PTLS_HANDSHAKE_TYPE_NEW_SESSION_TICKET, {
        ptls_buffer_push32(output, tls->ctx->ticket_lifetime);
        ptls_buffer_push32(output, age_add);
        ptls_buffer_push_block(output, 1, { ptls_buffer_pushv(output, nonce, sizeof(nonce)); });
        ptls_buffer_push_block(output, 2, {
            if ((ret = tls->ctx->encrypt_ticket->cb(tls->ctx->encrypt_ticket, tls, 1, output,
                                                   ptls_iovec_init(session.base, session.off))) != 0)
                goto Exit;
        });
        ptls_buffer_push_block(output, 2, {});
    });
Exit:
    if (ret != 0 && output->off > original_offset) {
        ptls_clear_memory(output->base + original_offset, output->off - original_offset);
        output->off = original_offset;
    }
    ptls_clear_memory(session.base, session.off);
    ptls_buffer_dispose(&session);
    ptls_clear_memory(nonce, sizeof(nonce));
    return ret;
}
