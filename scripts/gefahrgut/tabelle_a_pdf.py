"""Read ADR Tabelle A (3.2.1) out of the German text as the Bundesgesetzblatt prints it.

The table runs over facing pages: columns (1) to (11) on the left page, (12) to (20) on the right,
and the right page repeats (1) and (2) after (20). Both halves are read by word position, using the
column headers "(1)", "(2)", ... printed on each page as the ruler, and paired row by row. The two
copies of the Benennung are typeset at different widths, which is what decides whether a hyphen at
the end of a line belongs to the name or only to the line break.

Only columns (1), (2), (3a), (5) and (20) are kept: UN-Nummer, Benennung und Beschreibung, Klasse,
Gefahrzettel and Nummer zur Kennzeichnung der Gefahr.
"""

from __future__ import annotations

import itertools
import re
from dataclasses import dataclass, replace

UN = re.compile(r"\d{4}")
HEADER = re.compile(r"\((\d+[a-c]?)\)")
LINE_TOLERANCE = 3.0


@dataclass(frozen=True)
class Row:
    un: str
    name: str
    klasse: str
    zettel: str
    gefahrnummer: str
    page: int
    status: str  # "", "verboten" or "frei"
    ambiguous: bool


@dataclass
class _Half:
    un: str
    top: float
    cells: dict[str, list[list[str]]]  # column -> lines -> words


def _headers(words):
    found = {}
    for w in words:
        m = HEADER.fullmatch(w["text"])
        if m:
            found.setdefault(m.group(1), w)
    return found


def _lines(words):
    """Group words into lines by their top edge, left to right."""
    out: list[list[dict]] = []
    for w in sorted(words, key=lambda w: (round(w["top"]), w["x0"])):
        if out and abs(out[-1][0]["top"] - w["top"]) <= LINE_TOLERANCE:
            out[-1].append(w)
        else:
            out.append([w])
    return [sorted(line, key=lambda w: w["x0"]) for line in out]


def _halves(page, columns, un_column):
    """Split one page into rows, each starting at a UN-Nummer in `un_column`.

    `columns` maps a column name to its [x0, x1) range. Returns the rows found and the words above
    the first row (a row continued from the previous page), keyed by column.
    """
    words = page.extract_words(keep_blank_chars=False, use_text_flow=False)
    heads = _headers(words)
    body_top = max(w["bottom"] for w in heads.values()) + 1
    footer = [w for w in words if w["text"].startswith("Bundesgesetzblatt") or w["text"] == "Das"]
    body_bottom = min((w["top"] for w in footer if w["top"] > body_top), default=page.height) - 1
    body = [w for w in words if body_top <= w["top"] < body_bottom]
    # The page number, "3-66", sits centred under the table.
    body = [w for w in body if not re.fullmatch(r"3-\d+", w["text"])]

    x0, x1 = columns[un_column]
    starts = sorted(
        (w for w in body if x0 <= w["x0"] < x1 and UN.fullmatch(w["text"])),
        key=lambda w: w["top"],
    )
    bands = [(s["top"] - LINE_TOLERANCE, s["text"]) for s in starts]

    def cells_in(lo, hi):
        cells = {}
        for col, (cx0, cx1) in columns.items():
            ws = [w for w in body if lo <= w["top"] < hi and cx0 <= w["x0"] < cx1]
            cells[col] = [[w["text"] for w in line] for line in _lines(ws)]
        return cells

    halves = []
    for i, (top, un) in enumerate(bands):
        bottom = bands[i + 1][0] if i + 1 < len(bands) else body_bottom
        halves.append(_Half(un, top, cells_in(top, bottom)))
    first = bands[0][0] if bands else body_bottom
    carry = cells_in(body_top, first)
    return halves, carry


