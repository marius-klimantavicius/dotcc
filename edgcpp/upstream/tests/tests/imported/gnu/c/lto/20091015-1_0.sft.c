//type: fp
//options: 
# 0 "./lto/20091015-1_0.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20091015-1_0.c"





# 1 "./lto/20091015-1_b.h" 1
typedef struct _IO_FILE FILE;
extern struct _IO_FILE *stderr;
# 7 "./lto/20091015-1_0.c" 2
void diagnostic_initialize (FILE **stream) { *stream = stderr; }
