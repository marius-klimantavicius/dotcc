//remark:Microsoft mode nonreal instantiations and constexpr members
//options:--microsoft_v=1915 -tused;cp

template<typename>
struct foo { static const bool value = false; };
 
template<class T>
constexpr bool val_v = foo<T>::value;
                
template<class T>
class reverse_iterator {
static constexpr bool val = val_v<T>;
};
 
template <typename T>
struct myclass : reverse_iterator<T> {  };
