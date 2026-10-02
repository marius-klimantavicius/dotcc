//type:cn
//options_all:--c++20

template<typename T>
void func();

template<typename T>
void func();

template<typename T>
void func()
{
}

template<typename T>
void func();

template<typename T>
void func()
{
}

template<typename T>
struct class_ty;

template<typename T>
struct class_ty;

template<typename T>
struct class_ty
{
};

template<typename T>
struct class_ty;

template<typename T>
struct class_ty
{
};

struct non_templ_class_ty
{
  template<typename T>
  void mem_func();
};

template<typename T>
void non_templ_class_ty::mem_func()
{
}

template<typename T>
void non_templ_class_ty::mem_func()
{
}

template<typename T>
T var;

template<typename T>
T var;
