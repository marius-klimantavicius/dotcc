//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

extern "C" 
{
  import foo; // { dg-warning "inside language-linkage" }
}
extern "C++"
{
  import foo; // { dg-warning "inside language-linkage" }
}
