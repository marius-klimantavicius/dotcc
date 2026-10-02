//type:cp
//options:--c++17

struct W { W(int); };
struct X : virtual W { using W::W; X() = delete; };
struct Y : X { using X::X; };
struct Z : Y, virtual W { using Y::Y; };

Z z(0); // OK: initialization of Y does not invoke default constructor of X
