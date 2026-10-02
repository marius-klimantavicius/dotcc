//type: fp
//options: 
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
/* { dg-do compile { target i?86-*-* x86_64-*-* } } */
/* { dg-require-ifunc "" } */

class b
{
public:
  __attribute__ ((target ("aes"))) b () {}
  __attribute__ ((target ("default"))) b () {}
};
class c
{
  b d;
};
void
fn1 ()
{
  c a;
}
__attribute__ ((target_clones ("sse", "default"))) void
e ()
{
}
