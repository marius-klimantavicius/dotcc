//options_all:--c++20 -A
void f() {
    unsigned(-1);     // OK, equivalent to (int) -1
    
    void{};           // OK, prvalue of type void
    void(1);          // OK, equivalent to (void) 1
    
}
