//type:fn
//options_all:--c++17 -tused -A 
//
      // _CXX11 - Implements core 1572 - in C++14 status 2015
    struct A_209_853j5223c_z { };
    A_209_853j5223c_z a_209_853j5223c_z ;
    struct B_209_853j5223c_z {
        operator const A_209_853j5223c_z() { return a_209_853j5223c_z; }
    };
    struct C_209_853j5223c_z {
        operator A_209_853j5223c_z&() { return a_209_853j5223c_z; }
    };
    int d_209_853j5223c_z() {
        typedef const A_209_853j5223c_z E;
        A_209_853j5223c_z&& a = B_209_853j5223c_z(); // error - drops qualifiers
        return 1;
    }
    int mymain() { return 0; }
