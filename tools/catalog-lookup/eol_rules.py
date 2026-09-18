###############################################################################################
##  Catalog Lookup - eol_rules
##
##  Normalizes patch-catalog product lists onto one comparable unit and flags end-of-life (EOL)
##  products, so OPSWAT's catalog can be compared like-for-like with competitors' published
##  supported-product lists (Ivanti, Automox, SecPod, ...).
##
##  Three adjustments make lists comparable:
##    1. collapse_versions()  - trailing version numbers fold into one TITLE (Wireshark 1.0 ... 4.2
##                               is one title). Four-digit years are kept: "Word 2016" is an edition,
##                               the convention Ivanti and Automox both use.
##    2. is_microsoft()       - Microsoft rows are split out. Competitor lists carry Windows/Office/
##                               Exchange/SQL versions there; that is OS/suite patching, a different
##                               category from third-party application patching.
##    3. classify()           - vendor-announced EOL (end of support on or before the RULES_AS_OF
##                               date). Rules are regexes over a normalized "vendor | product" key,
##                               each with its reason, so every hit can be audited.
##
##  Usage:
##      from eol_rules import summarize, classify, collapse_versions
##      s = summarize([(vendor, product), ...])   # -> dict of titles / microsoft / eol / active_tp
##
##  Created by Chris Seiler - OPSWAT OEM Field CTO
###############################################################################################

import re

RULES_AS_OF = "2026-09-14"

