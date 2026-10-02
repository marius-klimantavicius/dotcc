//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts -fcontracts -fcontract-role=default:ignore,ignore,ignore" }
module foo;

int fn_int(int n)
{
  return fn_t(n);
}

