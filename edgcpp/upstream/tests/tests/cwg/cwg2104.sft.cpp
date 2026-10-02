//type:fp
//options_all:-tused --c++17 -A
//
constexpr const int n = 42;
constexpr const int& ref = n;
int arr[ref];
// apparently an odr-use of ref according to
              // 3.2 (The lvalue-to-rvalue conversion is
              // applied to n, not ref)
const int * p = &ref; // apparently an odr-use of ref but
                      // not of n (3.2 p3 applies to "A
                      // variable x whose name appears", but
                      // the name "n" does not appear here).

//cwg: 2104
//title: Internal-linkage constexpr references and ODR requirements
//meeting: Jacksonville 2/16
//edg_status: Passes
