//type: fp
//options: 
# 1 "SemaCXX/unique_object_duplication.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 482 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/unique_object_duplication.cpp" 2







# 1 "SemaCXX/unique_object_duplication.h" 1
# 16 "SemaCXX/unique_object_duplication.h"
constexpr int init_constexpr(int x) { return x; };
extern double init_dynamic(int);




namespace StaticLocalTest {

inline void has_static_locals_external() {

  static int disallowedStatic1 = 0;


  static const double disallowedStatic2 = disallowedStatic1++;


  static constexpr int allowedStatic1 = 0;
  static const float allowedStatic2 = 1;
  static constexpr int allowedStatic3 = init_constexpr(2);
  static const int allowedStatic4 = init_constexpr(3);
}



void has_static_locals_non_inline() {

  static int allowedStatic1 = 0;

  static const double allowedStatic2 = allowedStatic1++;
}


static void has_static_locals_internal() {
  static int allowedStatic1 = 0;
  static double allowedStatic2 = init_dynamic(2);
  static char allowedStatic3 = []() { return allowedStatic1++; }();
  static constexpr int allowedStatic4 = 0;
}

namespace {


void has_static_locals_anon() {
  static int allowedStatic1 = 0;
  static double allowedStatic2 = init_dynamic(2);
  static char allowedStatic3 = []() { return allowedStatic1++; }();
  static constexpr int allowedStatic4 = init_constexpr(3);
}

}

__attribute__((visibility("hidden"))) inline void static_local_always_hidden() {
    static int disallowedStatic1 = 3;


    {
      static int disallowedStatic2 = 3;


    }

    auto lmb = []() {
      static int disallowedStatic3 = 3;


    };
}


__attribute__((visibility("default"))) void static_local_never_hidden() {
    static int allowedStatic1 = 3;

    {
      static int allowedStatic2 = 3;
    }

    auto lmb = []() {
      static int allowedStatic3 = 3;
    };
}


const int setByLambda = ([]() { static int x = 3; return x++; })();

inline void has_extern_local() {
  extern int allowedAddressExtern;
}

inline void has_regular_local() {
  int allowedAddressLocal = 0;
}

inline void has_thread_local() {

  thread_local int disallowedThreadLocal = 0;

}


inline auto& allowedFunctionReference = has_static_locals_external;

}




namespace GlobalTest {

  inline float disallowedGlobal1 = 3.14;




  inline const double disallowedGlobal5 = disallowedGlobal1++;


  static float allowedGlobal1 = 3.14;
  const double allowedGlobal2 = init_dynamic(2);
  static const char allowedGlobal3 = []() { return disallowedGlobal1++; }();
  static inline double allowedGlobal4 = init_dynamic(2);


  constexpr int allowedGlobal5 = 0;
  const float allowedGlobal6 = 1;
  constexpr int allowedGlobal7 = init_constexpr(2);
  const int allowedGlobal8 = init_constexpr(3);



  float allowedGlobal9 = 3.14;


  inline float& nonConstReference = disallowedGlobal1;

  const inline int& constReference = allowedGlobal5;

  inline int* nonConstPointerToNonConst = nullptr;

  inline int const* nonConstPointerToConst = nullptr;

  inline int* const constPointerToNonConst = nullptr;

  inline int const* const constPointerToConst = nullptr;

  inline int const* const constPointerToConstNew = new int(7);

  inline int const * const * const * const nestedConstPointer = nullptr;
  inline int const * const ** const * const nestedNonConstPointer = nullptr;


  struct Test {
    static inline float disallowedStaticMember1;


    static float disallowedStaticMember2;

    static float allowedStaticMember1;


    __attribute__((visibility("default"))) static inline float allowedStaticMember2 = 0.0;
  };

  inline float Test::disallowedStaticMember2 = 2.3;



  struct __attribute__((visibility("default"))) NeverHidden {
    static inline float allowedStaticMember3;
    static float allowedStaticMember4;
  };

  inline float NeverHidden::allowedStaticMember4 = 3.4;
}





namespace TemplateTest {




template <typename T>
int allowedTemplate1 = 0;

template int allowedTemplate1<int>;

template <typename T>
inline int allowedTemplate2 = 0;

template int allowedTemplate2<int>;

}





namespace NestedClassTest {





class __attribute__((visibility("default"))) Outer {

  inline static int allowedOuterMember = 0;
  int* allowedOuterFunction() {
    static int allowed = 0;
    return &allowed;
  }


  class HiddenOnWindows {
    inline static int disallowedInnerMember = 0;


    int* disallowedInnerFunction() {
      static int disallowed = 0;
      return &disallowed;
    }
  };

  class __attribute__((visibility("default"))) AlwaysVisible {
    inline static int allowedInnerMember = 0;

    int* allowedInnerFunction() {
      static int allowed = 0;
      return &allowed;
    }
  };
};

}
# 9 "SemaCXX/unique_object_duplication.cpp" 2




namespace GlobalTest {
  float Test::allowedStaticMember1 = 2.3;
}

bool disallowed4 = true;
constexpr inline bool disallowed5 = true;
