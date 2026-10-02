//type: fp
//options:  --c++11 --exceptions -DUSE_CAPABILITY=0: --c++11 --exceptions -DUSE_CAPABILITY=1: --c++17 --exceptions -DUSE_CAPABILITY=0: --c++17 --exceptions -DUSE_CAPABILITY=1: --c++11
# 1 "SemaCXX/warn-thread-safety-analysis.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-thread-safety-analysis.cpp" 2








# 1 "SemaCXX/thread-safety-annotations.h" 1
# 10 "SemaCXX/warn-thread-safety-analysis.cpp" 2

class __attribute__((lockable)) Mutex {
 public:
  void Lock() __attribute__((exclusive_lock_function()));
  void ReaderLock() __attribute__((shared_lock_function()));
  void Unlock() __attribute__((unlock_function()));
  void ExclusiveUnlock() __attribute__((release_capability()));
  void ReaderUnlock() __attribute__((release_shared_capability()));
  bool TryLock() __attribute__((exclusive_trylock_function(true)));
  bool ReaderTryLock() __attribute__((shared_trylock_function(true)));
  void LockWhen(const int &cond) __attribute__((exclusive_lock_function()));

  void PromoteShared() __attribute__((release_shared_capability())) __attribute__((exclusive_lock_function()));
  void DemoteExclusive() __attribute__((release_capability())) __attribute__((shared_lock_function()));


  const Mutex& operator!() const { return *this; }

  void AssertHeld() __attribute__((assert_exclusive_lock()));
  void AssertReaderHeld() __attribute__((assert_shared_lock()));
};

class __attribute__((scoped_lockable)) MutexLock {
 public:
  MutexLock(Mutex *mu) __attribute__((exclusive_lock_function(mu)));
  MutexLock(Mutex *mu, bool adopt) __attribute__((exclusive_locks_required(mu)));
  ~MutexLock() __attribute__((unlock_function()));
};

class __attribute__((scoped_lockable)) ReaderMutexLock {
 public:
  ReaderMutexLock(Mutex *mu) __attribute__((shared_lock_function(mu)));
  ReaderMutexLock(Mutex *mu, bool adopt) __attribute__((shared_locks_required(mu)));
  ~ReaderMutexLock() __attribute__((unlock_function()));
};

class __attribute__((scoped_lockable)) ReleasableMutexLock {
 public:
  ReleasableMutexLock(Mutex *mu) __attribute__((exclusive_lock_function(mu)));
  ~ReleasableMutexLock() __attribute__((unlock_function()));

  void Release() __attribute__((unlock_function()));
};

class __attribute__((scoped_lockable)) DoubleMutexLock {
public:
  DoubleMutexLock(Mutex *mu1, Mutex *mu2) __attribute__((exclusive_lock_function(mu1, mu2)));
  ~DoubleMutexLock() __attribute__((unlock_function()));
};

template<typename Mu>
class __attribute__((scoped_lockable)) TemplateMutexLock {
public:
  TemplateMutexLock(Mu *mu) __attribute__((exclusive_lock_function(mu)));
  ~TemplateMutexLock() __attribute__((unlock_function()));
};

template<typename... Mus>
class __attribute__((scoped_lockable)) VariadicMutexLock {
public:
  VariadicMutexLock(Mus *...mus) __attribute__((exclusive_lock_function(mus...)));
  ~VariadicMutexLock() __attribute__((unlock_function()));
};



void beginNoWarnOnReads() __attribute__((shared_lock_function("*")));
void endNoWarnOnReads() __attribute__((unlock_function("*")));
void beginNoWarnOnWrites() __attribute__((exclusive_lock_function("*")));
void endNoWarnOnWrites() __attribute__((unlock_function("*")));



template<class T>
class SmartPtr {
public:
  SmartPtr(T* p) : ptr_(p) { }
  SmartPtr(const SmartPtr<T>& p) : ptr_(p.ptr_) { }
  ~SmartPtr();

  T* get() const { return ptr_; }
  T* operator->() const { return ptr_; }
  T& operator*() const { return *ptr_; }
  T& operator[](int i) const { return ptr_[i]; }

private:
  T* ptr_;
};

template<typename T, typename U>
U& operator->*(const SmartPtr<T>& ptr, U T::*p) { return ptr->*p; }



class MyString {
public:
  MyString(const char* s);
  ~MyString();
};



template <class K, class T>
class MyMap {
public:
  T& operator[](const K& k);
};



template <class T>
class MyContainer {
public:
  MyContainer();

  typedef T* iterator;
  typedef const T* const_iterator;

  T* begin();
  T* end();

  const T* cbegin();
  const T* cend();

  T& operator[](int i);
  const T& operator[](int i) const;

private:
  T* ptr_;
};



Mutex sls_mu;

Mutex sls_mu2 __attribute__((acquired_after(sls_mu)));
int sls_guard_var __attribute__((guarded_var)) = 0;
int sls_guardby_var __attribute__((guarded_by(sls_mu))) = 0;

bool getBool();

class MutexWrapper {
public:
   Mutex mu;
   int x __attribute__((guarded_by(mu)));
   void MyLock() __attribute__((exclusive_lock_function(mu)));
};

struct TestingMoreComplexAttributes {
   Mutex lock;
   struct { Mutex lock; } strct;
   union {
       bool a __attribute__((guarded_by(lock)));
       bool b __attribute__((guarded_by(strct.lock)));
       bool *ptr_a __attribute__((pt_guarded_by(lock)));
       bool *ptr_b __attribute__((pt_guarded_by(strct.lock)));
       Mutex lock1 __attribute__((acquired_before(lock))) __attribute__((acquired_before(strct.lock)));
       Mutex lock2 __attribute__((acquired_after(lock))) __attribute__((acquired_after(strct.lock)));
   };
} more_complex_atttributes;

void more_complex_attributes() {
    more_complex_atttributes.a = true;
    more_complex_atttributes.b = true;
    *more_complex_atttributes.ptr_a = true;
    *more_complex_atttributes.ptr_b = true;

    more_complex_atttributes.lock.Lock();
    more_complex_atttributes.lock1.Lock();
    more_complex_atttributes.lock1.Unlock();
    more_complex_atttributes.lock.Unlock();

    more_complex_atttributes.lock2.Lock();
    more_complex_atttributes.lock.Lock();
    more_complex_atttributes.lock.Unlock();
    more_complex_atttributes.lock2.Unlock();
}

MutexWrapper sls_mw;

void sls_fun_0() {
  sls_mw.mu.Lock();
  sls_mw.x = 5;
  sls_mw.mu.Unlock();
}

void sls_fun_2() {
  sls_mu.Lock();
  int x = sls_guard_var;
  sls_mu.Unlock();
}

void sls_fun_3() {
  sls_mu.Lock();
  sls_guard_var = 2;
  sls_mu.Unlock();
}

void sls_fun_4() {
  sls_mu2.Lock();
  sls_guard_var = 2;
  sls_mu2.Unlock();
}

void sls_fun_5() {
  sls_mu.Lock();
  int x = sls_guardby_var;
  sls_mu.Unlock();
}

void sls_fun_6() {
  sls_mu.Lock();
  sls_guardby_var = 2;
  sls_mu.Unlock();
}

void sls_fun_7() {
  sls_mu.Lock();
  sls_mu2.Lock();
  sls_mu2.Unlock();
  sls_mu.Unlock();
}

void sls_fun_8() {
  sls_mu.Lock();
  if (getBool())
    sls_mu.Unlock();
  else
    sls_mu.Unlock();
}

void sls_fun_9() {
  if (getBool())
    sls_mu.Lock();
  else
    sls_mu.Lock();
  sls_mu.Unlock();
}

void sls_fun_good_6() {
  if (getBool()) {
    sls_mu.Lock();
  } else {
    if (getBool()) {
      getBool();
    } else {
      getBool();
    }
    sls_mu.Lock();
  }
  sls_mu.Unlock();
}

void sls_fun_good_7() {
  sls_mu.Lock();
  while (getBool()) {
    sls_mu.Unlock();
    if (getBool()) {
      if (getBool()) {
        sls_mu.Lock();
        continue;
      }
    }
    sls_mu.Lock();
  }
  sls_mu.Unlock();
}

void sls_fun_good_8() {
  sls_mw.MyLock();
  sls_mw.mu.Unlock();
}

void sls_fun_bad_1() {
  sls_mu.Unlock();

}

void sls_fun_bad_2() {
  sls_mu.Lock();
  sls_mu.Lock();

  sls_mu.Unlock();
}

void sls_fun_bad_3() {
  sls_mu.Lock();
}

void sls_fun_bad_4() {
  if (getBool())
    sls_mu.Lock();
  else
    sls_mu2.Lock();
}


void sls_fun_bad_5() {
  sls_mu.Lock();
  if (getBool())
    sls_mu.Unlock();
}

void sls_fun_bad_6() {
  if (getBool()) {
    sls_mu.Lock();
  } else {
    if (getBool()) {
      getBool();
    } else {
      getBool();
    }
  }
  sls_mu.Unlock();


}

void sls_fun_bad_7() {
  sls_mu.Lock();
  while (getBool()) {

    sls_mu.Unlock();
    if (getBool()) {
      if (getBool()) {
        continue;
      }
    }
    sls_mu.Lock();
  }
  sls_mu.Unlock();
}

void sls_fun_bad_8() {
  sls_mu.Lock();

  do {
    sls_mu.Unlock();
  } while (getBool());
}

void sls_fun_bad_9() {
  do {
    sls_mu.Lock();


  } while (getBool());
  sls_mu.Unlock();
}

void sls_fun_bad_10() {
  sls_mu.Lock();
  while(getBool()) {
    sls_mu.Unlock();
  }
}

void sls_fun_bad_11() {
  while (getBool()) {

    sls_mu.Lock();
  }
  sls_mu.Unlock();

}

void sls_fun_bad_12() {
  sls_mu.Lock();
  while (getBool()) {
    sls_mu.Unlock();
    if (getBool()) {
      if (getBool()) {
        break;
      }
    }
    sls_mu.Lock();
  }
  sls_mu.Unlock();


}





Mutex aa_mu;

class GlobalLocker {
public:
  void globalLock() __attribute__((exclusive_lock_function(aa_mu)));
  void globalUnlock() __attribute__((unlock_function(aa_mu)));
};

GlobalLocker glock;

void aa_fun_1() {
  glock.globalLock();
  glock.globalUnlock();
}

void aa_fun_bad_1() {
  glock.globalUnlock();

}

void aa_fun_bad_2() {
  glock.globalLock();
  glock.globalLock();

  glock.globalUnlock();
}

void aa_fun_bad_3() {
  glock.globalLock();
}





Mutex wmu;


class WeirdMethods {

  WeirdMethods() {
    wmu.Lock();
  }
  ~WeirdMethods() {
    wmu.Lock();
  }
  void operator++() {
    wmu.Lock();
  }
  operator int*() {
    wmu.Lock();
    return 0;
  }
};





int *pgb_gvar __attribute__((pt_guarded_var));
int *pgb_var __attribute__((pt_guarded_by(sls_mu)));

class PGBFoo {
 public:
  int x;
  int *pgb_field __attribute__((guarded_by(sls_mu2)))
                 __attribute__((pt_guarded_by(sls_mu)));
  void testFoo() {
    pgb_field = &x;

    *pgb_field = x;

    x = *pgb_field;

    (*pgb_field)++;

  }
};

class GBFoo {
 public:
  int gb_field __attribute__((guarded_by(sls_mu)));

  void testFoo() {
    gb_field = 0;

  }

  void testNoAnal() __attribute__((no_thread_safety_analysis)) {
    gb_field = 0;
  }
};

GBFoo GlobalGBFoo __attribute__((guarded_by(sls_mu)));

void gb_fun_0() {
  sls_mu.Lock();
  int x = *pgb_var;
  sls_mu.Unlock();
}

void gb_fun_1() {
  sls_mu.Lock();
  *pgb_var = 2;
  sls_mu.Unlock();
}

void gb_fun_2() {
  int x;
  pgb_var = &x;
}

void gb_fun_3() {
  int *x = pgb_var;
}

void gb_bad_0() {
  sls_guard_var = 1;

}

void gb_bad_1() {
  int x = sls_guard_var;

}

void gb_bad_2() {
  sls_guardby_var = 1;

}

void gb_bad_3() {
  int x = sls_guardby_var;

}

void gb_bad_4() {
  *pgb_gvar = 1;

}

void gb_bad_5() {
  int x = *pgb_gvar;

}

void gb_bad_6() {
  *pgb_var = 1;

}

void gb_bad_7() {
  int x = *pgb_var;

}

void gb_bad_8() {
  GBFoo G;
  G.gb_field = 0;

}

void gb_bad_9() {
  sls_guard_var++;

  sls_guard_var--;

  ++sls_guard_var;

  --sls_guard_var;

}





class LateFoo {
public:
  int a __attribute__((guarded_by(mu)));
  int b;

  void foo() __attribute__((exclusive_locks_required(mu))) { }

  void test() {
    a = 0;

    b = a;

    c = 0;

  }

  int c __attribute__((guarded_by(mu)));

  Mutex mu;
};

class LateBar {
 public:
  int a_ __attribute__((guarded_by(mu1_)));
  int b_;
  int *q __attribute__((pt_guarded_by(mu)));
  Mutex mu1_;
  Mutex mu;
  LateFoo Foo;
  LateFoo Foo2;
  LateFoo *FooPointer;
};

LateBar b1, *b3;

void late_0() {
  LateFoo FooA;
  LateFoo FooB;
  FooA.mu.Lock();
  FooA.a = 5;
  FooA.mu.Unlock();
}

void late_1() {
  LateBar BarA;
  BarA.FooPointer->mu.Lock();
  BarA.FooPointer->a = 2;
  BarA.FooPointer->mu.Unlock();
}

void late_bad_0() {
  LateFoo fooA;
  LateFoo fooB;
  fooA.mu.Lock();
  fooB.a = 5;


  fooA.mu.Unlock();
}

void late_bad_1() {
  Mutex mu;
  mu.Lock();
  b1.mu1_.Lock();
  int res = b1.a_ + b3->b_;
  b3->b_ = *b1.q;

  b1.mu1_.Unlock();
  b1.b_ = res;
  mu.Unlock();
}

void late_bad_2() {
  LateBar BarA;
  BarA.FooPointer->mu.Lock();
  BarA.Foo.a = 2;


  BarA.FooPointer->mu.Unlock();
}

void late_bad_3() {
  LateBar BarA;
  BarA.Foo.mu.Lock();
  BarA.FooPointer->a = 2;


  BarA.Foo.mu.Unlock();
}

void late_bad_4() {
  LateBar BarA;
  BarA.Foo.mu.Lock();
  BarA.Foo2.a = 2;


  BarA.Foo.mu.Unlock();
}





void shared_fun_0() {
  sls_mu.Lock();
  do {
    sls_mu.Unlock();
    sls_mu.Lock();
  } while (getBool());
  sls_mu.Unlock();
}

void shared_fun_1() {
  sls_mu.ReaderLock();

  do {
    sls_mu.Unlock();
    sls_mu.Lock();

  } while (getBool());
  sls_mu.Unlock();
}

void shared_fun_3() {
  if (getBool())
    sls_mu.Lock();
  else
    sls_mu.Lock();
  *pgb_var = 1;
  sls_mu.Unlock();
}

void shared_fun_4() {
  if (getBool())
    sls_mu.ReaderLock();
  else
    sls_mu.ReaderLock();
  int x = sls_guardby_var;
  sls_mu.Unlock();
}

void shared_fun_8() {
  if (getBool())
    sls_mu.Lock();

  else
    sls_mu.ReaderLock();

  sls_mu.Unlock();
}

void shared_fun_9() {
  sls_mu.Lock();
  sls_mu.ExclusiveUnlock();

  sls_mu.ReaderLock();
  sls_mu.ReaderUnlock();
}

void shared_fun_10() {
  sls_mu.Lock();
  sls_mu.DemoteExclusive();
  sls_mu.ReaderUnlock();
}

void shared_fun_11() {
  sls_mu.ReaderLock();
  sls_mu.PromoteShared();
  sls_mu.Unlock();
}

void shared_bad_0() {
  sls_mu.Lock();

  do {
    sls_mu.Unlock();
    sls_mu.ReaderLock();

  } while (getBool());
  sls_mu.Unlock();
}

void shared_bad_1() {
  if (getBool())
    sls_mu.Lock();

  else
    sls_mu.ReaderLock();

  *pgb_var = 1;
  sls_mu.Unlock();
}

void shared_bad_2() {
  if (getBool())
    sls_mu.ReaderLock();

  else
    sls_mu.Lock();

  *pgb_var = 1;
  sls_mu.Unlock();
}

void shared_bad_3() {
  sls_mu.Lock();
  sls_mu.ReaderUnlock();

}

void shared_bad_4() {
  sls_mu.ReaderLock();
  sls_mu.ExclusiveUnlock();

}

void shared_bad_5() {
  sls_mu.Lock();
  sls_mu.PromoteShared();

  sls_mu.ExclusiveUnlock();
}

void shared_bad_6() {
  sls_mu.ReaderLock();
  sls_mu.DemoteExclusive();

  sls_mu.ReaderUnlock();
}


class LRBar {
 public:
  void aa_elr_fun() __attribute__((exclusive_locks_required(aa_mu)));
  void aa_elr_fun_s() __attribute__((shared_locks_required(aa_mu)));
  void le_fun() __attribute__((locks_excluded(sls_mu)));
};

class LRFoo {
 public:
  void test() __attribute__((exclusive_locks_required(sls_mu)));
  void testShared() __attribute__((shared_locks_required(sls_mu2)));
};

void elr_fun() __attribute__((exclusive_locks_required(sls_mu)));
void elr_fun() {}

LRFoo MyLRFoo;
LRBar Bar;

void es_fun_0() {
  aa_mu.Lock();
  Bar.aa_elr_fun();
  aa_mu.Unlock();
}

void es_fun_1() {
  aa_mu.Lock();
  Bar.aa_elr_fun_s();
  aa_mu.Unlock();
}

void es_fun_2() {
  aa_mu.ReaderLock();
  Bar.aa_elr_fun_s();
  aa_mu.Unlock();
}

void es_fun_3() {
  sls_mu.Lock();
  MyLRFoo.test();
  sls_mu.Unlock();
}

void es_fun_4() {
  sls_mu2.Lock();
  MyLRFoo.testShared();
  sls_mu2.Unlock();
}

void es_fun_5() {
  sls_mu2.ReaderLock();
  MyLRFoo.testShared();
  sls_mu2.Unlock();
}

void es_fun_6() {
  Bar.le_fun();
}

void es_fun_7() {
  sls_mu.Lock();
  elr_fun();
  sls_mu.Unlock();
}

void es_fun_8() __attribute__((no_thread_safety_analysis));

void es_fun_8() {
  Bar.aa_elr_fun_s();
}

void es_fun_9() __attribute__((shared_locks_required(aa_mu)));
void es_fun_9() {
  Bar.aa_elr_fun_s();
}

void es_fun_10() __attribute__((exclusive_locks_required(aa_mu)));
void es_fun_10() {
  Bar.aa_elr_fun_s();
}

void es_bad_0() {
  Bar.aa_elr_fun();

}

void es_bad_1() {
  aa_mu.ReaderLock();
  Bar.aa_elr_fun();

  aa_mu.Unlock();
}

void es_bad_2() {
  Bar.aa_elr_fun_s();

}

void es_bad_3() {
  MyLRFoo.test();

}

void es_bad_4() {
  MyLRFoo.testShared();

}

void es_bad_5() {
  sls_mu.ReaderLock();
  MyLRFoo.test();

  sls_mu.Unlock();
}

void es_bad_6() {
  sls_mu.Lock();
  Bar.le_fun();

  sls_mu.Unlock();
}

