//type: fp
//options: 
# 0 "./compat/struct-return-20_y.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/struct-return-20_y.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 2 "./compat/struct-return-20_y.c" 2

# 1 "./compat/compat-common.h" 1
# 53 "./compat/compat-common.h"

# 53 "./compat/compat-common.h"
extern void abort (void);

extern int fails;
# 4 "./compat/struct-return-20_y.c" 2

# 1 "./compat/mixed-struct-defs.h" 1


typedef struct { char c; double d; } Scd;
typedef struct { char c; double d; char b; } Scdc;
typedef struct { double d; } Sd;
typedef struct { double d; int i; } Sdi;
typedef struct { char c; Sd sd; } Scsds;
typedef struct { char c; Sd sd; char b; } Scsdsc;
typedef struct { char c; Sdi sdi; } Scsdis;
typedef struct { char c; Sdi sdi; char b; } Scsdisc;
typedef struct { Sd sd; } Ssds;
typedef struct { Sd sd; char c; } Ssdsc;
typedef struct { char c; Ssds ssds; } Scssdss;
typedef struct { char c; Ssds ssds; char b; } Scssdssc;

typedef struct { float f; int i; } Sfi;
typedef struct { float f; int i1; int i2; } Sfii;
typedef struct { float f; int i; Sfi fi; } Sfifi;
typedef struct { float f; int i1; int i2; Sfii fii; } Sfiifii;
# 6 "./compat/struct-return-20_y.c" 2
# 1 "./compat/mixed-struct-init.h" 1


void initScd (Scd *p, int i)
{ p->c = (char)i; p->d = (double)i+1; }
void initScdc (Scdc *p, int i)
{ p->c = (char)i; p->d = (double)i+1; p->b = (char)i+2; }
void initSd (Sd *p, int i)
{ p->d = (double)i; }
void initSdi (Sdi *p, int i)
{ p->d = (double)i; p->i = i+1; }
void initScsds (Scsds *p, int i)
{ p->c = (char)i; p->sd.d = (double)i+1; }
void initScsdsc (Scsdsc *p, int i)
{ p->c = (char)i; p->sd.d = (double)i+1; p->b = (char)i+2; }
void initScsdis (Scsdis *p, int i)
{ p->c = (char)i; p->sdi.d = (double)i+1; p->sdi.i = i+2; }
void initScsdisc (Scsdisc *p, int i)
{ p->c = (char)i; p->sdi.d = (double)i+1; p->sdi.i = i+2; p->b = (char)i+3; }
void initSsds (Ssds *p, int i)
{ p->sd.d = (double)i; }
void initSsdsc (Ssdsc *p, int i)
{ p->sd.d = (double)i; p->c = (char)i+1; }
void initScssdss (Scssdss *p, int i)
{ p->c = (char)i; p->ssds.sd.d = (double)i+1; }
void initScssdssc (Scssdssc *p, int i)
{ p->c = (char)i; p->ssds.sd.d = (double)i+1; p->b = (char)i+2; }

