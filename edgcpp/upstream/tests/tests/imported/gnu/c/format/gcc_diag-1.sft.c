//type: fp
//options: 
# 0 "./format/gcc_diag-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/gcc_diag-1.c"





# 1 "./format/format.h" 1
# 35 "./format/format.h"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 36 "./format/format.h" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
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
# 37 "./format/format.h" 2






# 42 "./format/format.h"
typedef unsigned int wint_t;
# 61 "./format/format.h"
typedef long signed int signed_size_t;

typedef long signed int ssize_t;


typedef unsigned long int unsigned_ptrdiff_t;


__extension__ typedef long long int llong;
__extension__ typedef unsigned long long int ullong;



typedef llong quad_t;
typedef ullong u_quad_t;

__extension__ typedef long int intmax_t;
__extension__ typedef long unsigned int uintmax_t;

__extension__ typedef signed char int_least8_t;
__extension__ typedef short int int_least16_t;
__extension__ typedef int int_least32_t;
__extension__ typedef long int int_least64_t;
__extension__ typedef unsigned char uint_least8_t;
__extension__ typedef short unsigned int uint_least16_t;
__extension__ typedef unsigned int uint_least32_t;
__extension__ typedef long unsigned int uint_least64_t;

__extension__ typedef signed char int_fast8_t;
__extension__ typedef long int int_fast16_t;
__extension__ typedef long int int_fast32_t;
__extension__ typedef long int int_fast64_t;
__extension__ typedef unsigned char uint_fast8_t;
__extension__ typedef long unsigned int uint_fast16_t;
__extension__ typedef long unsigned int uint_fast32_t;
__extension__ typedef long unsigned int uint_fast64_t;
# 105 "./format/format.h"
typedef struct _FILE FILE;
extern FILE *stdin;
extern FILE *stdout;

extern int fprintf (FILE *restrict, const char *restrict, ...);
extern int printf (const char *restrict, ...);
extern int fprintf_unlocked (FILE *restrict, const char *restrict, ...);
extern int printf_unlocked (const char *restrict, ...);
extern int sprintf (char *restrict, const char *restrict, ...);
extern int vfprintf (FILE *restrict, const char *restrict, va_list);
extern int vprintf (const char *restrict, va_list);
extern int vsprintf (char *restrict, const char *restrict, va_list);
extern int snprintf (char *restrict, size_t, const char *restrict, ...);
extern int vsnprintf (char *restrict, size_t, const char *restrict, va_list);

extern int fscanf (FILE *restrict, const char *restrict, ...);
extern int scanf (const char *restrict, ...);
extern int sscanf (const char *restrict, const char *restrict, ...);
extern int vfscanf (FILE *restrict, const char *restrict, va_list);
extern int vscanf (const char *restrict, va_list);
extern int vsscanf (const char *restrict, const char *restrict, va_list);

extern char *gettext (const char *);
extern char *dgettext (const char *, const char *);
extern char *dcgettext (const char *, const char *, int);

struct tm;

extern size_t strftime (char *restrict, size_t, const char *restrict,
   const struct tm *restrict);

extern ssize_t strfmon (char *restrict, size_t, const char *restrict, ...);
# 7 "./format/gcc_diag-1.c" 2





typedef long long __gcc_host_wide_int__;

typedef struct location_s
{
  const char *file;
  int line;
} location_t;

union tree_node;
typedef union tree_node *tree;

extern int diag (const char *, ...) __attribute__ ((__format__ (__gcc_diag__, 1, 2))) __attribute__ ((__nonnull__));;
extern int tdiag (const char *, ...) __attribute__ ((__format__ (__gcc_tdiag__, 1, 2))) __attribute__ ((__nonnull__));;
extern int cdiag (const char *, ...) __attribute__ ((__format__ (__gcc_cdiag__, 1, 2))) __attribute__ ((__nonnull__));;
extern int cxxdiag (const char *, ...) __attribute__ ((__format__ (__gcc_cxxdiag__, 1, 2))) __attribute__ ((__nonnull__));;
extern int dump (const char *, ...) __attribute__ ((__format__ (__gcc_dump_printf__, 1, 2))) __attribute__ ((__nonnull__));;

