//type: fp
//options: 
# 0 "./compat/struct-by-value-22_x.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/struct-by-value-22_x.c"



# 1 "./compat/compat-common.h" 1
# 53 "./compat/compat-common.h"
extern void abort (void);

extern int fails;
# 5 "./compat/struct-by-value-22_x.c" 2
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
# 6 "./compat/struct-by-value-22_x.c" 2
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
# 7 "./compat/struct-by-value-22_x.c" 2
# 22 "./compat/struct-by-value-22_x.c"
# 1 "./compat/struct-by-value-22_x.c" 1
# 58 "./compat/struct-by-value-22_x.c"
extern void testvacE (int n, ...); void testitcE (int n) { struct S { char a[n]; } s; int i; do {} while (0); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacE (n, s, n, s); } extern void testvasE (int n, ...); void testitsE (int n) { struct S { short a[n]; } s; int i; do {} while (0); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasE (n, s, n, s); } extern void testvauE (int n, ...); void testituE (int n) { struct S { unsigned a[n]; } s; int i; do {} while (0); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauE (n, s, n, s); } extern void testvadE (int n, ...); void testitdE (int n) { struct S { double a[n]; } s; int i; do {} while (0); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadE (n, s, n, s); }
extern void testvacn (int n, ...); void testitcn (int n) { struct S { int n; char a[n]; } s; int i; s.n = n; for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacn (n, s, n, s); } extern void testvasn (int n, ...); void testitsn (int n) { struct S { int n; short a[n]; } s; int i; s.n = n; for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasn (n, s, n, s); } extern void testvaun (int n, ...); void testitun (int n) { struct S { int n; unsigned a[n]; } s; int i; s.n = n; for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvaun (n, s, n, s); } extern void testvadn (int n, ...); void testitdn (int n) { struct S { int n; double a[n]; } s; int i; s.n = n; for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadn (n, s, n, s); }


