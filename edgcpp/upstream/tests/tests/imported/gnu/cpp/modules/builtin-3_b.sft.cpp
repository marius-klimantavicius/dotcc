//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
import builtins;

int main ()
{
  length ("");
  count (1, "", "", nullptr);
}
