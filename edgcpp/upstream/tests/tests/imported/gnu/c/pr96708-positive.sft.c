//type: rp
//options: 
# 0 "./pr96708-positive.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr96708-positive.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdbool.h" 1 3 4
# 5 "./pr96708-positive.c" 2

bool __attribute__ ((noinline))
test1(int a, int b)
{
    int tmp = (a < b) ? b : a;
    return tmp >= a;
}

bool __attribute__ ((noinline))
test2(int a, int b)
{
    int tmp = (a < b) ? b : a;
    return tmp < a;
}

bool __attribute__ ((noinline))
test3(int a, int b)
{
    int tmp = (a > b) ? b : a;
    return tmp <= a;
}

bool __attribute__ ((noinline))
test4(int a, int b)
{
    int tmp = (a > b) ? b : a;
    return tmp > a;
}

int main()
{
    if (!test1 (1, 2) || !test1 (2, 1) ||
        test2 (1, 2) || test2 (2, 1) ||
        !test3 (1, 2) || !test3 (2, 1) ||
        test4 (1, 2) || test4 (2, 1)) {
        __builtin_abort();
    }
    return 0;
}
