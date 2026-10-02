//type:fn
//options_all:--c++20  -A
void f() {
    unsigned{-1};     // ill-formed, narrowing conversion
    
    void{0};          // ill-formed
    void(1, 2);       // ill-formed
    int(1, 2);        // ill-formed
}