def _left_columns(heads):
    h = {k: v["x0"] for k, v in heads.items()}
    return {
        "1": (h["1"] - 15, h["1"] + 14),
        "2": (h["1"] + 14, h["3a"] - 3),
        "3a": (h["3a"] - 3, h["3b"] - 4),
        "5": (h["5"] - 8, h["6"] - 7),
        "rest": (h["3b"] - 4, 10_000),
    }


def _right_columns(heads, width):
    h = {k: v["x0"] for k, v in heads.items()}
    return {
        "20": (h["20"] - 9, h["1"] - 6),
        "1": (h["1"] - 6, h["1"] + 15),
        "2": (h["1"] + 15, width),
    }


# A locant or prefix before a hyphen is part of the name, never a break: o-CRESOL, tert-BUTYL.
LOCANT = re.compile(r"(?:^|[-\s(,])(?:o|m|p|n|N|O|S|alpha|beta|tert|sec|cis|trans|iso)-$")

NEXT_LOCANT = re.compile(r"(?:o|m|p|n|N|alpha|beta|tert|sec)-")
ELLIPSIS = re.compile(r"(?:oder|und|bzw\.|sowie) ")


def _name_candidates(lines: list[list[str]]) -> set[str]:
    """Every reading of a typeset name: each hyphen ending a line either stays or is a break."""
    texts = [" ".join(words) for words in lines if words]
    if not texts:
        return {""}
    joints = []
    for prev, nxt in zip(texts, texts[1:]):
        if not prev.endswith("-") or prev.endswith(" -"):
            joints.append((" ",))  # "VERSANDSTÜCK -" + "LEERE" is a dash between words
        elif ELLIPSIS.match(nxt):
            joints.append(("- ",))  # "Industrie-" + "oder anderen": the hyphen stands for a word
        elif LOCANT.search(prev) or NEXT_LOCANT.match(nxt) or not (prev[-2:-1].isalpha() and nxt[:1].isalpha()):
            joints.append(("-",))  # "o-" + "CRESOL", "(II)-" + "CYANID", "2,2'-" + "DICHLOR"
        else:
            joints.append(("-", ""))  # "MONO-" + "ETHYL": hyphen kept, or a soft break
    out = set()
    for choice in itertools.product(*joints):
        s = texts[0]
        for joint, nxt in zip(choice, texts[1:]):
            if joint == "-":
                s = s + nxt
            elif joint == "- ":
                s = s + " " + nxt
            elif joint == "":
                s = s[:-1] + nxt
            else:
                s = s + " " + nxt
        out.add(s)
    return out


def _resolve(left: set[str], right: set[str]) -> tuple[str, bool, set[str]]:
    both = left & right
    pool = both or left
    # Prefer the reading with the fewest kept line-end hyphens: German typesetting hyphenates
    # long chemical names far more often than a name carries a hyphen exactly at a break.
    best = sorted(pool, key=lambda s: (s.count("-"), s))
    return best[0], len(both) != 1, pool


WORD = re.compile(r"[A-Za-zÄÖÜäöüß0-9()\-]+")


def _vocabulary_pass(rows: list[Row], pools: list[set[str]], extra: set[str]) -> list[Row]:
    """Settle a name both copies break at the same hyphen by the words the clear names use.

    "DERIVAT-PESTIZID" printed whole elsewhere keeps its hyphen here as well. `extra` holds the
    words of Tabelle B, which sets most names on one line.
    """
    vocabulary = extra | {w for r in rows if not r.ambiguous for w in WORD.findall(r.name)}
    out = []
    for r, pool in zip(rows, pools):
        if r.ambiguous and len(pool) > 1:
            known = [c for c in pool if all(w in vocabulary for w in WORD.findall(c))]
            if len(known) == 1:
                r = replace(r, name=known[0], ambiguous=False)
        out.append(r)
    return out


def _cell(cells, col) -> str:
    return " ".join(" ".join(line) for line in cells.get(col, []))


KLASSE = re.compile(r"1|2|3|4\.[123]|5\.[12]|6\.[12]|7|8|9")


