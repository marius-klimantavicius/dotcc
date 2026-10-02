//type: fp
//options:  --c++11 --exceptions -DUSE_CAPABILITY=0: --c++11 --exceptions -DUSE_CAPABILITY=1: --c++11
# 1 "SemaCXX/warn-thread-safety-negative.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-thread-safety-negative.cpp" 2






# 1 "SemaCXX/thread-safety-annotations.h" 1
# 8 "SemaCXX/warn-thread-safety-negative.cpp" 2

class __attribute__((lockable)) Mutex {
 public:
  void Lock() __attribute__((exclusive_lock_function()));
  void ReaderLock() __attribute__((shared_lock_function()));
  void Unlock() __attribute__((unlock_function()));
  bool TryLock() __attribute__((exclusive_trylock_function(true)));
  bool ReaderTryLock() __attribute__((shared_trylock_function(true)));


  const Mutex& operator!() const { return *this; }

  void AssertHeld() __attribute__((assert_exclusive_lock()));
  void AssertReaderHeld() __attribute__((assert_shared_lock()));
};

class __attribute__((lockable)) __attribute__((reentrant_capability)) ReentrantMutex {
public:
  void Lock() __attribute__((exclusive_lock_function()));
  void Unlock() __attribute__((unlock_function()));


  const ReentrantMutex& operator!() const { return *this; }
};

class __attribute__((scoped_lockable)) MutexLock {
public:
  MutexLock(Mutex *mu) __attribute__((exclusive_lock_function(mu)));
  MutexLock(Mutex *mu, bool adopt) __attribute__((exclusive_locks_required(mu)));
  ~MutexLock() __attribute__((unlock_function()));
};

namespace SimpleTest {

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

  void test2() {
    mu.Lock();
    a = 0;
    mu.Unlock();
    baz();
  }

  void test3() __attribute__((exclusive_locks_required(!mu))) {
    mu.Lock();
    a = 0;
    mu.Unlock();
    baz();
  }

  void test4() {
    MutexLock lock(&mu);
  }
};

class Reentrant {
  ReentrantMutex mu;

public:
  void acquire() {
    mu.Lock();
    mu.Unlock();
  }

  void requireNegative() __attribute__((exclusive_locks_required(!mu))) {
    mu.Lock();
    mu.Unlock();
  }

  void callRequireNegative() {
    requireNegative();
  }

  void callHaveNegative() __attribute__((exclusive_locks_required(!mu))) {
    requireNegative();
  }
};

}

Mutex globalMutex;

namespace ScopeTest {

void f() __attribute__((exclusive_locks_required(!globalMutex)));
void fq() __attribute__((exclusive_locks_required(!::globalMutex)));

namespace ns {
  Mutex globalMutex;
  void f() __attribute__((exclusive_locks_required(!globalMutex)));
  void fq() __attribute__((exclusive_locks_required(!ns::globalMutex)));
}

void testGlobals() __attribute__((exclusive_locks_required(!ns::globalMutex))) {
  f();
  fq();
  ns::f();
  ns::fq();
}

void testNamespaceGlobals() __attribute__((exclusive_locks_required(!globalMutex))) {
  f();
  fq();
  ns::f();
  ns::fq();
}

class StaticMembers {
public:
  void pub() __attribute__((exclusive_locks_required(!publicMutex)));
  void prot() __attribute__((exclusive_locks_required(!protectedMutex)));
  void priv() __attribute__((exclusive_locks_required(!privateMutex)));
  void test() {
    pub();
    prot();
    priv();
  }

  static Mutex publicMutex;

protected:
  static Mutex protectedMutex;

private:
  static Mutex privateMutex;
};

void testStaticMembers() {
  StaticMembers x;
  x.pub();
  x.prot();
  x.priv();
}

}

namespace DoubleAttribute {

struct Foo {
  Mutex &mutex();
};

template <typename A>
class TemplateClass {
  template <typename B>
  static void Function(Foo *F)
      __attribute__((exclusive_locks_required(F->mutex()))) __attribute__((unlock_function(F->mutex()))) {}
};

void test() { TemplateClass<int> TC; }

}
