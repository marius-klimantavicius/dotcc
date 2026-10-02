struct type_with_dtor {
  ~type_with_dtor();
};

template<typename T>
struct type_with_def_arg {
  type_with_def_arg(T a = T());
};

template<typename T>
struct type_with_def_init {
  type_with_def_arg<T> val{};
};

extern template class type_with_def_init<type_with_dtor>;
