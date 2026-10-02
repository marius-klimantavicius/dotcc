//type: fp
//options: 
# 0 "./auto-init-uninit-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-5.c"





# 1 "./uninit-5.c" 1





extern void use(int);
extern void foo(void);

void
func1(int cond)
{
    int x;

    if(cond)
 x = 1;

    foo();

    if(cond)
 use(x);
}

void
func2 (int cond)
{
    int x;
    int flag = 0;

    if(cond)
    {
 x = 1;
 flag = 1;
    }

    foo();

    if(flag)
 use(x);
}
# 7 "./auto-init-uninit-5.c" 2
