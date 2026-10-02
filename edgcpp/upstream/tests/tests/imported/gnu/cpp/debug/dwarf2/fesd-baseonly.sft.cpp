//type: fp
//options: 
# 0 "./debug/dwarf2/fesd-baseonly.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./debug/dwarf2/fesd-baseonly.C"
# 1 "fesd-baseonly.C"
# 1 "<built-in>"
# 1 "<command-line>"
# 1 "fesd-baseonly.C"


# 1 "time.h" 1 3 4

# 1 "time.h" 3 4
struct timespec
  {
    long int tv_sec;
    long int tv_nsec;
  };

struct itimerspec
  {
    struct timespec it_interval;
    struct timespec it_value;
  };
# 6 "fesd-baseonly.C" 2


# 7 "fesd-baseonly.C"
struct timespec base_var8;
struct itimerspec *base_var9;

# 1 "./debug/dwarf2/fesd-baseonly.h" 1
# 1 "./debug/dwarf2/fesd.h" 1
# 10 "./debug/dwarf2/fesd.h"
struct gstruct_head_ordy_decl_not;
struct gstruct_head_ordy_defn_not { int field_head_ordy_defn_not; };

struct gstruct_head_ordy_decl_ref_head;
struct gstruct_head_ordy_defn_ref_head { int field_head_ordy_defn_ref_head; };
struct gstruct_head_ordy_defn_ptr_head { int field_head_ordy_defn_ptr_head; };
struct gstruct_head_ordy_defn_fld_head { int field_head_ordy_defn_fld_head; };
struct gstruct_head_ordy_defn_var_head {
    gstruct_head_ordy_decl_ref_head *field_head_ordy_defn_var_head_inc;
    gstruct_head_ordy_defn_ref_head *field_head_ordy_defn_var_head_ref;
    gstruct_head_ordy_defn_ptr_head *field_head_ordy_defn_var_head_ptr;
    gstruct_head_ordy_defn_fld_head field_head_ordy_defn_var_head_fld;
};
extern struct gstruct_head_ordy_defn_var_head head_var1;

struct gstruct_head_ordy_decl_ref_base;
struct gstruct_head_ordy_defn_ref_base { int field_head_ordy_defn_ref_base; };
struct gstruct_head_ordy_defn_ptr_base { int field_head_ordy_defn_ptr_base; };
struct gstruct_head_ordy_defn_fld_base { int field_head_ordy_defn_fld_base; };
struct gstruct_head_ordy_defn_var_base { int field_head_ordy_defn_var_base; };

template< typename T > struct gstruct_head_tmpl_decl_not;
template< typename T > struct gstruct_head_tmpl_defn_not
{ T field_head_tmpl_defn_not; };

template< typename T > struct gstruct_head_tmpl_decl_ref_head;
template< typename T > struct gstruct_head_tmpl_defn_ref_head
{ T field_head_tmpl_defn_ref_head; };
template< typename T > struct gstruct_head_tmpl_defn_ptr_head
{ T field_head_tmpl_defn_ptr_head; };
template< typename T > struct gstruct_head_tmpl_defn_fld_head
{ T field_head_tmpl_defn_fld_head; };
template< typename T > struct gstruct_head_tmpl_defn_var_head {
    gstruct_head_tmpl_decl_ref_head< T > *field_head_tmpl_defn_var_head_inc;
    gstruct_head_tmpl_defn_ref_head< T > *field_head_tmpl_defn_var_head_ref;
    gstruct_head_tmpl_defn_ptr_head< T > *field_head_tmpl_defn_var_head_ptr;
    gstruct_head_tmpl_defn_fld_head< T > field_head_tmpl_defn_var_head_fld;
};
extern gstruct_head_tmpl_defn_var_head< int > head_var5;

template< typename T > struct gstruct_head_tmpl_decl_ref_base;
template< typename T > struct gstruct_head_tmpl_defn_ref_base
{ T field_head_tmpl_defn_ref_base; };
template< typename T > struct gstruct_head_tmpl_defn_ptr_base
{ T field_head_tmpl_defn_ptr_base; };
template< typename T > struct gstruct_head_tmpl_defn_fld_base
{ T field_head_tmpl_defn_fld_base; };
template< typename T > struct gstruct_head_tmpl_defn_var_base
{ T field_head_tmpl_defn_var_base; };

inline int head_function() {
    return 0
+ head_var1.field_head_ordy_defn_var_head_ptr->field_head_ordy_defn_ptr_head
+ head_var1.field_head_ordy_defn_var_head_fld.field_head_ordy_defn_fld_head
+ head_var5.field_head_tmpl_defn_var_head_ptr->field_head_tmpl_defn_ptr_head
+ head_var5.field_head_tmpl_defn_var_head_fld.field_head_tmpl_defn_fld_head
;
}
# 2 "./debug/dwarf2/fesd-baseonly.h" 2

