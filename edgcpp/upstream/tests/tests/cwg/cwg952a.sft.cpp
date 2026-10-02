//type:fp
//options_all:--c++20 -tused -A

struct A {
     typedef int B;
};

struct C : private A {
     struct Y {
         void g() {
             struct Z {
                 int h() {
                     return B(0); // presumably not intended to
                                  // be an access violation
                 }
             };
         }
     };
};

//cwg: 952
//title: Insufficient description of “naming class”
//meeting: Virtual 11/20*
//edg_status: Passes
