# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

# This script is execute by build-edg-env before creation of the Docker image
# to build the image's dependencies.

from edgdocker import has_image_tagged

def main() -> None:
  # exec_build_pipeline and build_options are implicitly available (and
  # provided by the calling build-edg-env instance).
  if not has_image_tagged('native-tools') or build_options.no_cache:
    exec_build_pipeline('native-tools', build_options)

  if not has_image_tagged('softfloat') or build_options.no_cache:
    exec_build_pipeline('softfloat', build_options)

main()
