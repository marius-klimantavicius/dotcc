//type: fp
//options: --c17 --strict_gnu
# 0 "./lto/20091015-1_2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20091015-1_2.c"

# 1 "./lto/20091015-1_a.h" 1
struct _IO_FILE { int _flags;
};
# 3 "./lto/20091015-1_2.c" 2
# 1 "./lto/20091015-1_b.h" 1
typedef struct _IO_FILE FILE;
extern struct _IO_FILE *stderr;
# 4 "./lto/20091015-1_2.c" 2
void foo ();
void debug_optab_libfuncs (void) {
foo (stderr, 4 );
}
