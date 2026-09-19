/* Scalar COMIS/UCOMIS uses raw bits so NaNs and denormals cannot change host
 * floating state or lose signaling classification before guest exception tests. */
static void ScalarFpCompare(struct Machine *m, u64 left, u64 right,
                            unsigned fraction, unsigned exponent_bits,
                            bool unordered_instruction) {
  u64 fraction_mask = ((u64)1 << fraction) - 1;
  u64 exponent_mask = ((u64)1 << exponent_bits) - 1;
  u64 sign_mask = (u64)1 << (fraction + exponent_bits);
  u64 quiet_mask = (u64)1 << (fraction - 1);
  u64 lm = left & (sign_mask - 1);
  u64 rm = right & (sign_mask - 1);
  unsigned le = (left >> fraction) & exponent_mask;
  unsigned re = (right >> fraction) & exponent_mask;
  bool ln = le == exponent_mask && (left & fraction_mask);
  bool rn = re == exponent_mask && (right & fraction_mask);
  bool ld = !le && lm;
  bool rd = !re && rm;
  unsigned status = 0;
  bool unordered = ln || rn;
  if (unordered && (!unordered_instruction ||
                    (ln && !(left & quiet_mask)) ||
                    (rn && !(right & quiet_mask)))) status |= kMxcsrIe;
  if (m->mxcsr & kMxcsrDaz) {
    if (ld) lm = 0;
    if (rd) rm = 0;
  } else if ((ld || rd) && !unordered) {
    status |= kMxcsrDe;
  }
  m->mxcsr |= status;
  if (status & ~(m->mxcsr >> 7)) HaltMachine(m, kMachineSimdException);
  bool equal = !unordered && (lm == rm) &&
               (!lm || !((left ^ right) & sign_mask));
  bool less = false;
  if (!unordered && !equal) {
    if ((left ^ right) & sign_mask) less = (left & sign_mask) != 0;
    else less = left & sign_mask ? lm > rm : lm < rm;
  }
  m->flags = SetFlag(m->flags, FLAGS_ZF, unordered || equal);
  m->flags = SetFlag(m->flags, FLAGS_PF, unordered);
  m->flags = SetFlag(m->flags, FLAGS_CF, unordered || less);
  m->flags = SetFlag(m->flags, FLAGS_SF, false);
  m->flags = SetFlag(m->flags, FLAGS_OF, false);
  m->flags = SetFlag(m->flags, FLAGS_AF, false);
}

void OpComissVsWs(P) {
  bool unordered_instruction = ~Opcode(rde) & 1;
  if (!Osz(rde)) {
    u64 left = Read32(XmmRexrReg(m, rde));
    u64 right = Read32(GetModrmRegisterXmmPointerRead4(A));
    ScalarFpCompare(m, left, right, 23, 8, unordered_instruction);
  } else {
    u64 left = Read64(XmmRexrReg(m, rde));
    u64 right = Read64(GetModrmRegisterXmmPointerRead8(A));
    ScalarFpCompare(m, left, right, 52, 11, unordered_instruction);
  }
}
