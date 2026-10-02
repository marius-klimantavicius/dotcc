//remark:Pseudo-destructors
//options:--c++14;fn

using S = int;
int *p;

using X = decltype(p->~S);

void g() {
  p->S::~S;
}

