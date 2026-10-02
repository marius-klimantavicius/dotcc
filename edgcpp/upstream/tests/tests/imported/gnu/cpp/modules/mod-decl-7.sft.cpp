//type: fn
//options:  --c++20 --modules
// PR c++/115200
// { dg-additional-options "-fmodules-ts -Wno-global-module" }
// { dg-module-cmi !M }

module;

void foo() {

export module M;  // { dg-error "unexpected module directive" }

}
