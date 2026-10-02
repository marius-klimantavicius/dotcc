//remark:Overload resolution for C++20 operator==
//options:--c++17 -A;fp:--c++20 -A;fn:--c++20;fp

struct X { bool operator==(const X&); };
bool b = X() == X();
