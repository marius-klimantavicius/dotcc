//type: fn
//options: 
# 0 "./diagnostic/aka5a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./diagnostic/aka5a.C"

# 1 "./diagnostic/aka5.h" 1

       
# 3 "./diagnostic/aka5.h" 3



# 5 "./diagnostic/aka5.h" 3
typedef enum __internal_enum { A, B } user_enum;
typedef user_enum *user_enum_ptr;

typedef struct __internal_struct { int i; } user_struct;
typedef user_struct user_struct_copy;
typedef user_struct *user_struct_ptr;

typedef union __internal_union { int i; } user_union;
typedef user_union user_union_copy;
typedef user_union *user_union_ptr;

typedef unsigned int user_vector __attribute__((__vector_size__(16)));
typedef user_vector user_vector_copy;
typedef user_vector *user_vector_ptr;

typedef int user_int;
typedef user_int user_int_copy;
typedef user_int *user_int_ptr;
# 3 "./diagnostic/aka5a.C" 2


# 4 "./diagnostic/aka5a.C"
typedef user_enum user_enum_copy;

struct s { int i; };

user_enum ue1;
user_enum_copy ue2;
user_enum_ptr ue_ptr1;
user_enum *ue_ptr2;
const user_enum *const_ue_ptr1;
const user_enum_copy *const_ue_ptr2;
volatile user_enum *volatile_ue_ptr1;
volatile user_enum_copy *volatile_ue_ptr2;
user_enum (*ue_array_ptr1)[10];
user_enum_copy (*ue_array_ptr2)[10];
user_enum (*ue_fn_ptr1) (void);
void (*ue_fn_ptr2) (user_enum);
void (*ue_fn_ptr3) (user_enum, ...);
user_enum_copy (*ue_fn_ptr4) (void);
void (*ue_fn_ptr5) (user_enum_copy);
void (*ue_fn_ptr6) (user_enum_copy, ...);
user_enum (*__attribute__((__transaction_unsafe__)) unsafe_ue_fn_ptr1) (void);
user_enum_copy (*__attribute__((__transaction_unsafe__)) unsafe_ue_fn_ptr2) (void);

user_struct us1;
user_struct_copy us2;
user_struct_ptr us_ptr1;
user_struct *us_ptr2;
const user_struct *const_us_ptr1;
const user_struct_copy *const_us_ptr2;

user_union uu1;
user_union_copy uu2;
user_union_ptr uu_ptr1;
user_union *uu_ptr2;
const user_union *const_uu_ptr1;
const user_union_copy *const_uu_ptr2;

user_vector uv1;
user_vector_copy uv2;
user_vector_ptr uv_ptr1;
user_vector *uv_ptr2;
const user_vector *const_uv_ptr1;
const user_vector_copy *const_uv_ptr2;

user_int ui1;
user_int_copy ui2;
user_int_ptr ui_ptr1;
user_int *ui_ptr2;
const user_int *const_ui_ptr1;
const user_int_copy *const_ui_ptr2;
volatile user_int *volatile_ui_ptr1;
volatile user_int_copy *volatile_ui_ptr2;
user_int (*ui_array_ptr1)[10];
user_int_copy (*ui_array_ptr2)[10];
user_int (*ui_fn_ptr1) (void);
void (*ui_fn_ptr2) (user_int);
void (*ui_fn_ptr3) (user_int, ...);
user_int_copy (*ui_fn_ptr4) (void);
void (*ui_fn_ptr5) (user_int_copy);
void (*ui_fn_ptr6) (user_int_copy, ...);
user_int (*__attribute__((__transaction_unsafe__)) unsafe_ui_fn_ptr1) (void);
user_int_copy (*__attribute__((__transaction_unsafe__)) unsafe_ui_fn_ptr2) (void);

void f (s s1)
{
  ue1 = s1;
  ue2 = s1;
  ue_ptr1 = &s1;
  ue_ptr2 = &s1;
  const_ue_ptr1 = &s1;
  const_ue_ptr2 = &s1;
  volatile_ue_ptr1 = &s1;
  volatile_ue_ptr2 = &s1;
  ue_array_ptr1 = &s1;
  ue_array_ptr2 = &s1;
  ue_fn_ptr1 = &s1;
  ue_fn_ptr2 = &s1;
  ue_fn_ptr3 = &s1;
  ue_fn_ptr4 = &s1;
  ue_fn_ptr5 = &s1;
  ue_fn_ptr6 = &s1;
  unsafe_ue_fn_ptr1 = &s1;
  unsafe_ue_fn_ptr2 = &s1;

  us1 = s1;
  us2 = s1;
  us_ptr1 = &s1;
  us_ptr2 = &s1;
  const_us_ptr1 = &s1;
  const_us_ptr2 = &s1;

  uu1 = s1;
  uu2 = s1;
  uu_ptr1 = &s1;
  uu_ptr2 = &s1;
  const_uu_ptr1 = &s1;
  const_uu_ptr2 = &s1;

  uv1 = s1;
  uv2 = s1;
  uv_ptr1 = &s1;
  uv_ptr2 = &s1;
  const_uv_ptr1 = &s1;
  const_uv_ptr2 = &s1;

  ui1 = s1;
  ui2 = s1;
  ui_ptr1 = &s1;
  ui_ptr2 = &s1;
  const_ui_ptr1 = &s1;
  const_ui_ptr2 = &s1;
  volatile_ui_ptr1 = &s1;
  volatile_ui_ptr2 = &s1;
  ui_array_ptr1 = &s1;
  ui_array_ptr2 = &s1;
  ui_fn_ptr1 = &s1;
  ui_fn_ptr2 = &s1;
  ui_fn_ptr3 = &s1;
  ui_fn_ptr4 = &s1;
  ui_fn_ptr5 = &s1;
  ui_fn_ptr6 = &s1;
  unsafe_ui_fn_ptr1 = &s1;
  unsafe_ui_fn_ptr2 = &s1;
}