void es_bad_7() {
  sls_mu.ReaderLock();
  Bar.le_fun();

  sls_mu.Unlock();
}
# 952 "SemaCXX/warn-thread-safety-analysis.cpp"
namespace thread_annot_lock_20 {
class Bar {
 public:
  static int func1() __attribute__((exclusive_locks_required(mu1_)));
  static int b_ __attribute__((guarded_by(mu1_)));
  static Mutex mu1_;
  static int a_ __attribute__((guarded_by(mu1_)));
};

Bar b1;

int Bar::func1()
{
  int res = 5;

  if (a_ == 4)
    res = b_;
  return res;
}
}

namespace thread_annot_lock_22 {


Mutex mu;

class Bar {
 public:
  int a_ __attribute__((guarded_by(mu1_)));
  int b_;
  int *q __attribute__((pt_guarded_by(mu)));
  Mutex mu1_ __attribute__((acquired_after(mu)));
};

Bar b1, *b3;
int *p __attribute__((guarded_by(mu))) __attribute__((pt_guarded_by(mu)));
int res __attribute__((guarded_by(mu))) = 5;

int func(int i)
{
  int x;
  mu.Lock();
  b1.mu1_.Lock();
  res = b1.a_ + b3->b_;
  *p = i;
  b1.a_ = res + b3->b_;
  b3->b_ = *b1.q;
  b1.mu1_.Unlock();
  b1.b_ = res;
  x = res;
  mu.Unlock();
  return x;
}
}

namespace thread_annot_lock_27_modified {


Mutex mu1;
Mutex mu2 __attribute__((acquired_after(mu1)));

class Foo {
 public:
  int method1(int i) __attribute__((shared_locks_required(mu2))) __attribute__((exclusive_locks_required(mu1)));
};

int Foo::method1(int i) {
  return i;
}


int foo(int i) __attribute__((exclusive_locks_required(mu2))) __attribute__((shared_locks_required(mu1)));
int foo(int i) {
  return i;
}

static int bar(int i) __attribute__((exclusive_locks_required(mu1)));
static int bar(int i) {
  return i;
}

void main() {
  Foo a;

  mu1.Lock();
  mu2.Lock();
  a.method1(1);
  foo(2);
  mu2.Unlock();
  bar(3);
  mu1.Unlock();
}
}


namespace thread_annot_lock_38 {


class Foo {
 public:
  void func1(int y) __attribute__((locks_excluded(mu_)));
  template <typename T> void func2(T x) __attribute__((locks_excluded(mu_)));
 private:
  Mutex mu_;
};

Foo *foo;

void main()
{
  foo->func1(5);
  foo->func2(5);
}
}

namespace thread_annot_lock_43 {

class Foo {
 public:
  Mutex *mu_;
};

class FooBar {
 public:
  Foo *foo_;
  int GetA() __attribute__((exclusive_locks_required(foo_->mu_))) { return a_; }
  int a_ __attribute__((guarded_by(foo_->mu_)));
};

FooBar *fb;

void main()
{
  int x;
  fb->foo_->mu_->Lock();
  x = fb->GetA();
  fb->foo_->mu_->Unlock();
}
}

namespace thread_annot_lock_49 {

class Foo {
 public:
  Mutex foo_mu_;
};

class Bar {
 private:
  Foo *foo;
  Mutex bar_mu_ __attribute__((acquired_after(foo->foo_mu_)));

 public:
  void Test1() {
    foo->foo_mu_.Lock();
    bar_mu_.Lock();
    bar_mu_.Unlock();
    foo->foo_mu_.Unlock();
  }
};

void main() {
  Bar bar;
  bar.Test1();
}
}

namespace thread_annot_lock_61_modified {



  struct Foo { Foo &operator<< (bool) {return *this;} };
  Foo &getFoo();
  struct Bar { Foo &func () {return getFoo();} };
  struct Bas { void operator& (Foo &) {} };
  void mumble()
  {
    Bas() & Bar().func() << "" << "";
    Bas() & Bar().func() << "";
  }
}


namespace thread_annot_lock_65 {



enum MyFlags {
  Zero,
  One,
  Two,
  Three,
  Four,
  Five,
  Six,
  Seven,
  Eight,
  Nine
};

inline MyFlags
operator|(MyFlags a, MyFlags b)
{
  return MyFlags(static_cast<int>(a) | static_cast<int>(b));
}

inline MyFlags&
operator|=(MyFlags& a, MyFlags b)
{
    return a = a | b;
}
}

namespace thread_annot_lock_66_modified {



Mutex mu;

class Foo {
 public:
  int method1(int i) __attribute__((shared_locks_required(mu1, mu, mu2)));
  int data __attribute__((guarded_by(mu1)));
  Mutex *mu1;
  Mutex *mu2;
};

int Foo::method1(int i)
{
  return data + i;
}

void main()
{
  Foo a;

  a.mu2->Lock();
  a.mu1->Lock();
  mu.Lock();
  a.method1(1);
  mu.Unlock();
  a.mu1->Unlock();
  a.mu2->Unlock();
}
}

namespace thread_annot_lock_68_modified {



template <typename T>
class Bar {
  Mutex mu_;
};

template <typename T>
class Foo {
 public:
  void func(T x) {
    mu_.Lock();
    count_ = x;
    mu_.Unlock();
  }

 private:
  T count_ __attribute__((guarded_by(mu_)));
  Bar<T> bar_;
  Mutex mu_;
};

void main()
{
  Foo<int> *foo;
  foo->func(5);
}
}

namespace thread_annot_lock_30_modified {


int a = 0;

class Bar {
  struct Foo;

 public:
  void MyLock() __attribute__((exclusive_lock_function(mu)));

  int func() {
    MyLock();



    a = 5;
    mu.Unlock();
    return 1;
  }

  class FooBar {
    int x;
    int y;
  };

 private:
  Mutex mu;
};

Bar *bar;

void main()
{
  bar->func();
}
}

namespace thread_annot_lock_47 {



class Base {
 public:
  virtual void func1() __attribute__((exclusive_locks_required(mu_)));
  virtual void func2() __attribute__((locks_excluded(mu_)));
  Mutex mu_;
};

class Child : public Base {
 public:
  virtual void func1() __attribute__((exclusive_locks_required(mu_)));
  virtual void func2() __attribute__((locks_excluded(mu_)));
};

void main() {
  Child *c;
  Base *b = c;

  b->mu_.Lock();
  b->func1();
  b->mu_.Unlock();
  b->func2();

  c->mu_.Lock();
  c->func1();
  c->mu_.Unlock();
  c->func2();
}
}





namespace thread_annot_lock_13 {
Mutex mu1;
Mutex mu2;

int g __attribute__((guarded_by(mu1)));
int w __attribute__((guarded_by(mu2)));

class Foo {
 public:
  void bar() __attribute__((locks_excluded(mu_, mu1)));
  int foo() __attribute__((shared_locks_required(mu_))) __attribute__((exclusive_locks_required(mu2)));

 private:
  int a_ __attribute__((guarded_by(mu_)));
 public:
  Mutex mu_ __attribute__((acquired_after(mu1)));
};

int Foo::foo()
{
  int res;
  w = 5;
  res = a_ + 5;
  return res;
}

void Foo::bar()
{
  int x;
  mu_.Lock();
  x = foo();
  a_ = x + 1;
  mu_.Unlock();
  if (x > 5) {
    mu1.Lock();
    g = 2;
    mu1.Unlock();
  }
}

void main()
{
  Foo f1, *f2;
  f1.mu_.Lock();
  f1.bar();
  mu2.Lock();
  f1.foo();
  mu2.Unlock();
  f1.mu_.Unlock();
  f2->mu_.Lock();
  f2->bar();
  f2->mu_.Unlock();
  mu2.Lock();
  w = 2;
  mu2.Unlock();
}
}

namespace thread_annot_lock_18_modified {



  class Bar {
 public:
  bool MyLock() __attribute__((exclusive_lock_function(mu1_)));
  void MyUnlock() __attribute__((unlock_function(mu1_)));
  int a_ __attribute__((guarded_by(mu1_)));

 private:
  Mutex mu1_;
};

Bar *b1, *b2;

void func()
{
  b1->MyLock();
  b1->a_ = 5;
  b2->a_ = 3;


  b2->MyLock();
  b2->MyUnlock();
  b1->MyUnlock();
}
}

namespace thread_annot_lock_21 {


Mutex mu;

class Bar {
 public:
  int a_ __attribute__((guarded_by(mu1_)));
  int b_;
  int *q __attribute__((pt_guarded_by(mu)));
  Mutex mu1_ __attribute__((acquired_after(mu)));
};

Bar b1, *b3;
int *p __attribute__((guarded_by(mu))) __attribute__((pt_guarded_by(mu)));

int res __attribute__((guarded_by(mu))) = 5;

int func(int i)
{
  int x;
  b3->mu1_.Lock();
  res = b1.a_ + b3->b_;


  *p = i;

  b1.a_ = res + b3->b_;


  b3->b_ = *b1.q;
  b3->mu1_.Unlock();
  b1.b_ = res;
  x = res;
  return x;
}
}

namespace thread_annot_lock_35_modified {


class Foo {
 private:
  Mutex lock_;
  int a_ __attribute__((guarded_by(lock_)));

 public:
  void Func(Foo* child) __attribute__((locks_excluded(lock_))) {
     Foo *new_foo = new Foo;

     lock_.Lock();

     child->Func(new_foo);

     child->bar(7);


     child->a_ = 5;


     lock_.Unlock();
  }

  void bar(int y) __attribute__((exclusive_locks_required(lock_))) {
    a_ = y;
  }
};

Foo *x;

void main() {
  Foo *child = new Foo;
  x->Func(child);
}
}

namespace thread_annot_lock_36_modified {



class Foo {
 private:
  Mutex lock_;
  int a_ __attribute__((guarded_by(lock_)));

 public:
  void Func(Foo* child) __attribute__((locks_excluded(lock_)));
  void bar(int y) __attribute__((exclusive_locks_required(lock_)));
};

void Foo::Func(Foo* child) {
  Foo *new_foo = new Foo;

  lock_.Lock();

  child->lock_.Lock();
  child->Func(new_foo);
  child->bar(7);
  child->a_ = 5;
  child->lock_.Unlock();

  lock_.Unlock();
}

void Foo::bar(int y) {
  a_ = y;
}


Foo *x;

void main() {
  Foo *child = new Foo;
  x->Func(child);
}
}


namespace thread_annot_lock_42 {

class Foo {
 private:
  Mutex mu1, mu2, mu3;
  int x __attribute__((guarded_by(mu1))) __attribute__((guarded_by(mu2)));
  int y __attribute__((guarded_by(mu2)));

  void f2() __attribute__((locks_excluded(mu1))) __attribute__((locks_excluded(mu2))) __attribute__((locks_excluded(mu3))) {
    mu2.Lock();
    y = 2;
    mu2.Unlock();
  }

 public:
  void f1() __attribute__((exclusive_locks_required(mu2))) __attribute__((exclusive_locks_required(mu1))) {
    x = 5;
    f2();

  }
};

Foo *foo;

void func()
{
  foo->f1();

}
}

namespace thread_annot_lock_46 {

class Base {
 public:
  virtual void func1() __attribute__((exclusive_locks_required(mu_)));
  virtual void func2() __attribute__((locks_excluded(mu_)));
  Mutex mu_;
};

class Child : public Base {
 public:
  virtual void func1() __attribute__((exclusive_locks_required(mu_)));
  virtual void func2() __attribute__((locks_excluded(mu_)));
};

void main() {
  Child *c;
  Base *b = c;

  b->func1();
  b->mu_.Lock();
  b->func2();
  b->mu_.Unlock();

  c->func1();
  c->mu_.Lock();
  c->func2();
  c->mu_.Unlock();
}
}

namespace thread_annot_lock_67_modified {



Mutex mu;
Mutex mu3;

class Foo {
 public:
  int method1(int i) __attribute__((shared_locks_required(mu1, mu, mu2, mu3)));
  int data __attribute__((guarded_by(mu1)));
  Mutex *mu1;
  Mutex *mu2;
};

int Foo::method1(int i) {
  return data + i;
}

void main()
{
  Foo a;
  a.method1(1);



}
}


namespace substitution_test {
  class MyData {
  public:
    Mutex mu;

    void lockData() __attribute__((exclusive_lock_function(mu)));
    void unlockData() __attribute__((unlock_function(mu)));

    void doSomething() __attribute__((exclusive_locks_required(mu))) { }
    void operator()() __attribute__((exclusive_locks_required(mu))) { }

    MyData operator+(const MyData& other) const __attribute__((shared_locks_required(mu, other.mu)));
  };

  MyData operator-(const MyData& a, const MyData& b) __attribute__((shared_locks_required(a.mu, b.mu)));

  class DataLocker {
  public:
    void lockData (MyData *d) __attribute__((exclusive_lock_function(d->mu)));
    void unlockData(MyData *d) __attribute__((unlock_function(d->mu)));
  };


  class Foo {
  public:
    void foo(MyData* d) __attribute__((exclusive_locks_required(d->mu))) { }

    void subst(MyData& d) {
      d.doSomething();
      d();
      d.operator()();

      d.lockData();
      d.doSomething();
      d();
      d.operator()();
      d.unlockData();
    }

    void binop(MyData& a, MyData& b) __attribute__((exclusive_locks_required(a.mu))) {
      a + b;

      b + a;

      a - b;

    }

    void bar1(MyData* d) {
      d->lockData();
      foo(d);
      d->unlockData();
    }

    void bar2(MyData* d) {
      DataLocker dlr;
      dlr.lockData(d);
      foo(d);
      dlr.unlockData(d);
    }

    void bar3(MyData* d1, MyData* d2) {
      DataLocker dlr;
      dlr.lockData(d1);
      dlr.unlockData(d2);

    }

    void bar4(MyData* d1, MyData* d2) {
      DataLocker dlr;
      dlr.lockData(d1);
      foo(d2);


      dlr.unlockData(d1);
    }
  };



  struct DestructorRequires {
    Mutex mu;
    ~DestructorRequires() __attribute__((exclusive_locks_required(mu)));
  };

  void destructorRequires() {
    DestructorRequires rd;
    rd.mu.AssertHeld();
  }

  struct DestructorExcludes {
    Mutex mu;
    ~DestructorExcludes() __attribute__((locks_excluded(mu)));
  };

  void destructorExcludes() {
    DestructorExcludes ed;
    ed.mu.Lock();
  }


}



namespace constructor_destructor_tests {
  Mutex fooMu;
  int myVar __attribute__((guarded_by(fooMu)));

  class Foo {
  public:
    Foo() __attribute__((exclusive_lock_function(fooMu))) { }
    ~Foo() __attribute__((unlock_function(fooMu))) { }
  };

  void fooTest() {
    Foo foo;
    myVar = 0;
  }
}


namespace template_member_test {

  struct S { int n; };
  struct T {
    Mutex m;
    S *s __attribute__((guarded_by(this->m)));
  };
  Mutex m;
  struct U {
    union {
      int n;
    };
  } *u __attribute__((guarded_by(m)));

  template<typename U>
  struct IndirectLock {
    int DoNaughtyThings(T *t) {
      u->n = 0;
      return t->s->n;
    }
  };

  template struct IndirectLock<int>;

  struct V {
    void f(int);
    void f(double);

    Mutex m;
    V *p __attribute__((guarded_by(this->m)));
  };
  template<typename U> struct W {
    V v;
    void f(U u) {
      v.p->f(u);
    }
  };
  template struct W<int>;

}

namespace test_scoped_lockable {

struct TestScopedLockable {
  Mutex mu1;
  Mutex mu2;
  int a __attribute__((guarded_by(mu1)));
  int b __attribute__((guarded_by(mu2)));

  bool getBool();

  bool lock2Bool(MutexLock);

  void foo1() {
    MutexLock mulock(&mu1);
    a = 5;
  }
# 1788 "SemaCXX/warn-thread-safety-analysis.cpp"
  void temporary() {
    MutexLock{&mu1}, a = 5;
  }

  void temporary_cfg(int x) {

    lock2Bool(MutexLock{&mu1}) || x;
    MutexLock{&mu1};
  }

  void lifetime_extension() {
    const MutexLock &mulock = MutexLock(&mu1);
    a = 5;
  }

  void foo2() {
    ReaderMutexLock mulock1(&mu1);
    if (getBool()) {
      MutexLock mulock2a(&mu2);
      b = a + 1;
    }
    else {
      MutexLock mulock2b(&mu2);
      b = a + 2;
    }
  }

  void foo3() {
    MutexLock mulock_a(&mu1);
    MutexLock mulock_b(&mu1);

  }

  void temporary_double_lock() {
    MutexLock mulock_a(&mu1);
    MutexLock{&mu1};

  }

  void foo4() {
    MutexLock mulock1(&mu1), mulock2(&mu2);
    a = b+1;
    b = a+1;
  }

  void foo5() {
    DoubleMutexLock mulock(&mu1, &mu2);
    a = b + 1;
    b = a + 1;
  }

  void foo6() {
    TemplateMutexLock<Mutex> mulock1(&mu1), mulock2(&mu2);
    a = b + 1;
    b = a + 1;
  }

  void foo7() {
    VariadicMutexLock<Mutex, Mutex> mulock(&mu1, &mu2);
    a = b + 1;
    b = a + 1;
  }
};

namespace test_function_param_lock_unlock {
class A {
 public:
  A() __attribute__((exclusive_lock_function(mu_))) { mu_.Lock(); }
  ~A() __attribute__((unlock_function(mu_))) { mu_.Unlock(); }
 private:
  Mutex mu_;
};
int do_something(A a) { return 0; }



class B {
 public:
  B() {}
  B(int) {}
  ~B() __attribute__((unlock_function(mu_))) { mu_.Unlock(); }
 private:
  Mutex mu_;
};
int do_something(B b) { return 0; }

class __attribute__((scoped_lockable)) MutexWrapper {
public:
  MutexWrapper(Mutex *mu) : mu_(mu) {}
  ~MutexWrapper() __attribute__((unlock_function(mu_))) { mu_->Unlock(); }
  void Lock() __attribute__((exclusive_lock_function(mu_))) { mu_->Lock(); }

  Mutex *mu_;
};

void do_something(MutexWrapper mw) {
  mw.Lock();
}

}

}


namespace FunctionAttrTest {

class Foo {
public:
  Mutex mu_;
  int a __attribute__((guarded_by(mu_)));
};

Foo fooObj;

void foo() __attribute__((exclusive_locks_required(fooObj.mu_)));

void bar() {
  foo();
  fooObj.mu_.Lock();
  foo();
  fooObj.mu_.Unlock();
}

};


namespace TryLockTest {

struct TestTryLock {
  Mutex mu;
  int a __attribute__((guarded_by(mu)));
  bool cond;

  void foo1() {
    if (mu.TryLock()) {
      a = 1;
      mu.Unlock();
    }
  }

  void foo2() {
    if (!mu.TryLock()) return;
    a = 2;
    mu.Unlock();
  }

  void foo2_builtin_expect() {
    if (__builtin_expect(!mu.TryLock(), false))
      return;
    a = 2;
    mu.Unlock();
  }

  void foo3() {
    bool b = mu.TryLock();
    if (b) {
      a = 3;
      mu.Unlock();
    }
  }

  void foo3_builtin_expect() {
    bool b = mu.TryLock();
    if (__builtin_expect(b, true)) {
      a = 3;
      mu.Unlock();
    }
  }

  void foo4() {
    bool b = mu.TryLock();
    if (!b) return;
    a = 4;
    mu.Unlock();
  }

  void foo5() {
    while (mu.TryLock()) {
      a = a + 1;
      mu.Unlock();
    }
  }

  void foo6() {
    bool b = mu.TryLock();
    b = !b;
    if (b) return;
    a = 6;
    mu.Unlock();
  }

  void foo7() {
    bool b1 = mu.TryLock();
    bool b2 = !b1;
    bool b3 = !b2;
    if (b3) {
      a = 7;
      mu.Unlock();
    }
  }


