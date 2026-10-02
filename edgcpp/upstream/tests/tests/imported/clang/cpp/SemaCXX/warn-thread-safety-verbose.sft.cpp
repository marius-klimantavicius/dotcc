//type: fp
//options:  --c++11 --exceptions -DUSE_CAPABILITY=0: --c++11 --exceptions -DUSE_CAPABILITY=1
# 1 "SemaCXX/warn-thread-safety-verbose.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-thread-safety-verbose.cpp" 2



# 1 "SemaCXX/thread-safety-annotations.h" 1
# 5 "SemaCXX/warn-thread-safety-verbose.cpp" 2

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


class Test {
  Mutex mu;
  int a __attribute__((guarded_by(mu)));

  void foo1() __attribute__((exclusive_locks_required(mu)));
  void foo2() __attribute__((shared_locks_required(mu)));
  void foo3() __attribute__((locks_excluded(mu)));

  void test1() {
    a = 0;
  }

  void test2() {
    int b = a;
  }

  void test3() {
    foo1();
  }

  void test4() {
    foo2();
  }

  void test5() {
    mu.ReaderLock();
    foo1();
    mu.Unlock();
  }

  void test6() {
    mu.ReaderLock();
    a = 0;
    mu.Unlock();
  }

  void test7() {
    mu.Lock();
    foo3();
    mu.Unlock();
  }
};
