//type:fn
//options_all:--c++23
int main() {
    int x = 0;
    auto lambda = [x] (this auto self) { return x; };
    using Lambda = decltype(lambda);
    struct D : private Lambda {
      D(Lambda l) : Lambda(l) {}
      using Lambda::operator();
      friend Lambda;
    } d(lambda);
    d();
} 

//cwg: 2881
//title: Type restrictions for the explicit object parameter of a lambda
//meeting: St Louis 6/24
//edg_status: EDGcpfe/27414
