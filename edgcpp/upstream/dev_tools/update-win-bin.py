#!/usr/bin/env python3

# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

# This script updates the win-bin directory by generating appropraite wrapper
# scripts.

from pathlib import Path

tools_path = Path(__file__).parent

def create_python_invocation_script(source_script_path: Path) -> None:
  new_script_name = f"{source_script_path.name}.bat"
  with open(tools_path / 'win-bin' / new_script_name, 'w') as file_handle:
    file_handle.write('@echo off\n')
    file_handle.write(
      f"python.exe \"%~dp0..\\bin\\{source_script_path.name}\" %*"
    )

for file_path in (tools_path / 'bin').iterdir():
  try:
    with open(file_path, 'r', encoding = 'utf-8') as file_handle:
      shebang_line = file_handle.readline()
      if 'python' in shebang_line:
        create_python_invocation_script(file_path)
  except ValueError:
    pass
  except Exception as ex:
    ex.add_note(f"Error processing: {file_path}")
    raise
