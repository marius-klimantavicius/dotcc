//type: fp
//options: 
# 0 "./pr67730-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr67730-1.c"




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
# 6 "./pr67730-1.c" 2


# 7 "./pr67730-1.c"
extern void bar (unsigned char *);

unsigned char *
f (void *p)
{
   unsigned char *uc = 
# 12 "./pr67730-1.c" 3
                      (void *) 1
# 12 "./pr67730-1.c"
                          ;
   uc = 
# 13 "./pr67730-1.c" 3
       (void *) 1
# 13 "./pr67730-1.c"
           ;
   bar (
# 14 "./pr67730-1.c" 3
       (void *) 1
# 14 "./pr67730-1.c"
           );
   return 
# 15 "./pr67730-1.c" 3
         (void *) 1
# 15 "./pr67730-1.c"
             ;
}