# (regex over the normalized "vendor | product" key, reason). Case-insensitive. Families that are
# STILL supported are deliberately absent: .NET Framework 3.5 / 4.7 / 4.8, TFS 2017 / 2018,
# SCOM / SCVMM 2016+, Visual Studio 2017+, Skype for Business Server 2019, Apache OpenOffice.
EOL_RULES = [
    # --- Microsoft operating systems ---
    (r"microsoft \| windows (nt|2000|xp|vista|7|8|8\.1)\b", "OS end of support"),
    (r"microsoft \| windows 10\b", "Windows 10 end of support Oct 2025"),
    (r"microsoft \| windows server (2003|2008|2008 r2|2012|2012 r2)\b", "Server end of support"),
    (r"microsoft \| small business server", "end of support"),
    # --- Microsoft Office family (2016/2019 ended Oct 2025) ---
    (r"microsoft \| (office|access|excel|word|outlook|powerpoint|publisher|visio|project|onenote|infopath|lync) (97|98|2000|2002|2003|2007|2010|2013|2016|2019)\b", "Office family end of support"),
    (r"microsoft \| office (xp|access|infopath|onenote|powerpoint|publisher|visio|word viewer|small business|web apps|forms server|groove|ime|sharepoint server)", "Office component end of support"),
    (r"microsoft \| (access runtime|access database engine|access services|excel viewer|word viewer|powerpoint viewer|visio viewer|excel services|excel web app|powerpoint services|visio services|word server|slide library|server proof|proofing tools|office shared|office server|office groove|office ime|office pinyin|office compatibility pack|office converter pack|office file validation|office 2003 web components|office (2007|2010) filter pack|sharepoint workspace|business contact manager|infopath form|project web front end|project server|performancepoint|web analytics|sharepoint multilingual|skydrive pro|producer 2003)", "Office/SharePoint component end of support"),
    (r"microsoft \| sharepoint (server|foundation|designer|services|team services) ?(2\.0|3\.0|2007|2010|2013|2016|2019)?\b", "SharePoint end of support (2016/2019 Jul 2026)"),
    (r"microsoft \| sharepoint server 2013 client", "SharePoint 2013 end of support"),
    (r"microsoft \| exchange (server|system manager) (2003|2007|2010|2013|2016|2019)\b", "Exchange end of support (2016/2019 Oct 2025)"),
    (r"microsoft \| sql server (2000|2005|2008|2008 r2|2012|2014|2016)\b", "SQL Server end of support (2016 Jul 2026)"),
    (r"microsoft \| (sql server desktop engine|wmsde|sql server management studio (17|18|19)$|sql server management studio express)", "SQL tooling end of support"),
    (r"microsoft \| skype for business (server 2015|2016|basic 2016)", "Skype for Business end of support Oct 2025"),
    (r"microsoft \| skype$", "consumer Skype retired May 2025"),
    (r"microsoft \| (lync|office communicator|office communications server|live meeting|live messenger|msn messenger|windows messenger|live essentials|windows essentials|windows live|groove|windows mail|outlook express)", "discontinued communications/legacy product"),
    (r"microsoft \| internet explorer", "IE retired Jun 2022"),
    (r"microsoft \| silverlight", "Silverlight end of support Oct 2021"),
    (r"microsoft \| (front ?page|expression (web|design|blend|encoder))", "discontinued web/design tools"),
    (r"microsoft \| windows (media player|media encoder|media services|movie maker|search 4\.0|phone app|fax services|remote desktop connection [5-8])", "legacy Windows component"),
    (r"microsoft \| (works|journal viewer|snapshot viewer|step by step|virtual pc|virtual server|virtual machine|visual foxpro|visual basic 6|vbscript|security essentials|transaction server|telnet service|hyper-v server|windows azure pack|service bus 1\.1)", "discontinued product"),
    (r"microsoft \| visual studio (\.net 2002|\.net 2003|2005|2008|2010|2012|2013|2015)\b", "Visual Studio end of support (2015 Oct 2025)"),
    (r"microsoft \| visual studio team foundation server (2010|2012|2013|2015)\b", "TFS end of support"),
    (r"microsoft \| visual studio tools for applications 2\.0", "end of support"),
    (r"microsoft \| \.net core ?(1\.0|1\.1|2\.0|2\.1|2\.2|3\.0|3\.1|5\.0)\b", ".NET Core end of support"),
    (r"microsoft \| \.net ?(6\.0|7\.0)\b", ".NET 6/7 end of support"),
    (r"microsoft \| \.net sdk (6\.0|7\.0)\b", ".NET 6/7 SDK end of support"),
    (r"microsoft \| asp ?\.net core ?(1\.0|1\.1|2\.0|2\.1|2\.2|3\.0|3\.1|5\.0|6\.0|7\.0)\b", "ASP.NET Core end of support"),
    (r"microsoft \| \.net framework( 1\.0| 1\.1| 2\.0| 3\.0| 4| 4\.0| 4\.5| 4\.5 sp\d| 4\.5\.[12]| 4\.6| 4\.6\.1)?$", ".NET Framework <=4.6.1 end of support"),
    (r"microsoft \| powershell core (6\.\d|7\.0|7\.2|7\.3)\b", "PowerShell end of support"),
    (r"microsoft \| biztalk server (2002|2004|2010|2013|2013 r2)\b", "BizTalk end of support"),
    (r"microsoft \| (commerce server|content management server|host integration server (2004|2006|2009|2010)|search server|systems management server|system center configuration manager 2007|forefront|fast search|enhanced mitigation|capicom|mdac|msxml (2\.5|3\.0|4\.0)|xml core services (3\.0|4\.0|5\.0)|directx|iis (5\.0|6\.0|7\.0|8\.0)|internet information (server|services) (5\.0|5\.1|6\.0|7\.0|7\.5|8\.0|8\.5)|wsus 3\.0|report ?viewer|antixss|services for unix|small business accounting|ole db provider for db2)", "legacy server/runtime end of support"),
    (r"microsoft \| system center (operations manager|virtual machine manager|configuration manager) [a-z ]*(2007|2007 r2|2007 r3|2012|2012 r2)\b", "System Center 2007/2012 end of support"),
    (r"microsoft \| visual c\+\+ (2005|2008|2010|2012|2013)\b", "VC++ runtime end of support"),
    # --- Third-party vendor-discontinued products (normalized vendor; product exact-anchored) ---
    (r"^adobe \| (adobe )?(flash( player)?( activex| npapi| ppapi)?|shockwave( player)?|air|brackets)$", "Adobe discontinued (Flash/Shockwave 2020, AIR handed off 2020, Brackets 2021)"),
    (r"^adobe \| (adobe )?(acrobat|acrobat reader)( dc)? 2020 classic", "Acrobat/Reader 2020 Classic end of support Jun 2025"),
    (r"^adobe \| (adobe )?acrobat reader 2017", "Acrobat Reader 2017 Classic end of support Jun 2022"),
    (r"^apple \| (apple )?(quicktime|safari|ibooks author)$", "Apple discontinued"),
    (r"^google \| (google )?(talk|picasa|drive file stream|sketchup)$", "Google discontinued/renamed/divested"),
    (r"^github \| (github )?atom$", "Atom sunset Dec 2022"),
    (r"\| (twilio )?authy desktop$", "Authy Desktop discontinued Aug 2024"),
    (r"\| (vmware )?horizon client 7$", "Horizon 7 end of support Apr 2023"),
    (r"\| (vmware )?vsphere client (5\.5|6)$", "vSphere Client end of support"),
    (r"^autodesk \| (autodesk )?design review", "Design Review discontinued"),
    (r"\| bluejeans$", "BlueJeans shut down 2024"),
    (r"\| plantronics hub$", "replaced by Poly Lens 2024"),
    (r"^aol \| (aol )?(aim|instant messenger)$", "AIM shut down Dec 2017"),
    (r"^yahoo \| yahoo messenger", "shut down Jul 2018"),
    (r"^skype \| skype$", "consumer Skype retired May 2025"),
    (r"\| skype$", "consumer Skype retired May 2025"),
    (r"^atlassian \| hipchat", "HipChat discontinued Feb 2019"),
    (r"^cisco \| (cisco )?(spark|webex meeting center|webex productivity tools)$", "retired Webex client/brand"),
    (r"^citrix \| (citrix )?(receiver|xenapp|xendesktop)$", "Citrix retired/renamed"),
    (r"^emc \| mozy", "Mozy retired"),
    (r"^blue jeans \|", "BlueJeans shut down 2024"),
    (r"^ringcentral \| (glip|ringcentral app classic)$", "RingCentral retired client"),
    (r"^box(\.com)? \| (box sync|box edit)$", "Box retired (Sync 2021, Edit 2023)"),
    (r"^vmware \| (vmware )?(zimbra desktop|horizon view client|player|movie decoder)$", "VMware retired/renamed"),
    (r"^research in motion \| blackberry desktop", "discontinued"),
    (r"\| blackberry desktop software$", "discontinued"),
    (r"^oracle \| openoffice$", "Oracle OpenOffice discontinued 2011 (Apache OpenOffice is NOT EOL)"),
    (r"^oracle \| (oracle )?java (jdk|jre) 1\.[3-7]", "Java <=7 public updates ended"),
    (r"^classic shell \| classic shell", "discontinued Dec 2017"),
    (r"^quest \| powergui", "discontinued 2015"),
    (r"^prezi \| prezi desktop", "retired"),
    (r"^foxit \| (foxit )?(phantom|phantompdf|reader)$", "renamed (legacy duplicate of PDF Editor/Reader)"),
    (r"^novell \| (novell )?(client|groupwise)$", "Novell brand retired"),
    (r"\| flashget$", "FlashGet discontinued"),
    (r"^ivanti \| test deploy", "placeholder, not a product"),
]
COMPILED = [(re.compile(p, re.I), reason) for p, reason in EOL_RULES]

