//type: rp
//options: 
# 0 "./pr104992-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr104992-1.C"




# 1 "./../gcc.dg/pr104992.c" 1







__attribute__((noipa)) unsigned foo(unsigned x, unsigned y)
{
    return x / y * y == x;
}

__attribute__((noipa)) unsigned bar(unsigned x, unsigned y) {
    return x == x / y * y;
}


__attribute__((noipa)) unsigned baz (int x, int y) {
    return x / y * y == x;
}


__attribute__((noipa)) unsigned qux (unsigned x, unsigned y) {
    return y * (x / y) == x;
}


__attribute__((noipa)) unsigned corge(unsigned x, unsigned y) {
    int z = x / y;
    int q = z * y;
    return q == x;
}


__attribute__((noipa)) __attribute__((vector_size(4*sizeof(int)))) int thud(__attribute__((vector_size(4*sizeof(int)))) int x, __attribute__((vector_size(4*sizeof(int)))) int y) {
    return x / y * y == x;
}


__attribute__((noipa)) int goo(_Complex int x, _Complex int y)
{
    _Complex int z = x / y;
    _Complex int q = z * y;
    return q == x;
}


__attribute__((noipa)) unsigned fred (unsigned x, unsigned y) {
    return y * x / y == x;
}


__attribute__((noipa)) unsigned waldo (unsigned x, unsigned y, unsigned z) {
    return x / y * z == x;
}
# 6 "./pr104992-1.C" 2

int main () {


    if (!foo(6, 3)
        || !bar(12, 2)
        || !baz(34, 17)
        || !qux(50, 10)
        || !fred(16, 8)
        || !baz(-9, 3)
        || !baz(9, -3)
        || !baz(-9, -3)
        ) {
            __builtin_abort();
         }


    if (foo(5, 30)
        || bar(72, 27)
        || baz(42, 15)) {
            __builtin_abort();
        }

    return 0;
}
