//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module bob;
// { dg-module-cmi bob }

#define SEMI ;

module :private SEMI // { dg-message "sorry, unimplemented: private module fragment" }
int i;
