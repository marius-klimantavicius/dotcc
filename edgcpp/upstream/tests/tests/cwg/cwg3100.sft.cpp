//type:rp
//options:--c++11
//options_all:-A -tused

extern "C" int printf(const char *, ...);

struct DTemp {
  ~DTemp() {
    printf("~DTemp\n");
  }
};

struct Temp {
  ~Temp() {
    static DTemp dt;
  }
};
struct BTemp {
  ~BTemp() {
    printf("~BTemp\n");
  }
};
struct A {
  const BTemp &tb;
  ~A() {
    printf("~A\n");
  }
};

// In the following program, the elements of a are destroyed, followed by dt,
// and finally the two BTemp objects:
A a[] = { (Temp(), BTemp()), BTemp() };

int main() { }

//cwg: 3100
//title: Destruction order for objects with static storage duration
//meeting: Kona 11/25
//edg_status: EDGcpfe/28549
