//type: fp
//options: 
# 0 "./pr67730-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr67730-2.c"




# 1 "./pr67730.h" 1
       
# 2 "./pr67730.h" 3





# 6 "./pr67730.h" 3
extern void sysbar (unsigned char *);

unsigned char *
sysfn1 (void *p)
{
   unsigned char *uc = (void *) 1;
   uc = (void *) 1;
   sysbar ((void *) 1);
   return (void *) 1;
}

extern void sysbar2 (int);

int
sysfn2 (void)
{
  int a = (void *) 0;
  a = (void *) 0;
  sysbar2 ((void *) 0);
  return (void *) 0;
}

int
sysfn3 (void)
{
  return (void *) 0;
}
# 6 "./pr67730-2.c" 2


# 7 "./pr67730-2.c"
extern void bar (int);

int
fn1 (void)
{
  int a = 
# 12 "./pr67730-2.c" 3
         (void *) 0
# 12 "./pr67730-2.c"
             ;
  a = 
# 13 "./pr67730-2.c" 3
     (void *) 0
# 13 "./pr67730-2.c"
         ;
  bar (
# 14 "./pr67730-2.c" 3
      (void *) 0
# 14 "./pr67730-2.c"
          );
  return 
# 15 "./pr67730-2.c" 3
        (void *) 0
# 15 "./pr67730-2.c"
            ;
}

int
fn2 (void)
{
  
# 21 "./pr67730-2.c" 3
 return (void *) 0
# 21 "./pr67730-2.c"
       ;
}
