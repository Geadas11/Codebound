"""Read-only PE32+ header/import/export/debug parser (stdlib only).
Usage: python -I pe_dump.py <exe> <out.json>
"""
import hashlib, json, mmap, struct, sys


def main():
    path, out = sys.argv[1], sys.argv[2]
    f = open(path, "rb")
    m = mmap.mmap(f.fileno(), 0, access=mmap.ACCESS_READ)
    pe = struct.unpack_from("<I", m, 0x3C)[0]
    assert m[pe:pe + 4] == b"PE\0\0"
    machine, nsec, ts, _p, _n, optsz, chars = struct.unpack_from("<HHIIIHH", m, pe + 4)
    opt = pe + 24
    magic = struct.unpack_from("<H", m, opt)[0]
    assert magic == 0x20B, "PE32+ expected"
    (lmaj, lmin, codesz, idatasz, udatasz, entry, codebase) = struct.unpack_from("<BBIIIII", m, opt + 2)
    imagebase = struct.unpack_from("<Q", m, opt + 24)[0]
    salign, falign = struct.unpack_from("<II", m, opt + 32)
    osmaj, osmin = struct.unpack_from("<HH", m, opt + 40)
    subsys, dllch = struct.unpack_from("<HH", m, opt + 68)
    stackres, stackcom, heapres, heapcom = struct.unpack_from("<QQQQ", m, opt + 72)
    ndd = struct.unpack_from("<I", m, opt + 108)[0]
    dd = [struct.unpack_from("<II", m, opt + 112 + 8 * i) for i in range(ndd)]
    sh = opt + optsz
    secs = []
    for i in range(nsec):
        name = m[sh + 40 * i: sh + 40 * i + 8].rstrip(b"\0").decode()
        vsz, va, rsz, rptr = struct.unpack_from("<IIII", m, sh + 40 * i + 8)
        ch = struct.unpack_from("<I", m, sh + 40 * i + 36)[0]
        secs.append({"name": name, "va": hex(va), "vsize": vsz, "raw_size": rsz, "raw_ptr": hex(rptr), "chars": hex(ch)})

    def rva2off(rva):
        for i in range(nsec):
            vsz, va, rsz, rptr = struct.unpack_from("<IIII", m, sh + 40 * i + 8)
            if va <= rva < va + max(vsz, rsz):
                return rva - va + rptr
        raise ValueError(hex(rva))

    def cstr(off):
        e = m.find(b"\0", off)
        return m[off:e].decode("latin1")

    imports = {}
    irva, isz = dd[1]
    p = rva2off(irva)
    while True:
        oft, _ts, _fc, nrva, ft = struct.unpack_from("<IIIII", m, p)
        if nrva == 0:
            break
        dll = cstr(rva2off(nrva))
        names = []
        q = rva2off(oft or ft)
        while True:
            v = struct.unpack_from("<Q", m, q)[0]
            if v == 0:
                break
            names.append(f"#{v & 0xFFFF}" if v >> 63 else cstr(rva2off(v & 0x7FFFFFFF) + 2))
            q += 8
        imports[dll] = names
        p += 20
    # delay imports (dd[13])
    delay = {}
    drva, dsz = dd[13] if ndd > 13 else (0, 0)
    if drva:
        p = rva2off(drva)
        while True:
            attr, nrva, _hmod, _iat, int_rva = struct.unpack_from("<IIIII", m, p)
            if nrva == 0:
                break
            dll = cstr(rva2off(nrva))
            names, q = [], rva2off(int_rva)
            while True:
                v = struct.unpack_from("<Q", m, q)[0]
                if v == 0:
                    break
                names.append(f"#{v & 0xFFFF}" if v >> 63 else cstr(rva2off(v & 0x7FFFFFFF) + 2))
                q += 8
            delay[dll] = names
            p += 32
    exports = []
    erva, esz = dd[0]
    if erva:
        e = rva2off(erva)
        nfun, nnames, afun, anames, aord = struct.unpack_from("<IIIII", m, e + 20)
        dllname = cstr(rva2off(struct.unpack_from("<I", m, e + 12)[0]))
        for i in range(nnames):
            nr = struct.unpack_from("<I", m, rva2off(anames) + 4 * i)[0]
            exports.append(cstr(rva2off(nr)))
        exports = {"dll_name": dllname, "functions": nfun, "names": exports}
    debug = []
    drva, dsz = dd[6]
    if drva:
        p = rva2off(drva)
        for i in range(dsz // 28):
            _c, dts, _ma, _mi, typ, sz, rva, ptr = struct.unpack_from("<IIHHIIII", m, p + 28 * i)
            ent = {"type": typ, "size": sz, "timestamp": dts}
            if typ == 2 and m[ptr:ptr + 4] == b"RSDS":
                ent["codeview_guid"] = m[ptr + 4:ptr + 20].hex()
                ent["age"] = struct.unpack_from("<I", m, ptr + 20)[0]
                ent["pdb_path"] = cstr(ptr + 24)
            debug.append(ent)
    tls = None
    trva, tsz = dd[9]
    if trva:
        t = rva2off(trva)
        s, e_, idx, cb = struct.unpack_from("<QQQQ", m, t)
        tls = {"raw_data_size": e_ - s, "callbacks_va": hex(cb)}
    pdata_entries = dd[3][1] // 12
    res = {
        "file": path, "size": len(m), "sha256": hashlib.sha256(m).hexdigest(),
        "machine": hex(machine), "timestamp_raw": ts, "characteristics": hex(chars),
        "linker": f"{lmaj}.{lmin}", "image_base": hex(imagebase), "entry_rva": hex(entry),
        "subsystem": subsys, "dll_characteristics": hex(dllch), "os_version": f"{osmaj}.{osmin}",
        "stack_reserve": stackres, "stack_commit": stackcom, "heap_reserve": heapres,
        "sections": secs,
        "data_directories": {n: {"rva": hex(a), "size": b} for n, (a, b) in zip(
            ["export", "import", "resource", "exception", "security", "basereloc", "debug", "arch",
             "globalptr", "tls", "loadconfig", "boundimport", "iat", "delayimport", "clr", "reserved"], dd)},
        "pdata_runtime_functions": pdata_entries,
        "imports": imports, "delay_imports": delay, "exports": exports, "debug": debug, "tls": tls,
    }
    json.dump(res, open(out, "w"), indent=1)
    print(json.dumps({k: res[k] for k in ("size", "sha256", "linker", "entry_rva", "subsystem", "dll_characteristics", "pdata_runtime_functions", "tls")}, indent=1))
    print("sections:", [(s["name"], s["vsize"]) for s in secs])
    print("imports:", {k: len(v) for k, v in imports.items()})
    print("delay:", {k: len(v) for k, v in delay.items()})
    print("exports:", exports if not exports else (exports["dll_name"], exports["functions"], exports["names"][:40]))
    print("debug:", debug)


if __name__ == "__main__":
    main()