def _klasse(cells) -> str:
    # A wide Klassifizierungscode such as 2TOC starts left of its header and lands in (3a) too.
    found = [w for line in cells.get("3a", []) for w in line if KLASSE.fullmatch(w)]
    return found[0] if found else ""


def _zettel(cells) -> str:
    # "6.1+3" over "+8" is one cell: 6.1+3+8. The 2021 text writes some as "2,1".
    out = ""
    for line in cells.get("5", []):
        text = " ".join(line)
        glued = not out or text.startswith("+") or out.endswith(("+", "."))
        out = out + text if glued else out + " " + text
    if out == "keine":
        return ""
    return re.sub(r"(?<=\d),(?=\d)", ".", out)


def _status(cells) -> str:
    rest = " ".join(" ".join(line) for line in cells.get("rest", [])).upper()
    if "BEFÖRDERUNG VERBOTEN" in rest:
        return "verboten"
    if "UNTERLIEGT NICHT" in rest:
        return "frei"
    return ""


def is_table_page(page) -> bool:
    heads = _headers(page.extract_words())
    return "3a" in heads and "1" in heads and "2" in heads


def is_right_page(page) -> bool:
    heads = _headers(page.extract_words())
    return "20" in heads and "1" in heads


def tabelle_b_words(pdf, page_indexes) -> set[str]:
    """The words of Tabelle B (3.2.2), leaving out any that a line break split."""
    words: set[str] = set()
    for i in page_indexes:
        for line in (pdf.pages[i].extract_text() or "").splitlines():
            found = WORD.findall(line)
            if found and line.rstrip().endswith("-"):
                found = found[:-1]
            words.update(found)
    return words


def read(pdf, page_indexes, vocabulary: set[str] | None = None) -> list[Row]:
    """Read Tabelle A from consecutive (left, right) page pairs."""
    rows: list[tuple[Row, set[str]]] = []
    pending_left: _Half | None = None
    pending_right: _Half | None = None
    pages = list(page_indexes)
    for li, ri in zip(pages[0::2], pages[1::2]):
        lp, rp = pdf.pages[li], pdf.pages[ri]
        lh = _headers(lp.extract_words())
        rh = _headers(rp.extract_words())
        if "3a" not in lh or "20" not in rh:
            raise ValueError(f"pages {li}/{ri} are not a left/right pair of Tabelle A")
        left, lcarry = _halves(lp, _left_columns(lh), "1")
        right, rcarry = _halves(rp, _right_columns(rh, rp.width), "1")
        if pending_left is not None:
            for col, lines in lcarry.items():
                pending_left.cells[col].extend(lines)
            for col, lines in rcarry.items():
                pending_right.cells[col].extend(lines)
            rows.append(_row(pending_left, pending_right, li))
        if len(left) != len(right):
            raise ValueError(f"pages {li}/{ri}: {len(left)} rows left, {len(right)} right")
        for a, b in zip(left[:-1], right[:-1]):
            rows.append(_row(a, b, li))
        pending_left, pending_right = (left[-1], right[-1]) if left else (None, None)
    if pending_left is not None:
        rows.append(_row(pending_left, pending_right, pages[-2]))
    return _vocabulary_pass([r for r, _ in rows], [p for _, p in rows], vocabulary or set())


def _row(a: _Half, b: _Half, page: int) -> tuple[Row, set[str]]:
    if a.un != b.un:
        raise ValueError(f"page {page}: left row {a.un} pairs with right row {b.un}")
    name, ambiguous, pool = _resolve(_name_candidates(a.cells["2"]), _name_candidates(b.cells["2"]))
    return Row(
        un=a.un,
        name=name,
        klasse=_klasse(a.cells),
        zettel="" if _status(a.cells) else _zettel(a.cells),
        gefahrnummer=_cell(b.cells, "20"),
        page=page,
        status=_status(a.cells),
        ambiguous=ambiguous,
    ), pool