  void foo8() {
    bool b = mu.TryLock();
    bool b2 = b;
    if (cond)
      b = true;
    if (b) {
      a = 8;
    }
    if (b2) {
      a = 8;
      mu.Unlock();
    }
  }


  void foo9() {
    bool b = mu.TryLock();

    for (int i = 0; i < 10; ++i);

    if (b) {
      a = 9;
      mu.Unlock();
    }
  }


  void foo10() {
    bool b = mu.TryLock();

    while (cond) {
      if (b) {
        a = 10;
      }
      b = !b;
    }
  }


  void foo11() {
   if (cond) {
     if (!mu.TryLock())
       return;
   }
   else {
     mu.Lock();
   }
   a = 10;
   mu.Unlock();
  }


  void foo12() {
   if (cond) {
     if (!mu.ReaderTryLock())
       return;
   }
   else {
     mu.ReaderLock();
   }
   int i = a;
   mu.Unlock();
  }


  void foo13() {
    if (mu.TryLock() ? 1 : 0)
      mu.Unlock();
  }

  void foo14() {
    if (mu.TryLock() ? 0 : 1)
      return;
    mu.Unlock();
  }

  void foo15() {
    if (mu.TryLock() ? 0 : 1)
      mu.Unlock();
  }
};

}


namespace TestTemplateAttributeInstantiation {

class Foo1 {
public:
  Mutex mu_;
  int a __attribute__((guarded_by(mu_)));
};

class Foo2 {
public:
  int a __attribute__((guarded_by(mu_)));
  Mutex mu_;
};


class Bar {
public:

  template <class T>
  void barND(Foo1 *foo, T *fooT) __attribute__((exclusive_locks_required(foo->mu_))) {
    foo->a = 0;
  }


  template <class T>
  void barD(Foo1 *foo, T *fooT) __attribute__((exclusive_locks_required(fooT->mu_))) {
    fooT->a = 0;
  }
};


template <class T>
class BarT {
public:
  Foo1 fooBase;
  T fooBaseT;


  void barND() __attribute__((exclusive_locks_required(fooBase.mu_))) {
    fooBase.a = 0;
  }


  void barD() __attribute__((exclusive_locks_required(fooBaseT.mu_))) {
    fooBaseT.a = 0;
  }


  template <class T2>
  void barTD(T2 *fooT) __attribute__((exclusive_locks_required(fooBaseT.mu_, fooT->mu_))) {
    fooBaseT.a = 0;
    fooT->a = 0;
  }
};

template <class T>
class Cell {
public:
  Mutex mu_;

  T data __attribute__((guarded_by(mu_)));

  void fooEx() __attribute__((exclusive_locks_required(mu_))) {
    data = 0;
  }

  void foo() {
    mu_.Lock();
    data = 0;
    mu_.Unlock();
  }
};

void test() {
  Bar b;
  BarT<Foo2> bt;
  Foo1 f1;
  Foo2 f2;

  f1.mu_.Lock();
  f2.mu_.Lock();
  bt.fooBase.mu_.Lock();
  bt.fooBaseT.mu_.Lock();

  b.barND(&f1, &f2);
  b.barD(&f1, &f2);
  bt.barND();
  bt.barD();
  bt.barTD(&f2);

  f1.mu_.Unlock();
  bt.barTD(&f1);



  bt.fooBase.mu_.Unlock();
  bt.fooBaseT.mu_.Unlock();
  f2.mu_.Unlock();

  Cell<int> cell;
  cell.data = 0;

  cell.foo();
  cell.mu_.Lock();
  cell.fooEx();
  cell.mu_.Unlock();
}


template <class T>
class CellDelayed {
public:

  T data __attribute__((guarded_by(mu_)));
  static T static_data __attribute__((guarded_by(static_mu_)));

  void fooEx(CellDelayed<T> *other) __attribute__((exclusive_locks_required(mu_, other->mu_))) {
    this->data = other->data;
  }

  template <class T2>
  void fooExT(CellDelayed<T2> *otherT) __attribute__((exclusive_locks_required(mu_, otherT->mu_))) {
    this->data = otherT->data;
  }

  void foo() {
    mu_.Lock();
    data = 0;
    mu_.Unlock();
  }

  Mutex mu_;
  static Mutex static_mu_;
};

void testDelayed() {
  CellDelayed<int> celld;
  CellDelayed<int> celld2;
  celld.foo();
  celld.mu_.Lock();
  celld2.mu_.Lock();

  celld.fooEx(&celld2);
  celld.fooExT(&celld2);

  celld2.mu_.Unlock();
  celld.mu_.Unlock();
}

};


namespace FunctionDeclDefTest {

class Foo {
public:
  Mutex mu_;
  int a __attribute__((guarded_by(mu_)));

  virtual void foo1(Foo *f_declared) __attribute__((exclusive_locks_required(f_declared->mu_)));
};


void Foo::foo1(Foo *f_defined) {
  f_defined->a = 0;
};

void test() {
  Foo myfoo;
  myfoo.foo1(&myfoo);

  myfoo.mu_.Lock();
  myfoo.foo1(&myfoo);
  myfoo.mu_.Unlock();
}

};

namespace GoingNative {

  struct __attribute__((lockable)) mutex {
    void lock() __attribute__((exclusive_lock_function()));
    void unlock() __attribute__((unlock_function()));

  };
  bool foo();
  bool bar();
  mutex m;
  void test() {
    m.lock();
    while (foo()) {
      m.unlock();

      if (bar()) {

        if (foo())
          continue;

      }

      m.lock();
    }
    m.unlock();
  }

}



namespace FunctionDefinitionTest {

class Foo {
public:
  void foo1();
  void foo2();
  void foo3(Foo *other);

  template<class T>
  void fooT1(const T& dummy1);

  template<class T>
  void fooT2(const T& dummy2) __attribute__((exclusive_locks_required(mu_)));

  Mutex mu_;
  int a __attribute__((guarded_by(mu_)));
};

template<class T>
class FooT {
public:
  void foo();

  Mutex mu_;
  T a __attribute__((guarded_by(mu_)));
};


void Foo::foo1() __attribute__((no_thread_safety_analysis)) {
  a = 1;
}

void Foo::foo2() __attribute__((exclusive_locks_required(mu_))) {
  a = 2;
}

void Foo::foo3(Foo *other) __attribute__((exclusive_locks_required(other->mu_))) {
  other->a = 3;
}

template<class T>
void Foo::fooT1(const T& dummy1) __attribute__((exclusive_locks_required(mu_))) {
  a = dummy1;
}
# 2336 "SemaCXX/warn-thread-safety-analysis.cpp"
void fooF1(Foo *f) __attribute__((exclusive_locks_required(f->mu_))) {
  f->a = 1;
}

void fooF2(Foo *f);
void fooF2(Foo *f) __attribute__((exclusive_locks_required(f->mu_))) {
  f->a = 2;
}

void fooF3(Foo *f) __attribute__((exclusive_locks_required(f->mu_)));
void fooF3(Foo *f) {
  f->a = 3;
}

template<class T>
void FooT<T>::foo() __attribute__((exclusive_locks_required(mu_))) {
  a = 0;
}

void test() {
  int dummy = 0;
  Foo myFoo;

  myFoo.foo2();

  myFoo.foo3(&myFoo);

  myFoo.fooT1(dummy);


  myFoo.fooT2(dummy);


  fooF1(&myFoo);

  fooF2(&myFoo);

  fooF3(&myFoo);


  myFoo.mu_.Lock();
  myFoo.foo2();
  myFoo.foo3(&myFoo);
  myFoo.fooT1(dummy);

  myFoo.fooT2(dummy);

  fooF1(&myFoo);
  fooF2(&myFoo);
  fooF3(&myFoo);
  myFoo.mu_.Unlock();

  FooT<int> myFooT;
  myFooT.foo();

}

}


namespace SelfLockingTest {

class __attribute__((lockable)) MyLock {
public:
  int foo __attribute__((guarded_by(this)));

  void lock() __attribute__((exclusive_lock_function()));
  void unlock() __attribute__((unlock_function()));

  void doSomething() {
    this->lock();
    foo = 0;
    doSomethingElse();
    this->unlock();
  }

  void doSomethingElse() __attribute__((exclusive_locks_required(this))) {
    foo = 1;
  };

  void test() {
    foo = 2;

  }
};


class __attribute__((lockable)) MyLock2 {
public:
  Mutex mu_;
  int foo __attribute__((guarded_by(this)));


  void lock() __attribute__((exclusive_lock_function())) { mu_.Lock(); }
  void unlock() __attribute__((unlock_function())) { mu_.Unlock(); }


  MyLock2() { foo = 1; }
  ~MyLock2() { foo = 0; }
};


}


namespace InvalidNonstatic {



class Foo;

class Foo {
  Mutex* mutex_;

  int foo __attribute__((guarded_by(mutex_)));
};

}


namespace NoReturnTest {

bool condition();
void fatal() __attribute__((noreturn));

Mutex mu_;

void test1() {
  MutexLock lock(&mu_);
  if (condition()) {
    fatal();
    return;
  }
}

}


namespace TestMultiDecl {

class Foo {
public:
  int __attribute__((guarded_by(mu_))) a;
  int __attribute__((guarded_by(mu_))) b, c;

  void foo() {
    a = 0;

    b = 0;

    c = 0;

  }

private:
  Mutex mu_;
};

}


namespace WarnNoDecl {

class Foo {
  void foo(int a); __attribute__((

    exclusive_locks_required(a)));


};

}



namespace MoreLockExpressions {

class Foo {
public:
  Mutex mu_;
  int a __attribute__((guarded_by(mu_)));
};

class Bar {
public:
  int b;
  Foo* f;

  Foo& getFoo() { return *f; }
  Foo& getFoo2(int c) { return *f; }
  Foo& getFoo3(int c, int d) { return *f; }

  Foo& getFooey() { return *f; }
};

Foo& getBarFoo(Bar &bar, int c) { return bar.getFoo2(c); }

void test() {
  Foo foo;
  Foo *fooArray;
  Foo &(*fooFuncPtr)();
  Bar bar;
  int a;
  int b;
  int c;

  bar.getFoo().mu_.Lock();
  bar.getFoo().a = 0;
  bar.getFoo().mu_.Unlock();

  (bar.getFoo().mu_).Lock();
  bar.getFoo().a = 0;
  (bar.getFoo().mu_).Unlock();

  bar.getFoo2(a).mu_.Lock();
  bar.getFoo2(a).a = 0;
  bar.getFoo2(a).mu_.Unlock();

  bar.getFoo3(a, b).mu_.Lock();
  bar.getFoo3(a, b).a = 0;
  bar.getFoo3(a, b).mu_.Unlock();

  getBarFoo(bar, a).mu_.Lock();
  getBarFoo(bar, a).a = 0;
  getBarFoo(bar, a).mu_.Unlock();

  bar.getFoo2(10).mu_.Lock();
  bar.getFoo2(10).a = 0;
  bar.getFoo2(10).mu_.Unlock();

  bar.getFoo2(a + 1).mu_.Lock();
  bar.getFoo2(a + 1).a = 0;
  bar.getFoo2(a + 1).mu_.Unlock();

  (a > 0 ? fooArray[1] : fooArray[b]).mu_.Lock();
  (a > 0 ? fooArray[1] : fooArray[b]).a = 0;
  (a > 0 ? fooArray[1] : fooArray[b]).mu_.Unlock();

  fooFuncPtr().mu_.Lock();
  fooFuncPtr().a = 0;
  fooFuncPtr().mu_.Unlock();
}


void test2() {
  Foo *fooArray;
  Bar bar;
  int a;
  int b;
  int c;

  bar.getFoo().mu_.Lock();
  bar.getFooey().a = 0;


  bar.getFoo().mu_.Unlock();

  bar.getFoo2(a).mu_.Lock();
  bar.getFoo2(b).a = 0;


  bar.getFoo2(a).mu_.Unlock();

  bar.getFoo3(a, b).mu_.Lock();
  bar.getFoo3(a, c).a = 0;


  bar.getFoo3(a, b).mu_.Unlock();

  getBarFoo(bar, a).mu_.Lock();
  getBarFoo(bar, b).a = 0;


  getBarFoo(bar, a).mu_.Unlock();

  (a > 0 ? fooArray[1] : fooArray[b]).mu_.Lock();
  (a > 0 ? fooArray[b] : fooArray[c]).a = 0;


  (a > 0 ? fooArray[1] : fooArray[b]).mu_.Unlock();
}


}


namespace TrylockJoinPoint {

class Foo {
  Mutex mu;
  bool c;

  void foo() {
    if (c) {
      if (!mu.TryLock())
        return;
    } else {
      mu.Lock();
    }
    mu.Unlock();
  }
};

}


namespace LockReturned {

class Foo {
public:
  int a __attribute__((guarded_by(mu_)));
  void foo() __attribute__((exclusive_locks_required(mu_)));
  void foo2(Foo* f) __attribute__((exclusive_locks_required(mu_, f->mu_)));

  static void sfoo(Foo* f) __attribute__((exclusive_locks_required(f->mu_)));

  Mutex* getMu() __attribute__((lock_returned(mu_)));

  Mutex mu_;

  static Mutex* getMu(Foo* f) __attribute__((lock_returned(f->mu_)));
};



void test1(Foo* f1, Foo* f2) {
  f1->a = 0;
  f1->foo();

  f1->foo2(f2);

  Foo::sfoo(f1);

  f1->getMu()->Lock();

  f1->a = 0;
  f1->foo();
  f1->foo2(f2);



  Foo::getMu(f2)->Lock();
  f1->foo2(f2);
  Foo::getMu(f2)->Unlock();

  Foo::sfoo(f1);

  f1->getMu()->Unlock();
}


Mutex* getFooMu(Foo* f) __attribute__((lock_returned(Foo::getMu(f))));

class Bar : public Foo {
public:
  int b __attribute__((guarded_by(getMu())));
  void bar() __attribute__((exclusive_locks_required(getMu())));
  void bar2(Bar* g) __attribute__((exclusive_locks_required(getMu(this), g->getMu())));

  static void sbar(Bar* g) __attribute__((exclusive_locks_required(g->getMu())));
  static void sbar2(Bar* g) __attribute__((exclusive_locks_required(getFooMu(g))));
};





void test2(Bar* b1, Bar* b2) {
  b1->b = 0;
  b1->bar();
  b1->bar2(b2);

  Bar::sbar(b1);
  Bar::sbar2(b1);

  b1->getMu()->Lock();

  b1->b = 0;
  b1->bar();
  b1->bar2(b2);



  b2->getMu()->Lock();
  b1->bar2(b2);

  b2->getMu()->Unlock();

  Bar::sbar(b1);
  Bar::sbar2(b1);

  b1->getMu()->Unlock();
}




void test3(Bar* b1, Bar* b2) {
  b1->mu_.Lock();
  b1->b = 0;
  b1->bar();

  getFooMu(b2)->Lock();
  b1->bar2(b2);
  getFooMu(b2)->Unlock();

  Bar::sbar(b1);
  Bar::sbar2(b1);

  b1->mu_.Unlock();
}

}


namespace ReleasableScopedLock {

class Foo {
  Mutex mu_;
  bool c;
  int a __attribute__((guarded_by(mu_)));

  void test1();
  void test2();
  void test3();
  void test4();
  void test5();
  void test6();
};


void Foo::test1() {
  ReleasableMutexLock rlock(&mu_);
  rlock.Release();
}

void Foo::test2() {
  ReleasableMutexLock rlock(&mu_);
  if (c) {
    rlock.Release();
  }

}

void Foo::test3() {
  ReleasableMutexLock rlock(&mu_);
  a = 0;
  rlock.Release();
  a = 1;
}

void Foo::test4() {
  ReleasableMutexLock rlock(&mu_);
  rlock.Release();
  rlock.Release();
}

void Foo::test5() {
  ReleasableMutexLock rlock(&mu_);
  if (c) {
    rlock.Release();
  }

  rlock.Release();
}

void Foo::test6() {
  ReleasableMutexLock rlock(&mu_);
  do {
    if (c) {
      rlock.Release();
      break;
    }
  } while (c);

  a = 1;
}


}


namespace RelockableScopedLock {

class DeferTraits {};

class __attribute__((scoped_lockable)) RelockableExclusiveMutexLock {
public:
  RelockableExclusiveMutexLock(Mutex *mu) __attribute__((exclusive_lock_function(mu)));
  RelockableExclusiveMutexLock(Mutex *mu, DeferTraits) __attribute__((locks_excluded(mu)));
  ~RelockableExclusiveMutexLock() __attribute__((release_capability()));

  void Lock() __attribute__((exclusive_lock_function()));
  void Unlock() __attribute__((unlock_function()));
};

struct SharedTraits {};
struct ExclusiveTraits {};

class __attribute__((scoped_lockable)) RelockableMutexLock {
public:
  RelockableMutexLock(Mutex *mu, DeferTraits) __attribute__((locks_excluded(mu)));
  RelockableMutexLock(Mutex *mu, SharedTraits) __attribute__((shared_lock_function(mu)));
  RelockableMutexLock(Mutex *mu, ExclusiveTraits) __attribute__((exclusive_lock_function(mu)));
  ~RelockableMutexLock() __attribute__((unlock_function()));

  void Lock() __attribute__((exclusive_lock_function()));
  void Unlock() __attribute__((unlock_function()));

  void ReaderLock() __attribute__((shared_lock_function()));
  void ReaderUnlock() __attribute__((unlock_function()));

