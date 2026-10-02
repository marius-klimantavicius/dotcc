//type:cp
//options_all:--c --microsoft
/* Microsoft nonstandard anonymous unions: use of named struct. */
struct S
{
   int i;
};
struct S2
{
   struct S;
};

void f(void)
{
   struct S s;
   struct S2 s2;

   s.i = 0;
   s2.i = 0;
}

