//remark:if consteval
//options:--c++23;fp

  consteval int f(int i) { return i; }
  constexpr int g(int i) {
    if consteval {
      return f(i);  // Not an error even though i is not a constant
                    // expression because this is a consteval context.
    }
    return 0;
  }
  static_assert(g(42) == 42);
