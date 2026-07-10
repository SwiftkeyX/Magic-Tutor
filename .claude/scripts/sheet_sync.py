#!/usr/bin/env python3
import sys
import argparse
import json
import datetime
from pathlib import Path
import gspread

CREDENTIALS_FILE = "google-service-credential.json"
SHEETS_CONFIG_FILE = Path(__file__).parent / "sheets_config.json"
DEFAULT_SHEET = "auto-battler"


def load_sheets_config():
    with open(SHEETS_CONFIG_FILE, encoding="utf-8") as f:
        return json.load(f)


def get_sheet_entry(name):
    config = load_sheets_config()
    if name not in config:
        print(f"Unknown sheet '{name}'. Available: {list(config.keys())}")
        sys.exit(1)
    return config[name]


def get_client():
    try:
        return gspread.service_account(filename=CREDENTIALS_FILE)
    except Exception as e:
        print(f"Error authenticating with {CREDENTIALS_FILE}: {e}")
        print("Please make sure the credentials file exists at the root and is valid.")
        sys.exit(1)


def get_spreadsheet(client, sheet_name=DEFAULT_SHEET):
    entry = get_sheet_entry(sheet_name)
    try:
        return client.open_by_key(entry["key"])
    except Exception as e:
        print(f"Error opening spreadsheet '{sheet_name}' ({entry['key']}): {e}")
        print("Please ensure the spreadsheet key is correct and shared with the service account client_email.")
        sys.exit(1)


def print_table(rows):
    if not rows:
        print("Empty worksheet.")
        return
    # Find max width of each column for formatting
    col_widths = [0] * max(len(row) for row in rows)
    for row in rows:
        for idx, cell in enumerate(row):
            if idx < len(col_widths):
                col_widths[idx] = max(col_widths[idx], len(str(cell)))

    # Print table
    border = "+" + "+".join("-" * (w + 2) for w in col_widths) + "+"
    print(border)
    for r_idx, row in enumerate(rows):
        row_str = "| " + " | ".join(str(cell).ljust(col_widths[idx]) for idx, cell in enumerate(row) if idx < len(col_widths)) + " |"
        print(row_str)
        if r_idx == 0:  # Header separator
            print(border)
    print(border)


def cmd_read(args):
    client = get_client()
    sh = get_spreadsheet(client, args.sheet)
    try:
        ws = sh.worksheet(args.worksheet)
        rows = ws.get_all_values()
        if args.format == "table":
            print_table(rows)
        elif args.format == "csv":
            import csv
            import io
            output = io.StringIO()
            writer = csv.writer(output)
            writer.writerows(rows)
            print(output.getvalue())
        elif args.format == "json":
            # Assume first row is header
            if len(rows) > 1:
                header = rows[0]
                records = []
                for row in rows[1:]:
                    record = {}
                    for idx, cell in enumerate(row):
                        key = header[idx] if idx < len(header) and header[idx] else f"col_{idx}"
                        record[key] = cell
                    records.append(record)
                print(json.dumps(records, indent=2))
            else:
                print(json.dumps(rows, indent=2))
    except gspread.exceptions.WorksheetNotFound:
        print(f"Worksheet '{args.worksheet}' not found. Available: {[w.title for w in sh.worksheets()]}")
        sys.exit(1)


def cmd_write(args):
    entry = get_sheet_entry(args.sheet)
    if not entry.get("writable", False):
        print(f"Sheet '{args.sheet}' is marked read-only (role={entry.get('role')}) in sheets_config.json — refusing to write.")
        print("This is a reference sheet we don't own. Edit sheets_config.json only if that has changed.")
        sys.exit(1)
    client = get_client()
    sh = get_spreadsheet(client, args.sheet)
    try:
        ws = sh.worksheet(args.worksheet)
        ws.update_acell(args.cell, args.value)
        print(f"Successfully updated '{args.worksheet}'!{args.cell} to '{args.value}'")
    except Exception as e:
        print(f"Error updating sheet: {e}")
        sys.exit(1)