  void PromoteShared() __attribute__((unlock_function())) __attribute__((exclusive_lock_function()));
  void DemoteExclusive() __attribute__((unlock_function())) __attribute__((shared_lock_function()));
};

Mutex mu;
int x __attribute__((guarded_by(mu)));
bool b;

void print(int);

void relock() {
  RelockableExclusiveMutexLock scope(&mu);
  x = 2;
  scope.Unlock();

  x = 3;

  scope.Lock();
  x = 4;
}

void deferLock() {
  RelockableExclusiveMutexLock scope(&mu, DeferTraits{});
  x = 2;
  scope.Lock();
  x = 3;
}

void relockExclusive() {
  RelockableMutexLock scope(&mu, SharedTraits{});
  print(x);
  x = 2;
  scope.ReaderUnlock();

  print(x);

  scope.Lock();
  print(x);
  x = 4;

  scope.DemoteExclusive();
  print(x);
  x = 5;
}

void relockShared() {
  RelockableMutexLock scope(&mu, ExclusiveTraits{});
  print(x);
  x = 2;
  scope.Unlock();

  print(x);

  scope.ReaderLock();
  print(x);
  x = 4;

  scope.PromoteShared();
  print(x);
  x = 5;
}

void deferLockShared() {
  RelockableMutexLock scope(&mu, DeferTraits{});
  print(x);
  scope.ReaderLock();
  print(x);
  x = 2;
}

void doubleUnlock() {
  RelockableExclusiveMutexLock scope(&mu);
  scope.Unlock();
  scope.Unlock();
}

void doubleLock1() {
  RelockableExclusiveMutexLock scope(&mu);
  scope.Lock();
}

void doubleLock2() {
  RelockableExclusiveMutexLock scope(&mu);
  scope.Unlock();
  scope.Lock();
  scope.Lock();
}

void lockJoin() {
  RelockableMutexLock scope(&mu, DeferTraits{});
  if (b)
    scope.Lock();

  x = 2;
}

void unlockJoin() {
  RelockableMutexLock scope(&mu, DeferTraits{});
  scope.Lock();
  if (b)
    scope.Unlock();

  x = 2;
}

void loopAcquire() {
  RelockableMutexLock scope(&mu, DeferTraits{});
  for (unsigned i = 1; i < 10; ++i)
    scope.Lock();
}

void loopRelease() {
  RelockableMutexLock scope(&mu, ExclusiveTraits{});

  for (unsigned i = 1; i < 10; ++i) {
    x = 1;
    if (i == 5)
      scope.Unlock();
  }
}

void loopPromote() {
  RelockableMutexLock scope(&mu, SharedTraits{});
  for (unsigned i = 1; i < 10; ++i) {
    x = 1;
    if (i == 5)
      scope.PromoteShared();
  }
}

void loopDemote() {
  RelockableMutexLock scope(&mu, ExclusiveTraits{});

  for (unsigned i = 1; i < 10; ++i) {
    x = 1;
    if (i == 5)
      scope.DemoteExclusive();
  }
}

void loopAcquireContinue() {
  RelockableMutexLock scope(&mu, DeferTraits{});
  for (unsigned i = 1; i < 10; ++i) {
    x = 1;
    if (i == 5) {
      scope.Lock();
      continue;
    }
  }
}

void loopReleaseContinue() {
  RelockableMutexLock scope(&mu, ExclusiveTraits{});

  for (unsigned i = 1; i < 10; ++i) {
    x = 1;
    if (i == 5) {
      scope.Unlock();
      continue;
    }
  }
}

void loopPromoteContinue() {
  RelockableMutexLock scope(&mu, SharedTraits{});
  for (unsigned i = 1; i < 10; ++i) {
    x = 1;
    if (i == 5) {
      scope.PromoteShared();
      continue;
    }
  }
}

void loopDemoteContinue() {
  RelockableMutexLock scope(&mu, ExclusiveTraits{});

  for (unsigned i = 1; i < 10; ++i) {
    x = 1;
    if (i == 5) {
      scope.DemoteExclusive();
      continue;
    }
  }
}

void exclusiveSharedJoin() {
  RelockableMutexLock scope(&mu, DeferTraits{});
  if (b)
    scope.Lock();
  else
    scope.ReaderLock();

  print(x);
  x = 2;
}

void sharedExclusiveJoin() {
  RelockableMutexLock scope(&mu, DeferTraits{});
  if (b)
    scope.ReaderLock();
  else
    scope.Lock();

  print(x);
  x = 2;
}

void assertJoin() {
  RelockableMutexLock scope(&mu, DeferTraits{});
  if (b)
    scope.Lock();
  else
    mu.AssertHeld();
  x = 2;
}

void assertSharedJoin() {
  RelockableMutexLock scope(&mu, DeferTraits{});
  if (b)
    scope.ReaderLock();
  else
    mu.AssertReaderHeld();
  print(x);
  x = 2;
}

void assertStrongerJoin() {
  RelockableMutexLock scope(&mu, DeferTraits{});
  if (b)
    scope.ReaderLock();
  else
    mu.AssertHeld();
  print(x);
  x = 2;
}

void assertWeakerJoin() {
  RelockableMutexLock scope(&mu, DeferTraits{});
  if (b)
    scope.Lock();
  else
    mu.AssertReaderHeld();
  print(x);
  x = 2;
}

void directUnlock() {
  RelockableExclusiveMutexLock scope(&mu);
  mu.Unlock();



  scope.Lock();
}

void directRelock() {
  RelockableExclusiveMutexLock scope(&mu);
  scope.Unlock();
  mu.Lock();

  scope.Unlock();
}


void destructLock() {
  RelockableExclusiveMutexLock scope(&mu);
  scope.~RelockableExclusiveMutexLock();
  scope.Lock();
}

class __attribute__((scoped_lockable)) MemberLock {
public:
  MemberLock() __attribute__((exclusive_lock_function(mutex)));
  ~MemberLock() __attribute__((unlock_function(mutex)));
  void Lock() __attribute__((exclusive_lock_function(mutex)));
  Mutex mutex;
};

void relockShared2() {
  MemberLock lock;
  lock.Lock();
}

class __attribute__((scoped_lockable)) WeirdScope {
private:
  Mutex *other;

public:
  WeirdScope(Mutex *mutex) __attribute__((exclusive_lock_function(mutex)));
  void unlock() __attribute__((release_capability())) __attribute__((release_capability(other)));
  void lock() __attribute__((exclusive_lock_function())) __attribute__((exclusive_lock_function(other)));
  ~WeirdScope() __attribute__((release_capability()));

  void requireOther() __attribute__((exclusive_locks_required(other)));
};

void relockWeird() {
  WeirdScope scope(&mu);
  x = 1;
  scope.unlock();
  x = 2;

  scope.requireOther();

  scope.lock();
  x = 3;
  scope.requireOther();
}

}


namespace ScopedUnlock {

class __attribute__((scoped_lockable)) MutexUnlock {
public:
  MutexUnlock(Mutex *mu) __attribute__((release_capability(mu)));
  ~MutexUnlock() __attribute__((release_capability()));

  void Lock() __attribute__((release_capability()));
  void Unlock() __attribute__((exclusive_lock_function()));
};

class __attribute__((scoped_lockable)) ReaderMutexUnlock {
public:
  ReaderMutexUnlock(Mutex *mu) __attribute__((release_shared_capability(mu)));
  ~ReaderMutexUnlock() __attribute__((release_capability()));

  void Lock() __attribute__((release_capability()));
  void Unlock() __attribute__((exclusive_lock_function()));
};

template<typename... Mus>
class __attribute__((scoped_lockable)) VariadicMutexUnlock {
public:
  VariadicMutexUnlock(Mus *...mus) __attribute__((release_capability(mus...)));
  ~VariadicMutexUnlock() __attribute__((release_capability()));

  void Lock() __attribute__((release_capability()));
  void Unlock() __attribute__((exclusive_lock_function()));
};

Mutex mu;
int x __attribute__((guarded_by(mu)));
bool c;
void print(int);

void simple() __attribute__((exclusive_locks_required(mu))) {
  x = 1;
  MutexUnlock scope(&mu);
  x = 2;
}

void simpleShared() __attribute__((shared_locks_required(mu))) {
  print(x);
  ReaderMutexUnlock scope(&mu);
  print(x);
}

void innerUnlock() {
  MutexLock outer(&mu);
  if (x == 0) {
    MutexUnlock inner(&mu);
    x = 1;
  }
  x = 2;
}

void innerUnlockShared() {
  ReaderMutexLock outer(&mu);
  if (x == 0) {
    ReaderMutexUnlock inner(&mu);
    print(x);
  }
  print(x);
}

void manual() __attribute__((exclusive_locks_required(mu))) {
  MutexUnlock scope(&mu);
  scope.Lock();
  x = 2;
  scope.Unlock();
  x = 3;
}

void join() __attribute__((exclusive_locks_required(mu))) {
  MutexUnlock scope(&mu);
  if (c)
    scope.Lock();

  scope.Lock();
}

void doubleLock() __attribute__((exclusive_locks_required(mu))) {
  MutexUnlock scope(&mu);
  scope.Lock();
  scope.Lock();
}

void doubleUnlock() __attribute__((exclusive_locks_required(mu))) {
  MutexUnlock scope(&mu);
  scope.Unlock();
}

Mutex mu2;
int y __attribute__((guarded_by(mu2)));

void variadic() __attribute__((exclusive_locks_required(mu, mu2))) {
  VariadicMutexUnlock<Mutex, Mutex> scope(&mu, &mu2);
  x = 2;
  y = 3;
  scope.Lock();
  x = y = 4;
}

class __attribute__((scoped_lockable)) MutexLockUnlock {
public:
  MutexLockUnlock(Mutex *mu1, Mutex *mu2) __attribute__((release_capability(mu1))) __attribute__((exclusive_lock_function(mu2)));
  ~MutexLockUnlock() __attribute__((release_capability()));

  void Release() __attribute__((release_capability()));
  void Acquire() __attribute__((exclusive_lock_function()));
};

Mutex other;
void fn() __attribute__((exclusive_locks_required(other)));

void lockUnlock() __attribute__((exclusive_locks_required(mu))) {
  MutexLockUnlock scope(&mu, &other);
  fn();
  x = 1;
}

}

namespace PassingScope {

class __attribute__((scoped_lockable)) RelockableScope {
public:
  RelockableScope(Mutex *mu) __attribute__((exclusive_lock_function(mu)));
  void Release() __attribute__((unlock_function()));
  void Acquire() __attribute__((exclusive_lock_function()));
  ~RelockableScope() __attribute__((release_capability()));
};

class __attribute__((scoped_lockable)) ReaderRelockableScope {
public:
  ReaderRelockableScope(Mutex *mu) __attribute__((shared_lock_function(mu)));
  void Release() __attribute__((unlock_function()));
  void Acquire() __attribute__((shared_lock_function()));
  ~ReaderRelockableScope() __attribute__((unlock_function()));
};

Mutex mu;
Mutex mu1;
Mutex mu2;
int x __attribute__((guarded_by(mu)));
int y __attribute__((guarded_by(mu))) __attribute__((guarded_by(mu2)));
void print(int);



void sharedRequired(ReleasableMutexLock& scope __attribute__((shared_locks_required(mu)))) {
  print(x);
}

void sharedAcquire(ReaderRelockableScope& scope __attribute__((shared_lock_function(mu)))) {
  scope.Acquire();
  print(x);
}

void sharedRelease(RelockableScope& scope __attribute__((release_shared_capability(mu)))) {
  print(x);
  scope.Release();
  print(x);
}

void requiredLock(ReleasableMutexLock& scope __attribute__((exclusive_locks_required(mu)))) {
  x = 1;
}

void reacquireRequiredLock(RelockableScope& scope __attribute__((exclusive_locks_required(mu)))) {
  scope.Release();
  scope.Acquire();
  x = 2;
}

void releaseSingleMutex(ReleasableMutexLock& scope __attribute__((release_capability(mu)))) {
  x = 1;
  scope.Release();
  x = 2;
}

void releaseMultipleMutexes(ReleasableMutexLock& scope __attribute__((release_capability(mu, mu2)))) {
  y = 1;
  scope.Release();
  y = 2;

}

void acquireLock(RelockableScope& scope __attribute__((exclusive_lock_function(mu)))) {
  x = 1;
  scope.Acquire();
  x = 2;
}

void acquireMultipleLocks(RelockableScope& scope __attribute__((exclusive_lock_function(mu, mu2)))) {
  y = 1;

  scope.Acquire();
  y = 2;
}

void excludedLock(ReleasableMutexLock& scope __attribute__((locks_excluded(mu)))) {
  x = 1;
}

void acquireAndReleaseExcludedLocks(RelockableScope& scope __attribute__((locks_excluded(mu)))) {
  scope.Acquire();
  scope.Release();
}


void unreleasedMutex(ReleasableMutexLock& scope __attribute__((release_capability(mu)))) {
  x = 1;
}


void acquireAlreadyHeldMutex(RelockableScope& scope __attribute__((release_capability(mu)))) {
  scope.Acquire();
  scope.Release();
}

void reacquireMutex(RelockableScope& scope __attribute__((release_capability(mu)))) {
  scope.Release();
  scope.Acquire();
  x = 2;
}


void requireSingleMutex(ReleasableMutexLock& scope __attribute__((exclusive_locks_required(mu)))) {
  x = 1;
  scope.Release();
  x = 2;
}


void requireMultipleMutexes(ReleasableMutexLock& scope __attribute__((exclusive_locks_required(mu, mu2)))) {
  y = 1;
  scope.Release();
  y = 2;

}



void acquireAlreadyHeldLock(RelockableScope& scope __attribute__((exclusive_locks_required(mu)))) {
  scope.Acquire();
}


void releaseWithoutHoldingLock(ReleasableMutexLock& scope __attribute__((exclusive_lock_function(mu)))) {
  scope.Release();
}


void endWithReleasedMutex(RelockableScope& scope __attribute__((exclusive_lock_function(mu)))) {
  scope.Acquire();
  scope.Release();
}

void acquireExcludedLock(RelockableScope& scope __attribute__((locks_excluded(mu)))) {
  x = 1;
  scope.Acquire();
  x = 2;
}

void acquireMultipleExcludedLocks(RelockableScope& scope __attribute__((locks_excluded(mu, mu2)))) {
  y = 1;

  scope.Acquire();
  y = 2;
}


void reacquireExcludedLocks(RelockableScope& scope __attribute__((locks_excluded(mu)))) {
  scope.Release();
  scope.Acquire();
  x = 2;
}


void sharedRequired2(ReleasableMutexLock& scope __attribute__((shared_locks_required(mu)))) {
  print(x);
  scope.Release();
  print(x);
}


void sharedAcquire2(RelockableScope& scope __attribute__((shared_lock_function(mu)))) {
  print(x);
  scope.Release();
}


void sharedRelease2(RelockableScope& scope __attribute__((release_shared_capability(mu)))) {
  scope.Acquire();
}




void release(ReleasableMutexLock& scope __attribute__((release_capability(mu))));

void release_DoubleMutexLock(DoubleMutexLock& scope __attribute__((release_capability(mu))));

void release_two(ReleasableMutexLock& scope __attribute__((release_capability(mu, mu2))));

void release_double(DoubleMutexLock& scope __attribute__((release_capability(mu, mu2))));
void require(ReleasableMutexLock& scope __attribute__((exclusive_locks_required(mu))));
void acquire(RelockableScope& scope __attribute__((exclusive_lock_function(mu))));
void exclude(RelockableScope& scope __attribute__((locks_excluded(mu))));

void release_shared(ReaderRelockableScope& scope __attribute__((release_shared_capability(mu))));
void require_shared(ReleasableMutexLock& scope __attribute__((shared_locks_required(mu))));
void acquire_shared(ReaderRelockableScope& scope __attribute__((shared_lock_function(mu))));

void unlockCall() {
  ReleasableMutexLock scope(&mu);
  x = 1;
  release(scope);
  x = 2;
}

void unlockSharedCall() {
  ReaderRelockableScope scope(&mu);
  print(x);
  release_shared(scope);
  print(x);
}

void requireCall() {
  ReleasableMutexLock scope(&mu);
  x = 1;
  require(scope);
  x = 2;
}

void requireSharedCall() {
  ReleasableMutexLock scope(&mu);
  print(x);
  require_shared(scope);
  print(x);
}

void acquireCall() {
  RelockableScope scope(&mu);
  scope.Release();
  acquire(scope);
  x = 2;
}

void acquireSharedCall() {
  ReaderRelockableScope scope(&mu);
  scope.Release();
  acquire_shared(scope);
  print(x);
}

void writeAfterExcludeCall() {
  RelockableScope scope(&mu);
  scope.Release();
  exclude(scope);
  x = 2;
}

void unlockCallAfterExplicitRelease() {
  ReleasableMutexLock scope(&mu);
  x = 1;
  scope.Release();
  release(scope);
  x = 2;
}

void unmatchedMutexes() {
  ReleasableMutexLock scope(&mu2);
  release(scope);

}

void wrongOrder() {
  DoubleMutexLock scope(&mu2, &mu);
  release_double(scope);
}

void differentNumberOfMutexes() {
  ReleasableMutexLock scope(&mu);
  release_two(scope);

}

void differentNumberOfMutexes2() {
  ReleasableMutexLock scope(&mu2);
  release_two(scope);

}

void differentNumberOfMutexes3() {
  DoubleMutexLock scope(&mu, &mu2);
  release_DoubleMutexLock(scope);
}

void releaseDefault(ReleasableMutexLock& scope __attribute__((release_capability(mu))), int = 0);

void unlockFunctionDefault() {
  ReleasableMutexLock scope(&mu);
  x = 1;
  releaseDefault(scope);
  x = 2;
}

void requireCallWithReleasedLock() {
  ReleasableMutexLock scope(&mu);
  scope.Release();
  require(scope);
}

void acquireCallWithAlreadyHeldLock() {
  RelockableScope scope(&mu);
  acquire(scope);
  x = 1;
}

void excludeCallWithAlreadyHeldLock() {
  RelockableScope scope(&mu);
  exclude(scope);
  x = 2;
}

void requireConst(const ReleasableMutexLock& scope __attribute__((exclusive_locks_required(mu))));
void requireConstCall() {
  requireConst(ReleasableMutexLock(&mu));
}

void passScopeUndeclared(ReleasableMutexLock &scope) {
  release(scope);

}

class __attribute__((scoped_lockable)) ScopedWithoutLock {
public:
  ScopedWithoutLock();

  ~ScopedWithoutLock() __attribute__((release_capability()));
};

void require(ScopedWithoutLock &scope __attribute__((exclusive_locks_required(mu))));

void constructWithoutLock() {
  ScopedWithoutLock scope;
  require(scope);

}

void requireConst(const ScopedWithoutLock& scope __attribute__((exclusive_locks_required(mu))));

void requireCallWithReleasedLock2() {
  requireConst(ScopedWithoutLock());


}

void requireDecl(RelockableScope &scope __attribute__((exclusive_locks_required(mu))));
void requireDecl(RelockableScope &scope) {
  scope.Release();
  scope.Acquire();
}

struct foo
{
  Mutex mu;

  void require(RelockableScope &scope __attribute__((exclusive_locks_required(mu))));
  void callRequire(){
    RelockableScope scope(&mu);

    require(scope);


  }
};

struct ObjectWithMutex {
  Mutex mu;
  int x __attribute__((guarded_by(mu)));
};
void releaseMember(ObjectWithMutex& object, ReleasableMutexLock& scope __attribute__((release_capability(object.mu)))) {
  object.x = 1;
  scope.Release();
}
void releaseMemberCall() {
  ObjectWithMutex obj;
  ReleasableMutexLock lock(&obj.mu);
  releaseMember(obj, lock);
}

}

namespace TrylockFunctionTest {

class Foo {
public:
  Mutex mu1_;
  Mutex mu2_;
  bool c;

  bool lockBoth() __attribute__((exclusive_trylock_function(true, mu1_, mu2_)));
};

bool Foo::lockBoth() {
  if (!mu1_.TryLock())
    return false;

  mu2_.Lock();
  if (!c) {
    mu1_.Unlock();
    mu2_.Unlock();
    return false;
  }

  return true;
}


}



namespace DoubleLockBug {

class Foo {
public:
  Mutex mu_;
  int a __attribute__((guarded_by(mu_)));

  void foo1() __attribute__((exclusive_locks_required(mu_)));
  int foo2() __attribute__((shared_locks_required(mu_)));
};


void Foo::foo1() __attribute__((exclusive_locks_required(mu_))) {
  a = 0;
}

int Foo::foo2() __attribute__((shared_locks_required(mu_))) {
  return a;
}

}



namespace UnlockBug {

class Foo {
public:
  Mutex mutex_;

  void foo1() __attribute__((exclusive_locks_required(mutex_))) {
    mutex_.Unlock();
  }


  void foo2() __attribute__((shared_locks_required(mutex_))) {
    mutex_.Unlock();
  }
};

}



namespace FoolishScopedLockableBug {

class __attribute__((scoped_lockable)) WTF_ScopedLockable {
public:
  WTF_ScopedLockable(Mutex* mu) __attribute__((exclusive_lock_function(mu)));


  ~WTF_ScopedLockable();

  void release() __attribute__((unlock_function()));
};


class Foo {
  Mutex mu_;
  int a __attribute__((guarded_by(mu_)));
  bool c;

  void doSomething();

  void test1() {
    WTF_ScopedLockable wtf(&mu_);
    wtf.release();
  }

  void test2() {
    WTF_ScopedLockable wtf(&mu_);
  }

  void test3() {
    if (c) {
      WTF_ScopedLockable wtf(&mu_);
      wtf.release();
    }
  }

  void test4() {
    if (c) {
      doSomething();
    }
    else {
      WTF_ScopedLockable wtf(&mu_);
      wtf.release();
    }
  }

  void test5() {
    if (c) {
      WTF_ScopedLockable wtf(&mu_);
    }
  }

  void test6() {
    if (c) {
      doSomething();
    }
    else {
      WTF_ScopedLockable wtf(&mu_);
    }
  }
};


}



namespace TemporaryCleanupExpr {

class Foo {
  int a __attribute__((guarded_by(getMutexPtr().get())));

  SmartPtr<Mutex> getMutexPtr();

  void test();
};


void Foo::test() {
  {
    ReaderMutexLock lock(getMutexPtr().get());
    int b = a;
  }
  int b = a;
}
# 3831 "SemaCXX/warn-thread-safety-analysis.cpp"
}



