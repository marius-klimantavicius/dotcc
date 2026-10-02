//type: fp
//options:  --c++11 -DSYSTEM_WARNINGS: --c++11
# 1 "SemaCXX/warn-zero-nullptr.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-zero-nullptr.cpp" 2



# 1 "SemaCXX/Inputs/warn-zero-nullptr.h" 1
# 5 "SemaCXX/warn-zero-nullptr.cpp" 2




struct S {};

int (S::*mp0) = nullptr;
void (*fp0)() = nullptr;
void* p0 = nullptr;

int (S::*mp1) = 0;
void (*fp1)() = 0;
void* p1 = 0;


void* p2 = __null;
void (*fp2)() = __null;
int (S::*mp2) = __null;

void f0(void* v = (0));
void f1(void* v = (0));
void f2(void* v = (0));
void f3(void* v = ((0)));
void f4(void* v = 0);
void f5(void* v);

void g() {
  f1(0);
}


void* pp = (decltype(nullptr))0;
void* pp2 = static_cast<decltype(nullptr)>(0);


namespace pr34362 {
struct A { operator int*() { return nullptr; } };
void func() { if (nullptr == A()) {} }
void func2() { if ((nullptr) == A()) {} }
}

template <typename T> void TmplFunc0(T var) {}
void Func0Test() {
  TmplFunc0<int>(0);
  TmplFunc0<int*>(0);
  TmplFunc0<void*>(0);
}


template <typename T> void TmplFunc1(int a, T default_value = 0) {}
void FuncTest() {
  TmplFunc1<int>(0);
  TmplFunc1<int*>(0);
  TmplFunc1<void*>(0);
}

template<typename T>
class TemplateClass0 {
 public:
  explicit TemplateClass0(T var) {}
};
void TemplateClass0Test() {
  TemplateClass0<int> a(0);
  TemplateClass0<int*> b(0);
  TemplateClass0<void*> c(0);
}

template<typename T>
class TemplateClass1 {
 public:

  explicit TemplateClass1(int a, T default_value = 0) {}
};
void IgnoreSubstTemplateType1() {
  TemplateClass1<int> a(1);
  TemplateClass1<int*> b(1);
  TemplateClass1<void*> c(1);
}




void* sys_init = (0);
void* sys_init2 = ((0));
