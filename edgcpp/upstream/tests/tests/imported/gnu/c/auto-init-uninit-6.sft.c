//type: fp
//options: 
# 0 "./auto-init-uninit-6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-6.c"






# 1 "./uninit-6.c" 1






# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
} max_align_t;
# 450 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
  typedef __typeof__(nullptr) nullptr_t;
# 8 "./uninit-6.c" 2


# 9 "./uninit-6.c"
struct tree
{
    struct tree *car;
    struct tree *cdr;
    int type, data;
};

extern void *malloc(size_t);
# 33 "./uninit-6.c"
struct tree *
make_something(int a, int b, int c)
{
    struct tree *rv;
    struct tree *field;

    rv = malloc (sizeof (struct tree));
    rv->car = 0;

    do { struct tree *tmp = malloc (sizeof (struct tree)); tmp->car = 0; tmp->cdr = 0; tmp->type = 1; tmp->data = a; if (rv->car) field->cdr = tmp; else rv->car = tmp; field = tmp; } while(0);
    do { struct tree *tmp = malloc (sizeof (struct tree)); tmp->car = 0; tmp->cdr = 0; tmp->type = 2; tmp->data = b; if (rv->car) field->cdr = tmp; else rv->car = tmp; field = tmp; } while(0);
    do { struct tree *tmp = malloc (sizeof (struct tree)); tmp->car = 0; tmp->cdr = 0; tmp->type = 1; tmp->data = c; if (rv->car) field->cdr = tmp; else rv->car = tmp; field = tmp; } while(0);

    return rv;
}
# 8 "./auto-init-uninit-6.c" 2
