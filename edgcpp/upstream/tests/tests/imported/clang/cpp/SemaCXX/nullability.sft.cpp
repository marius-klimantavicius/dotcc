//type: fn
//options:  --c++11
# 1 "SemaCXX/nullability.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/nullability.cpp" 2
# 12 "SemaCXX/nullability.cpp"
# 1 "SemaCXX/Inputs/nullability-completeness.h" 1
template<typename T> struct Template final {
  int *_Nonnull x;
  T y;
};
# 13 "SemaCXX/nullability.cpp" 2

typedef decltype(nullptr) nullptr_t;

class X {
};


typedef int (X::* _Nonnull member_function_type_1)(int);
typedef int X::* _Nonnull member_data_type_1;
typedef nullptr_t _Nonnull nonnull_nullptr_t;


typedef _Nonnull int (X::* member_function_type_2)(int);
typedef int (X::* _Nonnull member_function_type_3)(int);
typedef _Nonnull int X::* member_data_type_2;


template<typename T>
struct AddNonNull {
  typedef _Nonnull T type;


};

typedef AddNonNull<int *>::type nonnull_int_ptr_1;
typedef AddNonNull<int * _Nullable>::type nonnull_int_ptr_2;
typedef AddNonNull<nullptr_t>::type nonnull_int_ptr_3;

typedef AddNonNull<int>::type nonnull_non_pointer_1;


struct NotPtr{};
typedef AddNonNull<NotPtr>::type nonnull_non_pointer_2;
struct _Nullable SmartPtr{
  SmartPtr();
  SmartPtr(nullptr_t);
  SmartPtr(const SmartPtr&);
  SmartPtr(SmartPtr&&);
  SmartPtr &operator=(const SmartPtr&);
  SmartPtr &operator=(SmartPtr&&);
};
typedef AddNonNull<SmartPtr>::type nonnull_smart_pointer_1;
template<class> struct _Nullable SmartPtrTemplate{};
typedef AddNonNull<SmartPtrTemplate<int>>::type nonnull_smart_pointer_2;
namespace std { inline namespace __1 {
  template <class> class unique_ptr {};
  template <class> class function;
  template <class Ret, class... Args> class function<Ret(Args...)> {};
} }
typedef AddNonNull<std::unique_ptr<int>>::type nonnull_smart_pointer_3;
typedef AddNonNull<std::function<int()>>::type nonnull_smart_pointer_4;

class Derived : public SmartPtr {};
Derived _Nullable x;
class DerivedPrivate : private SmartPtr {};
DerivedPrivate _Nullable y;


template<typename T>
struct AddNonNull2 {
  typedef _Nonnull AddNonNull<T> invalid1;
  typedef _Nonnull AddNonNull2 invalid2;
  typedef _Nonnull AddNonNull2<T> invalid3;
  typedef _Nonnull typename AddNonNull<T>::type okay1;



  typedef _Nonnull AddNonNull<T> (*invalid4);
};


void (*accepts_nonnull_1)(_Nonnull int *ptr);
void (*& accepts_nonnull_2)(_Nonnull int *ptr) = accepts_nonnull_1;
void (X::* accepts_nonnull_3)(_Nonnull int *ptr);
void accepts_nonnull_4(_Nonnull int *ptr);
void (&accepts_nonnull_5)(_Nonnull int *ptr) = accepts_nonnull_4;
void accepts_nonnull_6(SmartPtr _Nonnull);

void test_accepts_nonnull_null_pointer_literal(X *x) {
  accepts_nonnull_1(0);
  accepts_nonnull_2(0);
  (x->*accepts_nonnull_3)(0);
  accepts_nonnull_4(0);
  accepts_nonnull_5(0);

  accepts_nonnull_6(nullptr);
}

template<void FP(_Nonnull int*)>
void test_accepts_nonnull_null_pointer_literal_template() {
  FP(0);
}

template void test_accepts_nonnull_null_pointer_literal_template<&accepts_nonnull_4>();

void TakeNonnull(void *_Nonnull);
void TakeSmartNonnull(SmartPtr _Nonnull);

void AssignAndInitNonNull() {
  void *_Nullable nullable;
  void *_Nonnull p(nullable);
  void *_Nonnull p2{nullable};
  void *_Nonnull p3 = {nullable};
  void *_Nonnull p4 = nullable;
  void *_Nonnull nonnull;
  nonnull = nullable;
  nonnull = {nullable};
  TakeNonnull(nullable);
  TakeNonnull(nonnull);
  nonnull = (void *_Nonnull)nullable;

  SmartPtr _Nullable s_nullable;
  SmartPtr _Nonnull s(s_nullable);
  SmartPtr _Nonnull s2{s_nullable};
  SmartPtr _Nonnull s3 = {s_nullable};
  SmartPtr _Nonnull s4 = s_nullable;
  SmartPtr _Nonnull s_nonnull;
  s_nonnull = s_nullable;
  s_nonnull = {s_nullable};
  TakeSmartNonnull(s_nullable);
  TakeSmartNonnull(s_nonnull);
  s_nonnull = (SmartPtr _Nonnull)s_nullable;
  s_nonnull = static_cast<SmartPtr _Nonnull>(s_nullable);
}

void *_Nullable ReturnNullable();
SmartPtr _Nullable ReturnSmartNullable();

void AssignAndInitNonNullFromFn() {
  void *_Nonnull p(ReturnNullable());
  void *_Nonnull p2{ReturnNullable()};
  void *_Nonnull p3 = {ReturnNullable()};
  void *_Nonnull p4 = ReturnNullable();
  void *_Nonnull nonnull;
  nonnull = ReturnNullable();
  nonnull = {ReturnNullable()};
  TakeNonnull(ReturnNullable());

  SmartPtr _Nonnull s(ReturnSmartNullable());
  SmartPtr _Nonnull s2{ReturnSmartNullable()};
  SmartPtr _Nonnull s3 = {ReturnSmartNullable()};
  SmartPtr _Nonnull s4 = ReturnSmartNullable();
  SmartPtr _Nonnull s_nonnull;
  s_nonnull = ReturnSmartNullable();
  s_nonnull = {ReturnSmartNullable()};
  TakeSmartNonnull(ReturnSmartNullable());
}

void ConditionalExpr(bool c) {
  struct Base {};
  struct Derived : Base {};

  Base * _Nonnull p;
  Base * _Nonnull nonnullB;
  Base * _Nullable nullableB;
  Derived * _Nonnull nonnullD;
  Derived * _Nullable nullableD;

  p = c ? nonnullB : nonnullD;
  p = c ? nonnullB : nullableD;
  p = c ? nullableB : nonnullD;
  p = c ? nullableB : nullableD;
  p = c ? nonnullD : nonnullB;
  p = c ? nonnullD : nullableB;
  p = c ? nullableD : nonnullB;
  p = c ? nullableD : nullableB;
}

void arraysInLambdas() {
  typedef int INTS[4];
  auto simple = [](int [_Nonnull 2]) {};
  simple(nullptr);
  auto nested = [](void *_Nullable [_Nonnull 2]) {};
  nested(nullptr);
  auto nestedBad = [](int [2][_Nonnull 2]) {};

  auto withTypedef = [](INTS _Nonnull) {};
  withTypedef(nullptr);
  auto withTypedefBad = [](INTS _Nonnull[2]) {};
}

void testNullabilityCompletenessWithTemplate() {
  Template<int*> tip;
}

namespace GH60344 {
class a;
template <typename b> using c = b _Nullable;
c<a>;
}
