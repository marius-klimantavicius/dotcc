//type: fn
//options: 
# 0 "./diag-aka-5a.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./diag-aka-5a.c"

# 1 "./diag-aka-5.h" 1

       
# 3 "./diag-aka-5.h" 3



# 5 "./diag-aka-5.h" 3
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
# 3 "./diag-aka-5a.c" 2


# 4 "./diag-aka-5a.c"
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
__extension__ _Atomic user_enum *atomic_ue_ptr1;
__extension__ _Atomic user_enum_copy *atomic_ue_ptr2;
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
__extension__ _Atomic user_int *atomic_ui_ptr1;
__extension__ _Atomic user_int_copy *atomic_ui_ptr2;
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

void f (struct s s)
{
  ue1 = s;
  ue2 = s;
  ue_ptr1 = &s;
  ue_ptr2 = &s;
  const_ue_ptr1 = &s;
  const_ue_ptr2 = &s;
  volatile_ue_ptr1 = &s;
  volatile_ue_ptr2 = &s;
  atomic_ue_ptr1 = &s;
  atomic_ue_ptr2 = &s;
  ue_array_ptr1 = &s;
  ue_array_ptr2 = &s;
  ue_fn_ptr1 = &s;
  ue_fn_ptr2 = &s;
  ue_fn_ptr3 = &s;
  ue_fn_ptr4 = &s;
  ue_fn_ptr5 = &s;
  ue_fn_ptr6 = &s;
  unsafe_ue_fn_ptr1 = &s;
  unsafe_ue_fn_ptr2 = &s;

  us1 = s;
  us2 = s;
  us_ptr1 = &s;
  us_ptr2 = &s;
  const_us_ptr1 = &s;
  const_us_ptr2 = &s;

  uu1 = s;
  uu2 = s;
  uu_ptr1 = &s;
  uu_ptr2 = &s;
  const_uu_ptr1 = &s;
  const_uu_ptr2 = &s;

  uv1 = s;
  uv2 = s;
  uv_ptr1 = &s;
  uv_ptr2 = &s;
  const_uv_ptr1 = &s;
  const_uv_ptr2 = &s;

  ui1 = s;
  ui2 = s;
  ui_ptr1 = &s;
  ui_ptr2 = &s;
  const_ui_ptr1 = &s;
  const_ui_ptr2 = &s;
  volatile_ui_ptr1 = &s;
  volatile_ui_ptr2 = &s;
  atomic_ui_ptr1 = &s;
  atomic_ui_ptr2 = &s;
  ui_array_ptr1 = &s;
  ui_array_ptr2 = &s;
  ui_fn_ptr1 = &s;
  ui_fn_ptr2 = &s;
  ui_fn_ptr3 = &s;
  ui_fn_ptr4 = &s;
  ui_fn_ptr5 = &s;
  ui_fn_ptr6 = &s;
  unsafe_ui_fn_ptr1 = &s;
  unsafe_ui_fn_ptr2 = &s;
}
