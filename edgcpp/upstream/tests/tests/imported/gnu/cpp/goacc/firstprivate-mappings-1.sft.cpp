//type: fp
//options: 
# 0 "./goacc/firstprivate-mappings-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./goacc/firstprivate-mappings-1.C"
# 15 "./goacc/firstprivate-mappings-1.C"
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdbool.h" 1 3 4
# 16 "./goacc/firstprivate-mappings-1.C" 2
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdint.h" 1 3 4
# 9 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdint.h" 3 4
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/usr/include/stdint.h" 1 3 4
# 25 "/usr/include/stdint.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 26 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wchar.h" 1 3 4
# 22 "/usr/include/bits/wchar.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/wchar.h" 2 3 4
# 27 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/stdint.h" 2 3 4
# 36 "/usr/include/stdint.h" 3 4
typedef signed char int8_t;
typedef short int int16_t;
typedef int int32_t;

typedef long int int64_t;







typedef unsigned char uint8_t;
typedef unsigned short int uint16_t;

typedef unsigned int uint32_t;



typedef unsigned long int uint64_t;
# 65 "/usr/include/stdint.h" 3 4
typedef signed char int_least8_t;
typedef short int int_least16_t;
typedef int int_least32_t;

typedef long int int_least64_t;






typedef unsigned char uint_least8_t;
typedef unsigned short int uint_least16_t;
typedef unsigned int uint_least32_t;

typedef unsigned long int uint_least64_t;
# 90 "/usr/include/stdint.h" 3 4
typedef signed char int_fast8_t;

typedef long int int_fast16_t;
typedef long int int_fast32_t;
typedef long int int_fast64_t;
# 103 "/usr/include/stdint.h" 3 4
typedef unsigned char uint_fast8_t;

typedef unsigned long int uint_fast16_t;
typedef unsigned long int uint_fast32_t;
typedef unsigned long int uint_fast64_t;
# 119 "/usr/include/stdint.h" 3 4
typedef long int intptr_t;


typedef unsigned long int uintptr_t;
# 134 "/usr/include/stdint.h" 3 4
typedef long int intmax_t;
typedef unsigned long int uintmax_t;
# 12 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdint.h" 2 3 4
#pragma GCC diagnostic pop
# 17 "./goacc/firstprivate-mappings-1.C" 2
# 1 "/usr/include/string.h" 1 3 4
# 27 "/usr/include/string.h" 3 4
extern "C" {




# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 33 "/usr/include/string.h" 2 3 4









extern void *memcpy (void *__restrict __dest, const void *__restrict __src,
       size_t __n) throw () __attribute__ ((__nonnull__ (1, 2)));


extern void *memmove (void *__dest, const void *__src, size_t __n)
     throw () __attribute__ ((__nonnull__ (1, 2)));






extern void *memccpy (void *__restrict __dest, const void *__restrict __src,
        int __c, size_t __n)
     throw () __attribute__ ((__nonnull__ (1, 2)));





extern void *memset (void *__s, int __c, size_t __n) throw () __attribute__ ((__nonnull__ (1)));


extern int memcmp (const void *__s1, const void *__s2, size_t __n)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));



extern "C++"
{
extern void *memchr (void *__s, int __c, size_t __n)
      throw () __asm ("memchr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
extern const void *memchr (const void *__s, int __c, size_t __n)
      throw () __asm ("memchr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
# 90 "/usr/include/string.h" 3 4
}










extern "C++" void *rawmemchr (void *__s, int __c)
     throw () __asm ("rawmemchr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
extern "C++" const void *rawmemchr (const void *__s, int __c)
     throw () __asm ("rawmemchr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));







extern "C++" void *memrchr (void *__s, int __c, size_t __n)
      throw () __asm ("memrchr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
extern "C++" const void *memrchr (const void *__s, int __c, size_t __n)
      throw () __asm ("memrchr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));









extern char *strcpy (char *__restrict __dest, const char *__restrict __src)
     throw () __attribute__ ((__nonnull__ (1, 2)));

extern char *strncpy (char *__restrict __dest,
        const char *__restrict __src, size_t __n)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern char *strcat (char *__restrict __dest, const char *__restrict __src)
     throw () __attribute__ ((__nonnull__ (1, 2)));

extern char *strncat (char *__restrict __dest, const char *__restrict __src,
        size_t __n) throw () __attribute__ ((__nonnull__ (1, 2)));


extern int strcmp (const char *__s1, const char *__s2)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));

extern int strncmp (const char *__s1, const char *__s2, size_t __n)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));


extern int strcoll (const char *__s1, const char *__s2)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));

