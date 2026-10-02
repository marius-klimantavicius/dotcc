//remark:enum class and generic lambdas
//options:--microsoft_v=1916 --c++14;fp:--c++14;fn

enum class E {};
auto r = [](enum class E) {};

