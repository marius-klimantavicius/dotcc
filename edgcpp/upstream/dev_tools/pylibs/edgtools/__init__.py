# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

from pathlib import Path

_TOOLS_DIR = Path(__file__).parent.parent.parent.absolute()

def get_tools_dir() -> Path:
  return _TOOLS_DIR
