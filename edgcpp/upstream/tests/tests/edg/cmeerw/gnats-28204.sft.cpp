//type:fp
//options:--gn 140200;fn:--gn 140300
//options_all:--target linux_aarch64

#pragma GCC aarch64 "arm_acle.h"
#pragma GCC aarch64 "arm_sve.h"

void f(__SVBool_t b, unsigned int ui)
{
  __tstart();
  __tcommit();

  svpsel_lane_b16(b, b, ui);
}
