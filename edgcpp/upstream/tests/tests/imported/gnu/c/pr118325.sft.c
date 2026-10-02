//type: fp
//options: --c17 --strict_gnu
/* { dg-do compile } */
/* { dg-require-effective-target trampolines } */
/* { dg-options "-std=gnu17 -fchecking" } */

void f(void*);

void z()
{
    void g()
    {
        __label__ out;
        int h(void) { goto out; }
        f(h);
    out:;
    }
    f(g);
}
