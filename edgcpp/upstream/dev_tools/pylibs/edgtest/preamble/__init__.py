# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

import os
import platform
import sys

def ensure_launched_with_alsr_disabled() -> None:
  if platform.system() == 'Linux':
    # Check the process personality.
    with open(f"/proc/{os.getpid()}/personality", 'r') as file_handle:
      personality = int(file_handle.read()[:-1], 16)
    if (personality & 0x40000) == 0x0:
      # The process personality does not have the no ASLR bit set, relaunch.
      os.execvp('setarch', ['setarch', platform.machine(), '-R', *sys.argv])
