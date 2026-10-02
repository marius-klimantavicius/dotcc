//remark:Member lookup during member substitution
//options:--c++17;fn

template<typename> int g();
template<typename> int g(...);
int r = g<float>();