namespace SmartPointerTests {

class Foo {
public:
  SmartPtr<Mutex> mu_;
  int a __attribute__((guarded_by(mu_)));
  int b __attribute__((guarded_by(mu_.get())));
  int c __attribute__((guarded_by(*mu_)));

  void Lock() __attribute__((exclusive_lock_function(mu_)));
  void Unlock() __attribute__((unlock_function(mu_)));

  void test0();
  void test1();
  void test2();
  void test3();
  void test4();
  void test5();
  void test6();
  void test7();
  void test8();
};

void Foo::test0() {
  a = 0;
  b = 0;
  c = 0;
}

void Foo::test1() {
  mu_->Lock();
  a = 0;
  b = 0;
  c = 0;
  mu_->Unlock();
}

void Foo::test2() {
  (*mu_).Lock();
  a = 0;
  b = 0;
  c = 0;
  (*mu_).Unlock();
}


void Foo::test3() {
  mu_.get()->Lock();
  a = 0;
  b = 0;
  c = 0;
  mu_.get()->Unlock();
}


void Foo::test4() {
  MutexLock lock(mu_.get());
  a = 0;
  b = 0;
  c = 0;
}


void Foo::test5() {
  MutexLock lock(&(*mu_));
  a = 0;
  b = 0;
  c = 0;
}


void Foo::test6() {
  Lock();
  a = 0;
  b = 0;
  c = 0;
  Unlock();
}


void Foo::test7() {
  {
    Lock();
    mu_->Unlock();
  }
  {
    mu_->Lock();
    Unlock();
  }
  {
    mu_.get()->Lock();
    mu_->Unlock();
  }
  {
    mu_->Lock();
    mu_.get()->Unlock();
  }
  {
    mu_.get()->Lock();
    (*mu_).Unlock();
  }
  {
    (*mu_).Lock();
    mu_->Unlock();
  }
}


void Foo::test8() {
  mu_->Lock();
  mu_.get()->Lock();
  (*mu_).Lock();
  mu_.get()->Unlock();
  Unlock();
}


class Bar {
  SmartPtr<Foo> foo;

  void test0();
  void test1();
  void test2();
  void test3();
};


void Bar::test0() {
  foo->a = 0;
  (*foo).b = 0;
  foo.get()->c = 0;
}


void Bar::test1() {
  foo->mu_->Lock();
  foo->a = 0;
  (*foo).b = 0;
  foo.get()->c = 0;
  foo->mu_->Unlock();
}


void Bar::test2() {
  (*foo).mu_->Lock();
  foo->a = 0;
  (*foo).b = 0;
  foo.get()->c = 0;
  foo.get()->mu_->Unlock();
}


void Bar::test3() {
  MutexLock lock(foo->mu_.get());
  foo->a = 0;
  (*foo).b = 0;
  foo.get()->c = 0;
}

}



namespace DuplicateAttributeTest {

class __attribute__((lockable)) Foo {
public:
  Mutex mu1_;
  Mutex mu2_;
  Mutex mu3_;
  int a __attribute__((guarded_by(mu1_)));
  int b __attribute__((guarded_by(mu2_)));
  int c __attribute__((guarded_by(mu3_)));

  void lock() __attribute__((exclusive_lock_function()));
  void unlock() __attribute__((unlock_function()));

  void lock1() __attribute__((exclusive_lock_function(mu1_)));
  void slock1() __attribute__((shared_lock_function(mu1_)));
  void lock3() __attribute__((exclusive_lock_function(mu1_, mu2_, mu3_)));
  void locklots()
    __attribute__((exclusive_lock_function(mu1_)))
    __attribute__((exclusive_lock_function(mu2_)))
    __attribute__((exclusive_lock_function(mu1_, mu2_, mu3_)));

  void unlock1() __attribute__((unlock_function(mu1_)));
  void unlock3() __attribute__((unlock_function(mu1_, mu2_, mu3_)));
  void unlocklots()
    __attribute__((unlock_function(mu1_)))
    __attribute__((unlock_function(mu2_)))
    __attribute__((unlock_function(mu1_, mu2_, mu3_)));
};


void Foo::lock() __attribute__((exclusive_lock_function())) { }
void Foo::unlock() __attribute__((unlock_function())) { }

void Foo::lock1() __attribute__((exclusive_lock_function(mu1_))) {
  mu1_.Lock();
}

void Foo::slock1() __attribute__((shared_lock_function(mu1_))) {
  mu1_.ReaderLock();
}

void Foo::lock3() __attribute__((exclusive_lock_function(mu1_, mu2_, mu3_))) {
  mu1_.Lock();
  mu2_.Lock();
  mu3_.Lock();
}

void Foo::locklots()
    __attribute__((exclusive_lock_function(mu1_, mu2_)))
    __attribute__((exclusive_lock_function(mu2_, mu3_))) {
  mu1_.Lock();
  mu2_.Lock();
  mu3_.Lock();
}

void Foo::unlock1() __attribute__((unlock_function(mu1_))) {
  mu1_.Unlock();
}

void Foo::unlock3() __attribute__((unlock_function(mu1_, mu2_, mu3_))) {
  mu1_.Unlock();
  mu2_.Unlock();
  mu3_.Unlock();
}

void Foo::unlocklots()
    __attribute__((unlock_function(mu1_, mu2_)))
    __attribute__((unlock_function(mu2_, mu3_))) {
  mu1_.Unlock();
  mu2_.Unlock();
  mu3_.Unlock();
}


void test0() {
  Foo foo;
  foo.lock();
  foo.unlock();

  foo.lock();
  foo.lock();
  foo.unlock();
  foo.unlock();
}


void test1() {
  Foo foo;
  foo.lock1();
  foo.a = 0;
  foo.unlock1();

  foo.lock1();
  foo.lock1();
  foo.a = 0;
  foo.unlock1();
  foo.unlock1();
}


int test2() {
  Foo foo;
  foo.slock1();
  int d1 = foo.a;
  foo.unlock1();

  foo.slock1();
  foo.slock1();
  int d2 = foo.a;
  foo.unlock1();
  foo.unlock1();
  return d1 + d2;
}


void test3() {
  Foo foo;
  foo.lock3();
  foo.a = 0;
  foo.b = 0;
  foo.c = 0;
  foo.unlock3();

  foo.lock3();
  foo.lock3();



  foo.a = 0;
  foo.b = 0;
  foo.c = 0;
  foo.unlock3();
  foo.unlock3();



}


void testlots() {
  Foo foo;
  foo.locklots();
  foo.a = 0;
  foo.b = 0;
  foo.c = 0;
  foo.unlocklots();

  foo.locklots();
  foo.locklots();



  foo.a = 0;
  foo.b = 0;
  foo.c = 0;
  foo.unlocklots();
  foo.unlocklots();



}

}



namespace TryLockEqTest {

class Foo {
  Mutex mu_;
  int a __attribute__((guarded_by(mu_)));
  bool c;

  int tryLockMutexI() __attribute__((exclusive_trylock_function(1, mu_)));
  Mutex* tryLockMutexP() __attribute__((exclusive_trylock_function(1, mu_)));
  void unlock() __attribute__((unlock_function(mu_)));

  void test1();
  void test2();
};


void Foo::test1() {
  if (tryLockMutexP() == 0) {
    a = 0;
    return;
  }
  a = 0;
  unlock();

  if (tryLockMutexP() != 0) {
    a = 0;
    unlock();
  }

  if (0 != tryLockMutexP()) {
    a = 0;
    unlock();
  }

  if (!(tryLockMutexP() == 0)) {
    a = 0;
    unlock();
  }

  if (tryLockMutexI() == 0) {
    a = 0;
    return;
  }
  a = 0;
  unlock();

  if (0 == tryLockMutexI()) {
    a = 0;
    return;
  }
  a = 0;
  unlock();

  if (tryLockMutexI() == 1) {
    a = 0;
    unlock();
  }

  if (mu_.TryLock() == false) {
    a = 0;
    return;
  }
  a = 0;
  unlock();

  if (mu_.TryLock() == true) {
    a = 0;
    unlock();
  }
  else {
    a = 0;
  }


  if (tryLockMutexP() == nullptr) {
    a = 0;
    return;
  }
  a = 0;
  unlock();

}

}


namespace ExistentialPatternMatching {

class Graph {
public:
  Mutex mu_;
};

void LockAllGraphs() __attribute__((exclusive_lock_function(&Graph::mu_)));
void UnlockAllGraphs() __attribute__((unlock_function(&Graph::mu_)));

class Node {
public:
  int a __attribute__((guarded_by(&Graph::mu_)));

  void foo() __attribute__((exclusive_locks_required(&Graph::mu_))) {
    a = 0;
  }
  void foo2() __attribute__((locks_excluded(&Graph::mu_)));
};

void test() {
  Graph g1;
  Graph g2;
  Node n1;

  n1.a = 0;
  n1.foo();
  n1.foo2();

  g1.mu_.Lock();
  n1.a = 0;
  n1.foo();
  n1.foo2();
  g1.mu_.Unlock();

  g2.mu_.Lock();
  n1.a = 0;
  n1.foo();
  n1.foo2();
  g2.mu_.Unlock();

  LockAllGraphs();
  n1.a = 0;
  n1.foo();
  n1.foo2();
  UnlockAllGraphs();

  LockAllGraphs();
  g1.mu_.Unlock();

  LockAllGraphs();
  g2.mu_.Unlock();

  LockAllGraphs();
  g1.mu_.Lock();
  g1.mu_.Unlock();
}

}


namespace StringIgnoreTest {

class Foo {
public:
  Mutex mu_;
  void lock() __attribute__((exclusive_lock_function("")));
  void unlock() __attribute__((unlock_function("")));
  void goober() __attribute__((exclusive_locks_required("")));
  void roober() __attribute__((shared_locks_required("")));
};


class Bar : public Foo {
public:
  void bar(Foo* f) {
    f->unlock();
    f->goober();
    f->roober();
    f->lock();
  };
};

}


namespace LockReturnedScopeFix {

class Base {
protected:
  struct Inner;
  bool c;

  const Mutex& getLock(const Inner* i);

  void lockInner (Inner* i) __attribute__((exclusive_lock_function(getLock(i))));
  void unlockInner(Inner* i) __attribute__((unlock_function(getLock(i))));
  void foo(Inner* i) __attribute__((exclusive_locks_required(getLock(i))));

  void bar(Inner* i);
};


struct Base::Inner {
  Mutex lock_;
  void doSomething() __attribute__((exclusive_locks_required(lock_)));
};


const Mutex& Base::getLock(const Inner* i) __attribute__((lock_returned(i->lock_))) {
  return i->lock_;
}


void Base::foo(Inner* i) {
  i->doSomething();
}

void Base::bar(Inner* i) {
  if (c) {
    i->lock_.Lock();
    unlockInner(i);
  }
  else {
    lockInner(i);
    i->lock_.Unlock();
  }
}

}


namespace TrylockWithCleanups {

struct Foo {
  Mutex mu_;
  int a __attribute__((guarded_by(mu_)));
};

Foo* GetAndLockFoo(const MyString& s)
    __attribute__((exclusive_trylock_function(true, &Foo::mu_)));

static void test() {
  Foo* lt = GetAndLockFoo("foo");
  if (!lt) return;
  int a = lt->a;
  lt->mu_.Unlock();
}

}


namespace UniversalLock {

class Foo {
  Mutex mu_;
  bool c;

  int a __attribute__((guarded_by(mu_)));
  void r_foo() __attribute__((shared_locks_required(mu_)));
  void w_foo() __attribute__((exclusive_locks_required(mu_)));

  void test1() {
    int b;

    beginNoWarnOnReads();
    b = a;
    r_foo();
    endNoWarnOnReads();

    beginNoWarnOnWrites();
    a = 0;
    w_foo();
    endNoWarnOnWrites();
  }


  void test2() {
    if (c) {
      beginNoWarnOnWrites();
    }
    a = 0;

    endNoWarnOnWrites();

  }



  void test3() {
    if (c) {
      mu_.Lock();
      beginNoWarnOnWrites();
    }
    else {
      beginNoWarnOnWrites();
      mu_.Lock();
    }
    a = 0;
    endNoWarnOnWrites();
    mu_.Unlock();
  }



  void test4() {
    beginNoWarnOnWrites();
    mu_.Lock();
    mu_.Unlock();
    endNoWarnOnWrites();

    mu_.Lock();
    beginNoWarnOnWrites();
    endNoWarnOnWrites();
    mu_.Unlock();

    mu_.Lock();
    beginNoWarnOnWrites();
    mu_.Unlock();
    endNoWarnOnWrites();
  }
};

}


namespace TemplateLockReturned {

template<class T>
class BaseT {
public:
  virtual void baseMethod() = 0;
  Mutex* get_mutex() __attribute__((lock_returned(mutex_))) { return &mutex_; }

  Mutex mutex_;
  int a __attribute__((guarded_by(mutex_)));
};


class Derived : public BaseT<int> {
public:
  void baseMethod() __attribute__((exclusive_locks_required(get_mutex()))) {
    a = 0;
  }
};

}


namespace ExprMatchingBugFix {

class Foo {
public:
  Mutex mu_;
};


class Bar {
public:
  bool c;
  Foo* foo;
  Bar(Foo* f) : foo(f) { }

  struct Nested {
    Foo* foo;
    Nested(Foo* f) : foo(f) { }

    void unlockFoo() __attribute__((unlock_function(&Foo::mu_)));
  };

  void test();
};


void Bar::test() {
  foo->mu_.Lock();
  if (c) {
    Nested *n = new Nested(foo);
    n->unlockFoo();
  }
  else {
    foo->mu_.Unlock();
  }
}

};


namespace ComplexNameTest {

class Foo {
public:
  static Mutex mu_;

  Foo() __attribute__((exclusive_locks_required(mu_))) { }
  ~Foo() __attribute__((exclusive_locks_required(mu_))) { }

  int operator[](int i) __attribute__((exclusive_locks_required(mu_))) { return 0; }
};

class Bar {
public:
  static Mutex mu_;

  Bar() __attribute__((locks_excluded(mu_))) { }
  ~Bar() __attribute__((locks_excluded(mu_))) { }

  int operator[](int i) __attribute__((locks_excluded(mu_))) { return 0; }
};


void test1() {
  Foo f;
  int a = f[0];
}


void test2() {
  Bar::mu_.Lock();
  {
    Bar b;
    int a = b[0];
  }
  Bar::mu_.Unlock();
}

};


namespace UnreachableExitTest {

class FemmeFatale {
public:
  FemmeFatale();
  ~FemmeFatale() __attribute__((noreturn));
};

void exitNow() __attribute__((noreturn));
void exitDestruct(const MyString& ms) __attribute__((noreturn));

Mutex fatalmu_;

void test1() __attribute__((exclusive_locks_required(fatalmu_))) {
  exitNow();
}

void test2() __attribute__((exclusive_locks_required(fatalmu_))) {
  FemmeFatale femme;
}

bool c;

void test3() __attribute__((exclusive_locks_required(fatalmu_))) {
  if (c) {
    exitNow();
  }
  else {
    FemmeFatale femme;
  }
}

void test4() __attribute__((exclusive_locks_required(fatalmu_))) {
  exitDestruct("foo");
}

}


namespace VirtualMethodCanonicalizationTest {

class Base {
public:
  virtual Mutex* getMutex() = 0;
};

class Base2 : public Base {
public:
  Mutex* getMutex();
};

class Base3 : public Base2 {
public:
  Mutex* getMutex();
};

class Derived : public Base3 {
public:
  Mutex* getMutex();
};

void baseFun(Base *b) __attribute__((exclusive_locks_required(b->getMutex()))) { }

void derivedFun(Derived *d) __attribute__((exclusive_locks_required(d->getMutex()))) {
  baseFun(d);
}

}


namespace TemplateFunctionParamRemapTest {

template <class T>
struct Cell {
  T dummy_;
  Mutex* mu_;
};

class Foo {
public:
  template <class T>
  void elr(Cell<T>* c) __attribute__((exclusive_locks_required(c->mu_)));

  void test();
};

template<class T>
void Foo::elr(Cell<T>* c1) { }

void Foo::test() {
  Cell<int> cell;
  elr(&cell);

}


template<class T>
void globalELR(Cell<T>* c) __attribute__((exclusive_locks_required(c->mu_)));

template<class T>
void globalELR(Cell<T>* c1) { }

void globalTest() {
  Cell<int> cell;
  globalELR(&cell);

}


template<class T>
void globalELR2(Cell<T>* c) __attribute__((exclusive_locks_required(c->mu_)));


template<class T>
void globalELR2(Cell<T>* c2);

template<class T>
void globalELR2(Cell<T>* c3) { }


template<class T>
void globalELR2(Cell<T>* c4);

void globalTest2() {
  Cell<int> cell;
  globalELR2(&cell);

}


template<class T>
class FooT {
public:
  void elr(Cell<T>* c) __attribute__((exclusive_locks_required(c->mu_)));
};

template<class T>
void FooT<T>::elr(Cell<T>* c1) { }

void testFooT() {
  Cell<int> cell;
  FooT<int> foo;
  foo.elr(&cell);

}

}


namespace SelfConstructorTest {

class SelfLock {
public:
  SelfLock() __attribute__((exclusive_lock_function(mu_)));
  ~SelfLock() __attribute__((unlock_function(mu_)));

  void foo() __attribute__((exclusive_locks_required(mu_)));

  Mutex mu_;
};

class __attribute__((lockable)) SelfLock2 {
public:
  SelfLock2() __attribute__((exclusive_lock_function()));
  ~SelfLock2() __attribute__((unlock_function()));

  void foo() __attribute__((exclusive_locks_required(this)));
};

class SelfLockDeferred {
public:
  SelfLockDeferred() __attribute__((locks_excluded(mu_)));
  ~SelfLockDeferred() __attribute__((unlock_function(mu_)));

  Mutex mu_;
};

class __attribute__((lockable)) SelfLockDeferred2 {
public:
  SelfLockDeferred2() __attribute__((locks_excluded(this)));
  ~SelfLockDeferred2() __attribute__((unlock_function()));
};


void test() {
  SelfLock s;
  s.foo();
}

void test2() {
  SelfLock2 s2;
  s2.foo();
}

void testDeferredTemporary() {
  SelfLockDeferred();
}

void testDeferredTemporary2() {
  SelfLockDeferred2();
}

}


namespace MultipleAttributeTest {

class Foo {
  Mutex mu1_;
  Mutex mu2_;
  int a __attribute__((guarded_by(mu1_)));
  int b __attribute__((guarded_by(mu2_)));
  int c __attribute__((guarded_by(mu1_))) __attribute__((guarded_by(mu2_)));
  int* d __attribute__((pt_guarded_by(mu1_))) __attribute__((pt_guarded_by(mu2_)));

  void foo1() __attribute__((exclusive_locks_required(mu1_)))
                       __attribute__((exclusive_locks_required(mu2_)));
  void foo2() __attribute__((shared_locks_required(mu1_)))
                       __attribute__((shared_locks_required(mu2_)));
  void foo3() __attribute__((locks_excluded(mu1_)))
                       __attribute__((locks_excluded(mu2_)));
  void lock() __attribute__((exclusive_lock_function(mu1_)))
                       __attribute__((exclusive_lock_function(mu2_)));
  void readerlock() __attribute__((shared_lock_function(mu1_)))
                       __attribute__((shared_lock_function(mu2_)));
  void unlock() __attribute__((unlock_function(mu1_)))
                       __attribute__((unlock_function(mu2_)));
  bool trylock() __attribute__((exclusive_trylock_function(true, mu1_)))
                       __attribute__((exclusive_trylock_function(true, mu2_)));
  bool readertrylock() __attribute__((shared_trylock_function(true, mu1_)))
                       __attribute__((shared_trylock_function(true, mu2_)));
  void assertBoth() __attribute__((assert_exclusive_lock(mu1_)))
                    __attribute__((assert_exclusive_lock(mu2_)));

  void alsoAssertBoth() __attribute__((assert_exclusive_lock(mu1_, mu2_)));

