//type: fp
//options:  --target linux_aarch64
# 1 "Sema/aarch64-sve-intrinsics/acle_neon_sve_bridge.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 479 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "Sema/aarch64-sve-intrinsics/acle_neon_sve_bridge.cpp" 2
# 13 "Sema/aarch64-sve-intrinsics/acle_neon_sve_bridge.cpp"
uint32x4_t __attribute__((target("+sve"))) foo(svuint32_t a) {
    return svget_neonq_u32(a);
}
