//type: fp
//options: 
# 1 "Sema/warn-lifetime-analysis-nocfg.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 479 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "Sema/warn-lifetime-analysis-nocfg.cpp" 2

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
# 3 "Sema/warn-lifetime-analysis-nocfg.cpp" 2
struct [[gsl::Owner(int)]] MyIntOwner {
  MyIntOwner();
  int &operator*();
};

struct [[gsl::Pointer(int)]] MyIntPointer {
  MyIntPointer(int *p = nullptr);



  MyIntPointer(const MyIntOwner &);
  int &operator*();
  MyIntOwner toOwner();
};

struct MySpecialIntPointer : MyIntPointer {
};



struct [[gsl::Owner(int)]] MyOwnerIntPointer : MyIntPointer {
};

struct [[gsl::Pointer(long)]] MyLongPointerFromConversion {
  MyLongPointerFromConversion(long *p = nullptr);
  long &operator*();
};

struct [[gsl::Owner(long)]] MyLongOwnerWithConversion {
  MyLongOwnerWithConversion();
  operator MyLongPointerFromConversion();
  long &operator*();
  MyIntPointer releaseAsMyPointer();
  long *releaseAsRawPointer();
};

void danglingHeapObject() {
  new MyLongPointerFromConversion(MyLongOwnerWithConversion{});
  new MyIntPointer(MyIntOwner{});
}

void intentionalFalseNegative() {
  int i;
  MyIntPointer p{&i};


  new MyIntPointer(p);
  new MyIntPointer(MyIntPointer{p});
}

MyIntPointer ownershipTransferToMyPointer() {
  MyLongOwnerWithConversion t;
  return t.releaseAsMyPointer();
}

long *ownershipTransferToRawPointer() {
  MyLongOwnerWithConversion t;
  return t.releaseAsRawPointer();
}

struct Y {
  int a[4];
};

void dangligGslPtrFromTemporary() {
  MyIntPointer p = Y{}.a;
  (void)p;
}

struct DanglingGslPtrField {
  MyIntPointer p;
  MyLongPointerFromConversion p2;
  DanglingGslPtrField(int i) : p(&i) {}
  DanglingGslPtrField() : p2(MyLongOwnerWithConversion{}) {}
  DanglingGslPtrField(double) : p(MyIntOwner{}) {}
};

MyIntPointer danglingGslPtrFromLocal() {
  int j;
  return &j;
}

MyIntPointer returningLocalPointer() {
  MyIntPointer localPointer;
  return localPointer;
}

MyIntPointer daglingGslPtrFromLocalOwner() {
  MyIntOwner localOwner;
  return localOwner;
}

MyLongPointerFromConversion daglingGslPtrFromLocalOwnerConv() {
  MyLongOwnerWithConversion localOwner;
  return localOwner;
}

MyIntPointer danglingGslPtrFromTemporary() {
  return MyIntOwner{};
}

MyIntOwner makeTempOwner();

MyIntPointer danglingGslPtrFromTemporary2() {
  return makeTempOwner();
}

MyLongPointerFromConversion danglingGslPtrFromTemporaryConv() {
  return MyLongOwnerWithConversion{};
}

int *noFalsePositive(MyIntOwner &o) {
  MyIntPointer p = o;
  return &*p;
}

MyIntPointer global;
MyLongPointerFromConversion global2;

void initLocalGslPtrWithTempOwner() {
  MyIntPointer p = MyIntOwner{};
  MyIntPointer pp = p = MyIntOwner{};
  p = MyIntOwner{};
  pp = p;
  global = MyIntOwner{};
  MyLongPointerFromConversion p2 = MyLongOwnerWithConversion{};
  p2 = MyLongOwnerWithConversion{};
  global2 = MyLongOwnerWithConversion{};
}


struct Unannotated {
  typedef std::vector<int>::iterator iterator;
  iterator begin();
  operator iterator() const;
};

void modelIterators() {
  std::vector<int>::iterator it = std::vector<int>().begin();
  (void)it;
}

std::vector<int>::iterator modelIteratorReturn() {
  return std::vector<int>().begin();
}

const int *modelFreeFunctions() {
  return std::data(std::vector<int>());
}

int &modelAnyCast() {
  return std::any_cast<int&>(std::any{});
}

int modelAnyCast2() {
  return std::any_cast<int>(std::any{});
}

