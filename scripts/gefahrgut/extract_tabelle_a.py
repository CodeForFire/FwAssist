#!/usr/bin/env python3
"""Build the Gefahrgut helper's TabelleA.tsv from the official German text of the ADR.

Germany publishes no consolidated ADR 2025. The text in force is the Bekanntmachung of the ADR 2021
(BGBl. 2021 II S. 1184, Anlageband), amended by the 29. and the 30. ADR-Änderungsverordnung. This
script reads Tabelle A from the 2021 Anlageband and applies the amendments, which are kept by hand in
aenderungen.tsv and neue-eintragungen.tsv beside it. Every one of these texts is an amtliches Werk
under § 5 Abs. 1 UrhG and free to reproduce.

The 2021 Anlageband comes from the Bundesgesetzblatt's free archive at bgbl.de. Give a directory that
does not exist yet: the PDF is downloaded into it and nothing is run from it.

Needs pdfplumber. Run it from a virtual environment outside the repository:

    python -m venv <venv> && <venv>/bin/pip install pdfplumber
    <venv>/bin/python -I scripts/gefahrgut/extract_tabelle_a.py <new download dir>

When ADR 2027 is promulgated, add its Tabelle A changes to the two TSV files and run this again.
"""

from __future__ import annotations

import csv
import http.cookiejar
import re
import sys
import urllib.request
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))

import pdfplumber  # noqa: E402

import tabelle_a_pdf  # noqa: E402

REPO = HERE.parent.parent
OUTPUT = REPO / "src/Helpers/Gefahrgut/FwAssist.Helpers.Gefahrgut/TabelleA.tsv"

ANLAGEBAND = "bgbl221024_Anlageband.pdf"
XAVER = "https://www.bgbl.de/xaver/bgbl/"
# Tabelle A runs over these pages of the Anlageband, counted from 0; Tabelle B follows it.
TABELLE_A = range(286, 594)
TABELLE_B = range(596, 641)

HEADER = """\
# ADR Tabelle A (3.2.1), amtliche deutsche Übersetzung, Stand ADR 2025.
# Quelle: Bekanntmachung der Neufassung der Anlagen A und B zum ADR vom 16. November 2021
# (BGBl. 2021 II S. 1184, Anlageband), geändert durch die 29. ADR-Änderungsverordnung vom
# 22. November 2022 (BGBl. 2022 II Nr. 20, Anlageband) und die 30. ADR-Änderungsverordnung vom
# 19. Februar 2025 (BGBl. 2025 II Nr. 57). Amtliches Werk nach § 5 Abs. 1 UrhG.
# Erzeugt von scripts/gefahrgut/extract_tabelle_a.py. Nicht von Hand ändern.
# Spalten: UN-Nummer (1), Benennung und Beschreibung (2), Klasse (3a), Gefahrzettel (5),
# Nummer zur Kennzeichnung der Gefahr (20), Status (verboten: Beförderung verboten;
# frei: unterliegt nicht den Vorschriften des ADR).
"""

GEFAHRNUMMER = re.compile(r"X?\d{2,3}|1\.\dD")
ZETTEL = re.compile(r"(?:1(?:\.[456])?|2\.[123]|3|4\.[123]|5\.[12]|6\.[12]|7X|7[A-E]|8|9A?)(?:\+[0-9.A-E]+)*|siehe 5\.2\.2\.1\.12")


def download(target: Path) -> Path:
    """Fetch the Anlageband the way the archive's viewer does: page, then the signed media link."""
    target.mkdir(parents=True)
    jar = http.cookiejar.CookieJar()
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))
    opener.addheaders = [("User-Agent", "FwAssist-tabelle-a/1.0")]
    start = (
        f"{XAVER}text.xav?SID=&tf=xaver.component.Text_0&tocf=&qmf=&hlf=xaver.component.Hitlist_0"
        f"&bk=bgbl&start=%2F%2F*%5B%40attr_id%3D%27{ANLAGEBAND}%27%5D&skin=pdf&tlevel=-2&nohist=1"
    )
    with opener.open(start, timeout=60) as response:
        page = response.read().decode("utf-8", "replace")
    match = re.search(r'src="(media\.xav/[^"]+)"', page)
    if not match:
        raise SystemExit("bgbl.de: no PDF link on the viewer page")
    pdf = target / ANLAGEBAND
    with opener.open(XAVER + match.group(1).replace("&amp;", "&"), timeout=600) as response:
        pdf.write_bytes(response.read())
    if not pdf.read_bytes().startswith(b"%PDF"):
        raise SystemExit("bgbl.de: the download is not a PDF")
    return pdf


