//type: fp
//options:  --c++20
# 1 "Sema/warn-lifetime-analysis-capture-by.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 489 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "Sema/warn-lifetime-analysis-capture-by.cpp" 2


# 1 "Sema/Inputs/lifetime-analysis.h" 1

namespace __gnu_cxx {
template <typename T>
struct basic_iterator {
  basic_iterator operator++();
  T& operator*() const;
  T* operator->() const;
};

template<typename T>
bool operator!=(basic_iterator<T>, basic_iterator<T>);
}

namespace std {
template<typename T> struct remove_reference { typedef T type; };
template<typename T> struct remove_reference<T &> { typedef T type; };
template<typename T> struct remove_reference<T &&> { typedef T type; };

template<typename T>
typename remove_reference<T>::type &&move(T &&t) noexcept;

template <typename C>
auto data(const C &c) -> decltype(c.data());

template <typename C>
auto begin(C &c) -> decltype(c.begin());

template<typename T, int N>
T *begin(T (&array)[N]);

using size_t = decltype(sizeof(0));

template<typename T>
struct initializer_list {
  const T* ptr; size_t sz;
};
template<typename T> class allocator {};
template <typename T, typename Alloc = allocator<T>>
struct vector {
  typedef __gnu_cxx::basic_iterator<T> iterator;
  iterator begin();
  iterator end();
  const T *data() const;
  vector();
  vector(initializer_list<T> __l,
         const Alloc& alloc = Alloc());

  template<typename InputIterator>
 vector(InputIterator first, InputIterator __last);

  T &at(int n);

  void push_back(const T&);
  void push_back(T&&);
  const T& back() const;
  void insert(iterator, T&&);
};

template<typename T>
struct basic_string_view {
  basic_string_view();
  basic_string_view(const T *);
  const T *begin() const;
  const T *data() const;
};
using string_view = basic_string_view<char>;

template<class _Mystr> struct iter {
    iter& operator-=(int);

    iter operator-(int _Off) const {
        iter _Tmp = *this;
        return _Tmp -= _Off;
    }
};

template<typename T>
struct basic_string {
  basic_string();
  basic_string(const T *);
  const T *c_str() const;
  operator basic_string_view<T> () const;
  using const_iterator = iter<T>;
  const T *data() const;
};
using string = basic_string<char>;

template<typename T>
struct unique_ptr {
  T &operator*();
  T *get() const;
};

template<typename T>
struct optional {
  optional();
  optional(const T&);

  template<typename U = T>
  optional(U&& t);

  template<typename U>
  optional(optional<U>&& __t);

  T &operator*() &;
  T &&operator*() &&;
  T &value() &;
  T &&value() &&;
};
template<typename T>
optional<__decay(T)> make_optional(T&&);


template<typename T>
struct stack {
  T &top();
};

struct any {};

template<typename T>
T any_cast(const any& operand);

template<typename T>
struct reference_wrapper {
  template<typename U>
  reference_wrapper(U &&);
};

template<typename T>
reference_wrapper<T> ref(T& t) noexcept;

template <typename T>
struct [[gsl::Pointer]] iterator {
  T& operator*() const;
};

struct false_type {
    static constexpr bool value = false;
    constexpr operator bool() const noexcept { return value; }
};
struct true_type {
    static constexpr bool value = true;
    constexpr operator bool() const noexcept { return value; }
};

template<class T> struct is_pointer : false_type {};
template<class T> struct is_pointer<T*> : true_type {};
template<class T> struct is_pointer<T* const> : true_type {};
}
# 4 "Sema/warn-lifetime-analysis-capture-by.cpp" 2




namespace capture_int {
struct X {} x;
void captureInt(const int &i [[clang::lifetime_capture_by(x)]], X &x);
void captureRValInt(int &&i [[clang::lifetime_capture_by(x)]], X &x);
void noCaptureInt(int i [[clang::lifetime_capture_by(x)]], X &x);

void use() {
  int local;
  captureInt(1,
            x);
  captureRValInt(1, x);
  captureInt(local, x);
  noCaptureInt(1, x);
  noCaptureInt(local, x);
}
}




namespace capture_string {
struct X {} x;
void captureString(const std::string &s [[clang::lifetime_capture_by(x)]], X &x);
void captureRValString(std::string &&s [[clang::lifetime_capture_by(x)]], X &x);

void use() {
  std::string local_string;
  captureString(std::string(), x);
  captureString(local_string, x);
  captureRValString(std::move(local_string), x);
  captureRValString(std::string(), x);
}
}




