//type: fn
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
import shadow;

void stat (); // { dg-error "conflicts with import" }
struct stat {}; // { dg-error "conflicts with import" }
