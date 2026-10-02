//type:fn
//options_all:--c++11 -tused

class S { };

template<typename... T> class X {
public:
  X(T ...args) : data_(args)... { }
private:
  S data_;
};

int main() {
  S s;
  X<> x1;
  X<S> x2(s);
}
