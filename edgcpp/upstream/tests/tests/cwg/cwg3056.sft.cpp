//type:fp
//options:--c++26
//options_all:-A -tused

template<typename T, typename T::type = 0> struct S;
template<typename T> using Ref = T&;

template<typename T> concept C = requires {
  typename T::inner;        // required nested member name
  typename S<T>;            // required valid ([temp.names]) template-id; fails if T​::​type does not exist as a type
  // to which 0 can be implicitly converted
  typename Ref<T>;          // required alias template substitution, fails if T is void
  typename [:T::r1:];       // fails if T​::​r1 is not a reflection of a type
  typename [:T::r2:]<int>;  // fails if T​::​r2 is not a reflection of a template Z for which Z<int> is a type
};

//cwg: 3056
//title: Missing semicolons in grammar for type-requirement
//meeting: Kona 11/25
//edg_status: EDGcpfe/28541
