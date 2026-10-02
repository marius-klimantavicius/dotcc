# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

# This script is execute by build-edg-env before creation of the Docker image
# to build the image's dependencies.

import os
import tarfile

import edgtools
import edgutil

from pathlib import Path

def create_scripts_and_libs_tar_file():
  '''Create native-tools.tar.gz for use by the Dockerfile.'''
  tools_dir = edgtools.get_tools_dir()
  tar_file_path = Path(os.getcwd()).joinpath('scripts-and-libs.tar.gz')

  with edgutil.CurrentWorkingDirectorySwap(tools_dir):
    with tarfile.open(tar_file_path, 'w:gz') as tar:
      tar.add('bin')
      tar.add('pylibs')

def main() -> None:
  create_scripts_and_libs_tar_file()

main()
