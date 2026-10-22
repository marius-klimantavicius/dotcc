#include <stdio.h>
char first[] = "ab", second[5] = "cd", third[] = "e" "f";
int run(void) {
    static const char lower[] = "0123456789abcdef", upper[] = "0123456789ABCDEF";
    char a[] = "gh", b[4] = "ij", c[] = "kl";
    a[0] = 'G';
    printf("%zu %zu %zu %zu %zu %zu %c %c %c %d\n", sizeof(first), sizeof(second), sizeof(third),
           sizeof(lower), sizeof(upper), sizeof(b), a[0], b[1], c[0], b[3]);
    return 0;
}
int main(void) { return run(); }
