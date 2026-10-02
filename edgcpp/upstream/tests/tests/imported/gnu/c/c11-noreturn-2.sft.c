//type: rp
//options: --c11
# 0 "./c11-noreturn-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c11-noreturn-2.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdnoreturn.h" 1 3 4
# 6 "./c11-noreturn-2.c" 2

extern int strcmp (const char *, const char *);


# 9 "./c11-noreturn-2.c" 3 4
_Noreturn 
# 9 "./c11-noreturn-2.c"
        void exit (int);

# 10 "./c11-noreturn-2.c" 3 4
_Noreturn 
# 10 "./c11-noreturn-2.c"
        void abort (void);


# 12 "./c11-noreturn-2.c" 3 4
_Noreturn 
# 12 "./c11-noreturn-2.c"
        int f1 (void);


# 14 "./c11-noreturn-2.c" 3 4
_Noreturn 
# 14 "./c11-noreturn-2.c"
        void f2 (void);

static void 
# 16 "./c11-noreturn-2.c" 3 4
           _Noreturn 
# 16 "./c11-noreturn-2.c"
                    f3 (void) { exit (0); }






# 22 "./c11-noreturn-2.c" 3 4
_Noreturn 
# 22 "./c11-noreturn-2.c"
        int
f4 (void)
{
  return 1;

}


# 29 "./c11-noreturn-2.c" 3 4
_Noreturn 
# 29 "./c11-noreturn-2.c"
        void
f5 (void)
{
  return;

}


# 36 "./c11-noreturn-2.c" 3 4
_Noreturn 
# 36 "./c11-noreturn-2.c"
        void
f6 (void)
{
}


# 41 "./c11-noreturn-2.c" 3 4
_Noreturn 
# 41 "./c11-noreturn-2.c"
        void
f7 (int a)
{
  if (a)
    exit (0);
}



void f2 (void);

void f8 (void);

# 53 "./c11-noreturn-2.c" 3 4
_Noreturn 
# 53 "./c11-noreturn-2.c"
        void f8 (void);



# 56 "./c11-noreturn-2.c" 3 4
_Noreturn _Noreturn 
# 56 "./c11-noreturn-2.c"
                 void 
# 56 "./c11-noreturn-2.c" 3 4
                      _Noreturn 
# 56 "./c11-noreturn-2.c"
                               f9 (void);



void (*fp) (void) = f5;
# 69 "./c11-noreturn-2.c"
const char *s = "_Noreturn";

int
main (void)
{
  if (strcmp (s, "_Noreturn") != 0)
    abort ();
  exit (0);
}
