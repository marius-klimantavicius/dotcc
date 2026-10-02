//remark:Virtual function overriders
//type:fn
//name:
//options:
//options_all:-A
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

extern "C" int printf(char const*, ...);

struct A;

A* a0;
A* a1;
A* a2;

struct A {
  virtual void f () {}
};

struct B : virtual public A {
  virtual void f ();
};

void B::f () { printf ("%x\n", this); }

struct C : public B {};

struct D: public C, public B {};

struct E : public B, public D {};

int main () {
  E e;
  a0->f ();
}

