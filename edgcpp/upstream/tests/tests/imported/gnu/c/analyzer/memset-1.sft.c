//type: fp
//options: 
# 0 "./analyzer/memset-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/memset-1.c"
# 1 "/usr/include/string.h" 1 3 4
# 25 "/usr/include/string.h" 3 4
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
# 26 "/usr/include/string.h" 2 3 4






# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 1 3 4
# 214 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4

# 214 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 33 "/usr/include/string.h" 2 3 4









extern void *memcpy (void *__restrict __dest, const void *__restrict __src,
       size_t __n) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern void *memmove (void *__dest, const void *__src, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));






extern void *memccpy (void *__restrict __dest, const void *__restrict __src,
        int __c, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));





extern void *memset (void *__s, int __c, size_t __n) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int memcmp (const void *__s1, const void *__s2, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));
# 92 "/usr/include/string.h" 3 4
extern void *memchr (const void *__s, int __c, size_t __n)
      __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));


# 123 "/usr/include/string.h" 3 4


extern char *strcpy (char *__restrict __dest, const char *__restrict __src)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));

extern char *strncpy (char *__restrict __dest,
        const char *__restrict __src, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern char *strcat (char *__restrict __dest, const char *__restrict __src)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));

extern char *strncat (char *__restrict __dest, const char *__restrict __src,
        size_t __n) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int strcmp (const char *__s1, const char *__s2)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));

extern int strncmp (const char *__s1, const char *__s2, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));


extern int strcoll (const char *__s1, const char *__s2)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));

extern size_t strxfrm (char *__restrict __dest,
         const char *__restrict __src, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (2)));






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
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2, 3)));

extern size_t strxfrm_l (char *__dest, const char *__src, size_t __n,
    __locale_t __l) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (2, 4)));





extern char *strdup (const char *__s)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__malloc__)) __attribute__ ((__nonnull__ (1)));






extern char *strndup (const char *__string, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__malloc__)) __attribute__ ((__nonnull__ (1)));
# 207 "/usr/include/string.h" 3 4

# 232 "/usr/include/string.h" 3 4
extern char *strchr (const char *__s, int __c)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
# 259 "/usr/include/string.h" 3 4
extern char *strrchr (const char *__s, int __c)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));


# 278 "/usr/include/string.h" 3 4



extern size_t strcspn (const char *__s, const char *__reject)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));


extern size_t strspn (const char *__s, const char *__accept)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));
# 311 "/usr/include/string.h" 3 4
extern char *strpbrk (const char *__s, const char *__accept)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));
# 338 "/usr/include/string.h" 3 4
extern char *strstr (const char *__haystack, const char *__needle)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));




extern char *strtok (char *__restrict __s, const char *__restrict __delim)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (2)));




extern char *__strtok_r (char *__restrict __s,
    const char *__restrict __delim,
    char **__restrict __save_ptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (2, 3)));

extern char *strtok_r (char *__restrict __s, const char *__restrict __delim,
         char **__restrict __save_ptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (2, 3)));
# 393 "/usr/include/string.h" 3 4


extern size_t strlen (const char *__s)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));





extern size_t strnlen (const char *__string, size_t __maxlen)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));





extern char *strerror (int __errnum) __attribute__ ((__nothrow__ , __leaf__));

# 423 "/usr/include/string.h" 3 4
extern int strerror_r (int __errnum, char *__buf, size_t __buflen) __asm__ ("" "__xpg_strerror_r") __attribute__ ((__nothrow__ , __leaf__))

                        __attribute__ ((__nonnull__ (2)));
# 441 "/usr/include/string.h" 3 4
extern char *strerror_l (int __errnum, __locale_t __l) __attribute__ ((__nothrow__ , __leaf__));





extern void __bzero (void *__s, size_t __n) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));



extern void bcopy (const void *__src, void *__dest, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern void bzero (void *__s, size_t __n) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int bcmp (const void *__s1, const void *__s2, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));
# 485 "/usr/include/string.h" 3 4
extern char *index (const char *__s, int __c)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));
# 513 "/usr/include/string.h" 3 4
extern char *rindex (const char *__s, int __c)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1)));




extern int ffs (int __i) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__));
# 532 "/usr/include/string.h" 3 4
extern int strcasecmp (const char *__s1, const char *__s2)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));


extern int strncasecmp (const char *__s1, const char *__s2, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1, 2)));
# 555 "/usr/include/string.h" 3 4
extern char *strsep (char **__restrict __stringp,
       const char *__restrict __delim)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));




extern char *strsignal (int __sig) __attribute__ ((__nothrow__ , __leaf__));


extern char *__stpcpy (char *__restrict __dest, const char *__restrict __src)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));
extern char *stpcpy (char *__restrict __dest, const char *__restrict __src)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));



extern char *__stpncpy (char *__restrict __dest,
   const char *__restrict __src, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));
extern char *stpncpy (char *__restrict __dest,
        const char *__restrict __src, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));
# 642 "/usr/include/string.h" 3 4

# 2 "./analyzer/memset-1.c" 2
# 1 "./analyzer/analyzer-decls.h" 1








# 8 "./analyzer/analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 32 "./analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);
# 3 "./analyzer/memset-1.c" 2



void test_1 (void)
{
  char buf[256];
  memset (buf, 0, 256);
  __analyzer_eval (buf[42] == 0);
}



void test_1a (void)
{
  char buf[256];
  __builtin_memset (buf, 0, 256);
  __analyzer_eval (buf[42] == 0);
}