int modelAnyCast3() {
  return std::any_cast<int&>(std::any{});
}

const char *danglingRawPtrFromLocal() {
  std::basic_string<char> s;
  return s.c_str();
}

int &danglingRawPtrFromLocal2() {
  std::optional<int> o;
  return o.value();
}

int &danglingRawPtrFromLocal3() {
  std::optional<int> o;
  return *o;
}


std::string_view containerWithAnnotatedElements() {
  std::string_view c1 = std::vector<std::string>().at(0);
  c1 = std::vector<std::string>().at(0);


  std::string_view c2 = std::vector<std::string_view>().at(0);

  std::vector<std::string> local;
  return local.at(0);
}

std::string_view localUniquePtr(int i) {
  std::unique_ptr<std::string> c1;
  if (i)
    return *c1;
  std::unique_ptr<std::string_view> c2;
  return *c2;
}

std::string_view localOptional(int i) {
  std::optional<std::string> o;
  if (i)
    return o.value();
  std::optional<std::string_view> abc;
  return abc.value();
}

const char *danglingRawPtrFromTemp() {
  return std::basic_string<char>().c_str();
}

std::unique_ptr<int> getUniquePtr();

int *danglingUniquePtrFromTemp() {
  return getUniquePtr().get();
}

int *danglingUniquePtrFromTemp2() {
  return std::unique_ptr<int>().get();
}

void danglingReferenceFromTempOwner() {
  int &&r = *std::optional<int>();
  int &&r2 = *std::optional<int>(5);
  int &&r3 = std::optional<int>(5).value();
  int &r4 = std::vector<int>().at(3);
}

std::vector<int> getTempVec();
std::optional<std::vector<int>> getTempOptVec();

void testLoops() {
  for (auto i : getTempVec())
    ;
  for (auto i : *getTempOptVec())
    ;
}

int &usedToBeFalsePositive(std::vector<int> &v) {
  std::vector<int>::iterator it = v.begin();
  int& value = *it;
  return value;
}

int &doNotFollowReferencesForLocalOwner() {
  std::unique_ptr<int> localOwner;
  int &p = *localOwner.get();

  return p;
}

const char *trackThroughMultiplePointer() {
  return std::basic_string_view<char>(std::basic_string<char>()).begin();
}

struct X {
  X(std::unique_ptr<int> up) :
    pointee(*up), pointee2(up.get()), pointer(std::move(up)) {}
  int &pointee;
  int *pointee2;
  std::unique_ptr<int> pointer;
};

struct [[gsl::Owner]] XOwner {
  int* get() const [[clang::lifetimebound]];
};
struct X2 {


  X2(XOwner owner) :
    pointee(owner.get()),
    owner(std::move(owner)) {}
  int* pointee;
  XOwner owner;
};

std::vector<int>::iterator getIt();
std::vector<int> getVec();

const int &handleGslPtrInitsThroughReference() {
  const auto &it = getIt();
  return *it;
}

void handleGslPtrInitsThroughReference2() {
  const std::vector<int> &v = getVec();
  const int *val = v.data();
}

void handleTernaryOperator(bool cond) {
    std::basic_string<char> def;
    std::basic_string_view<char> v = cond ? def : "";
}

std::string operator+(std::string_view s1, std::string_view s2);
void danglingStringviewAssignment(std::string_view a1, std::string_view a2) {
  a1 = std::string();
  a2 = a1 + a1;
}

std::reference_wrapper<int> danglingPtrFromNonOwnerLocal() {
  int i = 5;
  return i;
}

std::reference_wrapper<int> danglingPtrFromNonOwnerLocal2() {
  int i = 5;
  return std::ref(i);
}

std::reference_wrapper<int> danglingPtrFromNonOwnerLocal3() {
  int i = 5;
  return std::reference_wrapper<int>(i);
}

std::reference_wrapper<Unannotated> danglingPtrFromNonOwnerLocal4() {
  Unannotated i;
  return std::reference_wrapper<Unannotated>(i);
}

std::reference_wrapper<Unannotated> danglingPtrFromNonOwnerLocal5() {
  Unannotated i;
  return std::ref(i);
}

int *returnPtrToLocalArray() {
  int a[5];
  return std::begin(a);
}

struct ptr_wrapper {
  std::vector<int>::iterator member;
};

ptr_wrapper getPtrWrapper();

