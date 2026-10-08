"""Validate portable libc record boundaries against the owned semantic layout."""
import json


PROFILES = {
    '__edg_FILE': ('FILE', 0, 0, [], 'void'),
    '__edg_DIR': ('POSIX_DIR', 0, 0, [], 'void'),
    '__edg_locale': ('LOCALE', 0, 0, [], 'void'),
    'lconv': ('LCONV', 96, 8,
        [(name, 'ptr(i8)', index * 8) for index, name in enumerate(('decimal_point', 'thousands_sep', 'grouping',
          'int_curr_symbol', 'currency_symbol', 'mon_decimal_point', 'mon_thousands_sep', 'mon_grouping', 'positive_sign', 'negative_sign'))] +
        [(name, 'i8', 80 + index) for index, name in enumerate(('int_frac_digits', 'frac_digits', 'p_cs_precedes', 'p_sep_by_space',
          'n_cs_precedes', 'n_sep_by_space', 'p_sign_posn', 'n_sign_posn', 'int_p_cs_precedes', 'int_p_sep_by_space',
          'int_n_cs_precedes', 'int_n_sep_by_space', 'int_p_sign_posn', 'int_n_sign_posn'))], 'void'),
    '__edg_fpos': ('FPOS', 16, 8, [('offset', 'i64', 0), ('state', 'u32', 8), ('value', 'u32', 12)], 'void'),
    'timespec': ('TIMESPEC', 16, 8, [('tv_sec', 'i64', 0), ('tv_nsec', 'i64', 8)], 'void'),
    'timeval': ('POSIX_TIMEVAL', 16, 8, [('tv_sec', 'i64', 0), ('tv_usec', 'i64', 8)], 'global::Edg.Runtime.PosixTimeval'),
    'rlimit': ('POSIX_RLIMIT', 16, 8, [('rlim_cur', 'u64', 0), ('rlim_max', 'u64', 8)], 'global::Edg.Runtime.PosixRlimit'),
    'stat': ('POSIX_STAT', 120, 8, [
        ('st_dev', 'u64', 0), ('st_ino', 'u64', 8), ('st_mode', 'u32', 16), ('st_nlink', 'u64', 24),
        ('st_uid', 'u32', 32), ('st_gid', 'u32', 36), ('st_size', 'i64', 40), ('st_atim', 'TIMESPEC', 48),
        ('st_mtim', 'TIMESPEC', 64), ('st_ctim', 'TIMESPEC', 80), ('st_rdev', 'u64', 96), ('st_blksize', 'i64', 104),
        ('st_blocks', 'i64', 112)], 'global::Edg.Runtime.PosixStat'),
    'statvfs': ('POSIX_STATVFS', 88, 8, [(name, 'u64', index * 8) for index, name in enumerate(
        ('f_bsize', 'f_frsize', 'f_blocks', 'f_bfree', 'f_bavail', 'f_files', 'f_ffree', 'f_favail', 'f_fsid', 'f_flag', 'f_namemax'))],
        'global::Edg.Runtime.PosixStatvfs'),
    'sockaddr': ('POSIX_SOCKADDR', 16, 2, [('sa_family', 'u16', 0), ('sa_data', 'array(14,i8)', 2)], 'void'),
    'dirent': ('POSIX_DIRENT', 4096, 1, [('d_name', 'array(4096,i8)', 0)], 'void'),
}


class Boundaries:
    def __init__(self, project, report):
        self.records = {}
        source = json.loads((project / report['sourceMap']).read_text())
        objects = {obj['unitId']: obj for obj in source['objects']}
        for record in report.get('records', []):
            profile = PROFILES.get(record['sourceName'])
            if profile is None:
                continue
            token, size, alignment, expected, runtime_type = profile
            obj = objects[record['unitId']]
            types = {row['id']: row for row in obj['executionTypes']}
            records = {row['typeId']: row for row in obj['recordTypes']}

            def kind(identifier):
                node = types[identifier]
                if node['kind'] == 'array':
                    return 'array(' + str(node['elementCount']) + ',' + kind(node['elementTypeId']) + ')'
                if node['kind'] == 'pointer':
                    return 'ptr(' + kind(node['pointeeTypeId']) + ')'
                if node['kind'] == 'record':
                    nested = records[identifier]
                    nested_fields = sorted((f for f in obj['recordFields'] if f['recordTypeId'] == identifier),
                                           key=lambda f: f['ordinal'])
                    # Only this exact leaf record is permitted in the portable
                    # stat ABI; equal names or aggregate sizes are insufficient.
                    if (nested['sourceName'], nested['size'], nested['alignment']) == ('timespec', 16, 8):
                        shape = [(f['sourceName'], types[f['typeId']]['kind'], f['byteOffset']) for f in nested_fields]
                        if shape == PROFILES['timespec'][3] and all(f.get('bitWidth') is None for f in nested_fields):
                            return 'TIMESPEC'
                return node['kind']

            fields = sorted((f for f in obj['recordFields'] if f['recordTypeId'] == record['typeId']), key=lambda f: f['ordinal'])
            actual = [(f['sourceName'], kind(f['typeId']), f['byteOffset']) for f in fields]
            if (record['size'], record['alignment'], actual) != (size, alignment, expected):
                continue
            if any(f.get('bitWidth') is not None for f in fields):
                continue
            self.records[record['generatedType']] = (token, runtime_type)

    def classify(self, csharp_type):
        """Return the canonical binding token and optional pointer bridge cast."""
        if csharp_type.endswith('*') and csharp_type[:-1] in self.records:
            token, runtime_type = self.records[csharp_type[:-1]]
            return token + '*', runtime_type + '*'
        return csharp_type, None
