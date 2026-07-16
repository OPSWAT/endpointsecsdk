#!/usr/bin/env python3
###############################################################################################
##  simple-proxy - functional_test.py
##
##  Black-box functional tests for a RUNNING simple-proxy instance. Exercises the documented
##  behavior over the network: /health, input validation, the cache MISS -> cached -> HIT
##  lifecycle, SSRF protection (hostname-resolving-to-private and raw private IP), scheme
##  enforcement, and UDP self-discovery.
##
##  Start a proxy first, e.g.:
##      simple-proxy-demo --discovery
##  then run:
##      python functional_test.py                              # against http://127.0.0.1:8080
##      python functional_test.py --base http://192.168.0.6:8080 --discovery-port 8099
##      python functional_test.py --base http://host:8080 --token s3cret   # secure-mode proxy
##
##  Exit code 0 if all checks pass, 1 otherwise. Standard library only.
##
##  Created by Chris Seiler - OPSWAT OEM Field CTO
###############################################################################################

import argparse
import json
import socket
import sys
import time
import urllib.error
import urllib.parse
import urllib.request

DISCOVERY_MAGIC = b"SIMPLE-PROXY-DISCOVER v1"


def main():
    ap = argparse.ArgumentParser(description="Functional tests against a running simple-proxy.")
    ap.add_argument("--base", default="http://127.0.0.1:8080", help="proxy base URL")
    ap.add_argument("--token", help="secure-mode token, if the proxy requires one")
    ap.add_argument("--discovery-port", type=int, default=8099)
    ap.add_argument("--no-discovery", action="store_true",
                    help="skip the UDP discovery checks (proxy started without --discovery)")
    ap.add_argument("--skip-redirect", action="store_true",
                    help="skip the redirect happy-path check (downloads a few MB)")
    args = ap.parse_args()

    base = args.base.rstrip("/")
    host = urllib.parse.urlsplit(base).hostname
    results = []

    def check(name, cond, detail=""):
        results.append(bool(cond))
        print(f"  [{'PASS' if cond else 'FAIL'}] {name}" + (f"  ({detail})" if detail else ""))

    def http_get(path, timeout=60):
        req = urllib.request.Request(base + path)
        if args.token:
            req.add_header("X-Proxy-Token", args.token)
        try:
            with urllib.request.urlopen(req, timeout=timeout) as r:
                return r.status, r.headers, r.read()
        except urllib.error.HTTPError as e:
            return e.code, e.headers, e.read()

    def enc(u):
        return urllib.parse.urlencode({"url": u})

    print(f"== simple-proxy functional tests against {base} ==")

    # health
    st, _, body = http_get("/health")
    try:
        h = json.loads(body)
    except Exception:
        h = {}
    check("health 200", st == 200, f"status={st}")
    check("health status=ok", h.get("status") == "ok", str(h))
    check("health reports secure_mode", "secure_mode" in h)

    # input validation
    check("/download without url -> 400", http_get("/download")[0] == 400)
    check("/cached without url -> 400", http_get("/cached")[0] == 400)

    # MISS -> cached -> HIT (unique URL guarantees a fresh entry)
    fresh = (f"https://raw.githubusercontent.com/rust-lang/rust/master/LICENSE-APACHE"
             f"?nocache={int(time.time())}")
    c0 = json.loads(http_get(f"/cached?{enc(fresh)}")[2])
    check("fresh url not cached", c0.get("cached") is False, str(c0))

    st, hdrs, body = http_get(f"/download?{enc(fresh)}")
    miss_len = len(body)
    check("download MISS 200", st == 200, f"status={st}")
    check("download MISS X-Cache=MISS", hdrs.get("X-Cache") == "MISS", f"X-Cache={hdrs.get('X-Cache')}")
    check("download MISS returns bytes", miss_len > 0, f"{miss_len} bytes")

    time.sleep(1.0)  # let the background cache-commit finish
    c1 = json.loads(http_get(f"/cached?{enc(fresh)}")[2])
    check("after download -> cached", c1.get("cached") is True, str(c1))

    st, hdrs, body2 = http_get(f"/download?{enc(fresh)}")
    check("download HIT 200", st == 200, f"status={st}")
    check("download HIT X-Cache=HIT", hdrs.get("X-Cache") == "HIT", f"X-Cache={hdrs.get('X-Cache')}")
    check("HIT bytes match MISS", len(body2) == miss_len, f"{len(body2)} vs {miss_len}")

    # SSRF + scheme protection
    check("SSRF loopback host blocked",
          http_get(f"/download?{enc('https://localtest.me/')}", timeout=20)[0] in (502, 403))
    check("SSRF raw loopback blocked",
          http_get(f"/download?{enc('https://127.0.0.1/')}", timeout=20)[0] in (502, 403))
    check("http scheme rejected (403)",
          http_get(f"/download?{enc('http://example.com/')}", timeout=20)[0] == 403)

    # Redirect happy-path: a GitHub release URL 302-redirects to objects.githubusercontent.com;
    # a reliable public redirector (unlike httpbin-style services). The proxy should follow it,
    # return the content, and cache it under the requested URL.
    if not args.skip_redirect:
        redir = ("https://github.com/rainmeter/rainmeter/releases/download/"
                 "v4.5.23.3836/Rainmeter-4.5.23.exe")
        st, _, body = http_get(f"/download?{enc(redir)}", timeout=120)
        check("legit redirect followed (200)", st == 200, f"status={st}")
        check("redirected content returned", len(body) > 0, f"{len(body)} bytes")
        time.sleep(1.0)  # let the cache-commit finish
        check("redirect result cached", json.loads(http_get(f"/cached?{enc(redir)}")[2]).get("cached") is True)

    # discovery (connected unicast probe to the proxy host)
    if not args.no_discovery and host:
        got = {}
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.settimeout(3)
        try:
            s.connect((host, args.discovery_port))
            s.send(DISCOVERY_MAGIC)
            got = json.loads(s.recv(4096).decode())
        except Exception as e:  # noqa: BLE001
            got = {"error": str(e)}
        finally:
            s.close()
        check("discovery service=simple-proxy", got.get("service") == "simple-proxy", str(got))
        check("discovery advertises proxy_port", bool(got.get("proxy_port")),
              f"port={got.get('proxy_port')}")

    passed = sum(results)
    print(f"\n== {passed}/{len(results)} checks passed ==")
    return 0 if passed == len(results) else 1


if __name__ == "__main__":
    sys.exit(main())
