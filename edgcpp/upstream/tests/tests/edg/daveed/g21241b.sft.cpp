//remark:Variadic DMIs
//options:--c++11 --bool --typename --microsoft_version 1900;fp

template<typename ... Ts> struct X {
  int szs[sizeof...(Ts)] = { sizeof(Ts)... };
};

X<int, short> xis;