void
foo (int i, int i1, int i2, unsigned int u, double d, char *s, void *p,
     int *n, short int *hn, long int l, unsigned long int ul,
     long int *ln, long double ld, wint_t lc, wchar_t *ls, llong ll,
     ullong ull, unsigned int *un, const int *cn, signed char *ss,
     unsigned char *us, const signed char *css, unsigned int u1,
     unsigned int u2, location_t *loc, tree t1, union tree_node *t2,
     tree *t3, tree t4[], int *v, unsigned v_len, size_t sz, ptrdiff_t pd,
     ssize_t ssz, unsigned_ptrdiff_t upd)
{

  diag ("%%");
  tdiag ("%%");
  cdiag ("%%");
  cxxdiag ("%%");
  dump ("%%");
  diag ("%d%i%o%u%x%c%s%p%%", i, i, u, u, u, i, s, p);
  tdiag ("%d%i%o%u%x%c%s%p%%", i, i, u, u, u, i, s, p);
  cdiag ("%d%i%o%u%x%c%s%p%%", i, i, u, u, u, i, s, p);
  cxxdiag ("%d%i%o%u%x%c%s%p%%", i, i, u, u, u, i, s, p);
  dump ("%d%i%o%u%x%c%s%p%%", i, i, u, u, u, i, s, p);
  diag ("%qd%qi%qo%qu%qx%qc%qs%qp%<%%%'%>", i, i, u, u, u, i, s, p);
  tdiag ("%qd%qi%qo%qu%qx%qc%qs%qp%<%%%'%>", i, i, u, u, u, i, s, p);
  cdiag ("%qd%qi%qo%qu%qx%qc%qs%qp%<%%%'%>", i, i, u, u, u, i, s, p);
  cxxdiag ("%qd%qi%qo%qu%qx%qc%qs%qp%<%%%'%>", i, i, u, u, u, i, s, p);
  dump ("%qd%qi%qo%qu%qx%qc%qs%qp%<%%%'%>", i, i, u, u, u, i, s, p);
  diag ("%ld%li%lo%lu%lx", l, l, ul, ul, ul);
  tdiag ("%ld%li%lo%lu%lx", l, l, ul, ul, ul);
  cdiag ("%ld%li%lo%lu%lx", l, l, ul, ul, ul);
  cxxdiag ("%ld%li%lo%lu%lx", l, l, ul, ul, ul);
  dump ("%ld%li%lo%lu%lx", l, l, ul, ul, ul);
  diag ("%lld%lli%llo%llu%llx", ll, ll, ull, ull, ull);
  tdiag ("%lld%lli%llo%llu%llx", ll, ll, ull, ull, ull);
  cdiag ("%lld%lli%llo%llu%llx", ll, ll, ull, ull, ull);
  cxxdiag ("%lld%lli%llo%llu%llx", ll, ll, ull, ull, ull);
  dump ("%lld%lli%llo%llu%llx", ll, ll, ull, ull, ull);
  diag ("%wd%wi%wo%wu%wx", ll, ll, ull, ull, ull);
  tdiag ("%wd%wi%wo%wu%wx", ll, ll, ull, ull, ull);
  cdiag ("%wd%wi%wo%wu%wx", ll, ll, ull, ull, ull);
  cxxdiag ("%wd%wi%wo%wu%wx", ll, ll, ull, ull, ull);
  dump ("%wd%wi%wo%wu%wx", ll, ll, ull, ull, ull);
  diag ("%zd%zi%zo%zu%zx", ssz, ssz, sz, sz, sz);
  tdiag ("%zd%zi%zo%zu%zx", ssz, ssz, sz, sz, sz);
  cdiag ("%zd%zi%zo%zu%zx", ssz, ssz, sz, sz, sz);
  cxxdiag ("%zd%zi%zo%zu%zx", ssz, ssz, sz, sz, sz);
  dump ("%zd%zi%zo%zu%zx", ssz, ssz, sz, sz, sz);
  diag ("%td%ti%to%tu%tx", pd, pd, upd, upd, upd);
  tdiag ("%td%ti%to%tu%tx", pd, pd, upd, upd, upd);
  cdiag ("%td%ti%to%tu%tx", pd, pd, upd, upd, upd);
  cxxdiag ("%td%ti%to%tu%tx", pd, pd, upd, upd, upd);
  dump ("%td%ti%to%tu%tx", pd, pd, upd, upd, upd);
  diag ("%.*s", i, s);
  tdiag ("%.*s", i, s);
  cdiag ("%.*s", i, s);
  cxxdiag ("%.*s", i, s);
  dump ("%.*s", i, s);


  diag ("%m");
  tdiag ("%m");
  cdiag ("%m");
  cxxdiag ("%m");
  dump ("%m");


  tdiag ("%<%D%F%T%V%>", t1, t1, t1, t1);
  tdiag ("%<%+D%+F%+T%+V%>", t1, t1, t1, t1);
  tdiag ("%q+D%q+F%q+T%q+V", t1, t1, t1, t1);
  tdiag ("%<%D%D%D%D%>", t1, t2, *t3, t4[5]);
  cdiag ("%<%D%F%T%V%>", t1, t1, t1, t1);
  cdiag ("%<%+D%+F%+T%+V%>", t1, t1, t1, t1);
  cdiag ("%q+D%q+F%q+T%q+V", t1, t1, t1, t1);
  cdiag ("%<%D%D%D%D%>", t1, t2, *t3, t4[5]);
  cdiag ("%<%E%>", t1);
  cxxdiag ("%<%A%D%E%F%T%V%>", t1, t1, t1, t1, t1, t1);
  cxxdiag ("%<%D%D%D%D%>", t1, t2, *t3, t4[5]);
  cxxdiag ("%<%#A%#D%#E%#F%#T%#V%>", t1, t1, t1, t1, t1, t1);
  cxxdiag ("%<%+A%+D%+E%+F%+T%+V%>", t1, t1, t1, t1, t1, t1);
  cxxdiag ("%<%+#A%+#D%+#E%+#F%+#T%+#V%>", t1, t1, t1, t1, t1, t1);
  cxxdiag ("%C%L%O%P%Q", i, i, i, i, i);

  tdiag ("%v", i);
  cdiag ("%v%qv%#v", i, i, i);
  cxxdiag ("%v", i);

  tdiag ("%Z", v, v_len);
  cdiag ("%Z", v, v_len);
  cxxdiag ("%Z", v, v_len);
  dump ("%Z", v, v_len);


  diag ("%m", i);
  tdiag ("%m", i);
  cdiag ("%m", i);
  cxxdiag ("%m", i);
  dump ("%m", i);
  diag ("%#m");
  tdiag ("%#m");
  cdiag ("%#m");
  cxxdiag ("%#m");
  dump ("%#m");
  diag ("%+m");
  tdiag ("%+m");
  cdiag ("%+m");
  cxxdiag ("%+m");
  dump ("%+m");
  diag ("%D", t1);
  tdiag ("%A", t1);
  tdiag ("%E", t1);
  tdiag ("%#D", t1);
  cdiag ("%A", t1);
  cdiag ("%#D", t1);
  cdiag ("%<%+D%>", t1);
  cxxdiag ("%C");
  cxxdiag ("%C", l);
  cxxdiag ("%C", i, i);
  cxxdiag ("%#C", i);
  cxxdiag ("%+C", i);
  tdiag ("%D");
  cdiag ("%D");
  cxxdiag ("%D");
  tdiag ("%D", i);
  cdiag ("%D", i);
  cxxdiag ("%D", i);
  tdiag ("%D", t1, t1);
  cdiag ("%D", t1, t1);
  cxxdiag ("%D", t1, t1);

  tdiag ("%V", i);
  cdiag ("%V", i);
  cxxdiag ("%V", i);

  tdiag ("%v", t1);
  cdiag ("%v", t1);
  cxxdiag ("%v", t1);

  tdiag ("%Z");
  tdiag ("%Z", t1);


  diag ("%X\n", u);
  diag ("%f\n", d);
  diag ("%e\n", d);
  diag ("%E\n", d);
  diag ("%g\n", d);
  diag ("%G\n", d);
  diag ("%n\n", n);
  diag ("%hd\n", i);


  diag ("%-d", i);
  tdiag ("%-d", i);
  cdiag ("%-d", i);
  cxxdiag ("%-d", i);
  diag ("% d", i);
  tdiag ("% d", i);
  cdiag ("% d", i);
  cxxdiag ("% d", i);
  diag ("%#o", u);
  tdiag ("%#o", u);
  cdiag ("%#o", u);
  cxxdiag ("%#o", u);
  diag ("%0d", i);
  tdiag ("%0d", i);
  cdiag ("%0d", i);
  cxxdiag ("%0d", i);
  diag ("%08d", i);
  tdiag ("%08d", i);
  cdiag ("%08d", i);
  cxxdiag ("%08d", i);
  diag ("%+d\n", i);
  tdiag ("%+d\n", i);
  cdiag ("%+d\n", i);
  cxxdiag ("%+d\n", i);
  diag ("%3d\n", i);
  tdiag ("%3d\n", i);
  cdiag ("%3d\n", i);
  cxxdiag ("%3d\n", i);
  diag ("%-3d\n", i);
  tdiag ("%-3d\n", i);
  cdiag ("%-3d\n", i);
  cxxdiag ("%-3d\n", i);
  diag ("%.7d\n", i);
  tdiag ("%.7d\n", i);
  cdiag ("%.7d\n", i);
  cxxdiag ("%.7d\n", i);
  diag ("%+9.4d\n", i);
  tdiag ("%+9.4d\n", i);
  cdiag ("%+9.4d\n", i);
  cxxdiag ("%+9.4d\n", i);
  diag ("%.3ld\n", l);
  tdiag ("%.3ld\n", l);
  cdiag ("%.3ld\n", l);
  cxxdiag ("%.3ld\n", l);
  diag ("%d %lu\n", i, ul);
  diag ("%d", l);
  diag ("%wd", l);
  diag ("%d", ll);
  diag ("%*s", i, s);
  diag ("%*.*s", i, i, s);
  diag ("%*d\n", i1, i);
  diag ("%.*d\n", i2, i);
  diag ("%*.*ld\n", i1, i2, l);
  diag ("%ld", i);
  diag ("%s", n);


  diag ("%d%d", i);
  diag ("%d", i, i);

  diag ("");
  diag ("\0");
  diag ("%d\0", i);
  diag ("%d\0%d", i, i);
  diag (
# 243 "./format/gcc_diag-1.c" 3 4
       ((void *)0)
# 243 "./format/gcc_diag-1.c"
           );
  diag ("%");
  diag ((const char *)L"foo");
  diag ("%s", (char *)0);


  printf ("%d\n", ll);
}
