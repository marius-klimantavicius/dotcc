//type:fn
//options:--c++26
//options_all:-A -tused

int main() {
  try {
    throw 0;
  } catch (...) {
    constexpr int x = [] {
      try {
        throw;                  // error
      } catch (...) {
        return 1;
      }
    }();

    constexpr int y = [] {
      try {
        throw 0;                // OK
      } catch (...) {
        return 2;
      }
    }();
  }
}

//cwg: 3059
//title: throw; in constant expressions
//meeting: Kona 11/25
//edg_status: EDGcpfe/28542
