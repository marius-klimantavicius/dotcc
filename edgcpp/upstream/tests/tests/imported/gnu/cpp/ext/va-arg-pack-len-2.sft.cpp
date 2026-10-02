//type: fn
//options: 
# 0 "./ext/va-arg-pack-len-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/va-arg-pack-len-2.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./ext/va-arg-pack-len-2.C" 2


# 6 "./ext/va-arg-pack-len-2.C"
extern int error_open_missing_mode (void)
  __attribute__((__error__ ("open with O_CREAT needs 3 arguments, only 2 were given")));
extern int warn_open_too_many_arguments (void)
  __attribute__((__warning__ ("open called with more than 3 arguments")));

extern int myopen2 (const char *path, int oflag);
extern int myopenva (const char *path, int oflag, ...);

extern inline __attribute__((always_inline, gnu_inline)) int
myopen (const char *path, int oflag, ...)
{
  if (__builtin_va_arg_pack_len () > 1)
    warn_open_too_many_arguments ();

  if (__builtin_constant_p (oflag))
    {
      if ((oflag & 0x40) != 0 && __builtin_va_arg_pack_len () < 1)
 {
   error_open_missing_mode ();
   return myopen2 (path, oflag);
 }
      return myopenva (path, oflag, __builtin_va_arg_pack ());
    }

  if (__builtin_va_arg_pack_len () < 1)
    return myopen2 (path, oflag);

  return myopenva (path, oflag, __builtin_va_arg_pack ());
}

int
main (void)
{
  myopen ("h", 0x43);
  myopen ("i", 0x43, 0644, 0655);
  return 0;
}
