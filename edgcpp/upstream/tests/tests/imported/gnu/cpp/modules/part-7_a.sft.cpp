//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module foo:bill;
// { dg-module-cmi foo:bill }

export class frob;

export template<int I> class FROB;
