/* GCC parser bindings for DotCC builtin names. No observed CPython field
 * uses these by value; opaque declarations only. No replacement layouts or
 * service implementations. Actual target headers supply every measured type. */
typedef unsigned short wchar_t;
typedef struct dotcc_FILE FILE;
typedef struct dotcc_VaList VaList;
typedef struct dotcc_div_t div_t;
typedef struct dotcc_ldiv_t ldiv_t;
typedef struct dotcc_lldiv_t lldiv_t;
typedef struct dotcc_imaxdiv_t imaxdiv_t;
extern int errno;
#define offsetof(t,m) __builtin_offsetof(t,m)
