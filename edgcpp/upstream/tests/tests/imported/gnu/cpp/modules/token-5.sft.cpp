//type: fn
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
module;

class X; // { dg-error "global module fragment contents" }

class Y;

export module frob;
// { dg-module-cmi !frob }
