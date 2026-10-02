//type: fn
//options:  --c++20
# 1 "SemaCXX/builtin-dump-struct.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/builtin-dump-struct.cpp" 2


namespace std {
  typedef decltype(sizeof(int)) size_t;

  template <class E> struct initializer_list {
    const E *data;
    size_t size;

    constexpr initializer_list(const E *data, size_t size)
        : data(data), size(size) {}
    constexpr initializer_list() : data(), size() {}

    constexpr const E *begin() const { return data; }
    constexpr const E *end() const { return data + size; }
  };
}

struct ConstexprString {
  constexpr ConstexprString() : ConstexprString("") {}
  constexpr ConstexprString(const char *p, std::size_t size) : data(new char[size+1]) {
    __builtin_memcpy(data, p, size);
    data[size] = '\0';
  }
  constexpr ConstexprString(const char *p) : ConstexprString(p, __builtin_strlen(p)) {}
  constexpr explicit ConstexprString(const char *p, const char *q) : data(nullptr) {
    auto p_size = __builtin_strlen(p);
    auto q_size = __builtin_strlen(q);
    data = new char[p_size + q_size + 1];
    __builtin_memcpy(data, p, p_size);
    __builtin_memcpy(data + p_size, q, q_size + 1);
  }
  constexpr ConstexprString(const ConstexprString &o) : ConstexprString(o.data) {}
  constexpr ConstexprString(ConstexprString &&o) : data(o.data) { o.data = nullptr; }
  constexpr ConstexprString &operator=(const ConstexprString &o) {
    return *this = ConstexprString(o);
  }
  constexpr ConstexprString &operator=(ConstexprString &&o) {
    delete[] data;
    data = o.data;
    o.data = nullptr;
    return *this;
  }
  constexpr ~ConstexprString() { delete[] data; }
  char *data;

  friend constexpr ConstexprString operator+(const ConstexprString &a, const ConstexprString &b) {
    return ConstexprString(a.data, b.data);
  }
  friend constexpr ConstexprString &operator+=(ConstexprString &a, const ConstexprString &b) {
    return a = a + b;
  }
  friend constexpr bool operator==(const ConstexprString &a, const ConstexprString &b) {
    return __builtin_strcmp(a.data, b.data) == 0;
  }
};

template<typename... T> constexpr void Format(ConstexprString &out, const char *fmt, T... args);

struct Arg {
  template<typename T, int (*)[__is_integral(T) ? 1 : -1] = nullptr>
  constexpr Arg(T value) {
    bool negative = false;
    if (value < 0) {
      value = -value;
      negative = true;
    }
    while (value > 0) {
      char str[2] = {char('0' + value % 10), '\0'};
      s = ConstexprString(str) + s;
      value /= 10;
    }
    if (negative)
      s = "-" + s;
  }
  template<typename T, int (*)[__is_class(T) ? 1 : -1] = nullptr>
  constexpr Arg(const T &value) {
    __builtin_dump_struct(&value, Format, s);
  }
  constexpr Arg(const char *s) : s(s) {}
  constexpr Arg(const ConstexprString *s) : s("\"" + *s + "\"") {}
  template<typename T, int (*)[__is_integral(T) ? 1 : -1] = nullptr>
  constexpr Arg(const T *p) : s("reference to " + Arg(*p).s) {}
  ConstexprString s;
};

template<typename... T> constexpr void Format(ConstexprString &out, const char *fmt, T... args) {
  Arg formatted_args[] = {args...};
  int i = 0;
  while (const char *percent = __builtin_strchr(fmt, '%')) {
    if (percent[1] == '%') continue;
    if (percent != fmt && percent[-1] == '*') --percent;
    out += ConstexprString(fmt, percent - fmt);
    out += formatted_args[i++].s;


    fmt = percent;
    while (!__builtin_strchr("diouxXeEfFgGcsp", *fmt)) ++fmt;

    ++fmt;
  }
  out += ConstexprString(fmt);
}

template<typename T> constexpr ConstexprString ToString(const T &t) { return Arg(t).s; }

struct A {
  int x, y, z : 3;
  int : 4;
  ConstexprString s;
};
struct B : A {
  int p, q;
  struct {
    int anon1, anon2;
  };
  union {
    int anon3;
  };
  struct {
    int m;
  } c;
  int &&r;
};







static_assert(ToString(B{1, 2, 3, "hello", 4, 5, 6, 7, 8, 9, 10}) == &R"(
B {
  A {
    int x = 1
    int y = 2
    int z : 3 = 3
    ConstexprString s = "hello"
  }
  int p = 4
  int q = 5
  int anon1 = 6
  int anon2 = 7
  int anon3 = 8
  struct (unnamed) c = {
    int m = 9
  }
  int && r = reference to 10
}
)"[1]);
# 152 "SemaCXX/builtin-dump-struct.cpp"
class Incomplete;

template <class T>
class Class {
  T value = {};
};

void errors(B b) {
  ConstexprString cs;
  __builtin_dump_struct();
  __builtin_dump_struct(1);
  __builtin_dump_struct(1, 2);
  __builtin_dump_struct(&b, 2);
  __builtin_dump_struct(&b, Format, 0);


  __builtin_dump_struct((Incomplete *)nullptr, Format, cs);


  __builtin_dump_struct((Class<int> *)nullptr, Format, cs);
}




struct t1 {
  int v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16,
      v17, v18, v19, v20, v21, v22, v23, v24, v25, v26, v27, v28, v29, v30, v31,
      v32, v33, v34, v35, v36, v37, v38, v39, v40, v41, v42, v43, v44, v45, v46,
      v47, v48, v49, v50, v51, v52, v53, v54, v55, v56, v57, v58, v59, v60, v61,
      v62, v63, v64, v65, v66, v67, v68, v69, v70, v71, v72, v73, v74, v75, v76,
      v77, v78, v79, v80, v81, v82, v83, v84, v85, v86, v87, v88, v89, v90, v91,
      v92, v93, v94, v95, v96, v97, v98, v99;
};

struct t2 {
  t1 v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16,
      v17, v18, v19, v20, v21, v22, v23, v24, v25, v26, v27, v28, v29, v30, v31,
      v32, v33, v34, v35, v36, v37, v38, v39, v40, v41, v42, v43, v44, v45, v46,
      v47, v48, v49, v50, v51, v52, v53, v54, v55, v56, v57, v58, v59, v60, v61,
      v62, v63, v64, v65, v66, v67, v68, v69, v70, v71, v72, v73, v74, v75, v76,
      v77, v78, v79, v80, v81, v82, v83, v84, v85, v86, v87, v88, v89, v90, v91,
      v92, v93, v94, v95, v96, v97, v98, v99;
};

int printf(const char *, ...);
void f1(t2 w) { __builtin_dump_struct(&w, printf); }
