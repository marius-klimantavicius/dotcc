//remark:Unions and implicit initializers
//options:--c++17;fp

union U { long a, b = -42; } x{};