  void assertShared() __attribute__((assert_shared_lock(mu1_)))
                      __attribute__((assert_shared_lock(mu2_)));

  void alsoAssertShared() __attribute__((assert_shared_lock(mu1_, mu2_)));

  void test();
  void testAssert();
  void testAssertShared();
};


void Foo::foo1() {
  a = 1;
  b = 2;
}

void Foo::foo2() {
  int result = a + b;
}

void Foo::foo3() { }
void Foo::lock() { mu1_.Lock(); mu2_.Lock(); }
void Foo::readerlock() { mu1_.ReaderLock(); mu2_.ReaderLock(); }
void Foo::unlock() { mu1_.Unlock(); mu2_.Unlock(); }
bool Foo::trylock() { return true; }
bool Foo::readertrylock() { return true; }


void Foo::test() {
  mu1_.Lock();
  foo1();
  c = 0;
  *d = 0;
  mu1_.Unlock();

  mu1_.ReaderLock();
  foo2();
  int x = c;
  int y = *d;
  mu1_.Unlock();

  mu2_.Lock();
  foo3();
  mu2_.Unlock();

  lock();
  a = 0;
  b = 0;
  unlock();

  readerlock();
  int z = a + b;
  unlock();

  if (trylock()) {
    a = 0;
    b = 0;
    unlock();
  }

  if (readertrylock()) {
    int zz = a + b;
    unlock();
  }
}


void Foo::assertBoth() { }
void Foo::alsoAssertBoth() { }
void Foo::assertShared() { }
void Foo::alsoAssertShared() { }

void Foo::testAssert() {
  {
    assertBoth();
    a = 0;
    b = 0;
  }
  {
    alsoAssertBoth();
    a = 0;
    b = 0;
  }
}

void Foo::testAssertShared() {
  {
    assertShared();
    int zz = a + b;
  }

  {
    alsoAssertShared();
    int zz = a + b;
  }
}


}


namespace GuardedNonPrimitiveTypeTest {


class Data {
public:
  Data(int i) : dat(i) { }

  int getValue() const { return dat; }
  void setValue(int i) { dat = i; }

  int operator[](int i) const { return dat; }
  int& operator[](int i) { return dat; }

  void operator()() { }

  Data& operator+=(int);
  Data& operator-=(int);
  Data& operator*=(int);
  Data& operator/=(int);
  Data& operator%=(int);
  Data& operator^=(int);
  Data& operator&=(int);
  Data& operator|=(int);
  Data& operator<<=(int);
  Data& operator>>=(int);
  Data& operator++();
  Data& operator++(int);
  Data& operator--();
  Data& operator--(int);

private:
  int dat;
};

class DataWithAddrOf : public Data {
public:
  const Data* operator&();
  const Data* operator&() const;
};

class DataCell {
public:
  DataCell(const Data& d) : dat(d) { }

private:
  Data dat;
};


void showData(const Data* d);
void showDataCell(const DataCell& dc);


class Foo {
public:

  void test() {
    data_.setValue(0);

    int a = data_.getValue();


    datap1_->setValue(0);

    a = datap1_->getValue();


    datap2_->setValue(0);

    a = datap2_->getValue();


    (*datap2_).setValue(0);

    a = (*datap2_).getValue();


    mu_.Lock();
    data_.setValue(1);
    datap1_->setValue(1);
    datap2_->setValue(1);
    mu_.Unlock();

    mu_.ReaderLock();
    a = data_.getValue();
    datap1_->setValue(0);
    a = datap1_->getValue();
    a = datap2_->getValue();
    mu_.Unlock();
  }


  void test2() {
    data_ = Data(1);
    *datap1_ = data_;

    *datap2_ = data_;

    data_ = *datap1_;

    data_ = *datap2_;

    data_ += 1;
    data_ -= 1;
    data_ *= 1;
    data_ /= 1;
    data_ %= 1;
    data_ ^= 1;
    data_ &= 1;
    data_ |= 1;
    data_ <<= 1;
    data_ >>= 1;
    ++data_;
    data_++;
    --data_;
    data_--;

    data_[0] = 0;
    (*datap2_)[0] = 0;

    data_();


    (void)&data_ao_;
    (void)__builtin_addressof(data_ao_);
    showData(&data_ao_);
  }


  void test3() const {
    Data mydat(data_);





    int a = data_[0];

    (void)&data_ao_;
    showData(&data_ao_);
  }

private:
  Mutex mu_;
  Data data_ __attribute__((guarded_by(mu_)));
  Data* datap1_ __attribute__((guarded_by(mu_)));
  Data* datap2_ __attribute__((pt_guarded_by(mu_)));
  DataWithAddrOf data_ao_ __attribute__((guarded_by(mu_)));
};

}


namespace GuardedNonPrimitive_MemberAccess {

class Cell {
public:
  Cell(int i);

  void cellMethod();

  int a;
};


class Foo {
public:
  int a;
  Cell c __attribute__((guarded_by(cell_mu_)));
  Cell* cp __attribute__((pt_guarded_by(cell_mu_)));

  void myMethod();

  Mutex cell_mu_;
};


class Bar {
private:
  Mutex mu_;
  Foo foo __attribute__((guarded_by(mu_)));
  Foo* foop __attribute__((pt_guarded_by(mu_)));

  void test() {
    foo.myMethod();

    int fa = foo.a;
    foo.a = fa;

    fa = foop->a;
    foop->a = fa;

    fa = (*foop).a;
    (*foop).a = fa;

    foo.c = Cell(0);

    foo.c.cellMethod();


    foop->c = Cell(0);

    foop->c.cellMethod();


    (*foop).c = Cell(0);

    (*foop).c.cellMethod();

  };
};

}


namespace TestThrowExpr {

class Foo {
  Mutex mu_;

  bool hasError();

  void test() {
    mu_.Lock();
    if (hasError()) {
      throw "ugly";
    }
    mu_.Unlock();
  }
};

}


namespace UnevaluatedContextTest {



static inline Mutex* getMutex1();
static inline Mutex* getMutex2();

void bar() __attribute__((exclusive_locks_required(getMutex1())));

void bar2() __attribute__((exclusive_locks_required(getMutex1(), getMutex2())));

}


namespace LockUnlockFunctionTest {


class __attribute__((lockable)) MyLockable {
public:
  void lock() __attribute__((exclusive_lock_function())) { mu_.Lock(); }
  void readerLock() __attribute__((shared_lock_function())) { mu_.ReaderLock(); }
  void unlock() __attribute__((unlock_function())) { mu_.Unlock(); }

private:
  Mutex mu_;
};


class Foo {
public:

  void lock() __attribute__((exclusive_lock_function(mu_))) {
    mu_.Lock();
  }

  void readerLock() __attribute__((shared_lock_function(mu_))) {
    mu_.ReaderLock();
  }

  void unlock() __attribute__((unlock_function(mu_))) {
    mu_.Unlock();
  }

  void unlockExclusive() __attribute__((release_capability(mu_))) {
    mu_.Unlock();
  }

  void unlockShared() __attribute__((release_shared_capability(mu_))) {
    mu_.ReaderUnlock();
  }


  void lockBad() __attribute__((exclusive_lock_function(mu_))) {
    mu2_.Lock();
    mu2_.Unlock();
  }

  void readerLockBad() __attribute__((shared_lock_function(mu_))) {
    mu2_.Lock();
    mu2_.Unlock();
  }

  void unlockBad() __attribute__((unlock_function(mu_))) {
    mu2_.Lock();
    mu2_.Unlock();
  }


  void lockBad2() __attribute__((exclusive_lock_function(mu_))) {
    mu2_.Lock();
  }



  void readerLockBad2() __attribute__((shared_lock_function(mu_))) {
    mu2_.ReaderLock();
  }



  void unlockBad2() __attribute__((unlock_function(mu_))) {
    mu2_.Unlock();
  }

private:
  Mutex mu_;
  Mutex mu2_;
};

}


namespace AssertHeldTest {

class Foo {
public:
  int c;
  int a __attribute__((guarded_by(mu_)));
  Mutex mu_;

  void test1() {
    mu_.AssertHeld();
    int b = a;
    a = 0;
  }

  void test2() {
    mu_.AssertReaderHeld();
    int b = a;
    a = 0;
  }

  void test3() {
    if (c) {
      mu_.AssertHeld();
    }
    else {
      mu_.AssertHeld();
    }
    int b = a;
    a = 0;
  }

  void test4() __attribute__((exclusive_locks_required(mu_))) {
    mu_.AssertHeld();
    int b = a;
    a = 0;
  }

  void test5() __attribute__((unlock_function(mu_))) {
    mu_.AssertHeld();
    mu_.Unlock();
  }

  void test6() {
    mu_.AssertHeld();
    mu_.Unlock();
  }

  void test7() {
    if (c) {
      mu_.AssertHeld();
    }
    else {
      mu_.Lock();
    }
    int b = a;
    a = 0;
    mu_.Unlock();
  }

  void test8() {
    if (c) {
      mu_.Lock();
    }
    else {
      mu_.AssertHeld();
    }

    int b = a;
    a = 0;
    mu_.Unlock();
  }

  void test9() {
    if (c) {
      mu_.AssertHeld();
    }
    else {
      mu_.Lock();
    }
  }

  void test10() {
    if (c) {
      mu_.Lock();
    }
    else {
      mu_.AssertHeld();
    }
  }

  void assertMu() __attribute__((assert_exclusive_lock(mu_)));

  void test11() {
    assertMu();
    int b = a;
    a = 0;
  }

  void test12() {
    if (c)
      mu_.ReaderLock();
    else
      mu_.AssertHeld();

    int b = a;
    a = 0;
    mu_.Unlock();
  }

  void test13() {
    if (c)
      mu_.Lock();
    else
      mu_.AssertReaderHeld();

    int b = a;
    a = 0;
    mu_.Unlock();
  }
};

}


namespace LogicalConditionalTryLock {

class Foo {
public:
  Mutex mu;
  int a __attribute__((guarded_by(mu)));
  bool c;

  bool newc();

  void test1() {
    if (c && mu.TryLock()) {
      a = 0;
      mu.Unlock();
    }
  }

  void test2() {
    bool b = mu.TryLock();
    if (c && b) {
      a = 0;
      mu.Unlock();
    }
  }

  void test3() {
    if (c || !mu.TryLock())
      return;
    a = 0;
    mu.Unlock();
  }

  void test4() {
    while (c && mu.TryLock()) {
      a = 0;
      c = newc();
      mu.Unlock();
    }
  }

  void test5() {
    while (c) {
      if (newc() || !mu.TryLock())
        break;
      a = 0;
      mu.Unlock();
    }
  }

  void test6() {
    mu.Lock();
    do {
      a = 0;
      mu.Unlock();
    } while (newc() && mu.TryLock());
  }

  void test7() {
    for (bool b = mu.TryLock(); c && b;) {
      a = 0;
      mu.Unlock();
    }
  }

  void test8() {
    if (c && newc() && mu.TryLock()) {
      a = 0;
      mu.Unlock();
    }
  }

  void test9() {
    if (!(c && newc() && mu.TryLock()))
      return;
    a = 0;
    mu.Unlock();
  }

  void test10() {
    if (!(c || !mu.TryLock())) {
      a = 0;
      mu.Unlock();
    }
  }
};

}



namespace PtGuardedByTest {

void doSomething();

class Cell {
  public:
  int a;
};



class PtGuardedByCorrectnessTest {
  Mutex mu1;
  Mutex mu2;
  int* a __attribute__((guarded_by(mu1))) __attribute__((pt_guarded_by(mu2)));
  Cell* c __attribute__((guarded_by(mu1))) __attribute__((pt_guarded_by(mu2)));
  int sa[10] __attribute__((guarded_by(mu1)));
  Cell sc[10] __attribute__((guarded_by(mu1)));

  static constexpr int Cell::*pa = &Cell::a;

  void test1() {
    mu1.Lock();
    if (a == 0) doSomething();
    a = 0;
    c = 0;
    if (sa[0] == 42) doSomething();
    sa[0] = 57;
    if (sc[0].a == 42) doSomething();
    sc[0].a = 57;
    mu1.Unlock();
  }

  void test2() {
    mu1.ReaderLock();
    if (*a == 0) doSomething();
    *a = 0;

    if (c->a == 0) doSomething();
    c->a = 0;
    c->*pa = 0;

    if ((*c).a == 0) doSomething();
    (*c).a = 0;
    (*c).*pa = 0;

    if (a[0] == 42) doSomething();
    a[0] = 57;
    if (c[0].a == 42) doSomething();
    c[0].a = 57;
    mu1.Unlock();
  }

  void test3() {
    mu2.Lock();
    if (*a == 0) doSomething();
    *a = 0;

    if (c->a == 0) doSomething();
    c->a = 0;

    if ((*c).a == 0) doSomething();
    (*c).a = 0;

    if (a[0] == 42) doSomething();
    a[0] = 57;
    if (c[0].a == 42) doSomething();
    c[0].a = 57;
    mu2.Unlock();
  }

  void test4() {
    if (sa[0] == 42) doSomething();
    sa[0] = 57;
    if (sc[0].a == 42) doSomething();
    sc[0].a = 57;
    sc[0].*pa = 57;

    if (*sa == 42) doSomething();
    *sa = 57;
    if ((*sc).a == 42) doSomething();
    (*sc).a = 57;
    if (sc->a == 42) doSomething();
    sc->a = 57;
  }

  void test5() {
    mu1.ReaderLock();
    mu2.Lock();
    if (*a == 0) doSomething();
    *a = 0;

    if (c->a == 0) doSomething();
    c->a = 0;

    if ((*c).a == 0) doSomething();
    (*c).a = 0;
    mu2.Unlock();
    mu1.Unlock();
  }
};


class SmartPtr_PtGuardedBy_Test {
  Mutex mu1;
  Mutex mu2;
  SmartPtr<int> sp __attribute__((guarded_by(mu1))) __attribute__((pt_guarded_by(mu2)));
  SmartPtr<Cell> sq __attribute__((guarded_by(mu1))) __attribute__((pt_guarded_by(mu2)));

  static constexpr int Cell::*pa = &Cell::a;

  void test1() {
    mu1.ReaderLock();
    mu2.Lock();

    sp.get();
    if (*sp == 0) doSomething();
    *sp = 0;
    sq->a = 0;
    sq->*pa = 0;

    if (sp[0] == 0) doSomething();
    sp[0] = 0;

    mu2.Unlock();
    mu1.Unlock();
  }

  void test2() {
    mu2.Lock();

    sp.get();
    if (*sp == 0) doSomething();
    *sp = 0;
    sq->a = 0;
    sq->*pa = 0;

    if (sp[0] == 0) doSomething();
    sp[0] = 0;
    if (sq[0].a == 0) doSomething();
    sq[0].a = 0;

    mu2.Unlock();
  }

  void test3() {
    mu1.Lock();

    sp.get();
    if (*sp == 0) doSomething();
    *sp = 0;
    sq->a = 0;
    sq->*pa = 0;

    if (sp[0] == 0) doSomething();
    sp[0] = 0;
    if (sq[0].a == 0) doSomething();
    sq[0].a = 0;

    mu1.Unlock();
  }
};

}


namespace NonMemberCalleeICETest {

class A {
  void Run() {
  (RunHelper)();
 }

 void RunHelper() __attribute__((exclusive_locks_required(M)));
 Mutex M;
};

}


namespace pt_guard_attribute_type {
  int i __attribute__((pt_guarded_by(sls_mu)));
  int j __attribute__((pt_guarded_var));

  void test() {
    int i __attribute__((pt_guarded_by(sls_mu)));
    int j __attribute__((pt_guarded_var));

    typedef int __attribute__((pt_guarded_by(sls_mu))) bad1;
    typedef int __attribute__((pt_guarded_var)) bad2;
  }
}


namespace ThreadAttributesOnLambdas {

class Foo {
  Mutex mu_;

  void LockedFunction() __attribute__((exclusive_locks_required(mu_)));

  void test() {
    auto func1 = [this]() __attribute__((exclusive_locks_required(mu_))) {
      LockedFunction();
    };

    auto func2 = [this]() __attribute__((no_thread_safety_analysis)) {
      LockedFunction();
    };

    auto func3 = [this]() __attribute__((exclusive_lock_function(mu_))) {
      mu_.Lock();
    };

    func1();
    func1.operator()();
    func2();
    func2.operator()();
    func3();
    mu_.Unlock();
    func3.operator()();
    mu_.Unlock();
  }
};

}



namespace AttributeExpressionCornerCases {

class Foo {
  int a __attribute__((guarded_by(getMu())));

  Mutex* getMu() __attribute__((lock_returned("")));
  Mutex* getUniv() __attribute__((lock_returned("*")));

  void test1() {
    a = 0;
  }

  void test2() __attribute__((exclusive_locks_required(getUniv()))) {
    a = 0;
  }

  void foo(Mutex* mu) __attribute__((exclusive_locks_required(mu)));

  void test3() {
    foo(nullptr);
  }
};


class MapTest {
  struct MuCell { Mutex* mu; };

  MyMap<MyString, Mutex*> map;
  MyMap<MyString, MuCell> mapCell;

  int a __attribute__((guarded_by(map["foo"])));
  int b __attribute__((guarded_by(mapCell["foo"].mu)));

  void test() {
    map["foo"]->Lock();
    a = 0;
    map["foo"]->Unlock();
  }

  void test2() {
    mapCell["foo"].mu->Lock();
    b = 0;
    mapCell["foo"].mu->Unlock();
  }
};


class PreciseSmartPtr {
  SmartPtr<Mutex> mu;
  int val __attribute__((guarded_by(mu)));

  static bool compare(PreciseSmartPtr& a, PreciseSmartPtr &b) {
    a.mu->Lock();
    bool result = (a.val == b.val);

    a.mu->Unlock();
    return result;
  }
};


class SmartRedeclare {
  SmartPtr<Mutex> mu;
  int val __attribute__((guarded_by(mu)));

  void test() __attribute__((exclusive_locks_required(mu)));
  void test2() __attribute__((exclusive_locks_required(mu.get())));
  void test3() __attribute__((exclusive_locks_required(mu.get())));
};


void SmartRedeclare::test() __attribute__((exclusive_locks_required(mu.get()))) {
  val = 0;
}

void SmartRedeclare::test2() __attribute__((exclusive_locks_required(mu))) {
  val = 0;
}

void SmartRedeclare::test3() {
  val = 0;
}


namespace CustomMutex {


class __attribute__((lockable)) BaseMutex { };
class DerivedMutex : public BaseMutex { };

void customLock(const BaseMutex *m) __attribute__((exclusive_lock_function(m)));
void customUnlock(const BaseMutex *m) __attribute__((unlock_function(m)));

static struct DerivedMutex custMu;

static void doSomethingRequiringLock() __attribute__((exclusive_locks_required(custMu))) { }

void customTest() {
  customLock(reinterpret_cast<BaseMutex*>(&custMu));
  doSomethingRequiringLock();
  customUnlock(reinterpret_cast<BaseMutex*>(&custMu));
}

}

}


namespace ScopedLockReturnedInvalid {

class Opaque;

Mutex* getMutex(Opaque* o) __attribute__((lock_returned("")));

void test(Opaque* o) {
  MutexLock lock(getMutex(o));
}

}


namespace NegativeRequirements {

class Bar {
  Mutex mu;
  int a __attribute__((guarded_by(mu)));

public:
  void baz() __attribute__((exclusive_locks_required(!mu))) {
    mu.Lock();
    a = 0;
    mu.Unlock();
  }
};


class Foo {
  Mutex mu;
  int a __attribute__((guarded_by(mu)));

public:
  void foo() {
    mu.Lock();
    baz();
    bar();
    mu.Unlock();
  }

  void bar() {
    bar2();
  }

  void bar2() __attribute__((exclusive_locks_required(!mu))) {
    baz();
  }

  void baz() __attribute__((exclusive_locks_required(!mu))) {
    mu.Lock();
    a = 0;
    mu.Unlock();
  }