std::vector<int>::iterator returnPtrFromWrapper() {
  ptr_wrapper local = getPtrWrapper();
  return local.member;
}

std::vector<int>::iterator returnPtrFromWrapperThroughRef() {
  ptr_wrapper local = getPtrWrapper();
  ptr_wrapper &local2 = local;
  return local2.member;
}

std::vector<int>::iterator returnPtrFromWrapperThroughRef2() {
  ptr_wrapper local = getPtrWrapper();
  std::vector<int>::iterator &local2 = local.member;
  return local2;
}

void checkPtrMemberFromAggregate() {
  std::vector<int>::iterator local = getPtrWrapper().member;
}

std::vector<int>::iterator doNotInterferWithUnannotated() {
  Unannotated value;

  return std::begin(value);
}

std::vector<int>::iterator doNotInterferWithUnannotated2() {
  Unannotated value;
  return value;
}

std::vector<int>::iterator supportDerefAddrofChain(int a, std::vector<int>::iterator value) {
  switch (a) {
    default:
      return value;
    case 1:
      return *&value;
    case 2:
      return *&*&value;
    case 3:
      return *&*&*&value;
  }
}

int &supportDerefAddrofChain2(int a, std::vector<int>::iterator value) {
  switch (a) {
    default:
      return *value;
    case 1:
      return **&value;
    case 2:
      return **&*&value;
    case 3:
      return **&*&*&value;
  }
}

int *supportDerefAddrofChain3(int a, std::vector<int>::iterator value) {
  switch (a) {
    default:
      return &*value;
    case 1:
      return &*&*value;
    case 2:
      return &*&**&value;
    case 3:
      return &*&**&*&value;
  }
}

MyIntPointer handleDerivedToBaseCast1(MySpecialIntPointer ptr) {
  return ptr;
}

MyIntPointer handleDerivedToBaseCast2(MyOwnerIntPointer ptr) {
  return ptr;
}

std::vector<int>::iterator noFalsePositiveWithVectorOfPointers() {
  std::vector<std::vector<int>::iterator> iters;
  return iters.at(0);
}

void testForBug49342()
{
  auto it = std::iter<char>{} - 2;
}

namespace GH93386 {

struct [[gsl::Pointer]] S {
  S(const std::vector<int>& abc [[clang::lifetimebound]]);
};

S test(std::vector<int> a) {
  return S(a);
}

auto s = S(std::vector<int>());


std::string_view test2(int i, std::optional<std::string_view> a) {
  if (i)
    return std::move(*a);
  return std::move(a.value());
}

struct Foo;
struct FooView {
  FooView(const Foo& foo [[clang::lifetimebound]]);
};
FooView test3(int i, std::optional<Foo> a) {
  if (i)
    return *a;
  return a.value();
}
}

namespace GH100549 {
struct UrlAnalyzed {
  UrlAnalyzed(std::string_view url [[clang::lifetimebound]]);
};
std::string StrCat(std::string_view, std::string_view);
void test1() {
  UrlAnalyzed url(StrCat("abc", "bcd"));
}

std::string_view ReturnStringView(std::string_view abc [[clang::lifetimebound]]);

void test() {
  std::string_view svjkk1 = ReturnStringView(StrCat("bar", "x"));
}
}

namespace GH108272 {
template <typename T>
struct [[gsl::Owner]] StatusOr {
  const T &value() [[clang::lifetimebound]];
};

template <typename V>
class Wrapper1 {
 public:
  operator V() const;
  V value;
};
std::string_view test1() {
  StatusOr<Wrapper1<std::string_view>> k;


  std::string_view good = StatusOr<Wrapper1<std::string_view>>().value();
  return k.value();
}

template <typename V>
class Wrapper2 {
 public:
  operator V() const [[clang::lifetimebound]];
  V value;
};
std::string_view test2() {
  StatusOr<Wrapper2<std::string_view>> k;

  std::string_view bad = StatusOr<Wrapper2<std::string_view>>().value();
  return k.value();
}
}

