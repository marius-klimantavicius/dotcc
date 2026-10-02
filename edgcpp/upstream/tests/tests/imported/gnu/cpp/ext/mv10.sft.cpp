//type: fp
//options: 
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
// { dg-do assemble { target i?86-*-* x86_64-*-* } }
// { dg-options "" }

__attribute__((target ("popcnt"), used))
void foo (void)
{
}

__attribute__((target ("popcnt","avx"), used))
void foo (void)
{
}
