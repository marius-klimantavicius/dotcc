"""Compare authored host bindings with native GNU parsing of original CLI tables."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import uuid

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    output = args.output.resolve(); output.parent.mkdir(parents=True, exist_ok=True)
    work = ROOT / 'build/managed-getopt' / uuid.uuid4().hex[:12]; work.mkdir(parents=True)
    report = {'evidence_kind': 'test-only-native-getopt-binding-contract', 'status': 'failed', 'commands': [],
              'scope': 'Host CLI option parsing only; no TLS execution evidence.'}

    def command(argv, name):
        log = work / (name + '.log')
        with log.open('w') as stream:
            result = subprocess.run(list(map(str, argv)), stdout=stream, stderr=subprocess.STDOUT,
                                    timeout=300, env={k: v for k, v in os.environ.items() if k != 'POSIXLY_CORRECT'})
        report['commands'].append({'command': list(map(str, argv)), 'exit_code': result.returncode, 'log': str(log)})
        if result.returncode: raise RuntimeError('GNU getopt contract command failed: ' + str(log))
        return log

    try:
        scenarios, functions = [], []
        report['original_sources_sha256'] = {}
        for name in ('s2nc', 's2nd'):
            source = ROOT / 'ref/s2n-tls/bin' / (name + '.c'); text = source.read_text()
            report['original_sources_sha256'][str(source)] = hashlib.sha256(source.read_bytes()).hexdigest()
            definitions = dict(re.findall(r'^#define\s+(OPT_\w+)\s+(\d+)\s*$', text, re.M))
            table = re.search(r'(?:static )?struct option long_options\[\] = \{(.*?)\n    \};', text, re.S).group(1)
            short = re.search(r'getopt_long\(argc, argv, "([^"]+)"', text).group(1)
            options = []
            for option, argument, flag, value in re.findall(r'\{\s*"([^"]+)"\s*,\s*(\w+)\s*,\s*(0|NULL|&parallelize)\s*,\s*([^}]+)\}', table):
                value = value.strip()
                options.append({'name': option, 'argument': {'no_argument': 0, 'required_argument': 1, 'optional_argument': 2}[argument],
                                'flag': flag == '&parallelize', 'value': ord(value[1]) if value.startswith("'") else int(definitions.get(value, value))})
            if len(options) < 30: raise RuntimeError('Original CLI option inventory is incomplete')
            for option in options:
                arguments = ['--' + option['name']]
                if option['argument'] == 1: arguments += ['fixture-value']
                scenarios.append({'owner': name, 'options': options, 'short': short, 'argv': [name, 'host-before', *arguments, 'port-after']})
                if option['argument']:
                    scenarios.append({'owner': name, 'options': options, 'short': short, 'argv': [name, '--' + option['name'] + '=attached', 'host', '443']})
            for value, colons in re.findall(r'([^:])(:{0,2})', short):
                scenarios.append({'owner': name, 'options': options, 'short': short,
                                  'argv': [name, '-' + value + ('attached' if colons else ''), 'host', '443']})
            for ordering in ('', '+', '-'):
                for arguments in (['host', '--insecure', '443'], ['--', '--insecure'], ['--pre'], ['--unknown'], ['-z'], ['--ciphers']):
                    scenarios.append({'owner': name, 'options': options, 'short': ordering + short, 'argv': [name, *arguments]})
            macros = '\n'.join('#define ' + k + ' ' + v for k, v in definitions.items())
            function = ('#undef ' + '\n#undef '.join(definitions) + '\n' + macros + '\n' +
                'static void run_' + name + '(int scenario,int argc,char **argv,const char *shorts){\nint parallelize=0;\nstruct option long_options[]={' + table + '\n};\n' +
                'optind=0;optarg=NULL;opterr=0;optopt=\'?\';\nfor(int guard=0;;guard++){if(guard>200)exit(2);int idx=-9;int value=getopt_long(argc,argv,shorts,long_options,&idx);'
                'printf("%d:%d:%d:%s:%d:%d:%d\\n",scenario,value,optind,optarg?optarg:"~",idx,optopt,parallelize);if(value==-1)break;}\n'
                'printf("%d:argv:",scenario);for(int i=0;i<argc;i++)printf("%s%s",i?",":"",argv[i]);puts("");}\n')
            functions.append(function)
        source = work / 'native.c'
        source.write_text('#include <getopt.h>\n#include <stdio.h>\n#include <stdlib.h>\n' + '\n'.join(functions) + '\nint main(void){\n' +
            '\n'.join('{char *argv[]={' + ','.join(json.dumps(v) for v in s['argv']) + ',NULL};run_' + s['owner'] +
                      '(' + str(i) + ',' + str(len(s['argv'])) + ',argv,' + json.dumps(s['short']) + ');}' for i, s in enumerate(scenarios)) + '\nreturn 0;}\n')
        inputs = work / 'scenarios.json'; inputs.write_text(json.dumps(scenarios))
        native = work / 'native'
        command(['cc', '-std=c99', '-Wall', '-Wextra', source, '-o', native], 'native-build')
        expected = command([native], 'native')
        project = ROOT / 'tests/Upstream/GetoptProbe.csproj'
        command(['dotnet', 'build', project, '-c', 'Release', '--artifacts-path', work / 'artifacts', '-m:1', '--nologo'], 'managed-build')
        observed = command(['dotnet', work / 'artifacts/bin/GetoptProbe/release/GetoptProbe.dll', inputs], 'managed')
        if observed.read_bytes() != expected.read_bytes(): raise RuntimeError('Native/managed GNU getopt observations differ')
        report.update(status='passed', scenarios=len(scenarios), observations=len(observed.read_text().splitlines()),
                      native=str(expected), managed=str(observed), input=str(inputs), original_option_counts={
                          name: len(next(s['options'] for s in scenarios if s['owner'] == name)) for name in ('s2nc', 's2nd')})
    except Exception as error:
        report['error'] = str(error)
    output.write_text(json.dumps(report, indent=2) + '\n'); print('REPORT ' + str(output))
    return 0 if report['status'] == 'passed' else 1


if __name__ == '__main__':
    raise SystemExit(main())
