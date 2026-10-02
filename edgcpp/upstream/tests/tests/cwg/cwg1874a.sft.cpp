//type:fp
//options_all:--c++17 -tused -A
  template<class T> class myarray { /* ... */ };

  template<class K, class V, template<class T> class C = myarray>
  class Map {
    C<K> key;
    C<V> value;
  };

//cwg: 1874
//title: Type vs non-type template parameters with class keyword
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
