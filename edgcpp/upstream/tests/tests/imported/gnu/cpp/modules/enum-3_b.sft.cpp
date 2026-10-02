//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
module m0;

static_assert (!m0_ns::s0<int>::a);
