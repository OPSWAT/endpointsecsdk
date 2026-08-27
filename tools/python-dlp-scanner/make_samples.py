#!/usr/bin/env python3
###############################################################################
#  Sample Code for OESIS Endpoint-DLP
#  Generates a small corpus of realistic-looking documents to exercise the DLP
#  scanner: files that DO contain sensitive data (credit cards, SSNs, IBANs,
#  emails, ...) across several file types and languages, and files that are
#  deliberately CLEAN (no sensitive data) so you can confirm there are no false
#  positives.
#
#  Everything here is SYNTHETIC. Names are invented; SSNs are fictitious;
#  card numbers are the vendors' public test values; IBANs are the published
#  example IBANs. None of it is real personal data.
#
#  Created by Chris Seiler
#  OPSWAT OEM Solutions Architect
###############################################################################
"""Generate the DLP scanner's sample corpus.

Writes two folders under ./samples:

    samples/sensitive/   documents that SHOULD trigger detections
    samples/clean/       documents that should NOT trigger anything

Run:  python make_samples.py
Then: python dlp_scan.py samples/sensitive     (scan the whole folder)
"""

import os
import shutil

HERE = os.path.dirname(os.path.abspath(__file__))
SAMPLES = os.path.join(HERE, "samples")
SENSITIVE = os.path.join(SAMPLES, "sensitive")
CLEAN = os.path.join(SAMPLES, "clean")

# --- Synthetic test data (not real). -----------------------------------------
# Card numbers are the card brands' well-known TEST numbers; IBANs are the
# published example IBANs; SSNs are format-valid but fictitious.
VISA = "4111 1111 1111 1111"
MASTERCARD = "5555 5555 5555 4444"
AMEX = "3782 822463 10005"
IBAN_DE = "DE89 3704 0044 0532 0130 00"
IBAN_ES = "ES91 2100 0418 4502 0005 1332"
IBAN_FR = "FR14 2004 1010 0505 0001 3M02 606"
SSN_1 = "412-34-5678"
SSN_2 = "457-55-1329"


# ---------------------------------------------------------------------------
#  Format writers
# ---------------------------------------------------------------------------

def write_text(path, text, encoding="utf-8"):
    with open(path, "w", encoding=encoding, newline="\n") as f:
        f.write(text)


def make_pdf(path, lines):
    """Hand-build a minimal one-page PDF with selectable text (no dependencies).

    `lines` is a list of strings, rendered top-to-bottom in Helvetica 12pt.
    Kept ASCII on purpose: the base-14 PDF fonts don't carry accented glyphs
    reliably, so the multi-language samples use text formats instead.
    """
    content = ["BT", "/F1 12 Tf", "54 730 Td", "15 TL"]
    for line in lines:
        esc = line.replace("\\", r"\\").replace("(", r"\(").replace(")", r"\)")
        content.append("(%s) Tj" % esc)
        content.append("T*")
    content.append("ET")
    stream = "\n".join(content).encode("latin-1", "replace")

    objects = [
        b"<< /Type /Catalog /Pages 2 0 R >>",
        b"<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
        b"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] "
        b"/Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
        b"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
        b"<< /Length %d >>\nstream\n" % len(stream) + stream + b"\nendstream",
    ]

    out = bytearray(b"%PDF-1.4\n")
    offsets = []
    for i, body in enumerate(objects, start=1):
        offsets.append(len(out))
        out += b"%d 0 obj\n" % i + body + b"\nendobj\n"

    xref_pos = len(out)
    out += b"xref\n0 %d\n" % (len(objects) + 1)
    out += b"0000000000 65535 f \n"
    for off in offsets:
        out += b"%010d 00000 n \n" % off
    out += b"trailer\n<< /Size %d /Root 1 0 R >>\n" % (len(objects) + 1)
    out += b"startxref\n%d\n%%%%EOF\n" % xref_pos

    with open(path, "wb") as f:
        f.write(bytes(out))


def make_image(path, title, rows):
    """Render a 'scanned form' PNG so the DLP OCR extractor has text to read."""
    from PIL import Image, ImageDraw, ImageFont

    def load_font(size):
        for name in ("arial.ttf", "segoeui.ttf", "DejaVuSans.ttf"):
            try:
                return ImageFont.truetype(name, size)
            except OSError:
                continue
        return ImageFont.load_default()

    title_font = load_font(32)
    body_font = load_font(26)

    W = 950
    H = 150 + 52 * len(rows)
    img = Image.new("RGB", (W, H), "white")
    draw = ImageDraw.Draw(img)

    draw.text((40, 30), title, fill="black", font=title_font)
    draw.line((40, 80, W - 40, 80), fill="black", width=2)
    y = 110
    for row in rows:
        draw.text((40, y), row, fill="black", font=body_font)
        y += 52

    img.save(path, "PNG")