_SUFFIX = re.compile(r"[,.]?\s*\b(corporation|corp|incorporated|inc|llc|ltd|limited|gmbh|s\.l\.|s\.a\.|"
                     r"foundation|systems|software|technologies|network|developer community)\b\.?\s*$", re.I)

# Vendor-less lists (Automox, SecPod) embed the vendor in the product name; recognize a known
# leading token so vendor-anchored rules still bind. The full name stays the product.
KNOWN_VENDORS = ["microsoft", "adobe", "apple", "google", "cisco", "citrix", "vmware", "mozilla",
                 "oracle", "foxit", "plantronics", "box", "ringcentral", "blue jeans", "bluejeans",
                 "zoom", "amazon", "autodesk", "atlassian", "logitech", "dell", "github", "twilio",
                 "azure", "skype", "yahoo", "aol", "emc", "quest", "prezi", "classic shell", "ivanti",
                 "novell", "jetbrains", "azul"]


def guess_vendor(name):
    low = (name or "").lower()
    for v in KNOWN_VENDORS:
        if low.startswith(v + " ") or low == v:
            return "microsoft" if v == "azure" else v
    return ""


def is_microsoft(vendor, product):
    return "microsoft" in (vendor or "").lower() or (product or "").lower().startswith("microsoft ")


