//type: rp
//options: --c++17
// { dg-do run { target c++17 } }

int
main ()
{
  if (int i = 10, &ir = i; [=]{ return ir; }() != 10)
    __builtin_abort ();
}
