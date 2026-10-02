//options_all:--c++23 -tused -A
  template <typename T>
  concept C = requires {
    typename T::type<void>;   // template required?
  };

//cwg: 2806
//title: Make a type-requirement a type-only context
//meeting: Kona 11/23
//edg_status: EDGcpfe/26813
