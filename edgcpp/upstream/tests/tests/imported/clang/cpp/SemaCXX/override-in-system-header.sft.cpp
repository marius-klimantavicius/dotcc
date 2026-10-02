//type: fp
//options:  --c++11
# 1 "SemaCXX/override-in-system-header.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/override-in-system-header.cpp" 2


# 1 "SemaCXX/Inputs/override-system-header.h" 1
# 4 "SemaCXX/override-in-system-header.cpp" 2

struct A
{
  virtual void x();
  virtual unsigned AddRef(void) = 0;;
  virtual void Initialize();
};

struct B : A
{
  virtual void x() override;
  virtual unsigned AddRef(void) = 0;;
  virtual void Initialize();
};
