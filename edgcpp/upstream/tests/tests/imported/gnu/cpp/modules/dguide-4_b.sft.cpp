//type: fp
//options:  --c++20 --modules
// PR c++/116403
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi B }

export module B;
import A;  // not exported

export using ::GMF;
export using ::Attached;