namespace GH100526 {
void test() {
  std::vector<std::string_view> v1({std::string()});
  std::vector<std::string_view> v2({
    std::string(),
    std::string_view()
  });
  std::vector<std::string_view> v3({
    std::string_view(),
    std::string()
  });

  std::optional<std::string_view> o1 = std::string();

  std::string s;




  std::optional<std::string_view> o2 = std::make_optional(s);
  std::optional<std::string_view> o3 = std::optional<std::string>(s);
  std::optional<std::string_view> o4 = std::optional<std::string_view>(s);


  v1 = {std::string()};
  o1 = std::string();


  std::vector<std::string_view> n1 = {std::string_view()};
  std::optional<std::string_view> n2 = {std::string_view()};
  std::optional<std::string_view> n3 = std::string_view();
  std::optional<std::string_view> n4 = std::make_optional(std::string_view());
  const char* b = "";
  std::optional<std::string_view> n5 = std::make_optional(b);
  std::optional<std::string_view> n6 = std::make_optional("test");
}

std::vector<std::string_view> test2(int i) {
  std::vector<std::string_view> t;
  if (i)
    return t;
  return std::vector<std::string_view>(t.begin(), t.end());
}

class Foo {
  public:
   operator std::string_view() const { return ""; }
};
class [[gsl::Owner]] FooOwner {
  public:
   operator std::string_view() const { return ""; }
};
std::optional<Foo> GetFoo();
std::optional<FooOwner> GetFooOwner();

template <typename T>
struct [[gsl::Owner]] Container1 {
   Container1();
};
template <typename T>
struct [[gsl::Owner]] Container2 {
  template<typename U>
  Container2(const Container1<U>& C2);
};

std::optional<std::string_view> test3(int i) {
  std::string s;
  std::string_view sv;
  if (i)
   return s;
  return sv;
  Container2<std::string_view> c1 = Container1<Foo>();
  Container2<std::string_view> c2 = Container1<FooOwner>();
  return GetFoo();
  return GetFooOwner();
}

std::optional<int*> test4(int a) {
  return std::make_optional(nullptr);
}


template <typename T>
struct [[gsl::Owner]] StatusOr {
  const T &valueLB() const [[clang::lifetimebound]];
  const T &valueNoLB() const;
};

template<typename T>
struct [[gsl::Pointer]] Span {
  Span(const std::vector<T> &V);

  const int& getFieldLB() const [[clang::lifetimebound]];
  const int& getFieldNoLB() const;
};





std::string_view test5() {

  std::string_view a = StatusOr<std::string_view>().valueLB();
  return StatusOr<std::string_view>().valueLB();


  std::string_view b = StatusOr<std::string_view>().valueNoLB();
  return StatusOr<std::string_view>().valueNoLB();
}



Span<int*> test6(std::vector<int*> v) {
  Span<int *> dangling = std::vector<int*>();
  dangling = std::vector<int*>();
  return v;
}




int* test7(StatusOr<StatusOr<int*>> aa) {

  return aa.valueLB().valueLB();
}


std::vector<int*> test8(StatusOr<std::vector<int*>> aa) {
  return aa.valueLB();
  return aa.valueNoLB();
}


Span<int*> test9(StatusOr<std::vector<int*>> aa) {
  return aa.valueLB();
  return aa.valueNoLB();
}




Span<std::string> test10(StatusOr<std::vector<std::string>> aa) {
  return aa.valueLB();
  return aa.valueNoLB();
}




Span<std::string> test11(StatusOr<Span<std::string>> aa) {
  return aa.valueLB();
  return aa.valueNoLB();
}


const int& test12(Span<int> a) {
  return a.getFieldLB();
  return a.getFieldNoLB();
}

void test13() {

  std::optional<Span<int*>> abc = std::vector<int*>{};

  std::optional<Span<int>> t = std::vector<int> {};
}

}

namespace std {
template <typename T>
class __set_iterator {};

template<typename T>
struct BB {
  typedef __set_iterator<T> iterator;
};

template <typename T>
class set {
public:
  typedef typename BB<T>::iterator iterator;
  iterator begin() const;
};
}
namespace GH118064{

void test() {
  auto y = std::set<int>{}.begin();
}
}

