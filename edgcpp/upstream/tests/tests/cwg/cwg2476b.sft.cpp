//type:fn
//options_all:--c++23 -tused -A
   template<typename T> struct C { };
    C<auto(*)()> c1;         // Not OK
    C<auto(*)()->int> c2;    // OK
    C<auto(*)()->auto> c3;   // Not OK