# ---------------------------------------------------------------------------
#  Sensitive documents (should trigger detections)
# ---------------------------------------------------------------------------

def build_sensitive():
    # 1) PDF - employee record with a name and SSN.
    make_pdf(os.path.join(SENSITIVE, "employee_record.pdf"), [
        "NORTHWIND LOGISTICS - Confidential Employee Record",
        "",
        "Name:         Jordan M. Sample",
        "Employee ID:  NW-20418",
        "SSN:          " + SSN_1,
        "Department:   Finance",
        "Start Date:   2021-03-15",
    ])

    # 2) PDF - payment receipt with a credit-card number.
    make_pdf(os.path.join(SENSITIVE, "payment_receipt.pdf"), [
        "NORTHWIND STORE - Payment Receipt",
        "Order #100582      Date: 2026-08-14",
        "",
        "Bill To:   Dana R. Fielding",
        "Email:     dana.fielding@example.com",
        "",
        "Item                         Qty      Price",
        "Wireless Keyboard              1      59.00",
        "USB-C Dock                     1      129.00",
        "Total                                 188.00 USD",
        "",
        "Paid with Visa card " + VISA + ", exp 09/28",
    ])

    # 3) PDF - patient intake form (medical PII).
    make_pdf(os.path.join(SENSITIVE, "patient_intake.pdf"), [
        "LAKESIDE CLINIC - Patient Intake Form",
        "",
        "Patient:       Michael T. Rivera",
        "Date of Birth: 1984-07-22",
        "SSN:           " + SSN_2,
        "Member ID:     BCBS-483920172",
        "Reason for visit: annual physical",
    ])

    # 4) CSV - a database export mixing names, emails, cards and SSNs.
    write_text(os.path.join(SENSITIVE, "customer_export.csv"),
        "id,name,email,phone,card_number,ssn\n"
        "1,Dana Fielding,dana.fielding@example.com,555-0142,%s,%s\n"
        "2,Priya Nair,priya.nair@example.com,555-0177,%s,%s\n"
        "3,Marcus Webb,marcus.webb@example.com,555-0130,%s,412-90-5567\n"
        % (VISA, SSN_1, MASTERCARD, SSN_2, AMEX))

    # 5) EML - a French email with an IBAN and an Amex card.
    write_text(os.path.join(SENSITIVE, "virement_fr.eml"),
        "From: Comptabilite <compta@societe-exemple.fr>\n"
        "To: tresorerie@societe-exemple.fr\n"
        "Subject: Coordonnees bancaires pour le virement\n"
        "Date: Thu, 14 Aug 2026 10:12:00 +0200\n"
        "MIME-Version: 1.0\n"
        "Content-Type: text/plain; charset=utf-8\n"
        "\n"
        "Bonjour,\n\n"
        "Merci de proceder au virement du salaire sur le compte suivant :\n"
        "  Titulaire : Emile Dubois\n"
        "  IBAN      : %s\n"
        "  Carte American Express de secours : %s\n\n"
        "Cordialement,\n"
        "Le service comptabilite\n" % (IBAN_FR, AMEX))

    # 6) HTML - a Spanish invoice with a card number and an IBAN.
    write_text(os.path.join(SENSITIVE, "factura_es.html"),
        "<!DOCTYPE html>\n<html lang=\"es\">\n<head>\n"
        "  <meta charset=\"utf-8\">\n  <title>Factura 2026-0582</title>\n</head>\n"
        "<body>\n"
        "  <h1>Comercial Iberia S.L. - Factura</h1>\n"
        "  <p>Cliente: Maria Lopez Garcia</p>\n"
        "  <p>Correo: maria.lopez@ejemplo.es</p>\n"
        "  <table>\n"
        "    <tr><th>Concepto</th><th>Importe</th></tr>\n"
        "    <tr><td>Servicio de consultoria</td><td>1.250,00 EUR</td></tr>\n"
        "  </table>\n"
        "  <p>Pago con tarjeta Mastercard: %s</p>\n"
        "  <p>Domiciliacion IBAN: %s</p>\n"
        "</body>\n</html>\n" % (MASTERCARD, IBAN_ES))

    # 7) TXT - a German wire-transfer note with an IBAN.
    write_text(os.path.join(SENSITIVE, "ueberweisung_de.txt"),
        "Zahlungsanweisung - Musterbank AG\n"
        "==================================\n\n"
        "Empfaenger:   Hans Mueller\n"
        "Verwendung:   Gehaltszahlung August 2026\n"
        "IBAN:         %s\n"
        "Betrag:       3.450,00 EUR\n\n"
        "Bitte die Ueberweisung bis zum 28.08.2026 ausfuehren.\n" % IBAN_DE)

    # 8) PNG - a 'scanned' bank form (image, read via OCR).
    make_image(os.path.join(SENSITIVE, "bank_statement.png"),
        "ACME BANK - New Account Form", [
            "Applicant Name:  Jordan M. Sample",
            "Social Security:  " + SSN_1,
            "Card on File:     " + VISA,
            "Branch:           Downtown",
        ])


