//type: fn
//options: --c++11
// { dg-do compile { target c++11 } }

extern "C"_badlinkage {	// { dg-error "expected unqualified-id before" }

int foo();

}
