//type:lp
//options:--c++11

template<typename T>
struct X {
  // The class template must have a user defined destructor.
  ~X() {}
};

struct Y {
  Y() {}
  // There must be an in-field default initializer that initializes a class
  // template specialization.
  X<int> z = {};
};

// The TU must include only a dynamically allocated constructor call for the
// type.  No destructor calls must exist in the entire program.
Y* y = new Y();

int main() {
  return 0;
}