namespace LifetimeboundInterleave {

const std::string& Ref(const std::string& abc [[clang::lifetimebound]]);

std::string_view TakeSv(std::string_view abc [[clang::lifetimebound]]);
std::string_view TakeStrRef(const std::string& abc [[clang::lifetimebound]]);
std::string_view TakeStr(std::string abc [[clang::lifetimebound]]);

std::string_view test1() {
  std::string_view t1 = Ref(std::string());
  t1 = Ref(std::string());
  return Ref(std::string());

  std::string_view t2 = TakeSv(std::string());
  t2 = TakeSv(std::string());
  return TakeSv(std::string());

  std::string_view t3 = TakeStrRef(std::string());
  t3 = TakeStrRef(std::string());
  return TakeStrRef(std::string());


  std::string_view t4 = TakeStr(std::string());
  t4 = TakeStr(std::string());
  return TakeStr(std::string());
}

template <typename T>
struct Foo {
  const T& get() const [[clang::lifetimebound]];
  const T& getNoLB() const;
};
std::string_view test2(Foo<std::string> r1, Foo<std::string_view> r2) {
  std::string_view t1 = Foo<std::string>().get();
  t1 = Foo<std::string>().get();
  return r1.get();

  std::string_view t2 = Foo<std::string_view>().get();
  t2 = Foo<std::string_view>().get();
  return r2.get();


  std::string_view t3 = Foo<std::string>().getNoLB();
  t3 = Foo<std::string>().getNoLB();
  return r1.getNoLB();
}

struct Bar {};
struct [[gsl::Pointer]] Pointer {
  Pointer(const Bar & bar [[clang::lifetimebound]]);
};
Pointer test3(Bar bar) {
  Pointer p = Pointer(Bar());
  p = Pointer(Bar());
  return bar;
}

template<typename T>
struct MySpan {
  MySpan(const std::vector<T>& v);
  using iterator = std::iterator<T>;
  iterator begin() const [[clang::lifetimebound]];
};
template <typename T>
typename MySpan<T>::iterator ReturnFirstIt(const MySpan<T>& v [[clang::lifetimebound]]);

void test4() {
  std::vector<int> v{1};



  const int& t1 = *MySpan<int>(v).begin();
  const int& t2 = *ReturnFirstIt(MySpan<int>(v));


  const int& t4 = *MySpan<int>(std::vector<int>{}).begin();

  auto it1 = MySpan<int>(v).begin();
  auto it2 = ReturnFirstIt(MySpan<int>(v));
}

}

namespace GH120206 {
struct S {
  std::string_view s;
};

struct [[gsl::Owner]] Q1 {
  const S* get() const [[clang::lifetimebound]];
};
std::string_view test1(int c, std::string_view sv) {
  std::string_view k = c > 1 ? Q1().get()->s : sv;
  if (c == 1)
    return c > 1 ? Q1().get()->s : sv;
  Q1 q;
  return c > 1 ? q.get()->s : sv;
}

struct Q2 {
  const S* get() const [[clang::lifetimebound]];
};
std::string_view test2(int c, std::string_view sv) {
  std::string_view k = c > 1 ? Q2().get()->s : sv;
  if (c == 1)
    return c > 1 ? Q2().get()->s : sv;
  Q2 q;
  return c > 1 ? q.get()->s : sv;
}

}

namespace GH120543 {
struct S {
  std::string_view sv;
  std::string s;
};
struct Q {
  const S* get() const [[clang::lifetimebound]];
};

std::string_view foo(std::string_view sv [[clang::lifetimebound]]);

void test1() {
  std::string_view k1 = S().sv;
  std::string_view k2 = S().s;

  std::string_view k3 = Q().get()->sv;
  std::string_view k4 = Q().get()->s;

  std::string_view lb1 = foo(S().s);
  std::string_view lb2 = foo(Q().get()->s);
}

struct Bar {};
struct Foo {
  std::vector<Bar> v;
};
Foo getFoo();
void test2() {
  const Foo& foo = getFoo();
  const Bar& bar = foo.v.back();
}

struct Foo2 {
   std::unique_ptr<Bar> bar;
};

struct Test {
  Test(Foo2 foo) : bar(foo.bar.get()),
      storage(std::move(foo.bar)) {};

  Bar* bar;
  std::unique_ptr<Bar> storage;
};

}

namespace GH127195 {
template <typename T>
struct StatusOr {
  T* operator->() [[clang::lifetimebound]];
  T* value() [[clang::lifetimebound]];
};

const char* foo() {
  StatusOr<std::string> s;
  return s->data();

  StatusOr<std::string_view> s2;
  return s2->data();

  StatusOr<StatusOr<std::string_view>> s3;
  return s3.value()->value()->data();


  StatusOr<StatusOr<std::string>> s4;
  return s4.value()->value()->data();
}

}