  void test() {
    Bar b;
    b.baz();
  }
};

}


namespace NegativeThreadRoles {

typedef int __attribute__((capability("role"))) ThreadRole;

void acquire(ThreadRole R) __attribute__((exclusive_lock_function(R))) __attribute__((no_thread_safety_analysis)) {}
void release(ThreadRole R) __attribute__((unlock_function(R))) __attribute__((no_thread_safety_analysis)) {}

ThreadRole FlightControl, Logger;

extern void enque_log_msg(const char *msg);
void log_msg(const char *msg) {
  enque_log_msg(msg);
}

void dispatch_log(const char *msg) __attribute__((requires_capability(!FlightControl))) {}
void dispatch_log2(const char *msg) __attribute__((requires_capability(Logger))) {}

void flight_control_entry(void) __attribute__((requires_capability(FlightControl))) {
  dispatch_log("wrong");
  dispatch_log2("also wrong");
}

void spawn_fake_flight_control_thread(void) {
  acquire(FlightControl);
  flight_control_entry();
  release(FlightControl);
}

extern const char *deque_log_msg(void) __attribute__((requires_capability(Logger)));
void logger_entry(void) __attribute__((requires_capability(Logger)))
                        __attribute__((requires_capability(!FlightControl))) {
  const char *msg;

  while ((msg = deque_log_msg())) {
    dispatch_log(msg);
  }
}

void spawn_fake_logger_thread(void) __attribute__((requires_capability(!FlightControl))) {
  acquire(Logger);
  logger_entry();
  release(Logger);
}

int main(void) __attribute__((requires_capability(!FlightControl))) {
  spawn_fake_flight_control_thread();
  spawn_fake_logger_thread();

  for (;;)
    ;

  return 0;
}

}


namespace AssertSharedExclusive {

void doSomething();

class Foo {
  Mutex mu;
  int a __attribute__((guarded_by(mu)));

  void test() __attribute__((shared_locks_required(mu))) {
    mu.AssertHeld();
    if (a > 0)
      doSomething();
  }
};

}


namespace RangeBasedForAndReferences {

class Foo {
  struct MyStruct {
    int a;
  };

  Mutex mu;
  int a __attribute__((guarded_by(mu)));
  MyContainer<int> cntr __attribute__((guarded_by(mu)));
  MyStruct s __attribute__((guarded_by(mu)));
  int arr[10] __attribute__((guarded_by(mu)));

  void nonref_test() {
    int b = a;
    b = 0;
  }

  void auto_test() {
    auto b = a;
    b = 0;
    auto &c = a;
    c = 0;
  }

  void ref_test() {
    int &b = a;
    int &c = b;
    int &d = c;
    b = 0;
    c = 0;
    d = 0;

    MyStruct &rs = s;
    rs.a = 0;

    int (&rarr)[10] = arr;
    rarr[2] = 0;
  }

  void ptr_test() {
    int *b = &a;
    *b = 0;
  }

  void for_test() {
    int total = 0;
    for (int i : cntr) {
      total += i;
    }
  }
};


}



namespace PassByRefTest {

class Foo {
public:
  Foo() : a(0), b(0) { }

  int a;
  int b;

  void operator+(const Foo& f);

  void operator[](const Foo& g);

  void operator()();
};

template<class T>
T&& mymove(T& f);



void copy(Foo f);
void write1(Foo& f);
void write2(int a, Foo& f);
void write3(Foo* f);
void write4(Foo** f);
void read1(const Foo& f);
void read2(int a, const Foo& f);
void read3(const Foo* f);
void read4(Foo* const* f);
void destroy(Foo&& f);

void operator/(const Foo& f, const Foo& g);
void operator*(const Foo& f, const Foo& g);


struct FooRead {
  FooRead(const Foo &);
};
struct FooWrite {
  FooWrite(Foo &);
};


template<typename... T>
void copyVariadic(T...) {}
template<typename... T>
void writeVariadic(T&...) {}
template<typename... T>
void readVariadic(const T&...) {}

void copyVariadicC(int, ...);

class Bar {
public:
  Mutex mu;
  Foo foo __attribute__((guarded_by(mu)));
  Foo foo2 __attribute__((guarded_by(mu)));
  Foo* foop __attribute__((pt_guarded_by(mu)));
  Foo* foop2 __attribute__((guarded_by(mu)));
  SmartPtr<Foo> foosp __attribute__((pt_guarded_by(mu)));


  void mwrite1(Foo& f);
  void mwrite2(int a, Foo& f);
  void mwrite3(Foo* f);
  void mread1(const Foo& f);
  void mread2(int a, const Foo& f);
  void mread3(const Foo* f);


  static void smwrite1(Foo& f);
  static void smwrite2(int a, Foo& f);
  static void smwrite3(Foo* f);
  static void smread1(const Foo& f);
  static void smread2(int a, const Foo& f);
  static void smread3(const Foo* f);

  void operator<<(const Foo& f);

  void test1() {
    copy(foo);
    write1(foo);
    write2(10, foo);
    read1(foo);
    read2(10, foo);
    destroy(mymove(foo));

    copyVariadic(foo);
    readVariadic(foo);
    writeVariadic(foo);
    copyVariadicC(1, foo);

    FooRead reader(foo);
    FooWrite writer(foo);

    mwrite1(foo);
    mwrite2(10, foo);
    mread1(foo);
    mread2(10, foo);

    smwrite1(foo);
    smwrite2(10, foo);
    smread1(foo);
    smread2(10, foo);

    foo + foo2;

    foo / foo2;

    foo * foo2;

    foo[foo2];

    foo();
    (*this) << foo;

    copy(*foop);
    write1(*foop);
    write2(10, *foop);
    read1(*foop);
    read2(10, *foop);
    destroy(mymove(*foop));

    copy(*foosp);
    write1(*foosp);
    write2(10, *foosp);
    read1(*foosp);
    read2(10, *foosp);
    destroy(mymove(*foosp));


    copy(*foosp.get());
    write1(*foosp.get());
    write2(10, *foosp.get());
    read1(*foosp.get());
    read2(10, *foosp.get());
    destroy(mymove(*foosp.get()));
  }

  void test_pass_pointer() {
    (void)&foo;
    (void)foop;

    write3(&foo);
    write3(foop);
    write3(&*foop);
    write4((Foo **)&foo);
    write4(&foop);
    read3(&foo);
    read3(foop);
    read3(&*foop);
    read4((Foo **)&foo);
    read4(&foop);
    mwrite3(&foo);
    mwrite3(foop);
    mwrite3(&*foop);
    mread3(&foo);
    mread3(foop);
    mread3(&*foop);
    smwrite3(&foo);
    smwrite3(foop);
    smwrite3(&*foop);
    smread3(&foo);
    smread3(foop);
    smread3(&*foop);

    write3(foop2);
    write3(&*foop2);
    write4(&foop2);
    read3(foop2);
    read3(&*foop2);
    read4(&foop2);
    mwrite3(foop2);
    mwrite3(&*foop2);
    mread3(foop2);
    mread3(&*foop2);
    smwrite3(foop2);
    smwrite3(&*foop2);
    smread3(foop2);
    smread3(&*foop2);

    mu.Lock();
    write3(&foo);
    write3(foop);
    write3(&*foop);
    write3(foop2);
    write4(&foop2);
    read3(&foo);
    read3(foop);
    read3(&*foop);
    read3(foop2);
    read4(&foop2);
    mwrite3(&foo);
    mwrite3(foop);
    mwrite3(&*foop);
    mwrite3(foop2);
    mread3(&foo);
    mread3(foop);
    mread3(&*foop);
    mread3(foop2);
    smwrite3(&foo);
    smwrite3(foop);
    smwrite3(&*foop);
    smwrite3(foop2);
    smread3(&foo);
    smread3(foop);
    smread3(&*foop);
    smread3(foop2);
    mu.Unlock();

    mu.ReaderLock();
    write3(&foo);
    write3(foop);
    write3(&*foop);
    write3(foop2);
    write4(&foop2);
    read3(&foo);
    read3(foop);
    read3(&*foop);
    read3(foop2);
    read4(&foop2);
    mwrite3(&foo);
    mwrite3(foop);
    mwrite3(&*foop);
    mwrite3(foop2);
    mread3(&foo);
    mread3(foop);
    mread3(&*foop);
    mread3(foop2);
    smwrite3(&foo);
    smwrite3(foop);
    smwrite3(&*foop);
    smwrite3(foop2);
    smread3(&foo);
    smread3(foop);
    smread3(&*foop);
    smread3(foop2);
    mu.ReaderUnlock();
  }
};

class Return {
  Mutex mu;
  Foo foo __attribute__((guarded_by(mu)));
  Foo* foo_ptr __attribute__((pt_guarded_by(mu)));
  Foo foo_depr __attribute__((guarded_var));
  Foo* foo_ptr_depr __attribute__((pt_guarded_var));

  Foo returns_value_locked() {
    MutexLock lock(&mu);
    return foo;
  }

  Foo returns_value_locks_required() __attribute__((exclusive_locks_required(mu))) {
    return foo;
  }

  Foo returns_value_releases_lock_after_return() __attribute__((unlock_function(mu))) {
    MutexLock lock(&mu, true);
    return foo;
  }

  Foo returns_value_aquires_lock() __attribute__((exclusive_lock_function(mu))) {
    mu.Lock();
    return foo;
  }

  Foo returns_value_not_locked() {
    return foo;
  }

  Foo returns_value_releases_lock_before_return() __attribute__((unlock_function(mu))) {
    mu.Unlock();
    return foo;
  }

  Foo &returns_ref_not_locked() {
    return foo;
  }

  Foo &returns_ref_locked() {
    MutexLock lock(&mu);
    return foo;
  }

  Foo &returns_ref_shared_locks_required() __attribute__((shared_locks_required(mu))) {
    return foo;
  }

  Foo &returns_ref_exclusive_locks_required() __attribute__((exclusive_locks_required(mu))) {
    return foo;
  }

  Foo &returns_ref_releases_lock_after_return() __attribute__((unlock_function(mu))) {
    MutexLock lock(&mu, true);
    return foo;
  }

  Foo& returns_ref_releases_lock_before_return() __attribute__((unlock_function(mu))) {
    mu.Unlock();
    return foo;
  }

  Foo &returns_ref_aquires_lock() __attribute__((exclusive_lock_function(mu))) {
    mu.Lock();
    return foo;
  }

  const Foo &returns_constref_shared_locks_required() __attribute__((shared_locks_required(mu))) {
    return foo;
  }

  Foo *returns_ptr_exclusive_locks_required() __attribute__((exclusive_locks_required(mu))) {
    return &foo;
  }

  Foo *returns_pt_ptr_exclusive_locks_required() __attribute__((exclusive_locks_required(mu))) {
    return foo_ptr;
  }

  Foo *returns_ptr_shared_locks_required() __attribute__((shared_locks_required(mu))) {
    return &foo;
  }

  Foo *returns_pt_ptr_shared_locks_required() __attribute__((shared_locks_required(mu))) {
    return foo_ptr;
  }

  const Foo *returns_constptr_shared_locks_required() __attribute__((shared_locks_required(mu))) {
    return &foo;
  }

  const Foo *returns_pt_constptr_shared_locks_required() __attribute__((shared_locks_required(mu))) {
    return foo_ptr;
  }

  Foo *returns_ptr() {
    return &foo;
  }

  Foo *returns_pt_ptr() {
    return foo_ptr;
  }

  Foo &returns_ref2() {
    return *foo_ptr;
  }

  Foo *returns_ptr_deprecated() {
    return &foo_depr;
  }

  Foo *returns_pt_ptr_deprecated() {
    return foo_ptr_depr;
  }

  Foo &returns_ref_deprecated() {
    return *foo_ptr_depr;
  }


  Foo *returns_ptr_alias() {
    mu.Lock();
    Foo *ret = &foo;
    mu.Unlock();
    return ret;
  }

  Foo *returns_pt_ptr_alias() {
    mu.Lock();
    Foo *ret = foo_ptr;
    mu.Unlock();
    return ret;
  }

  Foo &returns_ref2_alias() {
    mu.Lock();
    Foo *ret = foo_ptr;
    mu.Unlock();
    return *ret;
  }
};


}


namespace AcquiredBeforeAfterText {

class Foo {
  Mutex mu1 __attribute__((acquired_before(mu2, mu3)));
  Mutex mu2;
  Mutex mu3;

  void test1() {
    mu1.Lock();
    mu2.Lock();
    mu3.Lock();

    mu3.Unlock();
    mu2.Unlock();
    mu1.Unlock();
  }

  void test2() {
    mu2.Lock();
    mu1.Lock();
    mu1.Unlock();
    mu2.Unlock();
  }

  void test3() {
    mu3.Lock();
    mu1.Lock();
    mu1.Unlock();
    mu3.Unlock();
  }

  void test4() __attribute__((exclusive_locks_required(mu1))) {
    mu2.Lock();
    mu2.Unlock();
  }

  void test5() __attribute__((exclusive_locks_required(mu2))) {
    mu1.Lock();
    mu1.Unlock();
  }

  void test6() __attribute__((exclusive_locks_required(mu2))) {
    mu1.AssertHeld();
  }

  void test7() __attribute__((exclusive_locks_required(mu1, mu2, mu3))) { }

  void test8() __attribute__((exclusive_locks_required(mu3, mu2, mu1))) { }
};


class Foo2 {
  Mutex mu1;
  Mutex mu2 __attribute__((acquired_after(mu1)));
  Mutex mu3 __attribute__((acquired_after(mu1)));

  void test1() {
    mu1.Lock();
    mu2.Lock();
    mu3.Lock();

    mu3.Unlock();
    mu2.Unlock();
    mu1.Unlock();
  }

  void test2() {
    mu2.Lock();
    mu1.Lock();
    mu1.Unlock();
    mu2.Unlock();
  }

  void test3() {
    mu3.Lock();
    mu1.Lock();
    mu1.Unlock();
    mu3.Unlock();
  }
};


class Foo3 {
  Mutex mu1 __attribute__((acquired_before(mu2)));
  Mutex mu2;
  Mutex mu3 __attribute__((acquired_after(mu2))) __attribute__((acquired_before(mu4)));
  Mutex mu4;

  void test1() {
    mu1.Lock();
    mu2.Lock();
    mu3.Lock();
    mu4.Lock();

    mu4.Unlock();
    mu3.Unlock();
    mu2.Unlock();
    mu1.Unlock();
  }

  void test2() {
    mu4.Lock();
    mu2.Lock();

    mu2.Unlock();
    mu4.Unlock();
  }

  void test3() {
    mu4.Lock();
    mu1.Lock();

    mu1.Unlock();
    mu4.Unlock();
  }

  void test4() {
    mu3.Lock();
    mu1.Lock();

    mu1.Unlock();
    mu3.Unlock();
  }
};



class Foo4 {
  Mutex mu1;
  Mutex mu2 __attribute__((acquired_after(mu1)));
  Mutex mu3 __attribute__((acquired_after(mu1)));
  Mutex mu4 __attribute__((acquired_after(mu2, mu3)));
  Mutex mu5 __attribute__((acquired_after(mu4)));
  Mutex mu6 __attribute__((acquired_after(mu4)));
  Mutex mu7 __attribute__((acquired_after(mu5, mu6)));
  Mutex mu8 __attribute__((acquired_after(mu7)));

  void test() {
    mu8.Lock();
    mu1.Lock();
    mu1.Unlock();
    mu8.Unlock();
  }
};



class Foo5 {
  Mutex mu1 __attribute__((acquired_before(mu2, mu3)));
  Mutex mu2 __attribute__((acquired_before(mu4)));
  Mutex mu3 __attribute__((acquired_before(mu4)));
  Mutex mu4 __attribute__((acquired_before(mu5, mu6)));
  Mutex mu5 __attribute__((acquired_before(mu7)));
  Mutex mu6 __attribute__((acquired_before(mu7)));
  Mutex mu7 __attribute__((acquired_before(mu8)));
  Mutex mu8;

  void test() {
    mu8.Lock();
    mu1.Lock();
    mu1.Unlock();
    mu8.Unlock();
  }
};


class Foo6 {
  Mutex mu1 __attribute__((acquired_after(mu3)));
  Mutex mu2 __attribute__((acquired_after(mu1)));
  Mutex mu3 __attribute__((acquired_after(mu2)));

  Mutex mu_b __attribute__((acquired_before(mu_b)));
  Mutex mu_a __attribute__((acquired_after(mu_a)));

  void test0() {
    mu_a.Lock();
    mu_b.Lock();
    mu_b.Unlock();
    mu_a.Unlock();
  }

  void test1a() {
    mu1.Lock();
    mu1.Unlock();
  }

  void test1b() {
    mu1.Lock();
    mu_a.Lock();
    mu_b.Lock();
    mu_b.Unlock();
    mu_a.Unlock();
    mu1.Unlock();
  }

  void test() {
    mu2.Lock();
    mu2.Unlock();
  }

  void test3() {
    mu3.Lock();
    mu3.Unlock();
  }
};

}


namespace ScopedAdoptTest {

class Foo {
  Mutex mu;
  int a __attribute__((guarded_by(mu)));
  int b;

  void test1() __attribute__((release_capability(mu))) {
    MutexLock slock(&mu, true);
    a = 0;
  }

  void test2() __attribute__((release_shared_capability(mu))) {
    ReaderMutexLock slock(&mu, true);
    b = a;
  }

  void test3() __attribute__((exclusive_locks_required(mu))) {
    MutexLock slock(&mu, true);
    a = 0;
  }

  void test4() __attribute__((shared_locks_required(mu))) {
    ReaderMutexLock slock(&mu, true);
    b = a;
  }

};

}


namespace TestReferenceNoThreadSafetyAnalysis {





template <class T>
inline const T& ts_unchecked_read(const T& v) __attribute__((no_thread_safety_analysis)) {
  return v;
}

template <class T>
inline T& ts_unchecked_read(T& v) __attribute__((no_thread_safety_analysis)) {
  return v;
}


class Foo {
public:
  Foo(): a(0) { }

  int a;
};


class Bar {
public:
  Bar() : a(0) { }

  Mutex mu;
  int a __attribute__((guarded_by(mu)));
  Foo foo __attribute__((guarded_by(mu)));
};


void test() {
  Bar bar;
  const Bar cbar;

  int a = ts_unchecked_read(bar.a);
  ts_unchecked_read(bar.a) = 1;

  int b = ts_unchecked_read(bar.foo).a;
  ts_unchecked_read(bar.foo).a = 1;

  int c = ts_unchecked_read(cbar.a);
}



}


namespace GlobalAcquiredBeforeAfterTest {

Mutex mu1;
Mutex mu2 __attribute__((acquired_after(mu1)));

void test3() {
  mu2.Lock();
  mu1.Lock();
  mu1.Unlock();
  mu2.Unlock();
}

}


namespace LifetimeExtensionText {

struct Holder {
  virtual ~Holder() throw() {}
  int i = 0;
};

void test() {

  const auto &value = Holder().i;
}

}


namespace LockableUnions {

union __attribute__((lockable)) MutexUnion {
  int a;
  char* b;

  void Lock() __attribute__((exclusive_lock_function()));
  void Unlock() __attribute__((unlock_function()));
};

MutexUnion muun2;
MutexUnion muun1 __attribute__((acquired_before(muun2)));

void test() {
  muun2.Lock();
  muun1.Lock();
  muun1.Unlock();
  muun2.Unlock();
}

}


class acquired_before_empty_str {
  void WaitUntilSpaceAvailable() {
    lock_.ReaderLock();
  }
  Mutex lock_ __attribute__((acquired_before("")));
};

namespace PR34800 {
struct A {
  operator int() const;
};
struct B {
  bool g() __attribute__((locks_excluded(h)));
  int h;
};
struct C {
  B *operator[](int);
};
C c;
void f() { c[A()]->g(); }
}
# 6812 "SemaCXX/warn-thread-safety-analysis.cpp"
namespace PR38640 {
void f() {


  int &i = i;
}
}

