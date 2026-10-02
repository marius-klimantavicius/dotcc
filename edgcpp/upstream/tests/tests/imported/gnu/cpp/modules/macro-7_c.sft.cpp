//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
import bar;

int main ()
{
  return !(One () == 1);
}
