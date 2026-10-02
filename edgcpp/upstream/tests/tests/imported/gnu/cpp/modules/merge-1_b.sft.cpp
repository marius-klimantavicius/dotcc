//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

module foo;

void frob ()
{
  __throw_with_nested_impl (integral_constant<bool, true> ());
}
