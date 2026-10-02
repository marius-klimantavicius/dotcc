//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
import hello;

int main ()
{
  return Check ("World") ? 0 : 1;
}
