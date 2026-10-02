struct type_with_dtor {
  ~type_with_dtor();
};

template<typename T>
struct type_with_def_arg {
  type_with_def_arg(T a = T());
};

struct type_with_def_init {
  type_with_def_arg<type_with_dtor> val{};
};
