//type: rp
//options:  --c++14
# 0 "./tree-ssa/pr95638.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/pr95638.C"





# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 1 3
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 1 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wvariadic-macros"

#pragma GCC diagnostic ignored "-Wc++11-extensions"
#pragma GCC diagnostic ignored "-Wc++23-extensions"
# 328 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  typedef long unsigned int size_t;
  typedef long int ptrdiff_t;


  typedef decltype(nullptr) nullptr_t;


#pragma GCC visibility push(default)


  extern "C++" __attribute__ ((__noreturn__, __always_inline__))
  inline void __terminate() noexcept
  {
    void terminate() noexcept __attribute__ ((__noreturn__,__cold__));
    terminate();
  }
#pragma GCC visibility pop
}
# 361 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
namespace __gnu_cxx
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
# 565 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
#pragma GCC visibility push(default)




  __attribute__((__always_inline__))
  constexpr inline bool
  __is_constant_evaluated() noexcept
  {





    return __builtin_is_constant_evaluated();



  }
#pragma GCC visibility pop
}
# 609 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
#pragma GCC visibility push(default)

  extern "C++" __attribute__ ((__noreturn__)) __attribute__((__cold__))
  void
  __glibcxx_assert_fail
    (const char* __file, int __line, const char* __function,
     const char* __condition)
  noexcept;
#pragma GCC visibility pop
}
# 719 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/os_defines.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/os_defines.h" 3
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
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/os_defines.h" 2 3
# 720 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/cpu_defines.h" 1 3
# 723 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3
# 879 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace __gnu_cxx
{
  typedef __decltype(0.0bf16) __bfloat16_t;
}
# 945 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
#pragma GCC diagnostic pop
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 2 3
# 1 "/usr/include/assert.h" 1 3 4
# 65 "/usr/include/assert.h" 3 4
extern "C" {


extern void __assert_fail (const char *__assertion, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));


extern void __assert_perror_fail (int __errnum, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));




extern void __assert (const char *__assertion, const char *__file, int __line)
     throw () __attribute__ ((__noreturn__));


}
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 2 3
# 7 "./tree-ssa/pr95638.C" 2
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdio" 1 3
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdio" 3
# 1 "/usr/include/stdio.h" 1 3 4
# 29 "/usr/include/stdio.h" 3 4
extern "C" {



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 34 "/usr/include/stdio.h" 2 3 4

# 1 "/usr/include/bits/types.h" 1 3 4
# 27 "/usr/include/bits/types.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/bits/types.h" 2 3 4


typedef unsigned char __u_char;
typedef unsigned short int __u_short;
typedef unsigned int __u_int;
typedef unsigned long int __u_long;


typedef signed char __int8_t;
typedef unsigned char __uint8_t;
typedef signed short int __int16_t;
typedef unsigned short int __uint16_t;
typedef signed int __int32_t;
typedef unsigned int __uint32_t;

typedef signed long int __int64_t;
typedef unsigned long int __uint64_t;







typedef long int __quad_t;
typedef unsigned long int __u_quad_t;
# 130 "/usr/include/bits/types.h" 3 4
# 1 "/usr/include/bits/typesizes.h" 1 3 4
# 131 "/usr/include/bits/types.h" 2 3 4


typedef unsigned long int __dev_t;
typedef unsigned int __uid_t;
typedef unsigned int __gid_t;
typedef unsigned long int __ino_t;
typedef unsigned long int __ino64_t;
typedef unsigned int __mode_t;
typedef unsigned long int __nlink_t;
typedef long int __off_t;
typedef long int __off64_t;
typedef int __pid_t;
typedef struct { int __val[2]; } __fsid_t;
typedef long int __clock_t;
typedef unsigned long int __rlim_t;
typedef unsigned long int __rlim64_t;
typedef unsigned int __id_t;
typedef long int __time_t;
typedef unsigned int __useconds_t;
typedef long int __suseconds_t;

typedef int __daddr_t;
typedef int __key_t;


typedef int __clockid_t;


typedef void * __timer_t;


typedef long int __blksize_t;




typedef long int __blkcnt_t;
typedef long int __blkcnt64_t;


typedef unsigned long int __fsblkcnt_t;
typedef unsigned long int __fsblkcnt64_t;


typedef unsigned long int __fsfilcnt_t;
typedef unsigned long int __fsfilcnt64_t;


typedef long int __fsword_t;

typedef long int __ssize_t;


typedef long int __syscall_slong_t;

typedef unsigned long int __syscall_ulong_t;



typedef __off64_t __loff_t;
typedef __quad_t *__qaddr_t;
typedef char *__caddr_t;


typedef long int __intptr_t;


typedef unsigned int __socklen_t;
# 36 "/usr/include/stdio.h" 2 3 4
# 44 "/usr/include/stdio.h" 3 4
struct _IO_FILE;



typedef struct _IO_FILE FILE;





# 64 "/usr/include/stdio.h" 3 4
typedef struct _IO_FILE __FILE;
# 74 "/usr/include/stdio.h" 3 4
# 1 "/usr/include/libio.h" 1 3 4
# 32 "/usr/include/libio.h" 3 4
# 1 "/usr/include/_G_config.h" 1 3 4
# 15 "/usr/include/_G_config.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 16 "/usr/include/_G_config.h" 2 3 4




# 1 "/usr/include/wchar.h" 1 3 4
# 82 "/usr/include/wchar.h" 3 4
typedef struct
{
  int __count;
  union
  {

    unsigned int __wch;



    char __wchb[4];
  } __value;
} __mbstate_t;
# 21 "/usr/include/_G_config.h" 2 3 4
typedef struct
{
  __off_t __pos;
  __mbstate_t __state;
} _G_fpos_t;
typedef struct
{
  __off64_t __pos;
  __mbstate_t __state;
} _G_fpos64_t;
# 33 "/usr/include/libio.h" 2 3 4
# 50 "/usr/include/libio.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 51 "/usr/include/libio.h" 2 3 4
# 145 "/usr/include/libio.h" 3 4
struct _IO_jump_t; struct _IO_FILE;
# 155 "/usr/include/libio.h" 3 4
typedef void _IO_lock_t;





struct _IO_marker {
  struct _IO_marker *_next;
  struct _IO_FILE *_sbuf;



  int _pos;
# 178 "/usr/include/libio.h" 3 4
};


enum __codecvt_result
{
  __codecvt_ok,
  __codecvt_partial,
  __codecvt_error,
  __codecvt_noconv
};
# 246 "/usr/include/libio.h" 3 4
struct _IO_FILE {
  int _flags;




  char* _IO_read_ptr;
  char* _IO_read_end;
  char* _IO_read_base;
  char* _IO_write_base;
  char* _IO_write_ptr;
  char* _IO_write_end;
  char* _IO_buf_base;
  char* _IO_buf_end;

  char *_IO_save_base;
  char *_IO_backup_base;
  char *_IO_save_end;

  struct _IO_marker *_markers;

  struct _IO_FILE *_chain;

  int _fileno;



  int _flags2;

  __off_t _old_offset;



  unsigned short _cur_column;
  signed char _vtable_offset;
  char _shortbuf[1];



  _IO_lock_t *_lock;
# 294 "/usr/include/libio.h" 3 4
  __off64_t _offset;
# 303 "/usr/include/libio.h" 3 4
  void *__pad1;
  void *__pad2;
  void *__pad3;
  void *__pad4;
  size_t __pad5;

  int _mode;

  char _unused2[15 * sizeof (int) - 4 * sizeof (void *) - sizeof (size_t)];

};





struct _IO_FILE_plus;

extern struct _IO_FILE_plus _IO_2_1_stdin_;
extern struct _IO_FILE_plus _IO_2_1_stdout_;
extern struct _IO_FILE_plus _IO_2_1_stderr_;
# 339 "/usr/include/libio.h" 3 4
typedef __ssize_t __io_read_fn (void *__cookie, char *__buf, size_t __nbytes);







typedef __ssize_t __io_write_fn (void *__cookie, const char *__buf,
     size_t __n);







typedef int __io_seek_fn (void *__cookie, __off64_t *__pos, int __w);


typedef int __io_close_fn (void *__cookie);




typedef __io_read_fn cookie_read_function_t;
typedef __io_write_fn cookie_write_function_t;
typedef __io_seek_fn cookie_seek_function_t;
typedef __io_close_fn cookie_close_function_t;


typedef struct
{
  __io_read_fn *read;
  __io_write_fn *write;
  __io_seek_fn *seek;
  __io_close_fn *close;
} _IO_cookie_io_functions_t;
typedef _IO_cookie_io_functions_t cookie_io_functions_t;

struct _IO_cookie_file;


extern void _IO_cookie_init (struct _IO_cookie_file *__cfile, int __read_write,
        void *__cookie, _IO_cookie_io_functions_t __fns);




extern "C" {


extern int __underflow (_IO_FILE *);
extern int __uflow (_IO_FILE *);
extern int __overflow (_IO_FILE *, int);
# 435 "/usr/include/libio.h" 3 4
extern int _IO_getc (_IO_FILE *__fp);
extern int _IO_putc (int __c, _IO_FILE *__fp);
extern int _IO_feof (_IO_FILE *__fp) throw ();
extern int _IO_ferror (_IO_FILE *__fp) throw ();

extern int _IO_peekc_locked (_IO_FILE *__fp);





extern void _IO_flockfile (_IO_FILE *) throw ();
extern void _IO_funlockfile (_IO_FILE *) throw ();
extern int _IO_ftrylockfile (_IO_FILE *) throw ();
# 465 "/usr/include/libio.h" 3 4
extern int _IO_vfscanf (_IO_FILE * __restrict, const char * __restrict,
   __gnuc_va_list, int *__restrict);
extern int _IO_vfprintf (_IO_FILE *__restrict, const char *__restrict,
    __gnuc_va_list);
extern __ssize_t _IO_padn (_IO_FILE *, int, __ssize_t);
extern size_t _IO_sgetn (_IO_FILE *, void *, size_t);

extern __off64_t _IO_seekoff (_IO_FILE *, __off64_t, int, int);
extern __off64_t _IO_seekpos (_IO_FILE *, __off64_t, int);

extern void _IO_free_backup_area (_IO_FILE *) throw ();
# 527 "/usr/include/libio.h" 3 4
}
# 75 "/usr/include/stdio.h" 2 3 4




typedef __gnuc_va_list va_list;
# 90 "/usr/include/stdio.h" 3 4
typedef __off_t off_t;






typedef __off64_t off64_t;




typedef __ssize_t ssize_t;







typedef _G_fpos_t fpos_t;





typedef _G_fpos64_t fpos64_t;
# 164 "/usr/include/stdio.h" 3 4
# 1 "/usr/include/bits/stdio_lim.h" 1 3 4
# 165 "/usr/include/stdio.h" 2 3 4



extern struct _IO_FILE *stdin;
extern struct _IO_FILE *stdout;
extern struct _IO_FILE *stderr;







extern int remove (const char *__filename) throw ();

extern int rename (const char *__old, const char *__new) throw ();




extern int renameat (int __oldfd, const char *__old, int __newfd,
       const char *__new) throw ();








extern FILE *tmpfile (void) ;
# 205 "/usr/include/stdio.h" 3 4
extern FILE *tmpfile64 (void) ;



extern char *tmpnam (char *__s) throw () ;





extern char *tmpnam_r (char *__s) throw () ;
# 227 "/usr/include/stdio.h" 3 4
extern char *tempnam (const char *__dir, const char *__pfx)
     throw () __attribute__ ((__malloc__)) ;








extern int fclose (FILE *__stream);




extern int fflush (FILE *__stream);

# 252 "/usr/include/stdio.h" 3 4
extern int fflush_unlocked (FILE *__stream);
# 262 "/usr/include/stdio.h" 3 4
extern int fcloseall (void);









extern FILE *fopen (const char *__restrict __filename,
      const char *__restrict __modes) ;




extern FILE *freopen (const char *__restrict __filename,
        const char *__restrict __modes,
        FILE *__restrict __stream) ;
# 295 "/usr/include/stdio.h" 3 4


extern FILE *fopen64 (const char *__restrict __filename,
        const char *__restrict __modes) ;
extern FILE *freopen64 (const char *__restrict __filename,
   const char *__restrict __modes,
   FILE *__restrict __stream) ;




extern FILE *fdopen (int __fd, const char *__modes) throw () ;





extern FILE *fopencookie (void *__restrict __magic_cookie,
     const char *__restrict __modes,
     _IO_cookie_io_functions_t __io_funcs) throw () ;




extern FILE *fmemopen (void *__s, size_t __len, const char *__modes)
  throw () ;




extern FILE *open_memstream (char **__bufloc, size_t *__sizeloc) throw () ;






extern void setbuf (FILE *__restrict __stream, char *__restrict __buf) throw ();



extern int setvbuf (FILE *__restrict __stream, char *__restrict __buf,
      int __modes, size_t __n) throw ();





extern void setbuffer (FILE *__restrict __stream, char *__restrict __buf,
         size_t __size) throw ();


extern void setlinebuf (FILE *__stream) throw ();








extern int fprintf (FILE *__restrict __stream,
      const char *__restrict __format, ...);




extern int printf (const char *__restrict __format, ...);

extern int sprintf (char *__restrict __s,
      const char *__restrict __format, ...) throw ();





extern int vfprintf (FILE *__restrict __s, const char *__restrict __format,
       __gnuc_va_list __arg);




extern int vprintf (const char *__restrict __format, __gnuc_va_list __arg);

extern int vsprintf (char *__restrict __s, const char *__restrict __format,
       __gnuc_va_list __arg) throw ();





extern int snprintf (char *__restrict __s, size_t __maxlen,
       const char *__restrict __format, ...)
     throw () __attribute__ ((__format__ (__printf__, 3, 4)));

extern int vsnprintf (char *__restrict __s, size_t __maxlen,
        const char *__restrict __format, __gnuc_va_list __arg)
     throw () __attribute__ ((__format__ (__printf__, 3, 0)));






extern int vasprintf (char **__restrict __ptr, const char *__restrict __f,
        __gnuc_va_list __arg)
     throw () __attribute__ ((__format__ (__printf__, 2, 0))) ;
extern int __asprintf (char **__restrict __ptr,
         const char *__restrict __fmt, ...)
     throw () __attribute__ ((__format__ (__printf__, 2, 3))) ;
extern int asprintf (char **__restrict __ptr,
       const char *__restrict __fmt, ...)
     throw () __attribute__ ((__format__ (__printf__, 2, 3))) ;




extern int vdprintf (int __fd, const char *__restrict __fmt,
       __gnuc_va_list __arg)
     __attribute__ ((__format__ (__printf__, 2, 0)));
extern int dprintf (int __fd, const char *__restrict __fmt, ...)
     __attribute__ ((__format__ (__printf__, 2, 3)));








extern int fscanf (FILE *__restrict __stream,
     const char *__restrict __format, ...) ;




extern int scanf (const char *__restrict __format, ...) ;

extern int sscanf (const char *__restrict __s,
     const char *__restrict __format, ...) throw ();
# 463 "/usr/include/stdio.h" 3 4








extern int vfscanf (FILE *__restrict __s, const char *__restrict __format,
      __gnuc_va_list __arg)
     __attribute__ ((__format__ (__scanf__, 2, 0))) ;





extern int vscanf (const char *__restrict __format, __gnuc_va_list __arg)
     __attribute__ ((__format__ (__scanf__, 1, 0))) ;


extern int vsscanf (const char *__restrict __s,
      const char *__restrict __format, __gnuc_va_list __arg)
     throw () __attribute__ ((__format__ (__scanf__, 2, 0)));
# 522 "/usr/include/stdio.h" 3 4









extern int fgetc (FILE *__stream);
extern int getc (FILE *__stream);





extern int getchar (void);

# 550 "/usr/include/stdio.h" 3 4
extern int getc_unlocked (FILE *__stream);
extern int getchar_unlocked (void);
# 561 "/usr/include/stdio.h" 3 4
extern int fgetc_unlocked (FILE *__stream);











extern int fputc (int __c, FILE *__stream);
extern int putc (int __c, FILE *__stream);





extern int putchar (int __c);

# 594 "/usr/include/stdio.h" 3 4
extern int fputc_unlocked (int __c, FILE *__stream);







extern int putc_unlocked (int __c, FILE *__stream);
extern int putchar_unlocked (int __c);






extern int getw (FILE *__stream);


extern int putw (int __w, FILE *__stream);








extern char *fgets (char *__restrict __s, int __n, FILE *__restrict __stream)
     ;
# 640 "/usr/include/stdio.h" 3 4

# 649 "/usr/include/stdio.h" 3 4
extern char *fgets_unlocked (char *__restrict __s, int __n,
        FILE *__restrict __stream) ;
# 665 "/usr/include/stdio.h" 3 4
extern __ssize_t __getdelim (char **__restrict __lineptr,
          size_t *__restrict __n, int __delimiter,
          FILE *__restrict __stream) ;
extern __ssize_t getdelim (char **__restrict __lineptr,
        size_t *__restrict __n, int __delimiter,
        FILE *__restrict __stream) ;







extern __ssize_t getline (char **__restrict __lineptr,
       size_t *__restrict __n,
       FILE *__restrict __stream) ;








extern int fputs (const char *__restrict __s, FILE *__restrict __stream);





extern int puts (const char *__s);






extern int ungetc (int __c, FILE *__stream);






extern size_t fread (void *__restrict __ptr, size_t __size,
       size_t __n, FILE *__restrict __stream) ;




extern size_t fwrite (const void *__restrict __ptr, size_t __size,
        size_t __n, FILE *__restrict __s);

# 726 "/usr/include/stdio.h" 3 4
extern int fputs_unlocked (const char *__restrict __s,
      FILE *__restrict __stream);
# 737 "/usr/include/stdio.h" 3 4
extern size_t fread_unlocked (void *__restrict __ptr, size_t __size,
         size_t __n, FILE *__restrict __stream) ;
extern size_t fwrite_unlocked (const void *__restrict __ptr, size_t __size,
          size_t __n, FILE *__restrict __stream);








extern int fseek (FILE *__stream, long int __off, int __whence);




extern long int ftell (FILE *__stream) ;




extern void rewind (FILE *__stream);

# 773 "/usr/include/stdio.h" 3 4
extern int fseeko (FILE *__stream, __off_t __off, int __whence);




extern __off_t ftello (FILE *__stream) ;
# 792 "/usr/include/stdio.h" 3 4






extern int fgetpos (FILE *__restrict __stream, fpos_t *__restrict __pos);




extern int fsetpos (FILE *__stream, const fpos_t *__pos);
# 815 "/usr/include/stdio.h" 3 4



extern int fseeko64 (FILE *__stream, __off64_t __off, int __whence);
extern __off64_t ftello64 (FILE *__stream) ;
extern int fgetpos64 (FILE *__restrict __stream, fpos64_t *__restrict __pos);
extern int fsetpos64 (FILE *__stream, const fpos64_t *__pos);




extern void clearerr (FILE *__stream) throw ();

extern int feof (FILE *__stream) throw () ;

extern int ferror (FILE *__stream) throw () ;




extern void clearerr_unlocked (FILE *__stream) throw ();
extern int feof_unlocked (FILE *__stream) throw () ;
extern int ferror_unlocked (FILE *__stream) throw () ;








extern void perror (const char *__s);






# 1 "/usr/include/bits/sys_errlist.h" 1 3 4
# 26 "/usr/include/bits/sys_errlist.h" 3 4
extern int sys_nerr;
extern const char *const sys_errlist[];


extern int _sys_nerr;
extern const char *const _sys_errlist[];
# 854 "/usr/include/stdio.h" 2 3 4




extern int fileno (FILE *__stream) throw () ;




extern int fileno_unlocked (FILE *__stream) throw () ;
# 873 "/usr/include/stdio.h" 3 4
extern FILE *popen (const char *__command, const char *__modes) ;





extern int pclose (FILE *__stream);





extern char *ctermid (char *__s) throw ();





extern char *cuserid (char *__s);




struct obstack;


extern int obstack_printf (struct obstack *__restrict __obstack,
      const char *__restrict __format, ...)
     throw () __attribute__ ((__format__ (__printf__, 2, 3)));
extern int obstack_vprintf (struct obstack *__restrict __obstack,
       const char *__restrict __format,
       __gnuc_va_list __args)
     throw () __attribute__ ((__format__ (__printf__, 2, 0)));







extern void flockfile (FILE *__stream) throw ();



extern int ftrylockfile (FILE *__stream) throw () ;


extern void funlockfile (FILE *__stream) throw ();
# 943 "/usr/include/stdio.h" 3 4
}
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdio" 2 3
# 98 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdio" 3
namespace std
{
  using ::FILE;
  using ::fpos_t;

  using ::clearerr;
  using ::fclose;
  using ::feof;
  using ::ferror;
  using ::fflush;
  using ::fgetc;
  using ::fgetpos;
  using ::fgets;
  using ::fopen;
  using ::fprintf;
  using ::fputc;
  using ::fputs;
  using ::fread;
  using ::freopen;
  using ::fscanf;
  using ::fseek;
  using ::fsetpos;
  using ::ftell;
  using ::fwrite;
  using ::getc;
  using ::getchar;




  using ::perror;
  using ::printf;
  using ::putc;
  using ::putchar;
  using ::puts;
  using ::remove;
  using ::rename;
  using ::rewind;
  using ::scanf;
  using ::setbuf;
  using ::setvbuf;
  using ::sprintf;
  using ::sscanf;
  using ::tmpfile;

  using ::tmpnam;

  using ::ungetc;
  using ::vfprintf;
  using ::vprintf;
  using ::vsprintf;
}
# 159 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdio" 3
namespace __gnu_cxx
{
# 177 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdio" 3
  using ::snprintf;
  using ::vfscanf;
  using ::vscanf;
  using ::vsnprintf;
  using ::vsscanf;

}

namespace std
{
  using ::__gnu_cxx::snprintf;
  using ::__gnu_cxx::vfscanf;
  using ::__gnu_cxx::vscanf;
  using ::__gnu_cxx::vsnprintf;
  using ::__gnu_cxx::vsscanf;
}
# 8 "./tree-ssa/pr95638.C" 2
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdint" 1 3
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdint" 3
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdint.h" 1 3 4
# 9 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdint.h" 3 4
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/usr/include/stdint.h" 1 3 4
# 26 "/usr/include/stdint.h" 3 4
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
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdint" 2 3


namespace std
{

  using ::int8_t;
  using ::int16_t;
  using ::int32_t;
  using ::int64_t;

  using ::int_fast8_t;
  using ::int_fast16_t;
  using ::int_fast32_t;
  using ::int_fast64_t;

  using ::int_least8_t;
  using ::int_least16_t;
  using ::int_least32_t;
  using ::int_least64_t;

  using ::intmax_t;
  using ::intptr_t;

  using ::uint8_t;
  using ::uint16_t;
  using ::uint32_t;
  using ::uint64_t;

  using ::uint_fast8_t;
  using ::uint_fast16_t;
  using ::uint_fast32_t;
  using ::uint_fast64_t;

  using ::uint_least8_t;
  using ::uint_least16_t;
  using ::uint_least32_t;
  using ::uint_least64_t;

  using ::uintmax_t;
  using ::uintptr_t;
# 144 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdint" 3
}
# 9 "./tree-ssa/pr95638.C" 2
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 1 3
# 65 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/memoryfwd.h" 1 3
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/memoryfwd.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/memoryfwd.h" 3
  template<typename>
    class allocator;

  template<>
    class allocator<void>;



  template<typename, typename>
    struct uses_allocator;

  template<typename>
    struct allocator_traits;





}
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 1 3
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++allocator.h" 1 3
# 33 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++allocator.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 1 3
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 1 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 3
extern "C++" {

namespace std __attribute__ ((__visibility__ ("default")))
{
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 3
  class exception
  {
  public:
    exception() noexcept { }
    virtual ~exception() noexcept;

    exception(const exception&) = default;
    exception& operator=(const exception&) = default;
    exception(exception&&) = default;
    exception& operator=(exception&&) = default;




    virtual const char*
    what() const noexcept;
  };



}

}
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 2 3





# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 2 3

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"

#pragma GCC visibility push(default)

extern "C++" {

namespace std
{






  class bad_alloc : public exception
  {
  public:
    bad_alloc() throw() { }


    bad_alloc(const bad_alloc&) = default;
    bad_alloc& operator=(const bad_alloc&) = default;




    virtual ~bad_alloc() throw();


    virtual const char* what() const throw();
  };


  class bad_array_new_length : public bad_alloc
  {
  public:
    bad_array_new_length() throw() { }



    virtual ~bad_array_new_length() throw();


    virtual const char* what() const throw();
  };






  struct nothrow_t
  {

    explicit nothrow_t() = default;

  };

  extern const nothrow_t nothrow;



  typedef void (*new_handler)();



  new_handler set_new_handler(new_handler) throw();



  new_handler get_new_handler() noexcept;

}
# 137 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
[[__nodiscard__]] void* operator new(std::size_t)
 
  __attribute__((__externally_visible__, __malloc__));
[[__nodiscard__]] void* operator new[](std::size_t)
 
  __attribute__((__externally_visible__, __malloc__));
void operator delete(void*) noexcept
  __attribute__((__externally_visible__));
void operator delete[](void*) noexcept
  __attribute__((__externally_visible__));

void operator delete(void*, std::size_t)
  noexcept
  __attribute__((__externally_visible__));
void operator delete[](void*, std::size_t)
  noexcept
  __attribute__((__externally_visible__));

[[__nodiscard__]] void* operator new(std::size_t, const std::nothrow_t&)
  noexcept
  __attribute__((__externally_visible__, __alloc_size__ (1), __malloc__));
[[__nodiscard__]] void* operator new[](std::size_t, const std::nothrow_t&)
  noexcept
  __attribute__((__externally_visible__, __alloc_size__ (1), __malloc__));
void operator delete(void*, const std::nothrow_t&)
  noexcept
  __attribute__((__externally_visible__));
void operator delete[](void*, const std::nothrow_t&)
  noexcept
  __attribute__((__externally_visible__));
# 205 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
[[__nodiscard__]] inline
void* operator new(std::size_t, void* __p)
  noexcept
{ return __p; }
[[__nodiscard__]] inline
void* operator new[](std::size_t, void* __p)
  noexcept
{ return __p; }




inline void operator delete (void*, void*)
  noexcept
{ }
inline void operator delete[](void*, void*)
  noexcept
{ }

}

namespace std
{
# 264 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
}

#pragma GCC visibility pop
#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functexcept.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functexcept.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_defines.h" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functexcept.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{




  void
  __throw_bad_exception(void) __attribute__((__noreturn__));


  void
  __throw_bad_alloc(void) __attribute__((__noreturn__));

  void
  __throw_bad_array_new_length(void) __attribute__((__noreturn__));


  void
  __throw_bad_cast(void) __attribute__((__noreturn__,__cold__));

  void
  __throw_bad_typeid(void) __attribute__((__noreturn__,__cold__));


  void
  __throw_logic_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_domain_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_invalid_argument(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_length_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_out_of_range(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_out_of_range_fmt(const char*, ...) __attribute__((__noreturn__,__cold__))
    __attribute__((__format__(__gnu_printf__, 1, 2)));

  void
  __throw_runtime_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_range_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_overflow_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_underflow_error(const char*) __attribute__((__noreturn__,__cold__));


  void
  __throw_ios_failure(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_ios_failure(const char*, int) __attribute__((__noreturn__,__cold__));


  void
  __throw_system_error(int) __attribute__((__noreturn__,__cold__));


  void
  __throw_future_error(int) __attribute__((__noreturn__,__cold__));


  void
  __throw_bad_function_call() __attribute__((__noreturn__,__cold__));
# 140 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functexcept.h" 3

}
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 1 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 1 3
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 2 3

extern "C++"
{
namespace std __attribute__ ((__visibility__ ("default")))
{


  template<typename _Tp>
    class reference_wrapper;
# 91 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp, _Tp __v>
    struct integral_constant
    {
      static constexpr _Tp value = __v;
      using value_type = _Tp;
      using type = integral_constant<_Tp, __v>;
      constexpr operator value_type() const noexcept { return value; }


      constexpr value_type operator()() const noexcept { return value; }

    };


  template<typename _Tp, _Tp __v>
    constexpr _Tp integral_constant<_Tp, __v>::value;




  template<bool __v>
    using __bool_constant = integral_constant<bool, __v>;



  using true_type = __bool_constant<true>;


  using false_type = __bool_constant<false>;
# 132 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<bool, typename _Tp = void>
    struct enable_if
    { };


  template<typename _Tp>
    struct enable_if<true, _Tp>
    { using type = _Tp; };


  template<bool _Cond, typename _Tp = void>
    using __enable_if_t = typename enable_if<_Cond, _Tp>::type;

  template<bool>
    struct __conditional
    {
      template<typename _Tp, typename>
 using type = _Tp;
    };

  template<>
    struct __conditional<false>
    {
      template<typename, typename _Up>
 using type = _Up;
    };


  template<bool _Cond, typename _If, typename _Else>
    using __conditional_t
      = typename __conditional<_Cond>::template type<_If, _Else>;


  template <typename _Type>
    struct __type_identity
    { using type = _Type; };

  template<typename _Tp>
    using __type_identity_t = typename __type_identity<_Tp>::type;

  namespace __detail
  {

    template<typename _Tp, typename...>
      using __first_t = _Tp;


    template<typename... _Bn>
      auto __or_fn(int) -> __first_t<false_type,
         __enable_if_t<!bool(_Bn::value)>...>;

    template<typename... _Bn>
      auto __or_fn(...) -> true_type;

    template<typename... _Bn>
      auto __and_fn(int) -> __first_t<true_type,
          __enable_if_t<bool(_Bn::value)>...>;

    template<typename... _Bn>
      auto __and_fn(...) -> false_type;
  }




  template<typename... _Bn>
    struct __or_
    : decltype(__detail::__or_fn<_Bn...>(0))
    { };

  template<typename... _Bn>
    struct __and_
    : decltype(__detail::__and_fn<_Bn...>(0))
    { };

  template<typename _Pp>
    struct __not_
    : __bool_constant<!bool(_Pp::value)>
    { };
# 282 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename>
    struct is_reference;
  template<typename>
    struct is_function;
  template<typename>
    struct is_void;
  template<typename>
    struct remove_cv;
  template<typename>
    struct is_const;


  template<typename>
    struct __is_array_unknown_bounds;




  template <typename _Tp, size_t = sizeof(_Tp)>
    constexpr true_type __is_complete_or_unbounded(__type_identity<_Tp>)
    { return {}; }

  template <typename _TypeIdentity,
      typename _NestedType = typename _TypeIdentity::type>
    constexpr typename __or_<
      is_reference<_NestedType>,
      is_function<_NestedType>,
      is_void<_NestedType>,
      __is_array_unknown_bounds<_NestedType>
    >::type __is_complete_or_unbounded(_TypeIdentity)
    { return {}; }


  template<typename _Tp>
    using __remove_cv_t = typename remove_cv<_Tp>::type;





  template<typename _Tp>
    struct is_void
    : public false_type { };

  template<>
    struct is_void<void>
    : public true_type { };

  template<>
    struct is_void<const void>
    : public true_type { };

  template<>
    struct is_void<volatile void>
    : public true_type { };

  template<>
    struct is_void<const volatile void>
    : public true_type { };


  template<typename>
    struct __is_integral_helper
    : public false_type { };

  template<>
    struct __is_integral_helper<bool>
    : public true_type { };

  template<>
    struct __is_integral_helper<char>
    : public true_type { };

  template<>
    struct __is_integral_helper<signed char>
    : public true_type { };

  template<>
    struct __is_integral_helper<unsigned char>
    : public true_type { };




  template<>
    struct __is_integral_helper<wchar_t>
    : public true_type { };







  template<>
    struct __is_integral_helper<char16_t>
    : public true_type { };

  template<>
    struct __is_integral_helper<char32_t>
    : public true_type { };

  template<>
    struct __is_integral_helper<short>
    : public true_type { };

  template<>
    struct __is_integral_helper<unsigned short>
    : public true_type { };

  template<>
    struct __is_integral_helper<int>
    : public true_type { };

  template<>
    struct __is_integral_helper<unsigned int>
    : public true_type { };

  template<>
    struct __is_integral_helper<long>
    : public true_type { };

  template<>
    struct __is_integral_helper<unsigned long>
    : public true_type { };

  template<>
    struct __is_integral_helper<long long>
    : public true_type { };

  template<>
    struct __is_integral_helper<unsigned long long>
    : public true_type { };
# 465 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_integral
    : public __is_integral_helper<__remove_cv_t<_Tp>>::type
    { };


  template<typename>
    struct __is_floating_point_helper
    : public false_type { };

  template<>
    struct __is_floating_point_helper<float>
    : public true_type { };

  template<>
    struct __is_floating_point_helper<double>
    : public true_type { };

  template<>
    struct __is_floating_point_helper<long double>
    : public true_type { };
# 525 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_floating_point
    : public __is_floating_point_helper<__remove_cv_t<_Tp>>::type
    { };



  template<typename _Tp>
    struct is_array
    : public __bool_constant<__is_array(_Tp)>
    { };
# 552 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_pointer
    : public __bool_constant<__is_pointer(_Tp)>
    { };
# 579 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename>
    struct is_lvalue_reference
    : public false_type { };

  template<typename _Tp>
    struct is_lvalue_reference<_Tp&>
    : public true_type { };


  template<typename>
    struct is_rvalue_reference
    : public false_type { };

  template<typename _Tp>
    struct is_rvalue_reference<_Tp&&>
    : public true_type { };



  template<typename _Tp>
    struct is_member_object_pointer
    : public __bool_constant<__is_member_object_pointer(_Tp)>
    { };
# 620 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_member_function_pointer
    : public __bool_constant<__is_member_function_pointer(_Tp)>
    { };
# 641 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_enum
    : public __bool_constant<__is_enum(_Tp)>
    { };


  template<typename _Tp>
    struct is_union
    : public __bool_constant<__is_union(_Tp)>
    { };


  template<typename _Tp>
    struct is_class
    : public __bool_constant<__is_class(_Tp)>
    { };



  template<typename _Tp>
    struct is_function
    : public __bool_constant<__is_function(_Tp)>
    { };
# 680 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_null_pointer
    : public false_type { };

  template<>
    struct is_null_pointer<std::nullptr_t>
    : public true_type { };

  template<>
    struct is_null_pointer<const std::nullptr_t>
    : public true_type { };

  template<>
    struct is_null_pointer<volatile std::nullptr_t>
    : public true_type { };

  template<>
    struct is_null_pointer<const volatile std::nullptr_t>
    : public true_type { };



  template<typename _Tp>
    struct __is_nullptr_t
    : public is_null_pointer<_Tp>
    { } __attribute__ ((__deprecated__ ("use '" "std::is_null_pointer" "' instead")));






  template<typename _Tp>
    struct is_reference
    : public __bool_constant<__is_reference(_Tp)>
    { };
# 734 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_arithmetic
    : public __or_<is_integral<_Tp>, is_floating_point<_Tp>>::type
    { };


  template<typename _Tp>
    struct is_fundamental
    : public __or_<is_arithmetic<_Tp>, is_void<_Tp>,
     is_null_pointer<_Tp>>::type
    { };



  template<typename _Tp>
    struct is_object
    : public __bool_constant<__is_object(_Tp)>
    { };
# 760 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename>
    struct is_member_pointer;


  template<typename _Tp>
    struct is_scalar
    : public __or_<is_arithmetic<_Tp>, is_enum<_Tp>, is_pointer<_Tp>,
                   is_member_pointer<_Tp>, is_null_pointer<_Tp>>::type
    { };


  template<typename _Tp>
    struct is_compound
    : public __bool_constant<!is_fundamental<_Tp>::value> { };



  template<typename _Tp>
    struct is_member_pointer
    : public __bool_constant<__is_member_pointer(_Tp)>
    { };
# 798 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename, typename>
    struct is_same;


  template<typename _Tp, typename... _Types>
    using __is_one_of = __or_<is_same<_Tp, _Types>...>;


  __extension__
  template<typename _Tp>
    using __is_signed_integer = __is_one_of<__remove_cv_t<_Tp>,
   signed char, signed short, signed int, signed long,
   signed long long
# 823 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
   >;


  __extension__
  template<typename _Tp>
    using __is_unsigned_integer = __is_one_of<__remove_cv_t<_Tp>,
   unsigned char, unsigned short, unsigned int, unsigned long,
   unsigned long long
# 843 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
   >;


  template<typename _Tp>
    using __is_standard_integer
      = __or_<__is_signed_integer<_Tp>, __is_unsigned_integer<_Tp>>;


  template<typename...> using __void_t = void;






  template<typename _Tp>
    struct is_const
    : public __bool_constant<__is_const(_Tp)>
    { };
# 874 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_volatile
    : public __bool_constant<__is_volatile(_Tp)>
    { };
# 895 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct
   
    is_trivial
    : public __bool_constant<__is_trivial(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_trivially_copyable
    : public __bool_constant<__is_trivially_copyable(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_standard_layout
    : public __bool_constant<__is_standard_layout(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };






  template<typename _Tp>
    struct
   
    is_pod
    : public __bool_constant<__is_pod(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };





  template<typename _Tp>
    struct
   
    is_literal_type
    : public __bool_constant<__is_literal_type(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_empty
    : public __bool_constant<__is_empty(_Tp)>
    { };


  template<typename _Tp>
    struct is_polymorphic
    : public __bool_constant<__is_polymorphic(_Tp)>
    { };




  template<typename _Tp>
    struct is_final
    : public __bool_constant<__is_final(_Tp)>
    { };



  template<typename _Tp>
    struct is_abstract
    : public __bool_constant<__is_abstract(_Tp)>
    { };


  template<typename _Tp,
    bool = is_arithmetic<_Tp>::value>
    struct __is_signed_helper
    : public false_type { };

  template<typename _Tp>
    struct __is_signed_helper<_Tp, true>
    : public __bool_constant<_Tp(-1) < _Tp(0)>
    { };



  template<typename _Tp>
    struct is_signed
    : public __is_signed_helper<_Tp>::type
    { };


  template<typename _Tp>
    struct is_unsigned
    : public __and_<is_arithmetic<_Tp>, __not_<is_signed<_Tp>>>::type
    { };


  template<typename _Tp, typename _Up = _Tp&&>
    _Up
    __declval(int);

  template<typename _Tp>
    _Tp
    __declval(long);


  template<typename _Tp>
    auto declval() noexcept -> decltype(__declval<_Tp>(0));

  template<typename>
    struct remove_all_extents;


  template<typename _Tp>
    struct __is_array_known_bounds
    : public false_type
    { };

  template<typename _Tp, size_t _Size>
    struct __is_array_known_bounds<_Tp[_Size]>
    : public true_type
    { };

  template<typename _Tp>
    struct __is_array_unknown_bounds
    : public false_type
    { };

  template<typename _Tp>
    struct __is_array_unknown_bounds<_Tp[]>
    : public true_type
    { };
# 1047 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  struct __do_is_destructible_impl
  {
    template<typename _Tp, typename = decltype(declval<_Tp&>().~_Tp())>
      static true_type __test(int);

    template<typename>
      static false_type __test(...);
  };

  template<typename _Tp>
    struct __is_destructible_impl
    : public __do_is_destructible_impl
    {
      using type = decltype(__test<_Tp>(0));
    };

  template<typename _Tp,
           bool = __or_<is_void<_Tp>,
                        __is_array_unknown_bounds<_Tp>,
                        is_function<_Tp>>::value,
           bool = __or_<is_reference<_Tp>, is_scalar<_Tp>>::value>
    struct __is_destructible_safe;

  template<typename _Tp>
    struct __is_destructible_safe<_Tp, false, false>
    : public __is_destructible_impl<typename
               remove_all_extents<_Tp>::type>::type
    { };

  template<typename _Tp>
    struct __is_destructible_safe<_Tp, true, false>
    : public false_type { };

  template<typename _Tp>
    struct __is_destructible_safe<_Tp, false, true>
    : public true_type { };



  template<typename _Tp>
    struct is_destructible
    : public __is_destructible_safe<_Tp>::type
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };







  struct __do_is_nt_destructible_impl
  {
    template<typename _Tp>
      static __bool_constant<noexcept(declval<_Tp&>().~_Tp())>
      __test(int);

    template<typename>
      static false_type __test(...);
  };

  template<typename _Tp>
    struct __is_nt_destructible_impl
    : public __do_is_nt_destructible_impl
    {
      using type = decltype(__test<_Tp>(0));
    };

  template<typename _Tp,
           bool = __or_<is_void<_Tp>,
                        __is_array_unknown_bounds<_Tp>,
                        is_function<_Tp>>::value,
           bool = __or_<is_reference<_Tp>, is_scalar<_Tp>>::value>
    struct __is_nt_destructible_safe;

  template<typename _Tp>
    struct __is_nt_destructible_safe<_Tp, false, false>
    : public __is_nt_destructible_impl<typename
               remove_all_extents<_Tp>::type>::type
    { };

  template<typename _Tp>
    struct __is_nt_destructible_safe<_Tp, true, false>
    : public false_type { };

  template<typename _Tp>
    struct __is_nt_destructible_safe<_Tp, false, true>
    : public true_type { };



  template<typename _Tp>
    struct is_nothrow_destructible
    : public __is_nt_destructible_safe<_Tp>::type
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp, typename... _Args>
    using __is_constructible_impl
      = __bool_constant<__is_constructible(_Tp, _Args...)>;



  template<typename _Tp, typename... _Args>
    struct is_constructible
      : public __is_constructible_impl<_Tp, _Args...>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_default_constructible
    : public __is_constructible_impl<_Tp>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };



  template<typename _Tp>
    using __add_lval_ref_t = __add_lvalue_reference(_Tp);
# 1191 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_copy_constructible
    : public __is_constructible_impl<_Tp, __add_lval_ref_t<const _Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };



  template<typename _Tp>
    using __add_rval_ref_t = __add_rvalue_reference(_Tp);
# 1218 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_move_constructible
    : public __is_constructible_impl<_Tp, __add_rval_ref_t<_Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp, typename... _Args>
    using __is_nothrow_constructible_impl
      = __bool_constant<__is_nothrow_constructible(_Tp, _Args...)>;



  template<typename _Tp, typename... _Args>
    struct is_nothrow_constructible
    : public __is_nothrow_constructible_impl<_Tp, _Args...>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_nothrow_default_constructible
    : public __is_nothrow_constructible_impl<_Tp>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_nothrow_copy_constructible
    : public __is_nothrow_constructible_impl<_Tp, __add_lval_ref_t<const _Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_nothrow_move_constructible
    : public __is_nothrow_constructible_impl<_Tp, __add_rval_ref_t<_Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp, typename _Up>
    using __is_assignable_impl = __bool_constant<__is_assignable(_Tp, _Up)>;



  template<typename _Tp, typename _Up>
    struct is_assignable
    : public __is_assignable_impl<_Tp, _Up>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_copy_assignable
    : public __is_assignable_impl<__add_lval_ref_t<_Tp>,
      __add_lval_ref_t<const _Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_move_assignable
    : public __is_assignable_impl<__add_lval_ref_t<_Tp>, __add_rval_ref_t<_Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp, typename _Up>
    using __is_nothrow_assignable_impl
      = __bool_constant<__is_nothrow_assignable(_Tp, _Up)>;



  template<typename _Tp, typename _Up>
    struct is_nothrow_assignable
    : public __is_nothrow_assignable_impl<_Tp, _Up>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_nothrow_copy_assignable
    : public __is_nothrow_assignable_impl<__add_lval_ref_t<_Tp>,
       __add_lval_ref_t<const _Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_nothrow_move_assignable
    : public __is_nothrow_assignable_impl<__add_lval_ref_t<_Tp>,
       __add_rval_ref_t<_Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp, typename... _Args>
    using __is_trivially_constructible_impl
      = __bool_constant<__is_trivially_constructible(_Tp, _Args...)>;



  template<typename _Tp, typename... _Args>
    struct is_trivially_constructible
    : public __is_trivially_constructible_impl<_Tp, _Args...>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_trivially_default_constructible
    : public __is_trivially_constructible_impl<_Tp>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };
# 1370 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  struct __do_is_implicitly_default_constructible_impl
  {
    template <typename _Tp>
    static void __helper(const _Tp&);

    template <typename _Tp>
    static true_type __test(const _Tp&,
                            decltype(__helper<const _Tp&>({}))* = 0);

    static false_type __test(...);
  };

  template<typename _Tp>
    struct __is_implicitly_default_constructible_impl
    : public __do_is_implicitly_default_constructible_impl
    {
      using type = decltype(__test(declval<_Tp>()));
    };

  template<typename _Tp>
    struct __is_implicitly_default_constructible_safe
    : public __is_implicitly_default_constructible_impl<_Tp>::type
    { };

  template <typename _Tp>
    struct __is_implicitly_default_constructible
    : public __and_<__is_constructible_impl<_Tp>,
      __is_implicitly_default_constructible_safe<_Tp>>::type
    { };



  template<typename _Tp>
    struct is_trivially_copy_constructible
    : public __is_trivially_constructible_impl<_Tp, __add_lval_ref_t<const _Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_trivially_move_constructible
    : public __is_trivially_constructible_impl<_Tp, __add_rval_ref_t<_Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp, typename _Up>
    using __is_trivially_assignable_impl
      = __bool_constant<__is_trivially_assignable(_Tp, _Up)>;



  template<typename _Tp, typename _Up>
    struct is_trivially_assignable
    : public __is_trivially_assignable_impl<_Tp, _Up>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_trivially_copy_assignable
    : public __is_trivially_assignable_impl<__add_lval_ref_t<_Tp>,
         __add_lval_ref_t<const _Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_trivially_move_assignable
    : public __is_trivially_assignable_impl<__add_lval_ref_t<_Tp>,
         __add_rval_ref_t<_Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_trivially_destructible
    : public __and_<__is_destructible_safe<_Tp>,
      __bool_constant<__has_trivial_destructor(_Tp)>>::type
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };



  template<typename _Tp>
    struct has_virtual_destructor
    : public __bool_constant<__has_virtual_destructor(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };





  template<typename _Tp>
    struct alignment_of
    : public integral_constant<std::size_t, alignof(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };



  template<typename _Tp>
    struct rank
    : public integral_constant<std::size_t, __array_rank(_Tp)> { };
# 1506 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename, unsigned _Uint = 0>
    struct extent
    : public integral_constant<size_t, 0> { };

  template<typename _Tp, size_t _Size>
    struct extent<_Tp[_Size], 0>
    : public integral_constant<size_t, _Size> { };

  template<typename _Tp, unsigned _Uint, size_t _Size>
    struct extent<_Tp[_Size], _Uint>
    : public extent<_Tp, _Uint - 1>::type { };

  template<typename _Tp>
    struct extent<_Tp[], 0>
    : public integral_constant<size_t, 0> { };

  template<typename _Tp, unsigned _Uint>
    struct extent<_Tp[], _Uint>
    : public extent<_Tp, _Uint - 1>::type { };






  template<typename _Tp, typename _Up>
    struct is_same
    : public __bool_constant<__is_same(_Tp, _Up)>
    { };
# 1548 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Base, typename _Derived>
    struct is_base_of
    : public __bool_constant<__is_base_of(_Base, _Derived)>
    { };
# 1563 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _From, typename _To>
    struct is_convertible
    : public __bool_constant<__is_convertible(_From, _To)>
    { };
# 1606 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _ToElementType, typename _FromElementType>
    using __is_array_convertible
      = is_convertible<_FromElementType(*)[], _ToElementType(*)[]>;
# 1666 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++14-extensions"
  template<typename _Tp, typename... _Args>
    struct __is_nothrow_new_constructible_impl
    : __bool_constant<
 noexcept(::new(std::declval<void*>()) _Tp(std::declval<_Args>()...))
      >
    { };

  template<typename _Tp, typename... _Args>
    constexpr bool __is_nothrow_new_constructible
      = __and_<is_constructible<_Tp, _Args...>,
        __is_nothrow_new_constructible_impl<_Tp, _Args...>>::value;
#pragma GCC diagnostic pop




  template<typename _Tp>
    struct remove_const
    { using type = _Tp; };

  template<typename _Tp>
    struct remove_const<_Tp const>
    { using type = _Tp; };


  template<typename _Tp>
    struct remove_volatile
    { using type = _Tp; };

  template<typename _Tp>
    struct remove_volatile<_Tp volatile>
    { using type = _Tp; };



  template<typename _Tp>
    struct remove_cv
    { using type = __remove_cv(_Tp); };
# 1725 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct add_const
    { using type = _Tp const; };


  template<typename _Tp>
    struct add_volatile
    { using type = _Tp volatile; };


  template<typename _Tp>
    struct add_cv
    { using type = _Tp const volatile; };



  template<typename _Tp>
    using remove_const_t = typename remove_const<_Tp>::type;


  template<typename _Tp>
    using remove_volatile_t = typename remove_volatile<_Tp>::type;


  template<typename _Tp>
    using remove_cv_t = typename remove_cv<_Tp>::type;


  template<typename _Tp>
    using add_const_t = typename add_const<_Tp>::type;


  template<typename _Tp>
    using add_volatile_t = typename add_volatile<_Tp>::type;


  template<typename _Tp>
    using add_cv_t = typename add_cv<_Tp>::type;






  template<typename _Tp>
    struct remove_reference
    { using type = __remove_reference(_Tp); };
# 1787 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct add_lvalue_reference
    { using type = __add_lval_ref_t<_Tp>; };


  template<typename _Tp>
    struct add_rvalue_reference
    { using type = __add_rval_ref_t<_Tp>; };



  template<typename _Tp>
    using remove_reference_t = typename remove_reference<_Tp>::type;


  template<typename _Tp>
    using add_lvalue_reference_t = typename add_lvalue_reference<_Tp>::type;


  template<typename _Tp>
    using add_rvalue_reference_t = typename add_rvalue_reference<_Tp>::type;







  template<typename _Unqualified, bool _IsConst, bool _IsVol>
    struct __cv_selector;

  template<typename _Unqualified>
    struct __cv_selector<_Unqualified, false, false>
    { using __type = _Unqualified; };

  template<typename _Unqualified>
    struct __cv_selector<_Unqualified, false, true>
    { using __type = volatile _Unqualified; };

  template<typename _Unqualified>
    struct __cv_selector<_Unqualified, true, false>
    { using __type = const _Unqualified; };

  template<typename _Unqualified>
    struct __cv_selector<_Unqualified, true, true>
    { using __type = const volatile _Unqualified; };

  template<typename _Qualified, typename _Unqualified,
    bool _IsConst = is_const<_Qualified>::value,
    bool _IsVol = is_volatile<_Qualified>::value>
    class __match_cv_qualifiers
    {
      using __match = __cv_selector<_Unqualified, _IsConst, _IsVol>;

    public:
      using __type = typename __match::__type;
    };


  template<typename _Tp>
    struct __make_unsigned
    { using __type = _Tp; };

  template<>
    struct __make_unsigned<char>
    { using __type = unsigned char; };

  template<>
    struct __make_unsigned<signed char>
    { using __type = unsigned char; };

  template<>
    struct __make_unsigned<short>
    { using __type = unsigned short; };

  template<>
    struct __make_unsigned<int>
    { using __type = unsigned int; };

  template<>
    struct __make_unsigned<long>
    { using __type = unsigned long; };

  template<>
    struct __make_unsigned<long long>
    { using __type = unsigned long long; };
# 1900 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp,
    bool _IsInt = is_integral<_Tp>::value,
    bool _IsEnum = __is_enum(_Tp)>
    class __make_unsigned_selector;

  template<typename _Tp>
    class __make_unsigned_selector<_Tp, true, false>
    {
      using __unsigned_type
 = typename __make_unsigned<__remove_cv_t<_Tp>>::__type;

    public:
      using __type
 = typename __match_cv_qualifiers<_Tp, __unsigned_type>::__type;
    };

  class __make_unsigned_selector_base
  {
  protected:
    template<typename...> struct _List { };

    template<typename _Tp, typename... _Up>
      struct _List<_Tp, _Up...> : _List<_Up...>
      { static constexpr size_t __size = sizeof(_Tp); };

    template<size_t _Sz, typename _Tp, bool = (_Sz <= _Tp::__size)>
      struct __select;

    template<size_t _Sz, typename _Uint, typename... _UInts>
      struct __select<_Sz, _List<_Uint, _UInts...>, true>
      { using __type = _Uint; };

    template<size_t _Sz, typename _Uint, typename... _UInts>
      struct __select<_Sz, _List<_Uint, _UInts...>, false>
      : __select<_Sz, _List<_UInts...>>
      { };
  };


  template<typename _Tp>
    class __make_unsigned_selector<_Tp, false, true>
    : __make_unsigned_selector_base
    {

      using _UInts = _List<unsigned char, unsigned short, unsigned int,
      unsigned long, unsigned long long>;

      using __unsigned_type = typename __select<sizeof(_Tp), _UInts>::__type;

    public:
      using __type
 = typename __match_cv_qualifiers<_Tp, __unsigned_type>::__type;
    };





  template<>
    struct __make_unsigned<wchar_t>
    {
      using __type
 = typename __make_unsigned_selector<wchar_t, false, true>::__type;
    };
# 1974 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<>
    struct __make_unsigned<char16_t>
    {
      using __type
 = typename __make_unsigned_selector<char16_t, false, true>::__type;
    };

  template<>
    struct __make_unsigned<char32_t>
    {
      using __type
 = typename __make_unsigned_selector<char32_t, false, true>::__type;
    };






  template<typename _Tp>
    struct make_unsigned
    { using type = typename __make_unsigned_selector<_Tp>::__type; };


  template<> struct make_unsigned<bool>;
  template<> struct make_unsigned<bool const>;
  template<> struct make_unsigned<bool volatile>;
  template<> struct make_unsigned<bool const volatile>;




  template<typename _Tp>
    struct __make_signed
    { using __type = _Tp; };

  template<>
    struct __make_signed<char>
    { using __type = signed char; };

  template<>
    struct __make_signed<unsigned char>
    { using __type = signed char; };

  template<>
    struct __make_signed<unsigned short>
    { using __type = signed short; };

  template<>
    struct __make_signed<unsigned int>
    { using __type = signed int; };

  template<>
    struct __make_signed<unsigned long>
    { using __type = signed long; };

  template<>
    struct __make_signed<unsigned long long>
    { using __type = signed long long; };
# 2060 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp,
    bool _IsInt = is_integral<_Tp>::value,
    bool _IsEnum = __is_enum(_Tp)>
    class __make_signed_selector;

  template<typename _Tp>
    class __make_signed_selector<_Tp, true, false>
    {
      using __signed_type
 = typename __make_signed<__remove_cv_t<_Tp>>::__type;

    public:
      using __type
 = typename __match_cv_qualifiers<_Tp, __signed_type>::__type;
    };


  template<typename _Tp>
    class __make_signed_selector<_Tp, false, true>
    {
      using __unsigned_type = typename __make_unsigned_selector<_Tp>::__type;

    public:
      using __type = typename __make_signed_selector<__unsigned_type>::__type;
    };





  template<>
    struct __make_signed<wchar_t>
    {
      using __type
 = typename __make_signed_selector<wchar_t, false, true>::__type;
    };
# 2106 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<>
    struct __make_signed<char16_t>
    {
      using __type
 = typename __make_signed_selector<char16_t, false, true>::__type;
    };

  template<>
    struct __make_signed<char32_t>
    {
      using __type
 = typename __make_signed_selector<char32_t, false, true>::__type;
    };






  template<typename _Tp>
    struct make_signed
    { using type = typename __make_signed_selector<_Tp>::__type; };


  template<> struct make_signed<bool>;
  template<> struct make_signed<bool const>;
  template<> struct make_signed<bool volatile>;
  template<> struct make_signed<bool const volatile>;



  template<typename _Tp>
    using make_signed_t = typename make_signed<_Tp>::type;


  template<typename _Tp>
    using make_unsigned_t = typename make_unsigned<_Tp>::type;






  template<typename _Tp>
    struct remove_extent
    { using type = __remove_extent(_Tp); };
# 2168 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct remove_all_extents
    { using type = __remove_all_extents(_Tp); };
# 2187 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    using remove_extent_t = typename remove_extent<_Tp>::type;


  template<typename _Tp>
    using remove_all_extents_t = typename remove_all_extents<_Tp>::type;






  template<typename _Tp>
    struct remove_pointer
    { using type = __remove_pointer(_Tp); };
# 2219 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct add_pointer
    { using type = __add_pointer(_Tp); };
# 2247 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    using remove_pointer_t = typename remove_pointer<_Tp>::type;


  template<typename _Tp>
    using add_pointer_t = typename add_pointer<_Tp>::type;





  struct __attribute__((__aligned__)) __aligned_storage_max_align_t
  { };

  constexpr size_t
  __aligned_storage_default_alignment([[__maybe_unused__]] size_t __len)
  {
# 2278 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
    return alignof(__aligned_storage_max_align_t);

  }
# 2314 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<size_t _Len,
    size_t _Align = __aligned_storage_default_alignment(_Len)>
    struct
   
    aligned_storage
    {
      struct type
      {
 alignas(_Align) unsigned char __data[_Len];
      };
    };

  template <typename... _Types>
    struct __strictest_alignment
    {
      static const size_t _S_alignment = 0;
      static const size_t _S_size = 0;
    };

  template <typename _Tp, typename... _Types>
    struct __strictest_alignment<_Tp, _Types...>
    {
      static const size_t _S_alignment =
        alignof(_Tp) > __strictest_alignment<_Types...>::_S_alignment
 ? alignof(_Tp) : __strictest_alignment<_Types...>::_S_alignment;
      static const size_t _S_size =
        sizeof(_Tp) > __strictest_alignment<_Types...>::_S_size
 ? sizeof(_Tp) : __strictest_alignment<_Types...>::_S_size;
    };

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
# 2359 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template <size_t _Len, typename... _Types>
    struct
   
    aligned_union
    {
    private:
      static_assert(sizeof...(_Types) != 0, "At least one type is required");

      using __strictest = __strictest_alignment<_Types...>;
      static const size_t _S_len = _Len > __strictest::_S_size
 ? _Len : __strictest::_S_size;
    public:

      static const size_t alignment_value = __strictest::_S_alignment;

      using type = typename aligned_storage<_S_len, alignment_value>::type;
    };

  template <size_t _Len, typename... _Types>
    const size_t aligned_union<_Len, _Types...>::alignment_value;
#pragma GCC diagnostic pop




  template<typename _Tp>
    struct decay
    { using type = __decay(_Tp); };
# 2424 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct __strip_reference_wrapper
    {
      using __type = _Tp;
    };

  template<typename _Tp>
    struct __strip_reference_wrapper<reference_wrapper<_Tp> >
    {
      using __type = _Tp&;
    };


  template<typename _Tp>
    using __decay_t = typename decay<_Tp>::type;

  template<typename _Tp>
    using __decay_and_strip = __strip_reference_wrapper<__decay_t<_Tp>>;





  template<typename... _Cond>
    using _Require = __enable_if_t<__and_<_Cond...>::value>;


  template<typename _Tp>
    using __remove_cvref_t
     = typename remove_cv<typename remove_reference<_Tp>::type>::type;




  template<bool _Cond, typename _Iftrue, typename _Iffalse>
    struct conditional
    { using type = _Iftrue; };


  template<typename _Iftrue, typename _Iffalse>
    struct conditional<false, _Iftrue, _Iffalse>
    { using type = _Iffalse; };


  template<typename... _Tp>
    struct common_type;
# 2480 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct __success_type
    { using type = _Tp; };

  struct __failure_type
  { };

  struct __do_common_type_impl
  {
    template<typename _Tp, typename _Up>
      using __cond_t
 = decltype(true ? std::declval<_Tp>() : std::declval<_Up>());



    template<typename _Tp, typename _Up>
      static __success_type<__decay_t<__cond_t<_Tp, _Up>>>
      _S_test(int);
# 2507 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
    template<typename, typename>
      static __failure_type
      _S_test_2(...);

    template<typename _Tp, typename _Up>
      static decltype(_S_test_2<_Tp, _Up>(0))
      _S_test(...);
  };


  template<>
    struct common_type<>
    { };


  template<typename _Tp0>
    struct common_type<_Tp0>
    : public common_type<_Tp0, _Tp0>
    { };


  template<typename _Tp1, typename _Tp2,
    typename _Dp1 = __decay_t<_Tp1>, typename _Dp2 = __decay_t<_Tp2>>
    struct __common_type_impl
    {


      using type = common_type<_Dp1, _Dp2>;
    };

  template<typename _Tp1, typename _Tp2>
    struct __common_type_impl<_Tp1, _Tp2, _Tp1, _Tp2>
    : private __do_common_type_impl
    {


      using type = decltype(_S_test<_Tp1, _Tp2>(0));
    };


  template<typename _Tp1, typename _Tp2>
    struct common_type<_Tp1, _Tp2>
    : public __common_type_impl<_Tp1, _Tp2>::type
    { };

  template<typename...>
    struct __common_type_pack
    { };

  template<typename, typename, typename = void>
    struct __common_type_fold;


  template<typename _Tp1, typename _Tp2, typename... _Rp>
    struct common_type<_Tp1, _Tp2, _Rp...>
    : public __common_type_fold<common_type<_Tp1, _Tp2>,
    __common_type_pack<_Rp...>>
    { };




  template<typename _CTp, typename... _Rp>
    struct __common_type_fold<_CTp, __common_type_pack<_Rp...>,
         __void_t<typename _CTp::type>>
    : public common_type<typename _CTp::type, _Rp...>
    { };


  template<typename _CTp, typename _Rp>
    struct __common_type_fold<_CTp, _Rp, void>
    { };

  template<typename _Tp, bool = __is_enum(_Tp)>
    struct __underlying_type_impl
    {
      using type = __underlying_type(_Tp);
    };

  template<typename _Tp>
    struct __underlying_type_impl<_Tp, false>
    { };



  template<typename _Tp>
    struct underlying_type
    : public __underlying_type_impl<_Tp>
    { };


  template<typename _Tp>
    struct __declval_protector
    {
      static const bool __stop = false;
    };






  template<typename _Tp>
    auto declval() noexcept -> decltype(__declval<_Tp>(0))
    {
      static_assert(__declval_protector<_Tp>::__stop,
      "declval() must not be used!");
      return __declval<_Tp>(0);
    }


  template<typename _Signature>
    struct result_of;




  struct __invoke_memfun_ref { };
  struct __invoke_memfun_deref { };
  struct __invoke_memobj_ref { };
  struct __invoke_memobj_deref { };
  struct __invoke_other { };


  template<typename _Tp, typename _Tag>
    struct __result_of_success : __success_type<_Tp>
    { using __invoke_type = _Tag; };


  struct __result_of_memfun_ref_impl
  {
    template<typename _Fp, typename _Tp1, typename... _Args>
      static __result_of_success<decltype(
      (std::declval<_Tp1>().*std::declval<_Fp>())(std::declval<_Args>()...)
      ), __invoke_memfun_ref> _S_test(int);

    template<typename...>
      static __failure_type _S_test(...);
  };

  template<typename _MemPtr, typename _Arg, typename... _Args>
    struct __result_of_memfun_ref
    : private __result_of_memfun_ref_impl
    {
      using type = decltype(_S_test<_MemPtr, _Arg, _Args...>(0));
    };


  struct __result_of_memfun_deref_impl
  {
    template<typename _Fp, typename _Tp1, typename... _Args>
      static __result_of_success<decltype(
      ((*std::declval<_Tp1>()).*std::declval<_Fp>())(std::declval<_Args>()...)
      ), __invoke_memfun_deref> _S_test(int);

    template<typename...>
      static __failure_type _S_test(...);
  };

  template<typename _MemPtr, typename _Arg, typename... _Args>
    struct __result_of_memfun_deref
    : private __result_of_memfun_deref_impl
    {
      using type = decltype(_S_test<_MemPtr, _Arg, _Args...>(0));
    };


  struct __result_of_memobj_ref_impl
  {
    template<typename _Fp, typename _Tp1>
      static __result_of_success<decltype(
      std::declval<_Tp1>().*std::declval<_Fp>()
      ), __invoke_memobj_ref> _S_test(int);

    template<typename, typename>
      static __failure_type _S_test(...);
  };

  template<typename _MemPtr, typename _Arg>
    struct __result_of_memobj_ref
    : private __result_of_memobj_ref_impl
    {
      using type = decltype(_S_test<_MemPtr, _Arg>(0));
    };


  struct __result_of_memobj_deref_impl
  {
    template<typename _Fp, typename _Tp1>
      static __result_of_success<decltype(
      (*std::declval<_Tp1>()).*std::declval<_Fp>()
      ), __invoke_memobj_deref> _S_test(int);

    template<typename, typename>
      static __failure_type _S_test(...);
  };

  template<typename _MemPtr, typename _Arg>
    struct __result_of_memobj_deref
    : private __result_of_memobj_deref_impl
    {
      using type = decltype(_S_test<_MemPtr, _Arg>(0));
    };

  template<typename _MemPtr, typename _Arg>
    struct __result_of_memobj;

  template<typename _Res, typename _Class, typename _Arg>
    struct __result_of_memobj<_Res _Class::*, _Arg>
    {
      using _Argval = __remove_cvref_t<_Arg>;
      using _MemPtr = _Res _Class::*;
      using type = typename __conditional_t<__or_<is_same<_Argval, _Class>,
        is_base_of<_Class, _Argval>>::value,
        __result_of_memobj_ref<_MemPtr, _Arg>,
        __result_of_memobj_deref<_MemPtr, _Arg>
      >::type;
    };

  template<typename _MemPtr, typename _Arg, typename... _Args>
    struct __result_of_memfun;

  template<typename _Res, typename _Class, typename _Arg, typename... _Args>
    struct __result_of_memfun<_Res _Class::*, _Arg, _Args...>
    {
      using _Argval = typename remove_reference<_Arg>::type;
      using _MemPtr = _Res _Class::*;
      using type = typename __conditional_t<is_base_of<_Class, _Argval>::value,
        __result_of_memfun_ref<_MemPtr, _Arg, _Args...>,
        __result_of_memfun_deref<_MemPtr, _Arg, _Args...>
      >::type;
    };






  template<typename _Tp, typename _Up = __remove_cvref_t<_Tp>>
    struct __inv_unwrap
    {
      using type = _Tp;
    };

  template<typename _Tp, typename _Up>
    struct __inv_unwrap<_Tp, reference_wrapper<_Up>>
    {
      using type = _Up&;
    };

  template<bool, bool, typename _Functor, typename... _ArgTypes>
    struct __result_of_impl
    {
      using type = __failure_type;
    };

  template<typename _MemPtr, typename _Arg>
    struct __result_of_impl<true, false, _MemPtr, _Arg>
    : public __result_of_memobj<__decay_t<_MemPtr>,
    typename __inv_unwrap<_Arg>::type>
    { };

  template<typename _MemPtr, typename _Arg, typename... _Args>
    struct __result_of_impl<false, true, _MemPtr, _Arg, _Args...>
    : public __result_of_memfun<__decay_t<_MemPtr>,
    typename __inv_unwrap<_Arg>::type, _Args...>
    { };


  struct __result_of_other_impl
  {
    template<typename _Fn, typename... _Args>
      static __result_of_success<decltype(
      std::declval<_Fn>()(std::declval<_Args>()...)
      ), __invoke_other> _S_test(int);

    template<typename...>
      static __failure_type _S_test(...);
  };

  template<typename _Functor, typename... _ArgTypes>
    struct __result_of_impl<false, false, _Functor, _ArgTypes...>
    : private __result_of_other_impl
    {
      using type = decltype(_S_test<_Functor, _ArgTypes...>(0));
    };


  template<typename _Functor, typename... _ArgTypes>
    struct __invoke_result
    : public __result_of_impl<
        is_member_object_pointer<
          typename remove_reference<_Functor>::type
        >::value,
        is_member_function_pointer<
          typename remove_reference<_Functor>::type
        >::value,
 _Functor, _ArgTypes...
      >::type
    { };


  template<typename _Fn, typename... _Args>
    using __invoke_result_t = typename __invoke_result<_Fn, _Args...>::type;


  template<typename _Functor, typename... _ArgTypes>
    struct result_of<_Functor(_ArgTypes...)>
    : public __invoke_result<_Functor, _ArgTypes...>
    { } ;


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"

  template<size_t _Len,
    size_t _Align = __aligned_storage_default_alignment(_Len)>
    using aligned_storage_t = typename aligned_storage<_Len, _Align>::type;

  template <size_t _Len, typename... _Types>
    using aligned_union_t = typename aligned_union<_Len, _Types...>::type;
#pragma GCC diagnostic pop


  template<typename _Tp>
    using decay_t = typename decay<_Tp>::type;


  template<bool _Cond, typename _Tp = void>
    using enable_if_t = typename enable_if<_Cond, _Tp>::type;


  template<bool _Cond, typename _Iftrue, typename _Iffalse>
    using conditional_t = typename conditional<_Cond, _Iftrue, _Iffalse>::type;


  template<typename... _Tp>
    using common_type_t = typename common_type<_Tp...>::type;


  template<typename _Tp>
    using underlying_type_t = typename underlying_type<_Tp>::type;


  template<typename _Tp>
    using result_of_t = typename result_of<_Tp>::type;
# 2884 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Default, typename _AlwaysVoid,
    template<typename...> class _Op, typename... _Args>
    struct __detector
    {
      using type = _Default;
      using __is_detected = false_type;
    };


  template<typename _Default, template<typename...> class _Op,
     typename... _Args>
    struct __detector<_Default, __void_t<_Op<_Args...>>, _Op, _Args...>
    {
      using type = _Op<_Args...>;
      using __is_detected = true_type;
    };

  template<typename _Default, template<typename...> class _Op,
    typename... _Args>
    using __detected_or = __detector<_Default, void, _Op, _Args...>;



  template<typename _Default, template<typename...> class _Op,
    typename... _Args>
    using __detected_or_t
      = typename __detected_or<_Default, _Op, _Args...>::type;
# 2926 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template <typename _Tp>
    struct __is_swappable;

  template <typename _Tp>
    struct __is_nothrow_swappable;

  template<typename>
    struct __is_tuple_like_impl : false_type
    { };


  template<typename _Tp>
    struct __is_tuple_like
    : public __is_tuple_like_impl<__remove_cvref_t<_Tp>>::type
    { };


  template<typename _Tp>
   
    inline
    _Require<__not_<__is_tuple_like<_Tp>>,
      is_move_constructible<_Tp>,
      is_move_assignable<_Tp>>
    swap(_Tp&, _Tp&)
    noexcept(__and_<is_nothrow_move_constructible<_Tp>,
             is_nothrow_move_assignable<_Tp>>::value);

  template<typename _Tp, size_t _Nm>
   
    inline
    __enable_if_t<__is_swappable<_Tp>::value>
    swap(_Tp (&__a)[_Nm], _Tp (&__b)[_Nm])
    noexcept(__is_nothrow_swappable<_Tp>::value);


  namespace __swappable_details {
    using std::swap;

    struct __do_is_swappable_impl
    {
      template<typename _Tp, typename
               = decltype(swap(std::declval<_Tp&>(), std::declval<_Tp&>()))>
        static true_type __test(int);

      template<typename>
        static false_type __test(...);
    };

    struct __do_is_nothrow_swappable_impl
    {
      template<typename _Tp>
        static __bool_constant<
          noexcept(swap(std::declval<_Tp&>(), std::declval<_Tp&>()))
        > __test(int);

      template<typename>
        static false_type __test(...);
    };

  }

  template<typename _Tp>
    struct __is_swappable_impl
    : public __swappable_details::__do_is_swappable_impl
    {
      using type = decltype(__test<_Tp>(0));
    };

  template<typename _Tp>
    struct __is_nothrow_swappable_impl
    : public __swappable_details::__do_is_nothrow_swappable_impl
    {
      using type = decltype(__test<_Tp>(0));
    };

  template<typename _Tp>
    struct __is_swappable
    : public __is_swappable_impl<_Tp>::type
    { };

  template<typename _Tp>
    struct __is_nothrow_swappable
    : public __is_nothrow_swappable_impl<_Tp>::type
    { };
# 3148 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Result, typename _Ret,
    bool = is_void<_Ret>::value, typename = void>
    struct __is_invocable_impl
    : false_type
    {
      using __nothrow_conv = false_type;
    };


  template<typename _Result, typename _Ret>
    struct __is_invocable_impl<_Result, _Ret,
                                true,
          __void_t<typename _Result::type>>
    : true_type
    {
      using __nothrow_conv = true_type;
    };

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wctor-dtor-privacy"

  template<typename _Result, typename _Ret>
    struct __is_invocable_impl<_Result, _Ret,
                                false,
          __void_t<typename _Result::type>>
    {
    private:

      using _Res_t = typename _Result::type;



      static _Res_t _S_get() noexcept;


      template<typename _Tp>
 static void _S_conv(__type_identity_t<_Tp>) noexcept;


      template<typename _Tp,
        bool _Nothrow = noexcept(_S_conv<_Tp>(_S_get())),
        typename = decltype(_S_conv<_Tp>(_S_get())),

        bool _Dangle = __reference_converts_from_temporary(_Tp, _Res_t)



       >
 static __bool_constant<_Nothrow && !_Dangle>
 _S_test(int);

      template<typename _Tp, bool = false>
 static false_type
 _S_test(...);

    public:

      using type = decltype(_S_test<_Ret, true>(1));


      using __nothrow_conv = decltype(_S_test<_Ret>(1));
    };
#pragma GCC diagnostic pop

  template<typename _Fn, typename... _ArgTypes>
    struct __is_invocable
    : __is_invocable_impl<__invoke_result<_Fn, _ArgTypes...>, void>::type
    { };

  template<typename _Fn, typename _Tp, typename... _Args>
    constexpr bool __call_is_nt(__invoke_memfun_ref)
    {
      using _Up = typename __inv_unwrap<_Tp>::type;
      return noexcept((std::declval<_Up>().*std::declval<_Fn>())(
     std::declval<_Args>()...));
    }

  template<typename _Fn, typename _Tp, typename... _Args>
    constexpr bool __call_is_nt(__invoke_memfun_deref)
    {
      return noexcept(((*std::declval<_Tp>()).*std::declval<_Fn>())(
     std::declval<_Args>()...));
    }

  template<typename _Fn, typename _Tp>
    constexpr bool __call_is_nt(__invoke_memobj_ref)
    {
      using _Up = typename __inv_unwrap<_Tp>::type;
      return noexcept(std::declval<_Up>().*std::declval<_Fn>());
    }

  template<typename _Fn, typename _Tp>
    constexpr bool __call_is_nt(__invoke_memobj_deref)
    {
      return noexcept((*std::declval<_Tp>()).*std::declval<_Fn>());
    }

  template<typename _Fn, typename... _Args>
    constexpr bool __call_is_nt(__invoke_other)
    {
      return noexcept(std::declval<_Fn>()(std::declval<_Args>()...));
    }

  template<typename _Result, typename _Fn, typename... _Args>
    struct __call_is_nothrow
    : __bool_constant<
 std::__call_is_nt<_Fn, _Args...>(typename _Result::__invoke_type{})
      >
    { };

  template<typename _Fn, typename... _Args>
    using __call_is_nothrow_
      = __call_is_nothrow<__invoke_result<_Fn, _Args...>, _Fn, _Args...>;


  template<typename _Fn, typename... _Args>
    struct __is_nothrow_invocable
    : __and_<__is_invocable<_Fn, _Args...>,
             __call_is_nothrow_<_Fn, _Args...>>::type
    { };

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wctor-dtor-privacy"
  struct __nonesuchbase {};
  struct __nonesuch : private __nonesuchbase {
    ~__nonesuch() = delete;
    __nonesuch(__nonesuch const&) = delete;
    void operator=(__nonesuch const&) = delete;
  };
#pragma GCC diagnostic pop
# 4189 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3

}
}
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 2 3


namespace std __attribute__ ((__visibility__ ("default")))
{







  template<typename _Tp>
    __attribute__((__always_inline__))
    inline constexpr _Tp*
    __addressof(_Tp& __r) noexcept
    { return __builtin_addressof(__r); }
# 69 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
    [[__nodiscard__,__gnu__::__always_inline__]]
    constexpr _Tp&&
    forward(typename std::remove_reference<_Tp>::type& __t) noexcept
    { return static_cast<_Tp&&>(__t); }
# 82 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
    [[__nodiscard__,__gnu__::__always_inline__]]
    constexpr _Tp&&
    forward(typename std::remove_reference<_Tp>::type&& __t) noexcept
    {
      static_assert(!std::is_lvalue_reference<_Tp>::value,
   "std::forward must not be used to convert an rvalue to an lvalue");
      return static_cast<_Tp&&>(__t);
    }
# 135 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
    [[__nodiscard__,__gnu__::__always_inline__]]
    constexpr typename std::remove_reference<_Tp>::type&&
    move(_Tp&& __t) noexcept
    { return static_cast<typename std::remove_reference<_Tp>::type&&>(__t); }


  template<typename _Tp>
    struct __move_if_noexcept_cond
    : public __and_<__not_<is_nothrow_move_constructible<_Tp>>,
                    is_copy_constructible<_Tp>>::type { };
# 156 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
    [[__nodiscard__,__gnu__::__always_inline__]]
    constexpr
    __conditional_t<__move_if_noexcept_cond<_Tp>::value, const _Tp&, _Tp&&>
    move_if_noexcept(_Tp& __x) noexcept
    { return std::move(__x); }
# 173 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
    [[__nodiscard__,__gnu__::__always_inline__]]
    inline _Tp*
    addressof(_Tp& __r) noexcept
    { return std::__addressof(__r); }



  template<typename _Tp>
    const _Tp* addressof(const _Tp&&) = delete;


  template <typename _Tp, typename _Up = _Tp>
   
    inline _Tp
    __exchange(_Tp& __obj, _Up&& __new_val)
    {
      _Tp __old_val = std::move(__obj);
      __obj = std::forward<_Up>(__new_val);
      return __old_val;
    }
# 217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
   
    inline

    typename enable_if<__and_<__not_<__is_tuple_like<_Tp>>,
         is_move_constructible<_Tp>,
         is_move_assignable<_Tp>>::value>::type



    swap(_Tp& __a, _Tp& __b)
    noexcept(__and_<is_nothrow_move_constructible<_Tp>, is_nothrow_move_assignable<_Tp>>::value)

    {




      _Tp __tmp = std::move(__a);
      __a = std::move(__b);
      __b = std::move(__tmp);
    }




  template<typename _Tp, size_t _Nm>
   
    inline

    typename enable_if<__is_swappable<_Tp>::value>::type



    swap(_Tp (&__a)[_Nm], _Tp (&__b)[_Nm])
    noexcept(__is_nothrow_swappable<_Tp>::value)
    {
      for (size_t __n = 0; __n < _Nm; ++__n)
 swap(__a[__n], __b[__n]);
    }



}
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 2 3




namespace std __attribute__ ((__visibility__ ("default")))
{

# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
  template<typename _Tp>
    class __new_allocator
    {
    public:
      typedef _Tp value_type;
      typedef std::size_t size_type;
      typedef std::ptrdiff_t difference_type;

      typedef _Tp* pointer;
      typedef const _Tp* const_pointer;
      typedef _Tp& reference;
      typedef const _Tp& const_reference;

      template<typename _Tp1>
 struct rebind
 { typedef __new_allocator<_Tp1> other; };





      typedef std::true_type propagate_on_container_move_assignment;


      __attribute__((__always_inline__))
     
      __new_allocator() noexcept { }

      __attribute__((__always_inline__))
     
      __new_allocator(const __new_allocator&) noexcept { }

      template<typename _Tp1>
 __attribute__((__always_inline__))

 __new_allocator(const __new_allocator<_Tp1>&) noexcept { }


      __new_allocator& operator=(const __new_allocator&) = default;



      ~__new_allocator() noexcept { }

      pointer
      address(reference __x) const noexcept
      { return std::__addressof(__x); }

      const_pointer
      address(const_reference __x) const noexcept
      { return std::__addressof(__x); }
# 125 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
      [[__nodiscard__]] _Tp*
      allocate(size_type __n, const void* = static_cast<const void*>(0))
      {



 static_assert(sizeof(_Tp) != 0, "cannot allocate incomplete types");


 if (__builtin_expect(__n > this->_M_max_size(), false))
   {


     if (__n > (std::size_t(-1) / sizeof(_Tp)))
       std::__throw_bad_array_new_length();
     std::__throw_bad_alloc();
   }
# 151 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
 return static_cast<_Tp*>(__builtin_operator_new(__n * sizeof(_Tp)));
      }


      void
      deallocate(_Tp* __p, size_type __n __attribute__ ((__unused__)))
      {
# 172 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
 __builtin_operator_delete((__p), (__n) * sizeof(_Tp));
      }






      __attribute__((__always_inline__))
      size_type
      max_size() const noexcept
      { return _M_max_size(); }


      template<typename _Up, typename... _Args>
 __attribute__((__always_inline__))
 void
 construct(_Up* __p, _Args&&... __args)
 noexcept(__is_nothrow_new_constructible<_Up, _Args...>)
 { ::new((void *)__p) _Up(std::forward<_Args>(__args)...); }

      template<typename _Up>
 __attribute__((__always_inline__))
 void
 destroy(_Up* __p)
 noexcept(std::is_nothrow_destructible<_Up>::value)
 { __p->~_Up(); }
# 213 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
      template<typename _Up>
 friend __attribute__((__always_inline__)) bool
 operator==(const __new_allocator&, const __new_allocator<_Up>&)
 noexcept
 { return true; }


      template<typename _Up>
 friend __attribute__((__always_inline__)) bool
 operator!=(const __new_allocator&, const __new_allocator<_Up>&)
 noexcept
 { return false; }


    private:
      __attribute__((__always_inline__))
      constexpr size_type
      _M_max_size() const noexcept
      {

 return std::size_t(0x7fffffffffffffffL) / sizeof(_Tp);



      }
    };


}
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++allocator.h" 2 3


namespace std
{
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++allocator.h" 3
  template<typename _Tp>
    using __allocator_base = __new_allocator<_Tp>;
}
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 2 3





#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"

namespace std __attribute__ ((__visibility__ ("default")))
{

# 75 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
  template<>
    class allocator<void>
    {
    public:
      typedef void value_type;
      typedef size_t size_type;
      typedef ptrdiff_t difference_type;



      typedef void* pointer;
      typedef const void* const_pointer;

      template<typename _Tp1>
 struct rebind
 { typedef allocator<_Tp1> other; };





      using propagate_on_container_move_assignment = true_type;


      using is_always_equal

 = true_type;
# 120 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
    };
# 132 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
  template<typename _Tp>
    class allocator : public __allocator_base<_Tp>
    {
    public:
      typedef _Tp value_type;
      typedef size_t size_type;
      typedef ptrdiff_t difference_type;



      typedef _Tp* pointer;
      typedef const _Tp* const_pointer;
      typedef _Tp& reference;
      typedef const _Tp& const_reference;

      template<typename _Tp1>
 struct rebind
 { typedef allocator<_Tp1> other; };





      using propagate_on_container_move_assignment = true_type;


      using is_always_equal

 = true_type;





      __attribute__((__always_inline__))
     
      allocator() noexcept { }

      __attribute__((__always_inline__))
     
      allocator(const allocator& __a) noexcept
      : __allocator_base<_Tp>(__a) { }



      allocator& operator=(const allocator&) = default;


      template<typename _Tp1>
 __attribute__((__always_inline__))

 allocator(const allocator<_Tp1>&) noexcept { }

      __attribute__((__always_inline__))



      ~allocator() noexcept { }
# 219 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
      friend __attribute__((__always_inline__))
      bool
      operator==(const allocator&, const allocator&) noexcept
      { return true; }


      friend __attribute__((__always_inline__))
      bool
      operator!=(const allocator&, const allocator&) noexcept
      { return false; }



    };






  template<typename _T1, typename _T2>
    __attribute__((__always_inline__))
    inline bool
    operator==(const allocator<_T1>&, const allocator<_T2>&)
    noexcept
    { return true; }


  template<typename _T1, typename _T2>
    __attribute__((__always_inline__))
    inline bool
    operator!=(const allocator<_T1>&, const allocator<_T2>&)
    noexcept
    { return false; }






  template<typename _Tp>
    class allocator<const _Tp>
    {
    public:
      typedef _Tp value_type;
      allocator() { }
      template<typename _Up> allocator(const allocator<_Up>&) { }
    };

  template<typename _Tp>
    class allocator<volatile _Tp>
    {
    public:
      typedef _Tp value_type;
      allocator() { }
      template<typename _Up> allocator(const allocator<_Up>&) { }
    };

  template<typename _Tp>
    class allocator<const volatile _Tp>
    {
    public:
      typedef _Tp value_type;
      allocator() { }
      template<typename _Up> allocator(const allocator<_Up>&) { }
    };







  extern template class allocator<char>;
  extern template class allocator<wchar_t>;






}

#pragma GCC diagnostic pop
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 1 3
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 1 3
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 1 3
# 76 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 95 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  struct input_iterator_tag { };


  struct output_iterator_tag { };


  struct forward_iterator_tag : public input_iterator_tag { };



  struct bidirectional_iterator_tag : public forward_iterator_tag { };



  struct random_access_iterator_tag : public bidirectional_iterator_tag { };
# 127 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Category, typename _Tp, typename _Distance = ptrdiff_t,
           typename _Pointer = _Tp*, typename _Reference = _Tp&>
    struct iterator
    {

      typedef _Category iterator_category;

      typedef _Tp value_type;

      typedef _Distance difference_type;

      typedef _Pointer pointer;

      typedef _Reference reference;
    };
# 151 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Iterator>
    struct iterator_traits;




  template<typename _Iterator, typename = __void_t<>>
    struct __iterator_traits { };



  template<typename _Iterator>
    struct __iterator_traits<_Iterator,
        __void_t<typename _Iterator::iterator_category,
          typename _Iterator::value_type,
          typename _Iterator::difference_type,
          typename _Iterator::pointer,
          typename _Iterator::reference>>
    {
      typedef typename _Iterator::iterator_category iterator_category;
      typedef typename _Iterator::value_type value_type;
      typedef typename _Iterator::difference_type difference_type;
      typedef typename _Iterator::pointer pointer;
      typedef typename _Iterator::reference reference;
    };


  template<typename _Iterator>
    struct iterator_traits
    : public __iterator_traits<_Iterator> { };
# 211 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Tp>
    struct iterator_traits<_Tp*>
    {
      typedef random_access_iterator_tag iterator_category;
      typedef _Tp value_type;
      typedef ptrdiff_t difference_type;
      typedef _Tp* pointer;
      typedef _Tp& reference;
    };


  template<typename _Tp>
    struct iterator_traits<const _Tp*>
    {
      typedef random_access_iterator_tag iterator_category;
      typedef _Tp value_type;
      typedef ptrdiff_t difference_type;
      typedef const _Tp* pointer;
      typedef const _Tp& reference;
    };






  template<typename _Iter>
    __attribute__((__always_inline__))
    inline constexpr
    typename iterator_traits<_Iter>::iterator_category
    __iterator_category(const _Iter&)
    { return typename iterator_traits<_Iter>::iterator_category(); }




  template<typename _Iter>
    using __iter_category_t
      = typename iterator_traits<_Iter>::iterator_category;

  template<typename _InIter>
    using _RequireInputIter =
      __enable_if_t<is_convertible<__iter_category_t<_InIter>,
       input_iterator_tag>::value>;







  template<typename _It,
    typename _Cat = __iter_category_t<_It>>
    struct __is_random_access_iter
      : is_base_of<random_access_iterator_tag, _Cat>
    {
      typedef is_base_of<random_access_iterator_tag, _Cat> _Base;
      enum { __value = _Base::value };
    };








}
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 1 3
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/concept_check.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/concept_check.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wvariadic-macros"
# 86 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/concept_check.h" 3
#pragma GCC diagnostic pop
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/debug/assertions.h" 1 3
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 2 3


namespace std __attribute__ ((__visibility__ ("default")))
{




  template <typename> struct _List_iterator;
  template <typename> struct _List_const_iterator;


  template<typename _InputIterator>
    inline constexpr
    typename iterator_traits<_InputIterator>::difference_type
    __distance(_InputIterator __first, _InputIterator __last,
               input_iterator_tag)
    {

     

      typename iterator_traits<_InputIterator>::difference_type __n = 0;
      while (__first != __last)
 {
   ++__first;
   ++__n;
 }
      return __n;
    }

  template<typename _RandomAccessIterator>
    __attribute__((__always_inline__))
    inline constexpr
    typename iterator_traits<_RandomAccessIterator>::difference_type
    __distance(_RandomAccessIterator __first, _RandomAccessIterator __last,
               random_access_iterator_tag)
    {

     

      return __last - __first;
    }



  template<typename _Tp>
    ptrdiff_t
    __distance(std::_List_iterator<_Tp>,
        std::_List_iterator<_Tp>,
        input_iterator_tag);

  template<typename _Tp>
    ptrdiff_t
    __distance(std::_List_const_iterator<_Tp>,
        std::_List_const_iterator<_Tp>,
        input_iterator_tag);




  template<typename _OutputIterator>
    void
    __distance(_OutputIterator, _OutputIterator, output_iterator_tag) = delete;
# 146 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 3
  template<typename _InputIterator>
    [[__nodiscard__]] __attribute__((__always_inline__))
    inline
    typename iterator_traits<_InputIterator>::difference_type
    distance(_InputIterator __first, _InputIterator __last)
    {

      return std::__distance(__first, __last,
        std::__iterator_category(__first));
    }

  template<typename _InputIterator, typename _Distance>
    inline constexpr void
    __advance(_InputIterator& __i, _Distance __n, input_iterator_tag)
    {

     
      do { if (__builtin_expect(!bool(__n >= 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h", 163, __PRETTY_FUNCTION__, "__n >= 0"); } while (false);
      while (__n--)
 ++__i;
    }

  template<typename _BidirectionalIterator, typename _Distance>
    inline constexpr void
    __advance(_BidirectionalIterator& __i, _Distance __n,
       bidirectional_iterator_tag)
    {

     

      if (__n > 0)
        while (__n--)
   ++__i;
      else
        while (__n++)
   --__i;
    }

  template<typename _RandomAccessIterator, typename _Distance>
    inline constexpr void
    __advance(_RandomAccessIterator& __i, _Distance __n,
              random_access_iterator_tag)
    {

     

      if (__builtin_constant_p(__n) && __n == 1)
 ++__i;
      else if (__builtin_constant_p(__n) && __n == -1)
 --__i;
      else
 __i += __n;
    }



  template<typename _OutputIterator, typename _Distance>
    void
    __advance(_OutputIterator&, _Distance, output_iterator_tag) = delete;
# 219 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 3
  template<typename _InputIterator, typename _Distance>
    __attribute__((__always_inline__))
    inline void
    advance(_InputIterator& __i, _Distance __n)
    {

      typename iterator_traits<_InputIterator>::difference_type __d = __n;
      std::__advance(__i, __d, std::__iterator_category(__i));
    }



  template<typename _InputIterator>
    [[__nodiscard__]] [[__gnu__::__always_inline__]]
    inline _InputIterator
    next(_InputIterator __x, typename
  iterator_traits<_InputIterator>::difference_type __n = 1)
    {

     
      std::advance(__x, __n);
      return __x;
    }

  template<typename _BidirectionalIterator>
    [[__nodiscard__]] [[__gnu__::__always_inline__]]
    inline _BidirectionalIterator
    prev(_BidirectionalIterator __x, typename
  iterator_traits<_BidirectionalIterator>::difference_type __n = 1)
    {

     

      std::advance(__x, -__n);
      return __x;
    }




}
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 2 3
# 73 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 120 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _Tp, typename... _Args>
   
    inline void
    _Construct(_Tp* __p, _Args&&... __args)
    {
# 133 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
      ::new(static_cast<void*>(__p)) _Tp(std::forward<_Args>(__args)...);
    }
# 146 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _T1>
    inline void
    _Construct_novalue(_T1* __p)
    { ::new(static_cast<void*>(__p)) _T1; }

  template<typename _ForwardIterator>
    void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last);




  template<typename _Tp>
    constexpr inline void
    _Destroy(_Tp* __pointer)
    {



      __pointer->~_Tp();

    }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
# 199 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _ForwardIterator>
    inline void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
                       _Value_type;


      static_assert(is_destructible<_Value_type>::value,
      "value type is destructible");
      if constexpr (!__has_trivial_destructor(_Value_type))
 for (; __first != __last; ++__first)
   std::_Destroy(std::__addressof(*__first));
# 221 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
    }
# 255 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _ForwardIterator, typename _Size>
    inline _ForwardIterator
    _Destroy_n(_ForwardIterator __first, _Size __count)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
                       _Value_type;


      static_assert(is_destructible<_Value_type>::value,
      "value type is destructible");
      if constexpr (!__has_trivial_destructor(_Value_type))
 for (; __count > 0; (void)++__first, --__count)
   std::_Destroy(std::__addressof(*__first));





      else
 std::advance(__first, __count);
      return __first;




    }
#pragma GCC diagnostic pop
# 299 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3

}
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 1 3
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 1 3
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{



  template<typename _Tp>
    struct tuple_size;





  template<typename _Tp,
    typename _Up = typename remove_cv<_Tp>::type,
    typename = typename enable_if<is_same<_Tp, _Up>::value>::type,
    size_t = tuple_size<_Tp>::value>
    using __enable_if_has_tuple_size = _Tp;

  template<typename _Tp>
    struct tuple_size<const __enable_if_has_tuple_size<_Tp>>
    : public tuple_size<_Tp> { };

  template<typename _Tp>
    struct tuple_size<volatile __enable_if_has_tuple_size<_Tp>>
    : public tuple_size<_Tp> { };

  template<typename _Tp>
    struct tuple_size<const volatile __enable_if_has_tuple_size<_Tp>>
    : public tuple_size<_Tp> { };







  template<size_t __i, typename _Tp>
    struct tuple_element;


  template<size_t __i, typename _Tp>
    using __tuple_element_t = typename tuple_element<__i, _Tp>::type;

  template<size_t __i, typename _Tp>
    struct tuple_element<__i, const _Tp>
    {
      using type = const __tuple_element_t<__i, _Tp>;
    };

  template<size_t __i, typename _Tp>
    struct tuple_element<__i, volatile _Tp>
    {
      using type = volatile __tuple_element_t<__i, _Tp>;
    };

  template<size_t __i, typename _Tp>
    struct tuple_element<__i, const volatile _Tp>
    {
      using type = const volatile __tuple_element_t<__i, _Tp>;
    };





  template<typename _Tp, typename... _Types>
    constexpr size_t
    __find_uniq_type_in_pack()
    {
      constexpr size_t __sz = sizeof...(_Types);
      constexpr bool __found[__sz] = { __is_same(_Tp, _Types) ... };
      size_t __n = __sz;
      for (size_t __i = 0; __i < __sz; ++__i)
 {
   if (__found[__i])
     {
       if (__n < __sz)
  return __sz;
       __n = __i;
     }
 }
      return __n;
    }
# 136 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
  template<size_t __i, typename _Tp>
    using tuple_element_t = typename tuple_element<__i, _Tp>::type;




  template<size_t... _Indexes> struct _Index_tuple { };


  template<size_t _Num>
    struct _Build_index_tuple
    {
# 156 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
      using __type = _Index_tuple<__integer_pack(_Num)...>;

    };




  template<typename _Tp, _Tp... _Idx>
    struct integer_sequence
    {



      typedef _Tp value_type;
      static constexpr size_t size() noexcept { return sizeof...(_Idx); }
    };


  template<typename _Tp, _Tp _Num>
    using make_integer_sequence



      = integer_sequence<_Tp, __integer_pack(_Num)...>;



  template<size_t... _Idx>
    using index_sequence = integer_sequence<size_t, _Idx...>;


  template<size_t _Num>
    using make_index_sequence = make_integer_sequence<size_t, _Num>;


  template<typename... _Types>
    using index_sequence_for = make_index_sequence<sizeof...(_Types)>;
# 234 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
  template<size_t _Np, typename... _Types>
    struct _Nth_type
    { using type = __type_pack_element<_Np, _Types...>; };
# 284 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
  struct _Swallow_assign
  {
    template<class _Tp>
      constexpr const _Swallow_assign&
      operator=(const _Tp&) const noexcept
      { return *this; }
  };
# 309 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
  constexpr _Swallow_assign ignore{};
# 319 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3

}
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 2 3





namespace std __attribute__ ((__visibility__ ("default")))
{

# 79 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  struct piecewise_construct_t { explicit piecewise_construct_t() = default; };


  constexpr piecewise_construct_t piecewise_construct =
    piecewise_construct_t();




  template<typename _T1, typename _T2>
    struct pair;

  template<typename...>
    class tuple;





  template<typename _Tp, size_t _Nm>
    struct array;

  template<size_t...>
    struct _Index_tuple;

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&
    get(pair<_Tp1, _Tp2>& __in) noexcept;

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&&
    get(pair<_Tp1, _Tp2>&& __in) noexcept;

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr const typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&
    get(const pair<_Tp1, _Tp2>& __in) noexcept;

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr const typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&&
    get(const pair<_Tp1, _Tp2>&& __in) noexcept;

  template<size_t __i, typename... _Elements>
    constexpr __tuple_element_t<__i, tuple<_Elements...>>&
    get(tuple<_Elements...>& __t) noexcept;

  template<size_t __i, typename... _Elements>
    constexpr const __tuple_element_t<__i, tuple<_Elements...>>&
    get(const tuple<_Elements...>& __t) noexcept;

  template<size_t __i, typename... _Elements>
    constexpr __tuple_element_t<__i, tuple<_Elements...>>&&
    get(tuple<_Elements...>&& __t) noexcept;

  template<size_t __i, typename... _Elements>
    constexpr const __tuple_element_t<__i, tuple<_Elements...>>&&
    get(const tuple<_Elements...>&& __t) noexcept;

  template<size_t _Int, typename _Tp, size_t _Nm>
    constexpr _Tp&
    get(array<_Tp, _Nm>&) noexcept;

  template<size_t _Int, typename _Tp, size_t _Nm>
    constexpr _Tp&&
    get(array<_Tp, _Nm>&&) noexcept;

  template<size_t _Int, typename _Tp, size_t _Nm>
    constexpr const _Tp&
    get(const array<_Tp, _Nm>&) noexcept;

  template<size_t _Int, typename _Tp, size_t _Nm>
    constexpr const _Tp&&
    get(const array<_Tp, _Nm>&&) noexcept;







  template <bool, typename _T1, typename _T2>
    struct _PCC
    {
      template <typename _U1, typename _U2>
      static constexpr bool _ConstructiblePair()
      {
 return __and_<is_constructible<_T1, const _U1&>,
        is_constructible<_T2, const _U2&>>::value;
      }

      template <typename _U1, typename _U2>
      static constexpr bool _ImplicitlyConvertiblePair()
      {
 return __and_<is_convertible<const _U1&, _T1>,
        is_convertible<const _U2&, _T2>>::value;
      }

      template <typename _U1, typename _U2>
      static constexpr bool _MoveConstructiblePair()
      {
 return __and_<is_constructible<_T1, _U1&&>,
        is_constructible<_T2, _U2&&>>::value;
      }

      template <typename _U1, typename _U2>
      static constexpr bool _ImplicitlyMoveConvertiblePair()
      {
 return __and_<is_convertible<_U1&&, _T1>,
        is_convertible<_U2&&, _T2>>::value;
      }
    };

  template <typename _T1, typename _T2>
    struct _PCC<false, _T1, _T2>
    {
      template <typename _U1, typename _U2>
      static constexpr bool _ConstructiblePair()
      {
 return false;
      }

      template <typename _U1, typename _U2>
      static constexpr bool _ImplicitlyConvertiblePair()
      {
 return false;
      }

      template <typename _U1, typename _U2>
      static constexpr bool _MoveConstructiblePair()
      {
 return false;
      }

      template <typename _U1, typename _U2>
      static constexpr bool _ImplicitlyMoveConvertiblePair()
      {
 return false;
      }
    };
# 260 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _U1, typename _U2> class __pair_base
  {

    template<typename _T1, typename _T2> friend struct pair;
    __pair_base() = default;
    ~__pair_base() = default;
    __pair_base(const __pair_base&) = default;
    __pair_base& operator=(const __pair_base&) = delete;

  };
# 283 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    struct pair
    : public __pair_base<_T1, _T2>
    {
      typedef _T1 first_type;
      typedef _T2 second_type;

      _T1 first;
      _T2 second;


      constexpr pair(const pair&) = default;
      constexpr pair(pair&&) = default;

      template<typename... _Args1, typename... _Args2>

 pair(piecewise_construct_t, tuple<_Args1...>, tuple<_Args2...>);


      void
      swap(pair& __p)
      noexcept(__and_<__is_nothrow_swappable<_T1>,
        __is_nothrow_swappable<_T2>>::value)
      {
 using std::swap;
 swap(first, __p.first);
 swap(second, __p.second);
      }
# 331 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
    private:
      template<typename... _Args1, size_t... _Indexes1,
        typename... _Args2, size_t... _Indexes2>

 pair(tuple<_Args1...>&, tuple<_Args2...>&,
      _Index_tuple<_Indexes1...>, _Index_tuple<_Indexes2...>);
    public:
# 721 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
      template <typename _U1 = _T1,
                typename _U2 = _T2,
                typename enable_if<__and_<
                                     __is_implicitly_default_constructible<_U1>,
                                     __is_implicitly_default_constructible<_U2>>
                                   ::value, bool>::type = true>
      constexpr pair()
      : first(), second() { }

      template <typename _U1 = _T1,
                typename _U2 = _T2,
                typename enable_if<__and_<
                       is_default_constructible<_U1>,
                       is_default_constructible<_U2>,
                       __not_<
                         __and_<__is_implicitly_default_constructible<_U1>,
                                __is_implicitly_default_constructible<_U2>>>>
                                   ::value, bool>::type = false>
      explicit constexpr pair()
      : first(), second() { }



      using _PCCP = _PCC<true, _T1, _T2>;



      template<typename _U1 = _T1, typename _U2=_T2, typename
        enable_if<_PCCP::template
      _ConstructiblePair<_U1, _U2>()
                  && _PCCP::template
      _ImplicitlyConvertiblePair<_U1, _U2>(),
                         bool>::type=true>
      constexpr pair(const _T1& __a, const _T2& __b)
      : first(__a), second(__b) { }


       template<typename _U1 = _T1, typename _U2=_T2, typename
  enable_if<_PCCP::template
       _ConstructiblePair<_U1, _U2>()
                   && !_PCCP::template
       _ImplicitlyConvertiblePair<_U1, _U2>(),
                         bool>::type=false>
      explicit constexpr pair(const _T1& __a, const _T2& __b)
      : first(__a), second(__b) { }



      template <typename _U1, typename _U2>
        using _PCCFP = _PCC<!is_same<_T1, _U1>::value
       || !is_same<_T2, _U2>::value,
       _T1, _T2>;


      template<typename _U1, typename _U2, typename
        enable_if<_PCCFP<_U1, _U2>::template
      _ConstructiblePair<_U1, _U2>()
                  && _PCCFP<_U1, _U2>::template
      _ImplicitlyConvertiblePair<_U1, _U2>(),
     bool>::type=true>
 constexpr pair(const pair<_U1, _U2>& __p)
 : first(__p.first), second(__p.second)
 { ; }

      template<typename _U1, typename _U2, typename
        enable_if<_PCCFP<_U1, _U2>::template
      _ConstructiblePair<_U1, _U2>()
    && !_PCCFP<_U1, _U2>::template
      _ImplicitlyConvertiblePair<_U1, _U2>(),
                         bool>::type=false>
 explicit constexpr pair(const pair<_U1, _U2>& __p)
 : first(__p.first), second(__p.second)
 { ; }
# 805 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
    private:



      struct __zero_as_null_pointer_constant
      {
 __zero_as_null_pointer_constant(int __zero_as_null_pointer_constant::*)
 { }
 template<typename _Tp,
   typename = __enable_if_t<is_null_pointer<_Tp>::value>>
 __zero_as_null_pointer_constant(_Tp) = delete;
      };

    public:




      template<typename _U1,
        __enable_if_t<__and_<__not_<is_reference<_U1>>,
        is_pointer<_T2>,
        is_constructible<_T1, _U1>,
        __not_<is_constructible<_T1, const _U1&>>,
        is_convertible<_U1, _T1>>::value,
        bool> = true>
 __attribute__ ((__deprecated__ ("use 'nullptr' instead of '0' to " "initialize std::pair of move-only " "type and pointer")))
 constexpr
 pair(_U1&& __x, __zero_as_null_pointer_constant, ...)
 : first(std::forward<_U1>(__x)), second(nullptr)
 { ; }

      template<typename _U1,
        __enable_if_t<__and_<__not_<is_reference<_U1>>,
        is_pointer<_T2>,
        is_constructible<_T1, _U1>,
        __not_<is_constructible<_T1, const _U1&>>,
        __not_<is_convertible<_U1, _T1>>>::value,
        bool> = false>
 __attribute__ ((__deprecated__ ("use 'nullptr' instead of '0' to " "initialize std::pair of move-only " "type and pointer")))
 explicit constexpr
 pair(_U1&& __x, __zero_as_null_pointer_constant, ...)
 : first(std::forward<_U1>(__x)), second(nullptr)
 { ; }

      template<typename _U2,
        __enable_if_t<__and_<is_pointer<_T1>,
        __not_<is_reference<_U2>>,
        is_constructible<_T2, _U2>,
        __not_<is_constructible<_T2, const _U2&>>,
        is_convertible<_U2, _T2>>::value,
        bool> = true>
 __attribute__ ((__deprecated__ ("use 'nullptr' instead of '0' to " "initialize std::pair of move-only " "type and pointer")))
 constexpr
 pair(__zero_as_null_pointer_constant, _U2&& __y, ...)
 : first(nullptr), second(std::forward<_U2>(__y))
 { ; }

      template<typename _U2,
        __enable_if_t<__and_<is_pointer<_T1>,
        __not_<is_reference<_U2>>,
        is_constructible<_T2, _U2>,
        __not_<is_constructible<_T2, const _U2&>>,
        __not_<is_convertible<_U2, _T2>>>::value,
        bool> = false>
 __attribute__ ((__deprecated__ ("use 'nullptr' instead of '0' to " "initialize std::pair of move-only " "type and pointer")))
 explicit constexpr
 pair(__zero_as_null_pointer_constant, _U2&& __y, ...)
 : first(nullptr), second(std::forward<_U2>(__y))
 { ; }



      template<typename _U1, typename _U2, typename
        enable_if<_PCCP::template
      _MoveConstructiblePair<_U1, _U2>()
     && _PCCP::template
      _ImplicitlyMoveConvertiblePair<_U1, _U2>(),
                         bool>::type=true>
 constexpr pair(_U1&& __x, _U2&& __y)
 : first(std::forward<_U1>(__x)), second(std::forward<_U2>(__y))
 { ; }

      template<typename _U1, typename _U2, typename
        enable_if<_PCCP::template
      _MoveConstructiblePair<_U1, _U2>()
     && !_PCCP::template
      _ImplicitlyMoveConvertiblePair<_U1, _U2>(),
                         bool>::type=false>
 explicit constexpr pair(_U1&& __x, _U2&& __y)
 : first(std::forward<_U1>(__x)), second(std::forward<_U2>(__y))
 { ; }


      template<typename _U1, typename _U2, typename
        enable_if<_PCCFP<_U1, _U2>::template
      _MoveConstructiblePair<_U1, _U2>()
     && _PCCFP<_U1, _U2>::template
      _ImplicitlyMoveConvertiblePair<_U1, _U2>(),
                         bool>::type=true>
 constexpr pair(pair<_U1, _U2>&& __p)
 : first(std::forward<_U1>(__p.first)),
   second(std::forward<_U2>(__p.second))
 { ; }

      template<typename _U1, typename _U2, typename
        enable_if<_PCCFP<_U1, _U2>::template
      _MoveConstructiblePair<_U1, _U2>()
     && !_PCCFP<_U1, _U2>::template
      _ImplicitlyMoveConvertiblePair<_U1, _U2>(),
                         bool>::type=false>
 explicit constexpr pair(pair<_U1, _U2>&& __p)
 : first(std::forward<_U1>(__p.first)),
   second(std::forward<_U2>(__p.second))
 { ; }



      pair&
      operator=(__conditional_t<__and_<is_copy_assignable<_T1>,
           is_copy_assignable<_T2>>::value,
    const pair&, const __nonesuch&> __p)
      {
 first = __p.first;
 second = __p.second;
 return *this;
      }

      pair&
      operator=(__conditional_t<__and_<is_move_assignable<_T1>,
           is_move_assignable<_T2>>::value,
    pair&&, __nonesuch&&> __p)
      noexcept(__and_<is_nothrow_move_assignable<_T1>,
        is_nothrow_move_assignable<_T2>>::value)
      {
 first = std::forward<first_type>(__p.first);
 second = std::forward<second_type>(__p.second);
 return *this;
      }

      template<typename _U1, typename _U2>
 typename enable_if<__and_<is_assignable<_T1&, const _U1&>,
      is_assignable<_T2&, const _U2&>>::value,
      pair&>::type
 operator=(const pair<_U1, _U2>& __p)
 {
   first = __p.first;
   second = __p.second;
   return *this;
 }

      template<typename _U1, typename _U2>
 typename enable_if<__and_<is_assignable<_T1&, _U1&&>,
      is_assignable<_T2&, _U2&&>>::value,
      pair&>::type
 operator=(pair<_U1, _U2>&& __p)
 {
   first = std::forward<_U1>(__p.first);
   second = std::forward<_U2>(__p.second);
   return *this;
 }
# 997 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
    };
# 1039 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    [[__nodiscard__]]
    inline constexpr bool
    operator==(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return __x.first == __y.first && __x.second == __y.second; }
# 1052 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    [[__nodiscard__]]
    inline constexpr bool
    operator<(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return __x.first < __y.first
      || (!(__y.first < __x.first) && __x.second < __y.second); }


  template<typename _T1, typename _T2>
    [[__nodiscard__]]
    inline constexpr bool
    operator!=(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return !(__x == __y); }


  template<typename _T1, typename _T2>
    [[__nodiscard__]]
    inline constexpr bool
    operator>(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return __y < __x; }


  template<typename _T1, typename _T2>
    [[__nodiscard__]]
    inline constexpr bool
    operator<=(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return !(__y < __x); }


  template<typename _T1, typename _T2>
    [[__nodiscard__]]
    inline constexpr bool
    operator>=(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return !(__x < __y); }
# 1094 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    inline





    void

    swap(pair<_T1, _T2>& __x, pair<_T1, _T2>& __y)
    noexcept(noexcept(__x.swap(__y)))
    { __x.swap(__y); }
# 1143 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    constexpr pair<typename __decay_and_strip<_T1>::__type,
                   typename __decay_and_strip<_T2>::__type>
    make_pair(_T1&& __x, _T2&& __y)
    {
      typedef typename __decay_and_strip<_T1>::__type __ds_type1;
      typedef typename __decay_and_strip<_T2>::__type __ds_type2;
      typedef pair<__ds_type1, __ds_type2> __pair_type;
      return __pair_type(std::forward<_T1>(__x), std::forward<_T2>(__y));
    }
# 1166 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    struct __is_tuple_like_impl<pair<_T1, _T2>> : true_type
    { };



  template<class _Tp1, class _Tp2>
    struct tuple_size<pair<_Tp1, _Tp2>>
    : public integral_constant<size_t, 2> { };


  template<class _Tp1, class _Tp2>
    struct tuple_element<0, pair<_Tp1, _Tp2>>
    { typedef _Tp1 type; };


  template<class _Tp1, class _Tp2>
    struct tuple_element<1, pair<_Tp1, _Tp2>>
    { typedef _Tp2 type; };
# 1195 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++14-extensions"
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<typename _Tp>
    inline constexpr bool __is_pair = false;

  template<typename _Tp, typename _Up>
    inline constexpr bool __is_pair<pair<_Tp, _Up>> = true;
#pragma GCC diagnostic pop



  template<size_t _Int>
    struct __pair_get;

  template<>
    struct __pair_get<0>
    {
      template<typename _Tp1, typename _Tp2>
 static constexpr _Tp1&
 __get(pair<_Tp1, _Tp2>& __pair) noexcept
 { return __pair.first; }

      template<typename _Tp1, typename _Tp2>
 static constexpr _Tp1&&
 __move_get(pair<_Tp1, _Tp2>&& __pair) noexcept
 { return std::forward<_Tp1>(__pair.first); }

      template<typename _Tp1, typename _Tp2>
 static constexpr const _Tp1&
 __const_get(const pair<_Tp1, _Tp2>& __pair) noexcept
 { return __pair.first; }

      template<typename _Tp1, typename _Tp2>
 static constexpr const _Tp1&&
 __const_move_get(const pair<_Tp1, _Tp2>&& __pair) noexcept
 { return std::forward<const _Tp1>(__pair.first); }
    };

  template<>
    struct __pair_get<1>
    {
      template<typename _Tp1, typename _Tp2>
 static constexpr _Tp2&
 __get(pair<_Tp1, _Tp2>& __pair) noexcept
 { return __pair.second; }

      template<typename _Tp1, typename _Tp2>
 static constexpr _Tp2&&
 __move_get(pair<_Tp1, _Tp2>&& __pair) noexcept
 { return std::forward<_Tp2>(__pair.second); }

      template<typename _Tp1, typename _Tp2>
 static constexpr const _Tp2&
 __const_get(const pair<_Tp1, _Tp2>& __pair) noexcept
 { return __pair.second; }

      template<typename _Tp1, typename _Tp2>
 static constexpr const _Tp2&&
 __const_move_get(const pair<_Tp1, _Tp2>&& __pair) noexcept
 { return std::forward<const _Tp2>(__pair.second); }
    };






  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&
    get(pair<_Tp1, _Tp2>& __in) noexcept
    { return __pair_get<_Int>::__get(__in); }

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&&
    get(pair<_Tp1, _Tp2>&& __in) noexcept
    { return __pair_get<_Int>::__move_get(std::move(__in)); }

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr const typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&
    get(const pair<_Tp1, _Tp2>& __in) noexcept
    { return __pair_get<_Int>::__const_get(__in); }

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr const typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&&
    get(const pair<_Tp1, _Tp2>&& __in) noexcept
    { return __pair_get<_Int>::__const_move_get(std::move(__in)); }



  template <typename _Tp, typename _Up>
    constexpr _Tp&
    get(pair<_Tp, _Up>& __p) noexcept
    { return __p.first; }

  template <typename _Tp, typename _Up>
    constexpr const _Tp&
    get(const pair<_Tp, _Up>& __p) noexcept
    { return __p.first; }

  template <typename _Tp, typename _Up>
    constexpr _Tp&&
    get(pair<_Tp, _Up>&& __p) noexcept
    { return std::move(__p.first); }

  template <typename _Tp, typename _Up>
    constexpr const _Tp&&
    get(const pair<_Tp, _Up>&& __p) noexcept
    { return std::move(__p.first); }

  template <typename _Tp, typename _Up>
    constexpr _Tp&
    get(pair<_Up, _Tp>& __p) noexcept
    { return __p.second; }

  template <typename _Tp, typename _Up>
    constexpr const _Tp&
    get(const pair<_Up, _Tp>& __p) noexcept
    { return __p.second; }

  template <typename _Tp, typename _Up>
    constexpr _Tp&&
    get(pair<_Up, _Tp>&& __p) noexcept
    { return std::move(__p.second); }

  template <typename _Tp, typename _Up>
    constexpr const _Tp&&
    get(const pair<_Up, _Tp>&& __p) noexcept
    { return std::move(__p.second); }
# 1347 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3

}
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 2 3




#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
# 76 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
extern "C++" {

namespace std __attribute__ ((__visibility__ ("default")))
{


  struct __true_type { };
  struct __false_type { };

  template<bool>
    struct __truth_type
    { typedef __false_type __type; };

  template<>
    struct __truth_type<true>
    { typedef __true_type __type; };



  template<class _Sp, class _Tp>
    struct __traitor
    {
      enum { __value = bool(_Sp::__value) || bool(_Tp::__value) };
      typedef typename __truth_type<__value>::__type __type;
    };


  template<typename, typename>
    struct __are_same
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };

  template<typename _Tp>
    struct __are_same<_Tp, _Tp>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };




  template<typename _Tp>
    struct __is_integer
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };



  template<>
    struct __is_integer<bool>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<signed char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };


  template<>
    struct __is_integer<wchar_t>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 176 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<>
    struct __is_integer<char16_t>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<char32_t>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };


  template<>
    struct __is_integer<short>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned short>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<int>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned int>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<long>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned long>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<long long>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned long long>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 281 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Tp>
    struct __is_floating
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };


  template<>
    struct __is_floating<float>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_floating<double>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_floating<long double>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 358 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Tp>
    struct __is_arithmetic
    : public __traitor<__is_integer<_Tp>, __is_floating<_Tp> >
    { };




  template<typename _Tp>
    struct __is_char
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };

  template<>
    struct __is_char<char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };


  template<>
    struct __is_char<wchar_t>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };


  template<typename _Tp>
    struct __is_byte
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };

  template<>
    struct __is_byte<char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_byte<signed char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_byte<unsigned char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 438 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Tp>
    struct __is_nonvolatile_trivially_copyable
    {
      enum { __value = __is_trivially_copyable(_Tp) };
    };




  template<typename _Tp>
    struct __is_nonvolatile_trivially_copyable<volatile _Tp>
    {
      enum { __value = 0 };
    };


  template<typename _OutputIter, typename _InputIter>
    struct __memcpyable
    {
      enum { __value = 0 };
    };


  template<typename _Tp>
    struct __memcpyable<_Tp*, _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };


  template<typename _Tp>
    struct __memcpyable<_Tp*, const _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };

  template<typename _Tp> struct __memcpyable_integer;




  template<typename _Tp, typename _Up>
    struct __memcpyable<_Tp*, _Up*>
    {
      enum {
 __value = __memcpyable_integer<_Tp>::__width != 0
      && ((int)__memcpyable_integer<_Tp>::__width
     == (int)__memcpyable_integer<_Up>::__width)
      };
    };


  template<typename _Tp, typename _Up>
    struct __memcpyable<_Tp*, const _Up*>
    : __memcpyable<_Tp*, _Up*>
    { };

  template<typename _Tp>
    struct __memcpyable_integer
    {
      enum {
 __width = __is_integer<_Tp>::__value ? (sizeof(_Tp) * 8) : 0
      };
    };


  template<typename _Tp>
    struct __memcpyable_integer<volatile _Tp>
    { enum { __width = 0 }; };
# 551 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  __extension__
  template<>
    struct __memcpyable_integer<__int128> { enum { __width = 128 }; };
  __extension__
  template<>
    struct __memcpyable_integer<unsigned __int128> { enum { __width = 128 }; };
# 592 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Iter1, typename _Iter2>
    struct __memcmpable
    {
      enum { __value = 0 };
    };


  template<typename _Tp>
    struct __memcmpable<_Tp*, _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };

  template<typename _Tp>
    struct __memcmpable<const _Tp*, _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };

  template<typename _Tp>
    struct __memcmpable<_Tp*, const _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };







  template<typename _Tp, bool _TreatAsBytes =



 __is_byte<_Tp>::__value

    >
    struct __is_memcmp_ordered
    {
      static const bool __value = _Tp(-1) > _Tp(1);
    };

  template<typename _Tp>
    struct __is_memcmp_ordered<_Tp, false>
    {
      static const bool __value = false;
    };


  template<typename _Tp, typename _Up, bool = sizeof(_Tp) == sizeof(_Up)>
    struct __is_memcmp_ordered_with
    {
      static const bool __value = __is_memcmp_ordered<_Tp>::__value
 && __is_memcmp_ordered<_Up>::__value;
    };

  template<typename _Tp, typename _Up>
    struct __is_memcmp_ordered_with<_Tp, _Up, false>
    {
      static const bool __value = false;
    };
# 686 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Tp>
    struct __is_move_iterator
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };



  template<typename _Iterator>
   
    inline _Iterator
    __miter_base(_Iterator __it)
    { return __it; }


}
}

#pragma GCC diagnostic pop
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/type_traits.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/type_traits.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"

extern "C++" {

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{



  template<bool, typename>
    struct __enable_if
    { };

  template<typename _Tp>
    struct __enable_if<true, _Tp>
    { typedef _Tp __type; };



  template<bool _Cond, typename _Iftrue, typename _Iffalse>
    struct __conditional_type
    { typedef _Iftrue __type; };

  template<typename _Iftrue, typename _Iffalse>
    struct __conditional_type<false, _Iftrue, _Iffalse>
    { typedef _Iffalse __type; };



  template<typename _Tp>
    struct __add_unsigned
    {
    private:
      typedef __enable_if<std::__is_integer<_Tp>::__value, _Tp> __if_type;

    public:
      typedef typename __if_type::__type __type;
    };

  template<>
    struct __add_unsigned<char>
    { typedef unsigned char __type; };

  template<>
    struct __add_unsigned<signed char>
    { typedef unsigned char __type; };

  template<>
    struct __add_unsigned<short>
    { typedef unsigned short __type; };

  template<>
    struct __add_unsigned<int>
    { typedef unsigned int __type; };

  template<>
    struct __add_unsigned<long>
    { typedef unsigned long __type; };

  template<>
    struct __add_unsigned<long long>
    { typedef unsigned long long __type; };


  template<>
    struct __add_unsigned<bool>;

  template<>
    struct __add_unsigned<wchar_t>;



  template<typename _Tp>
    struct __remove_unsigned
    {
    private:
      typedef __enable_if<std::__is_integer<_Tp>::__value, _Tp> __if_type;

    public:
      typedef typename __if_type::__type __type;
    };

  template<>
    struct __remove_unsigned<char>
    { typedef signed char __type; };

  template<>
    struct __remove_unsigned<unsigned char>
    { typedef signed char __type; };

  template<>
    struct __remove_unsigned<unsigned short>
    { typedef short __type; };

  template<>
    struct __remove_unsigned<unsigned int>
    { typedef int __type; };

  template<>
    struct __remove_unsigned<unsigned long>
    { typedef long __type; };

  template<>
    struct __remove_unsigned<unsigned long long>
    { typedef long long __type; };


  template<>
    struct __remove_unsigned<bool>;

  template<>
    struct __remove_unsigned<wchar_t>;



  template<typename _Type>
    constexpr
    inline bool
    __is_null_pointer(_Type* __ptr)
    { return __ptr == 0; }

  template<typename _Type>
    constexpr
    inline bool
    __is_null_pointer(_Type)
    { return false; }


  constexpr bool
  __is_null_pointer(std::nullptr_t)
  { return true; }




  template<typename _Tp, bool = std::__is_integer<_Tp>::__value>
    struct __promote
    { typedef double __type; };




  template<typename _Tp>
    struct __promote<_Tp, false>
    { };

  template<>
    struct __promote<long double>
    { typedef long double __type; };

  template<>
    struct __promote<double>
    { typedef double __type; };

  template<>
    struct __promote<float>
    { typedef float __type; };
# 246 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/type_traits.h" 3
  template<typename _Tp, typename _Up,
           typename _Tp2 = typename __promote<_Tp>::__type,
           typename _Up2 = typename __promote<_Up>::__type>
    struct __promote_2
    {
      typedef __typeof__(_Tp2() + _Up2()) __type;
    };

  template<typename _Tp, typename _Up, typename _Vp,
           typename _Tp2 = typename __promote<_Tp>::__type,
           typename _Up2 = typename __promote<_Up>::__type,
           typename _Vp2 = typename __promote<_Vp>::__type>
    struct __promote_3
    {
      typedef __typeof__(_Tp2() + _Up2() + _Vp2()) __type;
    };

  template<typename _Tp, typename _Up, typename _Vp, typename _Wp,
           typename _Tp2 = typename __promote<_Tp>::__type,
           typename _Up2 = typename __promote<_Up>::__type,
           typename _Vp2 = typename __promote<_Vp>::__type,
           typename _Wp2 = typename __promote<_Wp>::__type>
    struct __promote_4
    {
      typedef __typeof__(_Tp2() + _Up2() + _Vp2() + _Wp2()) __type;
    };



}
}

#pragma GCC diagnostic pop
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 2 3

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{

# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3
  template<typename _Tp>
    struct __is_integer_nonstrict
    : public std::__is_integer<_Tp>
    {
      using std::__is_integer<_Tp>::__value;


      enum { __width = __value ? sizeof(_Tp) * 8 : 0 };
    };

  template<typename _Value>
    struct __numeric_traits_integer
    {

      static_assert(__is_integer_nonstrict<_Value>::__value,
      "invalid specialization");




      static const bool __is_signed = (_Value)(-1) < 0;
      static const int __digits
 = __is_integer_nonstrict<_Value>::__width - __is_signed;


      static const _Value __max = __is_signed
 ? (((((_Value)1 << (__digits - 1)) - 1) << 1) + 1)
 : ~(_Value)0;
      static const _Value __min = __is_signed ? -__max - 1 : (_Value)0;
    };

  template<typename _Value>
    const _Value __numeric_traits_integer<_Value>::__min;

  template<typename _Value>
    const _Value __numeric_traits_integer<_Value>::__max;

  template<typename _Value>
    const bool __numeric_traits_integer<_Value>::__is_signed;

  template<typename _Value>
    const int __numeric_traits_integer<_Value>::__digits;
# 132 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3
  __extension__ template<> struct __is_integer_nonstrict<__int128> { enum { __value = 1 }; typedef std::__true_type __type; enum { __width = 128 }; }; __extension__ template<> struct __is_integer_nonstrict<unsigned __int128> { enum { __value = 1 }; typedef std::__true_type __type; enum { __width = 128 }; };






  template<typename _Tp>
    using __int_traits = __numeric_traits_integer<_Tp>;
# 159 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3
  template<typename _Value>
    struct __numeric_traits_floating
    {

      static const int __max_digits10 = (2 + (std::__are_same<_Value, float>::__value ? 24 : std::__are_same<_Value, double>::__value ? 53 : 64) * 643L / 2136);


      static const bool __is_signed = true;
      static const int __digits10 = (std::__are_same<_Value, float>::__value ? 6 : std::__are_same<_Value, double>::__value ? 15 : 18);
      static const int __max_exponent10 = (std::__are_same<_Value, float>::__value ? 38 : std::__are_same<_Value, double>::__value ? 308 : 4932);
    };

  template<typename _Value>
    const int __numeric_traits_floating<_Value>::__max_digits10;

  template<typename _Value>
    const bool __numeric_traits_floating<_Value>::__is_signed;

  template<typename _Value>
    const int __numeric_traits_floating<_Value>::__digits10;

  template<typename _Value>
    const int __numeric_traits_floating<_Value>::__max_exponent10;






  template<typename _Value>
    struct __numeric_traits
    : public __numeric_traits_integer<_Value>
    { };

  template<>
    struct __numeric_traits<float>
    : public __numeric_traits_floating<float>
    { };

  template<>
    struct __numeric_traits<double>
    : public __numeric_traits_floating<double>
    { };

  template<>
    struct __numeric_traits<long double>
    : public __numeric_traits_floating<long double>
    { };
# 240 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3

}
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 77 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  namespace __detail
  {


    template<typename _Tp>
      inline _Tp*
      __get_temporary_buffer(ptrdiff_t __len) noexcept
      {
 if (__builtin_expect(size_t(__len) > (size_t(-1) / sizeof(_Tp)), 0))
   return 0;







 return (_Tp*) __builtin_operator_new(__len * sizeof(_Tp), nothrow_t());
      }



    template<typename _Tp>
      inline void
      __return_temporary_buffer(_Tp* __p,
    size_t __len __attribute__((__unused__)))
      {
# 118 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
 __builtin_operator_delete((__p), (__len) * sizeof(_Tp));
      }

  }
# 140 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  template<typename _Tp>
   
    pair<_Tp*, ptrdiff_t>
    get_temporary_buffer(ptrdiff_t __len) noexcept
    {
      const ptrdiff_t __max =
 __gnu_cxx::__numeric_traits<ptrdiff_t>::__max / sizeof(_Tp);
      if (__len > __max)
 __len = __max;

      while (__len > 0)
 {
   if (_Tp* __tmp = __detail::__get_temporary_buffer<_Tp>(__len))
     return pair<_Tp*, ptrdiff_t>(__tmp, __len);
   __len = __len == 1 ? 0 : ((__len + 1) / 2);
 }
      return pair<_Tp*, ptrdiff_t>();
    }
# 166 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  template<typename _Tp>
   
    inline void
    return_temporary_buffer(_Tp* __p)
    {





      __builtin_operator_delete(__p);
    }
# 187 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  template<typename _ForwardIterator, typename _Tp>
    class _Temporary_buffer
    {

     

    public:
      typedef _Tp value_type;
      typedef value_type* pointer;
      typedef pointer iterator;
      typedef ptrdiff_t size_type;

    protected:
      size_type _M_original_len;
      struct _Impl
      {
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
 explicit
 _Impl(ptrdiff_t __original_len)
 {
   pair<pointer, size_type> __p(
     std::get_temporary_buffer<value_type>(__original_len));
   _M_len = __p.second;
   _M_buffer = __p.first;
 }
#pragma GCC diagnostic pop

 ~_Impl()
 { std::__detail::__return_temporary_buffer(_M_buffer, _M_len); }

 size_type _M_len;
 pointer _M_buffer;
      } _M_impl;

    public:

      size_type
      size() const
      { return _M_impl._M_len; }


      size_type
      requested_size() const
      { return _M_original_len; }


      iterator
      begin()
      { return _M_impl._M_buffer; }


      iterator
      end()
      { return _M_impl._M_buffer + _M_impl._M_len; }





      _Temporary_buffer(_ForwardIterator __seed, size_type __original_len);

      ~_Temporary_buffer()
      { std::_Destroy(_M_impl._M_buffer, _M_impl._M_buffer + _M_impl._M_len); }

    private:

      _Temporary_buffer(const _Temporary_buffer&);

      void
      operator=(const _Temporary_buffer&);
    };


  template<bool>
    struct __uninitialized_construct_buf_dispatch
    {
      template<typename _Pointer, typename _ForwardIterator>
        static void
        __ucr(_Pointer __first, _Pointer __last,
       _ForwardIterator __seed)
        {
   if (__builtin_expect(__first == __last, 0))
     return;

   _Pointer __cur = __first;
   try
     {
       std::_Construct(std::__addressof(*__first),
         std::move(*__seed));
       _Pointer __prev = __cur;
       ++__cur;
       for(; __cur != __last; ++__cur, ++__prev)
  std::_Construct(std::__addressof(*__cur),
    std::move(*__prev));
       *__seed = std::move(*__prev);
     }
   catch(...)
     {
       std::_Destroy(__first, __cur);
       throw;
     }
 }
    };

  template<>
    struct __uninitialized_construct_buf_dispatch<true>
    {
      template<typename _Pointer, typename _ForwardIterator>
        static void
        __ucr(_Pointer, _Pointer, _ForwardIterator) { }
    };
# 311 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  template<typename _Tp, typename _ForwardIterator>
    inline void
    __uninitialized_construct_buf(_Tp* __first, _Tp* __last,
      _ForwardIterator __seed)
    {
      std::__uninitialized_construct_buf_dispatch<
 __has_trivial_constructor(_Tp)>::
   __ucr(__first, __last, __seed);
    }

  template<typename _ForwardIterator, typename _Tp>
    _Temporary_buffer<_ForwardIterator, _Tp>::
    _Temporary_buffer(_ForwardIterator __seed, size_type __original_len)
    : _M_original_len(__original_len), _M_impl(__original_len)
    {
      std::__uninitialized_construct_buf(begin(), end(), __seed);
    }


}
# 69 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 1 3
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ptr_traits.h" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ptr_traits.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{




  class __undefined;



  template<typename _Tp>
    struct __get_first_arg
    { using type = __undefined; };

  template<template<typename, typename...> class _SomeTemplate, typename _Tp,
           typename... _Types>
    struct __get_first_arg<_SomeTemplate<_Tp, _Types...>>
    { using type = _Tp; };



  template<typename _Tp, typename _Up>
    struct __replace_first_arg
    { };

  template<template<typename, typename...> class _SomeTemplate, typename _Up,
           typename _Tp, typename... _Types>
    struct __replace_first_arg<_SomeTemplate<_Tp, _Types...>, _Up>
    { using type = _SomeTemplate<_Up, _Types...>; };


  template<typename _Ptr, typename = void>
    struct __ptr_traits_elem : __get_first_arg<_Ptr>
    { };







  template<typename _Ptr>
    struct __ptr_traits_elem<_Ptr, __void_t<typename _Ptr::element_type>>
    { using type = typename _Ptr::element_type; };


  template<typename _Ptr>
    using __ptr_traits_elem_t = typename __ptr_traits_elem<_Ptr>::type;




  template<typename _Ptr, typename _Elt, bool = is_void<_Elt>::value>
    struct __ptr_traits_ptr_to
    {
      using pointer = _Ptr;
      using element_type = _Elt;







      static pointer
      pointer_to(element_type& __r)





      { return pointer::pointer_to(__r); }
    };


  template<typename _Ptr, typename _Elt>
    struct __ptr_traits_ptr_to<_Ptr, _Elt, true>
    { };


  template<typename _Tp>
    struct __ptr_traits_ptr_to<_Tp*, _Tp, false>
    {
      using pointer = _Tp*;
      using element_type = _Tp;






      static pointer
      pointer_to(element_type& __r) noexcept
      { return std::addressof(__r); }
    };

  template<typename _Ptr, typename _Elt>
    struct __ptr_traits_impl : __ptr_traits_ptr_to<_Ptr, _Elt>
    {
    private:
      template<typename _Tp>
 using __diff_t = typename _Tp::difference_type;

      template<typename _Tp, typename _Up>
 using __rebind = __type_identity<typename _Tp::template rebind<_Up>>;

    public:

      using pointer = _Ptr;


      using element_type = _Elt;


      using difference_type = __detected_or_t<ptrdiff_t, __diff_t, _Ptr>;


      template<typename _Up>
 using rebind = typename __detected_or_t<__replace_first_arg<_Ptr, _Up>,
      __rebind, _Ptr, _Up>::type;
    };



  template<typename _Ptr>
    struct __ptr_traits_impl<_Ptr, __undefined>
    { };







  template<typename _Ptr>
    struct pointer_traits : __ptr_traits_impl<_Ptr, __ptr_traits_elem_t<_Ptr>>
    { };







  template<typename _Tp>
    struct pointer_traits<_Tp*> : __ptr_traits_ptr_to<_Tp*, _Tp>
    {

      typedef _Tp* pointer;

      typedef _Tp element_type;

      typedef ptrdiff_t difference_type;

      template<typename _Up> using rebind = _Up*;
    };


  template<typename _Ptr, typename _Tp>
    using __ptr_rebind = typename pointer_traits<_Ptr>::template rebind<_Tp>;


  template<typename _Tp>
    [[__gnu__::__always_inline__]]
    constexpr _Tp*
    __to_address(_Tp* __ptr) noexcept
    {
      static_assert(!std::is_function<_Tp>::value, "std::to_address argument "
      "must not be a function pointer");
      return __ptr;
    }





  template<typename _Ptr>
    constexpr typename std::pointer_traits<_Ptr>::element_type*
    __to_address(const _Ptr& __ptr)
    { return std::__to_address(__ptr.operator->()); }
# 268 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ptr_traits.h" 3

}
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 1 3
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 1 3
# 85 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 108 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
 
# 108 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
# 130 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    class reverse_iterator
    : public iterator<typename iterator_traits<_Iterator>::iterator_category,
        typename iterator_traits<_Iterator>::value_type,
        typename iterator_traits<_Iterator>::difference_type,
        typename iterator_traits<_Iterator>::pointer,
                      typename iterator_traits<_Iterator>::reference>
    {
      template<typename _Iter>
 friend class reverse_iterator;
# 149 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
    protected:
      _Iterator current;

      typedef iterator_traits<_Iterator> __traits_type;

    public:
      typedef _Iterator iterator_type;
      typedef typename __traits_type::pointer pointer;

      typedef typename __traits_type::difference_type difference_type;
      typedef typename __traits_type::reference reference;
# 180 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      reverse_iterator()
      noexcept(noexcept(_Iterator()))
      : current()
      { }




      explicit
      reverse_iterator(iterator_type __x)
      noexcept(noexcept(_Iterator(__x)))
      : current(__x)
      { }




     
      reverse_iterator(const reverse_iterator& __x)
      noexcept(noexcept(_Iterator(__x.current)))
      : current(__x.current)
      { }


      reverse_iterator& operator=(const reverse_iterator&) = default;






      template<typename _Iter>




        reverse_iterator(const reverse_iterator<_Iter>& __x)
 noexcept(noexcept(_Iterator(__x.current)))
 : current(__x.current)
 { }


      template<typename _Iter>





 reverse_iterator&
 operator=(const reverse_iterator<_Iter>& __x)
 noexcept(noexcept(current = __x.current))
 {
   current = __x.current;
   return *this;
 }





      [[__nodiscard__]]
      iterator_type
      base() const
      noexcept(noexcept(_Iterator(current)))
      { return current; }
# 257 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      [[__nodiscard__]]
      reference
      operator*() const
      {
 _Iterator __tmp = current;
 return *--__tmp;
      }






      [[__nodiscard__]]
      pointer
      operator->() const




      {


 _Iterator __tmp = current;
 --__tmp;
 return _S_to_pointer(__tmp);
      }






      reverse_iterator&
      operator++()
      {
 --current;
 return *this;
      }






      reverse_iterator
      operator++(int)
      {
 reverse_iterator __tmp = *this;
 --current;
 return __tmp;
      }






      reverse_iterator&
      operator--()
      {
 ++current;
 return *this;
      }






      reverse_iterator
      operator--(int)
      {
 reverse_iterator __tmp = *this;
 ++current;
 return __tmp;
      }






      [[__nodiscard__]]
      reverse_iterator
      operator+(difference_type __n) const
      { return reverse_iterator(current - __n); }







      reverse_iterator&
      operator+=(difference_type __n)
      {
 current -= __n;
 return *this;
      }






      [[__nodiscard__]]
      reverse_iterator
      operator-(difference_type __n) const
      { return reverse_iterator(current + __n); }







      reverse_iterator&
      operator-=(difference_type __n)
      {
 current += __n;
 return *this;
      }






      [[__nodiscard__]]
      reference
      operator[](difference_type __n) const
      { return *(*this + __n); }
# 417 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
    private:
      template<typename _Tp>
 static _Tp*
 _S_to_pointer(_Tp* __p)
        { return __p; }

      template<typename _Tp>
 static pointer
 _S_to_pointer(_Tp __t)
        { return __t.operator->(); }
    };
# 440 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator==(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return __x.base() == __y.base(); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator<(const reverse_iterator<_Iterator>& __x,
       const reverse_iterator<_Iterator>& __y)
    { return __y.base() < __x.base(); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator!=(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return !(__x == __y); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator>(const reverse_iterator<_Iterator>& __x,
       const reverse_iterator<_Iterator>& __y)
    { return __y < __x; }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator<=(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return !(__y < __x); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator>=(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return !(__x < __y); }




  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator==(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() == __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator<(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    { return __x.base() > __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator!=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() != __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator>(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    { return __x.base() < __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    inline bool
    operator<=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() >= __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator>=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() <= __y.base(); }
# 617 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline auto
    operator-(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    -> decltype(__y.base() - __x.base())
    { return __y.base() - __x.base(); }


  template<typename _Iterator>
    [[__nodiscard__]]
    inline reverse_iterator<_Iterator>
    operator+(typename reverse_iterator<_Iterator>::difference_type __n,
       const reverse_iterator<_Iterator>& __x)
    { return reverse_iterator<_Iterator>(__x.base() - __n); }



  template<typename _Iterator>
    inline reverse_iterator<_Iterator>
    __make_reverse_iterator(_Iterator __i)
    { return reverse_iterator<_Iterator>(__i); }





  template<typename _Iterator>
    [[__nodiscard__]]
    inline reverse_iterator<_Iterator>
    make_reverse_iterator(_Iterator __i)
    { return reverse_iterator<_Iterator>(__i); }
# 659 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    struct __is_move_iterator<reverse_iterator<_Iterator> >
      : __is_move_iterator<_Iterator>
    { };
# 676 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    class back_insert_iterator
    : public iterator<output_iterator_tag, void, void, void, void>
    {
    protected:
      _Container* container;

    public:

      typedef _Container container_type;





      explicit
      back_insert_iterator(_Container& __x)
      : container(std::__addressof(__x)) { }
# 714 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      back_insert_iterator&
      operator=(const typename _Container::value_type& __value)
      {
 container->push_back(__value);
 return *this;
      }

     
      back_insert_iterator&
      operator=(typename _Container::value_type&& __value)
      {
 container->push_back(std::move(__value));
 return *this;
      }



      [[__nodiscard__]]
      back_insert_iterator&
      operator*()
      { return *this; }


     
      back_insert_iterator&
      operator++()
      { return *this; }


     
      back_insert_iterator
      operator++(int)
      { return *this; }
    };
# 761 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    [[__nodiscard__]]
    inline back_insert_iterator<_Container>
    back_inserter(_Container& __x)
    { return back_insert_iterator<_Container>(__x); }
# 777 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    class front_insert_iterator
    : public iterator<output_iterator_tag, void, void, void, void>
    {
    protected:
      _Container* container;

    public:

      typedef _Container container_type;





      explicit
      front_insert_iterator(_Container& __x)
      : container(std::__addressof(__x)) { }
# 815 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      front_insert_iterator&
      operator=(const typename _Container::value_type& __value)
      {
 container->push_front(__value);
 return *this;
      }

     
      front_insert_iterator&
      operator=(typename _Container::value_type&& __value)
      {
 container->push_front(std::move(__value));
 return *this;
      }



      [[__nodiscard__]]
      front_insert_iterator&
      operator*()
      { return *this; }


     
      front_insert_iterator&
      operator++()
      { return *this; }


     
      front_insert_iterator
      operator++(int)
      { return *this; }
    };
# 862 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    [[__nodiscard__]]
    inline front_insert_iterator<_Container>
    front_inserter(_Container& __x)
    { return front_insert_iterator<_Container>(__x); }
# 882 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    class insert_iterator
    : public iterator<output_iterator_tag, void, void, void, void>
    {



      typedef typename _Container::iterator _Iter;

    protected:
      _Container* container;
      _Iter iter;

    public:

      typedef _Container container_type;
# 907 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      insert_iterator(_Container& __x, _Iter __i)
      : container(std::__addressof(__x)), iter(__i) {}
# 943 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      insert_iterator&
      operator=(const typename _Container::value_type& __value)
      {
 iter = container->insert(iter, __value);
 ++iter;
 return *this;
      }

     
      insert_iterator&
      operator=(typename _Container::value_type&& __value)
      {
 iter = container->insert(iter, std::move(__value));
 ++iter;
 return *this;
      }



      [[__nodiscard__]]
      insert_iterator&
      operator*()
      { return *this; }


     
      insert_iterator&
      operator++()
      { return *this; }


     
      insert_iterator&
      operator++(int)
      { return *this; }
    };

#pragma GCC diagnostic pop
# 1002 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    [[__nodiscard__]]
    inline insert_iterator<_Container>
    inserter(_Container& __x, typename _Container::iterator __i)
    { return insert_iterator<_Container>(__x, __i); }





}

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{

# 1025 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator, typename _Container>
    class __normal_iterator
    {
    protected:
      _Iterator _M_current;

      typedef std::iterator_traits<_Iterator> __traits_type;


      template<typename _Iter>
 using __convertible_from
   = std::__enable_if_t<std::is_convertible<_Iter, _Iterator>::value>;


    public:
      typedef _Iterator iterator_type;
      typedef typename __traits_type::iterator_category iterator_category;
      typedef typename __traits_type::value_type value_type;
      typedef typename __traits_type::difference_type difference_type;
      typedef typename __traits_type::reference reference;
      typedef typename __traits_type::pointer pointer;





      __attribute__((__always_inline__))
      constexpr
      __normal_iterator() noexcept
      : _M_current() { }

      __attribute__((__always_inline__))
      explicit constexpr
      __normal_iterator(const _Iterator& __i) noexcept
      : _M_current(__i) { }






      template<typename _Iter, typename = __convertible_from<_Iter>>

 [[__gnu__::__always_inline__]]
 constexpr
 __normal_iterator(const __normal_iterator<_Iter, _Container>& __i)
 noexcept
# 1082 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
        : _M_current(__i.base()) { }



      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      reference
      operator*() const noexcept
      { return *_M_current; }

      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      pointer
      operator->() const noexcept
      { return _M_current; }

      __attribute__((__always_inline__))
      constexpr
      __normal_iterator&
      operator++() noexcept
      {
 ++_M_current;
 return *this;
      }

      __attribute__((__always_inline__))
      constexpr
      __normal_iterator
      operator++(int) noexcept
      { return __normal_iterator(_M_current++); }



      __attribute__((__always_inline__))
      constexpr
      __normal_iterator&
      operator--() noexcept
      {
 --_M_current;
 return *this;
      }

      __attribute__((__always_inline__))
      constexpr
      __normal_iterator
      operator--(int) noexcept
      { return __normal_iterator(_M_current--); }



      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      reference
      operator[](difference_type __n) const noexcept
      { return _M_current[__n]; }

      __attribute__((__always_inline__))
      constexpr
      __normal_iterator&
      operator+=(difference_type __n) noexcept
      { _M_current += __n; return *this; }

      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      __normal_iterator
      operator+(difference_type __n) const noexcept
      { return __normal_iterator(_M_current + __n); }

      __attribute__((__always_inline__))
      constexpr
      __normal_iterator&
      operator-=(difference_type __n) noexcept
      { _M_current -= __n; return *this; }

      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      __normal_iterator
      operator-(difference_type __n) const noexcept
      { return __normal_iterator(_M_current - __n); }

      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      const _Iterator&
      base() const noexcept
      { return _M_current; }
    };
# 1217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator==(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept
    { return __lhs.base() == __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator==(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() == __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator!=(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept
    { return __lhs.base() != __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator!=(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() != __rhs.base(); }


  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator<(const __normal_iterator<_IteratorL, _Container>& __lhs,
       const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept
    { return __lhs.base() < __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__))
    inline bool
    operator<(const __normal_iterator<_Iterator, _Container>& __lhs,
       const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() < __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator>(const __normal_iterator<_IteratorL, _Container>& __lhs,
       const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept
    { return __lhs.base() > __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator>(const __normal_iterator<_Iterator, _Container>& __lhs,
       const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() > __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator<=(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept
    { return __lhs.base() <= __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator<=(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() <= __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator>=(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept
    { return __lhs.base() >= __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator>=(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() >= __rhs.base(); }






  template<typename _IteratorL, typename _IteratorR, typename _Container>


    [[__nodiscard__, __gnu__::__always_inline__]]
    constexpr auto
    operator-(const __normal_iterator<_IteratorL, _Container>& __lhs,
       const __normal_iterator<_IteratorR, _Container>& __rhs) noexcept
    -> decltype(__lhs.base() - __rhs.base())





    { return __lhs.base() - __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline typename __normal_iterator<_Iterator, _Container>::difference_type
    operator-(const __normal_iterator<_Iterator, _Container>& __lhs,
       const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() - __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline __normal_iterator<_Iterator, _Container>
    operator+(typename __normal_iterator<_Iterator, _Container>::difference_type
       __n, const __normal_iterator<_Iterator, _Container>& __i)
    noexcept
    { return __normal_iterator<_Iterator, _Container>(__i.base() + __n); }


}

namespace std __attribute__ ((__visibility__ ("default")))
{

# 1434 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    class move_iterator



    {
      _Iterator _M_current;

      using __traits_type = iterator_traits<_Iterator>;

      using __base_ref = typename __traits_type::reference;


      template<typename _Iter2>
 friend class move_iterator;
# 1473 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
    public:
      using iterator_type = _Iterator;
# 1485 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      typedef typename __traits_type::iterator_category iterator_category;
      typedef typename __traits_type::value_type value_type;
      typedef typename __traits_type::difference_type difference_type;

      typedef _Iterator pointer;


      using reference
 = __conditional_t<is_reference<__base_ref>::value,
     typename remove_reference<__base_ref>::type&&,
     __base_ref>;


     
      move_iterator()
      : _M_current() { }

      explicit
      move_iterator(iterator_type __i)
      : _M_current(std::move(__i)) { }

      template<typename _Iter>




 move_iterator(const move_iterator<_Iter>& __i)
 : _M_current(__i._M_current) { }

      template<typename _Iter>





 move_iterator& operator=(const move_iterator<_Iter>& __i)
 {
   _M_current = __i._M_current;
   return *this;
 }


      [[__nodiscard__]]
      iterator_type
      base() const
      { return _M_current; }
# 1543 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      [[__nodiscard__]]
      reference
      operator*() const



      { return static_cast<reference>(*_M_current); }


      [[__nodiscard__]]
      pointer
      operator->() const
      { return _M_current; }

      move_iterator&
      operator++()
      {
 ++_M_current;
 return *this;
      }

      move_iterator
      operator++(int)
      {
 move_iterator __tmp = *this;
 ++_M_current;
 return __tmp;
      }







      move_iterator&
      operator--()
      {
 --_M_current;
 return *this;
      }

      move_iterator
      operator--(int)
      {
 move_iterator __tmp = *this;
 --_M_current;
 return __tmp;
      }

      [[__nodiscard__]]
      move_iterator
      operator+(difference_type __n) const
      { return move_iterator(_M_current + __n); }

      move_iterator&
      operator+=(difference_type __n)
      {
 _M_current += __n;
 return *this;
      }

      [[__nodiscard__]]
      move_iterator
      operator-(difference_type __n) const
      { return move_iterator(_M_current - __n); }

      move_iterator&
      operator-=(difference_type __n)
      {
 _M_current -= __n;
 return *this;
      }

      [[__nodiscard__]]
      reference
      operator[](difference_type __n) const



      { return std::move(_M_current[__n]); }
# 1657 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
    };

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator==(const move_iterator<_IteratorL>& __x,
        const move_iterator<_IteratorR>& __y)



    { return __x.base() == __y.base(); }
# 1678 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator!=(const move_iterator<_IteratorL>& __x,
        const move_iterator<_IteratorR>& __y)
    { return !(__x == __y); }


  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator<(const move_iterator<_IteratorL>& __x,
       const move_iterator<_IteratorR>& __y)



    { return __x.base() < __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator<=(const move_iterator<_IteratorL>& __x,
        const move_iterator<_IteratorR>& __y)



    { return !(__y < __x); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator>(const move_iterator<_IteratorL>& __x,
       const move_iterator<_IteratorR>& __y)



    { return __y < __x; }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator>=(const move_iterator<_IteratorL>& __x,
        const move_iterator<_IteratorR>& __y)



    { return !(__x < __y); }




  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator==(const move_iterator<_Iterator>& __x,
        const move_iterator<_Iterator>& __y)

    { return __x.base() == __y.base(); }
# 1745 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator!=(const move_iterator<_Iterator>& __x,
        const move_iterator<_Iterator>& __y)
    { return !(__x == __y); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator<(const move_iterator<_Iterator>& __x,
       const move_iterator<_Iterator>& __y)
    { return __x.base() < __y.base(); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator<=(const move_iterator<_Iterator>& __x,
        const move_iterator<_Iterator>& __y)
    { return !(__y < __x); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator>(const move_iterator<_Iterator>& __x,
       const move_iterator<_Iterator>& __y)
    { return __y < __x; }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator>=(const move_iterator<_Iterator>& __x,
        const move_iterator<_Iterator>& __y)
    { return !(__x < __y); }



  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline auto
    operator-(const move_iterator<_IteratorL>& __x,
       const move_iterator<_IteratorR>& __y)
    -> decltype(__x.base() - __y.base())
    { return __x.base() - __y.base(); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline move_iterator<_Iterator>
    operator+(typename move_iterator<_Iterator>::difference_type __n,
       const move_iterator<_Iterator>& __x)



    { return __x + __n; }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline move_iterator<_Iterator>
    make_move_iterator(_Iterator __i)
    { return move_iterator<_Iterator>(std::move(__i)); }

  template<typename _Iterator, typename _ReturnType
    = __conditional_t<__move_if_noexcept_cond
      <typename iterator_traits<_Iterator>::value_type>::value,
  _Iterator, move_iterator<_Iterator>>>
    [[__nodiscard__]]
    constexpr _ReturnType
    __make_move_if_noexcept_iterator(_Iterator __i)
    { return _ReturnType(__i); }



  template<typename _Tp, typename _ReturnType
    = __conditional_t<__move_if_noexcept_cond<_Tp>::value,
        const _Tp*, move_iterator<_Tp*>>>
    [[__nodiscard__]]
    constexpr _ReturnType
    __make_move_if_noexcept_iterator(_Tp* __i)
    { return _ReturnType(__i); }

  template<typename _Iterator>
    struct __is_move_iterator<move_iterator<_Iterator> >
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 2980 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3

}

namespace __gnu_debug
{
  template<typename _Iterator, typename _Sequence, typename _Category>
    class _Safe_iterator;
}

namespace std __attribute__ ((__visibility__ ("default")))
{





  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__))
   
    inline _Iterator
    __niter_base(__gnu_cxx::__normal_iterator<_Iterator, _Container> __it)
    noexcept(std::is_nothrow_copy_constructible<_Iterator>::value)
    { return __it.base(); }


  template<typename _Iterator>
    [[__nodiscard__]] __attribute__((__always_inline__))
   
    inline _Iterator
    __niter_base(_Iterator __it)
    noexcept(std::is_nothrow_copy_constructible<_Iterator>::value)
    { return __it; }
# 3027 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Ite, typename _Seq>
   
    decltype(std::__niter_base(std::declval<_Ite>()))
    __niter_base(const ::__gnu_debug::_Safe_iterator<_Ite, _Seq,
   std::random_access_iterator_tag>&)
    noexcept(std::is_nothrow_copy_constructible<_Ite>::value);



  template<typename _Iterator>
   
    inline auto
    __niter_base(reverse_iterator<_Iterator> __it)
    -> decltype(__make_reverse_iterator(__niter_base(__it.base())))
    { return __make_reverse_iterator(__niter_base(__it.base())); }

  template<typename _Iterator>
   
    inline auto
    __niter_base(move_iterator<_Iterator> __it)
    -> decltype(make_move_iterator(__niter_base(__it.base())))
    { return make_move_iterator(__niter_base(__it.base())); }

  template<typename _Iterator>
   
    inline auto
    __miter_base(reverse_iterator<_Iterator> __it)
    -> decltype(__make_reverse_iterator(__miter_base(__it.base())))
    { return __make_reverse_iterator(__miter_base(__it.base())); }

  template<typename _Iterator>
   
    inline auto
    __miter_base(move_iterator<_Iterator> __it)
    -> decltype(__miter_base(__it.base()))
    { return __miter_base(__it.base()); }






  template<typename _From, typename _To>
    [[__nodiscard__]]
   
    inline _From
    __niter_wrap(_From __from, _To __res)
    { return __from + (std::__niter_base(__res) - std::__niter_base(__from)); }


  template<typename _Iterator>
    [[__nodiscard__]] __attribute__((__always_inline__))
   
    inline _Iterator
    __niter_wrap(const _Iterator&, _Iterator __res)
    { return __res; }
# 3113 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3

}
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/debug/debug.h" 1 3
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/debug/debug.h" 3
namespace std
{
  namespace __debug { }
}




namespace __gnu_debug
{
  using namespace std::__debug;

  template<typename _Ite, typename _Seq, typename _Cat>
    struct _Safe_iterator;
}
# 70 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/predefined_ops.h" 1 3
# 35 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/predefined_ops.h" 3
namespace __gnu_cxx
{
namespace __ops
{
  struct _Iter_less_iter
  {
    template<typename _Iterator1, typename _Iterator2>
      constexpr
      bool
      operator()(_Iterator1 __it1, _Iterator2 __it2) const
      { return *__it1 < *__it2; }
  };

  constexpr
  inline _Iter_less_iter
  __iter_less_iter()
  { return _Iter_less_iter(); }

  struct _Iter_less_val
  {

    constexpr _Iter_less_val() = default;




   
    explicit
    _Iter_less_val(_Iter_less_iter) { }

    template<typename _Iterator, typename _Value>
     
      bool
      operator()(_Iterator __it, _Value& __val) const
      { return *__it < __val; }
  };

 
  inline _Iter_less_val
  __iter_less_val()
  { return _Iter_less_val(); }

 
  inline _Iter_less_val
  __iter_comp_val(_Iter_less_iter)
  { return _Iter_less_val(); }

  struct _Val_less_iter
  {

    constexpr _Val_less_iter() = default;




   
    explicit
    _Val_less_iter(_Iter_less_iter) { }

    template<typename _Value, typename _Iterator>
     
      bool
      operator()(_Value& __val, _Iterator __it) const
      { return __val < *__it; }
  };

 
  inline _Val_less_iter
  __val_less_iter()
  { return _Val_less_iter(); }

 
  inline _Val_less_iter
  __val_comp_iter(_Iter_less_iter)
  { return _Val_less_iter(); }

  struct _Iter_equal_to_iter
  {
    template<typename _Iterator1, typename _Iterator2>
     
      bool
      operator()(_Iterator1 __it1, _Iterator2 __it2) const
      { return *__it1 == *__it2; }
  };

 
  inline _Iter_equal_to_iter
  __iter_equal_to_iter()
  { return _Iter_equal_to_iter(); }

  struct _Iter_equal_to_val
  {
    template<typename _Iterator, typename _Value>
     
      bool
      operator()(_Iterator __it, _Value& __val) const
      { return *__it == __val; }
  };

 
  inline _Iter_equal_to_val
  __iter_equal_to_val()
  { return _Iter_equal_to_val(); }

 
  inline _Iter_equal_to_val
  __iter_comp_val(_Iter_equal_to_iter)
  { return _Iter_equal_to_val(); }

  template<typename _Compare>
    struct _Iter_comp_iter
    {
      _Compare _M_comp;

      explicit constexpr
      _Iter_comp_iter(_Compare __comp)
 : _M_comp(std::move(__comp))
      { }

      template<typename _Iterator1, typename _Iterator2>
        constexpr
        bool
        operator()(_Iterator1 __it1, _Iterator2 __it2)
        { return bool(_M_comp(*__it1, *__it2)); }
    };

  template<typename _Compare>
    constexpr
    inline _Iter_comp_iter<_Compare>
    __iter_comp_iter(_Compare __comp)
    { return _Iter_comp_iter<_Compare>(std::move(__comp)); }

  template<typename _Compare>
    struct _Iter_comp_val
    {
      _Compare _M_comp;

     
      explicit
      _Iter_comp_val(_Compare __comp)
 : _M_comp(std::move(__comp))
      { }

     
      explicit
      _Iter_comp_val(const _Iter_comp_iter<_Compare>& __comp)
 : _M_comp(__comp._M_comp)
      { }


     
      explicit
      _Iter_comp_val(_Iter_comp_iter<_Compare>&& __comp)
 : _M_comp(std::move(__comp._M_comp))
      { }


      template<typename _Iterator, typename _Value>

 bool
 operator()(_Iterator __it, _Value& __val)
 { return bool(_M_comp(*__it, __val)); }
    };

  template<typename _Compare>
   
    inline _Iter_comp_val<_Compare>
    __iter_comp_val(_Compare __comp)
    { return _Iter_comp_val<_Compare>(std::move(__comp)); }

  template<typename _Compare>
   
    inline _Iter_comp_val<_Compare>
    __iter_comp_val(_Iter_comp_iter<_Compare> __comp)
    { return _Iter_comp_val<_Compare>(std::move(__comp)); }

  template<typename _Compare>
    struct _Val_comp_iter
    {
      _Compare _M_comp;

     
      explicit
      _Val_comp_iter(_Compare __comp)
 : _M_comp(std::move(__comp))
      { }

     
      explicit
      _Val_comp_iter(const _Iter_comp_iter<_Compare>& __comp)
 : _M_comp(__comp._M_comp)
      { }


     
      explicit
      _Val_comp_iter(_Iter_comp_iter<_Compare>&& __comp)
 : _M_comp(std::move(__comp._M_comp))
      { }


      template<typename _Value, typename _Iterator>

 bool
 operator()(_Value& __val, _Iterator __it)
 { return bool(_M_comp(__val, *__it)); }
    };

  template<typename _Compare>
   
    inline _Val_comp_iter<_Compare>
    __val_comp_iter(_Compare __comp)
    { return _Val_comp_iter<_Compare>(std::move(__comp)); }

  template<typename _Compare>
   
    inline _Val_comp_iter<_Compare>
    __val_comp_iter(_Iter_comp_iter<_Compare> __comp)
    { return _Val_comp_iter<_Compare>(std::move(__comp)); }

  template<typename _Value>
    struct _Iter_equals_val
    {
      _Value& _M_value;

     
      explicit
      _Iter_equals_val(_Value& __value)
 : _M_value(__value)
      { }

      template<typename _Iterator>

 bool
 operator()(_Iterator __it)
 { return *__it == _M_value; }
    };

  template<typename _Value>
   
    inline _Iter_equals_val<_Value>
    __iter_equals_val(_Value& __val)
    { return _Iter_equals_val<_Value>(__val); }

  template<typename _Iterator1>
    struct _Iter_equals_iter
    {
      _Iterator1 _M_it1;

     
      explicit
      _Iter_equals_iter(_Iterator1 __it1)
 : _M_it1(__it1)
      { }

      template<typename _Iterator2>

 bool
 operator()(_Iterator2 __it2)
 { return *__it2 == *_M_it1; }
    };

  template<typename _Iterator>
   
    inline _Iter_equals_iter<_Iterator>
    __iter_comp_iter(_Iter_equal_to_iter, _Iterator __it)
    { return _Iter_equals_iter<_Iterator>(__it); }

  template<typename _Predicate>
    struct _Iter_pred
    {
      _Predicate _M_pred;

     
      explicit
      _Iter_pred(_Predicate __pred)
 : _M_pred(std::move(__pred))
      { }

      template<typename _Iterator>

 bool
 operator()(_Iterator __it)
 { return bool(_M_pred(*__it)); }
    };

  template<typename _Predicate>
   
    inline _Iter_pred<_Predicate>
    __pred_iter(_Predicate __pred)
    { return _Iter_pred<_Predicate>(std::move(__pred)); }

  template<typename _Compare, typename _Value>
    struct _Iter_comp_to_val
    {
      _Compare _M_comp;
      _Value& _M_value;

     
      _Iter_comp_to_val(_Compare __comp, _Value& __value)
 : _M_comp(std::move(__comp)), _M_value(__value)
      { }

      template<typename _Iterator>

 bool
 operator()(_Iterator __it)
 { return bool(_M_comp(*__it, _M_value)); }
    };

  template<typename _Compare, typename _Value>
    _Iter_comp_to_val<_Compare, _Value>
   
    __iter_comp_val(_Compare __comp, _Value &__val)
    {
      return _Iter_comp_to_val<_Compare, _Value>(std::move(__comp), __val);
    }

  template<typename _Compare, typename _Iterator1>
    struct _Iter_comp_to_iter
    {
      _Compare _M_comp;
      _Iterator1 _M_it1;

     
      _Iter_comp_to_iter(_Compare __comp, _Iterator1 __it1)
 : _M_comp(std::move(__comp)), _M_it1(__it1)
      { }

      template<typename _Iterator2>

 bool
 operator()(_Iterator2 __it2)
 { return bool(_M_comp(*__it2, *_M_it1)); }
    };

  template<typename _Compare, typename _Iterator>
   
    inline _Iter_comp_to_iter<_Compare, _Iterator>
    __iter_comp_iter(_Iter_comp_iter<_Compare> __comp, _Iterator __it)
    {
      return _Iter_comp_to_iter<_Compare, _Iterator>(
   std::move(__comp._M_comp), __it);
    }

  template<typename _Predicate>
    struct _Iter_negate
    {
      _Predicate _M_pred;

     
      explicit
      _Iter_negate(_Predicate __pred)
 : _M_pred(std::move(__pred))
      { }

      template<typename _Iterator>

 bool
 operator()(_Iterator __it)
 { return !bool(_M_pred(*__it)); }
    };

  template<typename _Predicate>
   
    inline _Iter_negate<_Predicate>
    __negate(_Iter_pred<_Predicate> __pred)
    { return _Iter_negate<_Predicate>(std::move(__pred._M_pred)); }

}
}
# 72 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3




# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/concepts" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/concepts" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/concepts" 2 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 2 3
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 157 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 3
  template<typename _Tp>
    constexpr _Tp
    __rotl(_Tp __x, int __s) noexcept
    {
      constexpr auto _Nd = __gnu_cxx::__int_traits<_Tp>::__digits;
      if ((_Nd & (_Nd - 1)) == 0)
 {


   constexpr unsigned __uNd = _Nd;
   const unsigned __r = __s;
   return (__x << (__r % __uNd)) | (__x >> ((-__r) % __uNd));
 }
      const int __r = __s % _Nd;
      if (__r == 0)
 return __x;
      else if (__r > 0)
 return (__x << __r) | (__x >> ((_Nd - __r) % _Nd));
      else
 return (__x >> -__r) | (__x << ((_Nd + __r) % _Nd));
    }

  template<typename _Tp>
    constexpr _Tp
    __rotr(_Tp __x, int __s) noexcept
    {
      constexpr auto _Nd = __gnu_cxx::__int_traits<_Tp>::__digits;
      if ((_Nd & (_Nd - 1)) == 0)
 {


   constexpr unsigned __uNd = _Nd;
   const unsigned __r = __s;
   return (__x >> (__r % __uNd)) | (__x << ((-__r) % __uNd));
 }
      const int __r = __s % _Nd;
      if (__r == 0)
 return __x;
      else if (__r > 0)
 return (__x >> __r) | (__x << ((_Nd - __r) % _Nd));
      else
 return (__x << -__r) | (__x >> ((_Nd + __r) % _Nd));
    }

  template<typename _Tp>
    constexpr int
    __countl_zero(_Tp __x) noexcept
    {
      using __gnu_cxx::__int_traits;
      constexpr auto _Nd = __int_traits<_Tp>::__digits;

      if (__x == 0)
        return _Nd;

      constexpr auto _Nd_ull = __int_traits<unsigned long long>::__digits;
      constexpr auto _Nd_ul = __int_traits<unsigned long>::__digits;
      constexpr auto _Nd_u = __int_traits<unsigned>::__digits;

      if (_Nd <= _Nd_u)
 {
   constexpr int __diff = _Nd_u - _Nd;
   return __builtin_clz(__x) - __diff;
 }
      else if (_Nd <= _Nd_ul)
 {
   constexpr int __diff = _Nd_ul - _Nd;
   return __builtin_clzl(__x) - __diff;
 }
      else if (_Nd <= _Nd_ull)
 {
   constexpr int __diff = _Nd_ull - _Nd;
   return __builtin_clzll(__x) - __diff;
 }
      else
 {
   static_assert(_Nd <= (2 * _Nd_ull),
   "Maximum supported integer size is 128-bit");

   unsigned long long __high = __x >> _Nd_ull;
   if (__high != 0)
     {
       constexpr int __diff = (2 * _Nd_ull) - _Nd;
       return __builtin_clzll(__high) - __diff;
     }
   constexpr auto __max_ull = __int_traits<unsigned long long>::__max;
   unsigned long long __low = __x & __max_ull;
   return (_Nd - _Nd_ull) + __builtin_clzll(__low);
 }
    }

  template<typename _Tp>
    constexpr int
    __countl_one(_Tp __x) noexcept
    {
      return std::__countl_zero<_Tp>((_Tp)~__x);
    }

  template<typename _Tp>
    constexpr int
    __countr_zero(_Tp __x) noexcept
    {
      using __gnu_cxx::__int_traits;
      constexpr auto _Nd = __int_traits<_Tp>::__digits;

      if (__x == 0)
        return _Nd;

      constexpr auto _Nd_ull = __int_traits<unsigned long long>::__digits;
      constexpr auto _Nd_ul = __int_traits<unsigned long>::__digits;
      constexpr auto _Nd_u = __int_traits<unsigned>::__digits;

      if (_Nd <= _Nd_u)
 return __builtin_ctz(__x);
      else if (_Nd <= _Nd_ul)
 return __builtin_ctzl(__x);
      else if (_Nd <= _Nd_ull)
 return __builtin_ctzll(__x);
      else
 {
   static_assert(_Nd <= (2 * _Nd_ull),
   "Maximum supported integer size is 128-bit");

   constexpr auto __max_ull = __int_traits<unsigned long long>::__max;
   unsigned long long __low = __x & __max_ull;
   if (__low != 0)
     return __builtin_ctzll(__low);
   unsigned long long __high = __x >> _Nd_ull;
   return __builtin_ctzll(__high) + _Nd_ull;
 }
    }

  template<typename _Tp>
    constexpr int
    __countr_one(_Tp __x) noexcept
    {
      return std::__countr_zero((_Tp)~__x);
    }

  template<typename _Tp>
    constexpr int
    __popcount(_Tp __x) noexcept
    {
      using __gnu_cxx::__int_traits;
      constexpr auto _Nd = __int_traits<_Tp>::__digits;

      constexpr auto _Nd_ull = __int_traits<unsigned long long>::__digits;
      constexpr auto _Nd_ul = __int_traits<unsigned long>::__digits;
      constexpr auto _Nd_u = __int_traits<unsigned>::__digits;

      if (_Nd <= _Nd_u)
 return __builtin_popcount(__x);
      else if (_Nd <= _Nd_ul)
 return __builtin_popcountl(__x);
      else if (_Nd <= _Nd_ull)
 return __builtin_popcountll(__x);
      else
 {
   static_assert(_Nd <= (2 * _Nd_ull),
   "Maximum supported integer size is 128-bit");

   constexpr auto __max_ull = __int_traits<unsigned long long>::__max;
   unsigned long long __low = __x & __max_ull;
   unsigned long long __high = __x >> _Nd_ull;
   return __builtin_popcountll(__low) + __builtin_popcountll(__high);
 }
    }

  template<typename _Tp>
    constexpr bool
    __has_single_bit(_Tp __x) noexcept
    { return std::__popcount(__x) == 1; }

  template<typename _Tp>
    constexpr _Tp
    __bit_ceil(_Tp __x) noexcept
    {
      using __gnu_cxx::__int_traits;
      constexpr auto _Nd = __int_traits<_Tp>::__digits;
      if (__x == 0 || __x == 1)
        return 1;
      auto __shift_exponent = _Nd - std::__countl_zero((_Tp)(__x - 1u));




      if (!std::__is_constant_evaluated())
 {
   do { if (__builtin_expect(!bool(__shift_exponent != __int_traits<_Tp>::__digits), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit", 344, __PRETTY_FUNCTION__, "__shift_exponent != __int_traits<_Tp>::__digits"); } while (false);
 }

      using __promoted_type = decltype(__x << 1);
      if (!is_same<__promoted_type, _Tp>::value)
 {





   const int __extra_exp = sizeof(__promoted_type) / sizeof(_Tp) / 2;
   __shift_exponent |= (__shift_exponent & _Nd) << __extra_exp;
 }
      return (_Tp)1u << __shift_exponent;
    }

  template<typename _Tp>
    constexpr _Tp
    __bit_floor(_Tp __x) noexcept
    {
      constexpr auto _Nd = __gnu_cxx::__int_traits<_Tp>::__digits;
      if (__x == 0)
        return 0;
      return (_Tp)1u << (_Nd - std::__countl_zero((_Tp)(__x >> 1)));
    }

  template<typename _Tp>
    constexpr int
    __bit_width(_Tp __x) noexcept
    {
      constexpr auto _Nd = __gnu_cxx::__int_traits<_Tp>::__digits;
      return _Nd - std::__countl_zero(__x);
    }
# 484 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 3

}
# 77 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3






namespace std __attribute__ ((__visibility__ ("default")))
{






  template<typename _Tp, typename _Up>
    constexpr
    inline int
    __memcmp(const _Tp* __first1, const _Up* __first2, size_t __num)
    {

      static_assert(sizeof(_Tp) == sizeof(_Up), "can be compared with memcmp");
# 109 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
 return __builtin_memcmp(__first1, __first2, sizeof(_Tp) * __num);
    }
# 153 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2>
   
    inline void
    iter_swap(_ForwardIterator1 __a, _ForwardIterator2 __b)
    {

     

     
# 186 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
      swap(*__a, *__b);

    }
# 202 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2>
   
    _ForwardIterator2
    swap_ranges(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
  _ForwardIterator2 __first2)
    {

     

     

      ;

      for (; __first1 != __last1; ++__first1, (void)++__first2)
 std::iter_swap(__first1, __first2);
      return __first2;
    }
# 231 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp>
    [[__nodiscard__]] constexpr
    inline const _Tp&
    min(const _Tp& __a, const _Tp& __b)
    {

     

      if (__b < __a)
 return __b;
      return __a;
    }
# 255 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp>
    [[__nodiscard__]] constexpr
    inline const _Tp&
    max(const _Tp& __a, const _Tp& __b)
    {

     

      if (__a < __b)
 return __b;
      return __a;
    }
# 279 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp, typename _Compare>
    [[__nodiscard__]] constexpr
    inline const _Tp&
    min(const _Tp& __a, const _Tp& __b, _Compare __comp)
    {

      if (__comp(__b, __a))
 return __b;
      return __a;
    }
# 301 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp, typename _Compare>
    [[__nodiscard__]] constexpr
    inline const _Tp&
    max(const _Tp& __a, const _Tp& __b, _Compare __comp)
    {

      if (__comp(__a, __b))
 return __b;
      return __a;
    }



  template<typename _Tp, typename _Ref, typename _Ptr>
    struct _Deque_iterator;

  struct _Bit_iterator;






  template<typename _CharT>
    struct char_traits;

  template<typename _CharT, typename _Traits>
    class istreambuf_iterator;

  template<typename _CharT, typename _Traits>
    class ostreambuf_iterator;

  template<bool _IsMove, typename _CharT>
    typename __gnu_cxx::__enable_if<__is_char<_CharT>::__value,
      ostreambuf_iterator<_CharT, char_traits<_CharT> > >::__type
    __copy_move_a2(_CharT*, _CharT*,
     ostreambuf_iterator<_CharT, char_traits<_CharT> >);

  template<bool _IsMove, typename _CharT>
    typename __gnu_cxx::__enable_if<__is_char<_CharT>::__value,
      ostreambuf_iterator<_CharT, char_traits<_CharT> > >::__type
    __copy_move_a2(const _CharT*, const _CharT*,
     ostreambuf_iterator<_CharT, char_traits<_CharT> >);

  template<bool _IsMove, typename _CharT>
    typename __gnu_cxx::__enable_if<__is_char<_CharT>::__value,
        _CharT*>::__type
    __copy_move_a2(istreambuf_iterator<_CharT, char_traits<_CharT> >,
     istreambuf_iterator<_CharT, char_traits<_CharT> >, _CharT*);

  template<bool _IsMove, typename _CharT>
    typename __gnu_cxx::__enable_if<
      __is_char<_CharT>::__value,
      std::_Deque_iterator<_CharT, _CharT&, _CharT*> >::__type
    __copy_move_a2(
 istreambuf_iterator<_CharT, char_traits<_CharT> >,
 istreambuf_iterator<_CharT, char_traits<_CharT> >,
 std::_Deque_iterator<_CharT, _CharT&, _CharT*>);
# 409 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<bool _IsMove, typename _OutIter, typename _InIter>
    __attribute__((__always_inline__))
    inline void
    __assign_one(_OutIter& __out, _InIter& __in)
    {

      if constexpr (_IsMove)
 *__out = std::move(*__in);
      else

 *__out = *__in;
    }

  template<bool _IsMove, typename _InIter, typename _Sent, typename _OutIter>
   
    inline _OutIter
    __copy_move_a2(_InIter __first, _Sent __last, _OutIter __result)
    {
      typedef __decltype(*__first) _InRef;
      typedef __decltype(*__result) _OutRef;
      if constexpr (!__is_trivially_assignable(_OutRef, _InRef))
 { }
      else if (std::__is_constant_evaluated())
 { }
      else if constexpr (__memcpyable<_OutIter, _InIter>::__value)
 {
   ptrdiff_t __n = std::distance(__first, __last);
   if (__builtin_expect(__n > 1, true))
     {
       __builtin_memmove(__result,
    __first,
    __n * sizeof(*__first));
       __result += __n;
     }
   else if (__n == 1)
     {
       std::__assign_one<_IsMove>(__result, __first);
       ++__result;
     }
   return __result;
 }
# 474 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
      for (; __first != __last; ++__result, (void)++__first)
 std::__assign_one<_IsMove>(__result, __first);
      return __result;
    }
#pragma GCC diagnostic pop

  template<bool _IsMove,
    typename _Tp, typename _Ref, typename _Ptr, typename _OI>
    _OI
    __copy_move_a1(std::_Deque_iterator<_Tp, _Ref, _Ptr>,
     std::_Deque_iterator<_Tp, _Ref, _Ptr>,
     _OI);

  template<bool _IsMove,
    typename _ITp, typename _IRef, typename _IPtr, typename _OTp>
    std::_Deque_iterator<_OTp, _OTp&, _OTp*>
    __copy_move_a1(std::_Deque_iterator<_ITp, _IRef, _IPtr>,
     std::_Deque_iterator<_ITp, _IRef, _IPtr>,
     std::_Deque_iterator<_OTp, _OTp&, _OTp*>);

  template<bool _IsMove, typename _II, typename _Tp>
    typename __gnu_cxx::__enable_if<
      __is_random_access_iter<_II>::__value,
      std::_Deque_iterator<_Tp, _Tp&, _Tp*> >::__type
    __copy_move_a1(_II, _II, std::_Deque_iterator<_Tp, _Tp&, _Tp*>);

  template<bool _IsMove, typename _II, typename _OI>
    __attribute__((__always_inline__))
   
    inline _OI
    __copy_move_a1(_II __first, _II __last, _OI __result)
    { return std::__copy_move_a2<_IsMove>(__first, __last, __result); }

  template<bool _IsMove, typename _II, typename _OI>
    __attribute__((__always_inline__))
   
    inline _OI
    __copy_move_a(_II __first, _II __last, _OI __result)
    {
      return std::__niter_wrap(__result,
  std::__copy_move_a1<_IsMove>(std::__niter_base(__first),
          std::__niter_base(__last),
          std::__niter_base(__result)));
    }

  template<bool _IsMove,
    typename _Ite, typename _Seq, typename _Cat, typename _OI>
   
    _OI
    __copy_move_a(const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&,
    const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&,
    _OI);

  template<bool _IsMove,
    typename _II, typename _Ite, typename _Seq, typename _Cat>
   
    __gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>
    __copy_move_a(_II, _II,
    const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&);

  template<bool _IsMove,
    typename _IIte, typename _ISeq, typename _ICat,
    typename _OIte, typename _OSeq, typename _OCat>
   
    ::__gnu_debug::_Safe_iterator<_OIte, _OSeq, _OCat>
    __copy_move_a(const ::__gnu_debug::_Safe_iterator<_IIte, _ISeq, _ICat>&,
    const ::__gnu_debug::_Safe_iterator<_IIte, _ISeq, _ICat>&,
    const ::__gnu_debug::_Safe_iterator<_OIte, _OSeq, _OCat>&);

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<typename _InputIterator, typename _Size, typename _OutputIterator>
   
    _OutputIterator
    __copy_n_a(_InputIterator __first, _Size __n, _OutputIterator __result,
        bool)
    {
      typedef __decltype(*__first) _InRef;
      typedef __decltype(*__result) _OutRef;
      if constexpr (!__is_trivially_assignable(_OutRef, _InRef))
 { }




      else if constexpr (__memcpyable<_OutputIterator,
            _InputIterator>::__value)
 {
   if (__builtin_expect(__n > 1, true))
     {
       __builtin_memmove(__result,
    __first,
    __n * sizeof(*__first));
       __result += __n;
     }
   else if (__n == 1)
     *__result++ = *__first;
   return __result;
 }
# 593 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
      if (__n > 0)
 {
   while (true)
     {
       *__result = *__first;
       ++__result;
       if (--__n > 0)
  ++__first;
       else
  break;
     }
 }
      return __result;
    }
#pragma GCC diagnostic pop


  template<typename _CharT, typename _Size>
    typename __gnu_cxx::__enable_if<
      __is_char<_CharT>::__value, _CharT*>::__type
    __copy_n_a(istreambuf_iterator<_CharT, char_traits<_CharT> >,
        _Size, _CharT*, bool);

  template<typename _CharT, typename _Size>
    typename __gnu_cxx::__enable_if<
      __is_char<_CharT>::__value,
      std::_Deque_iterator<_CharT, _CharT&, _CharT*> >::__type
    __copy_n_a(istreambuf_iterator<_CharT, char_traits<_CharT> >, _Size,
        std::_Deque_iterator<_CharT, _CharT&, _CharT*>,
        bool);
# 642 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _II, typename _OI>
   
    inline _OI
    copy(_II __first, _II __last, _OI __result)
    {

     
     

      ;

      return std::__copy_move_a<__is_move_iterator<_II>::__value>
      (std::__miter_base(__first), std::__miter_base(__last), __result);
    }
# 675 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _II, typename _OI>
   
    inline _OI
    move(_II __first, _II __last, _OI __result)
    {

     
     

      ;

      return std::__copy_move_a<true>(std::__miter_base(__first),
          std::__miter_base(__last), __result);
    }






#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<bool _IsMove, typename _BI1, typename _BI2>
   
    inline _BI2
    __copy_move_backward_a2(_BI1 __first, _BI1 __last, _BI2 __result)
    {
      typedef __decltype(*__first) _InRef;
      typedef __decltype(*__result) _OutRef;
      if constexpr (!__is_trivially_assignable(_OutRef, _InRef))
       { }




      else if constexpr (__memcpyable<_BI2, _BI1>::__value)
 {
   ptrdiff_t __n = std::distance(__first, __last);
   std::advance(__result, -__n);
   if (__builtin_expect(__n > 1, true))
     {
       __builtin_memmove(__result,
    __first,
    __n * sizeof(*__first));
     }
   else if (__n == 1)
     std::__assign_one<_IsMove>(__result, __first);
   return __result;
 }
# 746 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
      while (__first != __last)
 {
   --__last;
   --__result;
   std::__assign_one<_IsMove>(__result, __last);
 }
      return __result;
    }
#pragma GCC diagnostic pop




  template<bool _IsMove, typename _BI1, typename _BI2>
    __attribute__((__always_inline__))
   
    inline _BI2
    __copy_move_backward_a1(_BI1 __first, _BI1 __last, _BI2 __result)
    { return std::__copy_move_backward_a2<_IsMove>(__first, __last, __result); }

  template<bool _IsMove,
    typename _Tp, typename _Ref, typename _Ptr, typename _OI>
    _OI
    __copy_move_backward_a1(std::_Deque_iterator<_Tp, _Ref, _Ptr>,
       std::_Deque_iterator<_Tp, _Ref, _Ptr>,
       _OI);

  template<bool _IsMove,
    typename _ITp, typename _IRef, typename _IPtr, typename _OTp>
    std::_Deque_iterator<_OTp, _OTp&, _OTp*>
    __copy_move_backward_a1(
   std::_Deque_iterator<_ITp, _IRef, _IPtr>,
   std::_Deque_iterator<_ITp, _IRef, _IPtr>,
   std::_Deque_iterator<_OTp, _OTp&, _OTp*>);

  template<bool _IsMove, typename _II, typename _Tp>
    typename __gnu_cxx::__enable_if<
      __is_random_access_iter<_II>::__value,
      std::_Deque_iterator<_Tp, _Tp&, _Tp*> >::__type
    __copy_move_backward_a1(_II, _II,
       std::_Deque_iterator<_Tp, _Tp&, _Tp*>);

  template<bool _IsMove, typename _II, typename _OI>
    __attribute__((__always_inline__))
   
    inline _OI
    __copy_move_backward_a(_II __first, _II __last, _OI __result)
    {
      return std::__niter_wrap(__result,
  std::__copy_move_backward_a1<_IsMove>
    (std::__niter_base(__first), std::__niter_base(__last),
     std::__niter_base(__result)));
    }

  template<bool _IsMove,
    typename _Ite, typename _Seq, typename _Cat, typename _OI>
   
    _OI
    __copy_move_backward_a(
  const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&,
  const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&,
  _OI);

  template<bool _IsMove,
    typename _II, typename _Ite, typename _Seq, typename _Cat>
   
    __gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>
    __copy_move_backward_a(_II, _II,
  const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&);

  template<bool _IsMove,
    typename _IIte, typename _ISeq, typename _ICat,
    typename _OIte, typename _OSeq, typename _OCat>
   
    ::__gnu_debug::_Safe_iterator<_OIte, _OSeq, _OCat>
    __copy_move_backward_a(
  const ::__gnu_debug::_Safe_iterator<_IIte, _ISeq, _ICat>&,
  const ::__gnu_debug::_Safe_iterator<_IIte, _ISeq, _ICat>&,
  const ::__gnu_debug::_Safe_iterator<_OIte, _OSeq, _OCat>&);
# 844 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _BI1, typename _BI2>
    __attribute__((__always_inline__))
   
    inline _BI2
    copy_backward(_BI1 __first, _BI1 __last, _BI2 __result)
    {

     
     
     

      ;

      return std::__copy_move_backward_a<__is_move_iterator<_BI1>::__value>
      (std::__miter_base(__first), std::__miter_base(__last), __result);
    }
# 880 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _BI1, typename _BI2>
    __attribute__((__always_inline__))
   
    inline _BI2
    move_backward(_BI1 __first, _BI1 __last, _BI2 __result)
    {

     
     
     

      ;

      return std::__copy_move_backward_a<true>(std::__miter_base(__first),
            std::__miter_base(__last),
            __result);
    }






#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<typename _ForwardIterator, typename _Tp>
   
    inline void
    __fill_a1(_ForwardIterator __first, _ForwardIterator __last,
       const _Tp& __value)
    {
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"




      const bool __load_outside_loop =


     __is_trivially_constructible(_Tp, const _Tp&)
     && __is_trivially_assignable(__decltype(*__first), const _Tp&)




     && sizeof(_Tp) <= sizeof(long long);
#pragma GCC diagnostic pop



      typedef typename __gnu_cxx::__conditional_type<__load_outside_loop,
           const _Tp,
           const _Tp&>::__type _Up;
      _Up __val(__value);
      for (; __first != __last; ++__first)
 *__first = __val;
    }
#pragma GCC diagnostic pop


  template<typename _Up, typename _Tp>
   
    inline typename
    __gnu_cxx::__enable_if<__is_byte<_Up>::__value
        && (__are_same<_Up, _Tp>::__value
       || __memcpyable_integer<_Tp>::__value),
      void>::__type
    __fill_a1(_Up* __first, _Up* __last, const _Tp& __x)
    {


      const _Up __val = __x;







      if (const size_t __len = __last - __first)
 __builtin_memset(__first, static_cast<unsigned char>(__val), __len);
    }

  template<typename _Ite, typename _Cont, typename _Tp>
    __attribute__((__always_inline__))
   
    inline void
    __fill_a1(::__gnu_cxx::__normal_iterator<_Ite, _Cont> __first,
       ::__gnu_cxx::__normal_iterator<_Ite, _Cont> __last,
       const _Tp& __value)
    { std::__fill_a1(__first.base(), __last.base(), __value); }

  template<typename _Tp, typename _VTp>
    void
    __fill_a1(const std::_Deque_iterator<_Tp, _Tp&, _Tp*>&,
       const std::_Deque_iterator<_Tp, _Tp&, _Tp*>&,
       const _VTp&);

 
  void
  __fill_a1(std::_Bit_iterator, std::_Bit_iterator,
     const bool&);

  template<typename _FIte, typename _Tp>
    __attribute__((__always_inline__))
   
    inline void
    __fill_a(_FIte __first, _FIte __last, const _Tp& __value)
    { std::__fill_a1(__first, __last, __value); }

  template<typename _Ite, typename _Seq, typename _Cat, typename _Tp>
   
    void
    __fill_a(const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&,
      const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&,
      const _Tp&);
# 1010 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _ForwardIterator, typename _Tp>
    __attribute__((__always_inline__))
   
    inline void
    fill(_ForwardIterator __first, _ForwardIterator __last, const _Tp& __value)
    {

     

      ;

      std::__fill_a(__first, __last, __value);
    }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"

  inline constexpr int
  __size_to_integer(int __n) { return __n; }
  inline constexpr unsigned
  __size_to_integer(unsigned __n) { return __n; }
  inline constexpr long
  __size_to_integer(long __n) { return __n; }
  inline constexpr unsigned long
  __size_to_integer(unsigned long __n) { return __n; }
  inline constexpr long long
  __size_to_integer(long long __n) { return __n; }
  inline constexpr unsigned long long
  __size_to_integer(unsigned long long __n) { return __n; }
# 1065 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  inline constexpr long long
  __size_to_integer(float __n) { return (long long)__n; }
  inline constexpr long long
  __size_to_integer(double __n) { return (long long)__n; }
  inline constexpr long long
  __size_to_integer(long double __n) { return (long long)__n; }




#pragma GCC diagnostic pop

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
#pragma GCC diagnostic ignored "-Wlong-long"
  template<typename _OutputIterator, typename _Size, typename _Tp>
   
    inline _OutputIterator
    __fill_n_a1(_OutputIterator __first, _Size __n, const _Tp& __value)
    {

      const bool __load_outside_loop =


     __is_trivially_constructible(_Tp, const _Tp&)
     && __is_trivially_assignable(__decltype(*__first), const _Tp&)




     && sizeof(_Tp) <= sizeof(long long);



      typedef typename __gnu_cxx::__conditional_type<__load_outside_loop,
           const _Tp,
           const _Tp&>::__type _Up;
      _Up __val(__value);
      for (; __n > 0; --__n, (void) ++__first)
 *__first = __val;
      return __first;
    }
#pragma GCC diagnostic pop

  template<typename _Ite, typename _Seq, typename _Cat, typename _Size,
    typename _Tp>
   
    ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>
    __fill_n_a(const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>& __first,
        _Size __n, const _Tp& __value,
        std::input_iterator_tag);

  template<typename _OutputIterator, typename _Size, typename _Tp>
    __attribute__((__always_inline__))
   
    inline _OutputIterator
    __fill_n_a(_OutputIterator __first, _Size __n, const _Tp& __value,
        std::output_iterator_tag)
    {

      static_assert(is_integral<_Size>{}, "fill_n must pass integral size");

      return __fill_n_a1(__first, __n, __value);
    }

  template<typename _OutputIterator, typename _Size, typename _Tp>
    __attribute__((__always_inline__))
   
    inline _OutputIterator
    __fill_n_a(_OutputIterator __first, _Size __n, const _Tp& __value,
        std::input_iterator_tag)
    {

      static_assert(is_integral<_Size>{}, "fill_n must pass integral size");

      return __fill_n_a1(__first, __n, __value);
    }

  template<typename _OutputIterator, typename _Size, typename _Tp>
    __attribute__((__always_inline__))
   
    inline _OutputIterator
    __fill_n_a(_OutputIterator __first, _Size __n, const _Tp& __value,
        std::random_access_iterator_tag)
    {

      static_assert(is_integral<_Size>{}, "fill_n must pass integral size");

      if (__n <= 0)
 return __first;

      ;

      std::__fill_a(__first, __first + __n, __value);
      return __first + __n;
    }
# 1179 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _OI, typename _Size, typename _Tp>
    __attribute__((__always_inline__))
   
    inline _OI
    fill_n(_OI __first, _Size __n, const _Tp& __value)
    {

     

      return std::__fill_n_a(__first, std::__size_to_integer(__n), __value,
          std::__iterator_category(__first));
    }

  template<bool _BoolType>
    struct __equal
    {
      template<typename _II1, typename _II2>

 static bool
 equal(_II1 __first1, _II1 __last1, _II2 __first2)
 {
   for (; __first1 != __last1; ++__first1, (void) ++__first2)
     if (!(*__first1 == *__first2))
       return false;
   return true;
 }
    };

  template<>
    struct __equal<true>
    {
      template<typename _Tp>

 static bool
 equal(const _Tp* __first1, const _Tp* __last1, const _Tp* __first2)
 {
   if (const size_t __len = (__last1 - __first1))
     return !std::__memcmp(__first1, __first2, __len);
   return true;
 }
    };

  template<typename _Tp, typename _Ref, typename _Ptr, typename _II>
    typename __gnu_cxx::__enable_if<
      __is_random_access_iter<_II>::__value, bool>::__type
    __equal_aux1(std::_Deque_iterator<_Tp, _Ref, _Ptr>,
   std::_Deque_iterator<_Tp, _Ref, _Ptr>,
   _II);

  template<typename _Tp1, typename _Ref1, typename _Ptr1,
    typename _Tp2, typename _Ref2, typename _Ptr2>
    bool
    __equal_aux1(std::_Deque_iterator<_Tp1, _Ref1, _Ptr1>,
   std::_Deque_iterator<_Tp1, _Ref1, _Ptr1>,
   std::_Deque_iterator<_Tp2, _Ref2, _Ptr2>);

  template<typename _II, typename _Tp, typename _Ref, typename _Ptr>
    typename __gnu_cxx::__enable_if<
      __is_random_access_iter<_II>::__value, bool>::__type
    __equal_aux1(_II, _II,
  std::_Deque_iterator<_Tp, _Ref, _Ptr>);

  template<typename _II1, typename _II2>
   
    inline bool
    __equal_aux1(_II1 __first1, _II1 __last1, _II2 __first2)
    {
      typedef typename iterator_traits<_II1>::value_type _ValueType1;
      const bool __simple = ((__is_integer<_ValueType1>::__value

    || __is_pointer(_ValueType1)





        ) && __memcmpable<_II1, _II2>::__value);
      return std::__equal<__simple>::equal(__first1, __last1, __first2);
    }

  template<typename _II1, typename _II2>
    __attribute__((__always_inline__))
   
    inline bool
    __equal_aux(_II1 __first1, _II1 __last1, _II2 __first2)
    {
      return std::__equal_aux1(std::__niter_base(__first1),
          std::__niter_base(__last1),
          std::__niter_base(__first2));
    }

  template<typename _II1, typename _Seq1, typename _Cat1, typename _II2>
   
    bool
    __equal_aux(const ::__gnu_debug::_Safe_iterator<_II1, _Seq1, _Cat1>&,
  const ::__gnu_debug::_Safe_iterator<_II1, _Seq1, _Cat1>&,
  _II2);

  template<typename _II1, typename _II2, typename _Seq2, typename _Cat2>
   
    bool
    __equal_aux(_II1, _II1,
  const ::__gnu_debug::_Safe_iterator<_II2, _Seq2, _Cat2>&);

  template<typename _II1, typename _Seq1, typename _Cat1,
    typename _II2, typename _Seq2, typename _Cat2>
   
    bool
    __equal_aux(const ::__gnu_debug::_Safe_iterator<_II1, _Seq1, _Cat1>&,
  const ::__gnu_debug::_Safe_iterator<_II1, _Seq1, _Cat1>&,
  const ::__gnu_debug::_Safe_iterator<_II2, _Seq2, _Cat2>&);

  template<typename, typename>
    struct __lc_rai
    {
      template<typename _II1, typename _II2>

 static _II1
 __newlast1(_II1, _II1 __last1, _II2, _II2)
 { return __last1; }

      template<typename _II>

 static bool
 __cnd2(_II __first, _II __last)
 { return __first != __last; }
    };

  template<>
    struct __lc_rai<random_access_iterator_tag, random_access_iterator_tag>
    {
      template<typename _RAI1, typename _RAI2>

 static _RAI1
 __newlast1(_RAI1 __first1, _RAI1 __last1,
     _RAI2 __first2, _RAI2 __last2)
 {
   const typename iterator_traits<_RAI1>::difference_type
     __diff1 = __last1 - __first1;
   const typename iterator_traits<_RAI2>::difference_type
     __diff2 = __last2 - __first2;
   return __diff2 < __diff1 ? __first1 + __diff2 : __last1;
 }

      template<typename _RAI>
 static bool
 __cnd2(_RAI, _RAI)
 { return true; }
    };

  template<typename _II1, typename _II2, typename _Compare>
   
    bool
    __lexicographical_compare_impl(_II1 __first1, _II1 __last1,
       _II2 __first2, _II2 __last2,
       _Compare __comp)
    {
      typedef typename iterator_traits<_II1>::iterator_category _Category1;
      typedef typename iterator_traits<_II2>::iterator_category _Category2;
      typedef std::__lc_rai<_Category1, _Category2> __rai_type;

      __last1 = __rai_type::__newlast1(__first1, __last1, __first2, __last2);
      for (; __first1 != __last1 && __rai_type::__cnd2(__first2, __last2);
    ++__first1, (void)++__first2)
 {
   if (__comp(__first1, __first2))
     return true;
   if (__comp(__first2, __first1))
     return false;
 }
      return __first1 == __last1 && __first2 != __last2;
    }

  template<bool _BoolType>
    struct __lexicographical_compare
    {
      template<typename _II1, typename _II2>

 static bool
 __lc(_II1 __first1, _II1 __last1, _II2 __first2, _II2 __last2)
 {
   using __gnu_cxx::__ops::__iter_less_iter;
   return std::__lexicographical_compare_impl(__first1, __last1,
           __first2, __last2,
           __iter_less_iter());
 }

      template<typename _II1, typename _II2>

 static int
 __3way(_II1 __first1, _II1 __last1, _II2 __first2, _II2 __last2)
 {
   while (__first1 != __last1)
     {
       if (__first2 == __last2)
  return +1;
       if (*__first1 < *__first2)
  return -1;
       if (*__first2 < *__first1)
  return +1;
       ++__first1;
       ++__first2;
     }
   return int(__first2 == __last2) - 1;
 }
    };

  template<>
    struct __lexicographical_compare<true>
    {
      template<typename _Tp, typename _Up>

 static bool
 __lc(const _Tp* __first1, const _Tp* __last1,
      const _Up* __first2, const _Up* __last2)
 { return __3way(__first1, __last1, __first2, __last2) < 0; }

      template<typename _Tp, typename _Up>

 static ptrdiff_t
 __3way(const _Tp* __first1, const _Tp* __last1,
        const _Up* __first2, const _Up* __last2)
 {
   const size_t __len1 = __last1 - __first1;
   const size_t __len2 = __last2 - __first2;
   if (const size_t __len = std::min(__len1, __len2))
     if (int __result = std::__memcmp(__first1, __first2, __len))
       return __result;
   return ptrdiff_t(__len1 - __len2);
 }
    };

  template<typename _II1, typename _II2>
   
    inline bool
    __lexicographical_compare_aux1(_II1 __first1, _II1 __last1,
       _II2 __first2, _II2 __last2)
    {
      typedef typename iterator_traits<_II1>::value_type _ValueType1;
      typedef typename iterator_traits<_II2>::value_type _ValueType2;

      const bool __simple =
 (__is_memcmp_ordered_with<_ValueType1, _ValueType2>::__value
  && __is_pointer(_II1) && __is_pointer(_II2)







  );




      return std::__lexicographical_compare<__simple>::__lc(__first1, __last1,
           __first2, __last2);
    }

  template<typename _Tp1, typename _Ref1, typename _Ptr1,
    typename _Tp2>
    bool
    __lexicographical_compare_aux1(
 std::_Deque_iterator<_Tp1, _Ref1, _Ptr1>,
 std::_Deque_iterator<_Tp1, _Ref1, _Ptr1>,
 _Tp2*, _Tp2*);

  template<typename _Tp1,
    typename _Tp2, typename _Ref2, typename _Ptr2>
    bool
    __lexicographical_compare_aux1(_Tp1*, _Tp1*,
 std::_Deque_iterator<_Tp2, _Ref2, _Ptr2>,
 std::_Deque_iterator<_Tp2, _Ref2, _Ptr2>);

  template<typename _Tp1, typename _Ref1, typename _Ptr1,
    typename _Tp2, typename _Ref2, typename _Ptr2>
    bool
    __lexicographical_compare_aux1(
 std::_Deque_iterator<_Tp1, _Ref1, _Ptr1>,
 std::_Deque_iterator<_Tp1, _Ref1, _Ptr1>,
 std::_Deque_iterator<_Tp2, _Ref2, _Ptr2>,
 std::_Deque_iterator<_Tp2, _Ref2, _Ptr2>);

  template<typename _II1, typename _II2>
   
    inline bool
    __lexicographical_compare_aux(_II1 __first1, _II1 __last1,
      _II2 __first2, _II2 __last2)
    {
      return std::__lexicographical_compare_aux1(std::__niter_base(__first1),
       std::__niter_base(__last1),
       std::__niter_base(__first2),
       std::__niter_base(__last2));
    }

  template<typename _Iter1, typename _Seq1, typename _Cat1,
    typename _II2>
   
    bool
    __lexicographical_compare_aux(
  const ::__gnu_debug::_Safe_iterator<_Iter1, _Seq1, _Cat1>&,
  const ::__gnu_debug::_Safe_iterator<_Iter1, _Seq1, _Cat1>&,
  _II2, _II2);

  template<typename _II1,
    typename _Iter2, typename _Seq2, typename _Cat2>
   
    bool
    __lexicographical_compare_aux(
  _II1, _II1,
  const ::__gnu_debug::_Safe_iterator<_Iter2, _Seq2, _Cat2>&,
  const ::__gnu_debug::_Safe_iterator<_Iter2, _Seq2, _Cat2>&);

  template<typename _Iter1, typename _Seq1, typename _Cat1,
    typename _Iter2, typename _Seq2, typename _Cat2>
   
    bool
    __lexicographical_compare_aux(
  const ::__gnu_debug::_Safe_iterator<_Iter1, _Seq1, _Cat1>&,
  const ::__gnu_debug::_Safe_iterator<_Iter1, _Seq1, _Cat1>&,
  const ::__gnu_debug::_Safe_iterator<_Iter2, _Seq2, _Cat2>&,
  const ::__gnu_debug::_Safe_iterator<_Iter2, _Seq2, _Cat2>&);

  template<typename _ForwardIterator, typename _Tp, typename _Compare>
   
    _ForwardIterator
    __lower_bound(_ForwardIterator __first, _ForwardIterator __last,
    const _Tp& __val, _Compare __comp)
    {
      typedef typename iterator_traits<_ForwardIterator>::difference_type
 _DistanceType;

      _DistanceType __len = std::distance(__first, __last);

      while (__len > 0)
 {
   _DistanceType __half = __len >> 1;
   _ForwardIterator __middle = __first;
   std::advance(__middle, __half);
   if (__comp(__middle, __val))
     {
       __first = __middle;
       ++__first;
       __len = __len - __half - 1;
     }
   else
     __len = __half;
 }
      return __first;
    }
# 1542 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _ForwardIterator, typename _Tp>
    [[__nodiscard__]]
    inline _ForwardIterator
    lower_bound(_ForwardIterator __first, _ForwardIterator __last,
  const _Tp& __val)
    {

     
     

      ;

      return std::__lower_bound(__first, __last, __val,
    __gnu_cxx::__ops::__iter_less_val());
    }



  template<typename _Tp>
    inline constexpr _Tp
    __lg(_Tp __n)
    {

      return std::__bit_width(make_unsigned_t<_Tp>(__n)) - 1;
# 1578 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
    }


# 1594 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _II1, typename _II2>
    [[__nodiscard__]]
    inline bool
    equal(_II1 __first1, _II1 __last1, _II2 __first2)
    {

     
     
     


      ;

      return std::__equal_aux(__first1, __last1, __first2);
    }
# 1625 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _IIter1, typename _IIter2, typename _BinaryPredicate>
    [[__nodiscard__]]
    inline bool
    equal(_IIter1 __first1, _IIter1 __last1,
   _IIter2 __first2, _BinaryPredicate __binary_pred)
    {

     
     
      ;

      for (; __first1 != __last1; ++__first1, (void)++__first2)
 if (!bool(__binary_pred(*__first1, *__first2)))
   return false;
      return true;
    }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"


  template<typename _II1, typename _II2>
   
    inline bool
    __equal4(_II1 __first1, _II1 __last1, _II2 __first2, _II2 __last2)
    {
      using _RATag = random_access_iterator_tag;
      using _Cat1 = typename iterator_traits<_II1>::iterator_category;
      using _Cat2 = typename iterator_traits<_II2>::iterator_category;
      using _RAIters = __and_<is_same<_Cat1, _RATag>, is_same<_Cat2, _RATag>>;
      if constexpr (_RAIters::value)
 {
   if ((__last1 - __first1) != (__last2 - __first2))
     return false;
   return std::equal(__first1, __last1, __first2);
 }
      else
 {
   for (; __first1 != __last1 && __first2 != __last2;
        ++__first1, (void)++__first2)
     if (!(*__first1 == *__first2))
       return false;
   return __first1 == __last1 && __first2 == __last2;
 }
    }


  template<typename _II1, typename _II2, typename _BinaryPredicate>
   
    inline bool
    __equal4(_II1 __first1, _II1 __last1, _II2 __first2, _II2 __last2,
      _BinaryPredicate __binary_pred)
    {
      using _RATag = random_access_iterator_tag;
      using _Cat1 = typename iterator_traits<_II1>::iterator_category;
      using _Cat2 = typename iterator_traits<_II2>::iterator_category;
      using _RAIters = __and_<is_same<_Cat1, _RATag>, is_same<_Cat2, _RATag>>;
      if constexpr (_RAIters::value)
 {
   if ((__last1 - __first1) != (__last2 - __first2))
     return false;
   return std::equal(__first1, __last1, __first2,
           __binary_pred);
 }
      else
 {
   for (; __first1 != __last1 && __first2 != __last2;
        ++__first1, (void)++__first2)
     if (!bool(__binary_pred(*__first1, *__first2)))
       return false;
   return __first1 == __last1 && __first2 == __last2;
 }
    }
#pragma GCC diagnostic pop
# 1716 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _II1, typename _II2>
    [[__nodiscard__]]
    inline bool
    equal(_II1 __first1, _II1 __last1, _II2 __first2, _II2 __last2)
    {

     
     
     


      ;
      ;

      return std::__equal4(__first1, __last1, __first2, __last2);
    }
# 1749 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _IIter1, typename _IIter2, typename _BinaryPredicate>
    [[__nodiscard__]]
    inline bool
    equal(_IIter1 __first1, _IIter1 __last1,
   _IIter2 __first2, _IIter2 __last2, _BinaryPredicate __binary_pred)
    {

     
     
      ;
      ;

      return std::__equal4(__first1, __last1, __first2, __last2,
          __binary_pred);
    }
# 1781 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _II1, typename _II2>
    [[__nodiscard__]]
    inline bool
    lexicographical_compare(_II1 __first1, _II1 __last1,
       _II2 __first2, _II2 __last2)
    {





     
     
     
     
      ;
      ;

      return std::__lexicographical_compare_aux(__first1, __last1,
      __first2, __last2);
    }
# 1816 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _II1, typename _II2, typename _Compare>
    [[__nodiscard__]]
    inline bool
    lexicographical_compare(_II1 __first1, _II1 __last1,
       _II2 __first2, _II2 __last2, _Compare __comp)
    {

     
     
      ;
      ;

      return std::__lexicographical_compare_impl
 (__first1, __last1, __first2, __last2,
  __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }
# 1931 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _BinaryPredicate>
   
    pair<_InputIterator1, _InputIterator2>
    __mismatch(_InputIterator1 __first1, _InputIterator1 __last1,
        _InputIterator2 __first2, _BinaryPredicate __binary_pred)
    {
      while (__first1 != __last1 && __binary_pred(__first1, __first2))
 {
   ++__first1;
   ++__first2;
 }
      return pair<_InputIterator1, _InputIterator2>(__first1, __first2);
    }
# 1959 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _InputIterator1, typename _InputIterator2>
    [[__nodiscard__]]
    inline pair<_InputIterator1, _InputIterator2>
    mismatch(_InputIterator1 __first1, _InputIterator1 __last1,
      _InputIterator2 __first2)
    {

     
     
     


      ;

      return std::__mismatch(__first1, __last1, __first2,
        __gnu_cxx::__ops::__iter_equal_to_iter());
    }
# 1993 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _BinaryPredicate>
    [[__nodiscard__]]
    inline pair<_InputIterator1, _InputIterator2>
    mismatch(_InputIterator1 __first1, _InputIterator1 __last1,
      _InputIterator2 __first2, _BinaryPredicate __binary_pred)
    {

     
     
      ;

      return std::__mismatch(__first1, __last1, __first2,
 __gnu_cxx::__ops::__iter_comp_iter(__binary_pred));
    }


  template<typename _InputIterator1, typename _InputIterator2,
    typename _BinaryPredicate>
   
    pair<_InputIterator1, _InputIterator2>
    __mismatch(_InputIterator1 __first1, _InputIterator1 __last1,
        _InputIterator2 __first2, _InputIterator2 __last2,
        _BinaryPredicate __binary_pred)
    {
      while (__first1 != __last1 && __first2 != __last2
      && __binary_pred(__first1, __first2))
 {
   ++__first1;
   ++__first2;
 }
      return pair<_InputIterator1, _InputIterator2>(__first1, __first2);
    }
# 2041 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _InputIterator1, typename _InputIterator2>
    [[__nodiscard__]]
    inline pair<_InputIterator1, _InputIterator2>
    mismatch(_InputIterator1 __first1, _InputIterator1 __last1,
      _InputIterator2 __first2, _InputIterator2 __last2)
    {

     
     
     


      ;
      ;

      return std::__mismatch(__first1, __last1, __first2, __last2,
        __gnu_cxx::__ops::__iter_equal_to_iter());
    }
# 2077 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _BinaryPredicate>
    [[__nodiscard__]]
    inline pair<_InputIterator1, _InputIterator2>
    mismatch(_InputIterator1 __first1, _InputIterator1 __last1,
      _InputIterator2 __first2, _InputIterator2 __last2,
      _BinaryPredicate __binary_pred)
    {

     
     
      ;
      ;

      return std::__mismatch(__first1, __last1, __first2, __last2,
        __gnu_cxx::__ops::__iter_comp_iter(__binary_pred));
    }





  template<typename _Iterator, typename _Predicate>
   
    inline _Iterator
    __find_if(_Iterator __first, _Iterator __last, _Predicate __pred)
    {
#pragma GCC unroll 4
      while (__first != __last && !__pred(__first))
 ++__first;
      return __first;
    }

  template<typename _InputIterator, typename _Predicate>
   
    typename iterator_traits<_InputIterator>::difference_type
    __count_if(_InputIterator __first, _InputIterator __last, _Predicate __pred)
    {
      typename iterator_traits<_InputIterator>::difference_type __n = 0;
      for (; __first != __last; ++__first)
 if (__pred(__first))
   ++__n;
      return __n;
    }

  template<typename _ForwardIterator, typename _Predicate>
   
    _ForwardIterator
    __remove_if(_ForwardIterator __first, _ForwardIterator __last,
  _Predicate __pred)
    {
      __first = std::__find_if(__first, __last, __pred);
      if (__first == __last)
 return __first;
      _ForwardIterator __result = __first;
      ++__first;
      for (; __first != __last; ++__first)
 if (!__pred(__first))
   {
     *__result = std::move(*__first);
     ++__result;
   }
      return __result;
    }

  template<typename _ForwardIterator1, typename _ForwardIterator2,
    typename _BinaryPredicate>
   
    _ForwardIterator1
    __search(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
      _ForwardIterator2 __first2, _ForwardIterator2 __last2,
      _BinaryPredicate __predicate)
    {

      if (__first1 == __last1 || __first2 == __last2)
 return __first1;


      _ForwardIterator2 __p1(__first2);
      if (++__p1 == __last2)
 return std::__find_if(__first1, __last1,
  __gnu_cxx::__ops::__iter_comp_iter(__predicate, __first2));


      _ForwardIterator1 __current = __first1;

      for (;;)
 {
   __first1 =
     std::__find_if(__first1, __last1,
  __gnu_cxx::__ops::__iter_comp_iter(__predicate, __first2));

   if (__first1 == __last1)
     return __last1;

   _ForwardIterator2 __p = __p1;
   __current = __first1;
   if (++__current == __last1)
     return __last1;

   while (__predicate(__current, __p))
     {
       if (++__p == __last2)
  return __first1;
       if (++__current == __last1)
  return __last1;
     }
   ++__first1;
 }
      return __first1;
    }


  template<typename _ForwardIterator1, typename _ForwardIterator2,
    typename _BinaryPredicate>
   
    bool
    __is_permutation(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
       _ForwardIterator2 __first2, _BinaryPredicate __pred)
    {


      for (; __first1 != __last1; ++__first1, (void)++__first2)
 if (!__pred(__first1, __first2))
   break;

      if (__first1 == __last1)
 return true;



      _ForwardIterator2 __last2 = __first2;
      std::advance(__last2, std::distance(__first1, __last1));
      for (_ForwardIterator1 __scan = __first1; __scan != __last1; ++__scan)
 {
   if (__scan != std::__find_if(__first1, __scan,
     __gnu_cxx::__ops::__iter_comp_iter(__pred, __scan)))
     continue;

   auto __matches
     = std::__count_if(__first2, __last2,
   __gnu_cxx::__ops::__iter_comp_iter(__pred, __scan));
   if (0 == __matches ||
       std::__count_if(__scan, __last1,
   __gnu_cxx::__ops::__iter_comp_iter(__pred, __scan))
       != __matches)
     return false;
 }
      return true;
    }
# 2240 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2>
   
    inline bool
    is_permutation(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
     _ForwardIterator2 __first2)
    {

     
     
     


      ;

      return std::__is_permutation(__first1, __last1, __first2,
       __gnu_cxx::__ops::__iter_equal_to_iter());
    }



# 2282 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2,
    typename _BinaryPredicate>
   
    inline _ForwardIterator1
    search(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
    _ForwardIterator2 __first2, _ForwardIterator2 __last2,
    _BinaryPredicate __predicate)
    {

     
     
     


      ;
      ;

      return std::__search(__first1, __last1, __first2, __last2,
      __gnu_cxx::__ops::__iter_comp_iter(__predicate));
    }



}
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 2 3





# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 1 3
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{




 
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++14-extensions"
#pragma GCC diagnostic ignored "-Wc++17-extensions"


  struct __allocator_traits_base
  {



    template<typename _Tp, typename _Up, typename = void>

      struct __rebind : __replace_first_arg<_Tp, _Up>
      {
 static_assert(is_same<
   typename __replace_first_arg<_Tp, typename _Tp::value_type>::type,
   _Tp>::value,
   "allocator_traits<A>::rebind_alloc<A::value_type> must be A");
      };

    template<typename _Tp, typename _Up>




      struct __rebind<_Tp, _Up,
        __void_t<typename _Tp::template rebind<_Up>::other>>

      {
 using type = typename _Tp::template rebind<_Up>::other;

 static_assert(is_same<
   typename _Tp::template rebind<typename _Tp::value_type>::other,
   _Tp>::value,
   "allocator_traits<A>::rebind_alloc<A::value_type> must be A");
      };

  protected:
    template<typename _Tp>
      using __pointer = typename _Tp::pointer;
    template<typename _Tp>
      using __c_pointer = typename _Tp::const_pointer;
    template<typename _Tp>
      using __v_pointer = typename _Tp::void_pointer;
    template<typename _Tp>
      using __cv_pointer = typename _Tp::const_void_pointer;
    template<typename _Tp>
      using __pocca = typename _Tp::propagate_on_container_copy_assignment;
    template<typename _Tp>
      using __pocma = typename _Tp::propagate_on_container_move_assignment;
    template<typename _Tp>
      using __pocs = typename _Tp::propagate_on_container_swap;
    template<typename _Tp>
      using __equal = __type_identity<typename _Tp::is_always_equal>;
# 115 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
    template<typename _Alloc, typename _Sz, typename _Vp>
      using __allocate_hint_t
 = decltype(std::declval<_Alloc&>()
       .allocate(std::declval<_Sz>(), std::declval<_Vp>()));
    template<typename _Alloc, typename _Sz, typename _Vp, typename = void>
      static constexpr bool __has_allocate_hint = false;
    template<typename _Alloc, typename _Sz, typename _Vp>
      static constexpr bool
      __has_allocate_hint<_Alloc, _Sz, _Vp,
     __void_t<__allocate_hint_t<_Alloc, _Sz, _Vp>>>
 = true;
# 152 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
    template<typename _Alloc, typename _Tp, typename... _Args>
      using __construct_t
 = decltype(std::declval<_Alloc&>().construct(std::declval<_Tp*>(),
           std::declval<_Args>()...));
    template<typename _Alloc, typename _Tp, typename, typename... _Args>
      static constexpr bool __has_construct_impl = false;
    template<typename _Alloc, typename _Tp, typename... _Args>
      static constexpr bool
      __has_construct_impl<_Alloc, _Tp,
      __void_t<__construct_t<_Alloc, _Tp, _Args...>>,
      _Args...>
 = true;
    template<typename _Alloc, typename _Tp, typename... _Args>
      static constexpr bool __has_construct
 = __has_construct_impl<_Alloc, _Tp, void, _Args...>;
    template<typename _Tp, typename... _Args>
      using __new_expr_t
 = decltype(::new((void*)0) _Tp(std::declval<_Args>()...));
    template<typename _Tp, typename, typename... _Args>
      static constexpr bool __has_new_expr = false;
    template<typename _Tp, typename... _Args>
      static constexpr bool
      __has_new_expr<_Tp, __void_t<__new_expr_t<_Tp, _Args...>>, _Args...>
 = true;
    template<typename _Alloc, typename _Tp, typename... _Args>
      static constexpr bool __can_construct
 = __has_construct<_Alloc, _Tp, _Args...>
     || __has_new_expr<_Tp, void, _Args...>;
# 189 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
    template<typename _Alloc, typename _Tp>
      using __destroy_t
 = decltype(std::declval<_Alloc&>().destroy(std::declval<_Tp*>()));
    template<typename _Alloc, typename _Tp, typename = void>
      static constexpr bool __has_destroy = false;
    template<typename _Alloc, typename _Tp>
      static constexpr bool __has_destroy<_Alloc, _Tp,
       __void_t<__destroy_t<_Alloc, _Tp>>>
 = true;
# 207 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
    template<typename _Alloc>
      using __max_size_t = decltype(std::declval<const _Alloc&>().max_size());
    template<typename _Alloc, typename = void>
      static constexpr bool __has_max_size = false;
    template<typename _Alloc>
      static constexpr bool __has_max_size<_Alloc,
        __void_t<__max_size_t<_Alloc>>>
 = true;
# 225 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
    template<typename _Alloc>
      using __soccc_t
 = decltype(std::declval<const _Alloc&>()
       .select_on_container_copy_construction());
    template<typename _Alloc, typename = void>
      static constexpr bool __has_soccc = false;
    template<typename _Alloc>
      static constexpr bool __has_soccc<_Alloc, __void_t<__soccc_t<_Alloc>>>
 = true;

  };

  template<typename _Alloc, typename _Up>
    using __alloc_rebind
      = typename __allocator_traits_base::template __rebind<_Alloc, _Up>::type;
# 248 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<typename _Alloc>
    struct allocator_traits : __allocator_traits_base
    {

      typedef _Alloc allocator_type;

      typedef typename _Alloc::value_type value_type;






      using pointer = __detected_or_t<value_type*, __pointer, _Alloc>;

    private:

      template<template<typename> class _Func, typename _Tp, typename = void>
 struct _Ptr
 {
   using type = typename pointer_traits<pointer>::template rebind<_Tp>;
 };

      template<template<typename> class _Func, typename _Tp>
 struct _Ptr<_Func, _Tp, __void_t<_Func<_Alloc>>>
 {
   using type = _Func<_Alloc>;
 };


      template<typename _A2, typename _PtrT, typename = void>
 struct _Diff
 { using type = typename pointer_traits<_PtrT>::difference_type; };

      template<typename _A2, typename _PtrT>
 struct _Diff<_A2, _PtrT, __void_t<typename _A2::difference_type>>
 { using type = typename _A2::difference_type; };


      template<typename _A2, typename _DiffT, typename = void>
 struct _Size : make_unsigned<_DiffT> { };

      template<typename _A2, typename _DiffT>
 struct _Size<_A2, _DiffT, __void_t<typename _A2::size_type>>
 { using type = typename _A2::size_type; };

    public:






      using const_pointer = typename _Ptr<__c_pointer, const value_type>::type;







      using void_pointer = typename _Ptr<__v_pointer, void>::type;







      using const_void_pointer = typename _Ptr<__cv_pointer, const void>::type;







      using difference_type = typename _Diff<_Alloc, pointer>::type;







      using size_type = typename _Size<_Alloc, difference_type>::type;







      using propagate_on_container_copy_assignment
 = __detected_or_t<false_type, __pocca, _Alloc>;







      using propagate_on_container_move_assignment
 = __detected_or_t<false_type, __pocma, _Alloc>;







      using propagate_on_container_swap
 = __detected_or_t<false_type, __pocs, _Alloc>;







      using is_always_equal
 = typename __detected_or_t<is_empty<_Alloc>, __equal, _Alloc>::type;

      template<typename _Tp>
 using rebind_alloc = __alloc_rebind<_Alloc, _Tp>;
      template<typename _Tp>
 using rebind_traits = allocator_traits<rebind_alloc<_Tp>>;
# 383 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      [[__nodiscard__]] static pointer
      allocate(_Alloc& __a, size_type __n)
      { return __a.allocate(__n); }
# 398 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      [[__nodiscard__]] static pointer
      allocate(_Alloc& __a, size_type __n, const_void_pointer __hint)
      {
 if constexpr (__has_allocate_hint<_Alloc, size_type, const_void_pointer>)
   return __a.allocate(__n, __hint);
 else
   return __a.allocate(__n);
      }
# 415 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      static void
      deallocate(_Alloc& __a, pointer __p, size_type __n)
      { __a.deallocate(__p, __n); }
# 430 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      template<typename _Tp, typename... _Args>




 static __enable_if_t<__can_construct<_Alloc, _Tp, _Args...>>

 construct(_Alloc& __a, _Tp* __p, _Args&&... __args)
 noexcept(_S_nothrow_construct<_Tp, _Args...>())
 {
   if constexpr (__has_construct<_Alloc, _Tp, _Args...>)
     __a.construct(__p, std::forward<_Args>(__args)...);
   else
     std::_Construct(__p, std::forward<_Args>(__args)...);
 }
# 454 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      template<typename _Tp>
 static void
 destroy(_Alloc& __a, _Tp* __p)
 noexcept(_S_nothrow_destroy<_Tp>())
 {
   if constexpr (__has_destroy<_Alloc, _Tp>)
     __a.destroy(__p);
   else
     std::_Destroy(__p);
 }
# 473 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      static size_type
      max_size(const _Alloc& __a) noexcept
      {
 if constexpr (__has_max_size<_Alloc>)
   return __a.max_size();
 else


   return __gnu_cxx::__numeric_traits<size_type>::__max
     / sizeof(value_type);
      }
# 493 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      static _Alloc
      select_on_container_copy_construction(const _Alloc& __rhs)
      {
 if constexpr (__has_soccc<_Alloc>)
   return __rhs.select_on_container_copy_construction();
 else
   return __rhs;
      }

    private:

      template<typename _Tp, typename... _Args>
 static constexpr bool
 _S_nothrow_construct(_Alloc* __a = nullptr, _Tp* __p = nullptr)
 {
   if constexpr (__has_construct<_Alloc, _Tp, _Args...>)
     return noexcept(__a->construct(__p, std::declval<_Args>()...));
   else
     return __is_nothrow_new_constructible<_Tp, _Args...>;
 }

      template<typename _Tp>
 static constexpr bool
 _S_nothrow_destroy(_Alloc* __a = nullptr, _Tp* __p = nullptr)
 {
   if constexpr (__has_destroy<_Alloc, _Tp>)
     return noexcept(__a->destroy(__p));
   else
     return is_nothrow_destructible<_Tp>::value;
 }
# 548 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
    };
#pragma GCC diagnostic pop
# 559 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<typename _Tp>
    struct allocator_traits<allocator<_Tp>>
    {

      using allocator_type = allocator<_Tp>;


      using value_type = _Tp;


      using pointer = _Tp*;


      using const_pointer = const _Tp*;


      using void_pointer = void*;


      using const_void_pointer = const void*;


      using difference_type = std::ptrdiff_t;


      using size_type = std::size_t;


      using propagate_on_container_copy_assignment = false_type;


      using propagate_on_container_move_assignment = true_type;


      using propagate_on_container_swap = false_type;


      using is_always_equal = true_type;

      template<typename _Up>
 using rebind_alloc = allocator<_Up>;

      template<typename _Up>
 using rebind_traits = allocator_traits<allocator<_Up>>;
# 611 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      [[__nodiscard__,__gnu__::__always_inline__]]
      static pointer
      allocate(allocator_type& __a, size_type __n)
      { return __a.allocate(__n); }
# 626 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      [[__nodiscard__,__gnu__::__always_inline__]]
      static pointer
      allocate(allocator_type& __a, size_type __n,
        [[maybe_unused]] const_void_pointer __hint)
      {

 return __a.allocate(__n, __hint);



      }
# 646 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      [[__gnu__::__always_inline__]]
      static void
      deallocate(allocator_type& __a, pointer __p, size_type __n)
      { __a.deallocate(__p, __n); }
# 662 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      template<typename _Up, typename... _Args>
 [[__gnu__::__always_inline__]]
 static void
 construct(allocator_type& __a __attribute__((__unused__)),
    _Up* __p, _Args&&... __args)

 noexcept(noexcept(__a.construct(__p, std::forward<_Args>(__args)...)))



 {

   __a.construct(__p, std::forward<_Args>(__args)...);





 }
# 689 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      template<typename _Up>
 [[__gnu__::__always_inline__]]
 static void
 destroy(allocator_type& __a __attribute__((__unused__)), _Up* __p)
 noexcept(is_nothrow_destructible<_Up>::value)
 {

   __a.destroy(__p);



 }






      [[__gnu__::__always_inline__]]
      static size_type
      max_size(const allocator_type& __a __attribute__((__unused__))) noexcept
      {

 return __a.max_size();



      }






      [[__gnu__::__always_inline__]]
      static allocator_type
      select_on_container_copy_construction(const allocator_type& __rhs)
      { return __rhs; }
    };
# 736 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<>
    struct allocator_traits<allocator<void>>
    {

      using allocator_type = allocator<void>;


      using value_type = void;


      using pointer = void*;


      using const_pointer = const void*;


      using void_pointer = void*;


      using const_void_pointer = const void*;


      using difference_type = std::ptrdiff_t;


      using size_type = std::size_t;


      using propagate_on_container_copy_assignment = false_type;


      using propagate_on_container_move_assignment = true_type;


      using propagate_on_container_swap = false_type;


      using is_always_equal = true_type;

      template<typename _Up>
 using rebind_alloc = allocator<_Up>;

      template<typename _Up>
 using rebind_traits = allocator_traits<allocator<_Up>>;


      static void*
      allocate(allocator_type&, size_type, const void* = nullptr) = delete;


      static void
      deallocate(allocator_type&, void*, size_type) = delete;
# 800 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      template<typename _Up, typename... _Args>
 [[__gnu__::__always_inline__]]
 static void
 construct(allocator_type&, _Up* __p, _Args&&... __args)
 noexcept(__is_nothrow_new_constructible<_Up, _Args...>)
 { std::_Construct(__p, std::forward<_Args>(__args)...); }
# 814 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      template<typename _Up>
 [[__gnu__::__always_inline__]]
 static void
 destroy(allocator_type&, _Up* __p)
 noexcept(is_nothrow_destructible<_Up>::value)
 { std::_Destroy(__p); }


      static size_type
      max_size(const allocator_type&) = delete;






      [[__gnu__::__always_inline__]]
      static allocator_type
      select_on_container_copy_construction(const allocator_type& __rhs)
      { return __rhs; }
    };



#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<typename _Alloc>
    [[__gnu__::__always_inline__]]
    constexpr inline void
    __alloc_on_copy(_Alloc& __one, const _Alloc& __two)
    {
      using __traits = allocator_traits<_Alloc>;
      using __pocca =
 typename __traits::propagate_on_container_copy_assignment::type;
      if constexpr (__pocca::value)
 __one = __two;
    }

  template<typename _Alloc>
    [[__gnu__::__always_inline__]]
    constexpr _Alloc
    __alloc_on_copy(const _Alloc& __a)
    {
      typedef allocator_traits<_Alloc> __traits;
      return __traits::select_on_container_copy_construction(__a);
    }

  template<typename _Alloc>
    [[__gnu__::__always_inline__]]
    constexpr inline void
    __alloc_on_move(_Alloc& __one, _Alloc& __two)
    {
      using __traits = allocator_traits<_Alloc>;
      using __pocma
 = typename __traits::propagate_on_container_move_assignment::type;
      if constexpr (__pocma::value)
 __one = std::move(__two);
    }

  template<typename _Alloc>
    [[__gnu__::__always_inline__]]
    constexpr inline void
    __alloc_on_swap(_Alloc& __one, _Alloc& __two)
    {
      using __traits = allocator_traits<_Alloc>;
      using __pocs = typename __traits::propagate_on_container_swap::type;
      if constexpr (__pocs::value)
 {
   using std::swap;
   swap(__one, __two);
 }
    }
#pragma GCC diagnostic pop

  template<typename _Alloc, typename _Tp,
    typename _ValueT = __remove_cvref_t<typename _Alloc::value_type>,
    typename = void>
    struct __is_alloc_insertable_impl
    : false_type
    { };

  template<typename _Alloc, typename _Tp, typename _ValueT>
    struct __is_alloc_insertable_impl<_Alloc, _Tp, _ValueT,
      __void_t<decltype(allocator_traits<_Alloc>::construct(
     std::declval<_Alloc&>(), std::declval<_ValueT*>(),
     std::declval<_Tp>()))>>
    : true_type
    { };




  template<typename _Alloc>
    struct __is_copy_insertable
    : __is_alloc_insertable_impl<_Alloc,
     typename _Alloc::value_type const&>::type
    { };



  template<typename _Tp>
    struct __is_copy_insertable<allocator<_Tp>>
    : is_copy_constructible<_Tp>
    { };





  template<typename _Alloc>
    struct __is_move_insertable
    : __is_alloc_insertable_impl<_Alloc, typename _Alloc::value_type>::type
    { };



  template<typename _Tp>
    struct __is_move_insertable<allocator<_Tp>>
    : is_move_constructible<_Tp>
    { };



  template<typename _Alloc, typename = void>
    struct __is_allocator : false_type { };

  template<typename _Alloc>
    struct __is_allocator<_Alloc,
      __void_t<typename _Alloc::value_type,
        decltype(std::declval<_Alloc&>().allocate(size_t{}))>>
    : true_type { };

  template<typename _Alloc>
    using _RequireAllocator
      = typename enable_if<__is_allocator<_Alloc>::value, _Alloc>::type;

  template<typename _Alloc>
    using _RequireNotAllocator
      = typename enable_if<!__is_allocator<_Alloc>::value, _Alloc>::type;
# 970 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<typename _Alloc, bool = __is_empty(_Alloc)>
    struct __alloc_swap
    { static void _S_do_it(_Alloc&, _Alloc&) noexcept { } };

  template<typename _Alloc>
    struct __alloc_swap<_Alloc, false>
    {
      static void
      _S_do_it(_Alloc& __one, _Alloc& __two) noexcept
      {

 if (__one != __two)
   swap(__one, __two);
      }
    };


  template<typename _Tp, bool
    = __or_<is_copy_constructible<typename _Tp::value_type>,
            is_nothrow_move_constructible<typename _Tp::value_type>>::value>
    struct __shrink_to_fit_aux
    { static bool _S_do_it(_Tp&) noexcept { return false; } };

  template<typename _Tp>
    struct __shrink_to_fit_aux<_Tp, true>
    {
     
      static bool
      _S_do_it(_Tp& __c) noexcept
      {

 try
   {
     _Tp(__make_move_if_noexcept_iterator(__c.begin()),
  __make_move_if_noexcept_iterator(__c.end()),
  __c.get_allocator()).swap(__c);
     return true;
   }
 catch(...)
   { return false; }



      }
    };
# 1023 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<typename _ForwardIterator, typename _Allocator>
   
    void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last,
      _Allocator& __alloc)
    {
      for (; __first != __last; ++__first)



 allocator_traits<_Allocator>::destroy(__alloc,
           std::__addressof(*__first));

    }


  template<typename _ForwardIterator, typename _Tp>
    __attribute__((__always_inline__))
    inline void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last,
      allocator<_Tp>&)
    {
      std::_Destroy(__first, __last);
    }





}
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 2 3

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{






template<typename _Alloc, typename = typename _Alloc::value_type>
  struct __alloc_traits

  : std::allocator_traits<_Alloc>

  {
    typedef _Alloc allocator_type;

    typedef std::allocator_traits<_Alloc> _Base_type;
    typedef typename _Base_type::value_type value_type;
    typedef typename _Base_type::pointer pointer;
    typedef typename _Base_type::const_pointer const_pointer;
    typedef typename _Base_type::size_type size_type;
    typedef typename _Base_type::difference_type difference_type;

    typedef value_type& reference;
    typedef const value_type& const_reference;
    using _Base_type::allocate;
    using _Base_type::deallocate;
    using _Base_type::construct;
    using _Base_type::destroy;
    using _Base_type::max_size;

  private:
    template<typename _Ptr>
      using __is_custom_pointer
 = std::__and_<std::is_same<pointer, _Ptr>,
        std::__not_<std::is_pointer<_Ptr>>>;

  public:

    template<typename _Ptr, typename... _Args>
      [[__gnu__::__always_inline__]]
      static constexpr
      std::__enable_if_t<__is_custom_pointer<_Ptr>::value>
      construct(_Alloc& __a, _Ptr __p, _Args&&... __args)
      noexcept(noexcept(_Base_type::construct(__a, std::__to_address(__p),
           std::forward<_Args>(__args)...)))
      {
 _Base_type::construct(__a, std::__to_address(__p),
         std::forward<_Args>(__args)...);
      }


    template<typename _Ptr>
      [[__gnu__::__always_inline__]]
      static constexpr
      std::__enable_if_t<__is_custom_pointer<_Ptr>::value>
      destroy(_Alloc& __a, _Ptr __p)
      noexcept(noexcept(_Base_type::destroy(__a, std::__to_address(__p))))
      { _Base_type::destroy(__a, std::__to_address(__p)); }

    [[__gnu__::__always_inline__]]
    static constexpr _Alloc _S_select_on_copy(const _Alloc& __a)
    { return _Base_type::select_on_container_copy_construction(__a); }

    [[__gnu__::__always_inline__]]
    static constexpr void _S_on_swap(_Alloc& __a, _Alloc& __b)
    { std::__alloc_on_swap(__a, __b); }

    [[__gnu__::__always_inline__]]
    static constexpr bool _S_propagate_on_copy_assign()
    { return _Base_type::propagate_on_container_copy_assignment::value; }

    [[__gnu__::__always_inline__]]
    static constexpr bool _S_propagate_on_move_assign()
    { return _Base_type::propagate_on_container_move_assignment::value; }

    [[__gnu__::__always_inline__]]
    static constexpr bool _S_propagate_on_swap()
    { return _Base_type::propagate_on_container_swap::value; }

    [[__gnu__::__always_inline__]]
    static constexpr bool _S_always_equal()
    { return _Base_type::is_always_equal::value; }

    __attribute__((__always_inline__))
    static constexpr bool _S_nothrow_move()
    { return _S_propagate_on_move_assign() || _S_always_equal(); }

    template<typename _Tp>
      struct rebind
      { typedef typename _Base_type::template rebind_alloc<_Tp> other; };
# 182 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 3
  };


}
# 70 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{








  template<typename _ForwardIterator, typename _Alloc = void>
    struct _UninitDestroyGuard
    {
     
      explicit
      _UninitDestroyGuard(_ForwardIterator& __first, _Alloc& __a)
      : _M_first(__first), _M_cur(__builtin_addressof(__first)), _M_alloc(__a)
      { }

     
      ~_UninitDestroyGuard()
      {
 if (__builtin_expect(_M_cur != 0, 0))
   std::_Destroy(_M_first, *_M_cur, _M_alloc);
      }

     
      void release() { _M_cur = 0; }

    private:
      _ForwardIterator const _M_first;
      _ForwardIterator* _M_cur;
      _Alloc& _M_alloc;

      _UninitDestroyGuard(const _UninitDestroyGuard&);
    };

  template<typename _ForwardIterator>
    struct _UninitDestroyGuard<_ForwardIterator, void>
    {
     
      explicit
      _UninitDestroyGuard(_ForwardIterator& __first)
      : _M_first(__first), _M_cur(__builtin_addressof(__first))
      { }

     
      ~_UninitDestroyGuard()
      {
 if (__builtin_expect(_M_cur != 0, 0))
   std::_Destroy(_M_first, *_M_cur);
      }

     
      void release() { _M_cur = 0; }

      _ForwardIterator const _M_first;
      _ForwardIterator* _M_cur;

    private:
      _UninitDestroyGuard(const _UninitDestroyGuard&);
    };



  template<typename _InputIterator, typename _Sentinel,
    typename _ForwardIterator>
   
    _ForwardIterator
    __do_uninit_copy(_InputIterator __first, _Sentinel __last,
       _ForwardIterator __result)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__result);
      for (; __first != __last; ++__first, (void)++__result)
 std::_Construct(std::__addressof(*__result), *__first);
      __guard.release();
      return __result;
    }
# 217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
# 228 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _InputIterator, typename _ForwardIterator>
    inline _ForwardIterator
    uninitialized_copy(_InputIterator __first, _InputIterator __last,
         _ForwardIterator __result)
    {
# 255 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      using _Dest = decltype(std::__niter_base(__result));
      using _Src = decltype(std::__niter_base(__first));
      using _ValT = typename iterator_traits<_ForwardIterator>::value_type;

      if constexpr (!__is_trivially_constructible(_ValT, decltype(*__first)))
 return std::__do_uninit_copy(__first, __last, __result);
      else if constexpr (__memcpyable<_Dest, _Src>::__value)
 {
   ptrdiff_t __n = __last - __first;
   if (__n > 0) [[__likely__]]
     {
       using _ValT = typename remove_pointer<_Src>::type;
       __builtin_memcpy(std::__niter_base(__result),
          std::__niter_base(__first),
          __n * sizeof(_ValT));
       __result += __n;
     }
   return __result;
 }
# 296 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      else
 return std::__do_uninit_copy(__first, __last, __result);
# 311 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
    }
#pragma GCC diagnostic pop




  template<typename _ForwardIterator, typename _Tp>
    void
    __do_uninit_fill(_ForwardIterator __first, _ForwardIterator __last,
       const _Tp& __x)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__first);
      for (; __first != __last; ++__first)
 std::_Construct(std::__addressof(*__first), __x);
      __guard.release();
    }
# 383 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _ForwardIterator, typename _Tp>
    inline void
    uninitialized_fill(_ForwardIterator __first, _ForwardIterator __last,
         const _Tp& __x)
    {
# 397 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
      if constexpr (__is_byte<_ValueType>::__value)
 if constexpr (is_same<_ValueType, _Tp>::value
   || is_integral<_Tp>::value)
   {
     using _BasePtr = decltype(std::__niter_base(__first));
     if constexpr (is_pointer<_BasePtr>::value)
       {
  void* __dest = std::__niter_base(__first);
  ptrdiff_t __n = __last - __first;
  if (__n > 0) [[__likely__]]
    __builtin_memset(__dest, (unsigned char)__x, __n);
  return;
       }
# 426 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
   }
      std::__do_uninit_fill(__first, __last, __x);
#pragma GCC diagnostic pop






    }




  template<typename _ForwardIterator, typename _Size, typename _Tp>
   
    _ForwardIterator
    __do_uninit_fill_n(_ForwardIterator __first, _Size __n, const _Tp& __x)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__first);

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
      if constexpr (is_integral<_Size>::value)

 do { if (__builtin_expect(!bool(__n >= 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h", 451, __PRETTY_FUNCTION__, "__n >= 0"); } while (false);
      else if constexpr (is_floating_point<_Size>::value)

 do { if (__builtin_expect(!bool(__n >= 0 && static_cast<size_t>(__n) == __n), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h", 454, __PRETTY_FUNCTION__, "__n >= 0 && static_cast<size_t>(__n) == __n"); } while (false);
#pragma GCC diagnostic pop

      for (; __n--; ++__first)
 std::_Construct(std::__addressof(*__first), __x);
      __guard.release();
      return __first;
    }
# 498 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
# 511 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _ForwardIterator, typename _Size, typename _Tp>
    inline _ForwardIterator
    uninitialized_fill_n(_ForwardIterator __first, _Size __n, const _Tp& __x)
    {






      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;


      if constexpr (__is_byte<_ValueType>::__value)
 if constexpr (is_integral<_Tp>::value)
   if constexpr (is_integral<_Size>::value)
     {
       using _BasePtr = decltype(std::__niter_base(__first));
       if constexpr (is_pointer<_BasePtr>::value)
  {
    void* __dest = std::__niter_base(__first);
    if (__n > 0) [[__likely__]]
      {
        __builtin_memset(__dest, (unsigned char)__x, __n);
        __first += __n;
      }
    return __first;
  }
# 552 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
     }
      return std::__do_uninit_fill_n(__first, __n, __x);
# 562 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
    }
#pragma GCC diagnostic pop
# 573 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _InputIterator, typename _Sentinel,
    typename _ForwardIterator, typename _Allocator>
   
    _ForwardIterator
    __uninitialized_copy_a(_InputIterator __first, _Sentinel __last,
      _ForwardIterator __result, _Allocator& __alloc)
    {
      _UninitDestroyGuard<_ForwardIterator, _Allocator>
 __guard(__result, __alloc);

      typedef __gnu_cxx::__alloc_traits<_Allocator> __traits;
      for (; __first != __last; ++__first, (void)++__result)
 __traits::construct(__alloc, std::__addressof(*__result), *__first);
      __guard.release();
      return __result;
    }


  template<typename _InputIterator, typename _Sentinel,
    typename _ForwardIterator, typename _Tp>
   
    inline _ForwardIterator
    __uninitialized_copy_a(_InputIterator __first, _Sentinel __last,
      _ForwardIterator __result, allocator<_Tp>&)
    {
# 619 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      return std::uninitialized_copy(__first, __last, __result);

    }


  template<typename _InputIterator, typename _ForwardIterator,
    typename _Allocator>
   
    inline _ForwardIterator
    __uninitialized_move_a(_InputIterator __first, _InputIterator __last,
      _ForwardIterator __result, _Allocator& __alloc)
    {
      return std::__uninitialized_copy_a(std::make_move_iterator(__first),
      std::make_move_iterator(__last),
      __result, __alloc);
    }

  template<typename _InputIterator, typename _ForwardIterator,
    typename _Allocator>
   
    inline _ForwardIterator
    __uninitialized_move_if_noexcept_a(_InputIterator __first,
           _InputIterator __last,
           _ForwardIterator __result,
           _Allocator& __alloc)
    {
      return std::__uninitialized_copy_a
 (std::__make_move_if_noexcept_iterator(__first),
  std::__make_move_if_noexcept_iterator(__last), __result, __alloc);
    }

  template<typename _ForwardIterator, typename _Tp, typename _Allocator>
   
    void
    __uninitialized_fill_a(_ForwardIterator __first, _ForwardIterator __last,
      const _Tp& __x, _Allocator& __alloc)
    {
      _UninitDestroyGuard<_ForwardIterator, _Allocator>
 __guard(__first, __alloc);

      typedef __gnu_cxx::__alloc_traits<_Allocator> __traits;
      for (; __first != __last; ++__first)
 __traits::construct(__alloc, std::__addressof(*__first), __x);

      __guard.release();
    }


  template<typename _ForwardIterator, typename _Tp, typename _Tp2>
   
    inline void
    __uninitialized_fill_a(_ForwardIterator __first, _ForwardIterator __last,
      const _Tp& __x, allocator<_Tp2>&)
    {




      std::uninitialized_fill(__first, __last, __x);
    }


  template<typename _ForwardIterator, typename _Size, typename _Tp,
    typename _Allocator>
    
    _ForwardIterator
    __uninitialized_fill_n_a(_ForwardIterator __first, _Size __n,
        const _Tp& __x, _Allocator& __alloc)
    {
      _UninitDestroyGuard<_ForwardIterator, _Allocator>
 __guard(__first, __alloc);
      typedef __gnu_cxx::__alloc_traits<_Allocator> __traits;
      for (; __n > 0; --__n, (void) ++__first)
 __traits::construct(__alloc, std::__addressof(*__first), __x);
      __guard.release();
      return __first;
    }


  template<typename _ForwardIterator, typename _Size, typename _Tp,
    typename _Tp2>
   
    inline _ForwardIterator
    __uninitialized_fill_n_a(_ForwardIterator __first, _Size __n,
        const _Tp& __x, allocator<_Tp2>&)
    {




      return std::uninitialized_fill_n(__first, __n, __x);
    }
# 722 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _ForwardIterator, typename _Allocator>
    inline _ForwardIterator
    __uninitialized_copy_move(_InputIterator1 __first1,
         _InputIterator1 __last1,
         _InputIterator2 __first2,
         _InputIterator2 __last2,
         _ForwardIterator __result,
         _Allocator& __alloc)
    {
      _ForwardIterator __mid = std::__uninitialized_copy_a(__first1, __last1,
          __result, __alloc);
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__result,
        __alloc);
      __result = __mid;
      __result = std::__uninitialized_move_a(__first2, __last2, __mid, __alloc);
      __guard.release();
      return __result;
    }





  template<typename _InputIterator1, typename _InputIterator2,
    typename _ForwardIterator, typename _Allocator>
    inline _ForwardIterator
    __uninitialized_move_copy(_InputIterator1 __first1,
         _InputIterator1 __last1,
         _InputIterator2 __first2,
         _InputIterator2 __last2,
         _ForwardIterator __result,
         _Allocator& __alloc)
    {
      _ForwardIterator __mid = std::__uninitialized_move_a(__first1, __last1,
          __result, __alloc);
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__result,
        __alloc);
      __result = __mid;
      __result = std::__uninitialized_copy_a(__first2, __last2, __mid, __alloc);
      __guard.release();
      return __result;
    }




  template<typename _ForwardIterator, typename _Tp, typename _InputIterator,
    typename _Allocator>
    inline _ForwardIterator
    __uninitialized_fill_move(_ForwardIterator __result, _ForwardIterator __mid,
         const _Tp& __x, _InputIterator __first,
         _InputIterator __last, _Allocator& __alloc)
    {
      std::__uninitialized_fill_a(__result, __mid, __x, __alloc);
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__result,
        __alloc);
      __result = __mid;
      __result = std::__uninitialized_move_a(__first, __last, __mid, __alloc);
      __guard.release();
      return __result;
    }




  template<typename _InputIterator, typename _ForwardIterator, typename _Tp,
    typename _Allocator>
    inline void
    __uninitialized_move_fill(_InputIterator __first1, _InputIterator __last1,
         _ForwardIterator __first2,
         _ForwardIterator __last2, const _Tp& __x,
         _Allocator& __alloc)
    {
      _ForwardIterator __mid2 = std::__uninitialized_move_a(__first1, __last1,
           __first2,
           __alloc);
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__first2,
        __alloc);
      __first2 = __mid2;
      std::__uninitialized_fill_a(__mid2, __last2, __x, __alloc);
      __guard.release();
    }
# 814 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<bool _TrivialValueType>
    struct __uninitialized_default_1
    {
      template<typename _ForwardIterator>
        static void
        __uninit_default(_ForwardIterator __first, _ForwardIterator __last)
        {
   _UninitDestroyGuard<_ForwardIterator> __guard(__first);
   for (; __first != __last; ++__first)
     std::_Construct(std::__addressof(*__first));
   __guard.release();
 }
    };

  template<>
    struct __uninitialized_default_1<true>
    {
      template<typename _ForwardIterator>
        static void
        __uninit_default(_ForwardIterator __first, _ForwardIterator __last)
        {
   if (__first == __last)
     return;

   typename iterator_traits<_ForwardIterator>::value_type* __val
     = std::__addressof(*__first);
   std::_Construct(__val);
   if (++__first != __last)
     std::fill(__first, __last, *__val);
 }
    };

  template<bool _TrivialValueType>
    struct __uninitialized_default_n_1
    {
      template<typename _ForwardIterator, typename _Size>

        static _ForwardIterator
        __uninit_default_n(_ForwardIterator __first, _Size __n)
        {
   _UninitDestroyGuard<_ForwardIterator> __guard(__first);
   for (; __n > 0; --__n, (void) ++__first)
     std::_Construct(std::__addressof(*__first));
   __guard.release();
   return __first;
 }
    };

  template<>
    struct __uninitialized_default_n_1<true>
    {
      template<typename _ForwardIterator, typename _Size>

        static _ForwardIterator
        __uninit_default_n(_ForwardIterator __first, _Size __n)
        {
   if (__n > 0)
     {
       typename iterator_traits<_ForwardIterator>::value_type* __val
  = std::__addressof(*__first);
       std::_Construct(__val);
       ++__first;
       __first = std::fill_n(__first, __n - 1, *__val);
     }
   return __first;
 }
    };



  template<typename _ForwardIterator>
    inline void
    __uninitialized_default(_ForwardIterator __first,
       _ForwardIterator __last)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;

      const bool __assignable = is_copy_assignable<_ValueType>::value;

      std::__uninitialized_default_1<__is_trivial(_ValueType)
         && __assignable>::
 __uninit_default(__first, __last);
    }



  template<typename _ForwardIterator, typename _Size>
   
    inline _ForwardIterator
    __uninitialized_default_n(_ForwardIterator __first, _Size __n)
    {






      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;

      constexpr bool __can_fill
 = __and_<is_integral<_Size>, is_copy_assignable<_ValueType>>::value;

      return __uninitialized_default_n_1<__is_trivial(_ValueType)
      && __can_fill>::
 __uninit_default_n(__first, __n);
    }





  template<typename _ForwardIterator, typename _Allocator>
    void
    __uninitialized_default_a(_ForwardIterator __first,
         _ForwardIterator __last,
         _Allocator& __alloc)
    {
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__first,
        __alloc);
      typedef __gnu_cxx::__alloc_traits<_Allocator> __traits;
      for (; __first != __last; ++__first)
 __traits::construct(__alloc, std::__addressof(*__first));
      __guard.release();
    }


  template<typename _ForwardIterator, typename _Tp>
    inline void
    __uninitialized_default_a(_ForwardIterator __first,
         _ForwardIterator __last,
         allocator<_Tp>&)
    { std::__uninitialized_default(__first, __last); }





  template<typename _ForwardIterator, typename _Size, typename _Allocator>
    _ForwardIterator
    __uninitialized_default_n_a(_ForwardIterator __first, _Size __n,
    _Allocator& __alloc)
    {
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__first,
        __alloc);
      typedef __gnu_cxx::__alloc_traits<_Allocator> __traits;
      for (; __n > 0; --__n, (void) ++__first)
 __traits::construct(__alloc, std::__addressof(*__first));
      __guard.release();
      return __first;
    }




  template<typename _ForwardIterator, typename _Size, typename _Tp>
   
    inline _ForwardIterator
    __uninitialized_default_n_a(_ForwardIterator __first, _Size __n,
    allocator<_Tp>&)
    { return std::__uninitialized_default_n(__first, __n); }


  template<bool _TrivialValueType>
    struct __uninitialized_default_novalue_1
    {
      template<typename _ForwardIterator>
 static void
 __uninit_default_novalue(_ForwardIterator __first,
     _ForwardIterator __last)
 {
   _UninitDestroyGuard<_ForwardIterator> __guard(__first);
   for (; __first != __last; ++__first)
     std::_Construct_novalue(std::__addressof(*__first));
   __guard.release();
 }
    };

  template<>
    struct __uninitialized_default_novalue_1<true>
    {
      template<typename _ForwardIterator>
        static void
        __uninit_default_novalue(_ForwardIterator, _ForwardIterator)
 {
 }
    };

  template<bool _TrivialValueType>
    struct __uninitialized_default_novalue_n_1
    {
      template<typename _ForwardIterator, typename _Size>
 static _ForwardIterator
 __uninit_default_novalue_n(_ForwardIterator __first, _Size __n)
 {
   _UninitDestroyGuard<_ForwardIterator> __guard(__first);
   for (; __n > 0; --__n, (void) ++__first)
     std::_Construct_novalue(std::__addressof(*__first));
   __guard.release();
   return __first;
 }
    };

  template<>
    struct __uninitialized_default_novalue_n_1<true>
    {
      template<typename _ForwardIterator, typename _Size>
 static _ForwardIterator
 __uninit_default_novalue_n(_ForwardIterator __first, _Size __n)
 { return std::next(__first, __n); }
    };



  template<typename _ForwardIterator>
    inline void
    __uninitialized_default_novalue(_ForwardIterator __first,
        _ForwardIterator __last)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;

      std::__uninitialized_default_novalue_1<
 is_trivially_default_constructible<_ValueType>::value>::
 __uninit_default_novalue(__first, __last);
    }



  template<typename _ForwardIterator, typename _Size>
    inline _ForwardIterator
    __uninitialized_default_novalue_n(_ForwardIterator __first, _Size __n)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;

      return __uninitialized_default_novalue_n_1<
 is_trivially_default_constructible<_ValueType>::value>::
 __uninit_default_novalue_n(__first, __n);
    }

  template<typename _InputIterator, typename _Size,
    typename _ForwardIterator>
    _ForwardIterator
    __uninitialized_copy_n(_InputIterator __first, _Size __n,
      _ForwardIterator __result, input_iterator_tag)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__result);
      for (; __n > 0; --__n, (void) ++__first, ++__result)
 std::_Construct(std::__addressof(*__result), *__first);
      __guard.release();
      return __result;
    }

  template<typename _RandomAccessIterator, typename _Size,
    typename _ForwardIterator>
    inline _ForwardIterator
    __uninitialized_copy_n(_RandomAccessIterator __first, _Size __n,
      _ForwardIterator __result,
      random_access_iterator_tag)
    { return std::uninitialized_copy(__first, __first + __n, __result); }

  template<typename _InputIterator, typename _Size,
    typename _ForwardIterator>
    pair<_InputIterator, _ForwardIterator>
    __uninitialized_copy_n_pair(_InputIterator __first, _Size __n,
    _ForwardIterator __result, input_iterator_tag)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__result);
      for (; __n > 0; --__n, (void) ++__first, ++__result)
 std::_Construct(std::__addressof(*__result), *__first);
      __guard.release();
      return {__first, __result};
    }

  template<typename _RandomAccessIterator, typename _Size,
    typename _ForwardIterator>
    inline pair<_RandomAccessIterator, _ForwardIterator>
    __uninitialized_copy_n_pair(_RandomAccessIterator __first, _Size __n,
      _ForwardIterator __result,
      random_access_iterator_tag)
    {
      auto __second_res = uninitialized_copy(__first, __first + __n, __result);
      auto __first_res = std::next(__first, __n);
      return {__first_res, __second_res};
    }
# 1114 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _InputIterator, typename _Size, typename _ForwardIterator>
    inline _ForwardIterator
    uninitialized_copy_n(_InputIterator __first, _Size __n,
    _ForwardIterator __result)
    { return std::__uninitialized_copy_n(__first, __n, __result,
      std::__iterator_category(__first)); }


  template<typename _InputIterator, typename _Size, typename _ForwardIterator>
    inline pair<_InputIterator, _ForwardIterator>
    __uninitialized_copy_n_pair(_InputIterator __first, _Size __n,
         _ForwardIterator __result)
    {
      return
 std::__uninitialized_copy_n_pair(__first, __n, __result,
      std::__iterator_category(__first));
    }
# 1232 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _Tp, typename _Up, typename _Allocator>
   
    inline void
    __relocate_object_a(_Tp* __restrict __dest, _Up* __restrict __orig,
   _Allocator& __alloc)
    noexcept(noexcept(std::allocator_traits<_Allocator>::construct(__alloc,
    __dest, std::move(*__orig)))
      && noexcept(std::allocator_traits<_Allocator>::destroy(
       __alloc, std::__addressof(*__orig))))
    {
      typedef std::allocator_traits<_Allocator> __traits;
      __traits::construct(__alloc, __dest, std::move(*__orig));
      __traits::destroy(__alloc, std::__addressof(*__orig));
    }



  template<typename _Tp, typename = void>
    struct __is_bitwise_relocatable
    : __bool_constant<__is_trivial(_Tp)>
    { };

  template <typename _InputIterator, typename _ForwardIterator,
     typename _Allocator>
   
    inline _ForwardIterator
    __relocate_a_1(_InputIterator __first, _InputIterator __last,
     _ForwardIterator __result, _Allocator& __alloc)
    noexcept(noexcept(std::__relocate_object_a(std::addressof(*__result),
            std::addressof(*__first),
            __alloc)))
    {
      typedef typename iterator_traits<_InputIterator>::value_type
 _ValueType;
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType2;
      static_assert(std::is_same<_ValueType, _ValueType2>::value,
   "relocation is only possible for values of the same type");
      _ForwardIterator __cur = __result;
      for (; __first != __last; ++__first, (void)++__cur)
 std::__relocate_object_a(std::__addressof(*__cur),
     std::__addressof(*__first), __alloc);
      return __cur;
    }


  template <typename _Tp, typename _Up>
   
    inline __enable_if_t<std::__is_bitwise_relocatable<_Tp>::value, _Tp*>
    __relocate_a_1(_Tp* __first, _Tp* __last,
     _Tp* __result,
     [[__maybe_unused__]] allocator<_Up>& __alloc) noexcept
    {
      ptrdiff_t __count = __last - __first;
      if (__count > 0)
 {
# 1298 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
   __builtin_memcpy(__result, __first, __count * sizeof(_Tp));
 }
      return __result + __count;
    }


  template <typename _InputIterator, typename _ForwardIterator,
     typename _Allocator>
   
    inline _ForwardIterator
    __relocate_a(_InputIterator __first, _InputIterator __last,
   _ForwardIterator __result, _Allocator& __alloc)
    noexcept(noexcept(__relocate_a_1(std::__niter_base(__first),
         std::__niter_base(__last),
         std::__niter_base(__result), __alloc)))
    {
      return std::__relocate_a_1(std::__niter_base(__first),
     std::__niter_base(__last),
     std::__niter_base(__result), __alloc);
    }







}
# 72 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_raw_storage_iter.h" 1 3
# 59 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_raw_storage_iter.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{



 
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_raw_storage_iter.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"





  template <class _OutputIterator, class _Tp>
    class raw_storage_iterator
    : public iterator<output_iterator_tag, void, void, void, void>
    {
    protected:
      _OutputIterator _M_iter;

    public:
      explicit
      raw_storage_iterator(_OutputIterator __x)
      : _M_iter(__x) {}

      raw_storage_iterator&
      operator*() { return *this; }

      raw_storage_iterator&
      operator=(const _Tp& __element)
      {
 std::_Construct(std::__addressof(*_M_iter), __element);
 return *this;
      }




      raw_storage_iterator&
      operator=(_Tp&& __element)
      {
 std::_Construct(std::__addressof(*_M_iter), std::move(__element));
 return *this;
      }


      raw_storage_iterator&
      operator++()
      {
 ++_M_iter;
 return *this;
      }

      raw_storage_iterator
      operator++(int)
      {
 raw_storage_iterator __tmp = *this;
 ++_M_iter;
 return __tmp;
      }



      _OutputIterator base() const { return _M_iter; }
    };
#pragma GCC diagnostic pop


}
# 73 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3



# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/align.h" 1 3
# 35 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/align.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/align.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 59 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/align.h" 3
inline void*
align(size_t __align, size_t __size, void*& __ptr, size_t& __space) noexcept
{
  if (__space < __size)
    return nullptr;
  const auto __intptr = reinterpret_cast<long unsigned int>(__ptr);
  const auto __aligned = (__intptr - 1u + __align) & -__align;
  const auto __diff = __aligned - __intptr;
  if (__diff > (__space - __size))
    return nullptr;
  else
    {
      __space -= __diff;
      return __ptr = reinterpret_cast<void*>(__aligned);
    }
}
# 105 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/align.h" 3

}
# 77 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uses_allocator.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uses_allocator.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{




  struct __erased_type { };




  template<typename _Alloc, typename _Tp>
    using __is_erased_or_convertible
      = __or_<is_convertible<_Alloc, _Tp>, is_same<_Tp, __erased_type>>;


  struct allocator_arg_t { explicit allocator_arg_t() = default; };

  constexpr allocator_arg_t allocator_arg =
    allocator_arg_t();

  template<typename _Tp, typename _Alloc, typename = __void_t<>>
    struct __uses_allocator_helper
    : false_type { };

  template<typename _Tp, typename _Alloc>
    struct __uses_allocator_helper<_Tp, _Alloc,
       __void_t<typename _Tp::allocator_type>>
    : __is_erased_or_convertible<_Alloc, typename _Tp::allocator_type>::type
    { };


  template<typename _Tp, typename _Alloc>
    struct uses_allocator
    : __uses_allocator_helper<_Tp, _Alloc>::type
    { };

  struct __uses_alloc_base { };

  struct __uses_alloc0 : __uses_alloc_base
  {
    struct _Sink { void operator=(const void*) { } } _M_a;
  };

  template<typename _Alloc>
    struct __uses_alloc1 : __uses_alloc_base { const _Alloc* _M_a; };

  template<typename _Alloc>
    struct __uses_alloc2 : __uses_alloc_base { const _Alloc* _M_a; };

  template<bool, typename _Tp, typename _Alloc, typename... _Args>
    struct __uses_alloc;

  template<typename _Tp, typename _Alloc, typename... _Args>
    struct __uses_alloc<true, _Tp, _Alloc, _Args...>
    : __conditional_t<
        is_constructible<_Tp, allocator_arg_t, const _Alloc&, _Args...>::value,
        __uses_alloc1<_Alloc>,
        __uses_alloc2<_Alloc>>
    {


      static_assert(__or_<
   is_constructible<_Tp, allocator_arg_t, const _Alloc&, _Args...>,
   is_constructible<_Tp, _Args..., const _Alloc&>>::value,
   "construction with an allocator must be possible"
   " if uses_allocator is true");
    };

  template<typename _Tp, typename _Alloc, typename... _Args>
    struct __uses_alloc<false, _Tp, _Alloc, _Args...>
    : __uses_alloc0 { };

  template<typename _Tp, typename _Alloc, typename... _Args>
    using __uses_alloc_t =
      __uses_alloc<uses_allocator<_Tp, _Alloc>::value, _Tp, _Alloc, _Args...>;

  template<typename _Tp, typename _Alloc, typename... _Args>
   
    inline __uses_alloc_t<_Tp, _Alloc, _Args...>
    __use_alloc(const _Alloc& __a)
    {
      __uses_alloc_t<_Tp, _Alloc, _Args...> __ret;
      __ret._M_a = std::__addressof(__a);
      return __ret;
    }

  template<typename _Tp, typename _Alloc, typename... _Args>
    void
    __use_alloc(const _Alloc&&) = delete;
# 142 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uses_allocator.h" 3
  template<template<typename...> class _Predicate,
    typename _Tp, typename _Alloc, typename... _Args>
    struct __is_uses_allocator_predicate
    : __conditional_t<uses_allocator<_Tp, _Alloc>::value,
      __or_<_Predicate<_Tp, allocator_arg_t, _Alloc, _Args...>,
     _Predicate<_Tp, _Args..., _Alloc>>,
      _Predicate<_Tp, _Args...>> { };

  template<typename _Tp, typename _Alloc, typename... _Args>
    struct __is_uses_allocator_constructible
    : __is_uses_allocator_predicate<is_constructible, _Tp, _Alloc, _Args...>
    { };


  template<typename _Tp, typename _Alloc, typename... _Args>
    constexpr bool __is_uses_allocator_constructible_v =
      __is_uses_allocator_constructible<_Tp, _Alloc, _Args...>::value;


  template<typename _Tp, typename _Alloc, typename... _Args>
    struct __is_nothrow_uses_allocator_constructible
    : __is_uses_allocator_predicate<is_nothrow_constructible,
        _Tp, _Alloc, _Args...>
    { };



  template<typename _Tp, typename _Alloc, typename... _Args>
    constexpr bool
    __is_nothrow_uses_allocator_constructible_v =
      __is_nothrow_uses_allocator_constructible<_Tp, _Alloc, _Args...>::value;


  template<typename _Tp, typename... _Args>
    void __uses_allocator_construct_impl(__uses_alloc0, _Tp* __ptr,
      _Args&&... __args)
    { ::new ((void*)__ptr) _Tp(std::forward<_Args>(__args)...); }

  template<typename _Tp, typename _Alloc, typename... _Args>
    void __uses_allocator_construct_impl(__uses_alloc1<_Alloc> __a, _Tp* __ptr,
      _Args&&... __args)
    {
      ::new ((void*)__ptr) _Tp(allocator_arg, *__a._M_a,
          std::forward<_Args>(__args)...);
    }

  template<typename _Tp, typename _Alloc, typename... _Args>
    void __uses_allocator_construct_impl(__uses_alloc2<_Alloc> __a, _Tp* __ptr,
      _Args&&... __args)
    { ::new ((void*)__ptr) _Tp(std::forward<_Args>(__args)..., *__a._M_a); }

  template<typename _Tp, typename _Alloc, typename... _Args>
    void __uses_allocator_construct(const _Alloc& __a, _Tp* __ptr,
        _Args&&... __args)
    {
      std::__uses_allocator_construct_impl(
   std::__use_alloc<_Tp, _Alloc, _Args...>(__a), __ptr,
   std::forward<_Args>(__args)...);
    }



}
# 78 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 1 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 1 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/invoke.h" 1 3
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/invoke.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 55 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/invoke.h" 3
  template<typename _Tp, typename _Up = typename __inv_unwrap<_Tp>::type>
    constexpr _Up&&
    __invfwd(typename remove_reference<_Tp>::type& __t) noexcept
    { return static_cast<_Up&&>(__t); }

  template<typename _Res, typename _Fn, typename... _Args>
    constexpr _Res
    __invoke_impl(__invoke_other, _Fn&& __f, _Args&&... __args)
    { return std::forward<_Fn>(__f)(std::forward<_Args>(__args)...); }

  template<typename _Res, typename _MemFun, typename _Tp, typename... _Args>
    constexpr _Res
    __invoke_impl(__invoke_memfun_ref, _MemFun&& __f, _Tp&& __t,
    _Args&&... __args)
    { return (__invfwd<_Tp>(__t).*__f)(std::forward<_Args>(__args)...); }

  template<typename _Res, typename _MemFun, typename _Tp, typename... _Args>
    constexpr _Res
    __invoke_impl(__invoke_memfun_deref, _MemFun&& __f, _Tp&& __t,
    _Args&&... __args)
    {
      return ((*std::forward<_Tp>(__t)).*__f)(std::forward<_Args>(__args)...);
    }

  template<typename _Res, typename _MemPtr, typename _Tp>
    constexpr _Res
    __invoke_impl(__invoke_memobj_ref, _MemPtr&& __f, _Tp&& __t)
    { return __invfwd<_Tp>(__t).*__f; }

  template<typename _Res, typename _MemPtr, typename _Tp>
    constexpr _Res
    __invoke_impl(__invoke_memobj_deref, _MemPtr&& __f, _Tp&& __t)
    { return (*std::forward<_Tp>(__t)).*__f; }


  template<typename _Callable, typename... _Args>
    constexpr typename __invoke_result<_Callable, _Args...>::type
    __invoke(_Callable&& __fn, _Args&&... __args)
    noexcept(__is_nothrow_invocable<_Callable, _Args...>::value)
    {
      using __result = __invoke_result<_Callable, _Args...>;
      using __type = typename __result::type;
      using __tag = typename __result::__invoke_type;
      return std::__invoke_impl<__type>(__tag{}, std::forward<_Callable>(__fn),
     std::forward<_Args>(__args)...);
    }
# 129 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/invoke.h" 3
  template<typename _Res, typename _Callable, typename... _Args>
    constexpr __enable_if_t<!is_void<_Res>::value, _Res>
    __invoke_r(_Callable&& __fn, _Args&&... __args)
    {
      using __result = __invoke_result<_Callable, _Args...>;
      using __type = typename __result::type;

      static_assert(!__reference_converts_from_temporary(_Res, __type),
      "INVOKE<R> must not create a dangling reference");

      using __tag = typename __result::__invoke_type;
      return std::__invoke_impl<__type>(__tag{}, std::forward<_Callable>(__fn),
     std::forward<_Args>(__args)...);
    }


  template<typename _Res, typename _Callable, typename... _Args>
    constexpr __enable_if_t<is_void<_Res>::value, _Res>
    __invoke_r(_Callable&& __fn, _Args&&... __args)
    {
      using __result = __invoke_result<_Callable, _Args...>;
      using __type = typename __result::type;
      using __tag = typename __result::__invoke_type;
      std::__invoke_impl<__type>(__tag{}, std::forward<_Callable>(__fn),
     std::forward<_Args>(__args)...);
    }



}
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 2 3
# 57 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 58 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{







  template<typename... _Elements>
    class tuple;


  template<typename _Tp>
    struct __is_empty_non_tuple : is_empty<_Tp> { };


  template<typename _El0, typename... _El>
    struct __is_empty_non_tuple<tuple<_El0, _El...>> : false_type { };


  template<typename _Tp>
    using __empty_not_final
    = __conditional_t<__is_final(_Tp), false_type,
        __is_empty_non_tuple<_Tp>>;

  template<size_t _Idx, typename _Head,
    bool = __empty_not_final<_Head>::value>
    struct _Head_base;


  template<size_t _Idx, typename _Head>
    struct _Head_base<_Idx, _Head, true>
    {
      constexpr _Head_base()
      : _M_head_impl() { }

      constexpr _Head_base(const _Head& __h)
      : _M_head_impl(__h) { }

      constexpr _Head_base(const _Head_base&) = default;
      constexpr _Head_base(_Head_base&&) = default;

      template<typename _UHead>
 constexpr _Head_base(_UHead&& __h)
 : _M_head_impl(std::forward<_UHead>(__h)) { }

     
      _Head_base(allocator_arg_t, __uses_alloc0)
      : _M_head_impl() { }

      template<typename _Alloc>

 _Head_base(allocator_arg_t, __uses_alloc1<_Alloc> __a)
 : _M_head_impl(allocator_arg, *__a._M_a) { }

      template<typename _Alloc>

 _Head_base(allocator_arg_t, __uses_alloc2<_Alloc> __a)
 : _M_head_impl(*__a._M_a) { }

      template<typename _UHead>

 _Head_base(__uses_alloc0, _UHead&& __uhead)
 : _M_head_impl(std::forward<_UHead>(__uhead)) { }

      template<typename _Alloc, typename _UHead>

 _Head_base(__uses_alloc1<_Alloc> __a, _UHead&& __uhead)
 : _M_head_impl(allocator_arg, *__a._M_a, std::forward<_UHead>(__uhead))
 { }

      template<typename _Alloc, typename _UHead>

 _Head_base(__uses_alloc2<_Alloc> __a, _UHead&& __uhead)
 : _M_head_impl(std::forward<_UHead>(__uhead), *__a._M_a) { }

      static constexpr _Head&
      _M_head(_Head_base& __b) noexcept { return __b._M_head_impl; }

      static constexpr const _Head&
      _M_head(const _Head_base& __b) noexcept { return __b._M_head_impl; }

      [[__no_unique_address__]] _Head _M_head_impl;
    };
# 199 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<size_t _Idx, typename _Head>
    struct _Head_base<_Idx, _Head, false>
    {
      constexpr _Head_base()
      : _M_head_impl() { }

      constexpr _Head_base(const _Head& __h)
      : _M_head_impl(__h) { }

      constexpr _Head_base(const _Head_base&) = default;
      constexpr _Head_base(_Head_base&&) = default;

      template<typename _UHead>
        constexpr _Head_base(_UHead&& __h)
 : _M_head_impl(std::forward<_UHead>(__h)) { }

     
      _Head_base(allocator_arg_t, __uses_alloc0)
      : _M_head_impl() { }

      template<typename _Alloc>

 _Head_base(allocator_arg_t, __uses_alloc1<_Alloc> __a)
 : _M_head_impl(allocator_arg, *__a._M_a) { }

      template<typename _Alloc>

 _Head_base(allocator_arg_t, __uses_alloc2<_Alloc> __a)
 : _M_head_impl(*__a._M_a) { }

      template<typename _UHead>

 _Head_base(__uses_alloc0, _UHead&& __uhead)
 : _M_head_impl(std::forward<_UHead>(__uhead)) { }

      template<typename _Alloc, typename _UHead>

 _Head_base(__uses_alloc1<_Alloc> __a, _UHead&& __uhead)
 : _M_head_impl(allocator_arg, *__a._M_a, std::forward<_UHead>(__uhead))
 { }

      template<typename _Alloc, typename _UHead>

 _Head_base(__uses_alloc2<_Alloc> __a, _UHead&& __uhead)
 : _M_head_impl(std::forward<_UHead>(__uhead), *__a._M_a) { }

      static constexpr _Head&
      _M_head(_Head_base& __b) noexcept { return __b._M_head_impl; }

      static constexpr const _Head&
      _M_head(const _Head_base& __b) noexcept { return __b._M_head_impl; }

      _Head _M_head_impl;
    };
# 272 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<size_t _Idx, typename... _Elements>
    struct _Tuple_impl;






  template<size_t _Idx, typename _Head, typename... _Tail>
    struct _Tuple_impl<_Idx, _Head, _Tail...>
    : public _Tuple_impl<_Idx + 1, _Tail...>,
      private _Head_base<_Idx, _Head>
    {
      template<size_t, typename...> friend struct _Tuple_impl;

      typedef _Tuple_impl<_Idx + 1, _Tail...> _Inherited;
      typedef _Head_base<_Idx, _Head> _Base;

      static constexpr _Head&
      _M_head(_Tuple_impl& __t) noexcept { return _Base::_M_head(__t); }

      static constexpr const _Head&
      _M_head(const _Tuple_impl& __t) noexcept { return _Base::_M_head(__t); }

      static constexpr _Inherited&
      _M_tail(_Tuple_impl& __t) noexcept { return __t; }

      static constexpr const _Inherited&
      _M_tail(const _Tuple_impl& __t) noexcept { return __t; }

      constexpr _Tuple_impl()
      : _Inherited(), _Base() { }

      explicit constexpr
      _Tuple_impl(const _Head& __head, const _Tail&... __tail)
      : _Inherited(__tail...), _Base(__head)
      { }

      template<typename _UHead, typename... _UTail,
        typename = __enable_if_t<sizeof...(_Tail) == sizeof...(_UTail)>>
 explicit constexpr
 _Tuple_impl(_UHead&& __head, _UTail&&... __tail)
 : _Inherited(std::forward<_UTail>(__tail)...),
   _Base(std::forward<_UHead>(__head))
 { }

      constexpr _Tuple_impl(const _Tuple_impl&) = default;



      _Tuple_impl& operator=(const _Tuple_impl&) = delete;

      _Tuple_impl(_Tuple_impl&&) = default;

      template<typename... _UElements>
 constexpr
 _Tuple_impl(const _Tuple_impl<_Idx, _UElements...>& __in)
 : _Inherited(_Tuple_impl<_Idx, _UElements...>::_M_tail(__in)),
   _Base(_Tuple_impl<_Idx, _UElements...>::_M_head(__in))
 { }

      template<typename _UHead, typename... _UTails>
 constexpr
 _Tuple_impl(_Tuple_impl<_Idx, _UHead, _UTails...>&& __in)
 : _Inherited(std::move
       (_Tuple_impl<_Idx, _UHead, _UTails...>::_M_tail(__in))),
   _Base(std::forward<_UHead>
  (_Tuple_impl<_Idx, _UHead, _UTails...>::_M_head(__in)))
 { }
# 368 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a)
 : _Inherited(__tag, __a),
   _Base(__tag, __use_alloc<_Head>(__a))
 { }

      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      const _Head& __head, const _Tail&... __tail)
 : _Inherited(__tag, __a, __tail...),
   _Base(__use_alloc<_Head, _Alloc, _Head>(__a), __head)
 { }

      template<typename _Alloc, typename _UHead, typename... _UTail,
        typename = __enable_if_t<sizeof...(_Tail) == sizeof...(_UTail)>>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      _UHead&& __head, _UTail&&... __tail)
 : _Inherited(__tag, __a, std::forward<_UTail>(__tail)...),
   _Base(__use_alloc<_Head, _Alloc, _UHead>(__a),
  std::forward<_UHead>(__head))
 { }

      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      const _Tuple_impl& __in)
 : _Inherited(__tag, __a, _M_tail(__in)),
   _Base(__use_alloc<_Head, _Alloc, _Head>(__a), _M_head(__in))
 { }

      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      _Tuple_impl&& __in)
 : _Inherited(__tag, __a, std::move(_M_tail(__in))),
   _Base(__use_alloc<_Head, _Alloc, _Head>(__a),
  std::forward<_Head>(_M_head(__in)))
 { }

      template<typename _Alloc, typename _UHead, typename... _UTails>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      const _Tuple_impl<_Idx, _UHead, _UTails...>& __in)
 : _Inherited(__tag, __a,
       _Tuple_impl<_Idx, _UHead, _UTails...>::_M_tail(__in)),
   _Base(__use_alloc<_Head, _Alloc, const _UHead&>(__a),
  _Tuple_impl<_Idx, _UHead, _UTails...>::_M_head(__in))
 { }

      template<typename _Alloc, typename _UHead, typename... _UTails>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      _Tuple_impl<_Idx, _UHead, _UTails...>&& __in)
 : _Inherited(__tag, __a, std::move
       (_Tuple_impl<_Idx, _UHead, _UTails...>::_M_tail(__in))),
   _Base(__use_alloc<_Head, _Alloc, _UHead>(__a),
  std::forward<_UHead>
  (_Tuple_impl<_Idx, _UHead, _UTails...>::_M_head(__in)))
 { }
# 463 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename... _UElements>

 void
 _M_assign(const _Tuple_impl<_Idx, _UElements...>& __in)
 {
   _M_head(*this) = _Tuple_impl<_Idx, _UElements...>::_M_head(__in);
   _M_tail(*this)._M_assign(
       _Tuple_impl<_Idx, _UElements...>::_M_tail(__in));
 }

      template<typename _UHead, typename... _UTails>

 void
 _M_assign(_Tuple_impl<_Idx, _UHead, _UTails...>&& __in)
 {
   _M_head(*this) = std::forward<_UHead>
     (_Tuple_impl<_Idx, _UHead, _UTails...>::_M_head(__in));
   _M_tail(*this)._M_assign(
       std::move(_Tuple_impl<_Idx, _UHead, _UTails...>::_M_tail(__in)));
 }
# 523 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    protected:
     
      void
      _M_swap(_Tuple_impl& __in)
      {
 using std::swap;
 swap(_M_head(*this), _M_head(__in));
 _Inherited::_M_swap(_M_tail(__in));
      }
# 542 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    };


  template<size_t _Idx, typename _Head>
    struct _Tuple_impl<_Idx, _Head>
    : private _Head_base<_Idx, _Head>
    {
      template<size_t, typename...> friend struct _Tuple_impl;

      typedef _Head_base<_Idx, _Head> _Base;

      static constexpr _Head&
      _M_head(_Tuple_impl& __t) noexcept { return _Base::_M_head(__t); }

      static constexpr const _Head&
      _M_head(const _Tuple_impl& __t) noexcept { return _Base::_M_head(__t); }

      constexpr
      _Tuple_impl()
      : _Base() { }

      explicit constexpr
      _Tuple_impl(const _Head& __head)
      : _Base(__head)
      { }

      template<typename _UHead>
 explicit constexpr
 _Tuple_impl(_UHead&& __head)
 : _Base(std::forward<_UHead>(__head))
 { }

      constexpr _Tuple_impl(const _Tuple_impl&) = default;



      _Tuple_impl& operator=(const _Tuple_impl&) = delete;




      constexpr
      _Tuple_impl(_Tuple_impl&& __in)
      noexcept(is_nothrow_move_constructible<_Head>::value)
      : _Base(static_cast<_Base&&>(__in))
      { }


      template<typename _UHead>
 constexpr
 _Tuple_impl(const _Tuple_impl<_Idx, _UHead>& __in)
 : _Base(_Tuple_impl<_Idx, _UHead>::_M_head(__in))
 { }

      template<typename _UHead>
 constexpr
 _Tuple_impl(_Tuple_impl<_Idx, _UHead>&& __in)
 : _Base(std::forward<_UHead>(_Tuple_impl<_Idx, _UHead>::_M_head(__in)))
 { }
# 624 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a)
 : _Base(__tag, __use_alloc<_Head>(__a))
 { }

      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      const _Head& __head)
 : _Base(__use_alloc<_Head, _Alloc, const _Head&>(__a), __head)
 { }

      template<typename _Alloc, typename _UHead>

 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      _UHead&& __head)
 : _Base(__use_alloc<_Head, _Alloc, _UHead>(__a),
  std::forward<_UHead>(__head))
 { }

      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      const _Tuple_impl& __in)
 : _Base(__use_alloc<_Head, _Alloc, const _Head&>(__a), _M_head(__in))
 { }

      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      _Tuple_impl&& __in)
 : _Base(__use_alloc<_Head, _Alloc, _Head>(__a),
  std::forward<_Head>(_M_head(__in)))
 { }

      template<typename _Alloc, typename _UHead>

 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      const _Tuple_impl<_Idx, _UHead>& __in)
 : _Base(__use_alloc<_Head, _Alloc, const _UHead&>(__a),
  _Tuple_impl<_Idx, _UHead>::_M_head(__in))
 { }

      template<typename _Alloc, typename _UHead>

 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      _Tuple_impl<_Idx, _UHead>&& __in)
 : _Base(__use_alloc<_Head, _Alloc, _UHead>(__a),
  std::forward<_UHead>(_Tuple_impl<_Idx, _UHead>::_M_head(__in)))
 { }
# 703 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename _UHead>

 void
 _M_assign(const _Tuple_impl<_Idx, _UHead>& __in)
 {
   _M_head(*this) = _Tuple_impl<_Idx, _UHead>::_M_head(__in);
 }

      template<typename _UHead>

 void
 _M_assign(_Tuple_impl<_Idx, _UHead>&& __in)
 {
   _M_head(*this)
     = std::forward<_UHead>(_Tuple_impl<_Idx, _UHead>::_M_head(__in));
 }
# 749 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    protected:
     
      void
      _M_swap(_Tuple_impl& __in)
      {
 using std::swap;
 swap(_M_head(*this), _M_head(__in));
      }
# 766 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    };



  template<bool, typename... _Types>
    struct _TupleConstraints
    {
      template<typename... _UTypes>
 using __constructible = __and_<is_constructible<_Types, _UTypes>...>;

      template<typename... _UTypes>
 using __convertible = __and_<is_convertible<_UTypes, _Types>...>;




      template<typename... _UTypes>
 static constexpr bool __is_implicitly_constructible()
 {
   return __and_<__constructible<_UTypes...>,
   __convertible<_UTypes...>
   >::value;
 }




      template<typename... _UTypes>
 static constexpr bool __is_explicitly_constructible()
 {
   return __and_<__constructible<_UTypes...>,
   __not_<__convertible<_UTypes...>>
   >::value;
 }

      static constexpr bool __is_implicitly_default_constructible()
      {
 return __and_<std::__is_implicitly_default_constructible<_Types>...
        >::value;
      }

      static constexpr bool __is_explicitly_default_constructible()
      {
 return __and_<is_default_constructible<_Types>...,
        __not_<__and_<
   std::__is_implicitly_default_constructible<_Types>...>
        >>::value;
      }
    };



  template<typename... _Types>
    struct _TupleConstraints<false, _Types...>
    {
      template<typename... _UTypes>
 static constexpr bool __is_implicitly_constructible()
 { return false; }

      template<typename... _UTypes>
 static constexpr bool __is_explicitly_constructible()
 { return false; }
    };



  template<typename... _Elements>
    class tuple : public _Tuple_impl<0, _Elements...>
    {
      using _Inherited = _Tuple_impl<0, _Elements...>;
# 1350 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<bool _Cond>
 using _TCC = _TupleConstraints<_Cond, _Elements...>;


      template<bool _Dummy>
 using _ImplicitDefaultCtor = __enable_if_t<
   _TCC<_Dummy>::__is_implicitly_default_constructible(),
   bool>;


      template<bool _Dummy>
 using _ExplicitDefaultCtor = __enable_if_t<
   _TCC<_Dummy>::__is_explicitly_default_constructible(),
   bool>;


      template<bool _Cond, typename... _Args>
 using _ImplicitCtor = __enable_if_t<
   _TCC<_Cond>::template __is_implicitly_constructible<_Args...>(),
   bool>;


      template<bool _Cond, typename... _Args>
 using _ExplicitCtor = __enable_if_t<
   _TCC<_Cond>::template __is_explicitly_constructible<_Args...>(),
   bool>;


      template<typename... _UElements>
 static constexpr bool __nothrow_constructible()
 {
   return
     __and_<is_nothrow_constructible<_Elements, _UElements>...>::value;
 }


      template<typename _Up>
 static constexpr bool __valid_args()
 {
   return sizeof...(_Elements) == 1
     && !is_same<tuple, __remove_cvref_t<_Up>>::value;
 }


      template<typename, typename, typename... _Tail>
 static constexpr bool __valid_args()
 { return (sizeof...(_Tail) + 2) == sizeof...(_Elements); }
# 1407 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename _Tuple, typename = tuple,
        typename = __remove_cvref_t<_Tuple>>
 struct _UseOtherCtor
 : false_type
 { };


      template<typename _Tuple, typename _Tp, typename _Up>
 struct _UseOtherCtor<_Tuple, tuple<_Tp>, tuple<_Up>>
 : __or_<is_convertible<_Tuple, _Tp>, is_constructible<_Tp, _Tuple>>::type
 { };


      template<typename _Tuple, typename _Tp>
 struct _UseOtherCtor<_Tuple, tuple<_Tp>, tuple<_Tp>>
 : true_type
 { };




      template<typename _Tuple>
 static constexpr bool __use_other_ctor()
 { return _UseOtherCtor<_Tuple>::value; }
# 1453 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    public:
      template<typename _Dummy = void,
        _ImplicitDefaultCtor<is_void<_Dummy>::value> = true>
 constexpr
 tuple()
 noexcept(__and_<is_nothrow_default_constructible<_Elements>...>::value)
 : _Inherited() { }

      template<typename _Dummy = void,
        _ExplicitDefaultCtor<is_void<_Dummy>::value> = false>
 explicit constexpr
 tuple()
 noexcept(__and_<is_nothrow_default_constructible<_Elements>...>::value)
 : _Inherited() { }

      template<bool _NotEmpty = (sizeof...(_Elements) >= 1),
        _ImplicitCtor<_NotEmpty, const _Elements&...> = true>
 constexpr
 tuple(const _Elements&... __elements)
 noexcept(__nothrow_constructible<const _Elements&...>())
 : _Inherited(__elements...) { }

      template<bool _NotEmpty = (sizeof...(_Elements) >= 1),
        _ExplicitCtor<_NotEmpty, const _Elements&...> = false>
 explicit constexpr
 tuple(const _Elements&... __elements)
 noexcept(__nothrow_constructible<const _Elements&...>())
 : _Inherited(__elements...) { }

      template<typename... _UElements,
        bool _Valid = __valid_args<_UElements...>(),
        _ImplicitCtor<_Valid, _UElements...> = true>
 constexpr
 tuple(_UElements&&... __elements)
 noexcept(__nothrow_constructible<_UElements...>())
 : _Inherited(std::forward<_UElements>(__elements)...)
 { ; }

      template<typename... _UElements,
        bool _Valid = __valid_args<_UElements...>(),
        _ExplicitCtor<_Valid, _UElements...> = false>
 explicit constexpr
 tuple(_UElements&&... __elements)
 noexcept(__nothrow_constructible<_UElements...>())
 : _Inherited(std::forward<_UElements>(__elements)...)
 { ; }

      constexpr tuple(const tuple&) = default;

      constexpr tuple(tuple&&) = default;

      template<typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
      && !__use_other_ctor<const tuple<_UElements...>&>(),
        _ImplicitCtor<_Valid, const _UElements&...> = true>
 constexpr
 tuple(const tuple<_UElements...>& __in)
 noexcept(__nothrow_constructible<const _UElements&...>())
 : _Inherited(static_cast<const _Tuple_impl<0, _UElements...>&>(__in))
 { ; }

      template<typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
      && !__use_other_ctor<const tuple<_UElements...>&>(),
        _ExplicitCtor<_Valid, const _UElements&...> = false>
 explicit constexpr
 tuple(const tuple<_UElements...>& __in)
 noexcept(__nothrow_constructible<const _UElements&...>())
 : _Inherited(static_cast<const _Tuple_impl<0, _UElements...>&>(__in))
 { ; }

      template<typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
        && !__use_other_ctor<tuple<_UElements...>&&>(),
        _ImplicitCtor<_Valid, _UElements...> = true>
 constexpr
 tuple(tuple<_UElements...>&& __in)
 noexcept(__nothrow_constructible<_UElements...>())
 : _Inherited(static_cast<_Tuple_impl<0, _UElements...>&&>(__in))
 { ; }

      template<typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
        && !__use_other_ctor<tuple<_UElements...>&&>(),
        _ExplicitCtor<_Valid, _UElements...> = false>
 explicit constexpr
 tuple(tuple<_UElements...>&& __in)
 noexcept(__nothrow_constructible<_UElements...>())
 : _Inherited(static_cast<_Tuple_impl<0, _UElements...>&&>(__in))
 { ; }



      template<typename _Alloc,
        _ImplicitDefaultCtor<is_object<_Alloc>::value> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a)
 : _Inherited(__tag, __a) { }

      template<typename _Alloc,
        _ExplicitDefaultCtor<is_object<_Alloc>::value> = false>

 explicit
 tuple(allocator_arg_t __tag, const _Alloc& __a)
 : _Inherited(__tag, __a) { }

      template<typename _Alloc, bool _NotEmpty = (sizeof...(_Elements) >= 1),
        _ImplicitCtor<_NotEmpty, const _Elements&...> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const _Elements&... __elements)
 : _Inherited(__tag, __a, __elements...) { }

      template<typename _Alloc, bool _NotEmpty = (sizeof...(_Elements) >= 1),
        _ExplicitCtor<_NotEmpty, const _Elements&...> = false>

 explicit
 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const _Elements&... __elements)
 : _Inherited(__tag, __a, __elements...) { }

      template<typename _Alloc, typename... _UElements,
        bool _Valid = __valid_args<_UElements...>(),
        _ImplicitCtor<_Valid, _UElements...> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       _UElements&&... __elements)
 : _Inherited(__tag, __a, std::forward<_UElements>(__elements)...)
 { ; }

      template<typename _Alloc, typename... _UElements,
   bool _Valid = __valid_args<_UElements...>(),
        _ExplicitCtor<_Valid, _UElements...> = false>

 explicit
 tuple(allocator_arg_t __tag, const _Alloc& __a,
       _UElements&&... __elements)
 : _Inherited(__tag, __a, std::forward<_UElements>(__elements)...)
 { ; }

      template<typename _Alloc>

 tuple(allocator_arg_t __tag, const _Alloc& __a, const tuple& __in)
 : _Inherited(__tag, __a, static_cast<const _Inherited&>(__in)) { }

      template<typename _Alloc>

 tuple(allocator_arg_t __tag, const _Alloc& __a, tuple&& __in)
 : _Inherited(__tag, __a, static_cast<_Inherited&&>(__in)) { }

      template<typename _Alloc, typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
        && !__use_other_ctor<const tuple<_UElements...>&>(),
        _ImplicitCtor<_Valid, const _UElements&...> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const tuple<_UElements...>& __in)
 : _Inherited(__tag, __a,
       static_cast<const _Tuple_impl<0, _UElements...>&>(__in))
 { ; }

      template<typename _Alloc, typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
        && !__use_other_ctor<const tuple<_UElements...>&>(),
        _ExplicitCtor<_Valid, const _UElements&...> = false>

 explicit
 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const tuple<_UElements...>& __in)
 : _Inherited(__tag, __a,
       static_cast<const _Tuple_impl<0, _UElements...>&>(__in))
 { ; }

      template<typename _Alloc, typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
        && !__use_other_ctor<tuple<_UElements...>&&>(),
        _ImplicitCtor<_Valid, _UElements...> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       tuple<_UElements...>&& __in)
 : _Inherited(__tag, __a,
       static_cast<_Tuple_impl<0, _UElements...>&&>(__in))
 { ; }

      template<typename _Alloc, typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
        && !__use_other_ctor<tuple<_UElements...>&&>(),
        _ExplicitCtor<_Valid, _UElements...> = false>

 explicit
 tuple(allocator_arg_t __tag, const _Alloc& __a,
       tuple<_UElements...>&& __in)
 : _Inherited(__tag, __a,
       static_cast<_Tuple_impl<0, _UElements...>&&>(__in))
 { ; }
# 1885 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    private:
      template<typename... _UElements>
 static constexpr
 __enable_if_t<sizeof...(_UElements) == sizeof...(_Elements), bool>
 __assignable()
 { return __and_<is_assignable<_Elements&, _UElements>...>::value; }


      template<typename... _UElements>
 static constexpr bool __nothrow_assignable()
 {
   return
     __and_<is_nothrow_assignable<_Elements&, _UElements>...>::value;
 }

    public:

     
      tuple&
      operator=(__conditional_t<__assignable<const _Elements&...>(),
    const tuple&,
    const __nonesuch&> __in)
      noexcept(__nothrow_assignable<const _Elements&...>())
      {
 this->_M_assign(__in);
 return *this;
      }

     
      tuple&
      operator=(__conditional_t<__assignable<_Elements...>(),
    tuple&&,
    __nonesuch&&> __in)
      noexcept(__nothrow_assignable<_Elements...>())
      {
 this->_M_assign(std::move(__in));
 return *this;
      }

      template<typename... _UElements>

 __enable_if_t<__assignable<const _UElements&...>(), tuple&>
 operator=(const tuple<_UElements...>& __in)
 noexcept(__nothrow_assignable<const _UElements&...>())
 {
   this->_M_assign(__in);
   return *this;
 }

      template<typename... _UElements>

 __enable_if_t<__assignable<_UElements...>(), tuple&>
 operator=(tuple<_UElements...>&& __in)
 noexcept(__nothrow_assignable<_UElements...>())
 {
   this->_M_assign(std::move(__in));
   return *this;
 }



     
      void
      swap(tuple& __in)
      noexcept(__and_<__is_nothrow_swappable<_Elements>...>::value)
      { _Inherited::_M_swap(__in); }
# 1965 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    };
# 1981 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<>
    class tuple<>
    {
    public:
     
      void swap(tuple&) noexcept { }





      tuple() = default;

      template<typename _Alloc>

 tuple(allocator_arg_t, const _Alloc&) noexcept { }
      template<typename _Alloc>

 tuple(allocator_arg_t, const _Alloc&, const tuple&) noexcept { }
    };




  template<typename _T1, typename _T2>
    class tuple<_T1, _T2> : public _Tuple_impl<0, _T1, _T2>
    {
      typedef _Tuple_impl<0, _T1, _T2> _Inherited;


      template<bool _Dummy, typename _U1, typename _U2>
 using _ImplicitDefaultCtor = __enable_if_t<
   _TupleConstraints<_Dummy, _U1, _U2>::
     __is_implicitly_default_constructible(),
   bool>;


      template<bool _Dummy, typename _U1, typename _U2>
 using _ExplicitDefaultCtor = __enable_if_t<
   _TupleConstraints<_Dummy, _U1, _U2>::
     __is_explicitly_default_constructible(),
   bool>;

      template<bool _Dummy>
 using _TCC = _TupleConstraints<_Dummy, _T1, _T2>;


      template<bool _Cond, typename _U1, typename _U2>
 using _ImplicitCtor = __enable_if_t<
   _TCC<_Cond>::template __is_implicitly_constructible<_U1, _U2>(),
   bool>;


      template<bool _Cond, typename _U1, typename _U2>
 using _ExplicitCtor = __enable_if_t<
   _TCC<_Cond>::template __is_explicitly_constructible<_U1, _U2>(),
   bool>;

      template<typename _U1, typename _U2>
 static constexpr bool __assignable()
 {
   return __and_<is_assignable<_T1&, _U1>,
   is_assignable<_T2&, _U2>>::value;
 }

      template<typename _U1, typename _U2>
 static constexpr bool __nothrow_assignable()
 {
   return __and_<is_nothrow_assignable<_T1&, _U1>,
   is_nothrow_assignable<_T2&, _U2>>::value;
 }

      template<typename _U1, typename _U2>
 static constexpr bool __nothrow_constructible()
 {
   return __and_<is_nothrow_constructible<_T1, _U1>,
       is_nothrow_constructible<_T2, _U2>>::value;
 }

      static constexpr bool __nothrow_default_constructible()
      {
 return __and_<is_nothrow_default_constructible<_T1>,
        is_nothrow_default_constructible<_T2>>::value;
      }

      template<typename _U1>
 static constexpr bool __is_alloc_arg()
 { return is_same<__remove_cvref_t<_U1>, allocator_arg_t>::value; }
# 2084 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    public:
      template<bool _Dummy = true,
        _ImplicitDefaultCtor<_Dummy, _T1, _T2> = true>
 constexpr
 tuple()
 noexcept(__nothrow_default_constructible())
 : _Inherited() { }

      template<bool _Dummy = true,
        _ExplicitDefaultCtor<_Dummy, _T1, _T2> = false>
 explicit constexpr
 tuple()
 noexcept(__nothrow_default_constructible())
 : _Inherited() { }

      template<bool _Dummy = true,
        _ImplicitCtor<_Dummy, const _T1&, const _T2&> = true>
 constexpr
 tuple(const _T1& __a1, const _T2& __a2)
 noexcept(__nothrow_constructible<const _T1&, const _T2&>())
 : _Inherited(__a1, __a2) { }

      template<bool _Dummy = true,
        _ExplicitCtor<_Dummy, const _T1&, const _T2&> = false>
 explicit constexpr
 tuple(const _T1& __a1, const _T2& __a2)
 noexcept(__nothrow_constructible<const _T1&, const _T2&>())
 : _Inherited(__a1, __a2) { }

      template<typename _U1, typename _U2,
        _ImplicitCtor<!__is_alloc_arg<_U1>(), _U1, _U2> = true>
 constexpr
 tuple(_U1&& __a1, _U2&& __a2)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(std::forward<_U1>(__a1), std::forward<_U2>(__a2))
 { ; }

      template<typename _U1, typename _U2,
        _ExplicitCtor<!__is_alloc_arg<_U1>(), _U1, _U2> = false>
 explicit constexpr
 tuple(_U1&& __a1, _U2&& __a2)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(std::forward<_U1>(__a1), std::forward<_U2>(__a2))
 { ; }

      constexpr tuple(const tuple&) = default;

      constexpr tuple(tuple&&) = default;

      template<typename _U1, typename _U2,
        _ImplicitCtor<true, const _U1&, const _U2&> = true>
 constexpr
 tuple(const tuple<_U1, _U2>& __in)
 noexcept(__nothrow_constructible<const _U1&, const _U2&>())
 : _Inherited(static_cast<const _Tuple_impl<0, _U1, _U2>&>(__in))
 { ; }

      template<typename _U1, typename _U2,
        _ExplicitCtor<true, const _U1&, const _U2&> = false>
 explicit constexpr
 tuple(const tuple<_U1, _U2>& __in)
 noexcept(__nothrow_constructible<const _U1&, const _U2&>())
 : _Inherited(static_cast<const _Tuple_impl<0, _U1, _U2>&>(__in))
 { ; }

      template<typename _U1, typename _U2,
        _ImplicitCtor<true, _U1, _U2> = true>
 constexpr
 tuple(tuple<_U1, _U2>&& __in)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(static_cast<_Tuple_impl<0, _U1, _U2>&&>(__in))
 { ; }

      template<typename _U1, typename _U2,
        _ExplicitCtor<true, _U1, _U2> = false>
 explicit constexpr
 tuple(tuple<_U1, _U2>&& __in)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(static_cast<_Tuple_impl<0, _U1, _U2>&&>(__in))
 { ; }

      template<typename _U1, typename _U2,
        _ImplicitCtor<true, const _U1&, const _U2&> = true>
 constexpr
 tuple(const pair<_U1, _U2>& __in)
 noexcept(__nothrow_constructible<const _U1&, const _U2&>())
 : _Inherited(__in.first, __in.second)
 { ; }

      template<typename _U1, typename _U2,
        _ExplicitCtor<true, const _U1&, const _U2&> = false>
 explicit constexpr
 tuple(const pair<_U1, _U2>& __in)
 noexcept(__nothrow_constructible<const _U1&, const _U2&>())
 : _Inherited(__in.first, __in.second)
 { ; }

      template<typename _U1, typename _U2,
        _ImplicitCtor<true, _U1, _U2> = true>
 constexpr
 tuple(pair<_U1, _U2>&& __in)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(std::forward<_U1>(__in.first),
       std::forward<_U2>(__in.second))
 { ; }

      template<typename _U1, typename _U2,
        _ExplicitCtor<true, _U1, _U2> = false>
 explicit constexpr
 tuple(pair<_U1, _U2>&& __in)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(std::forward<_U1>(__in.first),
       std::forward<_U2>(__in.second))
 { ; }



      template<typename _Alloc,
        _ImplicitDefaultCtor<is_object<_Alloc>::value, _T1, _T2> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a)
 : _Inherited(__tag, __a) { }

      template<typename _Alloc,
        _ExplicitDefaultCtor<is_object<_Alloc>::value, _T1, _T2> = false>

 explicit
 tuple(allocator_arg_t __tag, const _Alloc& __a)
 : _Inherited(__tag, __a) { }

      template<typename _Alloc, bool _Dummy = true,
        _ImplicitCtor<_Dummy, const _T1&, const _T2&> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const _T1& __a1, const _T2& __a2)
 : _Inherited(__tag, __a, __a1, __a2) { }

      template<typename _Alloc, bool _Dummy = true,
        _ExplicitCtor<_Dummy, const _T1&, const _T2&> = false>
 explicit

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const _T1& __a1, const _T2& __a2)
 : _Inherited(__tag, __a, __a1, __a2) { }

      template<typename _Alloc, typename _U1, typename _U2,
        _ImplicitCtor<true, _U1, _U2> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a, _U1&& __a1, _U2&& __a2)
 : _Inherited(__tag, __a, std::forward<_U1>(__a1),
       std::forward<_U2>(__a2))
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ExplicitCtor<true, _U1, _U2> = false>
 explicit

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       _U1&& __a1, _U2&& __a2)
 : _Inherited(__tag, __a, std::forward<_U1>(__a1),
       std::forward<_U2>(__a2))
 { ; }

      template<typename _Alloc>

 tuple(allocator_arg_t __tag, const _Alloc& __a, const tuple& __in)
 : _Inherited(__tag, __a, static_cast<const _Inherited&>(__in)) { }

      template<typename _Alloc>

 tuple(allocator_arg_t __tag, const _Alloc& __a, tuple&& __in)
 : _Inherited(__tag, __a, static_cast<_Inherited&&>(__in)) { }

      template<typename _Alloc, typename _U1, typename _U2,
        _ImplicitCtor<true, const _U1&, const _U2&> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const tuple<_U1, _U2>& __in)
 : _Inherited(__tag, __a,
       static_cast<const _Tuple_impl<0, _U1, _U2>&>(__in))
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ExplicitCtor<true, const _U1&, const _U2&> = false>
 explicit

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const tuple<_U1, _U2>& __in)
 : _Inherited(__tag, __a,
       static_cast<const _Tuple_impl<0, _U1, _U2>&>(__in))
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ImplicitCtor<true, _U1, _U2> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a, tuple<_U1, _U2>&& __in)
 : _Inherited(__tag, __a, static_cast<_Tuple_impl<0, _U1, _U2>&&>(__in))
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ExplicitCtor<true, _U1, _U2> = false>
 explicit

 tuple(allocator_arg_t __tag, const _Alloc& __a, tuple<_U1, _U2>&& __in)
 : _Inherited(__tag, __a, static_cast<_Tuple_impl<0, _U1, _U2>&&>(__in))
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ImplicitCtor<true, const _U1&, const _U2&> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const pair<_U1, _U2>& __in)
 : _Inherited(__tag, __a, __in.first, __in.second)
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ExplicitCtor<true, const _U1&, const _U2&> = false>
 explicit

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const pair<_U1, _U2>& __in)
 : _Inherited(__tag, __a, __in.first, __in.second)
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ImplicitCtor<true, _U1, _U2> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a, pair<_U1, _U2>&& __in)
 : _Inherited(__tag, __a, std::forward<_U1>(__in.first),
       std::forward<_U2>(__in.second))
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ExplicitCtor<true, _U1, _U2> = false>
 explicit

 tuple(allocator_arg_t __tag, const _Alloc& __a, pair<_U1, _U2>&& __in)
 : _Inherited(__tag, __a, std::forward<_U1>(__in.first),
       std::forward<_U2>(__in.second))
 { ; }



     
      tuple&
      operator=(__conditional_t<__assignable<const _T1&, const _T2&>(),
    const tuple&,
    const __nonesuch&> __in)
      noexcept(__nothrow_assignable<const _T1&, const _T2&>())
      {
 this->_M_assign(__in);
 return *this;
      }

     
      tuple&
      operator=(__conditional_t<__assignable<_T1, _T2>(),
    tuple&&,
    __nonesuch&&> __in)
      noexcept(__nothrow_assignable<_T1, _T2>())
      {
 this->_M_assign(std::move(__in));
 return *this;
      }

      template<typename _U1, typename _U2>

 __enable_if_t<__assignable<const _U1&, const _U2&>(), tuple&>
 operator=(const tuple<_U1, _U2>& __in)
 noexcept(__nothrow_assignable<const _U1&, const _U2&>())
 {
   this->_M_assign(__in);
   return *this;
 }

      template<typename _U1, typename _U2>

 __enable_if_t<__assignable<_U1, _U2>(), tuple&>
 operator=(tuple<_U1, _U2>&& __in)
 noexcept(__nothrow_assignable<_U1, _U2>())
 {
   this->_M_assign(std::move(__in));
   return *this;
 }

      template<typename _U1, typename _U2>

 __enable_if_t<__assignable<const _U1&, const _U2&>(), tuple&>
 operator=(const pair<_U1, _U2>& __in)
 noexcept(__nothrow_assignable<const _U1&, const _U2&>())
 {
   this->_M_head(*this) = __in.first;
   this->_M_tail(*this)._M_head(*this) = __in.second;
   return *this;
 }

      template<typename _U1, typename _U2>

 __enable_if_t<__assignable<_U1, _U2>(), tuple&>
 operator=(pair<_U1, _U2>&& __in)
 noexcept(__nothrow_assignable<_U1, _U2>())
 {
   this->_M_head(*this) = std::forward<_U1>(__in.first);
   this->_M_tail(*this)._M_head(*this) = std::forward<_U2>(__in.second);
   return *this;
 }

     
      void
      swap(tuple& __in)
      noexcept(__and_<__is_nothrow_swappable<_T1>,
        __is_nothrow_swappable<_T2>>::value)
      { _Inherited::_M_swap(__in); }
    };



  template<typename... _Elements>
    struct tuple_size<tuple<_Elements...>>
    : public integral_constant<size_t, sizeof...(_Elements)> { };
# 2416 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<size_t __i, typename... _Types>
    struct tuple_element<__i, tuple<_Types...>>
    {
      static_assert(__i < sizeof...(_Types), "tuple index must be in range");

      using type = typename _Nth_type<__i, _Types...>::type;
    };

  template<size_t __i, typename _Head, typename... _Tail>
    constexpr _Head&
    __get_helper(_Tuple_impl<__i, _Head, _Tail...>& __t) noexcept
    { return _Tuple_impl<__i, _Head, _Tail...>::_M_head(__t); }

  template<size_t __i, typename _Head, typename... _Tail>
    constexpr const _Head&
    __get_helper(const _Tuple_impl<__i, _Head, _Tail...>& __t) noexcept
    { return _Tuple_impl<__i, _Head, _Tail...>::_M_head(__t); }


  template<size_t __i, typename... _Types>
    __enable_if_t<(__i >= sizeof...(_Types))>
    __get_helper(const tuple<_Types...>&) = delete;


  template<size_t __i, typename... _Elements>
    constexpr __tuple_element_t<__i, tuple<_Elements...>>&
    get(tuple<_Elements...>& __t) noexcept
    { return std::__get_helper<__i>(__t); }


  template<size_t __i, typename... _Elements>
    constexpr const __tuple_element_t<__i, tuple<_Elements...>>&
    get(const tuple<_Elements...>& __t) noexcept
    { return std::__get_helper<__i>(__t); }


  template<size_t __i, typename... _Elements>
    constexpr __tuple_element_t<__i, tuple<_Elements...>>&&
    get(tuple<_Elements...>&& __t) noexcept
    {
      typedef __tuple_element_t<__i, tuple<_Elements...>> __element_type;
      return std::forward<__element_type>(std::__get_helper<__i>(__t));
    }


  template<size_t __i, typename... _Elements>
    constexpr const __tuple_element_t<__i, tuple<_Elements...>>&&
    get(const tuple<_Elements...>&& __t) noexcept
    {
      typedef __tuple_element_t<__i, tuple<_Elements...>> __element_type;
      return std::forward<const __element_type>(std::__get_helper<__i>(__t));
    }



  template<size_t __i, typename... _Elements>
    constexpr __enable_if_t<(__i >= sizeof...(_Elements))>
    get(const tuple<_Elements...>&) = delete;




  template <typename _Tp, typename... _Types>
    constexpr _Tp&
    get(tuple<_Types...>& __t) noexcept
    {
      constexpr size_t __idx = __find_uniq_type_in_pack<_Tp, _Types...>();
      static_assert(__idx < sizeof...(_Types),
   "the type T in std::get<T> must occur exactly once in the tuple");
      return std::__get_helper<__idx>(__t);
    }


  template <typename _Tp, typename... _Types>
    constexpr _Tp&&
    get(tuple<_Types...>&& __t) noexcept
    {
      constexpr size_t __idx = __find_uniq_type_in_pack<_Tp, _Types...>();
      static_assert(__idx < sizeof...(_Types),
   "the type T in std::get<T> must occur exactly once in the tuple");
      return std::forward<_Tp>(std::__get_helper<__idx>(__t));
    }


  template <typename _Tp, typename... _Types>
    constexpr const _Tp&
    get(const tuple<_Types...>& __t) noexcept
    {
      constexpr size_t __idx = __find_uniq_type_in_pack<_Tp, _Types...>();
      static_assert(__idx < sizeof...(_Types),
   "the type T in std::get<T> must occur exactly once in the tuple");
      return std::__get_helper<__idx>(__t);
    }



  template <typename _Tp, typename... _Types>
    constexpr const _Tp&&
    get(const tuple<_Types...>&& __t) noexcept
    {
      constexpr size_t __idx = __find_uniq_type_in_pack<_Tp, _Types...>();
      static_assert(__idx < sizeof...(_Types),
   "the type T in std::get<T> must occur exactly once in the tuple");
      return std::forward<const _Tp>(std::__get_helper<__idx>(__t));
    }
# 2576 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<typename _Tp, typename _Up, size_t __i, size_t __size>
    struct __tuple_compare
    {
      static constexpr bool
      __eq(const _Tp& __t, const _Up& __u)
      {
 return bool(std::get<__i>(__t) == std::get<__i>(__u))
   && __tuple_compare<_Tp, _Up, __i + 1, __size>::__eq(__t, __u);
      }

      static constexpr bool
      __less(const _Tp& __t, const _Up& __u)
      {
 return bool(std::get<__i>(__t) < std::get<__i>(__u))
   || (!bool(std::get<__i>(__u) < std::get<__i>(__t))
       && __tuple_compare<_Tp, _Up, __i + 1, __size>::__less(__t, __u));
      }
    };

  template<typename _Tp, typename _Up, size_t __size>
    struct __tuple_compare<_Tp, _Up, __size, __size>
    {
      static constexpr bool
      __eq(const _Tp&, const _Up&) { return true; }

      static constexpr bool
      __less(const _Tp&, const _Up&) { return false; }
    };

  template<typename... _TElements, typename... _UElements>
    [[__nodiscard__]]
    constexpr bool
    operator==(const tuple<_TElements...>& __t,
        const tuple<_UElements...>& __u)
    {
      static_assert(sizeof...(_TElements) == sizeof...(_UElements),
   "tuple objects can only be compared if they have equal sizes.");
      using __compare = __tuple_compare<tuple<_TElements...>,
     tuple<_UElements...>,
     0, sizeof...(_TElements)>;
      return __compare::__eq(__t, __u);
    }

  template<typename... _TElements, typename... _UElements>
    [[__nodiscard__]]
    constexpr bool
    operator<(const tuple<_TElements...>& __t,
       const tuple<_UElements...>& __u)
    {
      static_assert(sizeof...(_TElements) == sizeof...(_UElements),
   "tuple objects can only be compared if they have equal sizes.");
      using __compare = __tuple_compare<tuple<_TElements...>,
     tuple<_UElements...>,
     0, sizeof...(_TElements)>;
      return __compare::__less(__t, __u);
    }

  template<typename... _TElements, typename... _UElements>
    [[__nodiscard__]]
    constexpr bool
    operator!=(const tuple<_TElements...>& __t,
        const tuple<_UElements...>& __u)
    { return !(__t == __u); }

  template<typename... _TElements, typename... _UElements>
    [[__nodiscard__]]
    constexpr bool
    operator>(const tuple<_TElements...>& __t,
       const tuple<_UElements...>& __u)
    { return __u < __t; }

  template<typename... _TElements, typename... _UElements>
    [[__nodiscard__]]
    constexpr bool
    operator<=(const tuple<_TElements...>& __t,
        const tuple<_UElements...>& __u)
    { return !(__u < __t); }

  template<typename... _TElements, typename... _UElements>
    [[__nodiscard__]]
    constexpr bool
    operator>=(const tuple<_TElements...>& __t,
        const tuple<_UElements...>& __u)
    { return !(__t < __u); }




  template<typename... _Elements>
    constexpr tuple<typename __decay_and_strip<_Elements>::__type...>
    make_tuple(_Elements&&... __args)
    {
      typedef tuple<typename __decay_and_strip<_Elements>::__type...>
 __result_type;
      return __result_type(std::forward<_Elements>(__args)...);
    }




  template<typename... _Elements>
    constexpr tuple<_Elements&&...>
    forward_as_tuple(_Elements&&... __args) noexcept
    { return tuple<_Elements&&...>(std::forward<_Elements>(__args)...); }


  template<size_t, typename, typename, size_t>
    struct __make_tuple_impl;

  template<size_t _Idx, typename _Tuple, typename... _Tp, size_t _Nm>
    struct __make_tuple_impl<_Idx, tuple<_Tp...>, _Tuple, _Nm>
    : __make_tuple_impl<_Idx + 1,
   tuple<_Tp..., __tuple_element_t<_Idx, _Tuple>>,
   _Tuple, _Nm>
    { };

  template<size_t _Nm, typename _Tuple, typename... _Tp>
    struct __make_tuple_impl<_Nm, tuple<_Tp...>, _Tuple, _Nm>
    {
      typedef tuple<_Tp...> __type;
    };

  template<typename _Tuple>
    struct __do_make_tuple
    : __make_tuple_impl<0, tuple<>, _Tuple, tuple_size<_Tuple>::value>
    { };


  template<typename _Tuple>
    struct __make_tuple
    : public __do_make_tuple<__remove_cvref_t<_Tuple>>
    { };


  template<typename...>
    struct __combine_tuples;

  template<>
    struct __combine_tuples<>
    {
      typedef tuple<> __type;
    };

  template<typename... _Ts>
    struct __combine_tuples<tuple<_Ts...>>
    {
      typedef tuple<_Ts...> __type;
    };

  template<typename... _T1s, typename... _T2s, typename... _Rem>
    struct __combine_tuples<tuple<_T1s...>, tuple<_T2s...>, _Rem...>
    {
      typedef typename __combine_tuples<tuple<_T1s..., _T2s...>,
     _Rem...>::__type __type;
    };


  template<typename... _Tpls>
    struct __tuple_cat_result
    {
      typedef typename __combine_tuples
        <typename __make_tuple<_Tpls>::__type...>::__type __type;
    };



  template<typename...>
    struct __make_1st_indices;

  template<>
    struct __make_1st_indices<>
    {
      typedef _Index_tuple<> __type;
    };

  template<typename _Tp, typename... _Tpls>
    struct __make_1st_indices<_Tp, _Tpls...>
    {
      typedef typename _Build_index_tuple<tuple_size<
 typename remove_reference<_Tp>::type>::value>::__type __type;
    };




  template<typename _Ret, typename _Indices, typename... _Tpls>
    struct __tuple_concater;

  template<typename _Ret, size_t... _Is, typename _Tp, typename... _Tpls>
    struct __tuple_concater<_Ret, _Index_tuple<_Is...>, _Tp, _Tpls...>
    {
      template<typename... _Us>
        static constexpr _Ret
        _S_do(_Tp&& __tp, _Tpls&&... __tps, _Us&&... __us)
        {
   typedef typename __make_1st_indices<_Tpls...>::__type __idx;
   typedef __tuple_concater<_Ret, __idx, _Tpls...> __next;
   return __next::_S_do(std::forward<_Tpls>(__tps)...,
          std::forward<_Us>(__us)...,
          std::get<_Is>(std::forward<_Tp>(__tp))...);
 }
    };

  template<typename _Ret>
    struct __tuple_concater<_Ret, _Index_tuple<>>
    {
      template<typename... _Us>
 static constexpr _Ret
 _S_do(_Us&&... __us)
        {
   return _Ret(std::forward<_Us>(__us)...);
 }
    };

  template<typename... _Tps>
    struct __is_tuple_like_impl<tuple<_Tps...>> : true_type
    { };






  template<typename... _Tpls, typename = typename
           enable_if<__and_<__is_tuple_like<_Tpls>...>::value>::type>

    constexpr auto
    tuple_cat(_Tpls&&... __tpls)
    -> typename __tuple_cat_result<_Tpls...>::__type
    {
      typedef typename __tuple_cat_result<_Tpls...>::__type __ret;
      typedef typename __make_1st_indices<_Tpls...>::__type __idx;
      typedef __tuple_concater<__ret, __idx, _Tpls...> __concater;
      return __concater::_S_do(std::forward<_Tpls>(__tpls)...);
    }




  template<typename... _Elements>
    constexpr tuple<_Elements&...>
    tie(_Elements&... __args) noexcept
    { return tuple<_Elements&...>(__args...); }


  template<typename... _Elements>
   
    inline





    void

    swap(tuple<_Elements...>& __x, tuple<_Elements...>& __y)
    noexcept(noexcept(__x.swap(__y)))
    { __x.swap(__y); }
# 2853 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<typename... _Types, typename _Alloc>
    struct uses_allocator<tuple<_Types...>, _Alloc> : true_type { };
# 2865 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<class _T1, class _T2>
    template<typename... _Args1, typename... _Args2>
     
      inline
      pair<_T1, _T2>::
      pair(piecewise_construct_t,
    tuple<_Args1...> __first, tuple<_Args2...> __second)
      : pair(__first, __second,
      typename _Build_index_tuple<sizeof...(_Args1)>::__type(),
      typename _Build_index_tuple<sizeof...(_Args2)>::__type())
      { }

  template<class _T1, class _T2>
    template<typename... _Args1, size_t... _Indexes1,
      typename... _Args2, size_t... _Indexes2>
      inline
      pair<_T1, _T2>::
      pair(tuple<_Args1...>& __tuple1, tuple<_Args2...>& __tuple2,
    _Index_tuple<_Indexes1...>, _Index_tuple<_Indexes2...>)
      : first(std::forward<_Args1>(std::get<_Indexes1>(__tuple1))...),
 second(std::forward<_Args2>(std::get<_Indexes2>(__tuple2))...)
      { }
# 3027 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3

}
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 1 3
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 116 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  template<typename _Arg, typename _Result>
    struct unary_function
    {

      typedef _Arg argument_type;


      typedef _Result result_type;
    } __attribute__ ((__deprecated__));





  template<typename _Arg1, typename _Arg2, typename _Result>
    struct binary_function
    {

      typedef _Arg1 first_argument_type;


      typedef _Arg2 second_argument_type;


      typedef _Result result_type;
    } __attribute__ ((__deprecated__));
# 157 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  struct __is_transparent;

  template<typename _Tp = void>
    struct plus;

  template<typename _Tp = void>
    struct minus;

  template<typename _Tp = void>
    struct multiplies;

  template<typename _Tp = void>
    struct divides;

  template<typename _Tp = void>
    struct modulus;

  template<typename _Tp = void>
    struct negate;



#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _Tp>
    struct plus : public binary_function<_Tp, _Tp, _Tp>
    {

      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x + __y; }
    };


  template<typename _Tp>
    struct minus : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x - __y; }
    };


  template<typename _Tp>
    struct multiplies : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x * __y; }
    };


  template<typename _Tp>
    struct divides : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x / __y; }
    };


  template<typename _Tp>
    struct modulus : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x % __y; }
    };


  template<typename _Tp>
    struct negate : public unary_function<_Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x) const
      { return -__x; }
    };
#pragma GCC diagnostic pop


  template<>
    struct plus<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) + std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) + std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) + std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct minus<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) - std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) - std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) - std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct multiplies<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) * std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) * std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) * std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct divides<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) / std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) / std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) / std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct modulus<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) % std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) % std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) % std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct negate<void>
    {
      template <typename _Tp>
 constexpr
 auto
 operator()(_Tp&& __t) const
 noexcept(noexcept(-std::forward<_Tp>(__t)))
 -> decltype(-std::forward<_Tp>(__t))
 { return -std::forward<_Tp>(__t); }

      typedef __is_transparent is_transparent;
    };
# 346 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  template<typename _Tp = void>
    struct equal_to;

  template<typename _Tp = void>
    struct not_equal_to;

  template<typename _Tp = void>
    struct greater;

  template<typename _Tp = void>
    struct less;

  template<typename _Tp = void>
    struct greater_equal;

  template<typename _Tp = void>
    struct less_equal;


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _Tp>
    struct equal_to : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x == __y; }
    };


  template<typename _Tp>
    struct not_equal_to : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x != __y; }
    };


  template<typename _Tp>
    struct greater : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x > __y; }
    };


  template<typename _Tp>
    struct less : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x < __y; }
    };


  template<typename _Tp>
    struct greater_equal : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x >= __y; }
    };


  template<typename _Tp>
    struct less_equal : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x <= __y; }
    };


  template<typename _Tp>
    struct greater<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      constexpr bool
      operator()(_Tp* __x, _Tp* __y) const noexcept
      {

 if (std::__is_constant_evaluated())
   return __x > __y;

 return (long unsigned int)__x > (long unsigned int)__y;
      }
    };


  template<typename _Tp>
    struct less<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      constexpr bool
      operator()(_Tp* __x, _Tp* __y) const noexcept
      {

 if (std::__is_constant_evaluated())
   return __x < __y;

 return (long unsigned int)__x < (long unsigned int)__y;
      }
    };


  template<typename _Tp>
    struct greater_equal<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      constexpr bool
      operator()(_Tp* __x, _Tp* __y) const noexcept
      {

 if (std::__is_constant_evaluated())
   return __x >= __y;

 return (long unsigned int)__x >= (long unsigned int)__y;
      }
    };


  template<typename _Tp>
    struct less_equal<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      constexpr bool
      operator()(_Tp* __x, _Tp* __y) const noexcept
      {

 if (std::__is_constant_evaluated())
   return __x <= __y;

 return (long unsigned int)__x <= (long unsigned int)__y;
      }
    };
#pragma GCC diagnostic pop



  template<>
    struct equal_to<void>
    {
      template <typename _Tp, typename _Up>
 constexpr auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) == std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) == std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) == std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct not_equal_to<void>
    {
      template <typename _Tp, typename _Up>
 constexpr auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) != std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) != std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) != std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct greater<void>
    {
      template <typename _Tp, typename _Up>
 constexpr auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) > std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) > std::forward<_Up>(__u))
 {
   return _S_cmp(std::forward<_Tp>(__t), std::forward<_Up>(__u),
   __ptr_cmp<_Tp, _Up>{});
 }

      template<typename _Tp, typename _Up>
 constexpr bool
 operator()(_Tp* __t, _Up* __u) const noexcept
 { return greater<common_type_t<_Tp*, _Up*>>{}(__t, __u); }

      typedef __is_transparent is_transparent;

    private:
      template <typename _Tp, typename _Up>
 static constexpr decltype(auto)
 _S_cmp(_Tp&& __t, _Up&& __u, false_type)
 { return std::forward<_Tp>(__t) > std::forward<_Up>(__u); }

      template <typename _Tp, typename _Up>
 static constexpr bool
 _S_cmp(_Tp&& __t, _Up&& __u, true_type) noexcept
 {
   return greater<const volatile void*>{}(
       static_cast<const volatile void*>(std::forward<_Tp>(__t)),
       static_cast<const volatile void*>(std::forward<_Up>(__u)));
 }


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded2 : true_type { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded2<_Tp, _Up, __void_t<
   decltype(std::declval<_Tp>().operator>(std::declval<_Up>()))>>
 : false_type { };


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded : __not_overloaded2<_Tp, _Up> { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded<_Tp, _Up, __void_t<
   decltype(operator>(std::declval<_Tp>(), std::declval<_Up>()))>>
 : false_type { };

      template<typename _Tp, typename _Up>
 using __ptr_cmp = __and_<__not_overloaded<_Tp, _Up>,
       is_convertible<_Tp, const volatile void*>,
       is_convertible<_Up, const volatile void*>>;
    };


  template<>
    struct less<void>
    {
      template <typename _Tp, typename _Up>
 constexpr auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) < std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) < std::forward<_Up>(__u))
 {
   return _S_cmp(std::forward<_Tp>(__t), std::forward<_Up>(__u),
   __ptr_cmp<_Tp, _Up>{});
 }

      template<typename _Tp, typename _Up>
 constexpr bool
 operator()(_Tp* __t, _Up* __u) const noexcept
 { return less<common_type_t<_Tp*, _Up*>>{}(__t, __u); }

      typedef __is_transparent is_transparent;

    private:
      template <typename _Tp, typename _Up>
 static constexpr decltype(auto)
 _S_cmp(_Tp&& __t, _Up&& __u, false_type)
 { return std::forward<_Tp>(__t) < std::forward<_Up>(__u); }

      template <typename _Tp, typename _Up>
 static constexpr bool
 _S_cmp(_Tp&& __t, _Up&& __u, true_type) noexcept
 {
   return less<const volatile void*>{}(
       static_cast<const volatile void*>(std::forward<_Tp>(__t)),
       static_cast<const volatile void*>(std::forward<_Up>(__u)));
 }


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded2 : true_type { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded2<_Tp, _Up, __void_t<
   decltype(std::declval<_Tp>().operator<(std::declval<_Up>()))>>
 : false_type { };


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded : __not_overloaded2<_Tp, _Up> { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded<_Tp, _Up, __void_t<
   decltype(operator<(std::declval<_Tp>(), std::declval<_Up>()))>>
 : false_type { };

      template<typename _Tp, typename _Up>
 using __ptr_cmp = __and_<__not_overloaded<_Tp, _Up>,
       is_convertible<_Tp, const volatile void*>,
       is_convertible<_Up, const volatile void*>>;
    };


  template<>
    struct greater_equal<void>
    {
      template <typename _Tp, typename _Up>
 constexpr auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) >= std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) >= std::forward<_Up>(__u))
 {
   return _S_cmp(std::forward<_Tp>(__t), std::forward<_Up>(__u),
   __ptr_cmp<_Tp, _Up>{});
 }

      template<typename _Tp, typename _Up>
 constexpr bool
 operator()(_Tp* __t, _Up* __u) const noexcept
 { return greater_equal<common_type_t<_Tp*, _Up*>>{}(__t, __u); }

      typedef __is_transparent is_transparent;

    private:
      template <typename _Tp, typename _Up>
 static constexpr decltype(auto)
 _S_cmp(_Tp&& __t, _Up&& __u, false_type)
 { return std::forward<_Tp>(__t) >= std::forward<_Up>(__u); }

      template <typename _Tp, typename _Up>
 static constexpr bool
 _S_cmp(_Tp&& __t, _Up&& __u, true_type) noexcept
 {
   return greater_equal<const volatile void*>{}(
       static_cast<const volatile void*>(std::forward<_Tp>(__t)),
       static_cast<const volatile void*>(std::forward<_Up>(__u)));
 }


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded2 : true_type { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded2<_Tp, _Up, __void_t<
   decltype(std::declval<_Tp>().operator>=(std::declval<_Up>()))>>
 : false_type { };


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded : __not_overloaded2<_Tp, _Up> { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded<_Tp, _Up, __void_t<
   decltype(operator>=(std::declval<_Tp>(), std::declval<_Up>()))>>
 : false_type { };

      template<typename _Tp, typename _Up>
 using __ptr_cmp = __and_<__not_overloaded<_Tp, _Up>,
       is_convertible<_Tp, const volatile void*>,
       is_convertible<_Up, const volatile void*>>;
    };


  template<>
    struct less_equal<void>
    {
      template <typename _Tp, typename _Up>
 constexpr auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) <= std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) <= std::forward<_Up>(__u))
 {
   return _S_cmp(std::forward<_Tp>(__t), std::forward<_Up>(__u),
   __ptr_cmp<_Tp, _Up>{});
 }

      template<typename _Tp, typename _Up>
 constexpr bool
 operator()(_Tp* __t, _Up* __u) const noexcept
 { return less_equal<common_type_t<_Tp*, _Up*>>{}(__t, __u); }

      typedef __is_transparent is_transparent;

    private:
      template <typename _Tp, typename _Up>
 static constexpr decltype(auto)
 _S_cmp(_Tp&& __t, _Up&& __u, false_type)
 { return std::forward<_Tp>(__t) <= std::forward<_Up>(__u); }

      template <typename _Tp, typename _Up>
 static constexpr bool
 _S_cmp(_Tp&& __t, _Up&& __u, true_type) noexcept
 {
   return less_equal<const volatile void*>{}(
       static_cast<const volatile void*>(std::forward<_Tp>(__t)),
       static_cast<const volatile void*>(std::forward<_Up>(__u)));
 }


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded2 : true_type { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded2<_Tp, _Up, __void_t<
   decltype(std::declval<_Tp>().operator<=(std::declval<_Up>()))>>
 : false_type { };


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded : __not_overloaded2<_Tp, _Up> { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded<_Tp, _Up, __void_t<
   decltype(operator<=(std::declval<_Tp>(), std::declval<_Up>()))>>
 : false_type { };

      template<typename _Tp, typename _Up>
 using __ptr_cmp = __and_<__not_overloaded<_Tp, _Up>,
       is_convertible<_Tp, const volatile void*>,
       is_convertible<_Up, const volatile void*>>;
    };
# 778 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  template<typename _Tp = void>
    struct logical_and;

  template<typename _Tp = void>
    struct logical_or;

  template<typename _Tp = void>
    struct logical_not;


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _Tp>
    struct logical_and : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x && __y; }
    };


  template<typename _Tp>
    struct logical_or : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x || __y; }
    };


  template<typename _Tp>
    struct logical_not : public unary_function<_Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x) const
      { return !__x; }
    };
#pragma GCC diagnostic pop



  template<>
    struct logical_and<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) && std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) && std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) && std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct logical_or<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) || std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) || std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) || std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct logical_not<void>
    {
      template <typename _Tp>
 constexpr
 auto
 operator()(_Tp&& __t) const
 noexcept(noexcept(!std::forward<_Tp>(__t)))
 -> decltype(!std::forward<_Tp>(__t))
 { return !std::forward<_Tp>(__t); }

      typedef __is_transparent is_transparent;
    };




  template<typename _Tp = void>
    struct bit_and;

  template<typename _Tp = void>
    struct bit_or;

  template<typename _Tp = void>
    struct bit_xor;

  template<typename _Tp = void>
    struct bit_not;


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"



  template<typename _Tp>
    struct bit_and : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x & __y; }
    };

  template<typename _Tp>
    struct bit_or : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x | __y; }
    };

  template<typename _Tp>
    struct bit_xor : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x ^ __y; }
    };

  template<typename _Tp>
    struct bit_not : public unary_function<_Tp, _Tp>
    {
    constexpr
      _Tp
      operator()(const _Tp& __x) const
      { return ~__x; }
    };
#pragma GCC diagnostic pop


  template <>
    struct bit_and<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) & std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) & std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) & std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };

  template <>
    struct bit_or<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) | std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) | std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) | std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };

  template <>
    struct bit_xor<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) ^ std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) ^ std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) ^ std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };

  template <>
    struct bit_not<void>
    {
      template <typename _Tp>
 constexpr
 auto
 operator()(_Tp&& __t) const
 noexcept(noexcept(~std::forward<_Tp>(__t)))
 -> decltype(~std::forward<_Tp>(__t))
 { return ~std::forward<_Tp>(__t); }

      typedef __is_transparent is_transparent;
    };


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
# 1020 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  template<typename _Predicate>
    class unary_negate
    : public unary_function<typename _Predicate::argument_type, bool>
    {
    protected:
      _Predicate _M_pred;

    public:
      constexpr
      explicit
      unary_negate(const _Predicate& __x) : _M_pred(__x) { }

      constexpr
      bool
      operator()(const typename _Predicate::argument_type& __x) const
      { return !_M_pred(__x); }
    };


  template<typename _Predicate>
   
    constexpr
    inline unary_negate<_Predicate>
    not1(const _Predicate& __pred)
    { return unary_negate<_Predicate>(__pred); }


  template<typename _Predicate>
    class binary_negate
    : public binary_function<typename _Predicate::first_argument_type,
        typename _Predicate::second_argument_type, bool>
    {
    protected:
      _Predicate _M_pred;

    public:
      constexpr
      explicit
      binary_negate(const _Predicate& __x) : _M_pred(__x) { }

      constexpr
      bool
      operator()(const typename _Predicate::first_argument_type& __x,
   const typename _Predicate::second_argument_type& __y) const
      { return !_M_pred(__x, __y); }
    };


  template<typename _Predicate>
   
    constexpr
    inline binary_negate<_Predicate>
    not2(const _Predicate& __pred)
    { return binary_negate<_Predicate>(__pred); }
# 1101 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  template<typename _Arg, typename _Result>
    class pointer_to_unary_function : public unary_function<_Arg, _Result>
    {
    protected:
      _Result (*_M_ptr)(_Arg);

    public:
      pointer_to_unary_function() { }

      explicit
      pointer_to_unary_function(_Result (*__x)(_Arg))
      : _M_ptr(__x) { }

      _Result
      operator()(_Arg __x) const
      { return _M_ptr(__x); }
    } __attribute__ ((__deprecated__));


  template<typename _Arg, typename _Result>
    __attribute__ ((__deprecated__ ("use '" "std::function" "' instead")))
    inline pointer_to_unary_function<_Arg, _Result>
    ptr_fun(_Result (*__x)(_Arg))
    { return pointer_to_unary_function<_Arg, _Result>(__x); }


  template<typename _Arg1, typename _Arg2, typename _Result>
    class pointer_to_binary_function
    : public binary_function<_Arg1, _Arg2, _Result>
    {
    protected:
      _Result (*_M_ptr)(_Arg1, _Arg2);

    public:
      pointer_to_binary_function() { }

      explicit
      pointer_to_binary_function(_Result (*__x)(_Arg1, _Arg2))
      : _M_ptr(__x) { }

      _Result
      operator()(_Arg1 __x, _Arg2 __y) const
      { return _M_ptr(__x, __y); }
    } __attribute__ ((__deprecated__));


  template<typename _Arg1, typename _Arg2, typename _Result>
    __attribute__ ((__deprecated__ ("use '" "std::function" "' instead")))
    inline pointer_to_binary_function<_Arg1, _Arg2, _Result>
    ptr_fun(_Result (*__x)(_Arg1, _Arg2))
    { return pointer_to_binary_function<_Arg1, _Arg2, _Result>(__x); }


  template<typename _Tp>
    struct _Identity
    : public unary_function<_Tp, _Tp>
    {
      _Tp&
      operator()(_Tp& __x) const
      { return __x; }

      const _Tp&
      operator()(const _Tp& __x) const
      { return __x; }
    };


  template<typename _Tp> struct _Identity<const _Tp> : _Identity<_Tp> { };

  template<typename _Pair>
    struct _Select1st
    : public unary_function<_Pair, typename _Pair::first_type>
    {
      typename _Pair::first_type&
      operator()(_Pair& __x) const
      { return __x.first; }

      const typename _Pair::first_type&
      operator()(const _Pair& __x) const
      { return __x.first; }


      template<typename _Pair2>
        typename _Pair2::first_type&
        operator()(_Pair2& __x) const
        { return __x.first; }

      template<typename _Pair2>
        const typename _Pair2::first_type&
        operator()(const _Pair2& __x) const
        { return __x.first; }

    };

  template<typename _Pair>
    struct _Select2nd
    : public unary_function<_Pair, typename _Pair::second_type>
    {
      typename _Pair::second_type&
      operator()(_Pair& __x) const
      { return __x.second; }

      const typename _Pair::second_type&
      operator()(const _Pair& __x) const
      { return __x.second; }
    };
# 1228 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  template<typename _Ret, typename _Tp>
    class mem_fun_t : public unary_function<_Tp*, _Ret>
    {
    public:
      explicit
      mem_fun_t(_Ret (_Tp::*__pf)())
      : _M_f(__pf) { }

      _Ret
      operator()(_Tp* __p) const
      { return (__p->*_M_f)(); }

    private:
      _Ret (_Tp::*_M_f)();
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp>
    class const_mem_fun_t : public unary_function<const _Tp*, _Ret>
    {
    public:
      explicit
      const_mem_fun_t(_Ret (_Tp::*__pf)() const)
      : _M_f(__pf) { }

      _Ret
      operator()(const _Tp* __p) const
      { return (__p->*_M_f)(); }

    private:
      _Ret (_Tp::*_M_f)() const;
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp>
    class mem_fun_ref_t : public unary_function<_Tp, _Ret>
    {
    public:
      explicit
      mem_fun_ref_t(_Ret (_Tp::*__pf)())
      : _M_f(__pf) { }

      _Ret
      operator()(_Tp& __r) const
      { return (__r.*_M_f)(); }

    private:
      _Ret (_Tp::*_M_f)();
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp>
    class const_mem_fun_ref_t : public unary_function<_Tp, _Ret>
    {
    public:
      explicit
      const_mem_fun_ref_t(_Ret (_Tp::*__pf)() const)
      : _M_f(__pf) { }

      _Ret
      operator()(const _Tp& __r) const
      { return (__r.*_M_f)(); }

    private:
      _Ret (_Tp::*_M_f)() const;
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp, typename _Arg>
    class mem_fun1_t : public binary_function<_Tp*, _Arg, _Ret>
    {
    public:
      explicit
      mem_fun1_t(_Ret (_Tp::*__pf)(_Arg))
      : _M_f(__pf) { }

      _Ret
      operator()(_Tp* __p, _Arg __x) const
      { return (__p->*_M_f)(__x); }

    private:
      _Ret (_Tp::*_M_f)(_Arg);
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp, typename _Arg>
    class const_mem_fun1_t : public binary_function<const _Tp*, _Arg, _Ret>
    {
    public:
      explicit
      const_mem_fun1_t(_Ret (_Tp::*__pf)(_Arg) const)
      : _M_f(__pf) { }

      _Ret
      operator()(const _Tp* __p, _Arg __x) const
      { return (__p->*_M_f)(__x); }

    private:
      _Ret (_Tp::*_M_f)(_Arg) const;
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp, typename _Arg>
    class mem_fun1_ref_t : public binary_function<_Tp, _Arg, _Ret>
    {
    public:
      explicit
      mem_fun1_ref_t(_Ret (_Tp::*__pf)(_Arg))
      : _M_f(__pf) { }

      _Ret
      operator()(_Tp& __r, _Arg __x) const
      { return (__r.*_M_f)(__x); }

    private:
      _Ret (_Tp::*_M_f)(_Arg);
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp, typename _Arg>
    class const_mem_fun1_ref_t : public binary_function<_Tp, _Arg, _Ret>
    {
    public:
      explicit
      const_mem_fun1_ref_t(_Ret (_Tp::*__pf)(_Arg) const)
      : _M_f(__pf) { }

      _Ret
      operator()(const _Tp& __r, _Arg __x) const
      { return (__r.*_M_f)(__x); }

    private:
      _Ret (_Tp::*_M_f)(_Arg) const;
    } __attribute__ ((__deprecated__));



  template<typename _Ret, typename _Tp>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline mem_fun_t<_Ret, _Tp>
    mem_fun(_Ret (_Tp::*__f)())
    { return mem_fun_t<_Ret, _Tp>(__f); }

  template<typename _Ret, typename _Tp>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline const_mem_fun_t<_Ret, _Tp>
    mem_fun(_Ret (_Tp::*__f)() const)
    { return const_mem_fun_t<_Ret, _Tp>(__f); }

  template<typename _Ret, typename _Tp>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline mem_fun_ref_t<_Ret, _Tp>
    mem_fun_ref(_Ret (_Tp::*__f)())
    { return mem_fun_ref_t<_Ret, _Tp>(__f); }

  template<typename _Ret, typename _Tp>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline const_mem_fun_ref_t<_Ret, _Tp>
    mem_fun_ref(_Ret (_Tp::*__f)() const)
    { return const_mem_fun_ref_t<_Ret, _Tp>(__f); }

  template<typename _Ret, typename _Tp, typename _Arg>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline mem_fun1_t<_Ret, _Tp, _Arg>
    mem_fun(_Ret (_Tp::*__f)(_Arg))
    { return mem_fun1_t<_Ret, _Tp, _Arg>(__f); }

  template<typename _Ret, typename _Tp, typename _Arg>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline const_mem_fun1_t<_Ret, _Tp, _Arg>
    mem_fun(_Ret (_Tp::*__f)(_Arg) const)
    { return const_mem_fun1_t<_Ret, _Tp, _Arg>(__f); }

  template<typename _Ret, typename _Tp, typename _Arg>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline mem_fun1_ref_t<_Ret, _Tp, _Arg>
    mem_fun_ref(_Ret (_Tp::*__f)(_Arg))
    { return mem_fun1_ref_t<_Ret, _Tp, _Arg>(__f); }

  template<typename _Ret, typename _Tp, typename _Arg>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline const_mem_fun1_ref_t<_Ret, _Tp, _Arg>
    mem_fun_ref(_Ret (_Tp::*__f)(_Arg) const)
    { return const_mem_fun1_ref_t<_Ret, _Tp, _Arg>(__f); }
#pragma GCC diagnostic pop




  template<typename _Func, typename _SfinaeType, typename = __void_t<>>
    struct __has_is_transparent
    { };

  template<typename _Func, typename _SfinaeType>
    struct __has_is_transparent<_Func, _SfinaeType,
    __void_t<typename _Func::is_transparent>>
    { typedef void type; };

  template<typename _Func, typename _SfinaeType>
    using __has_is_transparent_t
      = typename __has_is_transparent<_Func, _SfinaeType>::type;
# 1437 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3

}


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/binders.h" 1 3
# 60 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/binders.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"

namespace std __attribute__ ((__visibility__ ("default")))
{

# 107 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/binders.h" 3
  template<typename _Operation>
    class binder1st
    : public unary_function<typename _Operation::second_argument_type,
       typename _Operation::result_type>
    {
    protected:
      _Operation op;
      typename _Operation::first_argument_type value;

    public:
      binder1st(const _Operation& __x,
  const typename _Operation::first_argument_type& __y)
      : op(__x), value(__y) { }

      typename _Operation::result_type
      operator()(const typename _Operation::second_argument_type& __x) const
      { return op(value, __x); }



      typename _Operation::result_type
      operator()(typename _Operation::second_argument_type& __x) const
      { return op(value, __x); }
    } __attribute__ ((__deprecated__ ("use '" "std::bind" "' instead")));


  template<typename _Operation, typename _Tp>
    __attribute__ ((__deprecated__ ("use '" "std::bind" "' instead")))
    inline binder1st<_Operation>
    bind1st(const _Operation& __fn, const _Tp& __x)
    {
      typedef typename _Operation::first_argument_type _Arg1_type;
      return binder1st<_Operation>(__fn, _Arg1_type(__x));
    }


  template<typename _Operation>
    class binder2nd
    : public unary_function<typename _Operation::first_argument_type,
       typename _Operation::result_type>
    {
    protected:
      _Operation op;
      typename _Operation::second_argument_type value;

    public:
      binder2nd(const _Operation& __x,
  const typename _Operation::second_argument_type& __y)
      : op(__x), value(__y) { }

      typename _Operation::result_type
      operator()(const typename _Operation::first_argument_type& __x) const
      { return op(__x, value); }



      typename _Operation::result_type
      operator()(typename _Operation::first_argument_type& __x) const
      { return op(__x, value); }
    } __attribute__ ((__deprecated__ ("use '" "std::bind" "' instead")));


  template<typename _Operation, typename _Tp>
    __attribute__ ((__deprecated__ ("use '" "std::bind" "' instead")))
    inline binder2nd<_Operation>
    bind2nd(const _Operation& __fn, const _Tp& __x)
    {
      typedef typename _Operation::second_argument_type _Arg2_type;
      return binder2nd<_Operation>(__fn, _Arg2_type(__x));
    }



}

#pragma GCC diagnostic pop
# 1442 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 2 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/hash_bytes.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/hash_bytes.h" 3
namespace std
{







  size_t
  _Hash_bytes(const void* __ptr, size_t __len, size_t __seed);





  size_t
  _Fnv_hash_bytes(const void* __ptr, size_t __len, size_t __seed);


}
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 3
  template<typename _Result, typename _Arg>
    struct __hash_base
    {

      typedef _Result result_type ;
      typedef _Arg argument_type ;

    };



  template<typename _Tp> struct __hash_empty_base { };



  template<typename _Tp>
    struct hash;

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++14-extensions"
  template<typename _Tp, typename = void>
    constexpr bool __is_hash_enabled_for = false;

  template<typename _Tp>
    constexpr bool
    __is_hash_enabled_for<_Tp,
     __void_t<decltype(hash<_Tp>()(declval<_Tp>()))>>
      = true;
#pragma GCC diagnostic pop


  template<typename _Tp>
    struct __hash_not_enabled
    {
      __hash_not_enabled(__hash_not_enabled&&) = delete;
      ~__hash_not_enabled() = delete;
    };


  template<typename _Tp, bool = true>
    struct __hash_enum : public __hash_base<size_t, _Tp>
    {
      size_t
      operator()(_Tp __val) const noexcept
      {
       using __type = typename underlying_type<_Tp>::type;
       return hash<__type>{}(static_cast<__type>(__val));
      }
    };


  template<typename _Tp>
    struct hash
    : __conditional_t<__is_enum(_Tp), __hash_enum<_Tp>, __hash_not_enabled<_Tp>>
    { };


  template<typename _Tp>
    struct hash<_Tp*> : public __hash_base<size_t, _Tp*>
    {
      size_t
      operator()(_Tp* __p) const noexcept
      { return reinterpret_cast<size_t>(__p); }
    };
# 128 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 3
  template<> struct hash<bool> : public __hash_base<size_t, bool> { size_t operator()(bool __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<char> : public __hash_base<size_t, char> { size_t operator()(char __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<signed char> : public __hash_base<size_t, signed char> { size_t operator()(signed char __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<unsigned char> : public __hash_base<size_t, unsigned char> { size_t operator()(unsigned char __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<wchar_t> : public __hash_base<size_t, wchar_t> { size_t operator()(wchar_t __val) const noexcept { return static_cast<size_t>(__val); } };







  template<> struct hash<char16_t> : public __hash_base<size_t, char16_t> { size_t operator()(char16_t __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<char32_t> : public __hash_base<size_t, char32_t> { size_t operator()(char32_t __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<short> : public __hash_base<size_t, short> { size_t operator()(short __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<int> : public __hash_base<size_t, int> { size_t operator()(int __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<long> : public __hash_base<size_t, long> { size_t operator()(long __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<long long> : public __hash_base<size_t, long long> { size_t operator()(long long __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<unsigned short> : public __hash_base<size_t, unsigned short> { size_t operator()(unsigned short __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<unsigned int> : public __hash_base<size_t, unsigned int> { size_t operator()(unsigned int __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<unsigned long> : public __hash_base<size_t, unsigned long> { size_t operator()(unsigned long __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<unsigned long long> : public __hash_base<size_t, unsigned long long> { size_t operator()(unsigned long long __val) const noexcept { return static_cast<size_t>(__val); } };
# 204 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 3
  struct _Hash_impl
  {
    static size_t
    hash(const void* __ptr, size_t __clength,
  size_t __seed = static_cast<size_t>(0xc70f6907UL))
    { return _Hash_bytes(__ptr, __clength, __seed); }

    template<typename _Tp>
      static size_t
      hash(const _Tp& __val)
      { return hash(&__val, sizeof(__val)); }

    template<typename _Tp>
      static size_t
      __hash_combine(const _Tp& __val, size_t __hash)
      { return hash(&__val, sizeof(__val), __hash); }
  };


  struct _Fnv_hash_impl
  {
    static size_t
    hash(const void* __ptr, size_t __clength,
  size_t __seed = static_cast<size_t>(2166136261UL))
    { return _Fnv_hash_bytes(__ptr, __clength, __seed); }

    template<typename _Tp>
      static size_t
      hash(const _Tp& __val)
      { return hash(&__val, sizeof(__val)); }

    template<typename _Tp>
      static size_t
      __hash_combine(const _Tp& __val, size_t __hash)
      { return hash(&__val, sizeof(__val), __hash); }
  };


  template<>
    struct hash<float> : public __hash_base<size_t, float>
    {
      size_t
      operator()(float __val) const noexcept
      {

 return __val != 0.0f ? std::_Hash_impl::hash(__val) : 0;
      }
    };


  template<>
    struct hash<double> : public __hash_base<size_t, double>
    {
      size_t
      operator()(double __val) const noexcept
      {

 return __val != 0.0 ? std::_Hash_impl::hash(__val) : 0;
      }
    };


  template<>
    struct hash<long double>
    : public __hash_base<size_t, long double>
    {
      __attribute__ ((__pure__)) size_t
      operator()(long double __val) const noexcept;
    };
# 297 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 3
  template<typename _Hash>
    struct __is_fast_hash : public std::true_type
    { };

  template<>
    struct __is_fast_hash<hash<long double>> : public std::false_type
    { };


}
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 2 3







namespace std __attribute__ ((__visibility__ ("default")))
{








 
# 57 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
  template<typename> class auto_ptr;
#pragma GCC diagnostic pop







  template<typename _Tp>
    struct default_delete
    {

      constexpr default_delete() noexcept = default;






      template<typename _Up,
        typename = _Require<is_convertible<_Up*, _Tp*>>>

        default_delete(const default_delete<_Up>&) noexcept { }


     
      void
      operator()(_Tp* __ptr) const
      {
 static_assert(!is_void<_Tp>::value,
        "can't delete pointer to incomplete type");
 static_assert(sizeof(_Tp)>0,
        "can't delete pointer to incomplete type");
 delete __ptr;
      }
    };
# 105 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
  template<typename _Tp>
    struct default_delete<_Tp[]>
    {
    public:

      constexpr default_delete() noexcept = default;
# 121 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
      template<typename _Up,
        typename = _Require<is_convertible<_Up(*)[], _Tp(*)[]>>>

        default_delete(const default_delete<_Up[]>&) noexcept { }


      template<typename _Up>

 typename enable_if<is_convertible<_Up(*)[], _Tp(*)[]>::value>::type
 operator()(_Up* __ptr) const
 {
   static_assert(sizeof(_Tp)>0,
   "can't delete pointer to incomplete type");
   delete [] __ptr;
 }
    };




  template <typename _Tp, typename _Dp>
    class __uniq_ptr_impl
    {
      template <typename _Up, typename _Ep, typename = void>
 struct _Ptr
 {
   using type = _Up*;
 };

      template <typename _Up, typename _Ep>
 struct
 _Ptr<_Up, _Ep, __void_t<typename remove_reference<_Ep>::type::pointer>>
 {
   using type = typename remove_reference<_Ep>::type::pointer;
 };

    public:
      using _DeleterConstraint = enable_if<
        __and_<__not_<is_pointer<_Dp>>,
        is_default_constructible<_Dp>>::value>;

      using pointer = typename _Ptr<_Tp, _Dp>::type;

      static_assert( !is_rvalue_reference<_Dp>::value,
       "unique_ptr's deleter type must be a function object type"
       " or an lvalue reference type" );

      __uniq_ptr_impl() = default;
     
      __uniq_ptr_impl(pointer __p) : _M_t() { _M_ptr() = __p; }

      template<typename _Del>

 __uniq_ptr_impl(pointer __p, _Del&& __d)
 : _M_t(__p, std::forward<_Del>(__d)) { }

     
      __uniq_ptr_impl(__uniq_ptr_impl&& __u) noexcept
      : _M_t(std::move(__u._M_t))
      { __u._M_ptr() = nullptr; }

     
      __uniq_ptr_impl& operator=(__uniq_ptr_impl&& __u) noexcept
      {
 reset(__u.release());
 _M_deleter() = std::forward<_Dp>(__u._M_deleter());
 return *this;
      }

     
      pointer& _M_ptr() noexcept { return std::get<0>(_M_t); }
     
      pointer _M_ptr() const noexcept { return std::get<0>(_M_t); }
     
      _Dp& _M_deleter() noexcept { return std::get<1>(_M_t); }
     
      const _Dp& _M_deleter() const noexcept { return std::get<1>(_M_t); }

     
      void reset(pointer __p) noexcept
      {
 const pointer __old_p = _M_ptr();
 _M_ptr() = __p;
 if (__old_p)
   _M_deleter()(__old_p);
      }

     
      pointer release() noexcept
      {
 pointer __p = _M_ptr();
 _M_ptr() = nullptr;
 return __p;
      }

     
      void
      swap(__uniq_ptr_impl& __rhs) noexcept
      {
 using std::swap;
 swap(this->_M_ptr(), __rhs._M_ptr());
 swap(this->_M_deleter(), __rhs._M_deleter());
      }

    private:
      tuple<pointer, _Dp> _M_t;
    };


  template <typename _Tp, typename _Dp,
     bool = is_move_constructible<_Dp>::value,
     bool = is_move_assignable<_Dp>::value>
    struct __uniq_ptr_data : __uniq_ptr_impl<_Tp, _Dp>
    {
      using __uniq_ptr_impl<_Tp, _Dp>::__uniq_ptr_impl;
      __uniq_ptr_data(__uniq_ptr_data&&) = default;
      __uniq_ptr_data& operator=(__uniq_ptr_data&&) = default;
    };

  template <typename _Tp, typename _Dp>
    struct __uniq_ptr_data<_Tp, _Dp, true, false> : __uniq_ptr_impl<_Tp, _Dp>
    {
      using __uniq_ptr_impl<_Tp, _Dp>::__uniq_ptr_impl;
      __uniq_ptr_data(__uniq_ptr_data&&) = default;
      __uniq_ptr_data& operator=(__uniq_ptr_data&&) = delete;
    };

  template <typename _Tp, typename _Dp>
    struct __uniq_ptr_data<_Tp, _Dp, false, true> : __uniq_ptr_impl<_Tp, _Dp>
    {
      using __uniq_ptr_impl<_Tp, _Dp>::__uniq_ptr_impl;
      __uniq_ptr_data(__uniq_ptr_data&&) = delete;
      __uniq_ptr_data& operator=(__uniq_ptr_data&&) = default;
    };

  template <typename _Tp, typename _Dp>
    struct __uniq_ptr_data<_Tp, _Dp, false, false> : __uniq_ptr_impl<_Tp, _Dp>
    {
      using __uniq_ptr_impl<_Tp, _Dp>::__uniq_ptr_impl;
      __uniq_ptr_data(__uniq_ptr_data&&) = delete;
      __uniq_ptr_data& operator=(__uniq_ptr_data&&) = delete;
    };







  template <typename _Tp, typename _Dp = default_delete<_Tp>>
    class unique_ptr
    {
      template <typename _Up>
 using _DeleterConstraint =
   typename __uniq_ptr_impl<_Tp, _Up>::_DeleterConstraint::type;

      __uniq_ptr_data<_Tp, _Dp> _M_t;

    public:
      using pointer = typename __uniq_ptr_impl<_Tp, _Dp>::pointer;
      using element_type = _Tp;
      using deleter_type = _Dp;

    private:


      template<typename _Up, typename _Ep>
 using __safe_conversion_up = __and_<
   is_convertible<typename unique_ptr<_Up, _Ep>::pointer, pointer>,
   __not_<is_array<_Up>>
        >;

    public:



      template<typename _Del = _Dp, typename = _DeleterConstraint<_Del>>
 constexpr unique_ptr() noexcept
 : _M_t()
 { }







      template<typename _Del = _Dp, typename = _DeleterConstraint<_Del>>

 explicit
 unique_ptr(pointer __p) noexcept
 : _M_t(__p)
        { }
# 322 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
      template<typename _Del = deleter_type,
        typename = _Require<is_copy_constructible<_Del>>>

 unique_ptr(pointer __p, const deleter_type& __d) noexcept
 : _M_t(__p, __d) { }
# 335 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
      template<typename _Del = deleter_type,
        typename = _Require<is_move_constructible<_Del>>>

 unique_ptr(pointer __p,
     __enable_if_t<!is_lvalue_reference<_Del>::value,
     _Del&&> __d) noexcept
 : _M_t(__p, std::move(__d))
 { }

      template<typename _Del = deleter_type,
        typename _DelUnref = typename remove_reference<_Del>::type>

 unique_ptr(pointer,
     __enable_if_t<is_lvalue_reference<_Del>::value,
     _DelUnref&&>) = delete;


      template<typename _Del = _Dp, typename = _DeleterConstraint<_Del>>
 constexpr unique_ptr(nullptr_t) noexcept
 : _M_t()
 { }




      unique_ptr(unique_ptr&&) = default;







      template<typename _Up, typename _Ep, typename = _Require<
               __safe_conversion_up<_Up, _Ep>,
        __conditional_t<is_reference<_Dp>::value,
          is_same<_Ep, _Dp>,
          is_convertible<_Ep, _Dp>>>>

 unique_ptr(unique_ptr<_Up, _Ep>&& __u) noexcept
 : _M_t(__u.release(), std::forward<_Ep>(__u.get_deleter()))
 { }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"

      template<typename _Up,
        typename = _Require<is_convertible<_Up*, pointer>,
       is_same<_Dp, default_delete<_Tp>>>>
 unique_ptr(auto_ptr<_Up>&& __u) noexcept;
#pragma GCC diagnostic pop






      ~unique_ptr() noexcept
      {
 static_assert(__is_invocable<deleter_type&, pointer>::value,
        "unique_ptr's deleter must be invocable with a pointer");
 auto& __ptr = _M_t._M_ptr();
 if (__ptr != nullptr)
   get_deleter()(std::move(__ptr));
 __ptr = pointer();
      }







      unique_ptr& operator=(unique_ptr&&) = default;
# 418 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
      template<typename _Up, typename _Ep>

        typename enable_if< __and_<
          __safe_conversion_up<_Up, _Ep>,
          is_assignable<deleter_type&, _Ep&&>
          >::value,
          unique_ptr&>::type
 operator=(unique_ptr<_Up, _Ep>&& __u) noexcept
 {
   reset(__u.release());
   get_deleter() = std::forward<_Ep>(__u.get_deleter());
   return *this;
 }


     
      unique_ptr&
      operator=(nullptr_t) noexcept
      {
 reset();
 return *this;
      }




     
      typename add_lvalue_reference<element_type>::type
      operator*() const noexcept(noexcept(*std::declval<pointer>()))
      {
 do { if (__builtin_expect(!bool(get() != pointer()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h", 448, __PRETTY_FUNCTION__, "get() != pointer()"); } while (false);
 return *get();
      }


     
      pointer
      operator->() const noexcept
      {
 ;
 return get();
      }


     
      pointer
      get() const noexcept
      { return _M_t._M_ptr(); }


     
      deleter_type&
      get_deleter() noexcept
      { return _M_t._M_deleter(); }


     
      const deleter_type&
      get_deleter() const noexcept
      { return _M_t._M_deleter(); }


     
      explicit operator bool() const noexcept
      { return get() == pointer() ? false : true; }




     
      pointer
      release() noexcept
      { return _M_t.release(); }







     
      void
      reset(pointer __p = pointer()) noexcept
      {
 static_assert(__is_invocable<deleter_type&, pointer>::value,
        "unique_ptr's deleter must be invocable with a pointer");
 _M_t.reset(std::move(__p));
      }


     
      void
      swap(unique_ptr& __u) noexcept
      {
 static_assert(__is_swappable<_Dp>::value, "deleter must be swappable");
 _M_t.swap(__u._M_t);
      }


      unique_ptr(const unique_ptr&) = delete;
      unique_ptr& operator=(const unique_ptr&) = delete;

    private:






  };
# 537 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
  template<typename _Tp, typename _Dp>
    class unique_ptr<_Tp[], _Dp>
    {
      template <typename _Up>
      using _DeleterConstraint =
 typename __uniq_ptr_impl<_Tp, _Up>::_DeleterConstraint::type;

      __uniq_ptr_data<_Tp, _Dp> _M_t;


      template<typename _Up>
 using __is_derived_Tp
   = __and_< is_base_of<_Tp, _Up>,
      __not_<is_same<__remove_cv_t<_Tp>, __remove_cv_t<_Up>>> >;

    public:
      using pointer = typename __uniq_ptr_impl<_Tp, _Dp>::pointer;
      using element_type = _Tp;
      using deleter_type = _Dp;



      template<typename _Up, typename _Ep,
               typename _UPtr = unique_ptr<_Up, _Ep>,
        typename _UP_pointer = typename _UPtr::pointer,
        typename _UP_element_type = typename _UPtr::element_type>
 using __safe_conversion_up = __and_<
          is_array<_Up>,
          is_same<pointer, element_type*>,
          is_same<_UP_pointer, _UP_element_type*>,
          is_convertible<_UP_element_type(*)[], element_type(*)[]>
        >;


      template<typename _Up>
        using __safe_conversion_raw = __and_<
          __or_<__or_<is_same<_Up, pointer>,
                      is_same<_Up, nullptr_t>>,
                __and_<is_pointer<_Up>,
                       is_same<pointer, element_type*>,
                       is_convertible<
                         typename remove_pointer<_Up>::type(*)[],
                         element_type(*)[]>
                >
          >
        >;




      template<typename _Del = _Dp, typename = _DeleterConstraint<_Del>>
 constexpr unique_ptr() noexcept
 : _M_t()
 { }
# 599 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
      template<typename _Up,
        typename _Vp = _Dp,
        typename = _DeleterConstraint<_Vp>,
        typename = typename enable_if<
                 __safe_conversion_raw<_Up>::value, bool>::type>

 explicit
 unique_ptr(_Up __p) noexcept
 : _M_t(__p)
        { }
# 618 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
      template<typename _Up, typename _Del = deleter_type,
        typename = _Require<__safe_conversion_raw<_Up>,
       is_copy_constructible<_Del>>>

 unique_ptr(_Up __p, const deleter_type& __d) noexcept
 : _M_t(__p, __d) { }
# 633 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
      template<typename _Up, typename _Del = deleter_type,
        typename = _Require<__safe_conversion_raw<_Up>,
       is_move_constructible<_Del>>>

 unique_ptr(_Up __p,
     __enable_if_t<!is_lvalue_reference<_Del>::value,
     _Del&&> __d) noexcept
 : _M_t(std::move(__p), std::move(__d))
 { }

      template<typename _Up, typename _Del = deleter_type,
        typename _DelUnref = typename remove_reference<_Del>::type,
        typename = _Require<__safe_conversion_raw<_Up>>>
 unique_ptr(_Up,
     __enable_if_t<is_lvalue_reference<_Del>::value,
     _DelUnref&&>) = delete;


      unique_ptr(unique_ptr&&) = default;


      template<typename _Del = _Dp, typename = _DeleterConstraint<_Del>>
 constexpr unique_ptr(nullptr_t) noexcept
 : _M_t()
        { }

      template<typename _Up, typename _Ep, typename = _Require<
        __safe_conversion_up<_Up, _Ep>,
        __conditional_t<is_reference<_Dp>::value,
          is_same<_Ep, _Dp>,
          is_convertible<_Ep, _Dp>>>>

 unique_ptr(unique_ptr<_Up, _Ep>&& __u) noexcept
 : _M_t(__u.release(), std::forward<_Ep>(__u.get_deleter()))
 { }





      ~unique_ptr()
      {
 auto& __ptr = _M_t._M_ptr();
 if (__ptr != nullptr)
   get_deleter()(__ptr);
 __ptr = pointer();
      }







      unique_ptr&
      operator=(unique_ptr&&) = default;
# 697 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
      template<typename _Up, typename _Ep>

 typename
 enable_if<__and_<__safe_conversion_up<_Up, _Ep>,
                         is_assignable<deleter_type&, _Ep&&>
                  >::value,
                  unique_ptr&>::type
 operator=(unique_ptr<_Up, _Ep>&& __u) noexcept
 {
   reset(__u.release());
   get_deleter() = std::forward<_Ep>(__u.get_deleter());
   return *this;
 }


     
      unique_ptr&
      operator=(nullptr_t) noexcept
      {
 reset();
 return *this;
      }




     
      typename std::add_lvalue_reference<element_type>::type
      operator[](size_t __i) const
      {
 do { if (__builtin_expect(!bool(get() != pointer()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h", 727, __PRETTY_FUNCTION__, "get() != pointer()"); } while (false);
 return get()[__i];
      }


     
      pointer
      get() const noexcept
      { return _M_t._M_ptr(); }


     
      deleter_type&
      get_deleter() noexcept
      { return _M_t._M_deleter(); }


     
      const deleter_type&
      get_deleter() const noexcept
      { return _M_t._M_deleter(); }


     
      explicit operator bool() const noexcept
      { return get() == pointer() ? false : true; }




     
      pointer
      release() noexcept
      { return _M_t.release(); }







      template <typename _Up,
                typename = _Require<
                  __or_<is_same<_Up, pointer>,
                        __and_<is_same<pointer, element_type*>,
                               is_pointer<_Up>,
                               is_convertible<
                                 typename remove_pointer<_Up>::type(*)[],
                                 element_type(*)[]
                               >
                        >
                  >
               >>
     
      void
      reset(_Up __p) noexcept
      { _M_t.reset(std::move(__p)); }

     
      void reset(nullptr_t = nullptr) noexcept
      { reset(pointer()); }


     
      void
      swap(unique_ptr& __u) noexcept
      {
 static_assert(__is_swappable<_Dp>::value, "deleter must be swappable");
 _M_t.swap(__u._M_t);
      }


      unique_ptr(const unique_ptr&) = delete;
      unique_ptr& operator=(const unique_ptr&) = delete;

    private:




    };





  template<typename _Tp, typename _Dp>
    inline





    void

    swap(unique_ptr<_Tp, _Dp>& __x,
  unique_ptr<_Tp, _Dp>& __y) noexcept
    { __x.swap(__y); }
# 834 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
  template<typename _Tp, typename _Dp,
    typename _Up, typename _Ep>
    [[__nodiscard__]]
    inline bool
    operator==(const unique_ptr<_Tp, _Dp>& __x,
        const unique_ptr<_Up, _Ep>& __y)
    { return __x.get() == __y.get(); }


  template<typename _Tp, typename _Dp>
    [[__nodiscard__]]
    inline bool
    operator==(const unique_ptr<_Tp, _Dp>& __x, nullptr_t) noexcept
    { return !__x; }



  template<typename _Tp, typename _Dp>
    [[__nodiscard__]]
    inline bool
    operator==(nullptr_t, const unique_ptr<_Tp, _Dp>& __x) noexcept
    { return !__x; }


  template<typename _Tp, typename _Dp,
    typename _Up, typename _Ep>
    [[__nodiscard__]]
    inline bool
    operator!=(const unique_ptr<_Tp, _Dp>& __x,
        const unique_ptr<_Up, _Ep>& __y)
    { return __x.get() != __y.get(); }


  template<typename _Tp, typename _Dp>
    [[__nodiscard__]]
    inline bool
    operator!=(const unique_ptr<_Tp, _Dp>& __x, nullptr_t) noexcept
    { return (bool)__x; }


  template<typename _Tp, typename _Dp>
    [[__nodiscard__]]
    inline bool
    operator!=(nullptr_t, const unique_ptr<_Tp, _Dp>& __x) noexcept
    { return (bool)__x; }



  template<typename _Tp, typename _Dp,
    typename _Up, typename _Ep>
    [[__nodiscard__]]
    inline bool
    operator<(const unique_ptr<_Tp, _Dp>& __x,
       const unique_ptr<_Up, _Ep>& __y)
    {
      typedef typename
 std::common_type<typename unique_ptr<_Tp, _Dp>::pointer,
                  typename unique_ptr<_Up, _Ep>::pointer>::type _CT;
      return std::less<_CT>()(__x.get(), __y.get());
    }


  template<typename _Tp, typename _Dp>
    [[__nodiscard__]]
    inline bool
    operator<(const unique_ptr<_Tp, _Dp>& __x, nullptr_t)
    {
      return std::less<typename unique_ptr<_Tp, _Dp>::pointer>()(__x.get(),
         nullptr);
    }


  template<typename _Tp, typename _Dp>
    [[__nodiscard__]]
    inline bool
    operator<(nullptr_t, const unique_ptr<_Tp, _Dp>& __x)
    {
      return std::less<typename unique_ptr<_Tp, _Dp>::pointer>()(nullptr,
         __x.get());
    }


  template<typename _Tp, typename _Dp,
    typename _Up, typename _Ep>
    [[__nodiscard__]]
    inline bool
    operator<=(const unique_ptr<_Tp, _Dp>& __x,
        const unique_ptr<_Up, _Ep>& __y)
    { return !(__y < __x); }


  template<typename _Tp, typename _Dp>
    [[__nodiscard__]]
    inline bool
    operator<=(const unique_ptr<_Tp, _Dp>& __x, nullptr_t)
    { return !(nullptr < __x); }


  template<typename _Tp, typename _Dp>
    [[__nodiscard__]]
    inline bool
    operator<=(nullptr_t, const unique_ptr<_Tp, _Dp>& __x)
    { return !(__x < nullptr); }


  template<typename _Tp, typename _Dp,
    typename _Up, typename _Ep>
    [[__nodiscard__]]
    inline bool
    operator>(const unique_ptr<_Tp, _Dp>& __x,
       const unique_ptr<_Up, _Ep>& __y)
    { return (__y < __x); }


  template<typename _Tp, typename _Dp>
    [[__nodiscard__]]
    inline bool
    operator>(const unique_ptr<_Tp, _Dp>& __x, nullptr_t)
    {
      return std::less<typename unique_ptr<_Tp, _Dp>::pointer>()(nullptr,
         __x.get());
    }


  template<typename _Tp, typename _Dp>
    [[__nodiscard__]]
    inline bool
    operator>(nullptr_t, const unique_ptr<_Tp, _Dp>& __x)
    {
      return std::less<typename unique_ptr<_Tp, _Dp>::pointer>()(__x.get(),
         nullptr);
    }


  template<typename _Tp, typename _Dp,
    typename _Up, typename _Ep>
    [[__nodiscard__]]
    inline bool
    operator>=(const unique_ptr<_Tp, _Dp>& __x,
        const unique_ptr<_Up, _Ep>& __y)
    { return !(__x < __y); }


  template<typename _Tp, typename _Dp>
    [[__nodiscard__]]
    inline bool
    operator>=(const unique_ptr<_Tp, _Dp>& __x, nullptr_t)
    { return !(__x < nullptr); }


  template<typename _Tp, typename _Dp>
    [[__nodiscard__]] inline bool
    operator>=(nullptr_t, const unique_ptr<_Tp, _Dp>& __x)
    { return !(nullptr < __x); }
# 1015 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
  template<typename _Up, typename _Ptr = typename _Up::pointer>
    struct __uniq_ptr_hash
    : public __hash_base<size_t, _Up>

    , private __hash_empty_base<_Ptr>

    {
      size_t
      operator()(const _Up& __u) const
      noexcept(noexcept(std::declval<hash<_Ptr>>()(std::declval<_Ptr>())))
      { return hash<_Ptr>()(__u.get()); }
    };

  template<typename _Up>
    using __uniq_ptr_hash_base
      = __conditional_t<__is_hash_enabled_for<typename _Up::pointer>,
        __uniq_ptr_hash<_Up>,
        __hash_not_enabled<typename _Up::pointer>>;



  template<typename _Tp, typename _Dp>
    struct hash<unique_ptr<_Tp, _Dp>>
    : public __uniq_ptr_hash_base<unique_ptr<_Tp, _Dp>>
    { };



namespace __detail
{
  template<typename _Tp>
    struct _MakeUniq
    { typedef unique_ptr<_Tp> __single_object; };

  template<typename _Tp>
    struct _MakeUniq<_Tp[]>
    { typedef unique_ptr<_Tp[]> __array; };

  template<typename _Tp, size_t _Bound>
    struct _MakeUniq<_Tp[_Bound]>
    { struct __invalid_type { }; };

  template<typename _Tp>
    using __unique_ptr_t = typename _MakeUniq<_Tp>::__single_object;
  template<typename _Tp>
    using __unique_ptr_array_t = typename _MakeUniq<_Tp>::__array;
  template<typename _Tp>
    using __invalid_make_unique_t = typename _MakeUniq<_Tp>::__invalid_type;
}
# 1073 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
  template<typename _Tp, typename... _Args>
   
    inline __detail::__unique_ptr_t<_Tp>
    make_unique(_Args&&... __args)
    { return unique_ptr<_Tp>(new _Tp(std::forward<_Args>(__args)...)); }
# 1088 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
  template<typename _Tp>
   
    inline __detail::__unique_ptr_array_t<_Tp>
    make_unique(size_t __num)
    { return unique_ptr<_Tp>(new remove_extent_t<_Tp>[__num]()); }






  template<typename _Tp, typename... _Args>
    __detail::__invalid_make_unique_t<_Tp>
    make_unique(_Args&&...) = delete;
# 1159 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3
  template<typename _Tp>
    constexpr bool __is_unique_ptr = false;
  template<typename _Tp, typename _Del>
    constexpr bool __is_unique_ptr<unique_ptr<_Tp, _Del>> = true;
# 1181 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/unique_ptr.h" 3

}
# 81 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 1 3
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iosfwd" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iosfwd" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/requires_hosted.h" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iosfwd" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stringfwd.h" 1 3
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stringfwd.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{








  template<class _CharT>
    struct char_traits;

  template<> struct char_traits<char>;

  template<> struct char_traits<wchar_t>;






  template<> struct char_traits<char16_t>;
  template<> struct char_traits<char32_t>;


namespace __cxx11 {

  template<typename _CharT, typename _Traits = char_traits<_CharT>,
           typename _Alloc = allocator<_CharT> >
    class basic_string;

}


  typedef basic_string<char> string;


  typedef basic_string<wchar_t> wstring;
# 91 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stringfwd.h" 3
  typedef basic_string<char16_t> u16string;


  typedef basic_string<char32_t> u32string;





}
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iosfwd" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 1 3
# 49 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 3
# 1 "/usr/include/wchar.h" 1 3 4
# 39 "/usr/include/wchar.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/usr/include/wchar.h" 2 3 4
# 51 "/usr/include/wchar.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 359 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef unsigned int wint_t;
# 52 "/usr/include/wchar.h" 2 3 4
# 104 "/usr/include/wchar.h" 3 4


typedef __mbstate_t mbstate_t;






# 132 "/usr/include/wchar.h" 3 4
extern "C" {




struct tm;









extern wchar_t *wcscpy (wchar_t *__restrict __dest,
   const wchar_t *__restrict __src) throw ();

extern wchar_t *wcsncpy (wchar_t *__restrict __dest,
    const wchar_t *__restrict __src, size_t __n)
     throw ();


extern wchar_t *wcscat (wchar_t *__restrict __dest,
   const wchar_t *__restrict __src) throw ();

extern wchar_t *wcsncat (wchar_t *__restrict __dest,
    const wchar_t *__restrict __src, size_t __n)
     throw ();


extern int wcscmp (const wchar_t *__s1, const wchar_t *__s2)
     throw () __attribute__ ((__pure__));

extern int wcsncmp (const wchar_t *__s1, const wchar_t *__s2, size_t __n)
     throw () __attribute__ ((__pure__));




extern int wcscasecmp (const wchar_t *__s1, const wchar_t *__s2) throw ();


extern int wcsncasecmp (const wchar_t *__s1, const wchar_t *__s2,
   size_t __n) throw ();



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
# 181 "/usr/include/wchar.h" 2 3 4

extern int wcscasecmp_l (const wchar_t *__s1, const wchar_t *__s2,
    __locale_t __loc) throw ();

extern int wcsncasecmp_l (const wchar_t *__s1, const wchar_t *__s2,
     size_t __n, __locale_t __loc) throw ();





extern int wcscoll (const wchar_t *__s1, const wchar_t *__s2) throw ();



extern size_t wcsxfrm (wchar_t *__restrict __s1,
         const wchar_t *__restrict __s2, size_t __n) throw ();








extern int wcscoll_l (const wchar_t *__s1, const wchar_t *__s2,
        __locale_t __loc) throw ();




extern size_t wcsxfrm_l (wchar_t *__s1, const wchar_t *__s2,
    size_t __n, __locale_t __loc) throw ();


extern wchar_t *wcsdup (const wchar_t *__s) throw () __attribute__ ((__malloc__));





extern "C++" wchar_t *wcschr (wchar_t *__wcs, wchar_t __wc)
     throw () __asm ("wcschr") __attribute__ ((__pure__));
extern "C++" const wchar_t *wcschr (const wchar_t *__wcs, wchar_t __wc)
     throw () __asm ("wcschr") __attribute__ ((__pure__));






extern "C++" wchar_t *wcsrchr (wchar_t *__wcs, wchar_t __wc)
     throw () __asm ("wcsrchr") __attribute__ ((__pure__));
extern "C++" const wchar_t *wcsrchr (const wchar_t *__wcs, wchar_t __wc)
     throw () __asm ("wcsrchr") __attribute__ ((__pure__));









extern wchar_t *wcschrnul (const wchar_t *__s, wchar_t __wc)
     throw () __attribute__ ((__pure__));





extern size_t wcscspn (const wchar_t *__wcs, const wchar_t *__reject)
     throw () __attribute__ ((__pure__));


extern size_t wcsspn (const wchar_t *__wcs, const wchar_t *__accept)
     throw () __attribute__ ((__pure__));


extern "C++" wchar_t *wcspbrk (wchar_t *__wcs, const wchar_t *__accept)
     throw () __asm ("wcspbrk") __attribute__ ((__pure__));
extern "C++" const wchar_t *wcspbrk (const wchar_t *__wcs,
         const wchar_t *__accept)
     throw () __asm ("wcspbrk") __attribute__ ((__pure__));






extern "C++" wchar_t *wcsstr (wchar_t *__haystack, const wchar_t *__needle)
     throw () __asm ("wcsstr") __attribute__ ((__pure__));
extern "C++" const wchar_t *wcsstr (const wchar_t *__haystack,
        const wchar_t *__needle)
     throw () __asm ("wcsstr") __attribute__ ((__pure__));






extern wchar_t *wcstok (wchar_t *__restrict __s,
   const wchar_t *__restrict __delim,
   wchar_t **__restrict __ptr) throw ();


extern size_t wcslen (const wchar_t *__s) throw () __attribute__ ((__pure__));





extern "C++" wchar_t *wcswcs (wchar_t *__haystack, const wchar_t *__needle)
     throw () __asm ("wcswcs") __attribute__ ((__pure__));
extern "C++" const wchar_t *wcswcs (const wchar_t *__haystack,
        const wchar_t *__needle)
     throw () __asm ("wcswcs") __attribute__ ((__pure__));
# 306 "/usr/include/wchar.h" 3 4
extern size_t wcsnlen (const wchar_t *__s, size_t __maxlen)
     throw () __attribute__ ((__pure__));






extern "C++" wchar_t *wmemchr (wchar_t *__s, wchar_t __c, size_t __n)
     throw () __asm ("wmemchr") __attribute__ ((__pure__));
extern "C++" const wchar_t *wmemchr (const wchar_t *__s, wchar_t __c,
         size_t __n)
     throw () __asm ("wmemchr") __attribute__ ((__pure__));






extern int wmemcmp (const wchar_t *__s1, const wchar_t *__s2, size_t __n)
     throw () __attribute__ ((__pure__));


extern wchar_t *wmemcpy (wchar_t *__restrict __s1,
    const wchar_t *__restrict __s2, size_t __n) throw ();



extern wchar_t *wmemmove (wchar_t *__s1, const wchar_t *__s2, size_t __n)
     throw ();


extern wchar_t *wmemset (wchar_t *__s, wchar_t __c, size_t __n) throw ();





extern wchar_t *wmempcpy (wchar_t *__restrict __s1,
     const wchar_t *__restrict __s2, size_t __n)
     throw ();






extern wint_t btowc (int __c) throw ();



extern int wctob (wint_t __c) throw ();



extern int mbsinit (const mbstate_t *__ps) throw () __attribute__ ((__pure__));



extern size_t mbrtowc (wchar_t *__restrict __pwc,
         const char *__restrict __s, size_t __n,
         mbstate_t *__restrict __p) throw ();


extern size_t wcrtomb (char *__restrict __s, wchar_t __wc,
         mbstate_t *__restrict __ps) throw ();


extern size_t __mbrlen (const char *__restrict __s, size_t __n,
   mbstate_t *__restrict __ps) throw ();
extern size_t mbrlen (const char *__restrict __s, size_t __n,
        mbstate_t *__restrict __ps) throw ();

# 405 "/usr/include/wchar.h" 3 4



extern size_t mbsrtowcs (wchar_t *__restrict __dst,
    const char **__restrict __src, size_t __len,
    mbstate_t *__restrict __ps) throw ();



extern size_t wcsrtombs (char *__restrict __dst,
    const wchar_t **__restrict __src, size_t __len,
    mbstate_t *__restrict __ps) throw ();






extern size_t mbsnrtowcs (wchar_t *__restrict __dst,
     const char **__restrict __src, size_t __nmc,
     size_t __len, mbstate_t *__restrict __ps) throw ();



extern size_t wcsnrtombs (char *__restrict __dst,
     const wchar_t **__restrict __src,
     size_t __nwc, size_t __len,
     mbstate_t *__restrict __ps) throw ();






extern int wcwidth (wchar_t __c) throw ();



extern int wcswidth (const wchar_t *__s, size_t __n) throw ();






extern double wcstod (const wchar_t *__restrict __nptr,
        wchar_t **__restrict __endptr) throw ();





extern float wcstof (const wchar_t *__restrict __nptr,
       wchar_t **__restrict __endptr) throw ();
extern long double wcstold (const wchar_t *__restrict __nptr,
       wchar_t **__restrict __endptr) throw ();







extern long int wcstol (const wchar_t *__restrict __nptr,
   wchar_t **__restrict __endptr, int __base) throw ();



extern unsigned long int wcstoul (const wchar_t *__restrict __nptr,
      wchar_t **__restrict __endptr, int __base)
     throw ();






__extension__
extern long long int wcstoll (const wchar_t *__restrict __nptr,
         wchar_t **__restrict __endptr, int __base)
     throw ();



__extension__
extern unsigned long long int wcstoull (const wchar_t *__restrict __nptr,
     wchar_t **__restrict __endptr,
     int __base) throw ();






__extension__
extern long long int wcstoq (const wchar_t *__restrict __nptr,
        wchar_t **__restrict __endptr, int __base)
     throw ();



__extension__
extern unsigned long long int wcstouq (const wchar_t *__restrict __nptr,
           wchar_t **__restrict __endptr,
           int __base) throw ();
# 530 "/usr/include/wchar.h" 3 4
extern long int wcstol_l (const wchar_t *__restrict __nptr,
     wchar_t **__restrict __endptr, int __base,
     __locale_t __loc) throw ();

extern unsigned long int wcstoul_l (const wchar_t *__restrict __nptr,
        wchar_t **__restrict __endptr,
        int __base, __locale_t __loc) throw ();

__extension__
extern long long int wcstoll_l (const wchar_t *__restrict __nptr,
    wchar_t **__restrict __endptr,
    int __base, __locale_t __loc) throw ();

__extension__
extern unsigned long long int wcstoull_l (const wchar_t *__restrict __nptr,
       wchar_t **__restrict __endptr,
       int __base, __locale_t __loc)
     throw ();

extern double wcstod_l (const wchar_t *__restrict __nptr,
   wchar_t **__restrict __endptr, __locale_t __loc)
     throw ();

extern float wcstof_l (const wchar_t *__restrict __nptr,
         wchar_t **__restrict __endptr, __locale_t __loc)
     throw ();

extern long double wcstold_l (const wchar_t *__restrict __nptr,
         wchar_t **__restrict __endptr,
         __locale_t __loc) throw ();






extern wchar_t *wcpcpy (wchar_t *__restrict __dest,
   const wchar_t *__restrict __src) throw ();



extern wchar_t *wcpncpy (wchar_t *__restrict __dest,
    const wchar_t *__restrict __src, size_t __n)
     throw ();






extern __FILE *open_wmemstream (wchar_t **__bufloc, size_t *__sizeloc) throw ();






extern int fwide (__FILE *__fp, int __mode) throw ();






extern int fwprintf (__FILE *__restrict __stream,
       const wchar_t *__restrict __format, ...)
                                                           ;




extern int wprintf (const wchar_t *__restrict __format, ...)
                                                           ;

extern int swprintf (wchar_t *__restrict __s, size_t __n,
       const wchar_t *__restrict __format, ...)
     throw () ;





extern int vfwprintf (__FILE *__restrict __s,
        const wchar_t *__restrict __format,
        __gnuc_va_list __arg)
                                                           ;




extern int vwprintf (const wchar_t *__restrict __format,
       __gnuc_va_list __arg)
                                                           ;


extern int vswprintf (wchar_t *__restrict __s, size_t __n,
        const wchar_t *__restrict __format,
        __gnuc_va_list __arg)
     throw () ;






extern int fwscanf (__FILE *__restrict __stream,
      const wchar_t *__restrict __format, ...)
                                                          ;




extern int wscanf (const wchar_t *__restrict __format, ...)
                                                          ;

extern int swscanf (const wchar_t *__restrict __s,
      const wchar_t *__restrict __format, ...)
     throw () ;
# 680 "/usr/include/wchar.h" 3 4









extern int vfwscanf (__FILE *__restrict __s,
       const wchar_t *__restrict __format,
       __gnuc_va_list __arg)
                                                          ;




extern int vwscanf (const wchar_t *__restrict __format,
      __gnuc_va_list __arg)
                                                          ;

extern int vswscanf (const wchar_t *__restrict __s,
       const wchar_t *__restrict __format,
       __gnuc_va_list __arg)
     throw () ;
# 736 "/usr/include/wchar.h" 3 4









extern wint_t fgetwc (__FILE *__stream);
extern wint_t getwc (__FILE *__stream);





extern wint_t getwchar (void);






extern wint_t fputwc (wchar_t __wc, __FILE *__stream);
extern wint_t putwc (wchar_t __wc, __FILE *__stream);





extern wint_t putwchar (wchar_t __wc);







extern wchar_t *fgetws (wchar_t *__restrict __ws, int __n,
   __FILE *__restrict __stream);





extern int fputws (const wchar_t *__restrict __ws,
     __FILE *__restrict __stream);






extern wint_t ungetwc (wint_t __wc, __FILE *__stream);

# 801 "/usr/include/wchar.h" 3 4
extern wint_t getwc_unlocked (__FILE *__stream);
extern wint_t getwchar_unlocked (void);







extern wint_t fgetwc_unlocked (__FILE *__stream);







extern wint_t fputwc_unlocked (wchar_t __wc, __FILE *__stream);
# 827 "/usr/include/wchar.h" 3 4
extern wint_t putwc_unlocked (wchar_t __wc, __FILE *__stream);
extern wint_t putwchar_unlocked (wchar_t __wc);
# 837 "/usr/include/wchar.h" 3 4
extern wchar_t *fgetws_unlocked (wchar_t *__restrict __ws, int __n,
     __FILE *__restrict __stream);







extern int fputws_unlocked (const wchar_t *__restrict __ws,
       __FILE *__restrict __stream);







extern size_t wcsftime (wchar_t *__restrict __s, size_t __maxsize,
   const wchar_t *__restrict __format,
   const struct tm *__restrict __tp) throw ();







extern size_t wcsftime_l (wchar_t *__restrict __s, size_t __maxsize,
     const wchar_t *__restrict __format,
     const struct tm *__restrict __tp,
     __locale_t __loc) throw ();
# 891 "/usr/include/wchar.h" 3 4
}
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 2 3
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 3
namespace std
{
  using ::mbstate_t;
}
# 137 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 3
extern "C++"
{
namespace std __attribute__ ((__visibility__ ("default")))
{


  using ::wint_t;

  using ::btowc;
  using ::fgetwc;
  using ::fgetws;
  using ::fputwc;
  using ::fputws;
  using ::fwide;
  using ::fwprintf;
  using ::fwscanf;
  using ::getwc;
  using ::getwchar;
  using ::mbrlen;
  using ::mbrtowc;
  using ::mbsinit;
  using ::mbsrtowcs;
  using ::putwc;
  using ::putwchar;

  using ::swprintf;

  using ::swscanf;
  using ::ungetwc;
  using ::vfwprintf;

  using ::vfwscanf;


  using ::vswprintf;


  using ::vswscanf;

  using ::vwprintf;

  using ::vwscanf;

  using ::wcrtomb;
  using ::wcscat;
  using ::wcscmp;
  using ::wcscoll;
  using ::wcscpy;
  using ::wcscspn;
  using ::wcsftime;
  using ::wcslen;
  using ::wcsncat;
  using ::wcsncmp;
  using ::wcsncpy;
  using ::wcsrtombs;
  using ::wcsspn;
  using ::wcstod;

  using ::wcstof;

  using ::wcstok;
  using ::wcstol;
  using ::wcstoul;
  using ::wcsxfrm;
  using ::wctob;
  using ::wmemcmp;
  using ::wmemcpy;
  using ::wmemmove;
  using ::wmemset;
  using ::wprintf;
  using ::wscanf;
  using ::wcschr;
  using ::wcspbrk;
  using ::wcsrchr;
  using ::wcsstr;
  using ::wmemchr;
# 236 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 3

}
}







namespace __gnu_cxx
{





  using ::wcstold;
# 262 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 3
  using ::wcstoll;
  using ::wcstoull;

}

namespace std
{
  using ::__gnu_cxx::wcstold;
  using ::__gnu_cxx::wcstoll;
  using ::__gnu_cxx::wcstoull;
}
# 282 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 3
namespace std
{

  using std::wcstof;


  using std::vfwscanf;


  using std::vswscanf;


  using std::vwscanf;



  using std::wcstold;
  using std::wcstoll;
  using std::wcstoull;

}
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 55 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
 
# 55 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
  typedef long int streamoff;



#pragma GCC diagnostic pop


  typedef ptrdiff_t streamsize;
# 86 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
  template<typename _StateT>
    class fpos
    {
    private:
      streamoff _M_off;
      _StateT _M_state;

    public:




      fpos()
      : _M_off(0), _M_state() { }
# 108 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
      fpos(streamoff __off)
      : _M_off(__off), _M_state() { }


      fpos(const fpos&) = default;
      fpos& operator=(const fpos&) = default;
      ~fpos() = default;



      operator streamoff() const { return _M_off; }


      void
      state(_StateT __st)
      { _M_state = __st; }


      _StateT
      state() const
      { return _M_state; }





      fpos&
      operator+=(streamoff __off)
      {
 _M_off += __off;
 return *this;
      }





      fpos&
      operator-=(streamoff __off)
      {
 _M_off -= __off;
 return *this;
      }







      fpos
      operator+(streamoff __off) const
      {
 fpos __pos(*this);
 __pos += __off;
 return __pos;
      }







      fpos
      operator-(streamoff __off) const
      {
 fpos __pos(*this);
 __pos -= __off;
 return __pos;
      }






      streamoff
      operator-(const fpos& __other) const
      { return _M_off - __other._M_off; }
    };






  template<typename _StateT>
    inline bool
    operator==(const fpos<_StateT>& __lhs, const fpos<_StateT>& __rhs)
    { return streamoff(__lhs) == streamoff(__rhs); }

  template<typename _StateT>
    inline bool
    operator!=(const fpos<_StateT>& __lhs, const fpos<_StateT>& __rhs)
    { return streamoff(__lhs) != streamoff(__rhs); }





  typedef fpos<mbstate_t> streampos;

  typedef fpos<mbstate_t> wstreampos;
# 220 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
  typedef fpos<mbstate_t> u16streampos;

  typedef fpos<mbstate_t> u32streampos;



}
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iosfwd" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 78 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iosfwd" 3
  class ios_base;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_ios;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_streambuf;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_istream;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_ostream;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_iostream;


namespace __cxx11 {

  template<typename _CharT, typename _Traits = char_traits<_CharT>,
     typename _Alloc = allocator<_CharT> >
    class basic_stringbuf;

  template<typename _CharT, typename _Traits = char_traits<_CharT>,
    typename _Alloc = allocator<_CharT> >
    class basic_istringstream;

  template<typename _CharT, typename _Traits = char_traits<_CharT>,
    typename _Alloc = allocator<_CharT> >
    class basic_ostringstream;

  template<typename _CharT, typename _Traits = char_traits<_CharT>,
    typename _Alloc = allocator<_CharT> >
    class basic_stringstream;

}

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_filebuf;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_ifstream;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_ofstream;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_fstream;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class istreambuf_iterator;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class ostreambuf_iterator;



  typedef basic_ios<char> ios;


  typedef basic_streambuf<char> streambuf;


  typedef basic_istream<char> istream;


  typedef basic_ostream<char> ostream;


  typedef basic_iostream<char> iostream;


  typedef basic_stringbuf<char> stringbuf;


  typedef basic_istringstream<char> istringstream;


  typedef basic_ostringstream<char> ostringstream;


  typedef basic_stringstream<char> stringstream;


  typedef basic_filebuf<char> filebuf;


  typedef basic_ifstream<char> ifstream;


  typedef basic_ofstream<char> ofstream;


  typedef basic_fstream<char> fstream;



  typedef basic_ios<wchar_t> wios;


  typedef basic_streambuf<wchar_t> wstreambuf;


  typedef basic_istream<wchar_t> wistream;


  typedef basic_ostream<wchar_t> wostream;


  typedef basic_iostream<wchar_t> wiostream;


  typedef basic_stringbuf<wchar_t> wstringbuf;


  typedef basic_istringstream<wchar_t> wistringstream;


  typedef basic_ostringstream<wchar_t> wostringstream;


  typedef basic_stringstream<wchar_t> wstringstream;


  typedef basic_filebuf<wchar_t> wfilebuf;


  typedef basic_ifstream<wchar_t> wifstream;


  typedef basic_ofstream<wchar_t> wofstream;


  typedef basic_fstream<wchar_t> wfstream;
# 257 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iosfwd" 3

}
# 53 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 1 3
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 2 3

#pragma GCC visibility push(default)

extern "C++" {

namespace __cxxabiv1
{
  class __class_type_info;
}
# 85 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 3
namespace std
{






  class type_info
  {
  public:




    virtual ~type_info();



    const char* name() const noexcept
    { return __name[0] == '*' ? __name + 1 : __name; }



    bool before(const type_info& __arg) const noexcept;

   
    bool operator==(const type_info& __arg) const noexcept;


    bool operator!=(const type_info& __arg) const noexcept
    { return !operator==(__arg); }



    size_t hash_code() const noexcept
    {

      return _Hash_bytes(name(), __builtin_strlen(name()),
    static_cast<size_t>(0xc70f6907UL));



    }



    virtual bool __is_pointer_p() const;


    virtual bool __is_function_p() const;







    virtual bool __do_catch(const type_info *__thr_type, void **__thr_obj,
       unsigned __outer) const;


    virtual bool __do_upcast(const __cxxabiv1::__class_type_info *__target,
        void **__obj_ptr) const;

  protected:
    const char *__name;

    explicit type_info(const char *__n): __name(__n) { }

  private:


    type_info& operator=(const type_info&) = delete;
    type_info(const type_info&) = delete;
# 168 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 3
  };


  inline bool
  type_info::before(const type_info& __arg) const noexcept
  {




    if (__name[0] != '*' || __arg.__name[0] != '*')
      return __builtin_strcmp (__name, __arg.__name) < 0;
# 188 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 3
    return __name < __arg.__name;
  }






  inline bool
  type_info::operator==(const type_info& __arg) const noexcept
  {
    if (std::__is_constant_evaluated())
      return this == &__arg;

    if (__name == __arg.__name)
      return true;






    return __name[0] != '*' && __builtin_strcmp (__name, __arg.name()) == 0;



  }
# 224 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 3
  class bad_cast : public exception
  {
  public:
    bad_cast() noexcept { }



    virtual ~bad_cast() noexcept;


    virtual const char* what() const noexcept;
  };





  class bad_typeid : public exception
  {
  public:
    bad_typeid () noexcept { }



    virtual ~bad_typeid() noexcept;


    virtual const char* what() const noexcept;
  };
}

}

#pragma GCC visibility pop
# 53 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocated_ptr.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocated_ptr.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{




  template<typename _Alloc>
    struct __allocated_ptr
    {
      using pointer = typename allocator_traits<_Alloc>::pointer;
      using value_type = typename allocator_traits<_Alloc>::value_type;


      __allocated_ptr(_Alloc& __a, pointer __ptr) noexcept
      : _M_alloc(std::__addressof(__a)), _M_ptr(__ptr)
      { }


      template<typename _Ptr,
        typename _Req = _Require<is_same<_Ptr, value_type*>>>
      __allocated_ptr(_Alloc& __a, _Ptr __ptr)
      : _M_alloc(std::__addressof(__a)),
 _M_ptr(pointer_traits<pointer>::pointer_to(*__ptr))
      { }


      __allocated_ptr(__allocated_ptr&& __gd) noexcept
      : _M_alloc(__gd._M_alloc), _M_ptr(__gd._M_ptr)
      { __gd._M_ptr = nullptr; }


      ~__allocated_ptr()
      {
 if (_M_ptr != nullptr)
   std::allocator_traits<_Alloc>::deallocate(*_M_alloc, _M_ptr, 1);
      }


      __allocated_ptr&
      operator=(std::nullptr_t) noexcept
      {
 _M_ptr = nullptr;
 return *this;
      }

      explicit operator bool() const noexcept { return (bool)_M_ptr; }


      value_type* get() const { return std::__to_address(_M_ptr); }

      pointer release() { return std::__exchange(_M_ptr, nullptr); }

    private:
      _Alloc* _M_alloc;
      pointer _M_ptr;
    };


  template<typename _Alloc>
    inline __allocated_ptr<_Alloc>
    __allocate_guarded(_Alloc& __a)
    {
      return { __a, std::allocator_traits<_Alloc>::allocate(__a, 1) };
    }


  template<typename _Alloc>
    struct __allocated_obj : __allocated_ptr<_Alloc>
    {
      using value_type = typename __allocated_ptr<_Alloc>::value_type;

      __allocated_obj(__allocated_obj<_Alloc>&&) = default;


      __allocated_obj(__allocated_ptr<_Alloc>&& __ptr)
      : __allocated_ptr<_Alloc>(std::move(__ptr))
      { ::new ((void*)this->get()) value_type; }


      ~__allocated_obj()
      {
 if (static_cast<bool>(*this))
   this->get()->~value_type();
      }

      using __allocated_ptr<_Alloc>::operator=;

      value_type& operator*() const { return *this->get(); }
      value_type* operator->() const { return this->get(); }
    };


  template<typename _Alloc>
    inline __allocated_obj<_Alloc>
    __allocate_guarded_obj(_Alloc& __a)
    {
      return { std::__allocate_guarded(__a) };
    }



}
# 54 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 2 3



# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 1 3
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 58 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
  template<typename _Res, typename... _ArgTypes>
    struct _Maybe_unary_or_binary_function { };


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _Res, typename _T1>
    struct _Maybe_unary_or_binary_function<_Res, _T1>
    : std::unary_function<_T1, _Res> { };


  template<typename _Res, typename _T1, typename _T2>
    struct _Maybe_unary_or_binary_function<_Res, _T1, _T2>
    : std::binary_function<_T1, _T2, _Res> { };

#pragma GCC diagnostic pop

  template<typename _Signature>
    struct _Mem_fn_traits;

  template<typename _Res, typename _Class, typename... _ArgTypes>
    struct _Mem_fn_traits_base
    {
      using __result_type = _Res;
      using __maybe_type
 = _Maybe_unary_or_binary_function<_Res, _Class*, _ArgTypes...>;
      using __arity = integral_constant<size_t, sizeof...(_ArgTypes)>;
    };
# 109 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) > : _Mem_fn_traits_base<_Res, _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) > : _Mem_fn_traits_base<_Res, _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) const > : _Mem_fn_traits_base<_Res, const _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) const > : _Mem_fn_traits_base<_Res, const _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) volatile > : _Mem_fn_traits_base<_Res, volatile _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) volatile > : _Mem_fn_traits_base<_Res, volatile _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) const volatile > : _Mem_fn_traits_base<_Res, const volatile _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) const volatile > : _Mem_fn_traits_base<_Res, const volatile _Class, _ArgTypes...> { using __vararg = true_type; };
template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) &> : _Mem_fn_traits_base<_Res, _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) &> : _Mem_fn_traits_base<_Res, _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) const &> : _Mem_fn_traits_base<_Res, const _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) const &> : _Mem_fn_traits_base<_Res, const _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) volatile &> : _Mem_fn_traits_base<_Res, volatile _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) volatile &> : _Mem_fn_traits_base<_Res, volatile _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) const volatile &> : _Mem_fn_traits_base<_Res, const volatile _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) const volatile &> : _Mem_fn_traits_base<_Res, const volatile _Class, _ArgTypes...> { using __vararg = true_type; };
template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) &&> : _Mem_fn_traits_base<_Res, _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) &&> : _Mem_fn_traits_base<_Res, _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) const &&> : _Mem_fn_traits_base<_Res, const _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) const &&> : _Mem_fn_traits_base<_Res, const _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) volatile &&> : _Mem_fn_traits_base<_Res, volatile _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) volatile &&> : _Mem_fn_traits_base<_Res, volatile _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) const volatile &&> : _Mem_fn_traits_base<_Res, const volatile _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) const volatile &&> : _Mem_fn_traits_base<_Res, const volatile _Class, _ArgTypes...> { using __vararg = true_type; };
# 123 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
  template<typename _Functor, typename = __void_t<>>
    struct _Maybe_get_result_type
    { };

  template<typename _Functor>
    struct _Maybe_get_result_type<_Functor,
      __void_t<typename _Functor::result_type>>
    { typedef typename _Functor::result_type result_type; };





  template<typename _Functor>
    struct _Weak_result_type_impl
    : _Maybe_get_result_type<_Functor>
    { };


  template<typename _Res, typename... _ArgTypes >
    struct _Weak_result_type_impl<_Res(_ArgTypes...) >
    { typedef _Res result_type; };


  template<typename _Res, typename... _ArgTypes >
    struct _Weak_result_type_impl<_Res(_ArgTypes...,
           ...) >
    { typedef _Res result_type; };


  template<typename _Res, typename... _ArgTypes >
    struct _Weak_result_type_impl<_Res(*)(_ArgTypes...) >
    { typedef _Res result_type; };


  template<typename _Res, typename... _ArgTypes >
    struct
    _Weak_result_type_impl<_Res(*)(_ArgTypes..., ...) >
    { typedef _Res result_type; };


  template<typename _Functor,
    bool = is_member_function_pointer<_Functor>::value>
    struct _Weak_result_type_memfun
    : _Weak_result_type_impl<_Functor>
    { };


  template<typename _MemFunPtr>
    struct _Weak_result_type_memfun<_MemFunPtr, true>
    {
      using result_type = typename _Mem_fn_traits<_MemFunPtr>::__result_type;
    };


  template<typename _Func, typename _Class>
    struct _Weak_result_type_memfun<_Func _Class::*, false>
    { };





  template<typename _Functor>
    struct _Weak_result_type
    : _Weak_result_type_memfun<typename remove_cv<_Functor>::type>
    { };



  template<typename _Tp, typename = __void_t<>>
    struct _Refwrap_base_arg1
    { };


  template<typename _Tp>
    struct _Refwrap_base_arg1<_Tp,
         __void_t<typename _Tp::argument_type>>
    {
      typedef typename _Tp::argument_type argument_type;
    };


  template<typename _Tp, typename = __void_t<>>
    struct _Refwrap_base_arg2
    { };


  template<typename _Tp>
    struct _Refwrap_base_arg2<_Tp,
         __void_t<typename _Tp::first_argument_type,
           typename _Tp::second_argument_type>>
    {
      typedef typename _Tp::first_argument_type first_argument_type;
      typedef typename _Tp::second_argument_type second_argument_type;
    };







  template<typename _Tp>
    struct _Reference_wrapper_base
    : _Weak_result_type<_Tp>, _Refwrap_base_arg1<_Tp>, _Refwrap_base_arg2<_Tp>
    { };


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _Res, typename _T1 >
    struct _Reference_wrapper_base<_Res(_T1) >
    : unary_function<_T1, _Res>
    { };

  template<typename _Res, typename _T1>
    struct _Reference_wrapper_base<_Res(_T1) const>
    : unary_function<_T1, _Res>
    { };

  template<typename _Res, typename _T1>
    struct _Reference_wrapper_base<_Res(_T1) volatile>
    : unary_function<_T1, _Res>
    { };

  template<typename _Res, typename _T1>
    struct _Reference_wrapper_base<_Res(_T1) const volatile>
    : unary_function<_T1, _Res>
    { };


  template<typename _Res, typename _T1, typename _T2 >
    struct _Reference_wrapper_base<_Res(_T1, _T2) >
    : binary_function<_T1, _T2, _Res>
    { };

  template<typename _Res, typename _T1, typename _T2>
    struct _Reference_wrapper_base<_Res(_T1, _T2) const>
    : binary_function<_T1, _T2, _Res>
    { };

  template<typename _Res, typename _T1, typename _T2>
    struct _Reference_wrapper_base<_Res(_T1, _T2) volatile>
    : binary_function<_T1, _T2, _Res>
    { };

  template<typename _Res, typename _T1, typename _T2>
    struct _Reference_wrapper_base<_Res(_T1, _T2) const volatile>
    : binary_function<_T1, _T2, _Res>
    { };


  template<typename _Res, typename _T1 >
    struct _Reference_wrapper_base<_Res(*)(_T1) >
    : unary_function<_T1, _Res>
    { };


  template<typename _Res, typename _T1, typename _T2 >
    struct _Reference_wrapper_base<_Res(*)(_T1, _T2) >
    : binary_function<_T1, _T2, _Res>
    { };

  template<typename _Tp, bool = is_member_function_pointer<_Tp>::value>
    struct _Reference_wrapper_base_memfun
    : _Reference_wrapper_base<_Tp>
    { };

  template<typename _MemFunPtr>
    struct _Reference_wrapper_base_memfun<_MemFunPtr, true>
    : _Mem_fn_traits<_MemFunPtr>::__maybe_type
    {
      using result_type = typename _Mem_fn_traits<_MemFunPtr>::__result_type;
    };
#pragma GCC diagnostic pop
# 309 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
  template<typename _Tp>
    class reference_wrapper



    : public _Reference_wrapper_base_memfun<typename remove_cv<_Tp>::type>

    {
      _Tp* _M_data;

     
      static _Tp* _S_fun(_Tp& __r) noexcept { return std::__addressof(__r); }

      static void _S_fun(_Tp&&) = delete;

      template<typename _Up, typename _Up2 = __remove_cvref_t<_Up>>
 using __not_same
   = typename enable_if<!is_same<reference_wrapper, _Up2>::value>::type;

    public:
      typedef _Tp type;




      template<typename _Up, typename = __not_same<_Up>, typename
  = decltype(reference_wrapper::_S_fun(std::declval<_Up>()))>

 reference_wrapper(_Up&& __uref)
 noexcept(noexcept(reference_wrapper::_S_fun(std::declval<_Up>())))
 : _M_data(reference_wrapper::_S_fun(std::forward<_Up>(__uref)))
 { }

      reference_wrapper(const reference_wrapper&) = default;

      reference_wrapper&
      operator=(const reference_wrapper&) = default;

     
      operator _Tp&() const noexcept
      { return this->get(); }

     
      _Tp&
      get() const noexcept
      { return *_M_data; }

      template<typename... _Args>

 typename __invoke_result<_Tp&, _Args...>::type
 operator()(_Args&&... __args) const
 noexcept(__is_nothrow_invocable<_Tp&, _Args...>::value)
 {




   return std::__invoke(get(), std::forward<_Args>(__args)...);
 }
# 415 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
    };
# 425 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
  template<typename _Tp>
   
    inline reference_wrapper<_Tp>
    ref(_Tp& __t) noexcept
    { return reference_wrapper<_Tp>(__t); }


  template<typename _Tp>
   
    inline reference_wrapper<const _Tp>
    cref(const _Tp& __t) noexcept
    { return reference_wrapper<const _Tp>(__t); }

  template<typename _Tp>
    void ref(const _Tp&&) = delete;

  template<typename _Tp>
    void cref(const _Tp&&) = delete;


  template<typename _Tp>
   
    inline reference_wrapper<_Tp>
    ref(reference_wrapper<_Tp> __t) noexcept
    { return __t; }


  template<typename _Tp>
   
    inline reference_wrapper<const _Tp>
    cref(reference_wrapper<_Tp> __t) noexcept
    { return { __t.get() }; }




}
# 58 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/aligned_buffer.h" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/aligned_buffer.h" 3
namespace __gnu_cxx
{




  template<typename _Tp>
    struct __aligned_membuf
    {
# 60 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/aligned_buffer.h" 3
      alignas(_Tp)

 unsigned char _M_storage[sizeof(_Tp)];

      __aligned_membuf() = default;


      __aligned_membuf(std::nullptr_t) { }

      void*
      _M_addr() noexcept
      { return static_cast<void*>(&_M_storage); }

      const void*
      _M_addr() const noexcept
      { return static_cast<const void*>(&_M_storage); }

      _Tp*
      _M_ptr() noexcept
      { return static_cast<_Tp*>(_M_addr()); }

      const _Tp*
      _M_ptr() const noexcept
      { return static_cast<const _Tp*>(_M_addr()); }
    };
# 95 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/aligned_buffer.h" 3
  template<typename _Tp>
    struct __aligned_buffer
    {

      alignas(__alignof__(_Tp)) unsigned char _M_storage[sizeof(_Tp)];

      __aligned_buffer() = default;


      __aligned_buffer(std::nullptr_t) { }

      void*
      _M_addr() noexcept
      {
        return static_cast<void*>(&_M_storage);
      }

      const void*
      _M_addr() const noexcept
      {
        return static_cast<const void*>(&_M_storage);
      }

      _Tp*
      _M_ptr() noexcept
      { return static_cast<_Tp*>(_M_addr()); }

      const _Tp*
      _M_ptr() const noexcept
      { return static_cast<const _Tp*>(_M_addr()); }
    };


}
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/atomicity.h" 1 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/atomicity.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr.h" 1 3
# 30 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr.h" 3
#pragma GCC visibility push(default)
# 157 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 1 3
# 35 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
# 1 "/usr/include/pthread.h" 1 3 4
# 22 "/usr/include/pthread.h" 3 4
# 1 "/usr/include/endian.h" 1 3 4
# 36 "/usr/include/endian.h" 3 4
# 1 "/usr/include/bits/endian.h" 1 3 4
# 37 "/usr/include/endian.h" 2 3 4
# 60 "/usr/include/endian.h" 3 4
# 1 "/usr/include/bits/byteswap.h" 1 3 4
# 28 "/usr/include/bits/byteswap.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 29 "/usr/include/bits/byteswap.h" 2 3 4






# 1 "/usr/include/bits/byteswap-16.h" 1 3 4
# 36 "/usr/include/bits/byteswap.h" 2 3 4
# 44 "/usr/include/bits/byteswap.h" 3 4
static __inline unsigned int
__bswap_32 (unsigned int __bsx)
{
  return __builtin_bswap32 (__bsx);
}
# 108 "/usr/include/bits/byteswap.h" 3 4
static __inline __uint64_t
__bswap_64 (__uint64_t __bsx)
{
  return __builtin_bswap64 (__bsx);
}
# 61 "/usr/include/endian.h" 2 3 4
# 23 "/usr/include/pthread.h" 2 3 4
# 1 "/usr/include/sched.h" 1 3 4
# 29 "/usr/include/sched.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 30 "/usr/include/sched.h" 2 3 4



# 1 "/usr/include/time.h" 1 3 4
# 73 "/usr/include/time.h" 3 4


typedef __time_t time_t;



# 120 "/usr/include/time.h" 3 4
struct timespec
  {
    __time_t tv_sec;
    __syscall_slong_t tv_nsec;
  };
# 34 "/usr/include/sched.h" 2 3 4


typedef __pid_t pid_t;





# 1 "/usr/include/bits/sched.h" 1 3 4
# 73 "/usr/include/bits/sched.h" 3 4
struct sched_param
  {
    int __sched_priority;
  };

extern "C" {



extern int clone (int (*__fn) (void *__arg), void *__child_stack,
    int __flags, void *__arg, ...) throw ();


extern int unshare (int __flags) throw ();


extern int sched_getcpu (void) throw ();


extern int setns (int __fd, int __nstype) throw ();



}







struct __sched_param
  {
    int __sched_priority;
  };
# 119 "/usr/include/bits/sched.h" 3 4
typedef unsigned long int __cpu_mask;






typedef struct
{
  __cpu_mask __bits[1024 / (8 * sizeof (__cpu_mask))];
} cpu_set_t;
# 202 "/usr/include/bits/sched.h" 3 4
extern "C" {

extern int __sched_cpucount (size_t __setsize, const cpu_set_t *__setp)
  throw ();
extern cpu_set_t *__sched_cpualloc (size_t __count) throw () ;
extern void __sched_cpufree (cpu_set_t *__set) throw ();

}
# 43 "/usr/include/sched.h" 2 3 4




extern "C" {


extern int sched_setparam (__pid_t __pid, const struct sched_param *__param)
     throw ();


extern int sched_getparam (__pid_t __pid, struct sched_param *__param) throw ();


extern int sched_setscheduler (__pid_t __pid, int __policy,
          const struct sched_param *__param) throw ();


extern int sched_getscheduler (__pid_t __pid) throw ();


extern int sched_yield (void) throw ();


extern int sched_get_priority_max (int __algorithm) throw ();


extern int sched_get_priority_min (int __algorithm) throw ();


extern int sched_rr_get_interval (__pid_t __pid, struct timespec *__t) throw ();
# 117 "/usr/include/sched.h" 3 4
extern int sched_setaffinity (__pid_t __pid, size_t __cpusetsize,
         const cpu_set_t *__cpuset) throw ();


extern int sched_getaffinity (__pid_t __pid, size_t __cpusetsize,
         cpu_set_t *__cpuset) throw ();


}
# 24 "/usr/include/pthread.h" 2 3 4
# 1 "/usr/include/time.h" 1 3 4
# 29 "/usr/include/time.h" 3 4
extern "C" {







# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 38 "/usr/include/time.h" 2 3 4



# 1 "/usr/include/bits/time.h" 1 3 4
# 30 "/usr/include/bits/time.h" 3 4
struct timeval
  {
    __time_t tv_sec;
    __suseconds_t tv_usec;
  };
# 88 "/usr/include/bits/time.h" 3 4
# 1 "/usr/include/bits/timex.h" 1 3 4
# 25 "/usr/include/bits/timex.h" 3 4
struct timex
{
  unsigned int modes;
  __syscall_slong_t offset;
  __syscall_slong_t freq;
  __syscall_slong_t maxerror;
  __syscall_slong_t esterror;
  int status;
  __syscall_slong_t constant;
  __syscall_slong_t precision;
  __syscall_slong_t tolerance;
  struct timeval time;
  __syscall_slong_t tick;
  __syscall_slong_t ppsfreq;
  __syscall_slong_t jitter;
  int shift;
  __syscall_slong_t stabil;
  __syscall_slong_t jitcnt;
  __syscall_slong_t calcnt;
  __syscall_slong_t errcnt;
  __syscall_slong_t stbcnt;

  int tai;


  int :32; int :32; int :32; int :32;
  int :32; int :32; int :32; int :32;
  int :32; int :32; int :32;
};
# 89 "/usr/include/bits/time.h" 2 3 4

extern "C" {


extern int clock_adjtime (__clockid_t __clock_id, struct timex *__utx) throw ();

}
# 42 "/usr/include/time.h" 2 3 4
# 57 "/usr/include/time.h" 3 4


typedef __clock_t clock_t;



# 91 "/usr/include/time.h" 3 4
typedef __clockid_t clockid_t;
# 103 "/usr/include/time.h" 3 4
typedef __timer_t timer_t;
# 131 "/usr/include/time.h" 3 4


struct tm
{
  int tm_sec;
  int tm_min;
  int tm_hour;
  int tm_mday;
  int tm_mon;
  int tm_year;
  int tm_wday;
  int tm_yday;
  int tm_isdst;


  long int tm_gmtoff;
  const char *tm_zone;




};








struct itimerspec
  {
    struct timespec it_interval;
    struct timespec it_value;
  };


struct sigevent;
# 186 "/usr/include/time.h" 3 4



extern clock_t clock (void) throw ();


extern time_t time (time_t *__timer) throw ();


extern double difftime (time_t __time1, time_t __time0)
     throw () __attribute__ ((__const__));


extern time_t mktime (struct tm *__tp) throw ();





extern size_t strftime (char *__restrict __s, size_t __maxsize,
   const char *__restrict __format,
   const struct tm *__restrict __tp) throw ();





extern char *strptime (const char *__restrict __s,
         const char *__restrict __fmt, struct tm *__tp)
     throw ();







extern size_t strftime_l (char *__restrict __s, size_t __maxsize,
     const char *__restrict __format,
     const struct tm *__restrict __tp,
     __locale_t __loc) throw ();



extern char *strptime_l (const char *__restrict __s,
    const char *__restrict __fmt, struct tm *__tp,
    __locale_t __loc) throw ();






extern struct tm *gmtime (const time_t *__timer) throw ();



extern struct tm *localtime (const time_t *__timer) throw ();





extern struct tm *gmtime_r (const time_t *__restrict __timer,
       struct tm *__restrict __tp) throw ();



extern struct tm *localtime_r (const time_t *__restrict __timer,
          struct tm *__restrict __tp) throw ();





extern char *asctime (const struct tm *__tp) throw ();


extern char *ctime (const time_t *__timer) throw ();







extern char *asctime_r (const struct tm *__restrict __tp,
   char *__restrict __buf) throw ();


extern char *ctime_r (const time_t *__restrict __timer,
        char *__restrict __buf) throw ();




extern char *__tzname[2];
extern int __daylight;
extern long int __timezone;




extern char *tzname[2];



extern void tzset (void) throw ();



extern int daylight;
extern long int timezone;





extern int stime (const time_t *__when) throw ();
# 319 "/usr/include/time.h" 3 4
extern time_t timegm (struct tm *__tp) throw ();


extern time_t timelocal (struct tm *__tp) throw ();


extern int dysize (int __year) throw () __attribute__ ((__const__));
# 334 "/usr/include/time.h" 3 4
extern int nanosleep (const struct timespec *__requested_time,
        struct timespec *__remaining);



extern int clock_getres (clockid_t __clock_id, struct timespec *__res) throw ();


extern int clock_gettime (clockid_t __clock_id, struct timespec *__tp) throw ();


extern int clock_settime (clockid_t __clock_id, const struct timespec *__tp)
     throw ();






extern int clock_nanosleep (clockid_t __clock_id, int __flags,
       const struct timespec *__req,
       struct timespec *__rem);


extern int clock_getcpuclockid (pid_t __pid, clockid_t *__clock_id) throw ();




extern int timer_create (clockid_t __clock_id,
    struct sigevent *__restrict __evp,
    timer_t *__restrict __timerid) throw ();


extern int timer_delete (timer_t __timerid) throw ();


extern int timer_settime (timer_t __timerid, int __flags,
     const struct itimerspec *__restrict __value,
     struct itimerspec *__restrict __ovalue) throw ();


extern int timer_gettime (timer_t __timerid, struct itimerspec *__value)
     throw ();


extern int timer_getoverrun (timer_t __timerid) throw ();





extern int timespec_get (struct timespec *__ts, int __base)
     throw () __attribute__ ((__nonnull__ (1)));
# 403 "/usr/include/time.h" 3 4
extern int getdate_err;
# 412 "/usr/include/time.h" 3 4
extern struct tm *getdate (const char *__string);
# 426 "/usr/include/time.h" 3 4
extern int getdate_r (const char *__restrict __string,
        struct tm *__restrict __resbufp);


}
# 25 "/usr/include/pthread.h" 2 3 4

# 1 "/usr/include/bits/pthreadtypes.h" 1 3 4
# 21 "/usr/include/bits/pthreadtypes.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 22 "/usr/include/bits/pthreadtypes.h" 2 3 4
# 60 "/usr/include/bits/pthreadtypes.h" 3 4
typedef unsigned long int pthread_t;


union pthread_attr_t
{
  char __size[56];
  long int __align;
};

typedef union pthread_attr_t pthread_attr_t;





typedef struct __pthread_internal_list
{
  struct __pthread_internal_list *__prev;
  struct __pthread_internal_list *__next;
} __pthread_list_t;
# 90 "/usr/include/bits/pthreadtypes.h" 3 4
typedef union
{
  struct __pthread_mutex_s
  {
    int __lock;
    unsigned int __count;
    int __owner;

    unsigned int __nusers;



    int __kind;

    short __spins;
    short __elision;
    __pthread_list_t __list;
# 124 "/usr/include/bits/pthreadtypes.h" 3 4
  } __data;
  char __size[40];
  long int __align;
} pthread_mutex_t;

typedef union
{
  char __size[4];
  int __align;
} pthread_mutexattr_t;




typedef union
{
  struct
  {
    int __lock;
    unsigned int __futex;
    __extension__ unsigned long long int __total_seq;
    __extension__ unsigned long long int __wakeup_seq;
    __extension__ unsigned long long int __woken_seq;
    void *__mutex;
    unsigned int __nwaiters;
    unsigned int __broadcast_seq;
  } __data;
  char __size[48];
  __extension__ long long int __align;
} pthread_cond_t;

typedef union
{
  char __size[4];
  int __align;
} pthread_condattr_t;



typedef unsigned int pthread_key_t;



typedef int pthread_once_t;





typedef union
{

  struct
  {
    int __lock;
    unsigned int __nr_readers;
    unsigned int __readers_wakeup;
    unsigned int __writer_wakeup;
    unsigned int __nr_readers_queued;
    unsigned int __nr_writers_queued;
    int __writer;
    int __shared;
    unsigned long int __pad1;
    unsigned long int __pad2;


    unsigned int __flags;

  } __data;
# 211 "/usr/include/bits/pthreadtypes.h" 3 4
  char __size[56];
  long int __align;
} pthread_rwlock_t;

typedef union
{
  char __size[8];
  long int __align;
} pthread_rwlockattr_t;





typedef volatile int pthread_spinlock_t;




typedef union
{
  char __size[32];
  long int __align;
} pthread_barrier_t;

typedef union
{
  char __size[4];
  int __align;
} pthread_barrierattr_t;
# 27 "/usr/include/pthread.h" 2 3 4
# 1 "/usr/include/bits/setjmp.h" 1 3 4
# 26 "/usr/include/bits/setjmp.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 27 "/usr/include/bits/setjmp.h" 2 3 4




typedef long int __jmp_buf[8];
# 28 "/usr/include/pthread.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 29 "/usr/include/pthread.h" 2 3 4



enum
{
  PTHREAD_CREATE_JOINABLE,

  PTHREAD_CREATE_DETACHED

};



enum
{
  PTHREAD_MUTEX_TIMED_NP,
  PTHREAD_MUTEX_RECURSIVE_NP,
  PTHREAD_MUTEX_ERRORCHECK_NP,
  PTHREAD_MUTEX_ADAPTIVE_NP

  ,
  PTHREAD_MUTEX_NORMAL = PTHREAD_MUTEX_TIMED_NP,
  PTHREAD_MUTEX_RECURSIVE = PTHREAD_MUTEX_RECURSIVE_NP,
  PTHREAD_MUTEX_ERRORCHECK = PTHREAD_MUTEX_ERRORCHECK_NP,
  PTHREAD_MUTEX_DEFAULT = PTHREAD_MUTEX_NORMAL



  , PTHREAD_MUTEX_FAST_NP = PTHREAD_MUTEX_TIMED_NP

};




enum
{
  PTHREAD_MUTEX_STALLED,
  PTHREAD_MUTEX_STALLED_NP = PTHREAD_MUTEX_STALLED,
  PTHREAD_MUTEX_ROBUST,
  PTHREAD_MUTEX_ROBUST_NP = PTHREAD_MUTEX_ROBUST
};





enum
{
  PTHREAD_PRIO_NONE,
  PTHREAD_PRIO_INHERIT,
  PTHREAD_PRIO_PROTECT
};
# 125 "/usr/include/pthread.h" 3 4
enum
{
  PTHREAD_RWLOCK_PREFER_READER_NP,
  PTHREAD_RWLOCK_PREFER_WRITER_NP,
  PTHREAD_RWLOCK_PREFER_WRITER_NONRECURSIVE_NP,
  PTHREAD_RWLOCK_DEFAULT_NP = PTHREAD_RWLOCK_PREFER_READER_NP
};
# 166 "/usr/include/pthread.h" 3 4
enum
{
  PTHREAD_INHERIT_SCHED,

  PTHREAD_EXPLICIT_SCHED

};



enum
{
  PTHREAD_SCOPE_SYSTEM,

  PTHREAD_SCOPE_PROCESS

};



enum
{
  PTHREAD_PROCESS_PRIVATE,

  PTHREAD_PROCESS_SHARED

};
# 201 "/usr/include/pthread.h" 3 4
struct _pthread_cleanup_buffer
{
  void (*__routine) (void *);
  void *__arg;
  int __canceltype;
  struct _pthread_cleanup_buffer *__prev;
};


enum
{
  PTHREAD_CANCEL_ENABLE,

  PTHREAD_CANCEL_DISABLE

};
enum
{
  PTHREAD_CANCEL_DEFERRED,

  PTHREAD_CANCEL_ASYNCHRONOUS

};
# 239 "/usr/include/pthread.h" 3 4
extern "C" {




extern int pthread_create (pthread_t *__restrict __newthread,
      const pthread_attr_t *__restrict __attr,
      void *(*__start_routine) (void *),
      void *__restrict __arg) throw () __attribute__ ((__nonnull__ (1, 3)));





extern void pthread_exit (void *__retval) __attribute__ ((__noreturn__));







extern int pthread_join (pthread_t __th, void **__thread_return);




extern int pthread_tryjoin_np (pthread_t __th, void **__thread_return) throw ();







extern int pthread_timedjoin_np (pthread_t __th, void **__thread_return,
     const struct timespec *__abstime);






extern int pthread_detach (pthread_t __th) throw ();



extern pthread_t pthread_self (void) throw () __attribute__ ((__const__));


extern int pthread_equal (pthread_t __thread1, pthread_t __thread2)
  throw () __attribute__ ((__const__));







extern int pthread_attr_init (pthread_attr_t *__attr) throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_attr_destroy (pthread_attr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_attr_getdetachstate (const pthread_attr_t *__attr,
     int *__detachstate)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setdetachstate (pthread_attr_t *__attr,
     int __detachstate)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_attr_getguardsize (const pthread_attr_t *__attr,
          size_t *__guardsize)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setguardsize (pthread_attr_t *__attr,
          size_t __guardsize)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_attr_getschedparam (const pthread_attr_t *__restrict __attr,
           struct sched_param *__restrict __param)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setschedparam (pthread_attr_t *__restrict __attr,
           const struct sched_param *__restrict
           __param) throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_getschedpolicy (const pthread_attr_t *__restrict
     __attr, int *__restrict __policy)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setschedpolicy (pthread_attr_t *__attr, int __policy)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_attr_getinheritsched (const pthread_attr_t *__restrict
      __attr, int *__restrict __inherit)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setinheritsched (pthread_attr_t *__attr,
      int __inherit)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_attr_getscope (const pthread_attr_t *__restrict __attr,
      int *__restrict __scope)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setscope (pthread_attr_t *__attr, int __scope)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_attr_getstackaddr (const pthread_attr_t *__restrict
          __attr, void **__restrict __stackaddr)
     throw () __attribute__ ((__nonnull__ (1, 2))) __attribute__ ((__deprecated__));





extern int pthread_attr_setstackaddr (pthread_attr_t *__attr,
          void *__stackaddr)
     throw () __attribute__ ((__nonnull__ (1))) __attribute__ ((__deprecated__));


extern int pthread_attr_getstacksize (const pthread_attr_t *__restrict
          __attr, size_t *__restrict __stacksize)
     throw () __attribute__ ((__nonnull__ (1, 2)));




extern int pthread_attr_setstacksize (pthread_attr_t *__attr,
          size_t __stacksize)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_attr_getstack (const pthread_attr_t *__restrict __attr,
      void **__restrict __stackaddr,
      size_t *__restrict __stacksize)
     throw () __attribute__ ((__nonnull__ (1, 2, 3)));




extern int pthread_attr_setstack (pthread_attr_t *__attr, void *__stackaddr,
      size_t __stacksize) throw () __attribute__ ((__nonnull__ (1)));





extern int pthread_attr_setaffinity_np (pthread_attr_t *__attr,
     size_t __cpusetsize,
     const cpu_set_t *__cpuset)
     throw () __attribute__ ((__nonnull__ (1, 3)));



extern int pthread_attr_getaffinity_np (const pthread_attr_t *__attr,
     size_t __cpusetsize,
     cpu_set_t *__cpuset)
     throw () __attribute__ ((__nonnull__ (1, 3)));





extern int pthread_getattr_np (pthread_t __th, pthread_attr_t *__attr)
     throw () __attribute__ ((__nonnull__ (2)));







extern int pthread_setschedparam (pthread_t __target_thread, int __policy,
      const struct sched_param *__param)
     throw () __attribute__ ((__nonnull__ (3)));


extern int pthread_getschedparam (pthread_t __target_thread,
      int *__restrict __policy,
      struct sched_param *__restrict __param)
     throw () __attribute__ ((__nonnull__ (2, 3)));


extern int pthread_setschedprio (pthread_t __target_thread, int __prio)
     throw ();




extern int pthread_getname_np (pthread_t __target_thread, char *__buf,
          size_t __buflen)
     throw () __attribute__ ((__nonnull__ (2)));


extern int pthread_setname_np (pthread_t __target_thread, const char *__name)
     throw () __attribute__ ((__nonnull__ (2)));





extern int pthread_getconcurrency (void) throw ();


extern int pthread_setconcurrency (int __level) throw ();







extern int pthread_yield (void) throw ();




extern int pthread_setaffinity_np (pthread_t __th, size_t __cpusetsize,
       const cpu_set_t *__cpuset)
     throw () __attribute__ ((__nonnull__ (3)));


extern int pthread_getaffinity_np (pthread_t __th, size_t __cpusetsize,
       cpu_set_t *__cpuset)
     throw () __attribute__ ((__nonnull__ (3)));
# 497 "/usr/include/pthread.h" 3 4
extern int pthread_once (pthread_once_t *__once_control,
    void (*__init_routine) (void)) __attribute__ ((__nonnull__ (1, 2)));
# 509 "/usr/include/pthread.h" 3 4
extern int pthread_setcancelstate (int __state, int *__oldstate);



extern int pthread_setcanceltype (int __type, int *__oldtype);


extern int pthread_cancel (pthread_t __th);




extern void pthread_testcancel (void);




typedef struct
{
  struct
  {
    __jmp_buf __cancel_jmp_buf;
    int __mask_was_saved;
  } __cancel_jmp_buf[1];
  void *__pad[4];
} __pthread_unwind_buf_t __attribute__ ((__aligned__));
# 543 "/usr/include/pthread.h" 3 4
struct __pthread_cleanup_frame
{
  void (*__cancel_routine) (void *);
  void *__cancel_arg;
  int __do_it;
  int __cancel_type;
};




class __pthread_cleanup_class
{
  void (*__cancel_routine) (void *);
  void *__cancel_arg;
  int __do_it;
  int __cancel_type;

 public:
  __pthread_cleanup_class (void (*__fct) (void *), void *__arg)
    : __cancel_routine (__fct), __cancel_arg (__arg), __do_it (1) { }
  ~__pthread_cleanup_class () { if (__do_it) __cancel_routine (__cancel_arg); }
  void __setdoit (int __newval) { __do_it = __newval; }
  void __defer () { pthread_setcanceltype (PTHREAD_CANCEL_DEFERRED,
        &__cancel_type); }
  void __restore () const { pthread_setcanceltype (__cancel_type, 0); }
};
# 745 "/usr/include/pthread.h" 3 4
struct __jmp_buf_tag;
extern int __sigsetjmp (struct __jmp_buf_tag *__env, int __savemask) throw ();





extern int pthread_mutex_init (pthread_mutex_t *__mutex,
          const pthread_mutexattr_t *__mutexattr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_mutex_destroy (pthread_mutex_t *__mutex)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_mutex_trylock (pthread_mutex_t *__mutex)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_mutex_lock (pthread_mutex_t *__mutex)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_mutex_timedlock (pthread_mutex_t *__restrict __mutex,
        const struct timespec *__restrict
        __abstime) throw () __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_mutex_unlock (pthread_mutex_t *__mutex)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_mutex_getprioceiling (const pthread_mutex_t *
      __restrict __mutex,
      int *__restrict __prioceiling)
     throw () __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_mutex_setprioceiling (pthread_mutex_t *__restrict __mutex,
      int __prioceiling,
      int *__restrict __old_ceiling)
     throw () __attribute__ ((__nonnull__ (1, 3)));




extern int pthread_mutex_consistent (pthread_mutex_t *__mutex)
     throw () __attribute__ ((__nonnull__ (1)));

extern int pthread_mutex_consistent_np (pthread_mutex_t *__mutex)
     throw () __attribute__ ((__nonnull__ (1)));
# 809 "/usr/include/pthread.h" 3 4
extern int pthread_mutexattr_init (pthread_mutexattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_mutexattr_destroy (pthread_mutexattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_mutexattr_getpshared (const pthread_mutexattr_t *
      __restrict __attr,
      int *__restrict __pshared)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_mutexattr_setpshared (pthread_mutexattr_t *__attr,
      int __pshared)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_mutexattr_gettype (const pthread_mutexattr_t *__restrict
          __attr, int *__restrict __kind)
     throw () __attribute__ ((__nonnull__ (1, 2)));




extern int pthread_mutexattr_settype (pthread_mutexattr_t *__attr, int __kind)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_mutexattr_getprotocol (const pthread_mutexattr_t *
       __restrict __attr,
       int *__restrict __protocol)
     throw () __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_mutexattr_setprotocol (pthread_mutexattr_t *__attr,
       int __protocol)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_mutexattr_getprioceiling (const pthread_mutexattr_t *
          __restrict __attr,
          int *__restrict __prioceiling)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_mutexattr_setprioceiling (pthread_mutexattr_t *__attr,
          int __prioceiling)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_mutexattr_getrobust (const pthread_mutexattr_t *__attr,
     int *__robustness)
     throw () __attribute__ ((__nonnull__ (1, 2)));

extern int pthread_mutexattr_getrobust_np (const pthread_mutexattr_t *__attr,
        int *__robustness)
     throw () __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_mutexattr_setrobust (pthread_mutexattr_t *__attr,
     int __robustness)
     throw () __attribute__ ((__nonnull__ (1)));

extern int pthread_mutexattr_setrobust_np (pthread_mutexattr_t *__attr,
        int __robustness)
     throw () __attribute__ ((__nonnull__ (1)));
# 891 "/usr/include/pthread.h" 3 4
extern int pthread_rwlock_init (pthread_rwlock_t *__restrict __rwlock,
    const pthread_rwlockattr_t *__restrict
    __attr) throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlock_destroy (pthread_rwlock_t *__rwlock)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlock_rdlock (pthread_rwlock_t *__rwlock)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlock_tryrdlock (pthread_rwlock_t *__rwlock)
  throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_rwlock_timedrdlock (pthread_rwlock_t *__restrict __rwlock,
           const struct timespec *__restrict
           __abstime) throw () __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_rwlock_wrlock (pthread_rwlock_t *__rwlock)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlock_trywrlock (pthread_rwlock_t *__rwlock)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_rwlock_timedwrlock (pthread_rwlock_t *__restrict __rwlock,
           const struct timespec *__restrict
           __abstime) throw () __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_rwlock_unlock (pthread_rwlock_t *__rwlock)
     throw () __attribute__ ((__nonnull__ (1)));





extern int pthread_rwlockattr_init (pthread_rwlockattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlockattr_destroy (pthread_rwlockattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlockattr_getpshared (const pthread_rwlockattr_t *
       __restrict __attr,
       int *__restrict __pshared)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_rwlockattr_setpshared (pthread_rwlockattr_t *__attr,
       int __pshared)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlockattr_getkind_np (const pthread_rwlockattr_t *
       __restrict __attr,
       int *__restrict __pref)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_rwlockattr_setkind_np (pthread_rwlockattr_t *__attr,
       int __pref) throw () __attribute__ ((__nonnull__ (1)));







extern int pthread_cond_init (pthread_cond_t *__restrict __cond,
         const pthread_condattr_t *__restrict __cond_attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_cond_destroy (pthread_cond_t *__cond)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_cond_signal (pthread_cond_t *__cond)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_cond_broadcast (pthread_cond_t *__cond)
     throw () __attribute__ ((__nonnull__ (1)));






extern int pthread_cond_wait (pthread_cond_t *__restrict __cond,
         pthread_mutex_t *__restrict __mutex)
     __attribute__ ((__nonnull__ (1, 2)));
# 1003 "/usr/include/pthread.h" 3 4
extern int pthread_cond_timedwait (pthread_cond_t *__restrict __cond,
       pthread_mutex_t *__restrict __mutex,
       const struct timespec *__restrict __abstime)
     __attribute__ ((__nonnull__ (1, 2, 3)));




extern int pthread_condattr_init (pthread_condattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_condattr_destroy (pthread_condattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_condattr_getpshared (const pthread_condattr_t *
     __restrict __attr,
     int *__restrict __pshared)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_condattr_setpshared (pthread_condattr_t *__attr,
     int __pshared) throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_condattr_getclock (const pthread_condattr_t *
          __restrict __attr,
          __clockid_t *__restrict __clock_id)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_condattr_setclock (pthread_condattr_t *__attr,
          __clockid_t __clock_id)
     throw () __attribute__ ((__nonnull__ (1)));
# 1047 "/usr/include/pthread.h" 3 4
extern int pthread_spin_init (pthread_spinlock_t *__lock, int __pshared)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_spin_destroy (pthread_spinlock_t *__lock)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_spin_lock (pthread_spinlock_t *__lock)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_spin_trylock (pthread_spinlock_t *__lock)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_spin_unlock (pthread_spinlock_t *__lock)
     throw () __attribute__ ((__nonnull__ (1)));






extern int pthread_barrier_init (pthread_barrier_t *__restrict __barrier,
     const pthread_barrierattr_t *__restrict
     __attr, unsigned int __count)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_barrier_destroy (pthread_barrier_t *__barrier)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_barrier_wait (pthread_barrier_t *__barrier)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_barrierattr_init (pthread_barrierattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_barrierattr_destroy (pthread_barrierattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_barrierattr_getpshared (const pthread_barrierattr_t *
        __restrict __attr,
        int *__restrict __pshared)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_barrierattr_setpshared (pthread_barrierattr_t *__attr,
        int __pshared)
     throw () __attribute__ ((__nonnull__ (1)));
# 1114 "/usr/include/pthread.h" 3 4
extern int pthread_key_create (pthread_key_t *__key,
          void (*__destr_function) (void *))
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_key_delete (pthread_key_t __key) throw ();


extern void *pthread_getspecific (pthread_key_t __key) throw ();


extern int pthread_setspecific (pthread_key_t __key,
    const void *__pointer) throw () ;




extern int pthread_getcpuclockid (pthread_t __thread_id,
      __clockid_t *__clock_id)
     throw () __attribute__ ((__nonnull__ (2)));
# 1148 "/usr/include/pthread.h" 3 4
extern int pthread_atfork (void (*__prepare) (void),
      void (*__parent) (void),
      void (*__child) (void)) throw ();
# 1162 "/usr/include/pthread.h" 3 4
}
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 2 3
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
typedef pthread_t __gthread_t;
typedef pthread_key_t __gthread_key_t;
typedef pthread_once_t __gthread_once_t;
typedef pthread_mutex_t __gthread_mutex_t;



typedef pthread_mutex_t __gthread_recursive_mutex_t;
typedef pthread_cond_t __gthread_cond_t;
typedef struct timespec __gthread_time_t;
# 123 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
static __typeof(pthread_once) __gthrw_pthread_once __attribute__ ((__weakref__("pthread_once"), __copy__ (pthread_once)));
static __typeof(pthread_getspecific) __gthrw_pthread_getspecific __attribute__ ((__weakref__("pthread_getspecific"), __copy__ (pthread_getspecific)));
static __typeof(pthread_setspecific) __gthrw_pthread_setspecific __attribute__ ((__weakref__("pthread_setspecific"), __copy__ (pthread_setspecific)));

static __typeof(pthread_create) __gthrw_pthread_create __attribute__ ((__weakref__("pthread_create"), __copy__ (pthread_create)));
static __typeof(pthread_join) __gthrw_pthread_join __attribute__ ((__weakref__("pthread_join"), __copy__ (pthread_join)));
static __typeof(pthread_equal) __gthrw_pthread_equal __attribute__ ((__weakref__("pthread_equal"), __copy__ (pthread_equal)));
static __typeof(pthread_self) __gthrw_pthread_self __attribute__ ((__weakref__("pthread_self"), __copy__ (pthread_self)));
static __typeof(pthread_detach) __gthrw_pthread_detach __attribute__ ((__weakref__("pthread_detach"), __copy__ (pthread_detach)));

static __typeof(pthread_cancel) __gthrw_pthread_cancel __attribute__ ((__weakref__("pthread_cancel"), __copy__ (pthread_cancel)));

static __typeof(sched_yield) __gthrw_sched_yield __attribute__ ((__weakref__("sched_yield"), __copy__ (sched_yield)));

static __typeof(pthread_mutex_lock) __gthrw_pthread_mutex_lock __attribute__ ((__weakref__("pthread_mutex_lock"), __copy__ (pthread_mutex_lock)));
static __typeof(pthread_mutex_trylock) __gthrw_pthread_mutex_trylock __attribute__ ((__weakref__("pthread_mutex_trylock"), __copy__ (pthread_mutex_trylock)));

static __typeof(pthread_mutex_timedlock) __gthrw_pthread_mutex_timedlock __attribute__ ((__weakref__("pthread_mutex_timedlock"), __copy__ (pthread_mutex_timedlock)));

static __typeof(pthread_mutex_unlock) __gthrw_pthread_mutex_unlock __attribute__ ((__weakref__("pthread_mutex_unlock"), __copy__ (pthread_mutex_unlock)));
static __typeof(pthread_mutex_init) __gthrw_pthread_mutex_init __attribute__ ((__weakref__("pthread_mutex_init"), __copy__ (pthread_mutex_init)));
static __typeof(pthread_mutex_destroy) __gthrw_pthread_mutex_destroy __attribute__ ((__weakref__("pthread_mutex_destroy"), __copy__ (pthread_mutex_destroy)));

static __typeof(pthread_cond_init) __gthrw_pthread_cond_init __attribute__ ((__weakref__("pthread_cond_init"), __copy__ (pthread_cond_init)));
static __typeof(pthread_cond_broadcast) __gthrw_pthread_cond_broadcast __attribute__ ((__weakref__("pthread_cond_broadcast"), __copy__ (pthread_cond_broadcast)));
static __typeof(pthread_cond_signal) __gthrw_pthread_cond_signal __attribute__ ((__weakref__("pthread_cond_signal"), __copy__ (pthread_cond_signal)));
static __typeof(pthread_cond_wait) __gthrw_pthread_cond_wait __attribute__ ((__weakref__("pthread_cond_wait"), __copy__ (pthread_cond_wait)));
static __typeof(pthread_cond_timedwait) __gthrw_pthread_cond_timedwait __attribute__ ((__weakref__("pthread_cond_timedwait"), __copy__ (pthread_cond_timedwait)));
static __typeof(pthread_cond_destroy) __gthrw_pthread_cond_destroy __attribute__ ((__weakref__("pthread_cond_destroy"), __copy__ (pthread_cond_destroy)));

static __typeof(pthread_key_create) __gthrw_pthread_key_create __attribute__ ((__weakref__("pthread_key_create"), __copy__ (pthread_key_create)));
static __typeof(pthread_key_delete) __gthrw_pthread_key_delete __attribute__ ((__weakref__("pthread_key_delete"), __copy__ (pthread_key_delete)));
static __typeof(pthread_mutexattr_init) __gthrw_pthread_mutexattr_init __attribute__ ((__weakref__("pthread_mutexattr_init"), __copy__ (pthread_mutexattr_init)));
static __typeof(pthread_mutexattr_settype) __gthrw_pthread_mutexattr_settype __attribute__ ((__weakref__("pthread_mutexattr_settype"), __copy__ (pthread_mutexattr_settype)));
static __typeof(pthread_mutexattr_destroy) __gthrw_pthread_mutexattr_destroy __attribute__ ((__weakref__("pthread_mutexattr_destroy"), __copy__ (pthread_mutexattr_destroy)));
# 274 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
static __typeof(pthread_key_create) __gthrw___pthread_key_create __attribute__ ((__weakref__("__pthread_key_create"), __copy__ (pthread_key_create)));
# 284 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
#pragma GCC visibility push(hidden)
inline __attribute__((__always_inline__)) int
__gthread_active_p (void)
{
  static void *const __gthread_active_ptr
    = __extension__ (void *) &__gthrw___pthread_key_create;
  return __gthread_active_ptr != 0;
}
#pragma GCC visibility pop
# 705 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
inline __attribute__((__always_inline__)) int
__gthread_create (__gthread_t *__threadid, void *(*__func) (void*),
    void *__args)
{
  return __gthrw_pthread_create (__threadid, __null, __func, __args);
}

inline __attribute__((__always_inline__)) int
__gthread_join (__gthread_t __threadid, void **__value_ptr)
{
  return __gthrw_pthread_join (__threadid, __value_ptr);
}

inline __attribute__((__always_inline__)) int
__gthread_detach (__gthread_t __threadid)
{
  return __gthrw_pthread_detach (__threadid);
}

inline __attribute__((__always_inline__)) int
__gthread_equal (__gthread_t __t1, __gthread_t __t2)
{
  return __gthrw_pthread_equal (__t1, __t2);
}

inline __attribute__((__always_inline__)) __gthread_t
__gthread_self (void)
{
  return __gthrw_pthread_self ();
}

inline __attribute__((__always_inline__)) int
__gthread_yield (void)
{
  return __gthrw_sched_yield ();
}

inline __attribute__((__always_inline__)) int
__gthread_once (__gthread_once_t *__once, void (*__func) (void))
{
  if (__gthread_active_p ())
    return __gthrw_pthread_once (__once, __func);
  else
    return -1;
}

inline __attribute__((__always_inline__)) int
__gthread_key_create (__gthread_key_t *__key, void (*__dtor) (void *))
{
  return __gthrw_pthread_key_create (__key, __dtor);
}

inline __attribute__((__always_inline__)) int
__gthread_key_delete (__gthread_key_t __key)
{
  return __gthrw_pthread_key_delete (__key);
}

inline __attribute__((__always_inline__)) void *
__gthread_getspecific (__gthread_key_t __key)
{
  return __gthrw_pthread_getspecific (__key);
}

inline __attribute__((__always_inline__)) int
__gthread_setspecific (__gthread_key_t __key, const void *__ptr)
{
  return __gthrw_pthread_setspecific (__key, __ptr);
}

inline __attribute__((__always_inline__)) void
__gthread_mutex_init_function (__gthread_mutex_t *__mutex)
{
  if (__gthread_active_p ())
    __gthrw_pthread_mutex_init (__mutex, __null);
}

inline __attribute__((__always_inline__)) int
__gthread_mutex_destroy (__gthread_mutex_t *__mutex)
{
  if (__gthread_active_p ())
    return __gthrw_pthread_mutex_destroy (__mutex);
  else
    return 0;
}

inline __attribute__((__always_inline__)) int
__gthread_mutex_lock (__gthread_mutex_t *__mutex)
{
  if (__gthread_active_p ())
    return __gthrw_pthread_mutex_lock (__mutex);
  else
    return 0;
}

inline __attribute__((__always_inline__)) int
__gthread_mutex_trylock (__gthread_mutex_t *__mutex)
{
  if (__gthread_active_p ())
    return __gthrw_pthread_mutex_trylock (__mutex);
  else
    return 0;
}


inline __attribute__((__always_inline__)) int
__gthread_mutex_timedlock (__gthread_mutex_t *__mutex,
      const __gthread_time_t *__abs_timeout)
{
  if (__gthread_active_p ())
    return __gthrw_pthread_mutex_timedlock (__mutex, __abs_timeout);
  else
    return 0;
}


inline __attribute__((__always_inline__)) int
__gthread_mutex_unlock (__gthread_mutex_t *__mutex)
{
  if (__gthread_active_p ())
    return __gthrw_pthread_mutex_unlock (__mutex);
  else
    return 0;
}
# 854 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
inline __attribute__((__always_inline__)) int
__gthread_recursive_mutex_lock (__gthread_recursive_mutex_t *__mutex)
{
  return __gthread_mutex_lock (__mutex);
}

inline __attribute__((__always_inline__)) int
__gthread_recursive_mutex_trylock (__gthread_recursive_mutex_t *__mutex)
{
  return __gthread_mutex_trylock (__mutex);
}


inline __attribute__((__always_inline__)) int
__gthread_recursive_mutex_timedlock (__gthread_recursive_mutex_t *__mutex,
         const __gthread_time_t *__abs_timeout)
{
  return __gthread_mutex_timedlock (__mutex, __abs_timeout);
}


inline __attribute__((__always_inline__)) int
__gthread_recursive_mutex_unlock (__gthread_recursive_mutex_t *__mutex)
{
  return __gthread_mutex_unlock (__mutex);
}

inline __attribute__((__always_inline__)) int
__gthread_recursive_mutex_destroy (__gthread_recursive_mutex_t *__mutex)
{
  return __gthread_mutex_destroy (__mutex);
}
# 896 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
inline __attribute__((__always_inline__)) int
__gthread_cond_broadcast (__gthread_cond_t *__cond)
{
  return __gthrw_pthread_cond_broadcast (__cond);
}

inline __attribute__((__always_inline__)) int
__gthread_cond_signal (__gthread_cond_t *__cond)
{
  return __gthrw_pthread_cond_signal (__cond);
}

inline __attribute__((__always_inline__)) int
__gthread_cond_wait (__gthread_cond_t *__cond, __gthread_mutex_t *__mutex)
{
  return __gthrw_pthread_cond_wait (__cond, __mutex);
}

inline __attribute__((__always_inline__)) int
__gthread_cond_timedwait (__gthread_cond_t *__cond, __gthread_mutex_t *__mutex,
     const __gthread_time_t *__abs_timeout)
{
  return __gthrw_pthread_cond_timedwait (__cond, __mutex, __abs_timeout);
}

inline __attribute__((__always_inline__)) int
__gthread_cond_wait_recursive (__gthread_cond_t *__cond,
          __gthread_recursive_mutex_t *__mutex)
{
  return __gthread_cond_wait (__cond, __mutex);
}

inline __attribute__((__always_inline__)) int
__gthread_cond_destroy (__gthread_cond_t* __cond)
{
  return __gthrw_pthread_cond_destroy (__cond);
}
# 158 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr.h" 2 3


#pragma GCC visibility pop
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/atomicity.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/atomic_word.h" 1 3
# 32 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/atomic_word.h" 3
typedef int _Atomic_word;
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/atomicity.h" 2 3




namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{


  __attribute__((__always_inline__))
  inline bool
  __is_single_threaded() noexcept
  {





    return !__gthread_active_p();

  }






  inline _Atomic_word
  __attribute__((__always_inline__))
  __exchange_and_add(volatile _Atomic_word* __mem, int __val)
  { return __atomic_fetch_add(__mem, __val, 4); }

  inline void
  __attribute__((__always_inline__))
  __atomic_add(volatile _Atomic_word* __mem, int __val)
  { __atomic_fetch_add(__mem, __val, 4); }
# 82 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/atomicity.h" 3
  inline _Atomic_word
  __attribute__((__always_inline__))
  __exchange_and_add_single(_Atomic_word* __mem, int __val)
  {
    _Atomic_word __result = *__mem;
    *__mem += __val;
    return __result;
  }

  inline void
  __attribute__((__always_inline__))
  __atomic_add_single(_Atomic_word* __mem, int __val)
  { *__mem += __val; }

  inline _Atomic_word
  __attribute__ ((__always_inline__))
  __exchange_and_add_dispatch(_Atomic_word* __mem, int __val)
  {
    if (__is_single_threaded())
      return __exchange_and_add_single(__mem, __val);
    else
      return __exchange_and_add(__mem, __val);
  }

  inline void
  __attribute__ ((__always_inline__))
  __atomic_add_dispatch(_Atomic_word* __mem, int __val)
  {
    if (__is_single_threaded())
      __atomic_add_single(__mem, __val);
    else
      __atomic_add(__mem, __val);
  }


}
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/concurrence.h" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/concurrence.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 2 3

extern "C++" {

namespace std __attribute__ ((__visibility__ ("default")))
{
# 56 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 3
  class bad_exception : public exception
  {
  public:
    bad_exception() noexcept { }



    virtual ~bad_exception() noexcept;


    virtual const char*
    what() const noexcept;
  };


  typedef void (*terminate_handler) ();


  terminate_handler set_terminate(terminate_handler) noexcept;



  terminate_handler get_terminate() noexcept;




  void terminate() noexcept __attribute__ ((__noreturn__,__cold__));



  typedef void (*__attribute__ ((__deprecated__)) unexpected_handler) ();





  __attribute__ ((__deprecated__))
  unexpected_handler set_unexpected(unexpected_handler) noexcept;







  __attribute__ ((__deprecated__))
  unexpected_handler get_unexpected() noexcept;







  __attribute__ ((__deprecated__))
  void unexpected() __attribute__ ((__noreturn__,__cold__));
# 126 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 3
 
  bool uncaught_exception() noexcept __attribute__ ((__pure__));
# 138 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 3
}

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{

# 160 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 3
  void __verbose_terminate_handler();


}

}


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_ptr.h" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_ptr.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cxxabi_init_exception.h" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cxxabi_init_exception.h" 3
#pragma GCC visibility push(default)

# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 425 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
} max_align_t;






  typedef decltype(nullptr) nullptr_t;
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cxxabi_init_exception.h" 2 3
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cxxabi_init_exception.h" 3
namespace std
{
  class type_info;
}

namespace __cxxabiv1
{
  struct __cxa_refcounted_exception;

  extern "C"
    {

      void*
      __cxa_allocate_exception(size_t) noexcept;

      void
      __cxa_free_exception(void*) noexcept;


      __cxa_refcounted_exception*
      __cxa_init_primary_exception(void *__object, std::type_info *__tinfo,
                void ( *__dest) (void *))
 noexcept;

    }
}



#pragma GCC visibility pop
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_ptr.h" 2 3
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_ptr.h" 3
extern "C++" {

namespace std __attribute__ ((__visibility__ ("default")))
{
  class type_info;






  namespace __exception_ptr
  {
    class exception_ptr;
  }

  using __exception_ptr::exception_ptr;
# 75 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_ptr.h" 3
  exception_ptr current_exception() noexcept;

  template<typename _Ex>
  exception_ptr make_exception_ptr(_Ex) noexcept;


  void rethrow_exception(exception_ptr) __attribute__ ((__noreturn__));

  namespace __exception_ptr
  {
    using std::rethrow_exception;
# 97 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_ptr.h" 3
    class exception_ptr
    {
      void* _M_exception_object;

      explicit exception_ptr(void* __e) noexcept;

      void _M_addref() noexcept;
      void _M_release() noexcept;

      void *_M_get() const noexcept __attribute__ ((__pure__));

      friend exception_ptr std::current_exception() noexcept;
      friend void std::rethrow_exception(exception_ptr);
      template<typename _Ex>
      friend exception_ptr std::make_exception_ptr(_Ex) noexcept;

    public:
      exception_ptr() noexcept;

      exception_ptr(const exception_ptr&) noexcept;


      exception_ptr(nullptr_t) noexcept
      : _M_exception_object(nullptr)
      { }

      exception_ptr(exception_ptr&& __o) noexcept
      : _M_exception_object(__o._M_exception_object)
      { __o._M_exception_object = nullptr; }
# 135 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_ptr.h" 3
      exception_ptr&
      operator=(const exception_ptr&) noexcept;


      exception_ptr&
      operator=(exception_ptr&& __o) noexcept
      {
        exception_ptr(static_cast<exception_ptr&&>(__o)).swap(*this);
        return *this;
      }


      ~exception_ptr() noexcept;

      void
      swap(exception_ptr&) noexcept;
# 161 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_ptr.h" 3
      explicit operator bool() const noexcept
      { return _M_exception_object; }







      friend bool
      operator==(const exception_ptr& __x, const exception_ptr& __y)
      noexcept
      { return __x._M_exception_object == __y._M_exception_object; }

      friend bool
      operator!=(const exception_ptr& __x, const exception_ptr& __y)
      noexcept
      { return __x._M_exception_object != __y._M_exception_object; }


      const class std::type_info*
      __cxa_exception_type() const noexcept
 __attribute__ ((__pure__));
    };

   
    inline
    exception_ptr::exception_ptr() noexcept
    : _M_exception_object(0)
    { }

   
    inline
    exception_ptr::exception_ptr(const exception_ptr& __other)
    noexcept
    : _M_exception_object(__other._M_exception_object)
    {
      if (_M_exception_object)
 _M_addref();
    }

   
    inline
    exception_ptr::~exception_ptr() noexcept
    {
      if (_M_exception_object)
 _M_release();
    }

   
    inline exception_ptr&
    exception_ptr::operator=(const exception_ptr& __other) noexcept
    {
      exception_ptr(__other).swap(*this);
      return *this;
    }

   
    inline void
    exception_ptr::swap(exception_ptr &__other) noexcept
    {
      void *__tmp = _M_exception_object;
      _M_exception_object = __other._M_exception_object;
      __other._M_exception_object = __tmp;
    }


    inline void
    swap(exception_ptr& __lhs, exception_ptr& __rhs)
    { __lhs.swap(__rhs); }


    template<typename _Ex>
     
      inline void
      __dest_thunk(void* __x)
      { static_cast<_Ex*>(__x)->~_Ex(); }


  }

  using __exception_ptr::swap;



  template<typename _Ex>
    exception_ptr
    make_exception_ptr(_Ex __ex) noexcept
    {

      using _Ex2 = typename decay<_Ex>::type;
      void* __e = __cxxabiv1::__cxa_allocate_exception(sizeof(_Ex));
      (void) __cxxabiv1::__cxa_init_primary_exception(
   __e, const_cast<std::type_info*>(&typeid(_Ex)),
   __exception_ptr::__dest_thunk<_Ex2>);
      try
 {
   ::new (__e) _Ex2(__ex);
   return exception_ptr(__e);
 }
      catch(...)
 {
   __cxxabiv1::__cxa_free_exception(__e);
   return current_exception();
 }
# 276 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_ptr.h" 3
    }
# 290 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_ptr.h" 3
}

}
# 169 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/nested_exception.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/nested_exception.h" 3
extern "C++" {

namespace std __attribute__ ((__visibility__ ("default")))
{
# 59 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/nested_exception.h" 3
  class nested_exception
  {
    exception_ptr _M_ptr;

  public:

    nested_exception() noexcept : _M_ptr(current_exception()) { }

    nested_exception(const nested_exception&) noexcept = default;

    nested_exception& operator=(const nested_exception&) noexcept = default;

    virtual ~nested_exception() noexcept;


    [[noreturn]]
    void
    rethrow_nested() const
    {
      if (_M_ptr)
 rethrow_exception(_M_ptr);
      std::terminate();
    }


    exception_ptr
    nested_ptr() const noexcept
    { return _M_ptr; }
  };



  template<typename _Except>
    struct _Nested_exception : public _Except, public nested_exception
    {
      explicit _Nested_exception(const _Except& __ex)
      : _Except(__ex)
      { }

      explicit _Nested_exception(_Except&& __ex)
      : _Except(static_cast<_Except&&>(__ex))
      { }
    };





  template<typename _Tp>
    [[noreturn]]
    inline void
    __throw_with_nested_impl(_Tp&& __t, true_type)
    {
      throw _Nested_exception<__remove_cvref_t<_Tp>>{std::forward<_Tp>(__t)};
    }

  template<typename _Tp>
    [[noreturn]]
    inline void
    __throw_with_nested_impl(_Tp&& __t, false_type)
    { throw std::forward<_Tp>(__t); }
# 145 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/nested_exception.h" 3
  template<typename _Tp>
    [[noreturn]]
    inline void
    throw_with_nested(_Tp&& __t)
    {
      using _Up = typename decay<_Tp>::type;
      using _CopyConstructible
 = __and_<is_copy_constructible<_Up>, is_move_constructible<_Up>>;
      static_assert(_CopyConstructible::value,
   "throw_with_nested argument must be CopyConstructible");
# 163 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/nested_exception.h" 3
      using __nest = __and_<is_class<_Up>, __bool_constant<!__is_final(_Up)>,
       __not_<is_base_of<nested_exception, _Up>>>;
      std::__throw_with_nested_impl(std::forward<_Tp>(__t), __nest{});

    }





  template<typename _Ex>
    inline void
    __rethrow_if_nested_impl(const _Ex* __ptr, true_type)
    {
      if (auto __ne_ptr = dynamic_cast<const nested_exception*>(__ptr))
 __ne_ptr->rethrow_nested();
    }


  inline void
  __rethrow_if_nested_impl(const void*, false_type)
  { }
# 203 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/nested_exception.h" 3
  template<typename _Ex>



    inline void
    rethrow_if_nested(const _Ex& __ex)
    {
      const _Ex* __ptr = __builtin_addressof(__ex);


      using __cast = __and_<is_polymorphic<_Ex>,
       __or_<__not_<is_base_of<nested_exception, _Ex>>,
      is_convertible<_Ex*, nested_exception*>>>;





      std::__rethrow_if_nested_impl(__ptr, __cast{});
# 235 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/nested_exception.h" 3
    }


}

}
# 170 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 2 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/concurrence.h" 2 3





#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{







  enum _Lock_policy { _S_single, _S_mutex, _S_atomic };



  const _Lock_policy __default_lock_policy =



  _S_atomic;






  class __concurrence_lock_error : public std::exception
  {
  public:
    virtual char const*
    what() const throw()
    { return "__gnu_cxx::__concurrence_lock_error"; }
  };

  class __concurrence_unlock_error : public std::exception
  {
  public:
    virtual char const*
    what() const throw()
    { return "__gnu_cxx::__concurrence_unlock_error"; }
  };

  class __concurrence_broadcast_error : public std::exception
  {
  public:
    virtual char const*
    what() const throw()
    { return "__gnu_cxx::__concurrence_broadcast_error"; }
  };

  class __concurrence_wait_error : public std::exception
  {
  public:
    virtual char const*
    what() const throw()
    { return "__gnu_cxx::__concurrence_wait_error"; }
  };


  inline void
  __throw_concurrence_lock_error()
  { (throw (__concurrence_lock_error())); }

  inline void
  __throw_concurrence_unlock_error()
  { (throw (__concurrence_unlock_error())); }


  inline void
  __throw_concurrence_broadcast_error()
  { (throw (__concurrence_broadcast_error())); }

  inline void
  __throw_concurrence_wait_error()
  { (throw (__concurrence_wait_error())); }


  class __mutex
  {
  private:

    __gthread_mutex_t _M_mutex = { { 0, 0, 0, 0, 0, 0, 0, { 0, 0 } } };




    __mutex(const __mutex&);
    __mutex& operator=(const __mutex&);

  public:
    __mutex()
    {




    }
# 149 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/concurrence.h" 3
    void lock()
    {

      if (__gthread_active_p())
 {
   if (__gthread_mutex_lock(&_M_mutex) != 0)
     __throw_concurrence_lock_error();
 }

    }

    void unlock()
    {

      if (__gthread_active_p())
 {
   if (__gthread_mutex_unlock(&_M_mutex) != 0)
     __throw_concurrence_unlock_error();
 }

    }

    __gthread_mutex_t* gthread_mutex(void)
      { return &_M_mutex; }
  };

  class __recursive_mutex
  {
  private:

    __gthread_recursive_mutex_t _M_mutex = { { 0, 0, 0, 0, PTHREAD_MUTEX_RECURSIVE_NP, 0, 0, { 0, 0 } } };




    __recursive_mutex(const __recursive_mutex&);
    __recursive_mutex& operator=(const __recursive_mutex&);

  public:
    __recursive_mutex()
    {




    }
# 204 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/concurrence.h" 3
    void lock()
    {

      if (__gthread_active_p())
 {
   if (__gthread_recursive_mutex_lock(&_M_mutex) != 0)
     __throw_concurrence_lock_error();
 }

    }

    void unlock()
    {

      if (__gthread_active_p())
 {
   if (__gthread_recursive_mutex_unlock(&_M_mutex) != 0)
     __throw_concurrence_unlock_error();
 }

    }

    __gthread_recursive_mutex_t* gthread_recursive_mutex(void)
    { return &_M_mutex; }
  };




  class __scoped_lock
  {
  public:
    typedef __mutex __mutex_type;

  private:
    __mutex_type& _M_device;

    __scoped_lock(const __scoped_lock&);
    __scoped_lock& operator=(const __scoped_lock&);

  public:
    explicit __scoped_lock(__mutex_type& __name) : _M_device(__name)
    { _M_device.lock(); }

    ~__scoped_lock() throw()
    { _M_device.unlock(); }
  };


  class __cond
  {
  private:

    __gthread_cond_t _M_cond = { { 0, 0, 0, 0, 0, (void *) 0, 0, 0 } };




    __cond(const __cond&);
    __cond& operator=(const __cond&);

  public:
    __cond()
    {




    }
# 282 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/concurrence.h" 3
    void broadcast()
    {

      if (__gthread_active_p())
 {
   if (__gthread_cond_broadcast(&_M_cond) != 0)
     __throw_concurrence_broadcast_error();
 }

    }

    void wait(__mutex *mutex)
    {

      {
   if (__gthread_cond_wait(&_M_cond, mutex->gthread_mutex()) != 0)
     __throw_concurrence_wait_error();
      }

    }

    void wait_recursive(__recursive_mutex *mutex)
    {

      {
   if (__gthread_cond_wait_recursive(&_M_cond,
         mutex->gthread_recursive_mutex())
       != 0)
     __throw_concurrence_wait_error();
      }

    }
  };



}

#pragma GCC diagnostic pop
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 2 3







namespace std __attribute__ ((__visibility__ ("default")))
{



 
# 75 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
  template<typename> class auto_ptr;
#pragma GCC diagnostic pop






  class bad_weak_ptr : public std::exception
  {
  public:
    virtual char const* what() const noexcept;

    virtual ~bad_weak_ptr() noexcept;
  };


  inline void
  __throw_bad_weak_ptr()
  { (throw (bad_weak_ptr())); }

  using __gnu_cxx::_Lock_policy;
  using __gnu_cxx::__default_lock_policy;
  using __gnu_cxx::_S_single;
  using __gnu_cxx::_S_mutex;
  using __gnu_cxx::_S_atomic;


  template<_Lock_policy _Lp>
    class _Mutex_base
    {
    protected:

      enum { _S_need_barriers = 0 };
    };

  template<>
    class _Mutex_base<_S_mutex>
    : public __gnu_cxx::__mutex
    {
    protected:



      enum { _S_need_barriers = 1 };
    };

  template<_Lock_policy _Lp = __default_lock_policy>
    class _Sp_counted_base
    : public _Mutex_base<_Lp>
    {
    public:
      _Sp_counted_base() noexcept
      : _M_use_count(1), _M_weak_count(1) { }

      virtual
      ~_Sp_counted_base() noexcept
      { }



      virtual void
      _M_dispose() noexcept = 0;


      virtual void
      _M_destroy() noexcept
      { delete this; }

      virtual void*
      _M_get_deleter(const std::type_info&) noexcept = 0;


      void
      _M_add_ref_copy()
      { __gnu_cxx::__atomic_add_dispatch(&_M_use_count, 1); }


      void
      _M_add_ref_lock()
      {
 if (!_M_add_ref_lock_nothrow())
   __throw_bad_weak_ptr();
      }


      bool
      _M_add_ref_lock_nothrow() noexcept;


      void
      _M_release() noexcept;


      void
      _M_release_last_use() noexcept
      {
 ;
 _M_dispose();




 if (_Mutex_base<_Lp>::_S_need_barriers)
   {
     __atomic_thread_fence (4);
   }


 ;
 if (__gnu_cxx::__exchange_and_add_dispatch(&_M_weak_count,
         -1) == 1)
   {
     ;
     _M_destroy();
   }
      }


      __attribute__((__noinline__))
      void
      _M_release_last_use_cold() noexcept
      { _M_release_last_use(); }


      void
      _M_weak_add_ref() noexcept
      { __gnu_cxx::__atomic_add_dispatch(&_M_weak_count, 1); }


      void
      _M_weak_release() noexcept
      {

        ;
 if (__gnu_cxx::__exchange_and_add_dispatch(&_M_weak_count, -1) == 1)
   {
            ;
     if (_Mutex_base<_Lp>::_S_need_barriers)
       {


  __atomic_thread_fence (4);
       }
     _M_destroy();
   }
      }

      long
      _M_get_use_count() const noexcept
      {


        return __atomic_load_n(&_M_use_count, 0);
      }

    private:
      _Sp_counted_base(_Sp_counted_base const&) = delete;
      _Sp_counted_base& operator=(_Sp_counted_base const&) = delete;

      _Atomic_word _M_use_count;
      _Atomic_word _M_weak_count;
    };

  template<>
    inline bool
    _Sp_counted_base<_S_single>::
    _M_add_ref_lock_nothrow() noexcept
    {
      if (_M_use_count == 0)
 return false;
      ++_M_use_count;
      return true;
    }

  template<>
    inline bool
    _Sp_counted_base<_S_mutex>::
    _M_add_ref_lock_nothrow() noexcept
    {
      __gnu_cxx::__scoped_lock sentry(*this);
      if (__gnu_cxx::__exchange_and_add_dispatch(&_M_use_count, 1) == 0)
 {
   _M_use_count = 0;
   return false;
 }
      return true;
    }

  template<>
    inline bool
    _Sp_counted_base<_S_atomic>::
    _M_add_ref_lock_nothrow() noexcept
    {

      _Atomic_word __count = _M_get_use_count();
      do
 {
   if (__count == 0)
     return false;


 }
      while (!__atomic_compare_exchange_n(&_M_use_count, &__count, __count + 1,
       true, 4,
       0));
      return true;
    }

  template<>
    inline void
    _Sp_counted_base<_S_single>::_M_add_ref_copy()
    { ++_M_use_count; }

  template<>
    inline void
    _Sp_counted_base<_S_single>::_M_release() noexcept
    {
      if (--_M_use_count == 0)
        {
          _M_dispose();
          if (--_M_weak_count == 0)
            _M_destroy();
        }
    }

  template<>
    inline void
    _Sp_counted_base<_S_mutex>::_M_release() noexcept
    {

      ;
      if (__gnu_cxx::__exchange_and_add_dispatch(&_M_use_count, -1) == 1)
 {
   _M_release_last_use();
 }
    }

  template<>
    inline void
    _Sp_counted_base<_S_atomic>::_M_release() noexcept
    {
      ;

      constexpr bool __lock_free
 = __atomic_always_lock_free(sizeof(long long), 0)
 && __atomic_always_lock_free(sizeof(_Atomic_word), 0);
      constexpr bool __double_word
 = sizeof(long long) == 2 * sizeof(_Atomic_word);


      constexpr bool __aligned = __alignof(long long) <= alignof(void*);
      if (__lock_free && __double_word && __aligned)
 {
   constexpr int __wordbits = 8 * sizeof(_Atomic_word);
   constexpr int __shiftbits = __double_word ? __wordbits : 0;
   constexpr long long __unique_ref = 1LL + (1LL << __shiftbits);
   auto __both_counts = reinterpret_cast<long long*>(&_M_use_count);

   ;
   if (__atomic_load_n(__both_counts, 2) == __unique_ref)
     {




       _M_weak_count = _M_use_count = 0;
       ;
       ;
       _M_dispose();
       _M_destroy();
       return;
     }
   if (__gnu_cxx::__exchange_and_add_dispatch(&_M_use_count, -1) == 1)
     [[__unlikely__]]
     {
       _M_release_last_use_cold();
       return;
     }
 }
      else

      if (__gnu_cxx::__exchange_and_add_dispatch(&_M_use_count, -1) == 1)
 {
   _M_release_last_use();
 }
    }

  template<>
    inline void
    _Sp_counted_base<_S_single>::_M_weak_add_ref() noexcept
    { ++_M_weak_count; }

  template<>
    inline void
    _Sp_counted_base<_S_single>::_M_weak_release() noexcept
    {
      if (--_M_weak_count == 0)
        _M_destroy();
    }

  template<>
    inline long
    _Sp_counted_base<_S_single>::_M_get_use_count() const noexcept
    { return _M_use_count; }



  template<typename _Tp, _Lock_policy _Lp = __default_lock_policy>
    class __shared_ptr;

  template<typename _Tp, _Lock_policy _Lp = __default_lock_policy>
    class __weak_ptr;

  template<typename _Tp, _Lock_policy _Lp = __default_lock_policy>
    class __enable_shared_from_this;

  template<typename _Tp>
    class shared_ptr;

  template<typename _Tp>
    class weak_ptr;

  template<typename _Tp>
    struct owner_less;

  template<typename _Tp>
    class enable_shared_from_this;

  template<_Lock_policy _Lp = __default_lock_policy>
    class __weak_count;

  template<_Lock_policy _Lp = __default_lock_policy>
    class __shared_count;







  template<typename _Ptr, _Lock_policy _Lp>
    class _Sp_counted_ptr final : public _Sp_counted_base<_Lp>
    {
    public:
      explicit
      _Sp_counted_ptr(_Ptr __p) noexcept
      : _M_ptr(__p) { }

      virtual void
      _M_dispose() noexcept
      { delete _M_ptr; }

      virtual void
      _M_destroy() noexcept
      { delete this; }

      virtual void*
      _M_get_deleter(const std::type_info&) noexcept
      { return nullptr; }

      _Sp_counted_ptr(const _Sp_counted_ptr&) = delete;
      _Sp_counted_ptr& operator=(const _Sp_counted_ptr&) = delete;

    private:
      _Ptr _M_ptr;
    };

  template<>
    inline void
    _Sp_counted_ptr<nullptr_t, _S_single>::_M_dispose() noexcept { }

  template<>
    inline void
    _Sp_counted_ptr<nullptr_t, _S_mutex>::_M_dispose() noexcept { }

  template<>
    inline void
    _Sp_counted_ptr<nullptr_t, _S_atomic>::_M_dispose() noexcept { }






  template<int _Nm, typename _Tp,
    bool __use_ebo = !__is_final(_Tp) && __is_empty(_Tp)>
    struct _Sp_ebo_helper;


  template<int _Nm, typename _Tp>
    struct _Sp_ebo_helper<_Nm, _Tp, true> : private _Tp
    {
      explicit _Sp_ebo_helper(const _Tp& __tp) : _Tp(__tp) { }
      explicit _Sp_ebo_helper(_Tp&& __tp) : _Tp(std::move(__tp)) { }

      static _Tp&
      _S_get(_Sp_ebo_helper& __eboh) { return static_cast<_Tp&>(__eboh); }
    };


  template<int _Nm, typename _Tp>
    struct _Sp_ebo_helper<_Nm, _Tp, false>
    {
      explicit _Sp_ebo_helper(const _Tp& __tp) : _M_tp(__tp) { }
      explicit _Sp_ebo_helper(_Tp&& __tp) : _M_tp(std::move(__tp)) { }

      static _Tp&
      _S_get(_Sp_ebo_helper& __eboh)
      { return __eboh._M_tp; }

    private:
      _Tp _M_tp;
    };


  template<typename _Ptr, typename _Deleter, typename _Alloc, _Lock_policy _Lp>
    class _Sp_counted_deleter final : public _Sp_counted_base<_Lp>
    {
      class _Impl : _Sp_ebo_helper<0, _Deleter>, _Sp_ebo_helper<1, _Alloc>
      {
 typedef _Sp_ebo_helper<0, _Deleter> _Del_base;
 typedef _Sp_ebo_helper<1, _Alloc> _Alloc_base;

      public:
 _Impl(_Ptr __p, _Deleter __d, const _Alloc& __a) noexcept
 : _Del_base(std::move(__d)), _Alloc_base(__a), _M_ptr(__p)
 { }

 _Deleter& _M_del() noexcept { return _Del_base::_S_get(*this); }
 _Alloc& _M_alloc() noexcept { return _Alloc_base::_S_get(*this); }

 _Ptr _M_ptr;
      };

    public:
      using __allocator_type = __alloc_rebind<_Alloc, _Sp_counted_deleter>;


      _Sp_counted_deleter(_Ptr __p, _Deleter __d) noexcept
      : _M_impl(__p, std::move(__d), _Alloc()) { }


      _Sp_counted_deleter(_Ptr __p, _Deleter __d, const _Alloc& __a) noexcept
      : _M_impl(__p, std::move(__d), __a) { }

      ~_Sp_counted_deleter() noexcept { }

      virtual void
      _M_dispose() noexcept
      { _M_impl._M_del()(_M_impl._M_ptr); }

      virtual void
      _M_destroy() noexcept
      {
 __allocator_type __a(_M_impl._M_alloc());
 __allocated_ptr<__allocator_type> __guard_ptr{ __a, this };
 this->~_Sp_counted_deleter();
      }

      virtual void*
      _M_get_deleter(const type_info& __ti [[__gnu__::__unused__]]) noexcept
      {



        return __ti == typeid(_Deleter)
   ? std::__addressof(_M_impl._M_del())
   : nullptr;



      }

    private:



      _Impl _M_impl;
    };



  struct _Sp_make_shared_tag
  {
  private:
    template<typename _Tp, typename _Alloc, _Lock_policy _Lp>
      friend class _Sp_counted_ptr_inplace;

    static const type_info&
    _S_ti() noexcept __attribute__ ((__visibility__ ("default")))
    {
      alignas(type_info) static constexpr char __tag[sizeof(type_info)] = { };
      return reinterpret_cast<const type_info&>(__tag);
    }

    static bool _S_eq(const type_info&) noexcept;
  };

  template<typename _Alloc>
    struct _Sp_alloc_shared_tag
    {
      const _Alloc& _M_a;
    };

  template<typename _Tp, typename _Alloc, _Lock_policy _Lp>
    class _Sp_counted_ptr_inplace final : public _Sp_counted_base<_Lp>
    {
      class _Impl : _Sp_ebo_helper<0, _Alloc>
      {
 typedef _Sp_ebo_helper<0, _Alloc> _A_base;

      public:
 explicit _Impl(_Alloc __a) noexcept : _A_base(__a) { }

 _Alloc& _M_alloc() noexcept { return _A_base::_S_get(*this); }

 __gnu_cxx::__aligned_buffer<__remove_cv_t<_Tp>> _M_storage;
      };

    public:
      using __allocator_type = __alloc_rebind<_Alloc, _Sp_counted_ptr_inplace>;


      template<typename... _Args>
 _Sp_counted_ptr_inplace(_Alloc __a, _Args&&... __args)
 : _M_impl(__a)
 {


   allocator_traits<_Alloc>::construct(__a, _M_ptr(),
       std::forward<_Args>(__args)...);
 }

      ~_Sp_counted_ptr_inplace() noexcept { }

      virtual void
      _M_dispose() noexcept
      {
 allocator_traits<_Alloc>::destroy(_M_impl._M_alloc(), _M_ptr());
      }


      virtual void
      _M_destroy() noexcept
      {
 __allocator_type __a(_M_impl._M_alloc());
 __allocated_ptr<__allocator_type> __guard_ptr{ __a, this };
 this->~_Sp_counted_ptr_inplace();
      }

    private:
      friend class __shared_count<_Lp>;



      virtual void*
      _M_get_deleter(const std::type_info& __ti) noexcept override
      {




 if (&__ti == &_Sp_make_shared_tag::_S_ti()
     ||

     __ti == typeid(_Sp_make_shared_tag)



    )
   return _M_ptr();
 return nullptr;
      }

      __remove_cv_t<_Tp>*
      _M_ptr() noexcept { return _M_impl._M_storage._M_ptr(); }

      _Impl _M_impl;
    };
# 884 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 3
  struct __sp_array_delete
  {
    template<typename _Yp>
      void operator()(_Yp* __p) const { delete[] __p; }
  };

  template<_Lock_policy _Lp>
    class __shared_count
    {

      template<typename _Tp>
 struct __not_alloc_shared_tag { using type = void; };

      template<typename _Tp>
 struct __not_alloc_shared_tag<_Sp_alloc_shared_tag<_Tp>> { };






    public:
      constexpr __shared_count() noexcept : _M_pi(0)
      { }

      template<typename _Ptr>
        explicit
 __shared_count(_Ptr __p) : _M_pi(0)
 {
   try
     {
       _M_pi = new _Sp_counted_ptr<_Ptr, _Lp>(__p);
     }
   catch(...)
     {
       delete __p;
       throw;
     }
 }

      template<typename _Ptr>
 __shared_count(_Ptr __p, false_type)
 : __shared_count(__p)
 { }

      template<typename _Ptr>
 __shared_count(_Ptr __p, true_type)
 : __shared_count(__p, __sp_array_delete{}, allocator<void>())
 { }

      template<typename _Ptr, typename _Deleter,
        typename = typename __not_alloc_shared_tag<_Deleter>::type>
 __shared_count(_Ptr __p, _Deleter __d)
 : __shared_count(__p, std::move(__d), allocator<void>())
 { }

      template<typename _Ptr, typename _Deleter, typename _Alloc,
        typename = typename __not_alloc_shared_tag<_Deleter>::type>
 __shared_count(_Ptr __p, _Deleter __d, _Alloc __a) : _M_pi(0)
 {
   typedef _Sp_counted_deleter<_Ptr, _Deleter, _Alloc, _Lp> _Sp_cd_type;
   try
     {
       typename _Sp_cd_type::__allocator_type __a2(__a);
       auto __guard = std::__allocate_guarded(__a2);
       _Sp_cd_type* __mem = __guard.get();
       ::new (__mem) _Sp_cd_type(__p, std::move(__d), std::move(__a));
       _M_pi = __mem;
       __guard = nullptr;
     }
   catch(...)
     {
       __d(__p);
       throw;
     }
 }

      template<typename _Tp, typename _Alloc, typename... _Args>
 __shared_count(_Tp*& __p, _Sp_alloc_shared_tag<_Alloc> __a,
         _Args&&... __args)
 {
   using _Tp2 = __remove_cv_t<_Tp>;
   using _Sp_cp_type = _Sp_counted_ptr_inplace<_Tp2, _Alloc, _Lp>;
   typename _Sp_cp_type::__allocator_type __a2(__a._M_a);
   auto __guard = std::__allocate_guarded(__a2);
   _Sp_cp_type* __mem = __guard.get();
   auto __pi = ::new (__mem)
     _Sp_cp_type(__a._M_a, std::forward<_Args>(__args)...);
   __guard = nullptr;
   _M_pi = __pi;
   __p = __pi->_M_ptr();
 }
# 1023 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"

      template<typename _Tp>
        explicit
 __shared_count(std::auto_ptr<_Tp>&& __r);
#pragma GCC diagnostic pop



      template<typename _Tp, typename _Del>
        explicit
 __shared_count(std::unique_ptr<_Tp, _Del>&& __r) : _M_pi(0)
 {


   if (__r.get() == nullptr)
     return;

   using _Ptr = typename unique_ptr<_Tp, _Del>::pointer;
   using _Del2 = __conditional_t<is_reference<_Del>::value,
       reference_wrapper<typename remove_reference<_Del>::type>,
       _Del>;
   using _Sp_cd_type
     = _Sp_counted_deleter<_Ptr, _Del2, allocator<void>, _Lp>;
   using _Alloc = allocator<_Sp_cd_type>;
   using _Alloc_traits = allocator_traits<_Alloc>;
   _Alloc __a;
   _Sp_cd_type* __mem = _Alloc_traits::allocate(__a, 1);



   _Alloc_traits::construct(__a, __mem, __r.release(),
       std::forward<_Del>(__r.get_deleter()));
   _M_pi = __mem;
 }


      explicit __shared_count(const __weak_count<_Lp>& __r);


      explicit
      __shared_count(const __weak_count<_Lp>& __r, std::nothrow_t) noexcept;

      ~__shared_count() noexcept
      {
 if (_M_pi != nullptr)
   _M_pi->_M_release();
      }

      __shared_count(const __shared_count& __r) noexcept
      : _M_pi(__r._M_pi)
      {
 if (_M_pi != nullptr)
   _M_pi->_M_add_ref_copy();
      }

      __shared_count&
      operator=(const __shared_count& __r) noexcept
      {
 _Sp_counted_base<_Lp>* __tmp = __r._M_pi;
 if (__tmp != _M_pi)
   {
     if (__tmp != nullptr)
       __tmp->_M_add_ref_copy();
     if (_M_pi != nullptr)
       _M_pi->_M_release();
     _M_pi = __tmp;
   }
 return *this;
      }

      void
      _M_swap(__shared_count& __r) noexcept
      {
 _Sp_counted_base<_Lp>* __tmp = __r._M_pi;
 __r._M_pi = _M_pi;
 _M_pi = __tmp;
      }

      long
      _M_get_use_count() const noexcept
      { return _M_pi ? _M_pi->_M_get_use_count() : 0; }

      bool
      _M_unique() const noexcept
      { return this->_M_get_use_count() == 1; }

      void*
      _M_get_deleter(const std::type_info& __ti) const noexcept
      { return _M_pi ? _M_pi->_M_get_deleter(__ti) : nullptr; }

      bool
      _M_less(const __shared_count& __rhs) const noexcept
      { return std::less<_Sp_counted_base<_Lp>*>()(this->_M_pi, __rhs._M_pi); }

      bool
      _M_less(const __weak_count<_Lp>& __rhs) const noexcept
      { return std::less<_Sp_counted_base<_Lp>*>()(this->_M_pi, __rhs._M_pi); }


      friend inline bool
      operator==(const __shared_count& __a, const __shared_count& __b) noexcept
      { return __a._M_pi == __b._M_pi; }

    private:
      friend class __weak_count<_Lp>;







      _Sp_counted_base<_Lp>* _M_pi;
    };


  template<_Lock_policy _Lp>
    class __weak_count
    {
    public:
      constexpr __weak_count() noexcept : _M_pi(nullptr)
      { }

      __weak_count(const __shared_count<_Lp>& __r) noexcept
      : _M_pi(__r._M_pi)
      {
 if (_M_pi != nullptr)
   _M_pi->_M_weak_add_ref();
      }

      __weak_count(const __weak_count& __r) noexcept
      : _M_pi(__r._M_pi)
      {
 if (_M_pi != nullptr)
   _M_pi->_M_weak_add_ref();
      }

      __weak_count(__weak_count&& __r) noexcept
      : _M_pi(__r._M_pi)
      { __r._M_pi = nullptr; }

      ~__weak_count() noexcept
      {
 if (_M_pi != nullptr)
   _M_pi->_M_weak_release();
      }

      __weak_count&
      operator=(const __shared_count<_Lp>& __r) noexcept
      {
 _Sp_counted_base<_Lp>* __tmp = __r._M_pi;
 if (__tmp != nullptr)
   __tmp->_M_weak_add_ref();
 if (_M_pi != nullptr)
   _M_pi->_M_weak_release();
 _M_pi = __tmp;
 return *this;
      }

      __weak_count&
      operator=(const __weak_count& __r) noexcept
      {
 _Sp_counted_base<_Lp>* __tmp = __r._M_pi;
 if (__tmp != nullptr)
   __tmp->_M_weak_add_ref();
 if (_M_pi != nullptr)
   _M_pi->_M_weak_release();
 _M_pi = __tmp;
 return *this;
      }

      __weak_count&
      operator=(__weak_count&& __r) noexcept
      {
 if (_M_pi != nullptr)
   _M_pi->_M_weak_release();
 _M_pi = __r._M_pi;
        __r._M_pi = nullptr;
 return *this;
      }

      void
      _M_swap(__weak_count& __r) noexcept
      {
 _Sp_counted_base<_Lp>* __tmp = __r._M_pi;
 __r._M_pi = _M_pi;
 _M_pi = __tmp;
      }

      long
      _M_get_use_count() const noexcept
      { return _M_pi != nullptr ? _M_pi->_M_get_use_count() : 0; }

      bool
      _M_less(const __weak_count& __rhs) const noexcept
      { return std::less<_Sp_counted_base<_Lp>*>()(this->_M_pi, __rhs._M_pi); }

      bool
      _M_less(const __shared_count<_Lp>& __rhs) const noexcept
      { return std::less<_Sp_counted_base<_Lp>*>()(this->_M_pi, __rhs._M_pi); }


      friend inline bool
      operator==(const __weak_count& __a, const __weak_count& __b) noexcept
      { return __a._M_pi == __b._M_pi; }

    private:
      friend class __shared_count<_Lp>;




      _Sp_counted_base<_Lp>* _M_pi;
    };


  template<_Lock_policy _Lp>
    inline
    __shared_count<_Lp>::__shared_count(const __weak_count<_Lp>& __r)
    : _M_pi(__r._M_pi)
    {
      if (_M_pi == nullptr || !_M_pi->_M_add_ref_lock_nothrow())
 __throw_bad_weak_ptr();
    }


  template<_Lock_policy _Lp>
    inline
    __shared_count<_Lp>::
    __shared_count(const __weak_count<_Lp>& __r, std::nothrow_t) noexcept
    : _M_pi(__r._M_pi)
    {
      if (_M_pi && !_M_pi->_M_add_ref_lock_nothrow())
 _M_pi = nullptr;
    }





  template<typename _Yp_ptr, typename _Tp_ptr>
    struct __sp_compatible_with
    : false_type
    { };

  template<typename _Yp, typename _Tp>
    struct __sp_compatible_with<_Yp*, _Tp*>
    : is_convertible<_Yp*, _Tp*>::type
    { };

  template<typename _Up, size_t _Nm>
    struct __sp_compatible_with<_Up(*)[_Nm], _Up(*)[]>
    : true_type
    { };

  template<typename _Up, size_t _Nm>
    struct __sp_compatible_with<_Up(*)[_Nm], const _Up(*)[]>
    : true_type
    { };

  template<typename _Up, size_t _Nm>
    struct __sp_compatible_with<_Up(*)[_Nm], volatile _Up(*)[]>
    : true_type
    { };

  template<typename _Up, size_t _Nm>
    struct __sp_compatible_with<_Up(*)[_Nm], const volatile _Up(*)[]>
    : true_type
    { };


  template<typename _Up, size_t _Nm, typename _Yp, typename = void>
    struct __sp_is_constructible_arrN
    : false_type
    { };

  template<typename _Up, size_t _Nm, typename _Yp>
    struct __sp_is_constructible_arrN<_Up, _Nm, _Yp, __void_t<_Yp[_Nm]>>
    : is_convertible<_Yp(*)[_Nm], _Up(*)[_Nm]>::type
    { };


  template<typename _Up, typename _Yp, typename = void>
    struct __sp_is_constructible_arr
    : false_type
    { };

  template<typename _Up, typename _Yp>
    struct __sp_is_constructible_arr<_Up, _Yp, __void_t<_Yp[]>>
    : is_convertible<_Yp(*)[], _Up(*)[]>::type
    { };


  template<typename _Tp, typename _Yp>
    struct __sp_is_constructible;


  template<typename _Up, size_t _Nm, typename _Yp>
    struct __sp_is_constructible<_Up[_Nm], _Yp>
    : __sp_is_constructible_arrN<_Up, _Nm, _Yp>::type
    { };


  template<typename _Up, typename _Yp>
    struct __sp_is_constructible<_Up[], _Yp>
    : __sp_is_constructible_arr<_Up, _Yp>::type
    { };


  template<typename _Tp, typename _Yp>
    struct __sp_is_constructible
    : is_convertible<_Yp*, _Tp*>::type
    { };


  template<typename _Tp>
    [[__gnu__::__always_inline__]]
    inline _Tp*
    __shared_ptr_deref(_Tp* __p)
    {
      do { if (__builtin_expect(!bool(__p != nullptr), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h", 1345, __PRETTY_FUNCTION__, "__p != nullptr"); } while (false);
      return __p;
    }


  template<typename _Tp, _Lock_policy _Lp,
    bool = is_array<_Tp>::value, bool = is_void<_Tp>::value>
    class __shared_ptr_access
    {
    public:
      using element_type = _Tp;

      element_type&
      operator*() const noexcept
      { return *std::__shared_ptr_deref(_M_get()); }

      element_type*
      operator->() const noexcept
      {
 ;
 return _M_get();
      }

    private:
      element_type*
      _M_get() const noexcept
      { return static_cast<const __shared_ptr<_Tp, _Lp>*>(this)->get(); }
    };


  template<typename _Tp, _Lock_policy _Lp>
    class __shared_ptr_access<_Tp, _Lp, false, true>
    {
    public:
      using element_type = _Tp;

      element_type*
      operator->() const noexcept
      {
 auto __ptr = static_cast<const __shared_ptr<_Tp, _Lp>*>(this)->get();
 ;
 return __ptr;
      }
    };


  template<typename _Tp, _Lock_policy _Lp>
    class __shared_ptr_access<_Tp, _Lp, true, false>
    {
    public:
      using element_type = typename remove_extent<_Tp>::type;


      [[__deprecated__("shared_ptr<T[]>::operator* is absent from C++17")]]
      element_type&
      operator*() const noexcept
      { return *std::__shared_ptr_deref(_M_get()); }

      [[__deprecated__("shared_ptr<T[]>::operator-> is absent from C++17")]]
      element_type*
      operator->() const noexcept
      {
 ;
 return _M_get();
      }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
      element_type&
      operator[](ptrdiff_t __i) const noexcept
      {
 if constexpr (extent<_Tp>::value)
   do { if (__builtin_expect(!bool(__i < extent<_Tp>::value), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h", 1418, __PRETTY_FUNCTION__, "__i < extent<_Tp>::value"); } while (false);
 return std::__shared_ptr_deref(_M_get())[__i];
      }
#pragma GCC diagnostic pop

    private:
      element_type*
      _M_get() const noexcept
      { return static_cast<const __shared_ptr<_Tp, _Lp>*>(this)->get(); }
    };

  template<typename _Tp, _Lock_policy _Lp>
    class __shared_ptr
    : public __shared_ptr_access<_Tp, _Lp>
    {
    public:
      using element_type = typename remove_extent<_Tp>::type;

    private:

      template<typename _Yp>
 using _SafeConv
   = typename enable_if<__sp_is_constructible<_Tp, _Yp>::value>::type;


      template<typename _Yp, typename _Res = void>
 using _Compatible = typename
   enable_if<__sp_compatible_with<_Yp*, _Tp*>::value, _Res>::type;


      template<typename _Yp>
 using _Assignable = _Compatible<_Yp, __shared_ptr&>;


      template<typename _Yp, typename _Del, typename _Res = void,
        typename _Ptr = typename unique_ptr<_Yp, _Del>::pointer>
 using _UniqCompatible = __enable_if_t<__and_<
   __sp_compatible_with<_Yp*, _Tp*>,
   is_convertible<_Ptr, element_type*>,
   is_move_constructible<_Del>
   >::value, _Res>;


      template<typename _Yp, typename _Del>
 using _UniqAssignable = _UniqCompatible<_Yp, _Del, __shared_ptr&>;

    public:





      constexpr __shared_ptr() noexcept
      : _M_ptr(0), _M_refcount()
      { }

      template<typename _Yp, typename = _SafeConv<_Yp>>
 explicit
 __shared_ptr(_Yp* __p)
 : _M_ptr(__p), _M_refcount(__p, typename is_array<_Tp>::type())
 {
   static_assert( !is_void<_Yp>::value, "incomplete type" );
   static_assert( sizeof(_Yp) > 0, "incomplete type" );
   _M_enable_shared_from_this_with(__p);
 }

      template<typename _Yp, typename _Deleter, typename = _SafeConv<_Yp>>
 __shared_ptr(_Yp* __p, _Deleter __d)
 : _M_ptr(__p), _M_refcount(__p, std::move(__d))
 {
   static_assert(__is_invocable<_Deleter&, _Yp*&>::value,
       "deleter expression d(p) is well-formed");
   _M_enable_shared_from_this_with(__p);
 }

      template<typename _Yp, typename _Deleter, typename _Alloc,
        typename = _SafeConv<_Yp>>
 __shared_ptr(_Yp* __p, _Deleter __d, _Alloc __a)
 : _M_ptr(__p), _M_refcount(__p, std::move(__d), std::move(__a))
 {
   static_assert(__is_invocable<_Deleter&, _Yp*&>::value,
       "deleter expression d(p) is well-formed");
   _M_enable_shared_from_this_with(__p);
 }

      template<typename _Deleter>
 __shared_ptr(nullptr_t __p, _Deleter __d)
 : _M_ptr(0), _M_refcount(__p, std::move(__d))
 { }

      template<typename _Deleter, typename _Alloc>
        __shared_ptr(nullptr_t __p, _Deleter __d, _Alloc __a)
 : _M_ptr(0), _M_refcount(__p, std::move(__d), std::move(__a))
 { }


      template<typename _Yp>
 __shared_ptr(const __shared_ptr<_Yp, _Lp>& __r,
       element_type* __p) noexcept
 : _M_ptr(__p), _M_refcount(__r._M_refcount)
 { }


      template<typename _Yp>
 __shared_ptr(__shared_ptr<_Yp, _Lp>&& __r,
       element_type* __p) noexcept
 : _M_ptr(__p), _M_refcount()
 {
   _M_refcount._M_swap(__r._M_refcount);
   __r._M_ptr = nullptr;
 }

      __shared_ptr(const __shared_ptr&) noexcept = default;
      __shared_ptr& operator=(const __shared_ptr&) noexcept = default;
      ~__shared_ptr() = default;

      template<typename _Yp, typename = _Compatible<_Yp>>
 __shared_ptr(const __shared_ptr<_Yp, _Lp>& __r) noexcept
 : _M_ptr(__r._M_ptr), _M_refcount(__r._M_refcount)
 { }

      __shared_ptr(__shared_ptr&& __r) noexcept
      : _M_ptr(__r._M_ptr), _M_refcount()
      {
 _M_refcount._M_swap(__r._M_refcount);
 __r._M_ptr = nullptr;
      }

      template<typename _Yp, typename = _Compatible<_Yp>>
 __shared_ptr(__shared_ptr<_Yp, _Lp>&& __r) noexcept
 : _M_ptr(__r._M_ptr), _M_refcount()
 {
   _M_refcount._M_swap(__r._M_refcount);
   __r._M_ptr = nullptr;
 }

      template<typename _Yp, typename = _Compatible<_Yp>>
 explicit __shared_ptr(const __weak_ptr<_Yp, _Lp>& __r)
 : _M_refcount(__r._M_refcount)
 {


   _M_ptr = __r._M_ptr;
 }


      template<typename _Yp, typename _Del,
        typename = _UniqCompatible<_Yp, _Del>>
 __shared_ptr(unique_ptr<_Yp, _Del>&& __r)
 : _M_ptr(__r.get()), _M_refcount()
 {
   auto __raw = std::__to_address(__r.get());
   _M_refcount = __shared_count<_Lp>(std::move(__r));
   _M_enable_shared_from_this_with(__raw);
 }


    protected:

      template<typename _Tp1, typename _Del,
        typename enable_if<__and_<
   __not_<is_array<_Tp>>, is_array<_Tp1>,
          is_convertible<typename unique_ptr<_Tp1, _Del>::pointer, _Tp*>
        >::value, bool>::type = true>
 __shared_ptr(unique_ptr<_Tp1, _Del>&& __r, __sp_array_delete)
 : _M_ptr(__r.get()), _M_refcount()
 {
   auto __raw = std::__to_address(__r.get());
   _M_refcount = __shared_count<_Lp>(std::move(__r));
   _M_enable_shared_from_this_with(__raw);
 }
    public:



#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"

      template<typename _Yp, typename = _Compatible<_Yp>>
 __shared_ptr(auto_ptr<_Yp>&& __r);
#pragma GCC diagnostic pop


      constexpr __shared_ptr(nullptr_t) noexcept : __shared_ptr() { }

      template<typename _Yp>
 _Assignable<_Yp>
 operator=(const __shared_ptr<_Yp, _Lp>& __r) noexcept
 {
   _M_ptr = __r._M_ptr;
   _M_refcount = __r._M_refcount;
   return *this;
 }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
      template<typename _Yp>
 _Assignable<_Yp>
 operator=(auto_ptr<_Yp>&& __r)
 {
   __shared_ptr(std::move(__r)).swap(*this);
   return *this;
 }
#pragma GCC diagnostic pop


      __shared_ptr&
      operator=(__shared_ptr&& __r) noexcept
      {
 __shared_ptr(std::move(__r)).swap(*this);
 return *this;
      }

      template<class _Yp>
 _Assignable<_Yp>
 operator=(__shared_ptr<_Yp, _Lp>&& __r) noexcept
 {
   __shared_ptr(std::move(__r)).swap(*this);
   return *this;
 }

      template<typename _Yp, typename _Del>
 _UniqAssignable<_Yp, _Del>
 operator=(unique_ptr<_Yp, _Del>&& __r)
 {
   __shared_ptr(std::move(__r)).swap(*this);
   return *this;
 }

      void
      reset() noexcept
      { __shared_ptr().swap(*this); }

      template<typename _Yp>
 _SafeConv<_Yp>
 reset(_Yp* __p)
 {

   do { if (__builtin_expect(!bool(__p == nullptr || __p != _M_ptr), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h", 1657, __PRETTY_FUNCTION__, "__p == nullptr || __p != _M_ptr"); } while (false);
   __shared_ptr(__p).swap(*this);
 }

      template<typename _Yp, typename _Deleter>
 _SafeConv<_Yp>
 reset(_Yp* __p, _Deleter __d)
 { __shared_ptr(__p, std::move(__d)).swap(*this); }

      template<typename _Yp, typename _Deleter, typename _Alloc>
 _SafeConv<_Yp>
 reset(_Yp* __p, _Deleter __d, _Alloc __a)
        { __shared_ptr(__p, std::move(__d), std::move(__a)).swap(*this); }


      element_type*
      get() const noexcept
      { return _M_ptr; }


      explicit operator bool() const noexcept
      { return _M_ptr != nullptr; }


      bool
      unique() const noexcept
      { return _M_refcount._M_unique(); }


      long
      use_count() const noexcept
      { return _M_refcount._M_get_use_count(); }


      void
      swap(__shared_ptr<_Tp, _Lp>& __other) noexcept
      {
 std::swap(_M_ptr, __other._M_ptr);
 _M_refcount._M_swap(__other._M_refcount);
      }
# 1705 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 3
      template<typename _Tp1>
 bool
 owner_before(__shared_ptr<_Tp1, _Lp> const& __rhs) const noexcept
 { return _M_refcount._M_less(__rhs._M_refcount); }

      template<typename _Tp1>
 bool
 owner_before(__weak_ptr<_Tp1, _Lp> const& __rhs) const noexcept
 { return _M_refcount._M_less(__rhs._M_refcount); }


    protected:

      template<typename _Alloc, typename... _Args>
 __shared_ptr(_Sp_alloc_shared_tag<_Alloc> __tag, _Args&&... __args)
 : _M_ptr(), _M_refcount(_M_ptr, __tag, std::forward<_Args>(__args)...)
 { _M_enable_shared_from_this_with(_M_ptr); }

      template<typename _Tp1, _Lock_policy _Lp1, typename _Alloc,
        typename... _Args>
 friend __shared_ptr<_Tp1, _Lp1>
 __allocate_shared(const _Alloc& __a, _Args&&... __args);
# 1739 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 3
      __shared_ptr(const __weak_ptr<_Tp, _Lp>& __r, std::nothrow_t) noexcept
      : _M_refcount(__r._M_refcount, std::nothrow)
      {
 _M_ptr = _M_refcount._M_get_use_count() ? __r._M_ptr : nullptr;
      }

      friend class __weak_ptr<_Tp, _Lp>;

    private:

      template<typename _Yp>
 using __esft_base_t = decltype(__enable_shared_from_this_base(
       std::declval<const __shared_count<_Lp>&>(),
       std::declval<_Yp*>()));


      template<typename _Yp, typename = void>
 struct __has_esft_base
 : false_type { };

      template<typename _Yp>
 struct __has_esft_base<_Yp, __void_t<__esft_base_t<_Yp>>>
 : __not_<is_array<_Tp>> { };

      template<typename _Yp, typename _Yp2 = typename remove_cv<_Yp>::type>
 typename enable_if<__has_esft_base<_Yp2>::value>::type
 _M_enable_shared_from_this_with(_Yp* __p) noexcept
 {
   if (auto __base = __enable_shared_from_this_base(_M_refcount, __p))
     __base->_M_weak_assign(const_cast<_Yp2*>(__p), _M_refcount);
 }

      template<typename _Yp, typename _Yp2 = typename remove_cv<_Yp>::type>
 typename enable_if<!__has_esft_base<_Yp2>::value>::type
 _M_enable_shared_from_this_with(_Yp*) noexcept
 { }

      void*
      _M_get_deleter(const std::type_info& __ti) const noexcept
      { return _M_refcount._M_get_deleter(__ti); }

      template<typename _Tp1, _Lock_policy _Lp1> friend class __shared_ptr;
      template<typename _Tp1, _Lock_policy _Lp1> friend class __weak_ptr;

      template<typename _Del, typename _Tp1, _Lock_policy _Lp1>
 friend _Del* get_deleter(const __shared_ptr<_Tp1, _Lp1>&) noexcept;

      template<typename _Del, typename _Tp1>
 friend _Del* get_deleter(const shared_ptr<_Tp1>&) noexcept;
# 1796 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 3
      element_type* _M_ptr;
      __shared_count<_Lp> _M_refcount;
    };



  template<typename _Tp1, typename _Tp2, _Lock_policy _Lp>
    inline bool
    operator==(const __shared_ptr<_Tp1, _Lp>& __a,
        const __shared_ptr<_Tp2, _Lp>& __b) noexcept
    { return __a.get() == __b.get(); }

  template<typename _Tp, _Lock_policy _Lp>
    inline bool
    operator==(const __shared_ptr<_Tp, _Lp>& __a, nullptr_t) noexcept
    { return !__a; }
# 1828 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 3
  template<typename _Tp, _Lock_policy _Lp>
    inline bool
    operator==(nullptr_t, const __shared_ptr<_Tp, _Lp>& __a) noexcept
    { return !__a; }

  template<typename _Tp1, typename _Tp2, _Lock_policy _Lp>
    inline bool
    operator!=(const __shared_ptr<_Tp1, _Lp>& __a,
        const __shared_ptr<_Tp2, _Lp>& __b) noexcept
    { return __a.get() != __b.get(); }

  template<typename _Tp, _Lock_policy _Lp>
    inline bool
    operator!=(const __shared_ptr<_Tp, _Lp>& __a, nullptr_t) noexcept
    { return (bool)__a; }

  template<typename _Tp, _Lock_policy _Lp>
    inline bool
    operator!=(nullptr_t, const __shared_ptr<_Tp, _Lp>& __a) noexcept
    { return (bool)__a; }

  template<typename _Tp, typename _Up, _Lock_policy _Lp>
    inline bool
    operator<(const __shared_ptr<_Tp, _Lp>& __a,
       const __shared_ptr<_Up, _Lp>& __b) noexcept
    {
      using _Tp_elt = typename __shared_ptr<_Tp, _Lp>::element_type;
      using _Up_elt = typename __shared_ptr<_Up, _Lp>::element_type;
      using _Vp = typename common_type<_Tp_elt*, _Up_elt*>::type;
      return less<_Vp>()(__a.get(), __b.get());
    }

  template<typename _Tp, _Lock_policy _Lp>
    inline bool
    operator<(const __shared_ptr<_Tp, _Lp>& __a, nullptr_t) noexcept
    {
      using _Tp_elt = typename __shared_ptr<_Tp, _Lp>::element_type;
      return less<_Tp_elt*>()(__a.get(), nullptr);
    }

  template<typename _Tp, _Lock_policy _Lp>
    inline bool
    operator<(nullptr_t, const __shared_ptr<_Tp, _Lp>& __a) noexcept
    {
      using _Tp_elt = typename __shared_ptr<_Tp, _Lp>::element_type;
      return less<_Tp_elt*>()(nullptr, __a.get());
    }

  template<typename _Tp1, typename _Tp2, _Lock_policy _Lp>
    inline bool
    operator<=(const __shared_ptr<_Tp1, _Lp>& __a,
        const __shared_ptr<_Tp2, _Lp>& __b) noexcept
    { return !(__b < __a); }

  template<typename _Tp, _Lock_policy _Lp>
    inline bool
    operator<=(const __shared_ptr<_Tp, _Lp>& __a, nullptr_t) noexcept
    { return !(nullptr < __a); }

  template<typename _Tp, _Lock_policy _Lp>
    inline bool
    operator<=(nullptr_t, const __shared_ptr<_Tp, _Lp>& __a) noexcept
    { return !(__a < nullptr); }

  template<typename _Tp1, typename _Tp2, _Lock_policy _Lp>
    inline bool
    operator>(const __shared_ptr<_Tp1, _Lp>& __a,
       const __shared_ptr<_Tp2, _Lp>& __b) noexcept
    { return (__b < __a); }

  template<typename _Tp, _Lock_policy _Lp>
    inline bool
    operator>(const __shared_ptr<_Tp, _Lp>& __a, nullptr_t) noexcept
    { return nullptr < __a; }

  template<typename _Tp, _Lock_policy _Lp>
    inline bool
    operator>(nullptr_t, const __shared_ptr<_Tp, _Lp>& __a) noexcept
    { return __a < nullptr; }

  template<typename _Tp1, typename _Tp2, _Lock_policy _Lp>
    inline bool
    operator>=(const __shared_ptr<_Tp1, _Lp>& __a,
        const __shared_ptr<_Tp2, _Lp>& __b) noexcept
    { return !(__a < __b); }

  template<typename _Tp, _Lock_policy _Lp>
    inline bool
    operator>=(const __shared_ptr<_Tp, _Lp>& __a, nullptr_t) noexcept
    { return !(__a < nullptr); }

  template<typename _Tp, _Lock_policy _Lp>
    inline bool
    operator>=(nullptr_t, const __shared_ptr<_Tp, _Lp>& __a) noexcept
    { return !(nullptr < __a); }



  template<typename _Tp, _Lock_policy _Lp>
    inline void
    swap(__shared_ptr<_Tp, _Lp>& __a, __shared_ptr<_Tp, _Lp>& __b) noexcept
    { __a.swap(__b); }
# 1938 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 3
  template<typename _Tp, typename _Tp1, _Lock_policy _Lp>
    inline __shared_ptr<_Tp, _Lp>
    static_pointer_cast(const __shared_ptr<_Tp1, _Lp>& __r) noexcept
    {
      using _Sp = __shared_ptr<_Tp, _Lp>;
      return _Sp(__r, static_cast<typename _Sp::element_type*>(__r.get()));
    }






  template<typename _Tp, typename _Tp1, _Lock_policy _Lp>
    inline __shared_ptr<_Tp, _Lp>
    const_pointer_cast(const __shared_ptr<_Tp1, _Lp>& __r) noexcept
    {
      using _Sp = __shared_ptr<_Tp, _Lp>;
      return _Sp(__r, const_cast<typename _Sp::element_type*>(__r.get()));
    }






  template<typename _Tp, typename _Tp1, _Lock_policy _Lp>
    inline __shared_ptr<_Tp, _Lp>
    dynamic_pointer_cast(const __shared_ptr<_Tp1, _Lp>& __r) noexcept
    {
      using _Sp = __shared_ptr<_Tp, _Lp>;
      if (auto* __p = dynamic_cast<typename _Sp::element_type*>(__r.get()))
 return _Sp(__r, __p);
      return _Sp();
    }
# 1984 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 3
  template<typename _Tp, _Lock_policy _Lp>
    class __weak_ptr
    {
      template<typename _Yp, typename _Res = void>
 using _Compatible = typename
   enable_if<__sp_compatible_with<_Yp*, _Tp*>::value, _Res>::type;


      template<typename _Yp>
 using _Assignable = _Compatible<_Yp, __weak_ptr&>;

    public:
      using element_type = typename remove_extent<_Tp>::type;

      constexpr __weak_ptr() noexcept
      : _M_ptr(nullptr), _M_refcount()
      { }

      __weak_ptr(const __weak_ptr&) noexcept = default;

      ~__weak_ptr() = default;
# 2020 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 3
      template<typename _Yp, typename = _Compatible<_Yp>>
 __weak_ptr(const __weak_ptr<_Yp, _Lp>& __r) noexcept
 : _M_refcount(__r._M_refcount)
        { _M_ptr = __r.lock().get(); }

      template<typename _Yp, typename = _Compatible<_Yp>>
 __weak_ptr(const __shared_ptr<_Yp, _Lp>& __r) noexcept
 : _M_ptr(__r._M_ptr), _M_refcount(__r._M_refcount)
 { }

      __weak_ptr(__weak_ptr&& __r) noexcept
      : _M_ptr(__r._M_ptr), _M_refcount(std::move(__r._M_refcount))
      { __r._M_ptr = nullptr; }

      template<typename _Yp, typename = _Compatible<_Yp>>
 __weak_ptr(__weak_ptr<_Yp, _Lp>&& __r) noexcept
 : _M_ptr(__r.lock().get()), _M_refcount(std::move(__r._M_refcount))
        { __r._M_ptr = nullptr; }

      __weak_ptr&
      operator=(const __weak_ptr& __r) noexcept = default;

      template<typename _Yp>
 _Assignable<_Yp>
 operator=(const __weak_ptr<_Yp, _Lp>& __r) noexcept
 {
   _M_ptr = __r.lock().get();
   _M_refcount = __r._M_refcount;
   return *this;
 }

      template<typename _Yp>
 _Assignable<_Yp>
 operator=(const __shared_ptr<_Yp, _Lp>& __r) noexcept
 {
   _M_ptr = __r._M_ptr;
   _M_refcount = __r._M_refcount;
   return *this;
 }

      __weak_ptr&
      operator=(__weak_ptr&& __r) noexcept
      {
 __weak_ptr(std::move(__r)).swap(*this);
 return *this;
      }

      template<typename _Yp>
 _Assignable<_Yp>
 operator=(__weak_ptr<_Yp, _Lp>&& __r) noexcept
 {
   _M_ptr = __r.lock().get();
   _M_refcount = std::move(__r._M_refcount);
   __r._M_ptr = nullptr;
   return *this;
 }

      __shared_ptr<_Tp, _Lp>
      lock() const noexcept
      { return __shared_ptr<element_type, _Lp>(*this, std::nothrow); }

      long
      use_count() const noexcept
      { return _M_refcount._M_get_use_count(); }

      bool
      expired() const noexcept
      { return _M_refcount._M_get_use_count() == 0; }

      template<typename _Tp1>
 bool
 owner_before(const __shared_ptr<_Tp1, _Lp>& __rhs) const noexcept
 { return _M_refcount._M_less(__rhs._M_refcount); }

      template<typename _Tp1>
 bool
 owner_before(const __weak_ptr<_Tp1, _Lp>& __rhs) const noexcept
 { return _M_refcount._M_less(__rhs._M_refcount); }

      void
      reset() noexcept
      { __weak_ptr().swap(*this); }

      void
      swap(__weak_ptr& __s) noexcept
      {
 std::swap(_M_ptr, __s._M_ptr);
 _M_refcount._M_swap(__s._M_refcount);
      }

    private:

      void
      _M_assign(_Tp* __ptr, const __shared_count<_Lp>& __refcount) noexcept
      {
 if (use_count() == 0)
   {
     _M_ptr = __ptr;
     _M_refcount = __refcount;
   }
      }

      template<typename _Tp1, _Lock_policy _Lp1> friend class __shared_ptr;
      template<typename _Tp1, _Lock_policy _Lp1> friend class __weak_ptr;
      friend class __enable_shared_from_this<_Tp, _Lp>;
      friend class enable_shared_from_this<_Tp>;




      element_type* _M_ptr;
      __weak_count<_Lp> _M_refcount;
    };


  template<typename _Tp, _Lock_policy _Lp>
    inline void
    swap(__weak_ptr<_Tp, _Lp>& __a, __weak_ptr<_Tp, _Lp>& __b) noexcept
    { __a.swap(__b); }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
  template<typename _Tp, typename _Tp1>
    struct _Sp_owner_less : public binary_function<_Tp, _Tp, bool>
    {
      bool
      operator()(const _Tp& __lhs, const _Tp& __rhs) const noexcept
      { return __lhs.owner_before(__rhs); }

      bool
      operator()(const _Tp& __lhs, const _Tp1& __rhs) const noexcept
      { return __lhs.owner_before(__rhs); }

      bool
      operator()(const _Tp1& __lhs, const _Tp& __rhs) const noexcept
      { return __lhs.owner_before(__rhs); }
    };
#pragma GCC diagnostic pop

  template<>
    struct _Sp_owner_less<void, void>
    {
      template<typename _Tp, typename _Up>
 auto
 operator()(const _Tp& __lhs, const _Up& __rhs) const noexcept
 -> decltype(__lhs.owner_before(__rhs))
 { return __lhs.owner_before(__rhs); }

      using is_transparent = void;
    };

  template<typename _Tp, _Lock_policy _Lp>
    struct owner_less<__shared_ptr<_Tp, _Lp>>
    : public _Sp_owner_less<__shared_ptr<_Tp, _Lp>, __weak_ptr<_Tp, _Lp>>
    { };

  template<typename _Tp, _Lock_policy _Lp>
    struct owner_less<__weak_ptr<_Tp, _Lp>>
    : public _Sp_owner_less<__weak_ptr<_Tp, _Lp>, __shared_ptr<_Tp, _Lp>>
    { };


  template<typename _Tp, _Lock_policy _Lp>
    class __enable_shared_from_this
    {
    protected:
      constexpr __enable_shared_from_this() noexcept { }

      __enable_shared_from_this(const __enable_shared_from_this&) noexcept { }

      __enable_shared_from_this&
      operator=(const __enable_shared_from_this&) noexcept
      { return *this; }

      ~__enable_shared_from_this() { }

    public:
      __shared_ptr<_Tp, _Lp>
      shared_from_this()
      { return __shared_ptr<_Tp, _Lp>(this->_M_weak_this); }

      __shared_ptr<const _Tp, _Lp>
      shared_from_this() const
      { return __shared_ptr<const _Tp, _Lp>(this->_M_weak_this); }
# 2215 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_base.h" 3
    private:
      template<typename _Tp1>
 void
 _M_weak_assign(_Tp1* __p, const __shared_count<_Lp>& __n) const noexcept
 { _M_weak_this._M_assign(__p, __n); }

      friend const __enable_shared_from_this*
      __enable_shared_from_this_base(const __shared_count<_Lp>&,
         const __enable_shared_from_this* __p)
      { return __p; }

      template<typename, _Lock_policy>
 friend class __shared_ptr;

      mutable __weak_ptr<_Tp, _Lp> _M_weak_this;
    };

  template<typename _Tp, _Lock_policy _Lp = __default_lock_policy,
    typename _Alloc, typename... _Args>
    inline __shared_ptr<_Tp, _Lp>
    __allocate_shared(const _Alloc& __a, _Args&&... __args)
    {
      static_assert(!is_array<_Tp>::value, "make_shared<T[]> not supported");

      return __shared_ptr<_Tp, _Lp>(_Sp_alloc_shared_tag<_Alloc>{__a},
        std::forward<_Args>(__args)...);
    }

  template<typename _Tp, _Lock_policy _Lp = __default_lock_policy,
    typename... _Args>
    inline __shared_ptr<_Tp, _Lp>
    __make_shared(_Args&&... __args)
    {
      typedef typename std::remove_const<_Tp>::type _Tp_nc;
      return std::__allocate_shared<_Tp, _Lp>(std::allocator<_Tp_nc>(),
           std::forward<_Args>(__args)...);
    }


  template<typename _Tp, _Lock_policy _Lp>
    struct hash<__shared_ptr<_Tp, _Lp>>
    : public __hash_base<size_t, __shared_ptr<_Tp, _Lp>>
    {
      size_t
      operator()(const __shared_ptr<_Tp, _Lp>& __s) const noexcept
      {
 return hash<typename __shared_ptr<_Tp, _Lp>::element_type*>()(
     __s.get());
      }
    };


}
# 54 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
  template<typename _Ch, typename _Tr, typename _Tp, _Lock_policy _Lp>
    inline std::basic_ostream<_Ch, _Tr>&
    operator<<(std::basic_ostream<_Ch, _Tr>& __os,
        const __shared_ptr<_Tp, _Lp>& __p)
    {
      __os << __p.get();
      return __os;
    }

  template<typename _Del, typename _Tp, _Lock_policy _Lp>
    inline _Del*
    get_deleter(const __shared_ptr<_Tp, _Lp>& __p) noexcept
    {

      return static_cast<_Del*>(__p._M_get_deleter(typeid(_Del)));



    }





  template<typename _Del, typename _Tp>
    inline _Del*
    get_deleter(const shared_ptr<_Tp>& __p) noexcept
    {

      return static_cast<_Del*>(__p._M_get_deleter(typeid(_Del)));



    }
# 111 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
  template<typename _Tp>
    using _NonArray = __enable_if_t<!is_array<_Tp>::value, _Tp>;
# 174 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
  template<typename _Tp>
    class shared_ptr : public __shared_ptr<_Tp>
    {
      template<typename... _Args>
 using _Constructible = typename enable_if<
   is_constructible<__shared_ptr<_Tp>, _Args...>::value
 >::type;

      template<typename _Arg>
 using _Assignable = typename enable_if<
   is_assignable<__shared_ptr<_Tp>&, _Arg>::value, shared_ptr&
 >::type;

    public:


      using element_type = typename __shared_ptr<_Tp>::element_type;
# 201 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
      constexpr shared_ptr() noexcept : __shared_ptr<_Tp>() { }

      shared_ptr(const shared_ptr&) noexcept = default;







      template<typename _Yp, typename = _Constructible<_Yp*>>
 explicit
 shared_ptr(_Yp* __p) : __shared_ptr<_Tp>(__p) { }
# 228 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
      template<typename _Yp, typename _Deleter,
        typename = _Constructible<_Yp*, _Deleter>>
 shared_ptr(_Yp* __p, _Deleter __d)
        : __shared_ptr<_Tp>(__p, std::move(__d)) { }
# 246 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
      template<typename _Deleter>
 shared_ptr(nullptr_t __p, _Deleter __d)
        : __shared_ptr<_Tp>(__p, std::move(__d)) { }
# 265 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
      template<typename _Yp, typename _Deleter, typename _Alloc,
        typename = _Constructible<_Yp*, _Deleter, _Alloc>>
 shared_ptr(_Yp* __p, _Deleter __d, _Alloc __a)
 : __shared_ptr<_Tp>(__p, std::move(__d), std::move(__a)) { }
# 285 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
      template<typename _Deleter, typename _Alloc>
 shared_ptr(nullptr_t __p, _Deleter __d, _Alloc __a)
 : __shared_ptr<_Tp>(__p, std::move(__d), std::move(__a)) { }
# 309 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
      template<typename _Yp>
 shared_ptr(const shared_ptr<_Yp>& __r, element_type* __p) noexcept
 : __shared_ptr<_Tp>(__r, __p) { }
# 348 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
      template<typename _Yp,
        typename = _Constructible<const shared_ptr<_Yp>&>>
 shared_ptr(const shared_ptr<_Yp>& __r) noexcept
        : __shared_ptr<_Tp>(__r) { }






      shared_ptr(shared_ptr&& __r) noexcept
      : __shared_ptr<_Tp>(std::move(__r)) { }






      template<typename _Yp, typename = _Constructible<shared_ptr<_Yp>>>
 shared_ptr(shared_ptr<_Yp>&& __r) noexcept
 : __shared_ptr<_Tp>(std::move(__r)) { }
# 378 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
      template<typename _Yp, typename = _Constructible<const weak_ptr<_Yp>&>>
 explicit shared_ptr(const weak_ptr<_Yp>& __r)
 : __shared_ptr<_Tp>(__r) { }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
      template<typename _Yp, typename = _Constructible<auto_ptr<_Yp>>>
 shared_ptr(auto_ptr<_Yp>&& __r);
#pragma GCC diagnostic pop




      template<typename _Yp, typename _Del,
        typename = _Constructible<unique_ptr<_Yp, _Del>>>
 shared_ptr(unique_ptr<_Yp, _Del>&& __r)
 : __shared_ptr<_Tp>(std::move(__r)) { }





      template<typename _Yp, typename _Del,
  _Constructible<unique_ptr<_Yp, _Del>, __sp_array_delete>* = 0>
 shared_ptr(unique_ptr<_Yp, _Del>&& __r)
 : __shared_ptr<_Tp>(std::move(__r), __sp_array_delete()) { }






      constexpr shared_ptr(nullptr_t) noexcept : shared_ptr() { }

      shared_ptr& operator=(const shared_ptr&) noexcept = default;

      template<typename _Yp>
 _Assignable<const shared_ptr<_Yp>&>
 operator=(const shared_ptr<_Yp>& __r) noexcept
 {
   this->__shared_ptr<_Tp>::operator=(__r);
   return *this;
 }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
      template<typename _Yp>
 _Assignable<auto_ptr<_Yp>>
 operator=(auto_ptr<_Yp>&& __r)
 {
   this->__shared_ptr<_Tp>::operator=(std::move(__r));
   return *this;
 }
#pragma GCC diagnostic pop


      shared_ptr&
      operator=(shared_ptr&& __r) noexcept
      {
 this->__shared_ptr<_Tp>::operator=(std::move(__r));
 return *this;
      }

      template<class _Yp>
 _Assignable<shared_ptr<_Yp>>
 operator=(shared_ptr<_Yp>&& __r) noexcept
 {
   this->__shared_ptr<_Tp>::operator=(std::move(__r));
   return *this;
 }

      template<typename _Yp, typename _Del>
 _Assignable<unique_ptr<_Yp, _Del>>
 operator=(unique_ptr<_Yp, _Del>&& __r)
 {
   this->__shared_ptr<_Tp>::operator=(std::move(__r));
   return *this;
 }

    private:

      template<typename _Alloc, typename... _Args>
 shared_ptr(_Sp_alloc_shared_tag<_Alloc> __tag, _Args&&... __args)
 : __shared_ptr<_Tp>(__tag, std::forward<_Args>(__args)...)
 { }

      template<typename _Yp, typename _Alloc, typename... _Args>
 friend shared_ptr<_NonArray<_Yp>>
 allocate_shared(const _Alloc&, _Args&&...);

      template<typename _Yp, typename... _Args>
 friend shared_ptr<_NonArray<_Yp>>
 make_shared(_Args&&...);
# 534 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
      shared_ptr(const weak_ptr<_Tp>& __r, std::nothrow_t) noexcept
      : __shared_ptr<_Tp>(__r, std::nothrow) { }

      friend class weak_ptr<_Tp>;
    };
# 552 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
  template<typename _Tp, typename _Up>
    [[__nodiscard__]] inline bool
    operator==(const shared_ptr<_Tp>& __a, const shared_ptr<_Up>& __b) noexcept
    { return __a.get() == __b.get(); }


  template<typename _Tp>
    [[__nodiscard__]] inline bool
    operator==(const shared_ptr<_Tp>& __a, nullptr_t) noexcept
    { return !__a; }
# 579 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
  template<typename _Tp>
    [[__nodiscard__]] inline bool
    operator==(nullptr_t, const shared_ptr<_Tp>& __a) noexcept
    { return !__a; }


  template<typename _Tp, typename _Up>
    [[__nodiscard__]] inline bool
    operator!=(const shared_ptr<_Tp>& __a, const shared_ptr<_Up>& __b) noexcept
    { return __a.get() != __b.get(); }


  template<typename _Tp>
    [[__nodiscard__]] inline bool
    operator!=(const shared_ptr<_Tp>& __a, nullptr_t) noexcept
    { return (bool)__a; }


  template<typename _Tp>
    [[__nodiscard__]] inline bool
    operator!=(nullptr_t, const shared_ptr<_Tp>& __a) noexcept
    { return (bool)__a; }


  template<typename _Tp, typename _Up>
    [[__nodiscard__]] inline bool
    operator<(const shared_ptr<_Tp>& __a, const shared_ptr<_Up>& __b) noexcept
    {
      using _Tp_elt = typename shared_ptr<_Tp>::element_type;
      using _Up_elt = typename shared_ptr<_Up>::element_type;
      using _Vp = typename common_type<_Tp_elt*, _Up_elt*>::type;
      return less<_Vp>()(__a.get(), __b.get());
    }


  template<typename _Tp>
    [[__nodiscard__]] inline bool
    operator<(const shared_ptr<_Tp>& __a, nullptr_t) noexcept
    {
      using _Tp_elt = typename shared_ptr<_Tp>::element_type;
      return less<_Tp_elt*>()(__a.get(), nullptr);
    }


  template<typename _Tp>
    [[__nodiscard__]] inline bool
    operator<(nullptr_t, const shared_ptr<_Tp>& __a) noexcept
    {
      using _Tp_elt = typename shared_ptr<_Tp>::element_type;
      return less<_Tp_elt*>()(nullptr, __a.get());
    }


  template<typename _Tp, typename _Up>
    [[__nodiscard__]] inline bool
    operator<=(const shared_ptr<_Tp>& __a, const shared_ptr<_Up>& __b) noexcept
    { return !(__b < __a); }


  template<typename _Tp>
    [[__nodiscard__]] inline bool
    operator<=(const shared_ptr<_Tp>& __a, nullptr_t) noexcept
    { return !(nullptr < __a); }


  template<typename _Tp>
    [[__nodiscard__]] inline bool
    operator<=(nullptr_t, const shared_ptr<_Tp>& __a) noexcept
    { return !(__a < nullptr); }


  template<typename _Tp, typename _Up>
    [[__nodiscard__]] inline bool
    operator>(const shared_ptr<_Tp>& __a, const shared_ptr<_Up>& __b) noexcept
    { return (__b < __a); }


  template<typename _Tp>
    [[__nodiscard__]] inline bool
    operator>(const shared_ptr<_Tp>& __a, nullptr_t) noexcept
    { return nullptr < __a; }


  template<typename _Tp>
    [[__nodiscard__]] inline bool
    operator>(nullptr_t, const shared_ptr<_Tp>& __a) noexcept
    { return __a < nullptr; }


  template<typename _Tp, typename _Up>
    [[__nodiscard__]] inline bool
    operator>=(const shared_ptr<_Tp>& __a, const shared_ptr<_Up>& __b) noexcept
    { return !(__a < __b); }


  template<typename _Tp>
    [[__nodiscard__]] inline bool
    operator>=(const shared_ptr<_Tp>& __a, nullptr_t) noexcept
    { return !(__a < nullptr); }


  template<typename _Tp>
    [[__nodiscard__]] inline bool
    operator>=(nullptr_t, const shared_ptr<_Tp>& __a) noexcept
    { return !(nullptr < __a); }





  template<typename _Tp>
    inline void
    swap(shared_ptr<_Tp>& __a, shared_ptr<_Tp>& __b) noexcept
    { __a.swap(__b); }




  template<typename _Tp, typename _Up>
    inline shared_ptr<_Tp>
    static_pointer_cast(const shared_ptr<_Up>& __r) noexcept
    {
      using _Sp = shared_ptr<_Tp>;
      return _Sp(__r, static_cast<typename _Sp::element_type*>(__r.get()));
    }


  template<typename _Tp, typename _Up>
    inline shared_ptr<_Tp>
    const_pointer_cast(const shared_ptr<_Up>& __r) noexcept
    {
      using _Sp = shared_ptr<_Tp>;
      return _Sp(__r, const_cast<typename _Sp::element_type*>(__r.get()));
    }


  template<typename _Tp, typename _Up>
    inline shared_ptr<_Tp>
    dynamic_pointer_cast(const shared_ptr<_Up>& __r) noexcept
    {
      using _Sp = shared_ptr<_Tp>;
      if (auto* __p = dynamic_cast<typename _Sp::element_type*>(__r.get()))
 return _Sp(__r, __p);
      return _Sp();
    }
# 809 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
  template<typename _Tp>
    class weak_ptr : public __weak_ptr<_Tp>
    {
      template<typename _Arg>
 using _Constructible = typename enable_if<
   is_constructible<__weak_ptr<_Tp>, _Arg>::value
 >::type;

      template<typename _Arg>
 using _Assignable = typename enable_if<
   is_assignable<__weak_ptr<_Tp>&, _Arg>::value, weak_ptr&
 >::type;

    public:
      constexpr weak_ptr() noexcept = default;

      template<typename _Yp,
        typename = _Constructible<const shared_ptr<_Yp>&>>
 weak_ptr(const shared_ptr<_Yp>& __r) noexcept
 : __weak_ptr<_Tp>(__r) { }

      weak_ptr(const weak_ptr&) noexcept = default;

      template<typename _Yp, typename = _Constructible<const weak_ptr<_Yp>&>>
 weak_ptr(const weak_ptr<_Yp>& __r) noexcept
 : __weak_ptr<_Tp>(__r) { }

      weak_ptr(weak_ptr&&) noexcept = default;

      template<typename _Yp, typename = _Constructible<weak_ptr<_Yp>>>
 weak_ptr(weak_ptr<_Yp>&& __r) noexcept
 : __weak_ptr<_Tp>(std::move(__r)) { }

      weak_ptr&
      operator=(const weak_ptr& __r) noexcept = default;

      template<typename _Yp>
 _Assignable<const weak_ptr<_Yp>&>
 operator=(const weak_ptr<_Yp>& __r) noexcept
 {
   this->__weak_ptr<_Tp>::operator=(__r);
   return *this;
 }

      template<typename _Yp>
 _Assignable<const shared_ptr<_Yp>&>
 operator=(const shared_ptr<_Yp>& __r) noexcept
 {
   this->__weak_ptr<_Tp>::operator=(__r);
   return *this;
 }

      weak_ptr&
      operator=(weak_ptr&& __r) noexcept = default;

      template<typename _Yp>
 _Assignable<weak_ptr<_Yp>>
 operator=(weak_ptr<_Yp>&& __r) noexcept
 {
   this->__weak_ptr<_Tp>::operator=(std::move(__r));
   return *this;
 }

      shared_ptr<_Tp>
      lock() const noexcept
      { return shared_ptr<_Tp>(*this, std::nothrow); }
    };
# 885 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
  template<typename _Tp>
    inline void
    swap(weak_ptr<_Tp>& __a, weak_ptr<_Tp>& __b) noexcept
    { __a.swap(__b); }



  template<typename _Tp = void>
    struct owner_less;


  template<>
    struct owner_less<void> : _Sp_owner_less<void, void>
    { };


  template<typename _Tp>
    struct owner_less<shared_ptr<_Tp>>
    : public _Sp_owner_less<shared_ptr<_Tp>, weak_ptr<_Tp>>
    { };


  template<typename _Tp>
    struct owner_less<weak_ptr<_Tp>>
    : public _Sp_owner_less<weak_ptr<_Tp>, shared_ptr<_Tp>>
    { };






  template<typename _Tp>
    class enable_shared_from_this
    {
    protected:
      constexpr enable_shared_from_this() noexcept { }

      enable_shared_from_this(const enable_shared_from_this&) noexcept { }

      enable_shared_from_this&
      operator=(const enable_shared_from_this&) noexcept
      { return *this; }

      ~enable_shared_from_this() { }

    public:
      shared_ptr<_Tp>
      shared_from_this()
      { return shared_ptr<_Tp>(this->_M_weak_this); }

      shared_ptr<const _Tp>
      shared_from_this() const
      { return shared_ptr<const _Tp>(this->_M_weak_this); }
# 955 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
    private:
      template<typename _Tp1>
 void
 _M_weak_assign(_Tp1* __p, const __shared_count<>& __n) const noexcept
 { _M_weak_this._M_assign(__p, __n); }


      friend const enable_shared_from_this*
      __enable_shared_from_this_base(const __shared_count<>&,
         const enable_shared_from_this* __p)
      { return __p; }

      template<typename, _Lock_policy>
 friend class __shared_ptr;

      mutable weak_ptr<_Tp> _M_weak_this;
    };
# 986 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
  template<typename _Tp, typename _Alloc, typename... _Args>
    inline shared_ptr<_NonArray<_Tp>>
    allocate_shared(const _Alloc& __a, _Args&&... __args)
    {
      return shared_ptr<_Tp>(_Sp_alloc_shared_tag<_Alloc>{__a},
        std::forward<_Args>(__args)...);
    }
# 1001 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
  template<typename _Tp, typename... _Args>
    inline shared_ptr<_NonArray<_Tp>>
    make_shared(_Args&&... __args)
    {
      using _Alloc = allocator<void>;
      _Alloc __a;
      return shared_ptr<_Tp>(_Sp_alloc_shared_tag<_Alloc>{__a},
        std::forward<_Args>(__args)...);
    }
# 1150 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3
  template<typename _Tp>
    struct hash<shared_ptr<_Tp>>
    : public __hash_base<size_t, shared_ptr<_Tp>>
    {
      size_t
      operator()(const shared_ptr<_Tp>& __s) const noexcept
      {
 return std::hash<typename shared_ptr<_Tp>::element_type*>()(__s.get());
      }
    };


  template<typename _Tp>
    constexpr bool __is_shared_ptr = false;
  template<typename _Tp>
    constexpr bool __is_shared_ptr<shared_ptr<_Tp>> = true;
# 1192 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr.h" 3

}
# 83 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_atomic.h" 1 3
# 33 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_atomic.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_lockfree_defines.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 2 3
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 51 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 82 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
  enum memory_order : int
    {
      memory_order_relaxed,
      memory_order_consume,
      memory_order_acquire,
      memory_order_release,
      memory_order_acq_rel,
      memory_order_seq_cst
    };



  enum __memory_order_modifier
    {
      __memory_order_mask = 0x0ffff,
      __memory_order_modifier_mask = 0xffff0000,
      __memory_order_hle_acquire = 0x10000,
      __memory_order_hle_release = 0x20000
    };


  constexpr memory_order
  operator|(memory_order __m, __memory_order_modifier __mod) noexcept
  {
    return memory_order(int(__m) | int(__mod));
  }

  constexpr memory_order
  operator&(memory_order __m, __memory_order_modifier __mod) noexcept
  {
    return memory_order(int(__m) & int(__mod));
  }




  constexpr memory_order
  __cmpexch_failure_order2(memory_order __m) noexcept
  {
    return __m == memory_order_acq_rel ? memory_order_acquire
      : __m == memory_order_release ? memory_order_relaxed : __m;
  }

  constexpr memory_order
  __cmpexch_failure_order(memory_order __m) noexcept
  {
    return memory_order(__cmpexch_failure_order2(__m & __memory_order_mask)
      | __memory_order_modifier(__m & __memory_order_modifier_mask));
  }

  constexpr bool
  __is_valid_cmpexch_failure_order(memory_order __m) noexcept
  {
    return (__m & __memory_order_mask) != memory_order_release
 && (__m & __memory_order_mask) != memory_order_acq_rel;
  }


  template<typename _IntTp>
    struct __atomic_base;



  inline __attribute__((__always_inline__)) void
  atomic_thread_fence(memory_order __m) noexcept
  { __atomic_thread_fence(int(__m)); }

  inline __attribute__((__always_inline__)) void
  atomic_signal_fence(memory_order __m) noexcept
  { __atomic_signal_fence(int(__m)); }


  template<typename _Tp>
    inline _Tp
    kill_dependency(_Tp __y) noexcept
    {
      _Tp __ret(__y);
      return __ret;
    }
# 172 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
  template<typename _Tp>
    struct atomic;

  template<typename _Tp>
    struct atomic<_Tp*>;



    typedef bool __atomic_flag_data_type;
# 197 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
  extern "C" {

  struct __atomic_flag_base
  {
    __atomic_flag_data_type _M_i ;
  };

  }






  struct atomic_flag : public __atomic_flag_base
  {
    atomic_flag() noexcept = default;
    ~atomic_flag() noexcept = default;
    atomic_flag(const atomic_flag&) = delete;
    atomic_flag& operator=(const atomic_flag&) = delete;
    atomic_flag& operator=(const atomic_flag&) volatile = delete;


    constexpr atomic_flag(bool __i) noexcept
      : __atomic_flag_base{ _S_init(__i) }
    { }

    inline __attribute__((__always_inline__)) bool
    test_and_set(memory_order __m = memory_order_seq_cst) noexcept
    {
      return __atomic_test_and_set (&_M_i, int(__m));
    }

    inline __attribute__((__always_inline__)) bool
    test_and_set(memory_order __m = memory_order_seq_cst) volatile noexcept
    {
      return __atomic_test_and_set (&_M_i, int(__m));
    }
# 281 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
    inline __attribute__((__always_inline__)) void
    clear(memory_order __m = memory_order_seq_cst) noexcept
    {
      memory_order __b __attribute__ ((__unused__))
 = __m & __memory_order_mask;
      do { if (__builtin_expect(!bool(__b != memory_order_consume), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 286, __PRETTY_FUNCTION__, "__b != memory_order_consume"); } while (false);
      do { if (__builtin_expect(!bool(__b != memory_order_acquire), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 287, __PRETTY_FUNCTION__, "__b != memory_order_acquire"); } while (false);
      do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 288, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);

      __atomic_clear (&_M_i, int(__m));
    }

    inline __attribute__((__always_inline__)) void
    clear(memory_order __m = memory_order_seq_cst) volatile noexcept
    {
      memory_order __b __attribute__ ((__unused__))
 = __m & __memory_order_mask;
      do { if (__builtin_expect(!bool(__b != memory_order_consume), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 298, __PRETTY_FUNCTION__, "__b != memory_order_consume"); } while (false);
      do { if (__builtin_expect(!bool(__b != memory_order_acquire), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 299, __PRETTY_FUNCTION__, "__b != memory_order_acquire"); } while (false);
      do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 300, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);

      __atomic_clear (&_M_i, int(__m));
    }

  private:
    static constexpr __atomic_flag_data_type
    _S_init(bool __i)
    { return __i ? 1 : 0; }
  };
# 337 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
  template<typename _ITp>
    struct __atomic_base
    {
      using value_type = _ITp;
      using difference_type = value_type;

    private:
      typedef _ITp __int_type;

      static constexpr int _S_alignment =
 sizeof(_ITp) > alignof(_ITp) ? sizeof(_ITp) : alignof(_ITp);

      alignas(_S_alignment) __int_type _M_i ;

    public:
      __atomic_base() noexcept = default;
      ~__atomic_base() noexcept = default;
      __atomic_base(const __atomic_base&) = delete;
      __atomic_base& operator=(const __atomic_base&) = delete;
      __atomic_base& operator=(const __atomic_base&) volatile = delete;


      constexpr __atomic_base(__int_type __i) noexcept : _M_i (__i) { }

      operator __int_type() const noexcept
      { return load(); }

      operator __int_type() const volatile noexcept
      { return load(); }

      __int_type
      operator=(__int_type __i) noexcept
      {
 store(__i);
 return __i;
      }

      __int_type
      operator=(__int_type __i) volatile noexcept
      {
 store(__i);
 return __i;
      }

      __int_type
      operator++(int) noexcept
      { return fetch_add(1); }

      __int_type
      operator++(int) volatile noexcept
      { return fetch_add(1); }

      __int_type
      operator--(int) noexcept
      { return fetch_sub(1); }

      __int_type
      operator--(int) volatile noexcept
      { return fetch_sub(1); }

      __int_type
      operator++() noexcept
      { return __atomic_add_fetch(&_M_i, 1, int(memory_order_seq_cst)); }

      __int_type
      operator++() volatile noexcept
      { return __atomic_add_fetch(&_M_i, 1, int(memory_order_seq_cst)); }

      __int_type
      operator--() noexcept
      { return __atomic_sub_fetch(&_M_i, 1, int(memory_order_seq_cst)); }

      __int_type
      operator--() volatile noexcept
      { return __atomic_sub_fetch(&_M_i, 1, int(memory_order_seq_cst)); }

      __int_type
      operator+=(__int_type __i) noexcept
      { return __atomic_add_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator+=(__int_type __i) volatile noexcept
      { return __atomic_add_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator-=(__int_type __i) noexcept
      { return __atomic_sub_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator-=(__int_type __i) volatile noexcept
      { return __atomic_sub_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator&=(__int_type __i) noexcept
      { return __atomic_and_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator&=(__int_type __i) volatile noexcept
      { return __atomic_and_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator|=(__int_type __i) noexcept
      { return __atomic_or_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator|=(__int_type __i) volatile noexcept
      { return __atomic_or_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator^=(__int_type __i) noexcept
      { return __atomic_xor_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator^=(__int_type __i) volatile noexcept
      { return __atomic_xor_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      bool
      is_lock_free() const noexcept
      {

 return __atomic_is_lock_free(sizeof(_M_i),
     reinterpret_cast<void *>(-_S_alignment));
      }

      bool
      is_lock_free() const volatile noexcept
      {

 return __atomic_is_lock_free(sizeof(_M_i),
     reinterpret_cast<void *>(-_S_alignment));
      }

      inline __attribute__((__always_inline__)) void
      store(__int_type __i, memory_order __m = memory_order_seq_cst) noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_acquire), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 474, __PRETTY_FUNCTION__, "__b != memory_order_acquire"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 475, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_consume), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 476, __PRETTY_FUNCTION__, "__b != memory_order_consume"); } while (false);

 __atomic_store_n(&_M_i, __i, int(__m));
      }

      inline __attribute__((__always_inline__)) void
      store(__int_type __i,
     memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_acquire), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 487, __PRETTY_FUNCTION__, "__b != memory_order_acquire"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 488, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_consume), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 489, __PRETTY_FUNCTION__, "__b != memory_order_consume"); } while (false);

 __atomic_store_n(&_M_i, __i, int(__m));
      }

      inline __attribute__((__always_inline__)) __int_type
      load(memory_order __m = memory_order_seq_cst) const noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_release), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 499, __PRETTY_FUNCTION__, "__b != memory_order_release"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 500, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);

 return __atomic_load_n(&_M_i, int(__m));
      }

      inline __attribute__((__always_inline__)) __int_type
      load(memory_order __m = memory_order_seq_cst) const volatile noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_release), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 510, __PRETTY_FUNCTION__, "__b != memory_order_release"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 511, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);

 return __atomic_load_n(&_M_i, int(__m));
      }

      inline __attribute__((__always_inline__)) __int_type
      exchange(__int_type __i,
        memory_order __m = memory_order_seq_cst) noexcept
      {
 return __atomic_exchange_n(&_M_i, __i, int(__m));
      }


      inline __attribute__((__always_inline__)) __int_type
      exchange(__int_type __i,
        memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 return __atomic_exchange_n(&_M_i, __i, int(__m));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_weak(__int_type& __i1, __int_type __i2,
       memory_order __m1, memory_order __m2) noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 535, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_i, &__i1, __i2, 1,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_weak(__int_type& __i1, __int_type __i2,
       memory_order __m1,
       memory_order __m2) volatile noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 546, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_i, &__i1, __i2, 1,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_weak(__int_type& __i1, __int_type __i2,
       memory_order __m = memory_order_seq_cst) noexcept
      {
 return compare_exchange_weak(__i1, __i2, __m,
         __cmpexch_failure_order(__m));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_weak(__int_type& __i1, __int_type __i2,
     memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 return compare_exchange_weak(__i1, __i2, __m,
         __cmpexch_failure_order(__m));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_strong(__int_type& __i1, __int_type __i2,
         memory_order __m1, memory_order __m2) noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 572, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_i, &__i1, __i2, 0,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_strong(__int_type& __i1, __int_type __i2,
         memory_order __m1,
         memory_order __m2) volatile noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 583, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_i, &__i1, __i2, 0,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_strong(__int_type& __i1, __int_type __i2,
         memory_order __m = memory_order_seq_cst) noexcept
      {
 return compare_exchange_strong(__i1, __i2, __m,
           __cmpexch_failure_order(__m));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_strong(__int_type& __i1, __int_type __i2,
   memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 return compare_exchange_strong(__i1, __i2, __m,
           __cmpexch_failure_order(__m));
      }
# 629 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
      inline __attribute__((__always_inline__)) __int_type
      fetch_add(__int_type __i,
  memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_add(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_add(__int_type __i,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_add(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_sub(__int_type __i,
  memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_sub(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_sub(__int_type __i,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_sub(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_and(__int_type __i,
  memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_and(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_and(__int_type __i,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_and(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_or(__int_type __i,
        memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_or(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_or(__int_type __i,
        memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_or(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_xor(__int_type __i,
  memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_xor(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_xor(__int_type __i,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_xor(&_M_i, __i, int(__m)); }
    };



  template<typename _PTp>
    struct __atomic_base<_PTp*>
    {
    private:
      typedef _PTp* __pointer_type;

      __pointer_type _M_p ;

      static constexpr ptrdiff_t
      _S_type_size(ptrdiff_t __d)
      { return __d * sizeof(_PTp); }

    public:
      __atomic_base() noexcept = default;
      ~__atomic_base() noexcept = default;
      __atomic_base(const __atomic_base&) = delete;
      __atomic_base& operator=(const __atomic_base&) = delete;
      __atomic_base& operator=(const __atomic_base&) volatile = delete;


      constexpr __atomic_base(__pointer_type __p) noexcept : _M_p (__p) { }

      operator __pointer_type() const noexcept
      { return load(); }

      operator __pointer_type() const volatile noexcept
      { return load(); }

      __pointer_type
      operator=(__pointer_type __p) noexcept
      {
 store(__p);
 return __p;
      }

      __pointer_type
      operator=(__pointer_type __p) volatile noexcept
      {
 store(__p);
 return __p;
      }

      __pointer_type
      operator++(int) noexcept
      { return fetch_add(1); }

      __pointer_type
      operator++(int) volatile noexcept
      { return fetch_add(1); }

      __pointer_type
      operator--(int) noexcept
      { return fetch_sub(1); }

      __pointer_type
      operator--(int) volatile noexcept
      { return fetch_sub(1); }

      __pointer_type
      operator++() noexcept
      { return __atomic_add_fetch(&_M_p, _S_type_size(1),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator++() volatile noexcept
      { return __atomic_add_fetch(&_M_p, _S_type_size(1),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator--() noexcept
      { return __atomic_sub_fetch(&_M_p, _S_type_size(1),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator--() volatile noexcept
      { return __atomic_sub_fetch(&_M_p, _S_type_size(1),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator+=(ptrdiff_t __d) noexcept
      { return __atomic_add_fetch(&_M_p, _S_type_size(__d),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator+=(ptrdiff_t __d) volatile noexcept
      { return __atomic_add_fetch(&_M_p, _S_type_size(__d),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator-=(ptrdiff_t __d) noexcept
      { return __atomic_sub_fetch(&_M_p, _S_type_size(__d),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator-=(ptrdiff_t __d) volatile noexcept
      { return __atomic_sub_fetch(&_M_p, _S_type_size(__d),
      int(memory_order_seq_cst)); }

      bool
      is_lock_free() const noexcept
      {

 return __atomic_is_lock_free(sizeof(_M_p),
     reinterpret_cast<void *>(-__alignof(_M_p)));
      }

      bool
      is_lock_free() const volatile noexcept
      {

 return __atomic_is_lock_free(sizeof(_M_p),
     reinterpret_cast<void *>(-__alignof(_M_p)));
      }

      inline __attribute__((__always_inline__)) void
      store(__pointer_type __p,
     memory_order __m = memory_order_seq_cst) noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;

 do { if (__builtin_expect(!bool(__b != memory_order_acquire), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 803, __PRETTY_FUNCTION__, "__b != memory_order_acquire"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 804, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_consume), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 805, __PRETTY_FUNCTION__, "__b != memory_order_consume"); } while (false);

 __atomic_store_n(&_M_p, __p, int(__m));
      }

      inline __attribute__((__always_inline__)) void
      store(__pointer_type __p,
     memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_acquire), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 816, __PRETTY_FUNCTION__, "__b != memory_order_acquire"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 817, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_consume), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 818, __PRETTY_FUNCTION__, "__b != memory_order_consume"); } while (false);

 __atomic_store_n(&_M_p, __p, int(__m));
      }

      inline __attribute__((__always_inline__)) __pointer_type
      load(memory_order __m = memory_order_seq_cst) const noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_release), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 828, __PRETTY_FUNCTION__, "__b != memory_order_release"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 829, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);

 return __atomic_load_n(&_M_p, int(__m));
      }

      inline __attribute__((__always_inline__)) __pointer_type
      load(memory_order __m = memory_order_seq_cst) const volatile noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_release), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 839, __PRETTY_FUNCTION__, "__b != memory_order_release"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 840, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);

 return __atomic_load_n(&_M_p, int(__m));
      }

      inline __attribute__((__always_inline__)) __pointer_type
      exchange(__pointer_type __p,
        memory_order __m = memory_order_seq_cst) noexcept
      {
 return __atomic_exchange_n(&_M_p, __p, int(__m));
      }


      inline __attribute__((__always_inline__)) __pointer_type
      exchange(__pointer_type __p,
        memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 return __atomic_exchange_n(&_M_p, __p, int(__m));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_weak(__pointer_type& __p1, __pointer_type __p2,
       memory_order __m1,
       memory_order __m2) noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 865, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_p, &__p1, __p2, 1,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_weak(__pointer_type& __p1, __pointer_type __p2,
       memory_order __m1,
       memory_order __m2) volatile noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 876, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_p, &__p1, __p2, 1,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_strong(__pointer_type& __p1, __pointer_type __p2,
         memory_order __m1,
         memory_order __m2) noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 887, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_p, &__p1, __p2, 0,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_strong(__pointer_type& __p1, __pointer_type __p2,
         memory_order __m1,
         memory_order __m2) volatile noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 898, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_p, &__p1, __p2, 0,
        int(__m1), int(__m2));
      }
# 929 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
      inline __attribute__((__always_inline__)) __pointer_type
      fetch_add(ptrdiff_t __d,
  memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_add(&_M_p, _S_type_size(__d), int(__m)); }

      inline __attribute__((__always_inline__)) __pointer_type
      fetch_add(ptrdiff_t __d,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_add(&_M_p, _S_type_size(__d), int(__m)); }

      inline __attribute__((__always_inline__)) __pointer_type
      fetch_sub(ptrdiff_t __d,
  memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_sub(&_M_p, _S_type_size(__d), int(__m)); }

      inline __attribute__((__always_inline__)) __pointer_type
      fetch_sub(ptrdiff_t __d,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_sub(&_M_p, _S_type_size(__d), int(__m)); }
    };

  namespace __atomic_impl
  {


    template<typename _Tp>
      constexpr bool
      __maybe_has_padding()
      {



 return !__has_unique_object_representations(_Tp)
   && !is_same<_Tp, float>::value && !is_same<_Tp, double>::value;



      }

    template<typename _Tp>
      inline __attribute__((__always_inline__)) constexpr _Tp*
      __clear_padding(_Tp& __val) noexcept
      {
 auto* __ptr = std::__addressof(__val);

 if (__atomic_impl::__maybe_has_padding<_Tp>())
   __builtin_clear_padding(__ptr);

 return __ptr;
      }


    template<typename _Tp>
      using _Val = typename remove_volatile<_Tp>::type;

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"

    template<bool _AtomicRef = false, typename _Tp>
      inline __attribute__((__always_inline__)) bool
      __compare_exchange(_Tp& __val, _Val<_Tp>& __e, _Val<_Tp>& __i,
    bool __is_weak,
    memory_order __s, memory_order __f) noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__f)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 993, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__f)"); } while (false);

 using _Vp = _Val<_Tp>;
 _Tp* const __pval = std::__addressof(__val);

 if constexpr (!__atomic_impl::__maybe_has_padding<_Vp>())
   {
     return __atomic_compare_exchange(__pval, std::__addressof(__e),
          std::__addressof(__i), __is_weak,
          int(__s), int(__f));
   }
 else if constexpr (!_AtomicRef)
   {

     _Vp* const __pi = __atomic_impl::__clear_padding(__i);

     _Vp __exp = __e;

     _Vp* const __pexp = __atomic_impl::__clear_padding(__exp);



     if (__atomic_compare_exchange(__pval, __pexp, __pi,
       __is_weak, int(__s), int(__f)))
       return true;

     __builtin_memcpy(std::__addressof(__e), __pexp, sizeof(_Vp));
     return false;
   }
 else
   {

     _Vp* const __pi = __atomic_impl::__clear_padding(__i);


     _Vp __exp = __e;


     _Vp* const __pexp = __atomic_impl::__clear_padding(__exp);
# 1043 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
     while (true)
       {

  _Vp __orig = __exp;

  if (__atomic_compare_exchange(__pval, __pexp, __pi,
           __is_weak, int(__s), int(__f)))
    return true;


  _Vp __curr = __exp;


  if (__builtin_memcmp(__atomic_impl::__clear_padding(__orig),
         __atomic_impl::__clear_padding(__curr),
         sizeof(_Vp)))
    {

      __builtin_memcpy(std::__addressof(__e), __pexp,
         sizeof(_Vp));
      return false;
    }
       }
   }
      }
#pragma GCC diagnostic pop
  }
# 2070 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3

}
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_atomic.h" 2 3
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_atomic.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 74 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_atomic.h" 3
  struct _Sp_locker
  {
    _Sp_locker(const _Sp_locker&) = delete;
    _Sp_locker& operator=(const _Sp_locker&) = delete;


    explicit
    _Sp_locker(const void*) noexcept;
    _Sp_locker(const void*, const void*) noexcept;
    ~_Sp_locker();

  private:
    unsigned char _M_key1;
    unsigned char _M_key2;



  };
# 101 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_atomic.h" 3
  template<typename _Tp, _Lock_policy _Lp>
   
    inline bool
    atomic_is_lock_free(const __shared_ptr<_Tp, _Lp>*)
    {

      return __gthread_active_p() == 0;



    }

  template<typename _Tp>
   
    inline bool
    atomic_is_lock_free(const shared_ptr<_Tp>* __p)
    { return std::atomic_is_lock_free<_Tp, __default_lock_policy>(__p); }
# 130 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_atomic.h" 3
  template<typename _Tp>
   
    inline shared_ptr<_Tp>
    atomic_load_explicit(const shared_ptr<_Tp>* __p, memory_order)
    {
      _Sp_locker __lock{__p};
      return *__p;
    }

  template<typename _Tp>
   
    inline shared_ptr<_Tp>
    atomic_load(const shared_ptr<_Tp>* __p)
    { return std::atomic_load_explicit(__p, memory_order_seq_cst); }

  template<typename _Tp, _Lock_policy _Lp>
   
    inline __shared_ptr<_Tp, _Lp>
    atomic_load_explicit(const __shared_ptr<_Tp, _Lp>* __p, memory_order)
    {
      _Sp_locker __lock{__p};
      return *__p;
    }

  template<typename _Tp, _Lock_policy _Lp>
   
    inline __shared_ptr<_Tp, _Lp>
    atomic_load(const __shared_ptr<_Tp, _Lp>* __p)
    { return std::atomic_load_explicit(__p, memory_order_seq_cst); }
# 170 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_atomic.h" 3
  template<typename _Tp>
   
    inline void
    atomic_store_explicit(shared_ptr<_Tp>* __p, shared_ptr<_Tp> __r,
     memory_order)
    {
      _Sp_locker __lock{__p};
      __p->swap(__r);
    }

  template<typename _Tp>
   
    inline void
    atomic_store(shared_ptr<_Tp>* __p, shared_ptr<_Tp> __r)
    { std::atomic_store_explicit(__p, std::move(__r), memory_order_seq_cst); }

  template<typename _Tp, _Lock_policy _Lp>
   
    inline void
    atomic_store_explicit(__shared_ptr<_Tp, _Lp>* __p,
     __shared_ptr<_Tp, _Lp> __r,
     memory_order)
    {
      _Sp_locker __lock{__p};
      __p->swap(__r);
    }

  template<typename _Tp, _Lock_policy _Lp>
   
    inline void
    atomic_store(__shared_ptr<_Tp, _Lp>* __p, __shared_ptr<_Tp, _Lp> __r)
    { std::atomic_store_explicit(__p, std::move(__r), memory_order_seq_cst); }
# 211 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_atomic.h" 3
  template<typename _Tp>
   
    inline shared_ptr<_Tp>
    atomic_exchange_explicit(shared_ptr<_Tp>* __p, shared_ptr<_Tp> __r,
        memory_order)
    {
      _Sp_locker __lock{__p};
      __p->swap(__r);
      return __r;
    }

  template<typename _Tp>
   
    inline shared_ptr<_Tp>
    atomic_exchange(shared_ptr<_Tp>* __p, shared_ptr<_Tp> __r)
    {
      return std::atomic_exchange_explicit(__p, std::move(__r),
        memory_order_seq_cst);
    }

  template<typename _Tp, _Lock_policy _Lp>
   
    inline __shared_ptr<_Tp, _Lp>
    atomic_exchange_explicit(__shared_ptr<_Tp, _Lp>* __p,
        __shared_ptr<_Tp, _Lp> __r,
        memory_order)
    {
      _Sp_locker __lock{__p};
      __p->swap(__r);
      return __r;
    }

  template<typename _Tp, _Lock_policy _Lp>
   
    inline __shared_ptr<_Tp, _Lp>
    atomic_exchange(__shared_ptr<_Tp, _Lp>* __p, __shared_ptr<_Tp, _Lp> __r)
    {
      return std::atomic_exchange_explicit(__p, std::move(__r),
        memory_order_seq_cst);
    }
# 264 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_atomic.h" 3
  template<typename _Tp>
   
    bool
    atomic_compare_exchange_strong_explicit(shared_ptr<_Tp>* __p,
         shared_ptr<_Tp>* __v,
         shared_ptr<_Tp> __w,
         memory_order,
         memory_order)
    {
      shared_ptr<_Tp> __x;
      _Sp_locker __lock{__p, __v};
      owner_less<shared_ptr<_Tp>> __less;
      if (*__p == *__v && !__less(*__p, *__v) && !__less(*__v, *__p))
 {
   __x = std::move(*__p);
   *__p = std::move(__w);
   return true;
 }
      __x = std::move(*__v);
      *__v = *__p;
      return false;
    }

  template<typename _Tp>
   
    inline bool
    atomic_compare_exchange_strong(shared_ptr<_Tp>* __p, shared_ptr<_Tp>* __v,
     shared_ptr<_Tp> __w)
    {
      return std::atomic_compare_exchange_strong_explicit(__p, __v,
   std::move(__w), memory_order_seq_cst, memory_order_seq_cst);
    }

  template<typename _Tp>
   
    inline bool
    atomic_compare_exchange_weak_explicit(shared_ptr<_Tp>* __p,
       shared_ptr<_Tp>* __v,
       shared_ptr<_Tp> __w,
       memory_order __success,
       memory_order __failure)
    {
      return std::atomic_compare_exchange_strong_explicit(__p, __v,
   std::move(__w), __success, __failure);
    }

  template<typename _Tp>
   
    inline bool
    atomic_compare_exchange_weak(shared_ptr<_Tp>* __p, shared_ptr<_Tp>* __v,
     shared_ptr<_Tp> __w)
    {
      return std::atomic_compare_exchange_weak_explicit(__p, __v,
   std::move(__w), memory_order_seq_cst, memory_order_seq_cst);
    }

  template<typename _Tp, _Lock_policy _Lp>
   
    bool
    atomic_compare_exchange_strong_explicit(__shared_ptr<_Tp, _Lp>* __p,
         __shared_ptr<_Tp, _Lp>* __v,
         __shared_ptr<_Tp, _Lp> __w,
         memory_order,
         memory_order)
    {
      __shared_ptr<_Tp, _Lp> __x;
      _Sp_locker __lock{__p, __v};
      owner_less<__shared_ptr<_Tp, _Lp>> __less;
      if (*__p == *__v && !__less(*__p, *__v) && !__less(*__v, *__p))
 {
   __x = std::move(*__p);
   *__p = std::move(__w);
   return true;
 }
      __x = std::move(*__v);
      *__v = *__p;
      return false;
    }

  template<typename _Tp, _Lock_policy _Lp>
   
    inline bool
    atomic_compare_exchange_strong(__shared_ptr<_Tp, _Lp>* __p,
       __shared_ptr<_Tp, _Lp>* __v,
       __shared_ptr<_Tp, _Lp> __w)
    {
      return std::atomic_compare_exchange_strong_explicit(__p, __v,
   std::move(__w), memory_order_seq_cst, memory_order_seq_cst);
    }

  template<typename _Tp, _Lock_policy _Lp>
   
    inline bool
    atomic_compare_exchange_weak_explicit(__shared_ptr<_Tp, _Lp>* __p,
       __shared_ptr<_Tp, _Lp>* __v,
       __shared_ptr<_Tp, _Lp> __w,
       memory_order __success,
       memory_order __failure)
    {
      return std::atomic_compare_exchange_strong_explicit(__p, __v,
   std::move(__w), __success, __failure);
    }

  template<typename _Tp, _Lock_policy _Lp>
   
    inline bool
    atomic_compare_exchange_weak(__shared_ptr<_Tp, _Lp>* __p,
     __shared_ptr<_Tp, _Lp>* __v,
     __shared_ptr<_Tp, _Lp> __w)
    {
      return std::atomic_compare_exchange_weak_explicit(__p, __v,
   std::move(__w), memory_order_seq_cst, memory_order_seq_cst);
    }
# 868 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/shared_ptr_atomic.h" 3

}
# 84 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3




# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
  template<typename _Tp1>
    struct auto_ptr_ref
    {
      _Tp1* _M_ptr;

      explicit
      auto_ptr_ref(_Tp1* __p): _M_ptr(__p) { }
    } __attribute__ ((__deprecated__));

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
# 92 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
  template<typename _Tp>
    class auto_ptr
    {
    private:
      _Tp* _M_ptr;

    public:

      typedef _Tp element_type;







      explicit
      auto_ptr(element_type* __p = 0) throw() : _M_ptr(__p) { }
# 118 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      auto_ptr(auto_ptr& __a) throw() : _M_ptr(__a.release()) { }
# 130 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      template<typename _Tp1>
        auto_ptr(auto_ptr<_Tp1>& __a) throw() : _M_ptr(__a.release()) { }
# 141 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      auto_ptr&
      operator=(auto_ptr& __a) throw()
      {
 reset(__a.release());
 return *this;
      }
# 158 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      template<typename _Tp1>
        auto_ptr&
        operator=(auto_ptr<_Tp1>& __a) throw()
        {
   reset(__a.release());
   return *this;
 }
# 176 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      ~auto_ptr() { delete _M_ptr; }
# 186 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      element_type&
      operator*() const throw()
      {
 do { if (__builtin_expect(!bool(_M_ptr != 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h", 189, __PRETTY_FUNCTION__, "_M_ptr != 0"); } while (false);
 return *_M_ptr;
      }







      element_type*
      operator->() const throw()
      {
 do { if (__builtin_expect(!bool(_M_ptr != 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h", 202, __PRETTY_FUNCTION__, "_M_ptr != 0"); } while (false);
 return _M_ptr;
      }
# 216 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      element_type*
      get() const throw() { return _M_ptr; }
# 230 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      element_type*
      release() throw()
      {
 element_type* __tmp = _M_ptr;
 _M_ptr = 0;
 return __tmp;
      }
# 245 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      void
      reset(element_type* __p = 0) throw()
      {
 if (__p != _M_ptr)
   {
     delete _M_ptr;
     _M_ptr = __p;
   }
      }
# 270 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      auto_ptr(auto_ptr_ref<element_type> __ref) throw()
      : _M_ptr(__ref._M_ptr) { }

      auto_ptr&
      operator=(auto_ptr_ref<element_type> __ref) throw()
      {
 if (__ref._M_ptr != this->get())
   {
     delete _M_ptr;
     _M_ptr = __ref._M_ptr;
   }
 return *this;
      }

      template<typename _Tp1>
        operator auto_ptr_ref<_Tp1>() throw()
        { return auto_ptr_ref<_Tp1>(this->release()); }

      template<typename _Tp1>
        operator auto_ptr<_Tp1>() throw()
        { return auto_ptr<_Tp1>(this->release()); }
    } __attribute__ ((__deprecated__ ("use '" "std::unique_ptr" "' instead")));



  template<>
    class auto_ptr<void>
    {
    public:
      typedef void element_type;
    } __attribute__ ((__deprecated__));



  template<_Lock_policy _Lp>
  template<typename _Tp>
    inline
    __shared_count<_Lp>::__shared_count(std::auto_ptr<_Tp>&& __r)
    : _M_pi(new _Sp_counted_ptr<_Tp*, _Lp>(__r.get()))
    { __r.release(); }

  template<typename _Tp, _Lock_policy _Lp>
  template<typename _Tp1, typename>
    inline
    __shared_ptr<_Tp, _Lp>::__shared_ptr(std::auto_ptr<_Tp1>&& __r)
    : _M_ptr(__r.get()), _M_refcount()
    {
     
      static_assert( sizeof(_Tp1) > 0, "incomplete type" );
      _Tp1* __tmp = __r.get();
      _M_refcount = __shared_count<_Lp>(std::move(__r));
      _M_enable_shared_from_this_with(__tmp);
    }

  template<typename _Tp>
  template<typename _Tp1, typename>
    inline
    shared_ptr<_Tp>::shared_ptr(std::auto_ptr<_Tp1>&& __r)
    : __shared_ptr<_Tp>(std::move(__r)) { }


  template<typename _Tp, typename _Dp>
  template<typename _Up, typename>
    inline
    unique_ptr<_Tp, _Dp>::unique_ptr(auto_ptr<_Up>&& __u) noexcept
    : _M_t(__u.release(), deleter_type()) { }


#pragma GCC diagnostic pop


}
# 89 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3
# 117 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 118 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3


namespace std __attribute__ ((__visibility__ ("default")))
{

# 138 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 3
enum class pointer_safety { relaxed, preferred, strict };


inline void
declare_reachable(void*) { }


template <typename _Tp>
  inline _Tp*
  undeclare_reachable(_Tp* __p) { return __p; }


inline void
declare_no_pointers(char*, size_t) { }


inline void
undeclare_no_pointers(char*, size_t) { }


inline pointer_safety
get_pointer_safety() noexcept { return pointer_safety::relaxed; }



}
# 10 "./tree-ssa/pr95638.C" 2

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/utility" 1 3
# 70 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/utility" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_relops.h" 1 3
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_relops.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{


  namespace rel_ops
  {
# 86 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_relops.h" 3
    template <class _Tp>
      inline bool
      operator!=(const _Tp& __x, const _Tp& __y)
      { return !(__x == __y); }
# 99 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_relops.h" 3
    template <class _Tp>
      inline bool
      operator>(const _Tp& __x, const _Tp& __y)
      { return __y < __x; }
# 112 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_relops.h" 3
    template <class _Tp>
      inline bool
      operator<=(const _Tp& __x, const _Tp& __y)
      { return !(__y < __x); }
# 125 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_relops.h" 3
    template <class _Tp>
      inline bool
      operator>=(const _Tp& __x, const _Tp& __y)
      { return !(__x < __y); }
  }


}
# 71 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/utility" 2 3




# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/initializer_list" 1 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/initializer_list" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

  template<class _E>
    class initializer_list
    {
    public:
      typedef _E value_type;
      typedef const _E& reference;
      typedef const _E& const_reference;
      typedef size_t size_type;
      typedef const _E* iterator;
      typedef const _E* const_iterator;

    private:
      iterator _M_array;
      size_type _M_len;


      constexpr initializer_list(const_iterator __a, size_type __l)
      : _M_array(__a), _M_len(__l) { }

    public:
      constexpr initializer_list() noexcept
      : _M_array(0), _M_len(0) { }


      constexpr size_type
      size() const noexcept { return _M_len; }


      constexpr const_iterator
      begin() const noexcept { return _M_array; }


      constexpr const_iterator
      end() const noexcept { return begin() + size(); }
    };







  template<class _Tp>
    constexpr const _Tp*
    begin(initializer_list<_Tp> __ils) noexcept
    { return __ils.begin(); }







  template<class _Tp>
    constexpr const _Tp*
    end(initializer_list<_Tp> __ils) noexcept
    { return __ils.end(); }
}
# 76 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/utility" 2 3
# 103 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/utility" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 104 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/utility" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{




  template <typename _Tp, typename _Up = _Tp>
   
    inline _Tp
    exchange(_Tp& __obj, _Up&& __new_val)
    noexcept(__and_<is_nothrow_move_constructible<_Tp>,
      is_nothrow_assignable<_Tp&, _Up>>::value)
    { return std::__exchange(__obj, std::forward<_Up>(__new_val)); }
# 237 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/utility" 3

}
# 12 "./tree-ssa/pr95638.C" 2


# 13 "./tree-ssa/pr95638.C"
template <typename Derived>
class intrusive_ref_counter
{
public:
    intrusive_ref_counter() = default;

    intrusive_ref_counter(intrusive_ref_counter const&) :
        _counter(0)
    {
    }

    friend void intrusive_ptr_add_ref(const intrusive_ref_counter<Derived>* p)
    {
        ++p->_counter;
    }

    friend void intrusive_ptr_release(const intrusive_ref_counter<Derived>* p)
    {
        if (--p->_counter == 0)
        {
            delete static_cast<const Derived*>(p);
        }
    }

private:
    mutable int _counter = 0;
};

template <typename T>
class intrusive_ptr
{
public:
    intrusive_ptr() = default;

    intrusive_ptr(T* p): px(p)
    {
        if (px != 0) intrusive_ptr_add_ref(px);
    }

    intrusive_ptr(intrusive_ptr const & rhs): px(rhs.px)
    {
        if (px != 0) intrusive_ptr_add_ref(px);
    }

    ~intrusive_ptr()
    {
        if (px != 0) intrusive_ptr_release(px);
    }

    intrusive_ptr(intrusive_ptr && rhs) : px(rhs.px)
    {
        rhs.px = 0;
    }

    explicit operator bool() const
    {
        return px != 0;
    }

private:
    T* px = nullptr;
};

template <typename T, uint32_t MaxSize = 1>
class Storage
{
public:
    Storage() = default;

    Storage(Storage&& other)
    {
        for (int i = 0; i < other._size; ++i)
        {
            new (data() + i) T(std::move(other.data()[i]));
            ++_size;
        }
    }

    ~Storage()
    {
        for (int i = 0; i < _size; ++i)
        {
            data()[i].~T();
        }
    }

    void push_back(const T& value)
    {
        
# 101 "./tree-ssa/pr95638.C" 3 4
       ((
# 101 "./tree-ssa/pr95638.C"
       _size < MaxSize
# 101 "./tree-ssa/pr95638.C" 3 4
       ) ? static_cast<void> (0) : __assert_fail (
# 101 "./tree-ssa/pr95638.C"
       "_size < MaxSize"
# 101 "./tree-ssa/pr95638.C" 3 4
       , "./tree-ssa/pr95638.C", 101, __PRETTY_FUNCTION__))
# 101 "./tree-ssa/pr95638.C"
                              ;

        new (data() + _size) T(value);
        ++_size;
    }

    T& operator[](size_t idx)
    {
        
# 109 "./tree-ssa/pr95638.C" 3 4
       ((
# 109 "./tree-ssa/pr95638.C"
       idx < _size
# 109 "./tree-ssa/pr95638.C" 3 4
       ) ? static_cast<void> (0) : __assert_fail (
# 109 "./tree-ssa/pr95638.C"
       "idx < _size"
# 109 "./tree-ssa/pr95638.C" 3 4
       , "./tree-ssa/pr95638.C", 109, __PRETTY_FUNCTION__))
# 109 "./tree-ssa/pr95638.C"
                          ;
        return data()[idx];
    }

private:
    T* data()
    {
        return reinterpret_cast<T*>(&_data);
    }

private:
    uint32_t _size = 0;
    std::aligned_storage_t<sizeof(T[MaxSize])> _data;
};

struct Item: intrusive_ref_counter<Item>
{
};

using Collection = Storage<intrusive_ptr<Item>>;

struct Holder
{
    __attribute__ ((noinline)) Holder(Collection collection) : collection(std::move(collection)) {}

    int64_t dummy = 0;
    Collection collection;
};

int main()
{
    Collection collection;
    collection.push_back(intrusive_ptr<Item>(new Item()));

    Holder holder(std::move(collection));

    auto item = holder.collection[0];

    if (!item)
      __builtin_abort ();

    return 0;
}
