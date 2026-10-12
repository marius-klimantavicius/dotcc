#!/usr/bin/env python3
"""Generate valid independent inputs for normalization/retention measurements."""
from pathlib import Path
import argparse

p=argparse.ArgumentParser()
p.add_argument('--functions',type=int,default=1000)
p.add_argument('--elements',type=int,default=4000)
p.add_argument('--output',type=Path,required=True)
args=p.parse_args()
if args.functions<1 or args.elements<1:p.error('positive counts required')
args.output.parent.mkdir(parents=True,exist_ok=True)
header=args.output.with_suffix('.hpp')
header.write_text('''#pragma once
#define SCALE_INCREMENT(value) ((value) + 42)
template<class T> T scale_identity(T value) { return value; }
struct ScaleRecord { int first; long second; };
''')
lines=[f'#include "{header.name}"\n',
       f'static const int scale_values[{args.elements}] = {{'+','.join(['42']*args.elements)+'};\n',
       'static const char first_text[] = "repeated scalar text";\n',
       'static const char second_text[] = "repeated scalar text";\n']
for i in range(args.functions):
    lines.append(f'int scale_function_{i}(int value) {{ return scale_identity(SCALE_INCREMENT(value)); }}\n')
lines.append('int main() { return scale_function_0(scale_values[0]) + first_text[0] + second_text[0]; }\n')
args.output.write_text(''.join(lines))
print(f'{args.output}: {args.functions} retained functions, {args.elements} aggregate elements, repeated names/literals and macro invocations')
