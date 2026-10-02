//type:fp
//options: -A --c++20

consteval void foo() {}

int main() {
  foo();
}

//cwg: 3136
//title: Constant expressions of type void
//meeting: Croydon 3/26
//edg_status: Passes
