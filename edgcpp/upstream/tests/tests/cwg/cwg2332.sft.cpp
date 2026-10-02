//type: fp
//options_all: -A --c++20 -tused -e 200 --no_wrap

 template<typename T> struct X {
    template<typename Iter>
    X(Iter b, Iter e) { /* ... */ }

    template<typename Iter>
    auto foo(Iter b, Iter e) { 
      return X(b, e); // X<U> to avoid breaking change
    }

    template<typename Iter>
    auto bar(Iter b, Iter e) { 
      return X<Iter::value_type>(b, e); // Must specify what we want
    }
  };

//cwg: 2332
//title: template-name as simple-type-name vs injected-class-name
//meeting: Kona 02/19
//edg_status: EDGcpfe/20920
