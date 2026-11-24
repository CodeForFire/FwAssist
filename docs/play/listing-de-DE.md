# Play-Store-Eintrag — de-DE

Vorlage für den Haupteintrag im Play Console. Die Grenzen stehen dabei, weil die
Console nicht warnt, sondern abschneidet. `make play-listing-check` zählt nach.

Was hier **nicht** stehen darf, steht in
[`docs/releasing.md`](../releasing.md#what-the-listing-must-not-say) — kurz: nichts,
was amtlichen Status nahelegt, keine Alarmierung und keine Sicherheitsversprechen.

> Entwurf. Der App-Name ist ein Arbeitstitel, bis der endgültige Name feststeht.

---

## App-Name (max. 30 Zeichen)

```
FwAssist – Feuerwehr-Werkzeuge
```

## Kurzbeschreibung (max. 80 Zeichen)

```
Kleine Helfer für die Feuerwehr. Offline, ohne Konto, ohne Werbung. Open Source.
```

## Vollständige Beschreibung (max. 4000 Zeichen)

```
FwAssist ist eine Sammlung kleiner Werkzeuge für Feuerwehrangehörige — für die Hosentasche, offline, ohne Konto und ohne Abo.

JEDER HELFER MIT EIGENEM SYMBOL

Jeder Helfer kann ein eigenes Symbol auf dem Startbildschirm bekommen und öffnet sich dann mit einem Tipp direkt. Welche Symbole erscheinen, legst du in den Einstellungen fest.

DIE HELFER

• Stoppuhr — Zeit messen, zum Beispiel bei einer Übung
• Buchstabieralphabet — Wörter für den Funk buchstabieren, mit der alten Tafel, nach DIN 5009:2022 oder mit dem NATO-Alphabet
• Schaummittel — Schaum aus dem Vorrat und Bedarf für einen Raum berechnen
• RTH-Landeplatz — Vorgaben und Ablauf für den Landeplatz, bei Tag und bei Nacht
• Dekon-Platz — Sofortmaßnahmen, Aufbau und Technik für die Dekon-Stufen I bis III
• Hydrantenkarte — Hydranten an der Einsatzstelle auf einer Offline-Karte, die nächsten mit Entfernung und B-Schläuchen, und ein Richtungspfeil zum gewählten Hydranten
• Kennzeichnungen — Warntafeln, Gefahrzettel, GHS-Piktogramme, Rohrleitungen und Gasflaschen erkennen; die obere Nummer der Warntafel Ziffer für Ziffer erklärt, die untere, die UN-Nummer, mit Stoff, Klasse und Gefahrzettel nach ADR nachgeschlagen
• Strahlenschutz — Dosisleistung aus TKZ und Abstand, Absperrgrenze, als Richtwerte neben der Messung
• Sandsack — Sandsäcke, Sand und Arbeitszeit für einen Sandsackdamm, als Richtwerte

Weitere Helfer folgen. Ideen dafür sammeln wir offen auf GitHub.

DATENSCHUTZ

Kein Konto, keine Anmeldung, keine Cloud, kein Server. Keine Telemetrie, keine Absturzberichte, keine Werbe- oder Analysebibliotheken. Drei Berechtigungen: Der Internetzugriff dient nur dazu, auf Wunsch Kartenpakete für die Hydrantenkarte zu laden; dabei wird nichts über dich oder einen Einsatz gesendet, und die Karte funktioniert danach offline. Den Standort fragt die App erst, wenn du in der Hydrantenkarte den Richtungspfeil oder "Mein Standort" zum ersten Mal benutzt, und nutzt ihn nur, solange der Pfeil offen ist oder die Karte dich sucht; er wird weder gespeichert noch gesendet. Vibrieren darf die App nur, um dir die Ankunft am Hydranten zu melden, zusammen mit einem kurzen Ton; beides folgt der Lautlos-Einstellung deines Handys.

Der Hersteller erhält keinerlei Daten — CodeForFire betreibt keinen Dienst, an den etwas fließen könnte.

HINWEIS

FwAssist ist ein ehrenamtliches Open-Source-Projekt und steht in keiner Verbindung zu einer Behörde oder Leitstelle. Die App ersetzt keine Ausbildung und keine vorgeschriebene Ausrüstung.

Quelltext (MIT): https://github.com/CodeForFire/FwAssist
```

---

## Weitere Felder

| Feld | Wert |
|---|---|
| Kategorie | Tools |
| Kontakt-E-Mail | *(vom Play-Konto)* |
| Website | `https://codeforfire.github.io/` |
| Datenschutzerklärung | `https://codeforfire.github.io/fwassist/datenschutz/` |

## Hinweise für die Prüfung (App-Zugriff → Anleitung)

```
Die App benötigt kein Konto und keine Anmeldung. Sie startet auf einer Übersicht der Helfer; jeder lässt sich mit einem Tipp öffnen und sofort benutzen.

Ins Internet geht die App nur, wenn unter "Einstellungen" → "Kartenpakete" die Liste geladen oder ein Kartenpaket heruntergeladen wird (von github.com, ohne Anmeldung). Danach zeigt der Helfer "Hydrantenkarte" die Karte offline; ein Tipp auf die Karte setzt die Einsatzstelle, "NÄCHSTE HYDRANTEN" listet die nächsten Hydranten. Nach Auswahl eines Hydranten öffnet "PFEIL ZUM HYDRANTEN" den Richtungspfeil; der Knopf mit dem Fadenkreuz über +/− zeigt den eigenen Standort auf der Karte. Erst bei einem von beiden fragt die App nach dem Standort.

Unter "Einstellungen" lässt sich für jeden Helfer ein eigenes Symbol auf dem Startbildschirm ein- und ausschalten.

Quelltext (MIT): https://github.com/CodeForFire/FwAssist
```
