typedef struct { unsigned char *base; unsigned long len; } ptls_iovec_t;
ptls_iovec_t ptls_iovec_init(void *base, unsigned long len) { ptls_iovec_t value = {base, len}; return value; }
int main(void)
{
    unsigned char input = 7;
    ptls_iovec_t invec[3] = {ptls_iovec_init(&input, 1), ptls_iovec_init(&input, 2)};
    return invec[0].len != 1 || invec[1].len != 2 || invec[2].len != 0;
}