struct gstruct_base_ordy_decl_not;
struct gstruct_base_ordy_defn_not { int field_base_ordy_defn_not; };

struct gstruct_base_ordy_decl_ref_base;
struct gstruct_base_ordy_defn_ref_base { int field_base_ordy_defn_ref_base; };
struct gstruct_base_ordy_defn_ptr_base { int field_base_ordy_defn_ptr_base; };
struct gstruct_base_ordy_defn_fld_base { int field_base_ordy_defn_fld_base; };
struct gstruct_base_ordy_defn_var_base {
    gstruct_head_ordy_decl_ref_base *field1_base_ordy_defn_var_base_inc;
    gstruct_head_ordy_defn_ref_base *field1_base_ordy_defn_var_base_ref;
    gstruct_head_ordy_defn_ptr_base *field1_base_ordy_defn_var_base_ptr;
    gstruct_head_ordy_defn_fld_base field1_base_ordy_defn_var_base_fld;
    gstruct_base_ordy_decl_ref_base *field2_base_ordy_defn_var_base_inc;
    gstruct_base_ordy_defn_ref_base *field2_base_ordy_defn_var_base_ref;
    gstruct_base_ordy_defn_ptr_base *field2_base_ordy_defn_var_base_ptr;
    gstruct_base_ordy_defn_fld_base field2_base_ordy_defn_var_base_fld;
};

template< typename T > struct gstruct_base_tmpl_decl_not;
template< typename T > struct gstruct_base_tmpl_defn_not
{ int field_base_tmpl_defn_not; };

template< typename T > struct gstruct_base_tmpl_decl_ref_base;
template< typename T > struct gstruct_base_tmpl_defn_ref_base
{ int field_base_tmpl_defn_ref_base; };
template< typename T > struct gstruct_base_tmpl_defn_ptr_base
{ int field_base_tmpl_defn_ptr_base; };
template< typename T > struct gstruct_base_tmpl_defn_fld_base
{ int field_base_tmpl_defn_fld_base; };
template< typename T > struct gstruct_base_tmpl_defn_var_base {
    gstruct_head_tmpl_decl_ref_base< T > *field1_base_tmpl_defn_var_base_inc;
    gstruct_head_tmpl_defn_ref_base< T > *field1_base_tmpl_defn_var_base_ref;
    gstruct_head_tmpl_defn_ptr_base< T > *field1_base_tmpl_defn_var_base_ptr;
    gstruct_head_tmpl_defn_fld_base< T > field1_base_tmpl_defn_var_base_fld;
    gstruct_base_tmpl_decl_ref_base< T > *field2_base_tmpl_defn_var_base_inc;
    gstruct_base_tmpl_defn_ref_base< T > *field2_base_tmpl_defn_var_base_ref;
    gstruct_base_tmpl_defn_ptr_base< T > *field2_base_tmpl_defn_var_base_ptr;
    gstruct_base_tmpl_defn_fld_base< T > field2_base_tmpl_defn_var_base_fld;
};
# 11 "fesd-baseonly.C" 2

struct gstruct_head_ordy_defn_var_base base_var1;
struct gstruct_base_ordy_defn_var_base base_var2;

struct gstruct_head_tmpl_defn_var_base< int > base_var5;
struct gstruct_base_tmpl_defn_var_base< int > base_var6;

int base_function() {
    return 0
+ base_var1.field_head_ordy_defn_var_base
+ base_var2.field1_base_ordy_defn_var_base_ptr->field_head_ordy_defn_ptr_base
+ base_var2.field1_base_ordy_defn_var_base_fld.field_head_ordy_defn_fld_base
+ base_var2.field2_base_ordy_defn_var_base_ptr->field_base_ordy_defn_ptr_base
+ base_var2.field2_base_ordy_defn_var_base_fld.field_base_ordy_defn_fld_base
+ base_var5.field_head_tmpl_defn_var_base
+ base_var6.field1_base_tmpl_defn_var_base_ptr->field_head_tmpl_defn_ptr_base
+ base_var6.field1_base_tmpl_defn_var_base_fld.field_head_tmpl_defn_fld_base
+ base_var6.field2_base_tmpl_defn_var_base_ptr->field_base_tmpl_defn_ptr_base
+ base_var6.field2_base_tmpl_defn_var_base_fld.field_base_tmpl_defn_fld_base
;
}
