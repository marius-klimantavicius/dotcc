//type: fp
//options: 
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
/* { dg-do compile { target i?86-*-* x86_64-*-* } } */
/* { dg-require-ifunc "" } */

__attribute__((target_clones("avx","arch=slm","default")))
__attribute__((target("avx")))
int foo (); /* { dg-warning "'target' attribute ignored due to conflict with 'target_clones' attribute" } */

__attribute__((target_clones("avx","arch=slm","default"),always_inline))
int bar (); /* { dg-warning "'always_inline' attribute ignored due to conflict with 'target_clones' attribute" } */
