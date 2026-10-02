//type:cp
//options:--c++17

struct W { W(int); };
struct V: W { using W::W; };
struct X : virtual V { using V::V; X() = delete; };
struct Y : X { using X::X; };
struct Z : Y, virtual V { using Y::Y; };

Z z(0);