extern size_t strxfrm (char *__restrict __dest,
         const char *__restrict __src, size_t __n)
     throw () __attribute__ ((__nonnull__ (2)));






# 1 "/usr/include/xlocale.h" 1 3 4
# 27 "/usr/include/xlocale.h" 3 4
typedef struct __locale_struct
{

  struct __locale_data *__locales[13];


  const unsigned short int *__ctype_b;
  const int *__ctype_tolower;
  const int *__ctype_toupper;


  const char *__names[13];
} *__locale_t;


typedef __locale_t locale_t;
# 160 "/usr/include/string.h" 2 3 4


extern int strcoll_l (const char *__s1, const char *__s2, __locale_t __l)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2, 3)));

extern size_t strxfrm_l (char *__dest, const char *__src, size_t __n,
    __locale_t __l) throw () __attribute__ ((__nonnull__ (2, 4)));





extern char *strdup (const char *__s)
     throw () __attribute__ ((__malloc__)) __attribute__ ((__nonnull__ (1)));






extern char *strndup (const char *__string, size_t __n)
     throw () __attribute__ ((__malloc__)) __attribute__ ((__nonnull__ (1)));
# 207 "/usr/include/string.h" 3 4



extern "C++"
{
extern char *strchr (char *__s, int __c)
     throw () __asm ("strchr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
extern const char *strchr (const char *__s, int __c)
     throw () __asm ("strchr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
# 230 "/usr/include/string.h" 3 4
}






extern "C++"
{
extern char *strrchr (char *__s, int __c)
     throw () __asm ("strrchr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
extern const char *strrchr (const char *__s, int __c)
     throw () __asm ("strrchr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
# 257 "/usr/include/string.h" 3 4
}










extern "C++" char *strchrnul (char *__s, int __c)
     throw () __asm ("strchrnul") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
extern "C++" const char *strchrnul (const char *__s, int __c)
     throw () __asm ("strchrnul") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));









extern size_t strcspn (const char *__s, const char *__reject)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));


extern size_t strspn (const char *__s, const char *__accept)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));


extern "C++"
{
extern char *strpbrk (char *__s, const char *__accept)
     throw () __asm ("strpbrk") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));
extern const char *strpbrk (const char *__s, const char *__accept)
     throw () __asm ("strpbrk") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));
# 309 "/usr/include/string.h" 3 4
}






extern "C++"
{
extern char *strstr (char *__haystack, const char *__needle)
     throw () __asm ("strstr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));
extern const char *strstr (const char *__haystack, const char *__needle)
     throw () __asm ("strstr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));
# 336 "/usr/include/string.h" 3 4
}







extern char *strtok (char *__restrict __s, const char *__restrict __delim)
     throw () __attribute__ ((__nonnull__ (2)));




extern char *__strtok_r (char *__restrict __s,
    const char *__restrict __delim,
    char **__restrict __save_ptr)
     throw () __attribute__ ((__nonnull__ (2, 3)));

extern char *strtok_r (char *__restrict __s, const char *__restrict __delim,
         char **__restrict __save_ptr)
     throw () __attribute__ ((__nonnull__ (2, 3)));





extern "C++" char *strcasestr (char *__haystack, const char *__needle)
     throw () __asm ("strcasestr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));
extern "C++" const char *strcasestr (const char *__haystack,
         const char *__needle)
     throw () __asm ("strcasestr") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));
# 378 "/usr/include/string.h" 3 4
extern void *memmem (const void *__haystack, size_t __haystacklen,
       const void *__needle, size_t __needlelen)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 3)));



extern void *__mempcpy (void *__restrict __dest,
   const void *__restrict __src, size_t __n)
     throw () __attribute__ ((__nonnull__ (1, 2)));
extern void *mempcpy (void *__restrict __dest,
        const void *__restrict __src, size_t __n)
     throw () __attribute__ ((__nonnull__ (1, 2)));





extern size_t strlen (const char *__s)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));





extern size_t strnlen (const char *__string, size_t __maxlen)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));





extern char *strerror (int __errnum) throw ();