void initSfi (Sfi *x, int i)
{ x->f = (float)i; x->i = i+1; }
void initSfii (Sfii *x, int i)
{ x->f = (float)i; x->i1 = i+1; x->i2 = i+2; }
void initSfifi (Sfifi *x, int i)
{ x->fi.f = (float)i; x->fi.i = i+1; }
void initSfiifii (Sfiifii *x, int i)
{ x->fii.f = (float)i; x->fii.i1 = i+1; x->fii.i2 = i+2; }
# 7 "./compat/struct-return-20_y.c" 2
# 62 "./compat/struct-return-20_y.c"
extern Scd g01Scd, g02Scd, g03Scd, g04Scd; extern Scd g05Scd, g06Scd, g07Scd, g08Scd; extern Scd g09Scd, g10Scd, g11Scd, g12Scd; extern Scd g13Scd, g14Scd, g15Scd, g16Scd; extern void checkScd (Scd x, int i); void checkgScd (void) { checkScd (g01Scd, 1); checkScd (g02Scd, 2); checkScd (g03Scd, 3); checkScd (g04Scd, 4); checkScd (g05Scd, 5); checkScd (g06Scd, 6); checkScd (g07Scd, 7); checkScd (g08Scd, 8); checkScd (g09Scd, 9); checkScd (g10Scd, 10); checkScd (g11Scd, 11); checkScd (g12Scd, 12); checkScd (g13Scd, 13); checkScd (g14Scd, 14); checkScd (g15Scd, 15); checkScd (g16Scd, 16); } Scd test0Scd (void) { return g01Scd; } Scd test1Scd (Scd x01) { return x01; } Scd testvaScd (int n, ...) { int i; Scd rslt; va_list ap; 
# 62 "./compat/struct-return-20_y.c" 3 4
__builtin_c23_va_start(
# 62 "./compat/struct-return-20_y.c"
ap, n
# 62 "./compat/struct-return-20_y.c" 3 4
)
# 62 "./compat/struct-return-20_y.c"
; for (i = 0; i < n; i++) rslt = 
# 62 "./compat/struct-return-20_y.c" 3 4
__builtin_va_arg(
# 62 "./compat/struct-return-20_y.c"
ap
# 62 "./compat/struct-return-20_y.c" 3 4
,
# 62 "./compat/struct-return-20_y.c"
Scd
# 62 "./compat/struct-return-20_y.c" 3 4
)
# 62 "./compat/struct-return-20_y.c"
; 
# 62 "./compat/struct-return-20_y.c" 3 4
__builtin_va_end(
# 62 "./compat/struct-return-20_y.c"
ap
# 62 "./compat/struct-return-20_y.c" 3 4
)
# 62 "./compat/struct-return-20_y.c"
; return rslt; }
extern Scsds g01Scsds, g02Scsds, g03Scsds, g04Scsds; extern Scsds g05Scsds, g06Scsds, g07Scsds, g08Scsds; extern Scsds g09Scsds, g10Scsds, g11Scsds, g12Scsds; extern Scsds g13Scsds, g14Scsds, g15Scsds, g16Scsds; extern void checkScsds (Scsds x, int i); void checkgScsds (void) { checkScsds (g01Scsds, 1); checkScsds (g02Scsds, 2); checkScsds (g03Scsds, 3); checkScsds (g04Scsds, 4); checkScsds (g05Scsds, 5); checkScsds (g06Scsds, 6); checkScsds (g07Scsds, 7); checkScsds (g08Scsds, 8); checkScsds (g09Scsds, 9); checkScsds (g10Scsds, 10); checkScsds (g11Scsds, 11); checkScsds (g12Scsds, 12); checkScsds (g13Scsds, 13); checkScsds (g14Scsds, 14); checkScsds (g15Scsds, 15); checkScsds (g16Scsds, 16); } Scsds test0Scsds (void) { return g01Scsds; } Scsds test1Scsds (Scsds x01) { return x01; } Scsds testvaScsds (int n, ...) { int i; Scsds rslt; va_list ap; 
# 63 "./compat/struct-return-20_y.c" 3 4
__builtin_c23_va_start(
# 63 "./compat/struct-return-20_y.c"
ap, n
# 63 "./compat/struct-return-20_y.c" 3 4
)
# 63 "./compat/struct-return-20_y.c"
; for (i = 0; i < n; i++) rslt = 
# 63 "./compat/struct-return-20_y.c" 3 4
__builtin_va_arg(
# 63 "./compat/struct-return-20_y.c"
ap
# 63 "./compat/struct-return-20_y.c" 3 4
,
# 63 "./compat/struct-return-20_y.c"
Scsds
# 63 "./compat/struct-return-20_y.c" 3 4
)
# 63 "./compat/struct-return-20_y.c"
; 
# 63 "./compat/struct-return-20_y.c" 3 4
__builtin_va_end(
# 63 "./compat/struct-return-20_y.c"
ap
# 63 "./compat/struct-return-20_y.c" 3 4
)
# 63 "./compat/struct-return-20_y.c"
; return rslt; }
extern Scssdss g01Scssdss, g02Scssdss, g03Scssdss, g04Scssdss; extern Scssdss g05Scssdss, g06Scssdss, g07Scssdss, g08Scssdss; extern Scssdss g09Scssdss, g10Scssdss, g11Scssdss, g12Scssdss; extern Scssdss g13Scssdss, g14Scssdss, g15Scssdss, g16Scssdss; extern void checkScssdss (Scssdss x, int i); void checkgScssdss (void) { checkScssdss (g01Scssdss, 1); checkScssdss (g02Scssdss, 2); checkScssdss (g03Scssdss, 3); checkScssdss (g04Scssdss, 4); checkScssdss (g05Scssdss, 5); checkScssdss (g06Scssdss, 6); checkScssdss (g07Scssdss, 7); checkScssdss (g08Scssdss, 8); checkScssdss (g09Scssdss, 9); checkScssdss (g10Scssdss, 10); checkScssdss (g11Scssdss, 11); checkScssdss (g12Scssdss, 12); checkScssdss (g13Scssdss, 13); checkScssdss (g14Scssdss, 14); checkScssdss (g15Scssdss, 15); checkScssdss (g16Scssdss, 16); } Scssdss test0Scssdss (void) { return g01Scssdss; } Scssdss test1Scssdss (Scssdss x01) { return x01; } Scssdss testvaScssdss (int n, ...) { int i; Scssdss rslt; va_list ap; 
# 64 "./compat/struct-return-20_y.c" 3 4
__builtin_c23_va_start(
# 64 "./compat/struct-return-20_y.c"
ap, n
# 64 "./compat/struct-return-20_y.c" 3 4
)
# 64 "./compat/struct-return-20_y.c"
; for (i = 0; i < n; i++) rslt = 
# 64 "./compat/struct-return-20_y.c" 3 4
__builtin_va_arg(
# 64 "./compat/struct-return-20_y.c"
ap
# 64 "./compat/struct-return-20_y.c" 3 4
,
# 64 "./compat/struct-return-20_y.c"
Scssdss
# 64 "./compat/struct-return-20_y.c" 3 4
)
# 64 "./compat/struct-return-20_y.c"
; 
# 64 "./compat/struct-return-20_y.c" 3 4
__builtin_va_end(
# 64 "./compat/struct-return-20_y.c"
ap
# 64 "./compat/struct-return-20_y.c" 3 4
)
# 64 "./compat/struct-return-20_y.c"
; return rslt; }
