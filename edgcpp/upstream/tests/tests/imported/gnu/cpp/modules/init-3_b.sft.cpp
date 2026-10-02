//type: fp
//options:  --c++20 --modules
// { dg-additional-options {-fmodules-ts -Wno-pedantic} }

export module Two;
import One;

export Dyn two;
