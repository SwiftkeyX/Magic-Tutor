#!/usr/bin/env python3
import sys
import argparse
import json
import gspread

# Default file name for the credentials
CREDENTIALS_FILE = "google-service-credential.json"
SPREADSHEET_KEY = "1lz_WS8Vw0YjfmNZLI3JmRArfFBU2sPAk2qAG8AKN3jg"

def get_client():
    try:
        return gspread.service_account(filename=CREDENTIALS_FILE)
    except Exception as e:
        print(f"Error authenticating with {CREDENTIALS_FILE}: {e}")
        print("Please make sure the credentials file exists at the root and is valid.")
        sys.exit(1)

def get_spreadsheet(client):
    try:
        return client.open_by_key(SPREADSHEET_KEY)
    except Exception as e:
        print(f"Error opening spreadsheet '{SPREADSHEET_KEY}': {e}")
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
    sh = get_spreadsheet(client)
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
    client = get_client()
    sh = get_spreadsheet(client)
    try:
        ws = sh.worksheet(args.worksheet)
        ws.update_acell(args.cell, args.value)
        print(f"Successfully updated '{args.worksheet}'!{args.cell} to '{args.value}'")
    except Exception as e:
        print(f"Error updating sheet: {e}")
        sys.exit(1)

def cmd_dump(args):
    client = get_client()
    sh = get_spreadsheet(client)
    data = {}
    for ws in sh.worksheets():
        print(f"Dumping {ws.title}...")
        data[ws.title] = ws.get_all_values()
        
    output_file = args.output
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)
    print(f"Successfully dumped all tabs to {output_file}!")

def main():
    parser = argparse.ArgumentParser(description="Google Sheets Read/Write Script for Magic School")
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

    # Dump subcommand
    parser_dump = subparsers.add_parser("dump", help="Dump all worksheets to a local JSON file")
    parser_dump.add_argument("-o", "--output", default="sheet_dump.json", help="Path to output JSON file")
    parser_dump.set_defaults(func=cmd_dump)

    args = parser.parse_args()
    if not args.command:
        parser.print_help()
        sys.exit(1)

    args.func(args)

if __name__ == "__main__":
    main()