namespace capture_string_view {
struct X {} x;
void captureStringView(std::string_view s [[clang::lifetime_capture_by(x)]], X &x);
void captureRValStringView(std::string_view &&sv [[clang::lifetime_capture_by(x)]], X &x);
void noCaptureStringView(std::string_view sv, X &x);

std::string_view getLifetimeBoundView(const std::string& s [[clang::lifetimebound]]);
std::string_view getNotLifetimeBoundView(const std::string& s);
const std::string& getLifetimeBoundString(const std::string &s [[clang::lifetimebound]]);
const std::string& getLifetimeBoundString(std::string_view sv [[clang::lifetimebound]]);

void use() {
  std::string_view local_string_view;
  std::string local_string;
  captureStringView(local_string_view, x);
  captureStringView(std::string(),
            x);

  captureStringView(getLifetimeBoundView(local_string), x);
  captureStringView(getNotLifetimeBoundView(std::string()), x);
  captureRValStringView(std::move(local_string_view), x);
  captureRValStringView(std::string(), x);
  captureRValStringView(std::string_view{"abcd"}, x);

  noCaptureStringView(local_string_view, x);
  noCaptureStringView(std::string(), x);


  captureStringView(getLifetimeBoundView(
  std::string()
  ), x);
  captureRValStringView(getLifetimeBoundView(local_string), x);
  captureRValStringView(getLifetimeBoundView(std::string()), x);
  captureRValStringView(getNotLifetimeBoundView(std::string()), x);
  noCaptureStringView(getLifetimeBoundView(std::string()), x);
  captureStringView(getLifetimeBoundString(std::string()), x);
  captureStringView(getLifetimeBoundString(getLifetimeBoundView(std::string())), x);
  captureStringView(getLifetimeBoundString(getLifetimeBoundString(
    std::string()
    )), x);
}
}




const std::string* getLifetimeBoundPointer(const std::string &s [[clang::lifetimebound]]);
const std::string* getNotLifetimeBoundPointer(const std::string &s);

namespace capture_pointer {
struct X {} x;
void capturePointer(const std::string* sp [[clang::lifetime_capture_by(x)]], X &x);
void use() {
  capturePointer(getLifetimeBoundPointer(std::string()), x);
  capturePointer(getLifetimeBoundPointer(*getLifetimeBoundPointer(
    std::string()
    )), x);
  capturePointer(getNotLifetimeBoundPointer(std::string()), x);

}
}




namespace init_lists {
struct X {} x;
void captureVector(const std::vector<int> &a [[clang::lifetime_capture_by(x)]], X &x);
void captureArray(int array [[clang::lifetime_capture_by(x)]] [2], X &x);
void captureInitList(std::initializer_list<int> abc [[clang::lifetime_capture_by(x)]], X &x);


std::initializer_list<int> getLifetimeBoundInitList(std::initializer_list<int> abc [[clang::lifetimebound]]);

void use() {
  captureVector({1, 2, 3}, x);
  captureVector(std::vector<int>{}, x);
  std::vector<int> local_vector;
  captureVector(local_vector, x);
  int local_array[2];
  captureArray(local_array, x);
  captureInitList({1, 2}, x);
  captureInitList(getLifetimeBoundInitList({1, 2}), x);
}
}




namespace this_is_captured {
struct X {} x;
struct S {
  void capture(X &x) [[clang::lifetime_capture_by(x)]];
};
void use() {
  S{}.capture(x);
  S s;
  s.capture(x);
}
}

namespace temporary_capturing_object {
struct S {
  void add(const int& x [[clang::lifetime_capture_by(this)]]);
};

void test() {



  S().add(1);
  S{}.add(1);
}
}




namespace capture_by_global_unknown {
void captureByGlobal(std::string_view s [[clang::lifetime_capture_by(global)]]);
void captureByUnknown(std::string_view s [[clang::lifetime_capture_by(unknown)]]);

std::string_view getLifetimeBoundView(const std::string& s [[clang::lifetimebound]]);

void use() {
  std::string_view local_string_view;
  std::string local_string;

  captureByGlobal(std::string());
  captureByGlobal(getLifetimeBoundView(std::string()));
  captureByGlobal(local_string);
  captureByGlobal(local_string_view);


  captureByUnknown(std::string());
  captureByUnknown(getLifetimeBoundView(std::string()));
  captureByUnknown(local_string);
  captureByUnknown(local_string_view);
}
}




