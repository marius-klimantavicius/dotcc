    case kMachineFpuException:
      RestoreIp(m);
      m->faultaddr = m->ip;
      DeliverSignalToUser(m, SIGFPE_LINUX, FPE_FLTINV_LINUX);
      break;
    case kMachineSimdException: {
      /* Linux guest ABI: classify the unmasked status in priority order.
       * Includes older sticky status, which can affect a new fault's si_code.
       * This selects delivery metadata only; the instruction owns trap timing. */
      unsigned active = (m->mxcsr & 0x3f) & ~(m->mxcsr >> 7);
      const unsigned conditions[5] = {0x01, 0x04, 0x08, 0x12, 0x20};
      const int codes[5] = {FPE_FLTINV_LINUX, FPE_FLTDIV_LINUX,
                            FPE_FLTOVF_LINUX, FPE_FLTUND_LINUX,
                            FPE_FLTRES_LINUX};
      int signal_code = 0;
      for (unsigned i = 0; i < 5; ++i) {
        if (active & conditions[i]) {
          signal_code = codes[i];
          break;
        }
      }
      RestoreIp(m);
      m->faultaddr = m->ip;
      DeliverSignalToUser(m, SIGFPE_LINUX, signal_code);
      break;
    }
