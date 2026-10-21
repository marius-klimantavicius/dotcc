#!/usr/bin/env python3
"""Approximate TEXTUAL discovery, never a complete call graph or translation.

No subprocesses, preprocessing, compiler, source mutation, native execution,
or semantic code generation. Writes only inventory reports beside this file.
Unresolved/disabled branches are deliberately retained. A reviewer must open
the source to establish every routine's actual enabled branch/dependency set.
"""
import bisect
import hashlib
import json
from pathlib import Path
import re

OUT = Path(__file__).resolve().parent
ROOT = OUT.parents[3]
SRC = ROOT / 'edgcpp/upstream/src'

# Deliberately lexical: raw strings, macro expansion, templates, conditional
# compilation, and C++ grammar are not implemented by this indexer.
COMMENTS_AND_LITERALS = re.compile(
    r'/\*.*?\*/|//[^\n]*|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'', re.S)
END_MARKER = re.compile(r'^}[ \t]*/\*[ \t]*([\w:~]+)[ \t]*\*/', re.M)
TOKEN_CALL = re.compile(r'\b([A-Za-z_]\w*)\s*\(')
BRANCH = re.compile(r'\b(if|else|for|while|switch|case|default|goto|return|break|continue)\b|\?|&&|\|\|')
DIRECTIVE = re.compile(r'^\s*#\s*(if|ifdef|ifndef|elif|else|endif)\b(.*)')
NON_CALLS = {'if','while','switch','for','sizeof','alignof','decltype',
             'static_cast','const_cast','reinterpret_cast','dynamic_cast','return'}


def hash_bytes(b):
    return hashlib.sha256(b).hexdigest()


def blank(m):
    return re.sub(r'[^\n]', ' ', m.group())


def save_jsonl(path, rows):
    with path.open('w') as f:
        for row in rows:
            f.write(json.dumps(row, sort_keys=True) + '\n')


def main():
    compdb = json.loads((ROOT / 'edgcpp/build/compile_commands.json').read_text())
    memberships = {}
    for row in compdb:
        m = re.search(r'CMakeFiles/([^/]+)\.dir/', row.get('output',''))
        memberships.setdefault(str(Path(row['file']).resolve()), set()).add(m.group(1) if m else '?')
    files, routines, macros, types, includes, directives, residuals = [], [], [], [], [], [], []
    # Every src file, including text/table/config files, gets an accountability row.
    for path in sorted(p for p in SRC.rglob('*') if p.is_file()):
        raw = path.read_bytes()
        text = raw.decode('latin1')  # byte-preserving; hashes use original bytes
        rel = str(path.relative_to(ROOT))
        file_hash = hash_bytes(raw)
        lines = text.splitlines(keepends=True)
        starts = [0]
        for line in lines:
            starts.append(starts[-1]+len(line))
        lineno = lambda offset: bisect.bisect_right(starts, offset)
        masked = COMMENTS_AND_LITERALS.sub(blank, text)
        masked_lines = masked.splitlines()
        guards, at_line = [], {}
        before_routines, before_macros, before_types = len(routines), len(macros), len(types)
        for n, line in enumerate(lines, 1):
            d = DIRECTIVE.match(line)
            if d:
                op, expr = d.groups()
                directives.append(dict(file=rel,line=n,directive=op,text=expr.strip(),
                                       status='not evaluated'))
                if op in ('if','ifdef','ifndef'):
                    guards.append(dict(line=n, alternatives=[op+' '+expr.strip()]))
                elif op in ('elif','else') and guards:
                    guards[-1]['alternatives'].append(op+' '+expr.strip())
                elif op == 'endif' and guards:
                    guards.pop()
            # Store copies because branch alternatives change later.
            at_line[n] = [dict(line=g['line'],alternatives=g['alternatives'][:]) for g in guards]
            m = re.match(r'\s*#\s*define\s+(\w+)(.*)', line)
            if m:
                end = n
                while end < len(lines) and lines[end-1].rstrip('\r\n').endswith('\\'):
                    end += 1
                block = ''.join(lines[n-1:end]).encode('latin1')
                macros.append(dict(file=rel,name=m.group(1),start=n,end=end,
                    sha256=hash_bytes(block),file_sha256=file_hash,
                    replacement=''.join(lines[n-1:end]).rstrip(),
                    guards=at_line[n],status='unstarted; guard selection unresolved'))
            if re.match(r'\s*#\s*include\b', line):
                includes.append(dict(file=rel,line=n,text=line.strip(),guards=at_line[n]))
            # Type DECLARATION candidates only; do not claim layout or names of
            # multiline typedefs have been resolved.
            if re.match(r'\s*(typedef\b|(?:struct|class|union|enum)\s+\w+)', masked_lines[n-1] if n<=len(masked_lines) else ''):
                types.append(dict(file=rel,line=n,text=line.strip(),sha256=hash_bytes(line.encode('latin1')),
                                  guards=at_line[n],status='declaration candidate; layout unresolved'))
        if path.suffix in ('.c','.cpp','.h','.hpp'):
            last_end = 0
            spans = []
            for close in END_MARKER.finditer(text):
                name = close.group(1)
                if name in {'if','for','while','switch','do','namespace','class','struct','union','enum'}:
                    continue
                # Name occurs on the function's signature line. Reject obvious
                # comments/calls by looking for a source-oriented signature line.
                local = masked[last_end:close.start()]
                pattern = re.compile(r'^[ \t]*(?:[\w:<>~*&]+[ \t]+)*[\w:~]*\b'+re.escape(name.split('::')[-1])+r'\s*\(', re.M)
                candidates = list(pattern.finditer(local))
                # The first candidate after the previous top-level end is a
                # heuristic, not C++ overload/name resolution.
                if not candidates:
                    residuals.append(dict(file=rel,line=lineno(close.start()),name=name,
                                          reason='end marker without recognized signature'))
                    continue
                begin = last_end + candidates[0].start()
                body = masked.find('{', begin, close.start())
                if body < 0:
                    residuals.append(dict(file=rel,line=lineno(close.start()),name=name,
                                          reason='end marker without recognized body'))
                    continue
                end = close.end()
                if end < len(text) and text[end]=='\n':
                    end += 1
                body_text = masked[body:close.start()+1]
                ordered_calls = [dict(name=m.group(1),line=lineno(body+m.start()))
                                 for m in TOKEN_CALL.finditer(body_text)
                                 if m.group(1) not in NON_CALLS]
                branches = [dict(token=m.group(),line=lineno(body+m.start()))
                            for m in BRANCH.finditer(body_text)]
                internal_pp = [dict(line=n,text=lines[n-1].strip())
                               for n in range(lineno(begin),lineno(end-1)+1)
                               if DIRECTIVE.match(lines[n-1])]
                routines.append(dict(
                    id=f'{rel}:{lineno(begin)}:{name}', file=rel,name=name,
                    start=lineno(begin),body_start=lineno(body),end=lineno(end-1),
                    source_sha256=hash_bytes(raw[begin:end]),file_sha256=file_hash,
                    discovery='approximate EDG end-comment plus signature regex',
                    guards=at_line.get(lineno(begin),[]),internal_conditionals=internal_pp,
                    branch_candidates=branches,ordered_call_candidates=ordered_calls,
                    managed_counterpart=None,implementation_state='unstarted',
                    effects_identity_lifetime='uninspected',review='unreviewed',
                    configuration_status='unresolved; do not infer enabled/disabled from this index'))
                spans.append((begin,end))
                last_end = end
            # Every unclaimed source region remains explicit. This catches
            # inline/template/macro-defined routines missed by the heuristic.
            cursor = 0
            for begin,end in spans:
                if begin>cursor and masked[cursor:begin].strip():
                    residuals.append(dict(file=rel,start=lineno(cursor),end=lineno(begin-1),
                                          reason='unclassified source region; may contain missed routines/types/data'))
                cursor=end
            if masked[cursor:].strip():
                residuals.append(dict(file=rel,start=lineno(cursor),end=len(lines),
                                      reason='unclassified trailing region; may contain missed routines/types/data'))
        files.append(dict(file=rel,sha256=file_hash,bytes=len(raw),lines=len(lines),
            compiled_targets=sorted(memberships.get(str(path.resolve()),[])),
            routine_candidates=len(routines)-before_routines,macro_definitions=len(macros)-before_macros,
            type_declaration_candidates=len(types)-before_types,
            accountability='present; unstarted; enabled reachability and classification require manual review'))
    for name,rows in [('files',files),('routine-candidates',routines),('macro-definitions',macros),
                      ('type-candidates',types),('include-directives',includes),
                      ('conditional-directives',directives),('unresolved-regions',residuals)]:
        save_jsonl(OUT/(name+'.jsonl'),rows)
    summary=dict(schema=1,status='PARTIAL routine/type/dependency inventory; complete src-file enumeration',
        limitations=['No preprocessing or guard evaluation','No overload/template/virtual/function-pointer resolution',
                     'Macros/types may be mistaken for calls','Inline/constructor/operator/macro-defined routines may be missed',
                     'End-comment/signature regions require exact manual confirmation',
                     'No completeness claim for enabled routines or the call graph'],
        counts=dict(files=len(files),routine_candidates=len(routines),macro_definitions=len(macros),
                    type_declaration_candidates=len(types),include_directives=len(includes),
                    conditional_directives=len(directives),unresolved_regions=len(residuals)))
    (OUT/'summary.json').write_text(json.dumps(summary,indent=2,sort_keys=True)+'\n')
    print(json.dumps(summary['counts'],sort_keys=True))


if __name__ == '__main__':
    main()
