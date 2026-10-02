#include <typeinfo>
void __cxa_bad_typeid() {}
void f(std::type_info *info) {
 typeid(*info);
}
