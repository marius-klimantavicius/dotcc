//type: fn
//options: --c++11
// { dg-do compile { target c++11 } }

void *p = '\0';			// { dg-error "invalid conversion" }
