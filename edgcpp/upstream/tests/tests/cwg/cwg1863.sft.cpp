//type:fn
//options_all:--c++17 -tused -A
struct B {
   B() = default;
   B(const B &) = default;
   B(B &&) = default;

   template <typename T>
   B(T &t) { where_is_this(t); }
};

void foo() {
   throw B(); // causes ill-formed instantiation of the constructor template
}

//cwg: 1863
//title: Requirements on thrown object type to support std::current_exception()
//meeting: Kona 10/15
//edg_status: EDGcpfe/22216
