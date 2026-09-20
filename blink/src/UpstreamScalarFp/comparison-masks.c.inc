void OpCmppsd(P) {
  IGNORE_RACES_START();
  int imm = uimm0;
  if (Rep(rde) == 2) {
    union DoublePun x, y;
    y.i = Read64(GetModrmRegisterXmmPointerRead8(A));
    x.i = Read64(XmmRexrReg(m, rde));
    x.i = Cmpd(imm, x.f, y.f);
    Write64(XmmRexrReg(m, rde), x.i);
  } else if (Rep(rde) == 3) {
    union FloatPun x, y;
    y.i = Read32(GetModrmRegisterXmmPointerRead4(A));
    x.i = Read32(XmmRexrReg(m, rde));
    x.i = Cmps(imm, x.f, y.f);
    Write32(XmmRexrReg(m, rde), x.i);
  } else if (Osz(rde)) {
    u8 *p;
    union DoublePun x[2], y[2];
    p = GetModrmRegisterXmmPointerRead16(A);
    y[0].i = Read64(p + 0 * 8);
    y[1].i = Read64(p + 1 * 8);
    p = XmmRexrReg(m, rde);
    x[0].i = Read64(p + 0 * 8);
    x[1].i = Read64(p + 1 * 8);
    x[0].i = Cmpd(imm, x[0].f, y[0].f);
    x[1].i = Cmpd(imm, x[1].f, y[1].f);
    Write64(p + 0 * 8, x[0].i);
    Write64(p + 1 * 8, x[1].i);
  } else {
    u8 *p;
    union FloatPun x[4], y[4];
    p = GetModrmRegisterXmmPointerRead16(A);
    y[0].i = Read32(p + 0 * 4);
    y[1].i = Read32(p + 1 * 4);
    y[2].i = Read32(p + 2 * 4);
    y[3].i = Read32(p + 3 * 4);
    p = XmmRexrReg(m, rde);
    x[0].i = Read32(p + 0 * 4);
    x[1].i = Read32(p + 1 * 4);
    x[2].i = Read32(p + 2 * 4);
    x[3].i = Read32(p + 3 * 4);
    x[0].i = Cmps(imm, x[0].f, y[0].f);
    x[1].i = Cmps(imm, x[1].f, y[1].f);
    x[2].i = Cmps(imm, x[2].f, y[2].f);
    x[3].i = Cmps(imm, x[3].f, y[3].f);
    Write32(p + 0 * 4, x[0].i);
    Write32(p + 1 * 4, x[1].i);
    Write32(p + 2 * 4, x[2].i);
    Write32(p + 3 * 4, x[3].i);
  }
  IGNORE_RACES_END();
}
