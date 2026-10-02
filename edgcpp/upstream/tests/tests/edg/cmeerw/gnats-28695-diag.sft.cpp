//type:fn
//options:--c++20

struct C
{
  bool b = true;
};

C c = { true };

auto l = []<typename> requires (&c)->b { }; // error: operator cannot appear at the top level
