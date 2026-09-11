/* Compile against the pinned, unmodified public header. These are probe-only
 * appended contexts, not a production provider. */
#include <stddef.h>
#include <stdio.h>
#include "picotls.h"
struct hash_with_handle { ptls_hash_context_t header; intptr_t handle; };
struct aead_with_handle { ptls_aead_context_t header; intptr_t handle; };
#define SIZE(T) printf(#T ".size=%zu\n", sizeof(T)); printf(#T ".align=%zu\n", _Alignof(T))
#define FIELD(T, F) printf(#T "." #F "=%zu\n", offsetof(T,F))
int main(void) {
    SIZE(ptls_iovec_t); FIELD(ptls_iovec_t, base); FIELD(ptls_iovec_t, len);
    SIZE(ptls_buffer_t); FIELD(ptls_buffer_t, base); FIELD(ptls_buffer_t, capacity); FIELD(ptls_buffer_t, off); FIELD(ptls_buffer_t, is_allocated); FIELD(ptls_buffer_t, align_bits);
    SIZE(ptls_hash_context_t); FIELD(ptls_hash_context_t, update); FIELD(ptls_hash_context_t, final); FIELD(ptls_hash_context_t, clone_);
    SIZE(ptls_hash_algorithm_t); FIELD(ptls_hash_algorithm_t, name); FIELD(ptls_hash_algorithm_t, block_size); FIELD(ptls_hash_algorithm_t, digest_size); FIELD(ptls_hash_algorithm_t, create); FIELD(ptls_hash_algorithm_t, empty_digest);
    SIZE(ptls_cipher_context_t); FIELD(ptls_cipher_context_t, algo); FIELD(ptls_cipher_context_t, do_dispose); FIELD(ptls_cipher_context_t, do_init); FIELD(ptls_cipher_context_t, do_transform);
    SIZE(ptls_cipher_algorithm_t); FIELD(ptls_cipher_algorithm_t, name); FIELD(ptls_cipher_algorithm_t, key_size); FIELD(ptls_cipher_algorithm_t, block_size); FIELD(ptls_cipher_algorithm_t, iv_size); FIELD(ptls_cipher_algorithm_t, context_size); FIELD(ptls_cipher_algorithm_t, setup_crypto);
    SIZE(ptls_aead_context_t); FIELD(ptls_aead_context_t, algo); FIELD(ptls_aead_context_t, dispose_crypto); FIELD(ptls_aead_context_t, do_get_iv); FIELD(ptls_aead_context_t, do_set_iv); FIELD(ptls_aead_context_t, do_encrypt_init); FIELD(ptls_aead_context_t, do_encrypt_update); FIELD(ptls_aead_context_t, do_encrypt_final); FIELD(ptls_aead_context_t, do_encrypt); FIELD(ptls_aead_context_t, do_encrypt_v); FIELD(ptls_aead_context_t, do_decrypt);
    SIZE(ptls_aead_algorithm_t); FIELD(ptls_aead_algorithm_t, name); FIELD(ptls_aead_algorithm_t, confidentiality_limit); FIELD(ptls_aead_algorithm_t, integrity_limit); FIELD(ptls_aead_algorithm_t, ctr_cipher); FIELD(ptls_aead_algorithm_t, ecb_cipher); FIELD(ptls_aead_algorithm_t, key_size); FIELD(ptls_aead_algorithm_t, iv_size); FIELD(ptls_aead_algorithm_t, tag_size); FIELD(ptls_aead_algorithm_t, tls12); FIELD(ptls_aead_algorithm_t, align_bits); FIELD(ptls_aead_algorithm_t, context_size); FIELD(ptls_aead_algorithm_t, setup_crypto);
    SIZE(ptls_aead_supplementary_encryption_t); FIELD(ptls_aead_supplementary_encryption_t, ctx); FIELD(ptls_aead_supplementary_encryption_t, input); FIELD(ptls_aead_supplementary_encryption_t, output);
    SIZE(ptls_key_exchange_context_t); FIELD(ptls_key_exchange_context_t, algo); FIELD(ptls_key_exchange_context_t, pubkey); FIELD(ptls_key_exchange_context_t, on_exchange);
    SIZE(ptls_key_exchange_algorithm_t); FIELD(ptls_key_exchange_algorithm_t, id); FIELD(ptls_key_exchange_algorithm_t, create); FIELD(ptls_key_exchange_algorithm_t, exchange); FIELD(ptls_key_exchange_algorithm_t, data); FIELD(ptls_key_exchange_algorithm_t, name);
    SIZE(ptls_verify_certificate_t); FIELD(ptls_verify_certificate_t, cb); FIELD(ptls_verify_certificate_t, algos);
    SIZE(struct hash_with_handle); FIELD(struct hash_with_handle, header); FIELD(struct hash_with_handle, handle);
    SIZE(struct aead_with_handle); FIELD(struct aead_with_handle, header); FIELD(struct aead_with_handle, handle);
    return 0;
}
