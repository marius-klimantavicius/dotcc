//type: fp
//options:  --c++20 --modules
// PR c++/116403
// { dg-additional-options "-fmodules-ts -Wno-global-module" }
// { dg-module-cmi A }

module;

template <typename T> struct GMF {
  GMF(int);
};
GMF(int) -> GMF<int>;

export module A;
export using ::GMF;

export template <typename T> struct Attached {
  Attached(int);
};
Attached(int) -> Attached<int>;
