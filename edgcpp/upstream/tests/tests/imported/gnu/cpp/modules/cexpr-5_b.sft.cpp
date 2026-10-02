//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

module M;

constexpr auto& use_xr = xr;
constexpr auto& use_fr = fr;

static_assert(&cxr == &use_xr);
static_assert(&cfr == &use_fr);
