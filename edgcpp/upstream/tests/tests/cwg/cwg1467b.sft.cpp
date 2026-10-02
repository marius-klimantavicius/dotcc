//type:fp
//options_all:--c++17 -tused -A

    struct NonAggregate {
      NonAggregate() {}
    };

    struct WantsIt {
      WantsIt(NonAggregate);
    };

    void f(NonAggregate n);
    void f(WantsIt);

    int main() {
      NonAggregate n;

      // ambiguous!
      f({n});
    }
