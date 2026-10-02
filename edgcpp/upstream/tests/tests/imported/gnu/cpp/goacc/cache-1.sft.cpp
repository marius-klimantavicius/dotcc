//type: fp
//options: 
# 0 "./goacc/cache-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./goacc/cache-1.C"
# 9 "./goacc/cache-1.C"
# 1 "./goacc/../../c-c++-common/goacc/cache-1.c" 1
# 9 "./goacc/../../c-c++-common/goacc/cache-1.c"
template <int N>

static void
test ()
{

    int a[2], b[2];
    int i;

    for (i = 0; i < 2; i++)
    {
        a[i] = 3;
        b[i] = 0;
    }

#pragma acc parallel copyin (a[0:N]) copyout (b[0:N])
{
    int ii;

    for (ii = 0; ii < 2; ii++)
    {
        const int idx = ii;
        int n = 1;
        const int len = n;


#pragma acc cache (a[0:N])
#pragma acc cache (a[0:N], a[0:N])
#pragma acc cache (a[0:N], b[0:N])
#pragma acc cache (a[0])
#pragma acc cache (a[0], a[1], b[0:N])
#pragma acc cache (a[i - 5])
#pragma acc cache (a[i + 5:len])
#pragma acc cache (a[i + 5:len - 1])
#pragma acc cache (b[i])
#pragma acc cache (b[i:len])
#pragma acc cache (a[ii])
#pragma acc cache (a[ii:len])
#pragma acc cache (b[ii - 1])
#pragma acc cache (b[ii - 1:len])
#pragma acc cache (b[i - ii + 1])
#pragma acc cache (b[i + ii - 1:len])
#pragma acc cache (b[i * ii - 1:len + 1])
#pragma acc cache (a[idx + 2])
#pragma acc cache (a[idx:len + 2])
#pragma acc cache (a[idx])
#pragma acc cache (a[idx:len])
#pragma acc cache (a[idx + 2:len])
#pragma acc cache (a[idx + 2 + i:len])
#pragma acc cache (a[idx + 2 + i + ii:len])

        b[ii] = a[ii];
    }
}


    for (i = 0; i < 2; i++)
    {
        if (a[i] != b[i])
            __builtin_abort ();
    }
}
# 10 "./goacc/cache-1.C" 2

static void
instantiate ()
{
  &test<0>;
}
