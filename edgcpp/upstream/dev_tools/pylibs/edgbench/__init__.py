# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

import os
import sys

import edgutil

from pathlib import Path
from typing import List, Optional

def load_benchmark_home_or_exit(*, verbose: bool = False) -> Path:
  # Verify EDG_BENCH_HOME points at a directory.
  if 'EDG_BENCH_HOME' in os.environ:
    bench_home = Path(os.environ['EDG_BENCH_HOME'])

    if not bench_home.is_dir():
      edgutil.eprint(
        f"\"EDG_BENCH_HOME\" is set ({bench_home}) but is not a valid "
        'directory on this system.'
      )
      sys.exit(1)
  else:
    bench_home = edgutil.find_mono_repo_or_exit() / 'benchmarks'

  # Print bench home value in verbose mode.
  if verbose:
    if 'EDG_BENCH_HOME' in os.environ:
      edgutil.print_env_var('EDG_BENCH_HOME')

  return bench_home

def get_benchmark_runs(bench_home: Path) -> List[Path]:
  '''Get a list of all subdirectories of the bench runs folder with the most
  recently created first.
  '''
  runs_dir = bench_home / 'runs'

  if not runs_dir.exists():
    return []

  return edgutil.sorted_runs_in_dir(runs_dir)

class BenchmarkRunInspection:
  '''This class contains information about a benchmark run that's both
  computed and obtained from the user's command line arguments.
  '''
  def __init__(self, root_path: Optional[Path], num_runs_back: int,
               previous_run_dirs: List[Path]) -> None:
    self.root_path = root_path
    self.num_runs_back = num_runs_back
    self.previous_run_dirs = previous_run_dirs

  def is_previous_run_search(self) -> bool:
    return self.root_path is None

  def get_run_directory(self) -> Path:
    '''Return the path to walk for .rt.tar files.'''
    if self.is_previous_run_search():
      # Using -b flags and implicit root directory resolution.
      return self.previous_run_dirs[self.num_runs_back]
    else:
      # Using an explicit root directory.
      assert self.root_path is not None
      return self.root_path

def validate_inspection(inspection: BenchmarkRunInspection) -> None:
  '''This function validates the inspection before use (giving a user facing
  error if there is a problem).
  '''
  if inspection.is_previous_run_search():
    # Using -b flags and implicit root directory resolution.
    num_previous_runs = len(inspection.previous_run_dirs)
    if num_previous_runs == 0:
      # If there are no previous runs, handle the "default" (i.e., no -b flag)
      # case.
      edgutil.eprint('no previous runs are available')
      sys.exit(1)
    if inspection.num_runs_back >= num_previous_runs:
      # There are multiple runs available but not as many as the user
      # requested. Add one to the run count as no -b flags (i.e.,
      # inspection.num_runs_back) is still 1 run ago.
      edgutil.eprint(
        f"only {num_previous_runs} previous runs are available, "
        f"cannot examine {inspection.num_runs_back + 1} runs ago"
      )
      sys.exit(1)
  else:
    # Using an explicit root directory.
    if inspection.num_runs_back != 0:
      edgutil.eprint('-b cannot be combined with --root')
      sys.exit(1)

