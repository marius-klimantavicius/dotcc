// Address-only contract test against the actual EDG structures and comparator.
// Numeric hooks deliberately abort: those paths are exercised by the translated
// two-TU fixture, while this unit proves relocation comparison needs no frontend.
#include <cstdlib>
#include <cstdint>
#include "fe_common.h"
#include "il.h"
#include "const_ints.h"
#include "float_pt.h"
USING_NAMESPACE_EDG

static int unused_integer(a_constant_ptr, a_constant_ptr) { std::abort(); }
static a_boolean unused_float(a_float_kind, an_internal_float_value*, an_internal_float_value*) { std::abort(); }
static a_type_ptr unused_typeref(a_type_ptr) { std::abort(); }
#define cmp_integer_constants unused_integer
#define fp_same_representation unused_float
#define skip_typerefs unused_typeref
#include "../dot_gen_initializer_identity.h"
#undef skip_typerefs
#undef fp_same_representation
#undef cmp_integer_constants

static void require(bool value) { if (!value) std::abort(); }

int main() {
  a_type pointer_type{}, other_type{};
  pointer_type.kind = other_type.kind = tk_pointer;
  a_variable first{}, second{};
  a_routine routine_first{}, routine_second{};
  auto poison = reinterpret_cast<a_trans_unit_corresp_ptr>(std::uintptr_t(1));
  pointer_type.source_corresp.trans_unit_corresp = poison;
  other_type.source_corresp.trans_unit_corresp = poison;
  first.source_corresp.trans_unit_corresp = poison;
  second.source_corresp.trans_unit_corresp = poison;
  routine_first.source_corresp.trans_unit_corresp = poison;
  routine_second.source_corresp.trans_unit_corresp = poison;
  a_constant left{}, right{};
  left.kind = right.kind = ck_address;
  left.type = right.type = &pointer_type;
  left.variant.address.kind = right.variant.address.kind = abk_variable;
  left.variant.address.variant.variable = &first;
  right.variant.address.variant.variable = &second;
  require(!same_lowered_initializer(&left, &right));
  right.variant.address.variant.variable = &first;
  require(same_lowered_initializer(&left, &right));
  right.variant.address.offset = 8;
  require(!same_lowered_initializer(&left, &right));
  left.variant.address.offset = 8;
  require(same_lowered_initializer(&left, &right));
  right.variant.address.one_past_the_end = TRUE;
  require(!same_lowered_initializer(&left, &right));
  right.variant.address.one_past_the_end = FALSE;
  right.variant.address.is_object_reflection = TRUE;
  require(!same_lowered_initializer(&left, &right));
  right.variant.address.is_object_reflection = FALSE;
  right.type = &other_type;
  require(!same_lowered_initializer(&left, &right));
  right.type = &pointer_type;

  left.variant.address.kind = right.variant.address.kind = abk_routine;
  left.variant.address.variant.routine = &routine_first;
  right.variant.address.variant.routine = &routine_second;
  require(!same_lowered_initializer(&left, &right));
  right.variant.address.variant.routine = &routine_first;
  require(same_lowered_initializer(&left, &right));

  a_constant aggregate_left{}, aggregate_right{};
  aggregate_left.kind = aggregate_right.kind = ck_aggregate;
  aggregate_left.type = aggregate_right.type = &pointer_type;
  aggregate_left.variant.aggregate.first_constant = &left;
  aggregate_right.variant.aggregate.first_constant = &right;
  require(same_lowered_initializer(&aggregate_left, &aggregate_right));
  right.variant.address.variant.routine = &routine_second;
  require(!same_lowered_initializer(&aggregate_left, &aggregate_right));
  right.variant.address.variant.routine = &routine_first;
  aggregate_right.variant.aggregate.has_dynamic_init_component = TRUE;
  require(!same_lowered_initializer(&aggregate_left, &aggregate_right));

  a_field field_first{}, field_second{};
  left.kind = right.kind = ck_designator;
  left.variant.designator.is_generic = right.variant.designator.is_generic = FALSE;
  left.variant.designator.is_field_designator = right.variant.designator.is_field_designator = TRUE;
  left.variant.designator.variant.field = &field_first;
  right.variant.designator.variant.field = &field_second;
  require(!same_lowered_initializer(&left, &right));
  right.variant.designator.variant.field = &field_first;
  require(same_lowered_initializer(&left, &right));
  return 0;
}
