#!/usr/bin/env python3
"""Bound and record a large generated C# build, without persistent compiler servers.

The RSS limit is a process-tree safety bound, not an assertion about total host
memory. The managed GC heap cap is lower so compilation can fail diagnostically
before exhausting that bound. Logs and sampled measurements are always retained.
"""
import argparse
import datetime
import json
import os
from pathlib import Path
import signal
import subprocess
import time


def process_tree(pid):
    """Linux measurements; absent /proc is reported rather than guessed."""
    result = []
    pending = [pid]
    seen = set()
    while pending:
        current = pending.pop()
        if current in seen:
            continue
        seen.add(current)
        try:
            status = Path(f'/proc/{current}/status').read_text()
            fields = dict(line.split(':', 1) for line in status.splitlines() if ':' in line)
            rss = int(fields.get('VmRSS', '0 kB').split()[0]) * 1024
            result.append({'pid': current, 'name': fields.get('Name', '').strip(), 'rssBytes': rss})
            # MSBuild starts csc from a worker thread. Linux's children file
            # belongs to one task, so looking only at the main task misses it.
            for children in Path(f'/proc/{current}/task').glob('*/children'):
                try:
                    pending.extend(map(int, children.read_text().split()))
                except (OSError, ValueError):
                    pass
        except (OSError, ValueError):
            pass
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('project', type=Path)
    parser.add_argument('--output-dir', required=True, type=Path)
    parser.add_argument('--rss-gib', type=float, default=48)
    parser.add_argument('--heap-gib', type=float, default=40)
    parser.add_argument('--timeout', type=float, default=1800)
    parser.add_argument('--processors', type=int, default=4)
    parser.add_argument('--analyzers', action='store_true', help='Enable SDK analyzers (off for initial compiler diagnostics).')
    parser.add_argument('--publish-aot', action='store_true')
    parser.add_argument('--rid', default='linux-x64')
    parser.add_argument('--warnings-as-errors', action='store_true')
    args = parser.parse_args()
    if not 0 < args.heap_gib < args.rss_gib or args.processors < 1 or args.timeout <= 0:
        parser.error('Require 0 < heap-gib < rss-gib, positive processors and timeout.')
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    project = args.project.resolve(strict=True)
    command = ['dotnet', 'publish' if args.publish_aot else 'build', str(project), '-c', 'Release', '--nologo',
               '-m:1', '-p:BuildInParallel=false', '-p:UseSharedCompilation=false',
               '-p:RunAnalyzers=' + str(args.analyzers).lower(),
               '-p:TreatWarningsAsErrors=' + str(args.warnings_as_errors).lower()]
    if args.publish_aot:
        command += ['-r', args.rid, '-p:PublishAot=true', '-p:DebugType=None', '-p:DebugSymbols=false',
                    '-p:IlcTreatWarningsAsErrors=' + str(args.warnings_as_errors).lower(), '-o', str(out/'published')]
    overrides = {'DOTNET_GCHeapHardLimit': format(int(args.heap_gib * 1024**3), 'x'),
                 'DOTNET_GCConserveMemory': '5', 'DOTNET_PROCESSOR_COUNT': str(args.processors),
                 'MSBUILDDISABLENODEREUSE': '1', 'DOTNET_CLI_TELEMETRY_OPTOUT': '1'}
    report = {'command': command, 'environmentOverrides': overrides,
              'startedUtc': datetime.datetime.now(datetime.timezone.utc).isoformat(),
              'rssLimitBytes': int(args.rss_gib * 1024**3), 'timeoutSeconds': args.timeout,
              'analyzersEnabled': args.analyzers, 'measurementsAvailable': Path('/proc/self/status').exists()}
    (out/'results.json').write_text(json.dumps(report, indent=2)+'\n')
    started = time.monotonic()
    reason = None
    peak = 0
    with (out/'stdout.log').open('w') as stdout, (out/'stderr.log').open('w') as stderr, (out/'resources.jsonl').open('w') as samples:
        process = subprocess.Popen(command, stdout=stdout, stderr=stderr, env={**os.environ, **overrides}, start_new_session=True)
        try:
            while process.poll() is None:
                tree = process_tree(process.pid)
                rss = sum(p['rssBytes'] for p in tree)
                peak = max(peak, rss)
                elapsed = time.monotonic() - started
                samples.write(json.dumps({'seconds': round(elapsed, 3), 'rssBytes': rss, 'processes': tree})+'\n')
                samples.flush()
                if rss > report['rssLimitBytes']:
                    reason = 'rss-limit'
                elif elapsed > args.timeout:
                    reason = 'timeout'
                if reason:
                    os.killpg(process.pid, signal.SIGTERM)
                    try:
                        process.wait(timeout=10)
                    except subprocess.TimeoutExpired:
                        os.killpg(process.pid, signal.SIGKILL)
                    break
                time.sleep(1)
            code = process.wait()
        except BaseException:
            os.killpg(process.pid, signal.SIGKILL)
            process.wait()
            raise
    report.update({'exitCode': code, 'terminationReason': reason,
                   'elapsedSeconds': round(time.monotonic()-started, 3), 'peakTreeRssBytes': peak,
                   'status': 'passed' if code == 0 and reason is None else 'failed'})
    (out/'results.json').write_text(json.dumps(report, indent=2)+'\n')
    print(json.dumps(report, indent=2))
    return 0 if report['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