void test_2 (void)
{
  char buf[256];
  buf[42] = 'A';
  __analyzer_eval (buf[42] == 'A');
  memset (buf, 0, 256);
  __analyzer_eval (buf[42] == '\0');
}



void test_3 (int val)
{
  char buf[256];
  memset (buf, 'A', 256);
  __analyzer_eval (buf[42] == 'A');
}



void test_4 (char val)
{
  char buf[256];
  memset (buf, val, 256);
  __analyzer_eval (buf[42] == (char)val);
}



void test_5 (int n)
{
  char buf[256];
  buf[42] = 'A';
  __analyzer_eval (buf[42] == 'A');
  memset (buf, 0, n);


  __analyzer_eval (buf[42] == 'A');
  __analyzer_eval (buf[42] == '\0');
}



void test_5a (int n)
{
  char buf[256];
  buf[42] = 'A';
  __analyzer_eval (buf[42] == 'A');
  __builtin___memset_chk (buf, 0, n, __builtin_object_size (buf, 0));


  __analyzer_eval (buf[42] == 'A');
  __analyzer_eval (buf[42] == '\0');
}



static size_t __attribute__((noinline))
get_zero (void)
{
  return 0;
}

void test_6 (int val)
{
  char buf[256];
  buf[42] = 'A';
  memset (buf, 'B', get_zero ());
  __analyzer_eval (buf[42] == 'A');
}

void test_6b (int val)
{
  char buf[256];
  memset (buf, 'A', sizeof (buf));
  memset (buf, 'B', get_zero ());
  __analyzer_eval (buf[42] == 'A');
}



void test_7 (void)
{
  char buf[256];
  buf[128] = 'A';
  memset (buf, 0, 128);
  __analyzer_eval (buf[0] == '\0');
  __analyzer_eval (buf[127] == '\0');
  __analyzer_eval (buf[128] == 'A');
}

void test_8 (void)
{
  char buf[20];
  memset (buf + 0, 0, 1);
  memset (buf + 1, 1, 1);
  memset (buf + 2, 2, 1);
  memset (buf + 3, 3, 1);
  memset (buf + 4, 4, 2);
  memset (buf + 6, 6, 2);
  memset (buf + 8, 8, 4);
  memset (buf + 12, 12, 8);
  __analyzer_eval (buf[0] == 0);
  __analyzer_eval (buf[1] == 1);
  __analyzer_eval (buf[2] == 2);
  __analyzer_eval (buf[3] == 3);
  __analyzer_eval (buf[4] == 4);
  __analyzer_eval (buf[5] == 4);
  __analyzer_eval (buf[6] == 6);
  __analyzer_eval (buf[7] == 6);
  __analyzer_eval (buf[8] == 8);
  __analyzer_eval (buf[9] == 8);
  __analyzer_eval (buf[10] == 8);
  __analyzer_eval (buf[11] == 8);
  __analyzer_eval (buf[12] == 12);
  __analyzer_eval (buf[13] == 12);
  __analyzer_eval (buf[14] == 12);
  __analyzer_eval (buf[15] == 12);
  __analyzer_eval (buf[16] == 12);
  __analyzer_eval (buf[17] == 12);
  __analyzer_eval (buf[18] == 12);
  __analyzer_eval (buf[19] == 12);
}



void test_9 (void)
{
  char buf[8];
  memset (buf, 0, 8);
  __analyzer_eval (buf[0] == 0);
  __analyzer_eval (buf[1] == 0);
  __analyzer_eval (buf[2] == 0);
  __analyzer_eval (buf[3] == 0);
  __analyzer_eval (buf[4] == 0);
  __analyzer_eval (buf[5] == 0);
  __analyzer_eval (buf[6] == 0);
  __analyzer_eval (buf[7] == 0);

  memset (buf + 1, 1, 4);
  __analyzer_eval (buf[0] == 0);
  __analyzer_eval (buf[1] == 1);
  __analyzer_eval (buf[2] == 1);
  __analyzer_eval (buf[3] == 1);
  __analyzer_eval (buf[4] == 1);
  __analyzer_eval (buf[5] == 0);
  __analyzer_eval (buf[6] == 0);
  __analyzer_eval (buf[7] == 0);

  memset (buf + 2, 2, 4);
  __analyzer_eval (buf[0] == 0);
  __analyzer_eval (buf[1] == 1);
  __analyzer_eval (buf[2] == 2);
  __analyzer_eval (buf[3] == 2);
  __analyzer_eval (buf[4] == 2);
  __analyzer_eval (buf[5] == 2);
  __analyzer_eval (buf[6] == 0);
  __analyzer_eval (buf[7] == 0);

  memset (buf + 4, 3, 3);
  __analyzer_eval (buf[0] == 0);
  __analyzer_eval (buf[1] == 1);
  __analyzer_eval (buf[2] == 2);
  __analyzer_eval (buf[3] == 2);
  __analyzer_eval (buf[4] == 3);
  __analyzer_eval (buf[5] == 3);
  __analyzer_eval (buf[6] == 3);
  __analyzer_eval (buf[7] == 0);

  memset (buf + 0, 4, 3);
  __analyzer_eval (buf[0] == 4);
  __analyzer_eval (buf[1] == 4);
  __analyzer_eval (buf[2] == 4);
  __analyzer_eval (buf[3] == 2);
  __analyzer_eval (buf[4] == 3);
  __analyzer_eval (buf[5] == 3);
  __analyzer_eval (buf[6] == 3);
  __analyzer_eval (buf[7] == 0);
}
