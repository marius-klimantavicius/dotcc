#ifndef PROJECT_CPP_COMDAT_STORAGE_H
#define PROJECT_CPP_COMDAT_STORAGE_H
inline int inline_value = 3;
template<class T> struct SharedStorage { static int value; };
template<class T> int SharedStorage<T>::value = 17;
struct StaticStorage { inline static int value = 5; };
inline int* template_address() { return &SharedStorage<int>::value; }
int mutate_shared_storage();
int* provider_inline_address();
int* provider_template_address();
int* provider_member_address();
#endif
