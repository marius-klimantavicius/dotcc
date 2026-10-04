// Reproduce il_alloc.c::check_host_alignment_parameters against real EDG types.
#include "basic_hdrs.h"
static_assert(TARG_SUPPORTS_X86_64==1,"preserve original compiler output target");
#include "fe_common.h"
USING_NAMESPACE_EDG

#if defined(__EDG_DOT_TARGET__) && (defined(__linux__) || defined(__x86_64__) || defined(_WIN32))
#error Native OS/CPU macros leaked into the portable self profile
#endif
struct PointerAlignment { char dummy; void* ptr; };
struct PrefixAlignment { char dummy; an_il_entry_prefix prefix; };
struct ConstantAlignment { char dummy; a_constant constant; };
static_assert(offsetof(PointerAlignment,ptr)==HOST_POINTER_ALIGNMENT,"host pointer alignment");
static_assert(offsetof(PrefixAlignment,prefix)==HOST_IL_ENTRY_PREFIX_ALIGNMENT,"IL prefix alignment");
static_assert(offsetof(ConstantAlignment,constant)==HOST_ALIGNMENT_REQUIRED,"IL allocation alignment");
static_assert(HOST_HAS_FLOAT16_TYPE==1,"preserve host float16 representation");
static_assert(HOST_COMPILER_SUPPORTS_BFLOAT16==1,"preserve C backend bfloat16 capability");
static_assert(MAX_MULTIBYTE_CHAR_LENGTH==16,"preserve compiler multibyte buffer bound");
static_assert(sizeof(a_host_fp_value)==16,"preserve compiler binary128 calculations");
static_assert(sizeof(long double)==16,"portable binary80 storage");
int main() {
  PointerAlignment pointer{};
  PrefixAlignment prefix{};
  ConstantAlignment constant{};
  if ((char*)&pointer.ptr-(char*)&pointer!=HOST_POINTER_ALIGNMENT) return 1;
  if ((char*)&prefix.prefix-(char*)&prefix!=HOST_IL_ENTRY_PREFIX_ALIGNMENT) return 2;
  if ((char*)&constant.constant-(char*)&constant!=HOST_ALIGNMENT_REQUIRED) return 3;
  volatile _Float16 half = (_Float16)1.5;
  if (sizeof(half)!=2 || (float)(half+(_Float16)0.5)!=2.0f) return 4;
  return 0;
}
