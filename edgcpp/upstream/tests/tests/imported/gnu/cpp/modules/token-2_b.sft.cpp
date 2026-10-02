//type: fn
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
#define SEMI ; // this is ok since p1857
import bob SEMI
#define IMPORT import // { dg-error "does not name a type" }
IMPORT bob ;