def cmd_insert_column(args):
    entry = get_sheet_entry(args.sheet)
    if not entry.get("writable", False):
        print(f"Sheet '{args.sheet}' is marked read-only (role={entry.get('role')}) in sheets_config.json — refusing to write.")
        print("This is a reference sheet we don't own. Edit sheets_config.json only if that has changed.")
        sys.exit(1)

    client = get_client()
    sh = get_spreadsheet(client, args.sheet)
    try:
        ws = sh.worksheet(args.worksheet)
    except gspread.exceptions.WorksheetNotFound:
        print(f"Worksheet '{args.worksheet}' not found. Available: {[w.title for w in sh.worksheets()]}")
        sys.exit(1)

    try:
        ws.insert_cols([[]], col=args.index)
        if args.header:
            ws.update_cell(1, args.index, args.header)
        print(f"Inserted a new column at position {args.index} in '{args.worksheet}'"
              + (f" with header '{args.header}'." if args.header else "."))
    except Exception as e:
        print(f"Error inserting column: {e}")
        sys.exit(1)


def cmd_dump(args):
    client = get_client()
    entry = get_sheet_entry(args.sheet)
    sh = get_spreadsheet(client, args.sheet)
    tabs = {}
    for ws in sh.worksheets():
        print(f"Dumping {ws.title}...")
        tabs[ws.title] = ws.get_all_values()

    dumped_at = datetime.datetime.now().isoformat(timespec="seconds")
    data = {
        "_meta": {"sheet": args.sheet, "key": entry["key"], "dumped_at": dumped_at},
        **tabs,
    }

    output_file = args.output or f"{args.sheet}_dump_{datetime.date.today():%Y%m%d}.json"
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)
    print(f"Successfully dumped all tabs to {output_file}!")


def main():
    parser = argparse.ArgumentParser(description="Google Sheets Read/Write Script for Magic School")
    parser.add_argument("--sheet", default=DEFAULT_SHEET,
                         help=f"Named sheet from sheets_config.json (default: {DEFAULT_SHEET})")
    subparsers = parser.add_subparsers(dest="command", help="Command to run")

    # Read subcommand
    parser_read = subparsers.add_parser("read", help="Read data from a worksheet")
    parser_read.add_argument("worksheet", help="Name of the worksheet (e.g. Heroes, Origin, Class, Dashboard)")
    parser_read.add_argument("--format", choices=["table", "json", "csv"], default="table", help="Output format")
    parser_read.set_defaults(func=cmd_read)

    # Write subcommand
    parser_write = subparsers.add_parser("write", help="Write a value to a cell in a worksheet")
    parser_write.add_argument("worksheet", help="Name of the worksheet")
    parser_write.add_argument("cell", help="Cell coordinates (e.g., A1, B2)")
    parser_write.add_argument("value", help="Value to write")
    parser_write.set_defaults(func=cmd_write)

    # Insert-column subcommand
    parser_insert_col = subparsers.add_parser("insert-column", help="Insert a new blank column at a given position, shifting existing columns right")
    parser_insert_col.add_argument("worksheet", help="Name of the worksheet")
    parser_insert_col.add_argument("index", type=int, help="1-based column position to insert at (e.g. 5 for column E)")
    parser_insert_col.add_argument("--header", default=None, help="Optional header text to write into row 1 of the new column")
    parser_insert_col.set_defaults(func=cmd_insert_column)

    # Dump subcommand
    parser_dump = subparsers.add_parser("dump", help="Dump all worksheets to a local JSON file")
    parser_dump.add_argument("-o", "--output", default=None,
                              help="Path to output JSON file (default: <sheet>_dump_<date>.json)")
    parser_dump.set_defaults(func=cmd_dump)

    args = parser.parse_args()
    if not args.command:
        parser.print_help()
        sys.exit(1)

    args.func(args)

if __name__ == "__main__":
    main()
