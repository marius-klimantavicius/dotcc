//type: fp
//options_all: -A --c++20 -tused -e 200 --no_wrap

constexpr int f() {
       auto lam = [] { L: return 0; };
       return 0;
     }

//cwg: 2309
//title: Restrictions on nested statements within constexpr functions
//meeting: Kona 02/19
//edg_status: Passes