def normalize(vendor, product):
    """Normalized 'vendor | product' key the rules match against."""
    v = (vendor or "").lower().strip()
    p = re.sub(r"\s+", " ", (product or "").lower()).strip()
    if is_microsoft(vendor, product):
        v = "microsoft"
        p = re.sub(r"^microsoft\s+", "", p)
        # "Visual C++ Redistributable 2013" -> "visual c++ 2013"; ".NET Core Runtime 2.1" -> ".net core 2.1"
        p = re.sub(r"\b(redistributable|redistribution pkg|runtime|package)\b\s*", "", p).strip()
        p = re.sub(r"\s+", " ", p)
    else:
        for _ in range(3):
            v = _SUFFIX.sub("", v).strip(" ,.")
    return f"{v} | {p}"


# A line listing several versions ("SSMS 18, 19, 20", "Corretto (8, 11, 17)") covers a current
# release too, so version-based "end of support" rules must not fire on it; only whole-product
# discontinuations count.
_MULTI_VERSION = re.compile(r"\d[\d.]*\s*,\s*\d|\(\s*v?\d[\d.]*\s*-\s*v?\d")
_WHOLE_PRODUCT = ("discontinued", "retired", "sunset", "shut down", "placeholder", "renamed", "divested")


def classify(vendor, product):
    """Return the EOL reason for a product, or None if it is current."""
    key = normalize(vendor, product)
    multi = bool(_MULTI_VERSION.search(product or ""))
    for rx, reason in COMPILED:
        if rx.search(key):
            if multi and not any(w in reason.lower() for w in _WHOLE_PRODUCT):
                continue
            return reason
    return None


# Trailing pure version numbers (<= 3 digits, dotted, ".x", SP/R suffixes, parenthesized lists).
# Four-digit years are NOT versions here - they are editions.
_VER_TAIL = re.compile(
    r"(\s+(v|version\s*)?\d{1,3}(\.\d+)*(\.x)?|\s+sp\s?\d|\s+r\d|\s+service pack \d|\s*\([^)]*\d[^)]*\))+$", re.I)


def collapse_versions(product):
    """'Wireshark 4.2' -> 'Wireshark'; 'Word 2016' stays 'Word 2016'."""
    stripped = _VER_TAIL.sub("", (product or "").strip()).strip()
    return stripped or (product or "").strip()


def title_groups(rows):
    """Group (vendor, product) rows into titles: same Microsoft flag + normalized vendor +
    version-collapsed name. Each group records every row's EOL verdict."""
    groups = {}
    for v, p in rows:
        ms = is_microsoft(v, p)
        vend = "" if ms else normalize(v, p).split(" | ")[0]
        key = (ms, vend, collapse_versions(p).lower())
        g = groups.setdefault(key, {"display": f"{v} | {p}", "microsoft": ms, "eol": [], "n": 0})
        g["n"] += 1
        g["eol"].append(classify(v, p))
    return groups


def summarize(rows):
    """Count a list of (vendor, product) rows on the comparable unit.

    Returns: rows, titles, microsoft, third_party, eol, eol_microsoft, eol_third_party,
    active_third_party (the vendor-comparable number), plus the EOL third-party titles.
    A title is EOL only if EVERY row that collapsed into it is EOL."""
    groups = title_groups(rows)
    ms = [g for g in groups.values() if g["microsoft"]]
    tp = [g for g in groups.values() if not g["microsoft"]]
    eol_ms = [g for g in ms if all(g["eol"])]
    eol_tp = [g for g in tp if all(g["eol"])]
    return {
        "rows": len(rows), "titles": len(groups),
        "microsoft": len(ms), "third_party": len(tp),
        "eol": len(eol_ms) + len(eol_tp), "eol_microsoft": len(eol_ms), "eol_third_party": len(eol_tp),
        "active_third_party": len(tp) - len(eol_tp),
        "active_all": len(groups) - len(eol_ms) - len(eol_tp),
        "eol_third_party_titles": [(g["display"], g["eol"][0]) for g in eol_tp],
        "eol_microsoft_titles": [(g["display"], g["eol"][0]) for g in eol_ms],
    }