# 434 "/usr/include/string.h" 3 4
extern char *strerror_r (int __errnum, char *__buf, size_t __buflen)
     throw () __attribute__ ((__nonnull__ (2))) ;





extern char *strerror_l (int __errnum, __locale_t __l) throw ();





extern void __bzero (void *__s, size_t __n) throw () __attribute__ ((__nonnull__ (1)));



extern void bcopy (const void *__src, void *__dest, size_t __n)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern void bzero (void *__s, size_t __n) throw () __attribute__ ((__nonnull__ (1)));


extern int bcmp (const void *__s1, const void *__s2, size_t __n)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));



extern "C++"
{
extern char *index (char *__s, int __c)
     throw () __asm ("index") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
extern const char *index (const char *__s, int __c)
     throw () __asm ("index") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
# 483 "/usr/include/string.h" 3 4
}







extern "C++"
{
extern char *rindex (char *__s, int __c)
     throw () __asm ("rindex") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
extern const char *rindex (const char *__s, int __c)
     throw () __asm ("rindex") __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
# 511 "/usr/include/string.h" 3 4
}







extern int ffs (int __i) throw () __attribute__ ((__const__));




extern int ffsl (long int __l) throw () __attribute__ ((__const__));

__extension__ extern int ffsll (long long int __ll)
     throw () __attribute__ ((__const__));




extern int strcasecmp (const char *__s1, const char *__s2)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));


extern int strncasecmp (const char *__s1, const char *__s2, size_t __n)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));





extern int strcasecmp_l (const char *__s1, const char *__s2,
    __locale_t __loc)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2, 3)));

extern int strncasecmp_l (const char *__s1, const char *__s2,
     size_t __n, __locale_t __loc)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2, 4)));





extern char *strsep (char **__restrict __stringp,
       const char *__restrict __delim)
     throw () __attribute__ ((__nonnull__ (1, 2)));




extern char *strsignal (int __sig) throw ();


extern char *__stpcpy (char *__restrict __dest, const char *__restrict __src)
     throw () __attribute__ ((__nonnull__ (1, 2)));
extern char *stpcpy (char *__restrict __dest, const char *__restrict __src)
     throw () __attribute__ ((__nonnull__ (1, 2)));



extern char *__stpncpy (char *__restrict __dest,
   const char *__restrict __src, size_t __n)
     throw () __attribute__ ((__nonnull__ (1, 2)));
extern char *stpncpy (char *__restrict __dest,
        const char *__restrict __src, size_t __n)
     throw () __attribute__ ((__nonnull__ (1, 2)));




extern int strverscmp (const char *__s1, const char *__s2)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));


extern char *strfry (char *__string) throw () __attribute__ ((__nonnull__ (1)));


extern void *memfrob (void *__s, size_t __n) throw () __attribute__ ((__nonnull__ (1)));







extern "C++" char *basename (char *__filename)
     throw () __asm ("basename") __attribute__ ((__nonnull__ (1)));
extern "C++" const char *basename (const char *__filename)
     throw () __asm ("basename") __attribute__ ((__nonnull__ (1)));
# 642 "/usr/include/string.h" 3 4
}
# 18 "./goacc/firstprivate-mappings-1.C" 2
# 33 "./goacc/firstprivate-mappings-1.C"

