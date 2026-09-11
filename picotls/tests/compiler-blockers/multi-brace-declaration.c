typedef struct { unsigned char *base; unsigned long len; } ptls_iovec_t;
int main(void)
{
    ptls_iovec_t pubkey = {0}, ecdh_secret = {0};
    return pubkey.base != 0 || pubkey.len != 0 || ecdh_secret.base != 0 || ecdh_secret.len != 0;
}
