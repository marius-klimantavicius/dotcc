//type: fn
//options:  --c++17 --c++14
std::byte b;		 // { dg-error "byte" }
// { dg-message "cstddef" "" { target c++17 } .-1 }
// { dg-message "C..17" "" { target c++14_down } .-2 }