# ---------------------------------------------------------------------------
#  Clean documents (should NOT trigger anything)
# ---------------------------------------------------------------------------

def build_clean():
    # Numbers here look plausible but are order/build/room numbers, not PII, so
    # a well-tuned engine leaves them alone.
    write_text(os.path.join(CLEAN, "meeting_notes.txt"),
        "Team Sync - 2026-08-12\n"
        "======================\n\n"
        "- Q3 leads grew from 482 to 1203, win rate 34 percent.\n"
        "- Next standup at 3pm in room 214; invite 12 people.\n"
        "- Ship target: version 4.2.1, build 33021.\n"
        "- Action: Dana to draft the Q4 plan by Friday.\n")

    write_text(os.path.join(CLEAN, "release_notes.txt"),
        "Northwind App - Release Notes\n"
        "=============================\n\n"
        "Version 4.2.1 (build 33021)\n"
        "  * Fixed a crash when opening large reports.\n"
        "  * Improved sync speed by roughly 18 percent.\n"
        "  * Tested on 8 machines across Windows and macOS.\n")

    write_text(os.path.join(CLEAN, "product_overview.html"),
        "<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n"
        "  <meta charset=\"utf-8\">\n  <title>Northwind Analytics</title>\n</head>\n"
        "<body>\n"
        "  <h1>Northwind Analytics</h1>\n"
        "  <p>Turn your operational data into clear, shareable dashboards.</p>\n"
        "  <ul>\n"
        "    <li>Connect 40+ data sources in minutes.</li>\n"
        "    <li>Schedule reports for your whole team.</li>\n"
        "    <li>Trusted by 1,200 companies worldwide.</li>\n"
        "  </ul>\n"
        "</body>\n</html>\n")

    # German clean note.
    write_text(os.path.join(CLEAN, "besprechungsnotizen_de.txt"),
        "Teambesprechung - 12.08.2026\n"
        "============================\n\n"
        "- Umsatz im Q3 von 482 auf 1203 gestiegen.\n"
        "- Naechstes Meeting um 15 Uhr in Raum 214.\n"
        "- Zielversion 4.2.1, Build 33021.\n"
        "- Aufgabe: Angebot bis Freitag fertigstellen.\n")

    # Spanish clean note.
    write_text(os.path.join(CLEAN, "notas_reunion_es.txt"),
        "Reunion de equipo - 12/08/2026\n"
        "==============================\n\n"
        "- Los clientes potenciales crecieron de 482 a 1203.\n"
        "- Proxima reunion a las 15:00 en la sala 214.\n"
        "- Version objetivo 4.2.1, compilacion 33021.\n"
        "- Tarea: preparar el plan del Q4 para el viernes.\n")

    # Japanese clean note (UTF-8 text; no sensitive data).
    write_text(os.path.join(CLEAN, "kaigi_memo_ja.txt"),
        "チーム会議メモ - 2026年8月12日\n"
        "================================\n\n"
        "・見込客が482件から1203件に増加。\n"
        "・次回の会議は15時、214号室。\n"
        "・目標バージョン 4.2.1、ビルド 33021。\n")


def _reset(folder):
    """Empty (or create) a folder, leaving its parent alone. Skips files that
    are locked (e.g. a sample open in a viewer) rather than failing the run."""
    os.makedirs(folder, exist_ok=True)
    for name in os.listdir(folder):
        path = os.path.join(folder, name)
        try:
            if os.path.isdir(path):
                shutil.rmtree(path)
            else:
                os.remove(path)
        except OSError as ex:
            print("  (skipped locked file: %s - %s)" % (name, ex))


def main():
    # Rebuild the two subfolders from scratch so stale files don't linger. We do
    # not remove samples/ itself (it may be someone's terminal cwd on Windows).
    os.makedirs(SAMPLES, exist_ok=True)
    # Drop any files left at the samples root by older versions of this script.
    for stale in ("ssn_sample.pdf", "ssn_sample.png"):
        try:
            os.remove(os.path.join(SAMPLES, stale))
        except OSError:
            pass
    _reset(SENSITIVE)
    _reset(CLEAN)

    build_sensitive()
    build_clean()

    print("Wrote sample corpus under: %s\n" % SAMPLES)
    for folder, kind in ((SENSITIVE, "sensitive"), (CLEAN, "clean")):
        print("  %s/  (%s)" % (os.path.relpath(folder, HERE).replace("\\", "/"), kind))
        for name in sorted(os.listdir(folder)):
            print("    - " + name)
    print("\nSynthetic test data only: invented names; fictitious SSNs; "
          "vendor test card numbers; published example IBANs.")


if __name__ == "__main__":
    main()
