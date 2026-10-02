//type:fn
//options:--c++26 -A

#define prefix suffix
unsigned char arr1[] = {
#embed __FILE__ prefix(, 0)
};
#undef prefix

#define prefix(a, b) suffix(a, b)
unsigned char arr2[] = {
#embed __FILE__ prefix(, 0)
};
#undef prefix

#define suffix prefix
unsigned char arr3[] = {
#embed __FILE__ suffix(0,)
};
#undef suffix

#define suffix(a, b) prefix(a, b)
unsigned char arr4[] = {
#embed __FILE__ suffix(0,)
};
#undef suffix

#define limit prefix
unsigned char arr5[] = {
#embed __FILE__ limit(0,)
};
#undef limit

#define limit(a) prefix(a,)
unsigned char arr6[] = {
#embed __FILE__ limit(0)
};
#undef limit

#define if_empty prefix
unsigned char arr7[] = {
#embed __FILE__ if_empty(0,)
};
#undef if_empty

#define if_empty(a) prefix(a,)
unsigned char arr8[] = {
#embed __FILE__ if_empty(0)
};
#undef if_empty

//cwg: 3013
//title: Disallowing macros for #embed parameters
//meeting: Sofia 6/25
//edg_status: EDGcpfe/28247
