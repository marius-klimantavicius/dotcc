//type:fp
//options: -A --c++26

struct X {
  int n;
};

void begin(X *);
void end(X *);

int main() {
  template for (auto x : X{0}) {
    (void)x;
  }
}

//cwg: 3123
//title: Global lookup for begin and end for expansion statements
//meeting: Croydon 3/26