# 33 "./goacc/firstprivate-mappings-1.C"
extern "C" {
# 42 "./goacc/firstprivate-mappings-1.C"
static void
p (short *&spi)
{
  short *spo;
#pragma acc parallel copyout (spo) firstprivate (spi)


  {
    spo = ++spi;
  }
  if (spo != spi + 1)
    __builtin_abort ();
}


static void
b (bool &bi)
{
  bool bo;
#pragma acc parallel copyout (bo) firstprivate (bi)


  {
    bo = (bi = !bi);
  }
  if (bo != !bi)
    __builtin_abort ();
}


static void
i (int8_t &i8i,
   uint8_t &u8i,
   int16_t &i16i,
   uint16_t &u16i,
   int32_t &i32i,
   uint32_t &u32i,
   int64_t &i64i,
   uint64_t &u64i)
{
  int8_t i8o;
  uint8_t u8o;
  int16_t i16o;
  uint16_t u16o;
  int32_t i32o;
  uint32_t u32o;
  int64_t i64o;
  uint64_t u64o;
#pragma acc parallel copyout (i8o) firstprivate (i8i) copyout (u8o) firstprivate (u8i) copyout (i16o) firstprivate (i16i) copyout (u16o) firstprivate (u16i) copyout (i32o) firstprivate (i32i) copyout (u32o) firstprivate (u32i) copyout (i64o) firstprivate (i64i) copyout (u64o) firstprivate (u64i)
# 107 "./goacc/firstprivate-mappings-1.C"
  {
    i8o = --i8i;
    u8o = ++u8i;
    i16o = --i16i;
    u16o = ++u16i;
    i32o = --i32i;
    u32o = ++u32i;
    i64o = --i64i;
    u64o = ++u64i;
  }
  if (i8o != i8i - 1)
    __builtin_abort ();
  if (u8o != u8i + 1)
    __builtin_abort ();
  if (i16o != i16i - 1)
    __builtin_abort ();
  if (u16o != u16i + 1)
    __builtin_abort ();
  if (i32o != i32i - 1)
    __builtin_abort ();
  if (u32o != u32i + 1)
    __builtin_abort ();
  if (i64o != i64i - 1)
    __builtin_abort ();
  if (u64o != u64i + 1)
    __builtin_abort ();
}



static void
i128 (__int128 &i128i, unsigned __int128 &u128i)
{
  __int128 i128o;
  unsigned __int128 u128o;
#pragma acc parallel copyout (i128o) firstprivate (i128i) copyout(u128o) firstprivate (u128i)




  {
    i128o = --i128i;
    u128o = ++u128i;
  }
  if (i128o != i128i - 1)
    __builtin_abort ();
  if (u128o != u128i + 1)
    __builtin_abort ();
}



static void
flt_dbl (float &flti, double &dbli)
{
  float flto;
  double dblo;
#pragma acc parallel copyout (flto) firstprivate (flti) copyout (dblo) firstprivate (dbli)




  {
    flto = --flti;
    dblo = --dbli;
  }
  if (flto != flti - 1)
    __builtin_abort ();
  if (dblo != dbli - 1)
    __builtin_abort ();
}


static void
ldbl (long double &ldbli)
{

  long double ldblo;
#pragma acc parallel copyout (ldblo) firstprivate (ldbli)


  {
    ldblo = --ldbli;
  }
  if (ldblo != ldbli - 1)
    __builtin_abort ();

}


static void
c (_Complex unsigned char &cuci,
   _Complex signed short &cssi,
   _Complex unsigned int &cuii,
   _Complex signed long &csli,
   _Complex float &cflti,
   _Complex double &cdbli)
{
  _Complex unsigned char cuco;
  _Complex signed short csso;
  _Complex unsigned int cuio;
  _Complex signed long cslo;
  _Complex float cflto;
  _Complex double cdblo;
#pragma acc parallel copyout (cuco) firstprivate (cuci) copyout (csso) firstprivate (cssi) copyout (cuio) firstprivate (cuii) copyout (cslo) firstprivate (csli) copyout (cflto) firstprivate (cflti) copyout (cdblo) firstprivate (cdbli)
# 224 "./goacc/firstprivate-mappings-1.C"
  {
    cuco = (cuci += (1 + 1j));
    csso = (cssi -= (1 + 1j));
    cuio = (cuii += (1 + 1j));
    cslo = (csli -= (1 + 1j));
    cflto = (cflti -= (1 + 1j));
    cdblo = (cdbli -= (1 + 1j));
  }
  if (cuco != cuci + (1 + 1j))
    __builtin_abort ();
  if (csso != cssi - (1 + 1j))
    __builtin_abort ();
  if (cuio != cuii + (1 + 1j))
    __builtin_abort ();
  if (cslo != csli - (1 + 1j))
    __builtin_abort ();
  if (cflto != cflti - (1 + 1j))
    __builtin_abort ();
  if (cdblo != cdbli - (1 + 1j))
    __builtin_abort ();
}


static void
cldbl (_Complex long double &cldbli)
{

  _Complex long double cldblo;
#pragma acc parallel copyout (cldblo) firstprivate (cldbli)


  {
    cldblo = (cldbli -= (1 + 1j));
  }
  if (cldblo != cldbli - (1 + 1j))
    __builtin_abort ();

}
# 271 "./goacc/firstprivate-mappings-1.C"
typedef uint8_t __attribute__ ((vector_size (2 * sizeof (uint8_t)))) v2u8;
typedef int16_t __attribute__ ((vector_size (4 * sizeof (int16_t)))) v4i16;
typedef uint32_t __attribute__ ((vector_size (8 * sizeof (uint32_t)))) v8u32;
typedef int64_t __attribute__ ((vector_size (16 * sizeof (int64_t)))) v16i64;
typedef float __attribute__ ((vector_size (1 * sizeof (float)))) v1flt;
typedef float __attribute__ ((vector_size (2 * sizeof (float)))) v2flt;
typedef float __attribute__ ((vector_size (4 * sizeof (float)))) v4flt;
typedef float __attribute__ ((vector_size (8 * sizeof (float)))) v8flt;
typedef double __attribute__ ((vector_size (1 * sizeof (double)))) v1dbl;
typedef double __attribute__ ((vector_size (2 * sizeof (double)))) v2dbl;
typedef double __attribute__ ((vector_size (4 * sizeof (double)))) v4dbl;
typedef double __attribute__ ((vector_size (8 * sizeof (double)))) v8dbl;

static void
v (v2u8 &v2u8i, v4i16 &v4i16i, v8u32 &v8u32i, v16i64 &v16i64i,
   v1flt &v1flti, v2flt &v2flti, v4flt &v4flti, v8flt &v8flti,
   v1dbl &v1dbli, v2dbl &v2dbli, v4dbl &v4dbli, v8dbl &v8dbli)
{
  v2u8 v2u8o;
  v4i16 v4i16o;
  v8u32 v8u32o;
  v16i64 v16i64o;
  v1flt v1flto;
  v2flt v2flto;
  v4flt v4flto;
  v8flt v8flto;
  v1dbl v1dblo;
  v2dbl v2dblo;
  v4dbl v4dblo;
  v8dbl v8dblo;
#pragma acc parallel copyout (v2u8o) firstprivate (v2u8i) copyout (v4i16o) firstprivate (v4i16i) copyout (v8u32o) firstprivate (v8u32i) copyout (v16i64o) firstprivate (v16i64i) copyout (v1flto) firstprivate (v1flti) copyout (v2flto) firstprivate (v2flti) copyout (v4flto) firstprivate (v4flti) copyout (v8flto) firstprivate (v8flti) copyout (v1dblo) firstprivate (v1dbli) copyout (v2dblo) firstprivate (v2dbli) copyout (v4dblo) firstprivate (v4dbli) copyout (v8dblo) firstprivate (v8dbli)
# 326 "./goacc/firstprivate-mappings-1.C"
  {
    v2u8o = ++v2u8i;
    v4i16o = --v4i16i;
    v8u32o = ++v8u32i;
    v16i64o = --v16i64i;
    v1flto = --v1flti;
    v2flto = --v2flti;
    v4flto = --v4flti;
    v8flto = --v8flti;
    v1dblo = --v1dbli;
    v2dblo = --v2dbli;
    v4dblo = --v4dbli;
    v8dblo = --v8dbli;
  }
  if (!({ __typeof__ (v2u8o) v_d = (v2u8o) != (v2u8i + 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v4i16o) v_d = (v4i16o) != (v4i16i - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v8u32o) v_d = (v8u32o) != (v8u32i + 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v16i64o) v_d = (v16i64o) != (v16i64i - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v1flto) v_d = (v1flto) != (v1flti - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v2flto) v_d = (v2flto) != (v2flti - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v4flto) v_d = (v4flto) != (v4flti - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v8flto) v_d = (v8flto) != (v8flti - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v1dblo) v_d = (v1dblo) != (v1dbli - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v2dblo) v_d = (v2dblo) != (v2dbli - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v4dblo) v_d = (v4dblo) != (v4dbli - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v8dblo) v_d = (v8dblo) != (v8dbli - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
}




typedef long double __attribute__ ((vector_size (1 * sizeof (long double)))) v1ldbl;
typedef long double __attribute__ ((vector_size (2 * sizeof (long double)))) v2ldbl;
typedef long double __attribute__ ((vector_size (4 * sizeof (long double)))) v4ldbl;
typedef long double __attribute__ ((vector_size (8 * sizeof (long double)))) v8ldbl;

static void
vldbl (v1ldbl &v1ldbli, v2ldbl &v2ldbli, v4ldbl &v4ldbli, v8ldbl &v8ldbli)
{

  v1ldbl v1ldblo;
  v2ldbl v2ldblo;
  v4ldbl v4ldblo;
  v8ldbl v8ldblo;
#pragma acc parallel copyout (v1ldblo) firstprivate (v1ldbli) copyout (v2ldblo) firstprivate (v2ldbli) copyout (v4ldblo) firstprivate (v4ldbli) copyout (v8ldblo) firstprivate (v8ldbli)
# 391 "./goacc/firstprivate-mappings-1.C"
  {
    v1ldblo = --v1ldbli;
    v2ldblo = --v2ldbli;
    v4ldblo = --v4ldbli;
    v8ldblo = --v8ldbli;
  }
  if (!({ __typeof__ (v1ldblo) v_d = (v1ldblo) != (v1ldbli - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v2ldblo) v_d = (v2ldblo) != (v2ldbli - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v4ldblo) v_d = (v4ldblo) != (v4ldbli - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();
  if (!({ __typeof__ (v8ldblo) v_d = (v8ldblo) != (v8ldbli - 1); __typeof__ (v_d) v_0 = { 0 }; memcmp (&v_d, &v_0, sizeof v_d) == 0; }))
    __builtin_abort ();

}



static void
vla (int &array_li)
{
  _Complex double array[array_li];
  uint32_t array_so;
#pragma acc parallel copyout (array_so)





  {
    array_so = sizeof array;
  }
  if (array_so != sizeof array)
    __builtin_abort ();
}



}



int
main (int argc, char *argv[])
{
  {
    short s;
    short *sp = &s;
    p (sp);
  }

  {
    bool bi = true;
    b (bi);
  }

  {
    int8_t i8i = -1;
    uint8_t u8i = 1;
    int16_t i16i = -2;
    uint16_t u16i = 2;
    int32_t i32i = -3;
    uint32_t u32i = 3;
    int64_t i64i = -4;
    uint64_t u64i = 4;
    i (i8i, u8i, i16i, u16i, i32i, u32i, i64i, u64i);
  }


  {
    __int128 i128i = -8;
    unsigned __int128 u128i = 8;
    i128 (i128i, u128i);
  }


  {
    float flti = .5;
    double dbli = .25;
    flt_dbl (flti, dbli);
  }

  {
    long double ldbli = .125;
    ldbl (ldbli);
  }

  {
    _Complex unsigned char cuci = 1 + 2j;
    _Complex signed short cssi = -2 + (-4j);
    _Complex unsigned int cuii = 3 + 6j;
    _Complex signed long csli = -4 + (-8j);
    _Complex float cflti = .5 + 1j;
    _Complex double cdbli = .25 + .5j;
    c (cuci, cssi, cuii, csli, cflti, cdbli);
  }

  {
    _Complex long double cldbli = .125 + .25j;
    cldbl (cldbli);
  }

  {
    v2u8 v2u8i = {2, 3};
    v4i16 v4i16i = { -1, -2, 5, 4 };
    v8u32 v8u32i = { 3, 6, 9, 11};
    v16i64 v16i64i = { 10, 21, -25, 44, 31, -1, 1, 222, -1, -12, 52, -44, -13, 1, -1, -222};
    v1flt v1flti = { -.5 };
    v2flt v2flti = { 1.5, -2.5 };
    v4flt v4flti = { 3.5, -4.5, -5.5, -6.5 };
    v8flt v8flti = { -7.5, 8.5, 9.5, 10.5, -11.5, -12.5, 13.5, 14.5 };
    v1dbl v1dbli = { 0.25 };
    v2dbl v2dbli = { -1.25, -2.25 };
    v4dbl v4dbli = { 3.25, -4.25, 5.25, 6.25 };
    v8dbl v8dbli = { 7.25, 8.25, -9.25, -10.25, -11.25, 12.25, 13.25, -14.25 };
    v (v2u8i, v4i16i, v8u32i, v16i64i,
       v1flti, v2flti, v4flti, v8flti,
       v1dbli, v2dbli, v4dbli, v8dbli);
  }


  {
    v1ldbl v1ldbli = { -0.125 };
    v2ldbl v2ldbli = { 1.125, -2.125 };
    v4ldbl v4ldbli = { -3.125, -4.125, 5.125, -6.125 };
    v8ldbl v8ldbli = { 7.125, -8.125, -9.125, 10.125, 11.125, 12.125, 13.125, 14.125 };
    vldbl (v1ldbli, v2ldbli, v4ldbli, v8ldbli);
  }


  vla (argc);

  return 0;
}
