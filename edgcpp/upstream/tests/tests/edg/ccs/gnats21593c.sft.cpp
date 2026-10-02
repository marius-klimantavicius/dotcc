//type:cp
//options:--c++20

enum E { ea, eb };

E& operator+=(E, E);
E& operator++(E);
E& operator++(E, int);

struct A {
  A& operator=(A&);
  A& operator=(A&) volatile;
  A& operator+=(A&);
  A& operator+=(A&) volatile;
  A& operator++() volatile;
  A& operator++(int) volatile;
};

// None of these should be deprecated as they use overloaded operators
void f() {
  E e = ea;
  volatile E ve = eb;
  A a;
  volatile A va;

  ve += e;
  ve++;
  ++ve;

  va = va = a;
  va += a;
  va++;
  ++va;
}