def read_tsv(path: Path) -> list[dict[str, str]]:
    lines = [line for line in path.read_text(encoding="utf-8").splitlines() if line and not line.startswith("#")]
    # An empty last column has no trailing tab to show for it: restval fills it in.
    return list(csv.DictReader(lines, delimiter="\t", quoting=csv.QUOTE_NONE, restval=""))


def normalise(name: str) -> str:
    # The typesetting leaves a space before some commas and closing brackets: "ABFÄLLE , gemahlen",
    # "R 1113 )".
    name = re.sub(r"\s{2,}", " ", name)
    name = re.sub(r"\s+([,)])", r"\1", name)
    return re.sub(r"\(\s+", "(", name).strip()


def apply(rows: list[dict[str, str]]) -> list[dict[str, str]]:
    field = {"2": "name", "3a": "klasse", "5": "zettel", "20": "gefahrnummer"}
    for change in read_tsv(HERE / "aenderungen.tsv"):
        hits = [r for r in rows if r["un"] == change["un"]]
        if change["eintrag"] != "*":
            hits = [hits[int(change["eintrag"]) - 1]]
        if not hits:
            raise SystemExit(f"Änderung ohne Eintragung: {change}")
        if change["aktion"] == "streichen":
            rows = [r for r in rows if not any(r is h for h in hits)]
            continue
        key = field[change["spalte"]]
        for row in hits:
            if change["aktion"] == "setzen":
                row[key] = change["neu"]
            elif change["aktion"] == "ersetzen":
                if change["alt"] not in row[key]:
                    raise SystemExit(f"„{change['alt']}“ fehlt in {row['un']}: {row[key]}")
                row[key] = row[key].replace(change["alt"], change["neu"], 1)
            else:
                raise SystemExit(f"unbekannte Aktion: {change}")
    for new in read_tsv(HERE / "neue-eintragungen.tsv"):
        if any(r["un"] == new["un"] for r in rows):
            raise SystemExit(f"{new['un']} steht schon in Tabelle A")
        rows.append({
            "un": new["un"], "name": new["benennung"], "klasse": new["klasse"],
            "zettel": new["gefahrzettel"], "gefahrnummer": new["gefahrnummer"], "status": "",
        })
    return rows


def check(rows: list[dict[str, str]]) -> None:
    problems = []
    for r in rows:
        if not re.fullmatch(r"\d{4}", r["un"]):
            problems.append(f"UN-Nummer: {r}")
        if not r["name"] or "\t" in r["name"]:
            problems.append(f"Benennung: {r}")
        if not tabelle_a_pdf.KLASSE.fullmatch(r["klasse"]):
            problems.append(f"Klasse: {r}")
        if r["zettel"] and not ZETTEL.fullmatch(r["zettel"]):
            problems.append(f"Gefahrzettel: {r}")
        if r["gefahrnummer"] and not GEFAHRNUMMER.fullmatch(r["gefahrnummer"]):
            problems.append(f"Gefahrnummer: {r}")
    if problems:
        raise SystemExit("\n".join(problems))


def main() -> None:
    if len(sys.argv) != 2:
        raise SystemExit(__doc__)
    pdf_path = download(Path(sys.argv[1]))
    with pdfplumber.open(pdf_path) as pdf:
        vocabulary = tabelle_a_pdf.tabelle_b_words(pdf, TABELLE_B)
        parsed = tabelle_a_pdf.read(pdf, TABELLE_A, vocabulary)
    rows = [
        {
            "un": r.un, "name": normalise(r.name), "klasse": r.klasse, "zettel": r.zettel,
            "gefahrnummer": r.gefahrnummer, "status": r.status,
        }
        for r in parsed
    ]
    rows = apply(rows)
    check(rows)

    # Rows that differed only in a column this app does not keep, the Verpackungsgruppe say,
    # become one. The order of Tabelle A within a UN-Nummer is kept.
    seen = set()
    unique = []
    for r in sorted(rows, key=lambda r: r["un"]):
        key = tuple(r.values())
        if key not in seen:
            seen.add(key)
            unique.append(r)

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    with OUTPUT.open("w", encoding="utf-8", newline="\n") as out:
        out.write(HEADER)
        for r in unique:
            out.write("\t".join(r.values()) + "\n")
    unsure = [r for r in parsed if r.ambiguous]
    print(f"{len(unique)} Zeilen, {len({r['un'] for r in unique})} UN-Nummern -> {OUTPUT}")
    print(f"{len(unsure)} Benennungen mit einem Trennstrich am Zeilenende, nach Wortliste entschieden:")
    for r in unsure:
        print(f"  {r.un} {r.name}")


if __name__ == "__main__":
    main()
