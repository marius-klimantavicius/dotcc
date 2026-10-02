//options_all:--c++20 -tused -A
void f() {
    float x, &r = x;
    [=]() -> decltype((x)) { //lambda returns float const& because lambda is not mutable and
    // x is an lvalue
    decltype(x) y1;    // y1 has type float
    decltype((x)) y2 = y1;    //y2 has type float const&
    decltype(r) r1 = y1;       //r1 had type float&
    decltype((r)) r2 = y2;       //r2 has type float const&
    return y2;
    };
    [=](decltype((x)) y){
        decltype((x)) z = x;
    };
    [=]{
        [=](decltype((x)) y){}; // okay
         [x=1](decltype((x)) y) {
             decltype((x)) z = x;
         };

    };
}
