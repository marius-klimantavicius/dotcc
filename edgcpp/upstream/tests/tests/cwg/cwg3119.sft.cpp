//type:fp
//options: -A --c++26

struct B {
  int i;
  long l;
};

struct A {
  B b;
};

void f(int i, long l);

int main() {
  template for (auto [...e] : A()) {
    f(e...);
  }
}

//cwg: 3119
//title: for-range-declaration of an expansion-statement as a templated entity
//meeting: Croydon 3/26
