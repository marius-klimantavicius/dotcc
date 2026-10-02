//options_all:--c++23 -A
  template<typename T>
  concept C2 = requires(T p[2]) {
    (decltype(p))nullptr;           // OK, p has type "pointer to T"
  };
