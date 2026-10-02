//type:fp
//options: -A --c++20

template<typename U>
void fun(U u = U());

struct X {
  X(int) {}
};

template<typename T>
decltype(fun<T>()) g(T *) {}

void g(...);

int main() {
  X *p = 0;
  g(p);
}

//cwg: 2296
//title: Are default argument instantiation failures in the immediate context?
//meeting: Croydon 3/26
