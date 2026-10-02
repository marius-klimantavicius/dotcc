//remark:noexcept-specifier visibility
//options:--c++20;fn

template <class T> concept C = true;

void f ( C auto x ) noexcept ( f ( 29 ) ) { }