namespace capture_by_this {
struct S {
  void captureInt(const int& x [[clang::lifetime_capture_by(this)]]);
  void captureView(std::string_view sv [[clang::lifetime_capture_by(this)]]);
};
std::string_view getLifetimeBoundView(const std::string& s [[clang::lifetimebound]]);
std::string_view getNotLifetimeBoundView(const std::string& s);
const std::string& getLifetimeBoundString(const std::string &s [[clang::lifetimebound]]);

void use() {
  S s;
  s.captureInt(1);
  s.captureView(std::string());
  s.captureView(getLifetimeBoundView(std::string()));
  s.captureView(getLifetimeBoundString(std::string()));
  s.captureView(getNotLifetimeBoundView(std::string()));
}
}




namespace reference_field {
struct X {} x;
struct Foo {
  const int& b;
};
void captureField(Foo param [[clang::lifetime_capture_by(x)]], X &x);
void use() {
  captureField(Foo{
    1
  }, x);
  int local;
  captureField(Foo{local}, x);
}
}




namespace default_arg {
struct X {} x;
void captureDefaultArg(X &x, std::string_view s [[clang::lifetime_capture_by(x)]] = std::string());

std::string_view getLifetimeBoundView(const std::string& s [[clang::lifetimebound]]);

void useCaptureDefaultArg() {
  X x;
  captureDefaultArg(x);
  captureDefaultArg(x, std::string("temp"));
  captureDefaultArg(x, getLifetimeBoundView(std::string()));
  std::string local;
  captureDefaultArg(x, local);
}
}




namespace containers_no_distinction {
template<class T>
struct MySet {
  void insert(T&& t [[clang::lifetime_capture_by(this)]]);
  void insert(const T& t [[clang::lifetime_capture_by(this)]]);
};
void user_defined_containers() {
  MySet<int> set_of_int;
  set_of_int.insert(1);
  MySet<std::string_view> set_of_sv;
  set_of_sv.insert(std::string());
  set_of_sv.insert(std::string_view());
}
}




namespace conatiners_with_different {
template<typename T> struct IsPointerLikeTypeImpl : std::false_type {};
template<> struct IsPointerLikeTypeImpl<std::string_view> : std::true_type {};
template<typename T> concept IsPointerLikeType = std::is_pointer<T>::value || IsPointerLikeTypeImpl<T>::value;

template<class T> struct MyVector {
  void push_back(T&& t [[clang::lifetime_capture_by(this)]]) requires IsPointerLikeType<T>;
  void push_back(const T& t [[clang::lifetime_capture_by(this)]]) requires IsPointerLikeType<T>;

  void push_back(T&& t) requires (!IsPointerLikeType<T>);
  void push_back(const T& t) requires (!IsPointerLikeType<T>);
};

std::string_view getLifetimeBoundView(const std::string& s [[clang::lifetimebound]]);

void use_container() {
  std::string local;

  MyVector<std::string> vector_of_string;
  vector_of_string.push_back(std::string());

  MyVector<std::string_view> vector_of_view;
  vector_of_view.push_back(std::string());
  vector_of_view.push_back(getLifetimeBoundView(std::string()));

  MyVector<const std::string*> vector_of_pointer;
  vector_of_pointer.push_back(getLifetimeBoundPointer(std::string()));
  vector_of_pointer.push_back(getLifetimeBoundPointer(*getLifetimeBoundPointer(std::string())));
  vector_of_pointer.push_back(getLifetimeBoundPointer(local));
  vector_of_pointer.push_back(getNotLifetimeBoundPointer(std::string()));
}




struct [[gsl::Pointer()]] MyStringView : public std::string_view {
  MyStringView();
  MyStringView(std::string_view&&);
  MyStringView(const MyStringView&);
  MyStringView(const std::string&);
};
template<> struct IsPointerLikeTypeImpl<MyStringView> : std::true_type {};

std::optional<std::string_view> getOptionalSV();
std::optional<std::string> getOptionalS();
std::optional<MyStringView> getOptionalMySV();
MyStringView getMySV();

class MyStringViewNotPointer : public std::string_view {};
std::optional<MyStringViewNotPointer> getOptionalMySVNotP();
MyStringViewNotPointer getMySVNotP();

std::string_view getLifetimeBoundView(const std::string& s [[clang::lifetimebound]]);
std::string_view getNotLifetimeBoundView(const std::string& s);
const std::string& getLifetimeBoundString(const std::string &s [[clang::lifetimebound]]);
const std::string& getLifetimeBoundString(std::string_view sv [[clang::lifetimebound]]);

void use_my_view() {
  std::string local;
  MyVector<MyStringView> vector_of_my_view;
  vector_of_my_view.push_back(getMySV());
  vector_of_my_view.push_back(MyStringView{});
  vector_of_my_view.push_back(std::string_view{});
  vector_of_my_view.push_back(std::string{});
  vector_of_my_view.push_back(getLifetimeBoundView(std::string{}));
  vector_of_my_view.push_back(getLifetimeBoundString(getLifetimeBoundView(std::string{})));
  vector_of_my_view.push_back(getNotLifetimeBoundView(getLifetimeBoundString(getLifetimeBoundView(std::string{}))));


  MyVector<std::string_view> vector_of_view;
  vector_of_view.push_back(getMySV());
  vector_of_view.push_back(getMySVNotP());
}




void use_with_optional_view() {
  MyVector<std::string_view> vector_of_view;

  std::optional<std::string_view> optional_of_view;
  vector_of_view.push_back(optional_of_view.value());
  vector_of_view.push_back(getOptionalS().value());

  vector_of_view.push_back(getOptionalSV().value());
  vector_of_view.push_back(getOptionalMySV().value());
  vector_of_view.push_back(getOptionalMySVNotP().value());
}
}




