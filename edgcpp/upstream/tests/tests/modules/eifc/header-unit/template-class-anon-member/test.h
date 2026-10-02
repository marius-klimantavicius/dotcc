template<typename a_Value_type>
struct simple_container {
  union {
    a_Value_type stored_value;
  };
};
