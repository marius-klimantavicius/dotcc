//type:fn
//options::-A
//options_all:--c++11

struct S {
  S(bool);
};

char c = 'a';
S s{c};
S* p = new S{c};
