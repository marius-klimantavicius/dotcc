//type: fn
//options: --c++11
// { dg-do compile { target c++11 } }
template<class T, typename... VarArgs>
void print(T t, VarArgs args); // { dg-error "packs not expanded" }
// { dg-message "VarArgs" "note" { target *-*-* } .-1 }
