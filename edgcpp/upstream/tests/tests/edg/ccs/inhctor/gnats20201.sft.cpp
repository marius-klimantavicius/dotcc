//type:cp
//options:--c++17

struct W                { W(int); };
struct X : virtual W    { using W::W; X() = delete; };
struct Y : X            { using X::X; };

Y y(0);