extern void testvacScd (int n, ...); void testitcScd (int n) { struct S { Scd s; char a[n]; } s; int i; initScd (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacScd (n, s, n, s); } extern void testvasScd (int n, ...); void testitsScd (int n) { struct S { Scd s; short a[n]; } s; int i; initScd (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasScd (n, s, n, s); } extern void testvauScd (int n, ...); void testituScd (int n) { struct S { Scd s; unsigned a[n]; } s; int i; initScd (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauScd (n, s, n, s); } extern void testvadScd (int n, ...); void testitdScd (int n) { struct S { Scd s; double a[n]; } s; int i; initScd (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadScd (n, s, n, s); }
extern void testvacScdc (int n, ...); void testitcScdc (int n) { struct S { Scdc s; char a[n]; } s; int i; initScdc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacScdc (n, s, n, s); } extern void testvasScdc (int n, ...); void testitsScdc (int n) { struct S { Scdc s; short a[n]; } s; int i; initScdc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasScdc (n, s, n, s); } extern void testvauScdc (int n, ...); void testituScdc (int n) { struct S { Scdc s; unsigned a[n]; } s; int i; initScdc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauScdc (n, s, n, s); } extern void testvadScdc (int n, ...); void testitdScdc (int n) { struct S { Scdc s; double a[n]; } s; int i; initScdc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadScdc (n, s, n, s); }
extern void testvacSd (int n, ...); void testitcSd (int n) { struct S { Sd s; char a[n]; } s; int i; initSd (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacSd (n, s, n, s); } extern void testvasSd (int n, ...); void testitsSd (int n) { struct S { Sd s; short a[n]; } s; int i; initSd (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasSd (n, s, n, s); } extern void testvauSd (int n, ...); void testituSd (int n) { struct S { Sd s; unsigned a[n]; } s; int i; initSd (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauSd (n, s, n, s); } extern void testvadSd (int n, ...); void testitdSd (int n) { struct S { Sd s; double a[n]; } s; int i; initSd (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadSd (n, s, n, s); }
extern void testvacSdi (int n, ...); void testitcSdi (int n) { struct S { Sdi s; char a[n]; } s; int i; initSdi (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacSdi (n, s, n, s); } extern void testvasSdi (int n, ...); void testitsSdi (int n) { struct S { Sdi s; short a[n]; } s; int i; initSdi (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasSdi (n, s, n, s); } extern void testvauSdi (int n, ...); void testituSdi (int n) { struct S { Sdi s; unsigned a[n]; } s; int i; initSdi (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauSdi (n, s, n, s); } extern void testvadSdi (int n, ...); void testitdSdi (int n) { struct S { Sdi s; double a[n]; } s; int i; initSdi (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadSdi (n, s, n, s); }
extern void testvacScsds (int n, ...); void testitcScsds (int n) { struct S { Scsds s; char a[n]; } s; int i; initScsds (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacScsds (n, s, n, s); } extern void testvasScsds (int n, ...); void testitsScsds (int n) { struct S { Scsds s; short a[n]; } s; int i; initScsds (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasScsds (n, s, n, s); } extern void testvauScsds (int n, ...); void testituScsds (int n) { struct S { Scsds s; unsigned a[n]; } s; int i; initScsds (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauScsds (n, s, n, s); } extern void testvadScsds (int n, ...); void testitdScsds (int n) { struct S { Scsds s; double a[n]; } s; int i; initScsds (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadScsds (n, s, n, s); }
extern void testvacScsdsc (int n, ...); void testitcScsdsc (int n) { struct S { Scsdsc s; char a[n]; } s; int i; initScsdsc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacScsdsc (n, s, n, s); } extern void testvasScsdsc (int n, ...); void testitsScsdsc (int n) { struct S { Scsdsc s; short a[n]; } s; int i; initScsdsc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasScsdsc (n, s, n, s); } extern void testvauScsdsc (int n, ...); void testituScsdsc (int n) { struct S { Scsdsc s; unsigned a[n]; } s; int i; initScsdsc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauScsdsc (n, s, n, s); } extern void testvadScsdsc (int n, ...); void testitdScsdsc (int n) { struct S { Scsdsc s; double a[n]; } s; int i; initScsdsc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadScsdsc (n, s, n, s); }
extern void testvacScsdis (int n, ...); void testitcScsdis (int n) { struct S { Scsdis s; char a[n]; } s; int i; initScsdis (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacScsdis (n, s, n, s); } extern void testvasScsdis (int n, ...); void testitsScsdis (int n) { struct S { Scsdis s; short a[n]; } s; int i; initScsdis (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasScsdis (n, s, n, s); } extern void testvauScsdis (int n, ...); void testituScsdis (int n) { struct S { Scsdis s; unsigned a[n]; } s; int i; initScsdis (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauScsdis (n, s, n, s); } extern void testvadScsdis (int n, ...); void testitdScsdis (int n) { struct S { Scsdis s; double a[n]; } s; int i; initScsdis (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadScsdis (n, s, n, s); }
extern void testvacScsdisc (int n, ...); void testitcScsdisc (int n) { struct S { Scsdisc s; char a[n]; } s; int i; initScsdisc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacScsdisc (n, s, n, s); } extern void testvasScsdisc (int n, ...); void testitsScsdisc (int n) { struct S { Scsdisc s; short a[n]; } s; int i; initScsdisc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasScsdisc (n, s, n, s); } extern void testvauScsdisc (int n, ...); void testituScsdisc (int n) { struct S { Scsdisc s; unsigned a[n]; } s; int i; initScsdisc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauScsdisc (n, s, n, s); } extern void testvadScsdisc (int n, ...); void testitdScsdisc (int n) { struct S { Scsdisc s; double a[n]; } s; int i; initScsdisc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadScsdisc (n, s, n, s); }
extern void testvacSsds (int n, ...); void testitcSsds (int n) { struct S { Ssds s; char a[n]; } s; int i; initSsds (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacSsds (n, s, n, s); } extern void testvasSsds (int n, ...); void testitsSsds (int n) { struct S { Ssds s; short a[n]; } s; int i; initSsds (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasSsds (n, s, n, s); } extern void testvauSsds (int n, ...); void testituSsds (int n) { struct S { Ssds s; unsigned a[n]; } s; int i; initSsds (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauSsds (n, s, n, s); } extern void testvadSsds (int n, ...); void testitdSsds (int n) { struct S { Ssds s; double a[n]; } s; int i; initSsds (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadSsds (n, s, n, s); }
extern void testvacSsdsc (int n, ...); void testitcSsdsc (int n) { struct S { Ssdsc s; char a[n]; } s; int i; initSsdsc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacSsdsc (n, s, n, s); } extern void testvasSsdsc (int n, ...); void testitsSsdsc (int n) { struct S { Ssdsc s; short a[n]; } s; int i; initSsdsc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasSsdsc (n, s, n, s); } extern void testvauSsdsc (int n, ...); void testituSsdsc (int n) { struct S { Ssdsc s; unsigned a[n]; } s; int i; initSsdsc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauSsdsc (n, s, n, s); } extern void testvadSsdsc (int n, ...); void testitdSsdsc (int n) { struct S { Ssdsc s; double a[n]; } s; int i; initSsdsc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadSsdsc (n, s, n, s); }
extern void testvacScssdss (int n, ...); void testitcScssdss (int n) { struct S { Scssdss s; char a[n]; } s; int i; initScssdss (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacScssdss (n, s, n, s); } extern void testvasScssdss (int n, ...); void testitsScssdss (int n) { struct S { Scssdss s; short a[n]; } s; int i; initScssdss (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasScssdss (n, s, n, s); } extern void testvauScssdss (int n, ...); void testituScssdss (int n) { struct S { Scssdss s; unsigned a[n]; } s; int i; initScssdss (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauScssdss (n, s, n, s); } extern void testvadScssdss (int n, ...); void testitdScssdss (int n) { struct S { Scssdss s; double a[n]; } s; int i; initScssdss (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadScssdss (n, s, n, s); }
extern void testvacScssdssc (int n, ...); void testitcScssdssc (int n) { struct S { Scssdssc s; char a[n]; } s; int i; initScssdssc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacScssdssc (n, s, n, s); } extern void testvasScssdssc (int n, ...); void testitsScssdssc (int n) { struct S { Scssdssc s; short a[n]; } s; int i; initScssdssc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasScssdssc (n, s, n, s); } extern void testvauScssdssc (int n, ...); void testituScssdssc (int n) { struct S { Scssdssc s; unsigned a[n]; } s; int i; initScssdssc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauScssdssc (n, s, n, s); } extern void testvadScssdssc (int n, ...); void testitdScssdssc (int n) { struct S { Scssdssc s; double a[n]; } s; int i; initScssdssc (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadScssdssc (n, s, n, s); }
extern void testvacSfi (int n, ...); void testitcSfi (int n) { struct S { Sfi s; char a[n]; } s; int i; initSfi (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacSfi (n, s, n, s); } extern void testvasSfi (int n, ...); void testitsSfi (int n) { struct S { Sfi s; short a[n]; } s; int i; initSfi (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasSfi (n, s, n, s); } extern void testvauSfi (int n, ...); void testituSfi (int n) { struct S { Sfi s; unsigned a[n]; } s; int i; initSfi (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauSfi (n, s, n, s); } extern void testvadSfi (int n, ...); void testitdSfi (int n) { struct S { Sfi s; double a[n]; } s; int i; initSfi (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadSfi (n, s, n, s); }
extern void testvacSfii (int n, ...); void testitcSfii (int n) { struct S { Sfii s; char a[n]; } s; int i; initSfii (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacSfii (n, s, n, s); } extern void testvasSfii (int n, ...); void testitsSfii (int n) { struct S { Sfii s; short a[n]; } s; int i; initSfii (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasSfii (n, s, n, s); } extern void testvauSfii (int n, ...); void testituSfii (int n) { struct S { Sfii s; unsigned a[n]; } s; int i; initSfii (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauSfii (n, s, n, s); } extern void testvadSfii (int n, ...); void testitdSfii (int n) { struct S { Sfii s; double a[n]; } s; int i; initSfii (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadSfii (n, s, n, s); }
extern void testvacSfifi (int n, ...); void testitcSfifi (int n) { struct S { Sfifi s; char a[n]; } s; int i; initSfifi (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacSfifi (n, s, n, s); } extern void testvasSfifi (int n, ...); void testitsSfifi (int n) { struct S { Sfifi s; short a[n]; } s; int i; initSfifi (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasSfifi (n, s, n, s); } extern void testvauSfifi (int n, ...); void testituSfifi (int n) { struct S { Sfifi s; unsigned a[n]; } s; int i; initSfifi (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauSfifi (n, s, n, s); } extern void testvadSfifi (int n, ...); void testitdSfifi (int n) { struct S { Sfifi s; double a[n]; } s; int i; initSfifi (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadSfifi (n, s, n, s); }
extern void testvacSfiifii (int n, ...); void testitcSfiifii (int n) { struct S { Sfiifii s; char a[n]; } s; int i; initSfiifii (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvacSfiifii (n, s, n, s); } extern void testvasSfiifii (int n, ...); void testitsSfiifii (int n) { struct S { Sfiifii s; short a[n]; } s; int i; initSfiifii (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvasSfiifii (n, s, n, s); } extern void testvauSfiifii (int n, ...); void testituSfiifii (int n) { struct S { Sfiifii s; unsigned a[n]; } s; int i; initSfiifii (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvauSfiifii (n, s, n, s); } extern void testvadSfiifii (int n, ...); void testitdSfiifii (int n) { struct S { Sfiifii s; double a[n]; } s; int i; initSfiifii (&s.s, n); for (i = 0; i < n; ++i) s.a[i] = 12 + n - i; testvadSfiifii (n, s, n, s); }
# 23 "./compat/struct-by-value-22_x.c" 2



void
struct_by_value_22_x ()
{
  int n;




  for (n = 0; n < 16; ++n)
    {
# 1 "./compat/struct-by-value-22_x.c" 1
# 58 "./compat/struct-by-value-22_x.c"
testitcE (n); testitsE (n); testituE (n); testitdE (n);
testitcn (n); testitsn (n); testitun (n); testitdn (n);


testitcScd (n); testitsScd (n); testituScd (n); testitdScd (n);
testitcScdc (n); testitsScdc (n); testituScdc (n); testitdScdc (n);
testitcSd (n); testitsSd (n); testituSd (n); testitdSd (n);
testitcSdi (n); testitsSdi (n); testituSdi (n); testitdSdi (n);
testitcScsds (n); testitsScsds (n); testituScsds (n); testitdScsds (n);
testitcScsdsc (n); testitsScsdsc (n); testituScsdsc (n); testitdScsdsc (n);
testitcScsdis (n); testitsScsdis (n); testituScsdis (n); testitdScsdis (n);
testitcScsdisc (n); testitsScsdisc (n); testituScsdisc (n); testitdScsdisc (n);
testitcSsds (n); testitsSsds (n); testituSsds (n); testitdSsds (n);
testitcSsdsc (n); testitsSsdsc (n); testituSsdsc (n); testitdSsdsc (n);
testitcScssdss (n); testitsScssdss (n); testituScssdss (n); testitdScssdss (n);
testitcScssdssc (n); testitsScssdssc (n); testituScssdssc (n); testitdScssdssc (n);
testitcSfi (n); testitsSfi (n); testituSfi (n); testitdSfi (n);
testitcSfii (n); testitsSfii (n); testituSfii (n); testitdSfii (n);
testitcSfifi (n); testitsSfifi (n); testituSfifi (n); testitdSfifi (n);
testitcSfiifii (n); testitsSfiifii (n); testituSfiifii (n); testitdSfiifii (n);
# 37 "./compat/struct-by-value-22_x.c" 2
      ;
    }
  for (; n < 110; n += 13)
    {
# 1 "./compat/struct-by-value-22_x.c" 1
# 58 "./compat/struct-by-value-22_x.c"
testitcE (n); testitsE (n); testituE (n); testitdE (n);
testitcn (n); testitsn (n); testitun (n); testitdn (n);


testitcScd (n); testitsScd (n); testituScd (n); testitdScd (n);
testitcScdc (n); testitsScdc (n); testituScdc (n); testitdScdc (n);
testitcSd (n); testitsSd (n); testituSd (n); testitdSd (n);
testitcSdi (n); testitsSdi (n); testituSdi (n); testitdSdi (n);
testitcScsds (n); testitsScsds (n); testituScsds (n); testitdScsds (n);
testitcScsdsc (n); testitsScsdsc (n); testituScsdsc (n); testitdScsdsc (n);
testitcScsdis (n); testitsScsdis (n); testituScsdis (n); testitdScsdis (n);
testitcScsdisc (n); testitsScsdisc (n); testituScsdisc (n); testitdScsdisc (n);
testitcSsds (n); testitsSsds (n); testituSsds (n); testitdSsds (n);
testitcSsdsc (n); testitsSsdsc (n); testituSsdsc (n); testitdSsdsc (n);
testitcScssdss (n); testitsScssdss (n); testituScssdss (n); testitdScssdss (n);
testitcScssdssc (n); testitsScssdssc (n); testituScssdssc (n); testitdScssdssc (n);
testitcSfi (n); testitsSfi (n); testituSfi (n); testitdSfi (n);
testitcSfii (n); testitsSfii (n); testituSfii (n); testitdSfii (n);
testitcSfifi (n); testitsSfifi (n); testituSfifi (n); testitdSfifi (n);
testitcSfiifii (n); testitsSfiifii (n); testituSfiifii (n); testitdSfiifii (n);
# 42 "./compat/struct-by-value-22_x.c" 2
      ;
    }



if (fails != 0)
  abort ();
}