namespace temporary_views {
void capture1(std::string_view s [[clang::lifetime_capture_by(x)]], std::vector<std::string_view>& x);


void capture2(const std::string_view& s [[clang::lifetime_capture_by(x)]], std::vector<std::string_view*>& x);

void capture3(const std::string_view& s [[clang::lifetime_capture_by(x)]], std::vector<std::string_view>& x);

void use() {
  std::vector<std::string_view> x1;
  capture1(std::string(), x1);
  capture1(std::string_view(), x1);

  std::vector<std::string_view*> x2;


  capture2(std::string_view(), x2);
  capture2(std::string(), x2);

  std::vector<std::string_view> x3;
  capture3(std::string_view(), x3);
  capture3(std::string(), x3);
}
}




namespace inferred_capture_by {
const std::string* getLifetimeBoundPointer(const std::string &s [[clang::lifetimebound]]);
const std::string* getNotLifetimeBoundPointer(const std::string &s);

std::string_view getLifetimeBoundView(const std::string& s [[clang::lifetimebound]]);
std::string_view getNotLifetimeBoundView(const std::string& s);
void use() {
  std::string local;
  std::vector<std::string_view> views;
  views.push_back(std::string());
  views.insert(views.begin(),
            std::string());
  views.push_back(getLifetimeBoundView(std::string()));
  views.push_back(getNotLifetimeBoundView(std::string()));
  views.push_back(local);
  views.insert(views.end(), local);

  std::vector<std::string> strings;
  strings.push_back(std::string());
  strings.insert(strings.begin(), std::string());

  std::vector<const std::string*> pointers;
  pointers.push_back(getLifetimeBoundPointer(std::string()));
  pointers.push_back(&local);
}

namespace with_span {

template<typename T>
struct [[gsl::Pointer]] Span {
  Span(const std::vector<T> &V);
};

void use() {
  std::vector<Span<int>> spans;
  spans.push_back(std::vector<int>{1, 2, 3});
  std::vector<int> local;
  spans.push_back(local);
}
}
}

namespace on_constructor {
struct T {
  T(const int& t [[clang::lifetime_capture_by(this)]]);
};
struct T2 {
  T2(const int& t [[clang::lifetime_capture_by(x)]], int& x);
};
struct T3 {
  T3(const T& t [[clang::lifetime_capture_by(this)]]);
};

int foo(const T& t);
int bar(const T& t[[clang::lifetimebound]]);

void test() {
  auto x = foo(T(1));
  T(1);
  T t(1);
  auto y = bar(T(1));
  T3 t3(T(1));

  int a;
  T2(1, a);
}
}

namespace GH121391 {

struct Foo {};

template <typename T>
struct Container {
  const T& tt() [[clang::lifetimebound]];
};
template<typename T>
struct StatusOr {
   T* get() [[clang::lifetimebound]];
};
StatusOr<Container<const Foo*>> getContainer();

void test() {
  std::vector<const Foo*> vv;
  vv.push_back(getContainer().get()->tt());
}

}
