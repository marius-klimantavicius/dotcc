/* Reviewed interpreter-only scalar IEEE-to-signed-integer conversion.
 * Arithmetic is unsigned and every shift is bounded. Host FP state is unused. */
static u64 ScalarFpToInteger(struct Machine *m, u64 bits, unsigned fraction,
                             unsigned exponent_bits, unsigned bias,
                             bool wide, bool truncate) {
  u64 fraction_mask = ((u64)1 << fraction) - 1;
  unsigned exponent_mask = (1u << exponent_bits) - 1;
  unsigned exponent = (bits >> fraction) & exponent_mask;
  bool negative = (bits >> (fraction + exponent_bits)) & 1;
  u64 significand = bits & fraction_mask;
  u64 magnitude = 0;
  u64 limit = (u64)1 << (wide ? 63 : 31);
  unsigned status = 0;
  bool invalid = exponent == exponent_mask;
  bool inexact = false;
  if (!exponent && (m->mxcsr & kMxcsrDaz)) significand = 0;
  if (!invalid && (exponent || significand)) {
    int power = (int)(exponent ? exponent : 1) - (int)bias;
    if (exponent) significand |= (u64)1 << fraction;
    if (power > 63) {
      invalid = true;
    } else if (power >= (int)fraction) {
      magnitude = significand << (unsigned)(power - (int)fraction);
    } else {
      unsigned shift = (unsigned)((int)fraction - power);
      u64 remainder;
      int halfway = -1;
      if (shift >= 64) {
        remainder = significand;
      } else {
        u64 half = (u64)1 << (shift - 1);
        magnitude = significand >> shift;
        remainder = significand & (((u64)1 << shift) - 1);
        halfway = remainder > half ? 1 : remainder == half ? 0 : -1;
      }
      inexact = remainder != 0;
      unsigned rounding = truncate ? 3 : (m->mxcsr >> 13) & 3;
      if (inexact && ((rounding == 0 &&
                      (halfway > 0 || (halfway == 0 && (magnitude & 1)))) ||
                     (rounding == 1 && negative) ||
                     (rounding == 2 && !negative))) {
        ++magnitude;
      }
    }
    if (!invalid && (negative ? magnitude > limit : magnitude >= limit)) {
      invalid = true;
    }
  }
  /* Invalid takes precedence over precision; old sticky bits are retained.
   * These four scalar conversion instructions do not raise denormal faults. */
  if (invalid) status = kMxcsrIe;
  else if (inexact) status = kMxcsrPe;
  m->mxcsr |= status;
  if (status & ~(m->mxcsr >> 7)) HaltMachine(m, kMachineSimdException);
  u64 result = invalid ? limit : negative ? (u64)0 - magnitude : magnitude;
  return wide ? result : result & 0xffffffffu;
}

static void OpGdqpWssCvttss2si(P) {
  u64 bits = Read32(GetModrmRegisterXmmPointerRead4(A));
  u64 result = ScalarFpToInteger(m, bits, 23, 8, 127, Rexw(rde), true);
  Put64(RegRexrReg(m, rde), result);
}

static void OpGdqpWsdCvttsd2si(P) {
  u64 bits = Read64(GetModrmRegisterXmmPointerRead8(A));
  u64 result = ScalarFpToInteger(m, bits, 52, 11, 1023, Rexw(rde), true);
  Put64(RegRexrReg(m, rde), result);
}

static void OpGdqpWssCvtss2si(P) {
  u64 bits = Read32(GetModrmRegisterXmmPointerRead4(A));
  u64 result = ScalarFpToInteger(m, bits, 23, 8, 127, Rexw(rde), false);
  Put64(RegRexrReg(m, rde), result);
}

static void OpGdqpWsdCvtsd2si(P) {
  u64 bits = Read64(GetModrmRegisterXmmPointerRead8(A));
  u64 result = ScalarFpToInteger(m, bits, 52, 11, 1023, Rexw(rde), false);
  Put64(RegRexrReg(m, rde), result);
}
