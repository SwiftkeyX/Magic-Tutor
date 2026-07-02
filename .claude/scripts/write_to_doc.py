#!/usr/bin/env python3
import sys
import os
import requests
from google.oauth2 import service_account
import google.auth.transport.requests

CREDENTIALS_FILE = "google-service-credential.json"
DOCUMENT_ID = "1sIYYK3cSpP2VyOx50mTw6xQ9u7Qdwyu0KALVejZpkLA"
SCOPES = ['https://www.googleapis.com/auth/documents']

# Path to the research report markdown file
REPORT_PATH = r"C:\Users\ad\.gemini\antigravity\brain\34d7da39-aeac-472c-ad21-df5de0e2f560\tft_balance_research.md"

def read_report():
    if not os.path.exists(REPORT_PATH):
        print(f"Error: Report file not found at {REPORT_PATH}")
        sys.exit(1)
    with open(REPORT_PATH, 'r', encoding='utf-8') as f:
        return f.read()

def main():
    print("Reading research report...")
    content = read_report()
    
    # We want to insert the text. Let's make a simple conversion:
    # Google Docs API requires inserting text at indices.
    # To write to a clean document, we can overwrite or append.
    # Let's clear the document first by deleting any existing content, 
    # then insert the new content.
    
    print("Authenticating with Google Docs API...")
    try:
        creds = service_account.Credentials.from_service_account_file(
            CREDENTIALS_FILE, scopes=SCOPES
        )
        auth_req = google.auth.transport.requests.Request()
        creds.refresh(auth_req)
        token = creds.token
    except Exception as e:
        print(f"Authentication failed: {e}")
        sys.exit(1)
        
    headers = {
        "Authorization": f"Bearer {token}"
    }
    
    # First, get the document to find its current length (to clear it)
    get_url = f"https://docs.googleapis.com/v1/documents/{DOCUMENT_ID}"
    print(f"Fetching document metadata for ID {DOCUMENT_ID}...")
    res = requests.get(get_url, headers=headers)
    if res.status_code != 200:
        if "disabled" in res.text or "not been used" in res.text:
            print("\nError: Google Docs API is disabled in your Google Cloud Project.")
            print("Please enable it by opening this link in your browser:")
            print("https://console.developers.google.com/apis/api/docs.googleapis.com/overview?project=984945055946")
            print("\nAfter enabling it, re-run this script.")
        else:
            print(f"Error fetching document: {res.status_code} - {res.text}")
        sys.exit(1)
        
    doc_data = res.json()
    body = doc_data.get("body", {})
    content_list = body.get("content", [])
    
    # Find the end index of the document to delete existing text
    end_index = 1
    if content_list:
        end_index = content_list[-1].get("endIndex", 1)
        
    update_url = f"https://docs.googleapis.com/v1/documents/{DOCUMENT_ID}:batchUpdate"
    
    requests_body = []
    
    # Delete existing content if any (from index 1 to end_index - 1)
    if end_index > 2:
        requests_body.append({
            "deleteContentRange": {
                "range": {
                    "startIndex": 1,
                    "endIndex": end_index - 1
                }
            }
        })
        
    # Insert our new content at index 1
    requests_body.append({
        "insertText": {
            "location": {
                "index": 1
            },
            "text": content
        }
    })
    
    print("Writing report to Google Doc...")
    res = requests.post(update_url, headers=headers, json={"requests": requests_body})
    if res.status_code == 200:
        print("Successfully wrote TFT Balance Research Report to the Google Doc!")
    else:
        print(f"Error writing to document: {res.status_code} - {res.text}")
        sys.exit(1)

if __name__ == "__main__":
    main()