namespace Derived_Smart_Pointer {
template <class T>
class SmartPtr_Derived : public SmartPtr<T> {};

class Foo {
public:
  SmartPtr_Derived<Mutex> mu_;
  int a __attribute__((guarded_by(mu_)));
  int b __attribute__((guarded_by(mu_.get())));
  int c __attribute__((guarded_by(*mu_)));

  void Lock() __attribute__((exclusive_lock_function(mu_)));
  void Unlock() __attribute__((unlock_function(mu_)));

  void test0() {
    a = 1;
    b = 1;
    c = 1;
  }

  void test1() {
    Lock();
    a = 1;
    b = 1;
    c = 1;
    Unlock();
  }
};

class Bar {
  SmartPtr_Derived<Foo> foo;

  void test0() {
    foo->a = 1;
    (*foo).b = 1;
    foo.get()->c = 1;
  }

  void test1() {
    foo->Lock();
    foo->a = 1;
    foo->Unlock();

    foo->mu_->Lock();
    foo->b = 1;
    foo->mu_->Unlock();

    MutexLock lock(foo->mu_.get());
    foo->c = 1;
  }
};

class PointerGuard {
  Mutex mu1;
  Mutex mu2;
  SmartPtr_Derived<int> i __attribute__((guarded_by(mu1))) __attribute__((pt_guarded_by(mu2)));

  void test0() {
    i.get();
    *i = 2;


  }

  void test1() {
    mu1.Lock();

    i.get();
    *i = 2;

    mu1.Unlock();
  }

  void test2() {
    mu2.Lock();

    i.get();
    *i = 2;

    mu2.Unlock();
  }

  void test3() {
    mu1.Lock();
    mu2.Lock();

    i.get();
    *i = 2;

    mu2.Unlock();
    mu1.Unlock();
  }
};
}


namespace FunctionStaticVariable {
struct Data {
  Mutex mu;
  int x __attribute__((guarded_by(mu)));
};

void testStaticVariable() {
}

void testHeapAllocation() {
  static Data *d = new Data;
  d->mu.Lock();
  d->x = 5;
  d->mu.Unlock();
}

void testHeapAllocationBug() {
  static auto *d = new Data;
  d->x = 10;
}

void testHeapAllocationScopedLock() {
  static Mutex *mu = new Mutex;
  MutexLock lock(mu);
}
}

namespace Reentrancy {

class __attribute__((lockable)) __attribute__((reentrant_capability)) ReentrantMutex {
 public:
  void Lock() __attribute__((exclusive_lock_function()));
  void ReaderLock() __attribute__((shared_lock_function()));
  void Unlock() __attribute__((unlock_function()));
  void ExclusiveUnlock() __attribute__((release_capability()));
  void ReaderUnlock() __attribute__((release_shared_capability()));
  bool TryLock() __attribute__((exclusive_trylock_function(true)));
  bool ReaderTryLock() __attribute__((shared_trylock_function(true)));


  const ReentrantMutex& operator!() const { return *this; }

  void AssertHeld() __attribute__((assert_exclusive_lock()));
  void AssertReaderHeld() __attribute__((assert_shared_lock()));
};

class __attribute__((scoped_lockable)) ReentrantMutexLock {
 public:
  ReentrantMutexLock(ReentrantMutex *mu) __attribute__((exclusive_lock_function(mu)));
  ~ReentrantMutexLock() __attribute__((unlock_function()));
};

class __attribute__((scoped_lockable)) ReentrantReaderMutexLock {
 public:
  ReentrantReaderMutexLock(ReentrantMutex *mu) __attribute__((shared_lock_function(mu)));
  ~ReentrantReaderMutexLock() __attribute__((unlock_function()));
};

class __attribute__((scoped_lockable)) RelockableReentrantMutexLock {
public:
  RelockableReentrantMutexLock(ReentrantMutex *mu) __attribute__((exclusive_lock_function(mu)));
  ~RelockableReentrantMutexLock() __attribute__((release_capability()));

  void Lock() __attribute__((exclusive_lock_function()));
  void Unlock() __attribute__((unlock_function()));
};

ReentrantMutex rmu;
int guard_var __attribute__((guarded_var)) = 0;
int guardby_var __attribute__((guarded_by(rmu))) = 0;

void testReentrantMany() {
  rmu.Lock();
  rmu.Lock();
  rmu.Lock();
  rmu.Lock();
  rmu.Lock();
  rmu.Lock();
  rmu.Lock();
  rmu.Lock();
  rmu.Unlock();
  rmu.Unlock();
  rmu.Unlock();
  rmu.Unlock();
  rmu.Unlock();
  rmu.Unlock();
  rmu.Unlock();
  rmu.Unlock();
}

void testReentrantManyReader() {
  rmu.ReaderLock();
  rmu.ReaderLock();
  rmu.ReaderLock();
  rmu.ReaderLock();
  rmu.ReaderLock();
  rmu.ReaderLock();
  rmu.ReaderLock();
  rmu.ReaderLock();
  rmu.ReaderUnlock();
  rmu.ReaderUnlock();
  rmu.ReaderUnlock();
  rmu.ReaderUnlock();
  rmu.ReaderUnlock();
  rmu.ReaderUnlock();
  rmu.ReaderUnlock();
  rmu.ReaderUnlock();
}

void testReentrantLock1() {
  rmu.Lock();
  guard_var = 2;
  rmu.Lock();
  guard_var = 2;
  rmu.Unlock();
  guard_var = 2;
  rmu.Unlock();
  guard_var = 2;
}

void testReentrantReaderLock1() {
  rmu.ReaderLock();
  int x = guard_var;
  rmu.ReaderLock();
  int y = guard_var;
  rmu.ReaderUnlock();
  int z = guard_var;
  rmu.ReaderUnlock();
  int a = guard_var;
}

void testReentrantLock2() {
  rmu.Lock();
  guardby_var = 2;
  rmu.Lock();
  guardby_var = 2;
  rmu.Unlock();
  guardby_var = 2;
  rmu.Unlock();
  guardby_var = 2;
}

void testReentrantReaderLock2() {
  rmu.ReaderLock();
  int x = guardby_var;
  rmu.ReaderLock();
  int y = guardby_var;
  rmu.ReaderUnlock();
  int z = guardby_var;
  rmu.ReaderUnlock();
  int a = guardby_var;
}

void testReentrantTryLock1() {
  if (rmu.TryLock()) {
    guardby_var = 1;
    if (rmu.TryLock()) {
      guardby_var = 1;
      rmu.Unlock();
    }
    guardby_var = 1;
    rmu.Unlock();
  }
  guardby_var = 1;
}

void testReentrantTryLock2() {
  rmu.Lock();
  guardby_var = 1;
  if (rmu.TryLock()) {
    guardby_var = 1;
    rmu.Unlock();
  }
  guardby_var = 1;
  rmu.Unlock();
  guardby_var = 1;
}

void testReentrantNotHeld() {
  rmu.Unlock();

}

void testReentrantMissingUnlock() {
  rmu.Lock();
  rmu.Lock();
  rmu.Unlock();
}



void testMixedSharedExclusive() {
  rmu.ReaderLock();
  rmu.Lock();
  rmu.Unlock();
  rmu.ReaderUnlock();
}

void testReentrantIntersection() {
  rmu.Lock();
  if (guardby_var) {
    rmu.Lock();
    rmu.Lock();
    rmu.Unlock();
  } else {
    rmu.Lock();
    guardby_var = 1;
  }
  guardby_var = 1;
  rmu.Unlock();
  guardby_var = 1;
  rmu.Unlock();
  guardby_var = 1;
}

void testReentrantIntersectionBad() {
  rmu.Lock();
  if (guardby_var) {
    rmu.Lock();
    rmu.Lock();
  } else {
    rmu.Lock();
    guardby_var = 1;
  }
  guardby_var = 1;
  rmu.Unlock();
  guardby_var = 1;
  rmu.Unlock();
  guardby_var = 1;
  rmu.Unlock();
}

void testReentrantLoopBad() {
  rmu.Lock();
  while (guardby_var) {
    rmu.Lock();
  }
  rmu.Unlock();
}

void testLocksRequiredReentrant() __attribute__((exclusive_locks_required(rmu))) {
  guardby_var = 1;
  rmu.Lock();
  rmu.Lock();
  guardby_var = 1;
  rmu.Unlock();
  rmu.Unlock();
  guardby_var = 1;
}

void testAssertReentrant() {
  rmu.AssertHeld();
  guardby_var = 1;
  rmu.Lock();
  guardby_var = 1;
  rmu.Unlock();
  guardby_var = 1;
}

void testAssertReaderReentrant() {
  rmu.AssertReaderHeld();
  int x = guardby_var;
  rmu.ReaderLock();
  int y = guardby_var;
  rmu.ReaderUnlock();
  int z = guardby_var;
}

struct TestScopedReentrantLockable {
  ReentrantMutex mu1;
  ReentrantMutex mu2;
  int a __attribute__((guarded_by(mu1)));
  int b __attribute__((guarded_by(mu2)));

  bool getBool();

  void foo1() {
    ReentrantMutexLock mulock1(&mu1);
    a = 5;
    ReentrantMutexLock mulock2(&mu1);
    a = 5;
  }

  void foo2() {
    ReentrantMutexLock mulock1(&mu1);
    a = 5;
    mu1.Lock();
    a = 5;
    mu1.Unlock();
    a = 5;
  }
# 7217 "SemaCXX/warn-thread-safety-analysis.cpp"
  void temporary() {
    ReentrantMutexLock{&mu1}, a = 1, ReentrantMutexLock{&mu1}, a = 5;
  }

  void lifetime_extension() {
    const ReentrantMutexLock &mulock1 = ReentrantMutexLock(&mu1);
    a = 5;
    const ReentrantMutexLock &mulock2 = ReentrantMutexLock(&mu1);
    a = 5;
  }

  void foo3() {
    ReentrantReaderMutexLock mulock1(&mu1);
    if (getBool()) {
      ReentrantMutexLock mulock2a(&mu2);
      b = a + 1;
    }
    else {
      ReentrantMutexLock mulock2b(&mu2);
      b = a + 2;
    }
  }

  void foo4() {
    ReentrantMutexLock mulock_a(&mu1);
    ReentrantMutexLock mulock_b(&mu1);
  }

  void temporary_double_lock() {
    ReentrantMutexLock mulock_a(&mu1);
    ReentrantMutexLock{&mu1};
  }

  void foo5() {
    ReentrantMutexLock mulock1(&mu1), mulock2(&mu2);
    {
      ReentrantMutexLock mulock3(&mu1), mulock4(&mu2);
      a = b+1;
    }
    b = a+1;
  }
};

void scopedDoubleUnlock() {
  RelockableReentrantMutexLock scope(&rmu);
  scope.Unlock();
  scope.Unlock();
}

void scopedDoubleLock1() {
  RelockableReentrantMutexLock scope(&rmu);
  scope.Lock();
  scope.Unlock();
}

void scopedDoubleLock2() {
  RelockableReentrantMutexLock scope(&rmu);
  scope.Unlock();
  scope.Lock();
  scope.Lock();
  scope.Unlock();
}

typedef int __attribute__((capability("bitlock"))) __attribute__((reentrant_capability)) *bitlock_t;
void bit_lock(bitlock_t l) __attribute__((exclusive_lock_function(l)));
void bit_unlock(bitlock_t l) __attribute__((unlock_function(l)));
bitlock_t bl;
void testReentrantTypedef() {
  bit_lock(bl);
  bit_lock(bl);
  bit_unlock(bl);
  bit_unlock(bl);
}

class TestNegativeWithReentrantMutex {
  ReentrantMutex rmu;
  int a __attribute__((guarded_by(rmu)));

public:
  void baz() __attribute__((exclusive_locks_required(!rmu))) {
    rmu.Lock();
    rmu.Lock();
    a = 0;
    rmu.Unlock();
    rmu.Unlock();
  }
};

}


namespace CapabilityAliases {
struct Foo {
  Mutex mu;
  int data __attribute__((guarded_by(mu)));
};

Foo *returnsFoo();
Foo *returnsFoo(Foo *foo);
void locksRequired(Foo *foo) __attribute__((exclusive_locks_required(foo->mu)));
void escapeAlias(int a, Foo *&ptr);
void escapeAlias(int b, Foo **ptr);
void passByConstRef(Foo* const& ptr);

void testBasicPointerAlias(Foo *f) {
  Foo *ptr = f;
  ptr->mu.Lock();
  f->data = 42;
  ptr->mu.Unlock();
}

void testCastPointerAlias(Foo *f) {

  void *priv = (void *)(long unsigned int)(&f->mu);
  f->mu.Lock();
  f->data = 42;
  auto *mu = (Mutex *)priv;
  mu->Unlock();
}

void testBasicPointerAliasNoInit(Foo *f) {
  Foo *ptr;

  ptr = nullptr;
  ptr = f;
  ptr->mu.Lock();
  f->data = 42;
  ptr->mu.Unlock();
  ptr = nullptr;
}

void testBasicPointerAliasLoop() {
  for (;;) {
    Foo *f = returnsFoo();
    Foo *ptr = f;
    if (!ptr)
      break;
    ptr->mu.Lock();
    f->data = 42;
    ptr->mu.Unlock();
  }
}

void testPointerAliasNoEscape1(Foo *f) {
  Foo *ptr = f;
  testBasicPointerAlias(ptr);

  ptr->mu.Lock();
  f->data = 42;
  ptr->mu.Unlock();
}

void testPointerAliasNoEscape2(Foo *f) {
  Foo *ptr = f;
  passByConstRef(ptr);

  ptr->mu.Lock();
  f->data = 42;
  ptr->mu.Unlock();
}

void testPointerAliasNoEscape3() {
  Foo *ptr = returnsFoo();
  ptr->mu.Lock();
  locksRequired(ptr);
  ptr->mu.Unlock();
}

void testPointerAliasEscape1(Foo *f) {
  Foo *ptr = f;
  escapeAlias(0, ptr);

  ptr->mu.Lock();
  f->data = 42;

  ptr->mu.Unlock();
}

void testPointerAliasEscape2(Foo *f) {
  Foo *ptr = f;
  escapeAlias(0, &ptr);

  ptr->mu.Lock();
  f->data = 42;

  ptr->mu.Unlock();
}

void testPointerAliasEscape3(Foo *f) {
  Foo *ptr;

  ptr = f;
  escapeAlias(0, &ptr);

  ptr->mu.Lock();
  f->data = 42;

  ptr->mu.Unlock();
}

void testPointerAliasEscapeAndReset(Foo *f) {
  Foo *ptr;

  ptr = f;
  escapeAlias(0, &ptr);
  ptr = f;

  ptr->mu.Lock();
  f->data = 42;
  ptr->mu.Unlock();
}

void testPointerAliasTryLock1() {
  Foo *ptr = returnsFoo();
  if (ptr->mu.TryLock()) {
    locksRequired(ptr);
    ptr->mu.Unlock();
  }
}

void testPointerAliasTryLock2() {
  Foo *ptr;
  ptr = returnsFoo();
  Foo *ptr2 = ptr;
  if (ptr->mu.TryLock()) {
    locksRequired(ptr);
    ptr2->mu.Unlock();
  }
}
# 7465 "SemaCXX/warn-thread-safety-analysis.cpp"
void testPointerAliasTryLockDubious(int x) {
  Foo *ptr = returnsFoo();
  if (!ptr->mu.TryLock()) {
    if (x)
      ptr = returnsFoo(ptr);
    ptr->mu.Lock();
  }
  ptr->data = 42;


  ptr->mu.Unlock();
}

void testReassignment() {
  Foo f1, f2;
  Foo *ptr = &f1;
  ptr->mu.Lock();
  f1.data = 42;
  ptr->mu.Unlock();

  ptr = &f2;
  ptr->mu.Lock();
  f2.data = 42;
  f1.data = 42;

  ptr->mu.Unlock();
}


struct Container {
  Foo foo;
};

void testNestedAccess(Container *c) {
  Foo *ptr = &c->foo;
  ptr->mu.Lock();
  c->foo.data = 42;
  ptr->mu.Unlock();
}

void testNestedAcquire(Container *c) __attribute__((exclusive_lock_function(&c->foo.mu))) {
  Foo *buf = &c->foo;
  buf->mu.Lock();
}

struct ContainerOfPtr {
  Foo *foo_ptr;
  ContainerOfPtr *next;
};

void testIndirectAccess(ContainerOfPtr *fc) {
  Foo *ptr = fc->foo_ptr;
  ptr->mu.Lock();
  fc->foo_ptr->data = 42;
  ptr->mu.Unlock();
}

void testAliasChainUnrelatedReassignment1(ContainerOfPtr *list) {
  Foo *eb = list->foo_ptr;
  eb->mu.Lock();
  list = list->next;
  eb->data = 42;
  eb->mu.Unlock();
}

void testAliasChainUnrelatedReassignment2(ContainerOfPtr *list) {
  ContainerOfPtr *busyp = list;
  Foo *eb = busyp->foo_ptr;
  eb->mu.Lock();
  busyp = busyp->next;
  eb->data = 42;
  eb->mu.Unlock();
}

void testControlFlowDoWhile(Foo *f, int x) {
  Foo *ptr = f;

  f->mu.Lock();
  if (x) {

    do { } while (x--);
  }
  ptr->data = 42;
  ptr->mu.Unlock();
}


void testComplexControlFlow(Foo *f1, Foo *f2, bool cond) {
  Foo *ptr;
  if (cond) {
    ptr = f1;
  } else {
    ptr = f2;
  }
  ptr->mu.Lock();
  if (cond) {
    f1->data = 42;

  } else {
    f2->data = 42;

  }
  ptr->mu.Unlock();
}

void testLockFunction(Foo *f) __attribute__((exclusive_lock_function(&f->mu))) {
  Mutex *mu = &f->mu;
  mu->Lock();
}

void testUnlockFunction(Foo *f) __attribute__((unlock_function(&f->mu))) {
  Mutex *mu = &f->mu;
  mu->Unlock();
}




void lockWithinStatementExpr() {
  Foo *f = ({ auto x = returnsFoo(); x->mu.Lock(); x; });
  f->data = 42;
  f->mu.Unlock();
}



void testSelfInit() {
  Mutex *mu = mu;
  mu->Lock();
  mu->Unlock();
}

void testSelfAssign() {
  Foo *f = returnsFoo();
  f = f;
  f->mu.Lock();
  f->data = 42;
  f->mu.Unlock();
}

void testRecursiveAssign() {
  Foo *f = returnsFoo();
  f = returnsFoo(f);
  f->mu.Lock();
  f->data = 42;
  f->mu.Unlock();
}

void testNew(Mutex *&out, int &x) {
  Mutex *mu = new Mutex;
  __atomic_store_n(&out, mu, 3);
  mu->Lock();
  x = 42;
  mu->Unlock();
}

void testNestedLoopInvariant(Container *c, int n) {
  Foo *ptr = &c->foo;
  ptr->mu.Lock();

  for (int i = 0; i < n; ++i) {
    for (int j = 0; j < n; ++j) {
    }
  }

  c->foo.data = 42;
  ptr->mu.Unlock();
}

void testLoopWithBreak(Foo *f, bool cond) {
  Foo *ptr = f;
  ptr->mu.Lock();
  for (int i = 0; i < 10; ++i) {
    if (cond) {
      break;
    }
  }
  f->data = 42;
  ptr->mu.Unlock();
}

void testLoopWithContinue(Foo *f, bool cond) {
  Foo *ptr = f;
  ptr->mu.Lock();
  for (int i = 0; i < 10; ++i) {
    if (cond) {
      continue;
    }
  }
  f->data = 42;
  ptr->mu.Unlock();
}

void testLoopConditionalReassignment(Foo *f1, Foo *f2, bool cond) {
  Foo *ptr = f1;
  ptr->mu.Lock();

  for (int i = 0; i < 10; ++i) {
    if (cond) {
      ptr = f2;
    }
  }
  f1->data = 42;
  ptr->mu.Unlock();
}
}
