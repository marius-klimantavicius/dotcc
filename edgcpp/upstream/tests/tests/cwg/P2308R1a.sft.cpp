//options_all:--c++23 -tused -A
template<int i> struct C { /* ... */ };
C<{ 42 }> c1;  // OK
